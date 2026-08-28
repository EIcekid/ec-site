using EcSite.Api.DTOs.Orders;

namespace EcSite.Api.Services;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(int userId, CreateOrderRequest request);
}

public class OrderServiceException : Exception
{
    public OrderServiceException(string message) : base(message) { }
}
