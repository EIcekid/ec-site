using EcSite.Api.Data;
using EcSite.Api.DTOs.Cart;
using EcSite.Api.Models;
using EcSite.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcSite.Api.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly AppDbContext _db;
    public CartController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<CartItemDto>>> Get()
    {
        var userId = User.GetUserId();
        var items = await _db.CartItems.AsNoTracking()
            .Where(c => c.UserId == userId)
            .Include(c => c.Product!).ThenInclude(p => p.Images)
            .Select(c => new CartItemDto(
                c.Id, c.ProductId, c.Product!.Name,
                c.Product.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).FirstOrDefault(),
                c.Product.Price, c.Quantity, c.Product.Stock))
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<CartItemDto>> Add(AddCartItemRequest request)
    {
        if (request.Quantity < 1) return BadRequest(new { message = "数量は1以上を指定してください" });

        var userId = User.GetUserId();
        var product = await _db.Products.FindAsync(request.ProductId);
        if (product is null || !product.IsActive) return NotFound(new { message = "商品が見つかりません" });

        var existing = await _db.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == request.ProductId);
        if (existing is not null)
        {
            existing.Quantity = Math.Min(existing.Quantity + request.Quantity, product.Stock);
        }
        else
        {
            existing = new CartItem { UserId = userId, ProductId = request.ProductId, Quantity = Math.Min(request.Quantity, product.Stock) };
            _db.CartItems.Add(existing);
        }
        await _db.SaveChangesAsync();

        return Ok(new CartItemDto(existing.Id, product.Id, product.Name, null, product.Price, existing.Quantity, product.Stock));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateCartItemRequest request)
    {
        if (request.Quantity < 1) return BadRequest(new { message = "数量は1以上を指定してください" });

        var userId = User.GetUserId();
        var item = await _db.CartItems.Include(c => c.Product).FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (item is null) return NotFound();

        item.Quantity = Math.Min(request.Quantity, item.Product!.Stock);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.GetUserId();
        var item = await _db.CartItems.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (item is null) return NotFound();

        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
