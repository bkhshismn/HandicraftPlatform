using Handicraft.Application.Contracts.Persistence;
using Handicraft.Domain.Entities;
using Handicraft.Infrastructure.Persistence;
using Handicraft.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Handicraft.Infrastructure.Repositories;

public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<Order>> GetOrdersByUserAsync(
        Guid userId)
    {
        return await _context.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<Order?> GetOrderWithItemsAsync(
        Guid orderId)
    {
        return await _context.Orders
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<IReadOnlyList<Order>> GetAllOrdersAsync()
    {
        return await _context.Orders
            .Include(o => o.User)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }
    public async Task<Order?> UpdateByAdminAsync(
    Guid id,
    decimal? estimatedPrice,
    string status)
    {
        var order =
            await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
            return null;

        order.EstimatedPrice = estimatedPrice;
        order.Status = status;

        await _context.SaveChangesAsync();

        return order;
    }
}