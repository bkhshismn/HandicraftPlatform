using Handicraft.Domain.Entities;

namespace Handicraft.Application.Contracts.Persistence;

public interface IPortfolioItemRepository
    : IGenericRepository<PortfolioItem>
{
    Task<IReadOnlyList<PortfolioItem>> GetPublishedItemsAsync();

    Task<IReadOnlyList<PortfolioItem>> GetItemsByCategoryAsync(
        string category);
}