using EcSite.Api.Data;
using EcSite.Api.DTOs.Common;
using EcSite.Api.DTOs.Products;
using EcSite.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcSite.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/products")]
[Authorize(Roles = "Admin")]
public class AdminProductsController : ControllerBase
{
    private readonly AppDbContext _db;
    public AdminProductsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductListItemDto>>> List(
        [FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.Products.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(p => p.Name.Contains(keyword));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => new ProductListItemDto(
                p.Id, p.Name, p.Price,
                p.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).FirstOrDefault(),
                p.Variants.Any() ? p.Variants.Sum(v => v.Stock) : p.Stock,
                p.Category!.Name))
            .ToListAsync();

        return Ok(new PagedResult<ProductListItemDto>(items, total, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDetailDto>> Get(int id)
    {
        var product = await _db.Products.AsNoTracking()
            .Include(p => p.Images).Include(p => p.Category).Include(p => p.Reviews).Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();

        return Ok(new ProductDetailDto(
            product.Id, product.Name, product.Description, product.Price,
            product.Variants.Count > 0 ? product.Variants.Sum(v => v.Stock) : product.Stock,
            product.CategoryId, product.Category!.Name,
            product.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).ToList(),
            product.Reviews.Count > 0 ? product.Reviews.Average(r => r.Rating) : 0,
            product.Reviews.Count,
            product.Variants.Select(v => new ProductVariantDto(v.Id, v.Color, v.Size, v.Sku, v.PriceDelta, v.Stock)).ToList(),
            false));
    }

    [HttpPost]
    public async Task<ActionResult<ProductDetailDto>> Create(CreateProductRequest request)
    {
        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists) return BadRequest(new { message = "カテゴリーが存在しません" });

        var product = new Product
        {
            Name = request.Name, Description = request.Description, Price = request.Price,
            Stock = request.Stock, CategoryId = request.CategoryId, IsActive = true,
            Images = request.Images.Select((url, idx) => new ProductImage { Url = url, SortOrder = idx }).ToList(),
            Variants = request.Variants.Select(v => new ProductVariant
            {
                Color = v.Color, Size = v.Size, Sku = v.Sku, PriceDelta = v.PriceDelta, Stock = v.Stock
            }).ToList()
        };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = product.Id }, new { product.Id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateProductRequest request)
    {
        var product = await _db.Products.Include(p => p.Images).Include(p => p.Variants).FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();

        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists) return BadRequest(new { message = "カテゴリーが存在しません" });

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.CategoryId = request.CategoryId;
        product.IsActive = request.IsActive;

        _db.ProductImages.RemoveRange(product.Images);
        product.Images = request.Images.Select((url, idx) => new ProductImage { Url = url, SortOrder = idx, ProductId = id }).ToList();

        var oldVariantIds = product.Variants.Select(v => v.Id).ToList();
        if (oldVariantIds.Count > 0)
        {
            // Cart items are the only remaining reference that would block deleting these variant
            // rows (order items fall back to their VariantLabel snapshot when the variant is gone).
            var affectedCartItems = await _db.CartItems
                .Where(c => c.ProductVariantId != null && oldVariantIds.Contains(c.ProductVariantId.Value))
                .ToListAsync();
            _db.CartItems.RemoveRange(affectedCartItems);
        }

        _db.ProductVariants.RemoveRange(product.Variants);
        product.Variants = request.Variants.Select(v => new ProductVariant
        {
            ProductId = id, Color = v.Color, Size = v.Size, Sku = v.Sku, PriceDelta = v.PriceDelta, Stock = v.Stock
        }).ToList();

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null) return NotFound();

        product.IsActive = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
