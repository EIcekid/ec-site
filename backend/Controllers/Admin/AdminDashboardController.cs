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
}
