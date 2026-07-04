using Handicraft.Application.Services.Dtos;

namespace Handicraft.Application.Services;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetOrdersByUserAsync(Guid userId);
    Task<OrderDto?> GetByIdAsync(Guid id);
    Task<OrderDto> CreateAsync(OrderCreateDto dto);
}
