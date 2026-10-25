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
    string status,
    string? type = null,
    string? description = null)
    {
        var order =
            await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
            return null;

        order.EstimatedPrice = estimatedPrice;
        order.Status = status;
        if (!string.IsNullOrWhiteSpace(type)) order.Type = type.Trim();
        if (description is not null) order.Description = description.Trim();

        await _context.SaveChangesAsync();

        return order;
    }

    public async Task<Order?> UpdateByUserAsync(Guid id, Guid userId, string type, string description, string? imageUrl)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);
        if (order is null) return null;
        order.Type = type;
        order.Description = description;
        order.ImageUrl = imageUrl;
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid? ownerId = null)
    {
        var query = _context.Orders.Where(o => o.Id == id);
        if (ownerId.HasValue)
            query = query.Where(o => o.UserId == ownerId.Value);
        var order = await query.FirstOrDefaultAsync();
        if (order is null) return false;
        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
        return true;
    }
}
