using Handicraft.Application.Services.Dtos;

namespace Handicraft.Application.Services;

public interface IPortfolioService
{
    Task<IEnumerable<PortfolioItemDto>> GetPublishedAsync();
    Task<IEnumerable<PortfolioItemDto>> GetByCategoryAsync(string category);
    Task<PortfolioItemDto> CreateAsync(PortfolioCreateDto dto);
}
