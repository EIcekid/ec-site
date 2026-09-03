namespace EcSite.Api.DTOs.Cart;

public record CartItemDto(
    int Id, int ProductId, string ProductName, string? ImageUrl, decimal Price, int Quantity, int Stock,
    int? ProductVariantId, string? VariantLabel);

public record AddCartItemRequest(int ProductId, int Quantity, int? ProductVariantId);
public record UpdateCartItemRequest(int Quantity);
