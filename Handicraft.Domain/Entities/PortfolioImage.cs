namespace Handicraft.Domain.Entities;

public class PortfolioImage
{
    public Guid Id { get; set; }

    public Guid PortfolioItemId { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public int Order { get; set; }

    public PortfolioItem PortfolioItem { get; set; } = null!;
}