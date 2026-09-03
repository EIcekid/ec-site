using EcSite.Api.Data;
using EcSite.Api.DTOs.Products;
using EcSite.Api.Models;
using EcSite.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcSite.Api.Controllers;

[ApiController]
[Route("api/wishlist")]
[Authorize]
public class WishlistController : ControllerBase
{
    private readonly AppDbContext _db;
    public WishlistController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<WishlistItemDto>>> Get()
    {
        var userId = User.GetUserId();
        var items = await _db.WishlistItems.AsNoTracking()
            .Where(w => w.UserId == userId)
            .Include(w => w.Product!).ThenInclude(p => p.Images)
            .Include(w => w.Product!).ThenInclude(p => p.Variants)
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => new WishlistItemDto(
                w.Id, w.ProductId, w.Product!.Name,
                w.Product.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).FirstOrDefault(),
                w.Product.Price,
                w.Product.Variants.Any() ? w.Product.Variants.Sum(v => v.Stock) : w.Product.Stock))
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> Add(AddWishlistRequest request)
    {
        var userId = User.GetUserId();

        var productExists = await _db.Products.AnyAsync(p => p.Id == request.ProductId && p.IsActive);
        if (!productExists) return NotFound(new { message = "商品が見つかりません" });

        var exists = await _db.WishlistItems.AnyAsync(w => w.UserId == userId && w.ProductId == request.ProductId);
        if (!exists)
        {
            _db.WishlistItems.Add(new WishlistItem { UserId = userId, ProductId = request.ProductId });
            await _db.SaveChangesAsync();
        }

        return NoContent();
    }

    [HttpDelete("{productId:int}")]
    public async Task<IActionResult> Remove(int productId)
    {
        var userId = User.GetUserId();
        var item = await _db.WishlistItems.FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);
        if (item is null) return NotFound();

        _db.WishlistItems.Remove(item);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
