using EcSite.Api.Data;
using EcSite.Api.DTOs.Common;
using EcSite.Api.DTOs.Products;
using EcSite.Api.Models;
using EcSite.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcSite.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ProductsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductListItemDto>>> List(
        [FromQuery] string? keyword, [FromQuery] int? categoryId,
        [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice,
        [FromQuery] string? sort,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 12)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.Products.AsNoTracking().Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(p => p.Name.Contains(keyword) || p.Description.Contains(keyword));

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        var total = await query.CountAsync();

        query = sort switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "rating" => query.OrderByDescending(p => p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0),
            "sales" => query.OrderByDescending(p =>
                _db.OrderItems.Where(oi => oi.ProductId == p.Id).Sum(oi => (int?)oi.Quantity) ?? 0),
            _ => query.OrderByDescending(p => p.CreatedAt),
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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
            .Include(p => p.Images)
            .Include(p => p.Category)
            .Include(p => p.Reviews)
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (product is null) return NotFound();

        var isFavorited = false;
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = User.GetUserId();
            isFavorited = await _db.WishlistItems.AnyAsync(w => w.UserId == userId && w.ProductId == id);
        }

        var stock = product.Variants.Count > 0 ? product.Variants.Sum(v => v.Stock) : product.Stock;

        var dto = new ProductDetailDto(
            product.Id, product.Name, product.Description, product.Price, stock,
            product.CategoryId, product.Category!.Name,
            product.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).ToList(),
            product.Reviews.Count > 0 ? product.Reviews.Average(r => r.Rating) : 0,
            product.Reviews.Count,
            product.Variants.Select(v => new ProductVariantDto(v.Id, v.Color, v.Size, v.Sku, v.PriceDelta, v.Stock)).ToList(),
            isFavorited);

        return Ok(dto);
    }

    [HttpGet("{id:int}/related")]
    public async Task<ActionResult<List<ProductListItemDto>>> GetRelated(int id)
    {
        var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();

        var related = await _db.Products.AsNoTracking()
            .Where(p => p.IsActive && p.Id != id && p.CategoryId == product.CategoryId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(6)
            .Select(p => new ProductListItemDto(
                p.Id, p.Name, p.Price,
                p.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).FirstOrDefault(),
                p.Variants.Any() ? p.Variants.Sum(v => v.Stock) : p.Stock,
                p.Category!.Name))
            .ToListAsync();

        return Ok(related);
    }

    [HttpGet("{id:int}/reviews")]
    public async Task<ActionResult<List<ReviewDto>>> GetReviews(int id)
    {
        var reviews = await _db.Reviews.AsNoTracking()
            .Where(r => r.ProductId == id)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewDto(r.Id, r.User!.Name, r.Rating, r.Content, r.CreatedAt))
            .ToListAsync();

        return Ok(reviews);
    }

    [Authorize]
    [HttpPost("{id:int}/reviews")]
    public async Task<ActionResult<ReviewDto>> AddReview(int id, CreateReviewRequest request)
    {
        if (request.Rating is < 1 or > 5) return BadRequest(new { message = "評価は1〜5の範囲で入力してください" });

        var productExists = await _db.Products.AnyAsync(p => p.Id == id);
        if (!productExists) return NotFound();

        var userId = User.GetUserId();

        var hasPurchased = await _db.Orders
            .Where(o => o.UserId == userId && o.Status != OrderStatus.Cancelled)
            .SelectMany(o => o.Items)
            .AnyAsync(i => i.ProductId == id);
        if (!hasPurchased) return BadRequest(new { message = "この商品を購入したユーザーのみレビューを投稿できます" });

        var user = await _db.Users.FindAsync(userId);
        var review = new Review { ProductId = id, UserId = userId, Rating = request.Rating, Content = request.Content };
        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();

        return Ok(new ReviewDto(review.Id, user!.Name, review.Rating, review.Content, review.CreatedAt));
    }
}
