namespace Handicraft.Application.Services.Dtos;

public record PortfolioCreateDto(
    string Title,
    string Category,
    string Description,
    bool IsVisible
);
