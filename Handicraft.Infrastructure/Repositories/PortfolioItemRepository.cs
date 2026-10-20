using Handicraft.Application.Contracts.Persistence;
using Handicraft.Domain.Entities;
using Handicraft.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Handicraft.Infrastructure.Repositories;

public class PortfolioItemRepository
    : GenericRepository<PortfolioItem>,
      IPortfolioItemRepository
{
    public PortfolioItemRepository(
        AppDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<PortfolioItem>> GetAllWithImagesAsync()
    {
        return await _context.PortfolioItems
            .Include(x => x.Images)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<PortfolioItem>>
        GetPublishedItemsAsync()
    {
        return await _context.PortfolioItems
            .Include(x => x.Images)
            .Where(x => x.IsVisible)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<PortfolioItem>>
        GetItemsByCategoryAsync(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return [];

        return await _context.PortfolioItems
            .Include(x => x.Images)
            .Where(x =>
                x.IsVisible &&
                x.Category.ToLower() ==
                category.Trim().ToLower())
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public override async Task<PortfolioItem?> GetByIdAsync(Guid id)
    {
        return await _context.PortfolioItems
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}
