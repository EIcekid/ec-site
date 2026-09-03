namespace EcSite.Api.DTOs.Products;

public record WishlistItemDto(int Id, int ProductId, string ProductName, string? ImageUrl, decimal Price, int Stock);
public record AddWishlistRequest(int ProductId);
