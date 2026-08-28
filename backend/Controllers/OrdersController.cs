using EcSite.Api.Data;
using EcSite.Api.DTOs.Orders;
using EcSite.Api.Models;
using EcSite.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcSite.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IOrderService _orderService;

    public OrdersController(AppDbContext db, IOrderService orderService)
    {
        _db = db;
        _orderService = orderService;
    }

    private static OrderDto ToDto(Order o) => new(
        o.Id, o.Status.ToString(), o.TotalAmount, o.DiscountAmount,
        o.CreatedAt, o.PaidAt, o.ShippedAt, o.CompletedAt,
        new AddressDto(o.Address!.Id, o.Address.Recipient, o.Address.Phone, o.Address.Province, o.Address.City, o.Address.Detail, o.Address.IsDefault),
        o.Items.Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.ProductImageUrl, i.Price, i.Quantity)).ToList());

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderRequest request)
    {
        try
        {
            var order = await _orderService.CreateOrderAsync(User.GetUserId(), request);
            return Ok(order);
        }
        catch (OrderServiceException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<OrderDto>>> List()
    {
        var userId = User.GetUserId();
        var orders = await _db.Orders.AsNoTracking()
            .Include(o => o.Address)
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(orders.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDto>> Get(int id)
    {
        var userId = User.GetUserId();
        var order = await _db.Orders.AsNoTracking()
            .Include(o => o.Address)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (order is null) return NotFound();
        return Ok(ToDto(order));
    }

    [HttpPost("{id:int}/pay")]
    public async Task<ActionResult<OrderDto>> Pay(int id)
    {
        var userId = User.GetUserId();
        var order = await _db.Orders.Include(o => o.Address).Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);
        if (order is null) return NotFound();
        if (order.Status != OrderStatus.PendingPayment) return BadRequest(new { message = "订单状态不允许支付" });

        order.Status = OrderStatus.Paid;
        order.PaidAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(ToDto(order));
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult<OrderDto>> Cancel(int id)
    {
        var userId = User.GetUserId();
        var order = await _db.Orders.Include(o => o.Address).Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);
        if (order is null) return NotFound();
        if (order.Status != OrderStatus.PendingPayment) return BadRequest(new { message = "订单状态不允许取消" });

        order.Status = OrderStatus.Cancelled;
        foreach (var item in order.Items)
        {
            var product = await _db.Products.FindAsync(item.ProductId);
            if (product is not null) product.Stock += item.Quantity;
        }
        await _db.SaveChangesAsync();
        return Ok(ToDto(order));
    }
}
