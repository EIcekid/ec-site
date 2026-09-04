using EcSite.Api.Data;
using EcSite.Api.DTOs.Admin;
using EcSite.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcSite.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController : ControllerBase
{
    private readonly AppDbContext _db;
    public AdminDashboardController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<DashboardStatsDto>> Get()
    {
        var totalProducts = await _db.Products.CountAsync(p => p.IsActive);
        var totalOrders = await _db.Orders.CountAsync();
        var totalUsers = await _db.Users.CountAsync(u => u.Role == UserRole.Customer);
        var totalRevenue = await _db.Orders
            .Where(o => o.Status == OrderStatus.Paid || o.Status == OrderStatus.Shipped || o.Status == OrderStatus.Completed)
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

        return Ok(new DashboardStatsDto(totalProducts, totalOrders, totalUsers, totalRevenue));
    }

    [HttpGet("revenue-trend")]
    public async Task<ActionResult<List<RevenuePointDto>>> RevenueTrend([FromQuery] int days = 14)
    {
        days = Math.Clamp(days, 1, 90);
        var since = DateTime.UtcNow.Date.AddDays(-(days - 1));

        var raw = await _db.Orders
            .Where(o => o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.PendingPayment && o.CreatedAt >= since)
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Amount = g.Sum(o => o.TotalAmount) })
            .ToListAsync();

        var lookup = raw.ToDictionary(r => r.Date, r => r.Amount);
        var result = Enumerable.Range(0, days)
            .Select(i => since.AddDays(i))
            .Select(d => new RevenuePointDto(d.ToString("MM/dd"), lookup.TryGetValue(d, out var amt) ? amt : 0m))
            .ToList();

        return Ok(result);
    }

    [HttpGet("order-status")]
    public async Task<ActionResult<List<OrderStatusCountDto>>> OrderStatusDistribution()
    {
        var raw = await _db.Orders
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        return Ok(raw.Select(r => new OrderStatusCountDto(r.Status.ToString(), r.Count)).ToList());
    }

    [HttpGet("category-sales")]
    public async Task<ActionResult<List<CategorySalesDto>>> CategorySales()
    {
        var raw = await _db.Categories
            .Select(c => new
            {
                CategoryName = c.Name,
                Amount = _db.OrderItems
                    .Where(i => i.Order!.Status != OrderStatus.Cancelled && i.Order.Status != OrderStatus.PendingPayment)
                    .Join(_db.Products.Where(p => p.CategoryId == c.Id), i => i.ProductId, p => p.Id, (i, p) => i)
                    .Sum(i => (decimal?)(i.Price * i.Quantity)) ?? 0m
            })
            .Where(c => c.Amount > 0)
            .OrderByDescending(c => c.Amount)
            .ToListAsync();

        return Ok(raw.Select(r => new CategorySalesDto(r.CategoryName, r.Amount)).ToList());
    }
}
