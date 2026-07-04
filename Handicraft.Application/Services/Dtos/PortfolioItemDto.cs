namespace Handicraft.Application.Services.Dtos;

public record PortfolioItemDto(
    Guid Id,
    string Title,
    string Category,
    string ImageUrl,
    string Description,
    bool IsVisible,
    DateTime CreatedAt
);
