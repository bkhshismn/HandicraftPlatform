namespace Handicraft.Domain.Entities;

public class PortfolioItem : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // face, statue, decor
    public string Description { get; set; } = string.Empty;
    public bool IsVisible { get; set; } = true;
    public ICollection<PortfolioImage> Images { get; set; }
    = new List<PortfolioImage>();
}
