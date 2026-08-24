using Handicraft.Application.Services.Dtos;

namespace Handicraft.Application.Contracts.Services;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetOrdersByUserAsync(
        Guid userId);

    Task<IEnumerable<OrderDto>> GetAllOrdersAsync();

    Task<OrderDto?> GetByIdAsync(
        Guid id);

    Task<OrderDto> CreateAsync(
        OrderCreateDto dto,
        Guid userId);

    Task<OrderDto?> UpdateByAdminAsync(
        Guid id,
        AdminUpdateOrderDto dto);
}