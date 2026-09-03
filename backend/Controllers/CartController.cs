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
            .Include(c => c.ProductVariant)
            .ToListAsync();

        var dtos = items.Select(c => new CartItemDto(
            c.Id, c.ProductId, c.Product!.Name,
            c.Product.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).FirstOrDefault(),
            c.Product.Price + (c.ProductVariant?.PriceDelta ?? 0),
            c.Quantity,
            c.ProductVariant?.Stock ?? c.Product.Stock,
            c.ProductVariantId,
            c.ProductVariant?.Label)).ToList();

        return Ok(dtos);
    }

    [HttpPost]
    public async Task<ActionResult<CartItemDto>> Add(AddCartItemRequest request)
    {
        if (request.Quantity < 1) return BadRequest(new { message = "数量は1以上を指定してください" });

        var userId = User.GetUserId();
        var product = await _db.Products.Include(p => p.Variants).FirstOrDefaultAsync(p => p.Id == request.ProductId);
        if (product is null || !product.IsActive) return NotFound(new { message = "商品が見つかりません" });

        ProductVariant? variant = null;
        if (request.ProductVariantId.HasValue)
        {
            variant = product.Variants.FirstOrDefault(v => v.Id == request.ProductVariantId.Value);
            if (variant is null) return BadRequest(new { message = "指定した規格が見つかりません" });
        }
        else if (product.Variants.Count > 0)
        {
            return BadRequest(new { message = "規格を選択してください" });
        }

        var stock = variant?.Stock ?? product.Stock;
        var price = product.Price + (variant?.PriceDelta ?? 0);

        var existing = await _db.CartItems.FirstOrDefaultAsync(c =>
            c.UserId == userId && c.ProductId == request.ProductId && c.ProductVariantId == request.ProductVariantId);

        if (existing is not null)
        {
            existing.Quantity = Math.Min(existing.Quantity + request.Quantity, stock);
        }
        else
        {
            existing = new CartItem
            {
                UserId = userId, ProductId = request.ProductId, ProductVariantId = request.ProductVariantId,
                Quantity = Math.Min(request.Quantity, stock)
            };
            _db.CartItems.Add(existing);
        }
        await _db.SaveChangesAsync();

        return Ok(new CartItemDto(existing.Id, product.Id, product.Name, null, price, existing.Quantity, stock, request.ProductVariantId, variant?.Label));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateCartItemRequest request)
    {
        if (request.Quantity < 1) return BadRequest(new { message = "数量は1以上を指定してください" });

        var userId = User.GetUserId();
        var item = await _db.CartItems.Include(c => c.Product).Include(c => c.ProductVariant)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (item is null) return NotFound();

        var stock = item.ProductVariant?.Stock ?? item.Product!.Stock;
        item.Quantity = Math.Min(request.Quantity, stock);
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
