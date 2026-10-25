namespace Handicraft.Domain.Entities;

public sealed class ShopProduct
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = "/images/hero-handicrafts.png";
    public string Badge { get; set; } = "دست‌ساز";
    public string ImagePosition { get; set; } = "center";
    public string Availability { get; set; } = "هماهنگی با کارگاه";
    public bool IsPublished { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
