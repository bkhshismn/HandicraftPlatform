namespace Handicraft.Domain.Entities;

public class Order : BaseEntity
{
    public Guid UserId { get; set; }
    public string Type { get; set; } = string.Empty; // statue, painting, custom
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Cancelled
    public decimal? EstimatedPrice { get; set; }

    // Navigation Property
    public User User { get; set; } = null!;
}
