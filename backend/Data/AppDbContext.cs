using EcSite.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EcSite.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).HasMaxLength(256);
            e.Property(u => u.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.Property(p => p.Price).HasColumnType("decimal(18,2)");
            e.HasOne(p => p.Category).WithMany(c => c.Products).HasForeignKey(p => p.CategoryId);
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.HasOne(c => c.Parent).WithMany(c => c.Children).HasForeignKey(c => c.ParentId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CartItem>(e =>
        {
            // SQL Server treats each NULL as distinct in a unique index, so the no-variant case
            // needs its own filtered index on (UserId, ProductId) to actually enforce uniqueness.
            e.HasIndex(c => new { c.UserId, c.ProductId })
                .IsUnique()
                .HasFilter("[ProductVariantId] IS NULL")
                .HasDatabaseName("IX_CartItems_UserId_ProductId_NoVariant");
            e.HasIndex(c => new { c.UserId, c.ProductId, c.ProductVariantId })
                .IsUnique()
                .HasFilter("[ProductVariantId] IS NOT NULL")
                .HasDatabaseName("IX_CartItems_UserId_ProductId_ProductVariantId");
            e.HasOne(c => c.User).WithMany(u => u.CartItems).HasForeignKey(c => c.UserId);
            e.HasOne(c => c.Product).WithMany().HasForeignKey(c => c.ProductId);
            e.HasOne(c => c.ProductVariant).WithMany().HasForeignKey(c => c.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductVariant>(e =>
        {
            e.Property(v => v.PriceDelta).HasColumnType("decimal(18,2)");
            e.HasOne(v => v.Product).WithMany(p => p.Variants).HasForeignKey(v => v.ProductId);
        });

        modelBuilder.Entity<WishlistItem>(e =>
        {
            e.HasIndex(w => new { w.UserId, w.ProductId }).IsUnique();
            e.HasOne(w => w.User).WithMany(u => u.WishlistItems).HasForeignKey(w => w.UserId);
            e.HasOne(w => w.Product).WithMany().HasForeignKey(w => w.ProductId);
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
            e.Property(o => o.DiscountAmount).HasColumnType("decimal(18,2)");
            e.HasOne(o => o.User).WithMany(u => u.Orders).HasForeignKey(o => o.UserId);
            e.HasOne(o => o.Address).WithMany().HasForeignKey(o => o.AddressId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(o => o.Coupon).WithMany().HasForeignKey(o => o.CouponId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderItem>(e =>
        {
            e.Property(i => i.Price).HasColumnType("decimal(18,2)");
            e.HasOne(i => i.Order).WithMany(o => o.Items).HasForeignKey(i => i.OrderId);
            // SetNull (not Restrict): admin product edits replace variants wholesale, which would
            // otherwise fail once a variant has been referenced by an order. VariantLabel keeps the
            // historical snapshot even if the ProductVariant row is later deleted.
            e.HasOne<ProductVariant>().WithMany().HasForeignKey(i => i.ProductVariantId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Coupon>(e =>
        {
            e.HasIndex(c => c.Code).IsUnique();
            e.Property(c => c.Value).HasColumnType("decimal(18,2)");
            e.Property(c => c.MinOrderAmount).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Review>(e =>
        {
            e.HasOne(r => r.Product).WithMany(p => p.Reviews).HasForeignKey(r => r.ProductId);
            e.HasOne(r => r.User).WithMany(u => u.Reviews).HasForeignKey(r => r.UserId);
        });

        modelBuilder.Entity<Address>(e =>
        {
            e.HasOne(a => a.User).WithMany(u => u.Addresses).HasForeignKey(a => a.UserId);
        });
    }
}
