using EcSite.Api.Data;
using EcSite.Api.DTOs.Orders;
using EcSite.Api.Models;
using EcSite.Api.Services;

namespace EcSite.Api.Tests;

public class OrderServiceTests : IDisposable
{
    private readonly TestDbContextFactory _factory;
    private readonly AppDbContext _db;
    private readonly FakeEmailService _email = new();
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _factory = new TestDbContextFactory();
        _db = _factory.Db;
        _sut = new OrderService(_db, _email);
    }

    public void Dispose() => _factory.Dispose();

    private (User user, Address address, Category category) SeedUserAndAddress(int points = 0)
    {
        var user = new User { Email = "u@test.local", Name = "Test User", PasswordHash = "x", Points = points };
        var category = new Category { Name = "Cat" };
        _db.Users.Add(user);
        _db.Categories.Add(category);
        _db.SaveChanges();

        var address = new Address
        {
            UserId = user.Id, Recipient = "R", Phone = "000", Province = "P", City = "C", Detail = "D"
        };
        _db.Addresses.Add(address);
        _db.SaveChanges();

        return (user, address, category);
    }

    private Product SeedProduct(Category category, decimal price, int stock)
    {
        var product = new Product { Name = "Widget", Description = "d", Price = price, Stock = stock, CategoryId = category.Id, IsActive = true };
        _db.Products.Add(product);
        _db.SaveChanges();
        return product;
    }

    [Fact]
    public async Task CreateOrderAsync_throws_when_cart_is_empty()
    {
        var (user, address, _) = SeedUserAndAddress();

        var ex = await Assert.ThrowsAsync<OrderServiceException>(() =>
            _sut.CreateOrderAsync(user.Id, new CreateOrderRequest(address.Id, null, 0)));

        Assert.Equal("カートが空です", ex.Message);
    }

    [Fact]
    public async Task CreateOrderAsync_throws_when_address_does_not_belong_to_user()
    {
        var (user, _, category) = SeedUserAndAddress();
        var product = SeedProduct(category, 100m, 10);
        _db.CartItems.Add(new CartItem { UserId = user.Id, ProductId = product.Id, Quantity = 1 });
        _db.SaveChanges();

        var ex = await Assert.ThrowsAsync<OrderServiceException>(() =>
            _sut.CreateOrderAsync(user.Id, new CreateOrderRequest(AddressId: 9999, null, 0)));

        Assert.Equal("配送先住所が存在しません", ex.Message);
    }

    [Fact]
    public async Task CreateOrderAsync_throws_when_stock_is_insufficient()
    {
        var (user, address, category) = SeedUserAndAddress();
        var product = SeedProduct(category, 100m, 1);
        _db.CartItems.Add(new CartItem { UserId = user.Id, ProductId = product.Id, Quantity = 5 });
        _db.SaveChanges();

        var ex = await Assert.ThrowsAsync<OrderServiceException>(() =>
            _sut.CreateOrderAsync(user.Id, new CreateOrderRequest(address.Id, null, 0)));

        Assert.Contains("在庫が不足", ex.Message);
    }

    [Fact]
    public async Task CreateOrderAsync_deducts_stock_and_totals_correctly_with_no_discount()
    {
        var (user, address, category) = SeedUserAndAddress();
        var product = SeedProduct(category, 100m, 10);
        _db.CartItems.Add(new CartItem { UserId = user.Id, ProductId = product.Id, Quantity = 3 });
        _db.SaveChanges();

        var order = await _sut.CreateOrderAsync(user.Id, new CreateOrderRequest(address.Id, null, 0));

        Assert.Equal(300m, order.TotalAmount);
        Assert.Equal(0m, order.DiscountAmount);
        Assert.Equal(7, (await _db.Products.FindAsync(product.Id))!.Stock);
        Assert.Empty(_db.CartItems.Where(c => c.UserId == user.Id));
        Assert.Single(_email.Sent);
    }

    [Fact]
    public async Task CreateOrderAsync_applies_fixed_amount_coupon()
    {
        var (user, address, category) = SeedUserAndAddress();
        var product = SeedProduct(category, 100m, 10);
        _db.CartItems.Add(new CartItem { UserId = user.Id, ProductId = product.Id, Quantity = 2 });
        _db.Coupons.Add(new Coupon
        {
            Code = "SAVE10", Type = CouponType.FixedAmount, Value = 10m, MinOrderAmount = 50m,
            ExpiresAt = DateTime.UtcNow.AddDays(1), IsActive = true
        });
        _db.SaveChanges();

        var order = await _sut.CreateOrderAsync(user.Id, new CreateOrderRequest(address.Id, "SAVE10", 0));

        Assert.Equal(190m, order.TotalAmount);
        Assert.Equal(10m, order.DiscountAmount);
    }

    [Fact]
    public async Task CreateOrderAsync_applies_percentage_coupon()
    {
        var (user, address, category) = SeedUserAndAddress();
        var product = SeedProduct(category, 100m, 10);
        _db.CartItems.Add(new CartItem { UserId = user.Id, ProductId = product.Id, Quantity = 2 });
        _db.Coupons.Add(new Coupon
        {
            Code = "10PCT", Type = CouponType.Percentage, Value = 10m, MinOrderAmount = 0m,
            ExpiresAt = DateTime.UtcNow.AddDays(1), IsActive = true
        });
        _db.SaveChanges();

        var order = await _sut.CreateOrderAsync(user.Id, new CreateOrderRequest(address.Id, "10PCT", 0));

        Assert.Equal(180m, order.TotalAmount);
        Assert.Equal(20m, order.DiscountAmount);
    }

    [Fact]
    public async Task CreateOrderAsync_throws_when_order_total_is_below_coupon_minimum()
    {
        var (user, address, category) = SeedUserAndAddress();
        var product = SeedProduct(category, 10m, 10);
        _db.CartItems.Add(new CartItem { UserId = user.Id, ProductId = product.Id, Quantity = 1 });
        _db.Coupons.Add(new Coupon
        {
            Code = "SAVE10", Type = CouponType.FixedAmount, Value = 10m, MinOrderAmount = 50m,
            ExpiresAt = DateTime.UtcNow.AddDays(1), IsActive = true
        });
        _db.SaveChanges();

        var ex = await Assert.ThrowsAsync<OrderServiceException>(() =>
            _sut.CreateOrderAsync(user.Id, new CreateOrderRequest(address.Id, "SAVE10", 0)));

        Assert.Contains("以上のご注文", ex.Message);
    }

    [Fact]
    public async Task CreateOrderAsync_throws_when_coupon_is_expired()
    {
        var (user, address, category) = SeedUserAndAddress();
        var product = SeedProduct(category, 100m, 10);
        _db.CartItems.Add(new CartItem { UserId = user.Id, ProductId = product.Id, Quantity = 1 });
        _db.Coupons.Add(new Coupon
        {
            Code = "OLD", Type = CouponType.FixedAmount, Value = 10m, MinOrderAmount = 0m,
            ExpiresAt = DateTime.UtcNow.AddDays(-1), IsActive = true
        });
        _db.SaveChanges();

        var ex = await Assert.ThrowsAsync<OrderServiceException>(() =>
            _sut.CreateOrderAsync(user.Id, new CreateOrderRequest(address.Id, "OLD", 0)));

        Assert.Equal("クーポンの有効期限が切れています", ex.Message);
    }

    [Fact]
    public async Task CreateOrderAsync_redeems_points_and_deducts_balance()
    {
        var (user, address, category) = SeedUserAndAddress(points: 500);
        var product = SeedProduct(category, 100m, 10);
        _db.CartItems.Add(new CartItem { UserId = user.Id, ProductId = product.Id, Quantity = 1 });
        _db.SaveChanges();

        var order = await _sut.CreateOrderAsync(user.Id, new CreateOrderRequest(address.Id, null, 200));

        Assert.Equal(80m, order.TotalAmount); // ¥100 - (200pt * ¥0.1)
        Assert.Equal(200, order.PointsUsed);
        Assert.Equal(300, (await _db.Users.FindAsync(user.Id))!.Points);
    }

    [Fact]
    public async Task CreateOrderAsync_caps_points_discount_at_order_total()
    {
        var (user, address, category) = SeedUserAndAddress(points: 5000);
        var product = SeedProduct(category, 10m, 10);
        _db.CartItems.Add(new CartItem { UserId = user.Id, ProductId = product.Id, Quantity = 1 });
        _db.SaveChanges();

        // 5000pt would be worth ¥500, far more than the ¥10 order total.
        var order = await _sut.CreateOrderAsync(user.Id, new CreateOrderRequest(address.Id, null, 5000));

        Assert.Equal(0m, order.TotalAmount);
        Assert.Equal(10m, order.DiscountAmount);
    }

    [Fact]
    public async Task CreateOrderAsync_throws_when_points_exceed_balance()
    {
        var (user, address, category) = SeedUserAndAddress(points: 50);
        var product = SeedProduct(category, 100m, 10);
        _db.CartItems.Add(new CartItem { UserId = user.Id, ProductId = product.Id, Quantity = 1 });
        _db.SaveChanges();

        var ex = await Assert.ThrowsAsync<OrderServiceException>(() =>
            _sut.CreateOrderAsync(user.Id, new CreateOrderRequest(address.Id, null, 100)));

        Assert.Equal("保有ポイントが不足しています", ex.Message);
    }

    [Fact]
    public async Task CreateOrderAsync_uses_variant_price_and_deducts_variant_stock_not_product_stock()
    {
        var (user, address, category) = SeedUserAndAddress();
        var product = SeedProduct(category, 100m, 999); // product-level stock should stay untouched
        var variant = new ProductVariant { ProductId = product.Id, Size = "L", Sku = "SKU-L", PriceDelta = 15m, Stock = 5 };
        _db.ProductVariants.Add(variant);
        _db.SaveChanges();

        _db.CartItems.Add(new CartItem { UserId = user.Id, ProductId = product.Id, ProductVariantId = variant.Id, Quantity = 2 });
        _db.SaveChanges();

        var order = await _sut.CreateOrderAsync(user.Id, new CreateOrderRequest(address.Id, null, 0));

        Assert.Equal(230m, order.TotalAmount); // (100 + 15) * 2
        Assert.Equal("サイズ：L", order.Items[0].VariantLabel);
        Assert.Equal(999, (await _db.Products.FindAsync(product.Id))!.Stock);
        Assert.Equal(3, (await _db.ProductVariants.FindAsync(variant.Id))!.Stock);
    }
}
