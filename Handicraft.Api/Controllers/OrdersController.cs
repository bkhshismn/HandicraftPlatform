using Handicraft.Application.Contracts.Services;
using Handicraft.Application.Services.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Handicraft.Domain.Entities;
using Handicraft.Infrastructure.Persistence.Data;

namespace Handicraft.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly AppDbContext _db;

    public OrdersController(IOrderService orderService, AppDbContext db)
    {
        _orderService = orderService;
        _db = db;
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

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<OrderDto>> UpdateByUser(Guid id, [FromBody] OrderCreateDto dto)
    {
        if (dto is null || string.IsNullOrWhiteSpace(dto.Type) || dto.Type.Length > 50 || string.IsNullOrWhiteSpace(dto.Description) || dto.Description.Length > 2000 || dto.ImageUrl?.Length > 500)
            return BadRequest();
        var updated = await _orderService.UpdateByUserAsync(id, GetCurrentUserId(), dto);
        if (updated is null) return NotFound();
        await NotifyOrderChangedAsync(updated, "ویرایش سفارش", "اطلاعات سفارش شما توسط خودتان ویرایش شد.");
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteByUser(Guid id)
    {
        var ownerId = GetCurrentUserId();
        var existing = await _orderService.GetByIdAsync(id);
        if (existing is null || existing.UserId != ownerId) return NotFound();
        if (!await _orderService.DeleteAsync(id, ownerId)) return NotFound();
        await NotifyOrderChangedAsync(existing, "حذف سفارش", "سفارش شما توسط خودتان حذف شد.");
        return NoContent();
    }

    [HttpDelete("{id:guid}/admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteByAdmin(Guid id)
    {
        var existing = await _orderService.GetByIdAsync(id);
        if (existing is null) return NotFound();
        if (!await _orderService.DeleteAsync(id)) return NotFound();
        await NotifyOrderChangedAsync(existing, "حذف سفارش", "ادمین سفارش شما را حذف کرد.");
        return NoContent();
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

        if ((!string.IsNullOrWhiteSpace(dto.Type) && dto.Type.Length > 50) || dto.Description?.Length > 2000)
            return BadRequest("اطلاعات سفارش بیش از حد مجاز است.");

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

        await NotifyOrderChangedAsync(updated, "تغییر سفارش", "قیمت یا وضعیت سفارش شما توسط ادمین تغییر کرد.");
        return Ok(updated);
    }

    private async Task NotifyOrderChangedAsync(OrderDto order, string title, string userMessage)
    {
        var now = DateTime.UtcNow;
        _db.AdminNotifications.AddRange(
            new AdminNotification { Id = Guid.NewGuid(), Title = title, Message = $"سفارش {order.Type} (شناسه {order.Id}) تغییر کرد.", TargetUrl = "/admin/orders", CreatedAt = now },
            new AdminNotification { Id = Guid.NewGuid(), RecipientUserId = order.UserId, Title = title, Message = userMessage, TargetUrl = "/orders", CreatedAt = now });
        await _db.SaveChangesAsync();
    }
}
