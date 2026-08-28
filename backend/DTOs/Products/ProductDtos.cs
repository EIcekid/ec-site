namespace EcSite.Api.DTOs.Products;

public record ProductListItemDto(int Id, string Name, decimal Price, string? ImageUrl, int Stock, string CategoryName);

public record ProductDetailDto(
    int Id, string Name, string Description, decimal Price, int Stock,
    int CategoryId, string CategoryName, List<string> Images,
    double AverageRating, int ReviewCount);

public record CategoryDto(int Id, string Name, int? ParentId, List<CategoryDto> Children);

public record CreateProductRequest(string Name, string Description, decimal Price, int Stock, int CategoryId, List<string> Images);
public record UpdateProductRequest(string Name, string Description, decimal Price, int Stock, int CategoryId, bool IsActive, List<string> Images);

public record CreateReviewRequest(int Rating, string Content);
public record ReviewDto(int Id, string UserName, int Rating, string Content, DateTime CreatedAt);
