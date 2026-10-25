using Handicraft.Domain.Entities;

namespace Handicraft.Application.Contracts.Persistence;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<IReadOnlyList<Order>> GetOrdersByUserAsync(
        Guid userId);

    Task<IReadOnlyList<Order>> GetAllOrdersAsync();

    Task<Order?> GetOrderWithItemsAsync(
        Guid orderId);

    Task<Order?> UpdateByAdminAsync(
        Guid id,
        decimal? estimatedPrice,
        string status,
        string? type = null,
        string? description = null);

    Task<Order?> UpdateByUserAsync(Guid id, Guid userId, string type, string description, string? imageUrl);
    Task<bool> DeleteAsync(Guid id, Guid? ownerId = null);
}
