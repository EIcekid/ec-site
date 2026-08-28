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
                Name = "管理者",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = UserRole.Admin
            });
        }

        if (!await db.Categories.AnyAsync())
        {
            var electronics = new Category { Name = "デジタル家電" };
            var clothing = new Category { Name = "ファッション・シューズ" };
            var home = new Category { Name = "生活雑貨" };
            db.Categories.AddRange(electronics, clothing, home);
            await db.SaveChangesAsync();

            db.Products.AddRange(
                new Product
                {
                    Name = "ワイヤレスイヤホン", Description = "アクティブノイズキャンセリング、連続30時間再生", Price = 299.00m, Stock = 50,
                    CategoryId = electronics.Id,
                    Images = new List<ProductImage> { new() { Url = "https://picsum.photos/seed/headphone/600/600", SortOrder = 0 } }
                },
                new Product
                {
                    Name = "メカニカルキーボード", Description = "青軸、RGBバックライト搭載", Price = 359.00m, Stock = 30,
                    CategoryId = electronics.Id,
                    Images = new List<ProductImage> { new() { Url = "https://picsum.photos/seed/keyboard/600/600", SortOrder = 0 } }
                },
                new Product
                {
                    Name = "スマートウォッチ", Description = "心拍数モニター、7日間バッテリー", Price = 599.00m, Stock = 20,
                    CategoryId = electronics.Id,
                    Images = new List<ProductImage> { new() { Url = "https://picsum.photos/seed/watch/600/600", SortOrder = 0 } }
                },
                new Product
                {
                    Name = "コットンTシャツ", Description = "シンプルで合わせやすく、着心地も快適", Price = 89.00m, Stock = 100,
                    CategoryId = clothing.Id,
                    Images = new List<ProductImage> { new() { Url = "https://picsum.photos/seed/tshirt/600/600", SortOrder = 0 } }
                },
                new Product
                {
                    Name = "デニムジャケット", Description = "秋冬のコーディネートに使えるアイテム", Price = 259.00m, Stock = 40,
                    CategoryId = clothing.Id,
                    Images = new List<ProductImage> { new() { Url = "https://picsum.photos/seed/jacket/600/600", SortOrder = 0 } }
                },
                new Product
                {
                    Name = "北欧風テーブルランプ", Description = "目に優しい3段階調光機能付き", Price = 129.00m, Stock = 60,
                    CategoryId = home.Id,
                    Images = new List<ProductImage> { new() { Url = "https://picsum.photos/seed/lamp/600/600", SortOrder = 0 } }
                },
                new Product
                {
                    Name = "陶器マグカップセット", Description = "4点セット、ギフトボックス入り", Price = 79.00m, Stock = 80,
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
