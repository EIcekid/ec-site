using EcSite.Api.Data;
using EcSite.Api.DTOs.Orders;
using EcSite.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EcSite.Api.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;
    private readonly IEmailService _emailService;

    public OrderService(AppDbContext db, IEmailService emailService)
    {
        _db = db;
        _emailService = emailService;
    }

    public async Task<OrderDto> CreateOrderAsync(int userId, CreateOrderRequest request)
    {
        var address = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == request.AddressId && a.UserId == userId);
        if (address is null) throw new OrderServiceException("收货地址不存在");

        var cartItems = await _db.CartItems
            .Include(c => c.Product!).ThenInclude(p => p.Images)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (cartItems.Count == 0) throw new OrderServiceException("购物车为空");

        foreach (var item in cartItems)
        {
            if (item.Product is null || !item.Product.IsActive)
                throw new OrderServiceException($"商品「{item.ProductId}」已下架");
            if (item.Quantity > item.Product.Stock)
                throw new OrderServiceException($"商品「{item.Product.Name}」库存不足");
        }

        var totalAmount = cartItems.Sum(c => c.Product!.Price * c.Quantity);

        Coupon? coupon = null;
        var discountAmount = 0m;
        if (!string.IsNullOrWhiteSpace(request.CouponCode))
        {
            coupon = await _db.Coupons.FirstOrDefaultAsync(c => c.Code == request.CouponCode && c.IsActive);
            if (coupon is null) throw new OrderServiceException("优惠券不存在或已失效");
            if (coupon.ExpiresAt < DateTime.UtcNow) throw new OrderServiceException("优惠券已过期");
            if (totalAmount < coupon.MinOrderAmount) throw new OrderServiceException($"订单满{coupon.MinOrderAmount}元才能使用该优惠券");

            discountAmount = coupon.Type == CouponType.FixedAmount
                ? coupon.Value
                : Math.Round(totalAmount * coupon.Value / 100m, 2);
            discountAmount = Math.Min(discountAmount, totalAmount);
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();

        var order = new Order
        {
            UserId = userId,
            AddressId = address.Id,
            Status = OrderStatus.PendingPayment,
            TotalAmount = totalAmount - discountAmount,
            DiscountAmount = discountAmount,
            CouponId = coupon?.Id,
        };

        foreach (var item in cartItems)
        {
            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                ProductName = item.Product!.Name,
                ProductImageUrl = item.Product.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).FirstOrDefault(),
                Price = item.Product.Price,
                Quantity = item.Quantity
            });
            item.Product.Stock -= item.Quantity;
        }

        _db.Orders.Add(order);
        _db.CartItems.RemoveRange(cartItems);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        var user = await _db.Users.FindAsync(userId);
        if (user is not null)
        {
            _ = _emailService.SendAsync(
                user.Email,
                $"订单确认 #{order.Id}",
                $"<p>您好 {user.Name}，您的订单 #{order.Id} 已创建成功，应付金额 ¥{order.TotalAmount}。</p>");
        }

        return new OrderDto(
            order.Id, order.Status.ToString(), order.TotalAmount, order.DiscountAmount,
            order.CreatedAt, order.PaidAt, order.ShippedAt, order.CompletedAt,
            new AddressDto(address.Id, address.Recipient, address.Phone, address.Province, address.City, address.Detail, address.IsDefault),
            order.Items.Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.ProductImageUrl, i.Price, i.Quantity)).ToList());
    }
}
