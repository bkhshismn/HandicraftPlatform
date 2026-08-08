namespace Handicraft.Application.Services.Dtos;

public record PortfolioImageReorderDto(
    IReadOnlyList<PortfolioImageOrderDto> Images
);