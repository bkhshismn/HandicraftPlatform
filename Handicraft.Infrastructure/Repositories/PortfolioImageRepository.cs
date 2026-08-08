using Handicraft.Application.Contracts.Persistence;
using Handicraft.Domain.Entities;
using Handicraft.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Handicraft.Infrastructure.Repositories;

public sealed class PortfolioImageRepository
    : GenericRepository<PortfolioImage>,
      IPortfolioImageRepository
{
    public PortfolioImageRepository(
        AppDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<PortfolioImage>>
        GetByPortfolioIdAsync(Guid portfolioId)
    {
        return await _context.PortfolioImages
            .Where(x => x.PortfolioItemId == portfolioId)
            .OrderBy(x => x.Order)
            .ToListAsync();
    }
}