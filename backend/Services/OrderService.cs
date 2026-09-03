using EcSite.Api.Data;
using EcSite.Api.DTOs.Orders;
using EcSite.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EcSite.Api.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;
    private readonly IEmailService _emailService;

    public const decimal PointValueYen = 0.1m; // 1ポイント = ¥0.1（100ポイント = ¥10）
    public const int PointsEarnRateYen = 10; // ¥10の購入ごとに1ポイント

    public OrderService(AppDbContext db, IEmailService emailService)
    {
        _db = db;
        _emailService = emailService;
    }

    public async Task<OrderDto> CreateOrderAsync(int userId, CreateOrderRequest request)
    {
        var address = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == request.AddressId && a.UserId == userId);
        if (address is null) throw new OrderServiceException("配送先住所が存在しません");

        var user = await _db.Users.FindAsync(userId);
        if (user is null) throw new OrderServiceException("ユーザーが存在しません");

        if (request.PointsToUse < 0) throw new OrderServiceException("ポイント数が不正です");
        if (request.PointsToUse > user.Points) throw new OrderServiceException("保有ポイントが不足しています");

        var cartItems = await _db.CartItems
            .Include(c => c.Product!).ThenInclude(p => p.Images)
            .Include(c => c.ProductVariant)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (cartItems.Count == 0) throw new OrderServiceException("カートが空です");

        foreach (var item in cartItems)
        {
            if (item.Product is null || !item.Product.IsActive)
                throw new OrderServiceException($"商品「{item.ProductId}」は販売を終了しました");

            var stock = item.ProductVariant?.Stock ?? item.Product.Stock;
            if (item.Quantity > stock)
                throw new OrderServiceException($"商品「{item.Product.Name}」の在庫が不足しています");
        }

        var totalAmount = cartItems.Sum(c => (c.Product!.Price + (c.ProductVariant?.PriceDelta ?? 0)) * c.Quantity);

        Coupon? coupon = null;
        var couponDiscount = 0m;
        if (!string.IsNullOrWhiteSpace(request.CouponCode))
        {
            coupon = await _db.Coupons.FirstOrDefaultAsync(c => c.Code == request.CouponCode && c.IsActive);
            if (coupon is null) throw new OrderServiceException("クーポンが存在しないか、無効になっています");
            if (coupon.ExpiresAt < DateTime.UtcNow) throw new OrderServiceException("クーポンの有効期限が切れています");
            if (totalAmount < coupon.MinOrderAmount) throw new OrderServiceException($"このクーポンは{coupon.MinOrderAmount}円以上のご注文でご利用いただけます");

            couponDiscount = coupon.Type == CouponType.FixedAmount
                ? coupon.Value
                : Math.Round(totalAmount * coupon.Value / 100m, 2);
            couponDiscount = Math.Min(couponDiscount, totalAmount);
        }

        var afterCoupon = totalAmount - couponDiscount;
        var pointsDiscount = Math.Min(request.PointsToUse * PointValueYen, afterCoupon);
        var discountAmount = couponDiscount + pointsDiscount;

        await using var transaction = await _db.Database.BeginTransactionAsync();

        var order = new Order
        {
            UserId = userId,
            AddressId = address.Id,
            Status = OrderStatus.PendingPayment,
            TotalAmount = totalAmount - discountAmount,
            DiscountAmount = discountAmount,
            CouponId = coupon?.Id,
            PointsUsed = request.PointsToUse,
        };

        foreach (var item in cartItems)
        {
            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                ProductName = item.Product!.Name,
                ProductImageUrl = item.Product.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).FirstOrDefault(),
                ProductVariantId = item.ProductVariantId,
                VariantLabel = item.ProductVariant?.Label,
                Price = item.Product.Price + (item.ProductVariant?.PriceDelta ?? 0),
                Quantity = item.Quantity
            });

            if (item.ProductVariant is not null)
                item.ProductVariant.Stock -= item.Quantity;
            else
                item.Product.Stock -= item.Quantity;
        }

        user.Points -= request.PointsToUse;

        _db.Orders.Add(order);
        _db.CartItems.RemoveRange(cartItems);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        _ = _emailService.SendAsync(
            user.Email,
            $"ご注文確認 #{order.Id}",
            $"<p>{user.Name} 様、ご注文 #{order.Id} を承りました。お支払い金額は ¥{order.TotalAmount} です。</p>");

        return new OrderDto(
            order.Id, order.Status.ToString(), order.TotalAmount, order.DiscountAmount,
            order.PointsUsed, order.PointsEarned,
            order.CreatedAt, order.PaidAt, order.ShippedAt, order.CompletedAt,
            new AddressDto(address.Id, address.Recipient, address.Phone, address.Province, address.City, address.Detail, address.IsDefault),
            order.Items.Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.ProductImageUrl, i.VariantLabel, i.Price, i.Quantity)).ToList());
    }
}
