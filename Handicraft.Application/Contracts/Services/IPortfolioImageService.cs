using Handicraft.Application.Services.Dtos;

namespace Handicraft.Application.Contracts.Services;

public interface IPortfolioImageService
{
    Task<IReadOnlyList<PortfolioImageDto>> GetByPortfolioIdAsync(
        Guid portfolioId);

    Task<PortfolioImageDto> AddAsync(
        Guid portfolioId,
        PortfolioImageCreateDto dto);

    Task DeleteAsync(Guid imageId);

    Task ReorderAsync(
        PortfolioImageReorderDto dto);
}