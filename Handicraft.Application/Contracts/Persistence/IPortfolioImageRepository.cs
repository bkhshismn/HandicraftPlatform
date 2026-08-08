using Handicraft.Domain.Entities;

namespace Handicraft.Application.Contracts.Persistence;

public interface IPortfolioImageRepository
    : IGenericRepository<PortfolioImage>
{
    Task<IReadOnlyList<PortfolioImage>> GetByPortfolioIdAsync(Guid portfolioId);
}