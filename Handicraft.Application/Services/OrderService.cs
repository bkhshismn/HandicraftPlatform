using AutoMapper;
using Handicraft.Application.Contracts.Persistence;
using Handicraft.Application.Contracts.Services;
using Handicraft.Application.Services.Dtos;
using Handicraft.Domain.Entities;

namespace Handicraft.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public OrderService(
        IOrderRepository orderRepository,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    // ============================================================
    // Get Orders By User
    // ============================================================

    public async Task<IEnumerable<OrderDto>>
        GetOrdersByUserAsync(Guid userId)
    {
        var orders =
            await _orderRepository
                .GetOrdersByUserAsync(userId);

        return orders
            .Select(o => _mapper.Map<OrderDto>(o))
            .ToList();
    }

    // ============================================================
    // Get All Orders - Admin
    // ============================================================

    public async Task<IEnumerable<OrderDto>>
        GetAllOrdersAsync()
    {
        var orders =
            await _orderRepository
                .GetAllOrdersAsync();

        return orders
            .Select(o => _mapper.Map<OrderDto>(o))
            .ToList();
    }

    // ============================================================
    // Get Order By Id
    // ============================================================

    public async Task<OrderDto?> GetByIdAsync(Guid id)
    {
        var order =
            await _orderRepository
                .GetOrderWithItemsAsync(id);

        return order is null
            ? null
            : _mapper.Map<OrderDto>(order);
    }

    // ============================================================
    // Create Order
    // ============================================================

    public async Task<OrderDto> CreateAsync(
        OrderCreateDto dto,
        Guid userId)
    {
        var order =
            _mapper.Map<Order>(dto);

        // UserId فقط از JWT می‌آید.
        order.UserId = userId;

        // مشتری نمی‌تواند وضعیت یا قیمت را تعیین کند.
        order.Status = "Pending";
        order.EstimatedPrice = null;

        var created =
            await _orderRepository
                .AddAsync(order);

        return _mapper.Map<OrderDto>(created);
    }

    // ============================================================
    // Admin - Update Order
    // ============================================================

    public async Task<OrderDto?> UpdateByAdminAsync(
        Guid id,
        AdminUpdateOrderDto dto)
    {
        var order =
            await _orderRepository
                .UpdateByAdminAsync(
                    id,
                    dto.EstimatedPrice,
                    dto.Status);

        return order is null
            ? null
            : _mapper.Map<OrderDto>(order);
    }
}