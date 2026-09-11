namespace EcSite.Api.Models;

public class ReviewVote
{
    public int Id { get; set; }
    public int ReviewId { get; set; }
    public Review? Review { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
