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
        string status);
}