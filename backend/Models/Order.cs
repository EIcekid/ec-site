namespace EcSite.Api.Models;

public enum OrderStatus
{
    PendingPayment = 0,
    Paid = 1,
    Shipped = 2,
    Completed = 3,
    Cancelled = 4
}

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }

    public int AddressId { get; set; }
    public Address? Address { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.PendingPayment;
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }

    public int? CouponId { get; set; }
    public Coupon? Coupon { get; set; }

    public int PointsUsed { get; set; }
    public int PointsEarned { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public List<OrderItem> Items { get; set; } = new();
}
