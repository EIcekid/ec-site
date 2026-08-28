using EcSite.Api.Data;
using EcSite.Api.DTOs.Admin;
using EcSite.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcSite.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/coupons")]
[Authorize(Roles = "Admin")]
public class AdminCouponsController : ControllerBase
{
    private readonly AppDbContext _db;
    public AdminCouponsController(AppDbContext db) => _db = db;

    private static CouponDto ToDto(Coupon c) => new(c.Id, c.Code, c.Type.ToString(), c.Value, c.MinOrderAmount, c.ExpiresAt, c.IsActive);

    [HttpGet]
    public async Task<ActionResult<List<CouponDto>>> List()
    {
        var coupons = await _db.Coupons.AsNoTracking().OrderByDescending(c => c.Id).ToListAsync();
        return Ok(coupons.Select(ToDto));
    }

    [HttpPost]
    public async Task<ActionResult<CouponDto>> Create(CreateCouponRequest request)
    {
        if (!Enum.TryParse<CouponType>(request.Type, true, out var type))
            return BadRequest(new { message = "无效的优惠券类型" });

        var exists = await _db.Coupons.AnyAsync(c => c.Code == request.Code);
        if (exists) return BadRequest(new { message = "优惠券代码已存在" });

        var coupon = new Coupon
        {
            Code = request.Code, Type = type, Value = request.Value,
            MinOrderAmount = request.MinOrderAmount, ExpiresAt = request.ExpiresAt, IsActive = true
        };
        _db.Coupons.Add(coupon);
        await _db.SaveChangesAsync();
        return Ok(ToDto(coupon));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var coupon = await _db.Coupons.FindAsync(id);
        if (coupon is null) return NotFound();
        coupon.IsActive = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
