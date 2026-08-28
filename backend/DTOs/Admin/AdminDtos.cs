namespace EcSite.Api.DTOs.Admin;

public record UpdateOrderStatusRequest(string Status);

public record CreateCouponRequest(string Code, string Type, decimal Value, decimal MinOrderAmount, DateTime ExpiresAt);
public record CouponDto(int Id, string Code, string Type, decimal Value, decimal MinOrderAmount, DateTime ExpiresAt, bool IsActive);

public record AdminOrderListItemDto(int Id, string CustomerName, string Status, decimal TotalAmount, DateTime CreatedAt);

public record DashboardStatsDto(int TotalProducts, int TotalOrders, int TotalUsers, decimal TotalRevenue);
