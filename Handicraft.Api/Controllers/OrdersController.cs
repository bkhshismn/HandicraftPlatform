using Handicraft.Application.Contracts.Services;
using Handicraft.Application.Services.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Handicraft.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // ============================================================
    // ADMIN - Get All Orders
    // ============================================================

    //[HttpGet("admin")]
    //[Authorize(Roles = "Admin")]
    //public async Task<ActionResult<IEnumerable<OrderDto>>>
    //GetAllForAdmin()
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<OrderDto>>>
    GetAllForAdmin()
    {
        var orders =
            await _orderService.GetAllOrdersAsync();

        return Ok(orders);
    }

    // ============================================================
    // Get Orders By Current User
    // ============================================================

    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetByUser(
        Guid userId)
    {
        var currentUserId = GetCurrentUserId();

        // کاربر عادی فقط سفارش‌های خودش را می‌بیند.
        // Admin می‌تواند سفارش‌های هر کاربر را ببیند.
        if (!User.IsInRole("Admin") &&
            currentUserId != userId)
        {
            return Forbid();
        }

        var orders =
            await _orderService
                .GetOrdersByUserAsync(userId);

        return Ok(orders);
    }

    // ============================================================
    // Get Order By Id
    // ============================================================

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetById(
        Guid id)
    {
        var order =
            await _orderService.GetByIdAsync(id);

        if (order is null)
            return NotFound();

        var currentUserId = GetCurrentUserId();

        if (!User.IsInRole("Admin") &&
            order.UserId != currentUserId)
        {
            return Forbid();
        }

        return Ok(order);
    }

    // ============================================================
    // Create Order
    // ============================================================

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(
        [FromBody] OrderCreateDto dto)
    {
        if (dto is null)
            return BadRequest();

        var userId = GetCurrentUserId();

        var created =
            await _orderService.CreateAsync(
                dto,
                userId);

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                id = created.Id
            },
            created);
    }

    // ============================================================
    // Current User Id
    // ============================================================

    private Guid GetCurrentUserId()
    {
        var userIdClaim =
            User.FindFirst(
                JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim))
        {
            userIdClaim =
                User.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value;
        }

        if (!Guid.TryParse(
                userIdClaim,
                out var userId))
        {
            throw new UnauthorizedAccessException(
                "شناسه کاربر در توکن پیدا نشد.");
        }

        return userId;
    }
    // ============================================================
    // ADMIN - Update Order
    // ============================================================

    [HttpPut("{id:guid}/admin")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<OrderDto>> UpdateByAdmin(
        Guid id,
        [FromBody] AdminUpdateOrderDto dto)
    {
        if (dto is null)
            return BadRequest();

        if (dto.EstimatedPrice.HasValue &&
            dto.EstimatedPrice.Value < 0)
        {
            return BadRequest(
                "قیمت نمی‌تواند منفی باشد.");
        }

        var allowedStatuses = new[]
        {
        "Pending",
        "InProgress",
        "Completed",
        "Cancelled"
    };

        if (!allowedStatuses.Contains(dto.Status))
        {
            return BadRequest(
                "وضعیت سفارش نامعتبر است.");
        }

        var updated =
            await _orderService.UpdateByAdminAsync(
                id,
                dto);

        if (updated is null)
            return NotFound();

        return Ok(updated);
    }
}