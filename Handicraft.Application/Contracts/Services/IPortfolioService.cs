using Handicraft.Application.Services.Dtos;

namespace Handicraft.Application.Contracts.Services;

public interface IPortfolioService
{
    Task<IEnumerable<PortfolioItemDto>> GetAllAsync();
    Task<IEnumerable<PortfolioItemDto>> GetPublishedAsync();
    Task<IEnumerable<PortfolioItemDto>> GetByCategoryAsync(string category);
    Task<PortfolioItemDto> CreateAsync(PortfolioCreateDto dto);
    Task<PortfolioItemDto?> GetByIdAsync(Guid id);
    Task<PortfolioItemDto?> UpdateAsync(Guid id, PortfolioCreateDto dto);
    Task<bool> DeleteAsync(Guid id);
}
