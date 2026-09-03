using EcSite.Api.Data;
using EcSite.Api.DTOs.Admin;
using EcSite.Api.DTOs.Common;
using EcSite.Api.DTOs.Orders;
using EcSite.Api.Models;
using EcSite.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcSite.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/orders")]
[Authorize(Roles = "Admin")]
public class AdminOrdersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IEmailService _emailService;

    public AdminOrdersController(AppDbContext db, IEmailService emailService)
    {
        _db = db;
        _emailService = emailService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<AdminOrderListItemDto>>> List(
        [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.Orders.AsNoTracking().Include(o => o.User).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, true, out var parsed))
            query = query.Where(o => o.Status == parsed);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(o => new AdminOrderListItemDto(o.Id, o.User!.Name, o.Status.ToString(), o.TotalAmount, o.CreatedAt))
            .ToListAsync();

        return Ok(new PagedResult<AdminOrderListItemDto>(items, total, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDto>> Get(int id)
    {
        var order = await _db.Orders.AsNoTracking()
            .Include(o => o.Address).Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (order is null) return NotFound();

        return Ok(new OrderDto(
            order.Id, order.Status.ToString(), order.TotalAmount, order.DiscountAmount,
            order.PointsUsed, order.PointsEarned,
            order.CreatedAt, order.PaidAt, order.ShippedAt, order.CompletedAt,
            new AddressDto(order.Address!.Id, order.Address.Recipient, order.Address.Phone, order.Address.Province, order.Address.City, order.Address.Detail, order.Address.IsDefault),
            order.Items.Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.ProductImageUrl, i.VariantLabel, i.Price, i.Quantity)).ToList()));
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateOrderStatusRequest request)
    {
        var order = await _db.Orders.Include(o => o.User).FirstOrDefaultAsync(o => o.Id == id);
        if (order is null) return NotFound();

        if (!Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus))
            return BadRequest(new { message = "無効な注文状態です" });

        var allowed = order.Status switch
        {
            OrderStatus.Paid => newStatus == OrderStatus.Shipped,
            OrderStatus.Shipped => newStatus == OrderStatus.Completed,
            _ => false
        };
        if (!allowed) return BadRequest(new { message = $"注文状態を {order.Status} から {newStatus} に変更することはできません" });

        order.Status = newStatus;
        if (newStatus == OrderStatus.Shipped) order.ShippedAt = DateTime.UtcNow;
        if (newStatus == OrderStatus.Completed)
        {
            order.CompletedAt = DateTime.UtcNow;
            order.PointsEarned = (int)(order.TotalAmount / OrderService.PointsEarnRateYen);
            if (order.User is not null) order.User.Points += order.PointsEarned;
        }
        await _db.SaveChangesAsync();

        if (order.User is not null)
        {
            var statusText = newStatus == OrderStatus.Shipped ? "発送済み" : "完了";
            _ = _emailService.SendAsync(order.User.Email, $"注文ステータス更新のお知らせ #{order.Id}", $"<p>ご注文 #{order.Id} のステータスが「{statusText}」に更新されました。</p>");
        }

        return NoContent();
    }
}
