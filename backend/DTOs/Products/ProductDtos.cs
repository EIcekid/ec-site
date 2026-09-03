namespace EcSite.Api.DTOs.Products;

public record ProductListItemDto(int Id, string Name, decimal Price, string? ImageUrl, int Stock, string CategoryName);

public record ProductVariantDto(int Id, string? Color, string? Size, string Sku, decimal PriceDelta, int Stock);

public record ProductDetailDto(
    int Id, string Name, string Description, decimal Price, int Stock,
    int CategoryId, string CategoryName, List<string> Images,
    double AverageRating, int ReviewCount, List<ProductVariantDto> Variants, bool IsFavorited);

public record CategoryDto(int Id, string Name, int? ParentId, List<CategoryDto> Children);

public record ProductVariantInput(string? Color, string? Size, string Sku, decimal PriceDelta, int Stock);

public record CreateProductRequest(string Name, string Description, decimal Price, int Stock, int CategoryId, List<string> Images, List<ProductVariantInput> Variants);
public record UpdateProductRequest(string Name, string Description, decimal Price, int Stock, int CategoryId, bool IsActive, List<string> Images, List<ProductVariantInput> Variants);

public record CreateReviewRequest(int Rating, string Content);
public record ReviewDto(int Id, string UserName, int Rating, string Content, DateTime CreatedAt);
