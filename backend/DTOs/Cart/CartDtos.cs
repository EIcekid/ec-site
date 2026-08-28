namespace EcSite.Api.DTOs.Cart;

public record CartItemDto(int Id, int ProductId, string ProductName, string? ImageUrl, decimal Price, int Quantity, int Stock);
public record AddCartItemRequest(int ProductId, int Quantity);
public record UpdateCartItemRequest(int Quantity);
