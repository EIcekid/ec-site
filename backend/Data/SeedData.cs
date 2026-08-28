using EcSite.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EcSite.Api.Data;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext db)
    {
        await db.Database.MigrateAsync();

        if (!await db.Users.AnyAsync(u => u.Role == UserRole.Admin))
        {
            db.Users.Add(new User
            {
                Email = "admin@ec-site.local",
                Name = "管理员",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = UserRole.Admin
            });
        }

        if (!await db.Categories.AnyAsync())
        {
            var electronics = new Category { Name = "数码电子" };
            var clothing = new Category { Name = "服饰鞋包" };
            var home = new Category { Name = "家居生活" };
            db.Categories.AddRange(electronics, clothing, home);
            await db.SaveChangesAsync();

            db.Products.AddRange(
                new Product
                {
                    Name = "无线蓝牙耳机", Description = "主动降噪，续航30小时", Price = 299.00m, Stock = 50,
                    CategoryId = electronics.Id,
                    Images = new List<ProductImage> { new() { Url = "https://picsum.photos/seed/headphone/600/600", SortOrder = 0 } }
                },
                new Product
                {
                    Name = "机械键盘", Description = "青轴，RGB背光", Price = 359.00m, Stock = 30,
                    CategoryId = electronics.Id,
                    Images = new List<ProductImage> { new() { Url = "https://picsum.photos/seed/keyboard/600/600", SortOrder = 0 } }
                },
                new Product
                {
                    Name = "智能手表", Description = "心率监测，7天续航", Price = 599.00m, Stock = 20,
                    CategoryId = electronics.Id,
                    Images = new List<ProductImage> { new() { Url = "https://picsum.photos/seed/watch/600/600", SortOrder = 0 } }
                },
                new Product
                {
                    Name = "纯棉T恤", Description = "简约百搭，透气舒适", Price = 89.00m, Stock = 100,
                    CategoryId = clothing.Id,
                    Images = new List<ProductImage> { new() { Url = "https://picsum.photos/seed/tshirt/600/600", SortOrder = 0 } }
                },
                new Product
                {
                    Name = "牛仔外套", Description = "秋冬百搭款", Price = 259.00m, Stock = 40,
                    CategoryId = clothing.Id,
                    Images = new List<ProductImage> { new() { Url = "https://picsum.photos/seed/jacket/600/600", SortOrder = 0 } }
                },
                new Product
                {
                    Name = "北欧风台灯", Description = "护眼三档调光", Price = 129.00m, Stock = 60,
                    CategoryId = home.Id,
                    Images = new List<ProductImage> { new() { Url = "https://picsum.photos/seed/lamp/600/600", SortOrder = 0 } }
                },
                new Product
                {
                    Name = "陶瓷马克杯套装", Description = "四件套，礼盒装", Price = 79.00m, Stock = 80,
                    CategoryId = home.Id,
                    Images = new List<ProductImage> { new() { Url = "https://picsum.photos/seed/mug/600/600", SortOrder = 0 } }
                }
            );

            db.Coupons.Add(new Coupon
            {
                Code = "WELCOME10",
                Type = CouponType.FixedAmount,
                Value = 10.00m,
                MinOrderAmount = 100.00m,
                ExpiresAt = DateTime.UtcNow.AddMonths(6),
                IsActive = true
            });
        }

        await db.SaveChangesAsync();
    }
}
