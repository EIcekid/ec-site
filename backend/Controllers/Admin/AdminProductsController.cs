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
                p.Stock, p.Category!.Name))
            .ToListAsync();

        return Ok(new PagedResult<ProductListItemDto>(items, total, page, pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDetailDto>> Get(int id)
    {
        var product = await _db.Products.AsNoTracking()
            .Include(p => p.Images).Include(p => p.Category).Include(p => p.Reviews)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();

        return Ok(new ProductDetailDto(
            product.Id, product.Name, product.Description, product.Price, product.Stock,
            product.CategoryId, product.Category!.Name,
            product.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).ToList(),
            product.Reviews.Count > 0 ? product.Reviews.Average(r => r.Rating) : 0,
            product.Reviews.Count));
    }

    [HttpPost]
    public async Task<ActionResult<ProductDetailDto>> Create(CreateProductRequest request)
    {
        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists) return BadRequest(new { message = "分类不存在" });

        var product = new Product
        {
            Name = request.Name, Description = request.Description, Price = request.Price,
            Stock = request.Stock, CategoryId = request.CategoryId, IsActive = true,
            Images = request.Images.Select((url, idx) => new ProductImage { Url = url, SortOrder = idx }).ToList()
        };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = product.Id }, new { product.Id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateProductRequest request)
    {
        var product = await _db.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();

        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists) return BadRequest(new { message = "分类不存在" });

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.CategoryId = request.CategoryId;
        product.IsActive = request.IsActive;

        _db.ProductImages.RemoveRange(product.Images);
        product.Images = request.Images.Select((url, idx) => new ProductImage { Url = url, SortOrder = idx, ProductId = id }).ToList();

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
