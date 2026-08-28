namespace EcSite.Api.DTOs.Orders;

public record AddressDto(int Id, string Recipient, string Phone, string Province, string City, string Detail, bool IsDefault);
public record UpsertAddressRequest(string Recipient, string Phone, string Province, string City, string Detail, bool IsDefault);

public record CreateOrderRequest(int AddressId, string? CouponCode);

public record OrderItemDto(int ProductId, string ProductName, string? ProductImageUrl, decimal Price, int Quantity);

public record OrderDto(
    int Id, string Status, decimal TotalAmount, decimal DiscountAmount,
    DateTime CreatedAt, DateTime? PaidAt, DateTime? ShippedAt, DateTime? CompletedAt,
    AddressDto Address, List<OrderItemDto> Items);
