using Handicraft.Application.Contracts.Persistence;
using Handicraft.Domain.Entities;
using Handicraft.Infrastructure.Persistence;
using Handicraft.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Handicraft.Infrastructure.Repositories;

public class PortfolioItemRepository : GenericRepository<PortfolioItem>, IPortfolioItemRepository
{
    public PortfolioItemRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<PortfolioItem>> GetPublishedItemsAsync()
    {
        // The domain uses `IsVisible` for visibility of portfolio items.
        return await _context.PortfolioItems
            .Where(p => p.IsVisible)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<PortfolioItem>> GetItemsByCategoryAsync(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return Array.Empty<PortfolioItem>();

        return await _context.PortfolioItems
            .Where(p => p.IsVisible && p.Category.ToLower() == category.Trim().ToLower())
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }
}
