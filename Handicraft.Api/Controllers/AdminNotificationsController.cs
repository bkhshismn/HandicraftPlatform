using Handicraft.Infrastructure.Persistence.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Handicraft.Api.Controllers;

[ApiController]
[Route("api/admin-notifications")]
[Authorize(Roles = "Admin")]
public sealed class AdminNotificationsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUnread()
    {
        var notifications = await db.AdminNotifications
            .AsNoTracking()
            .Where(x => !x.IsRead && x.RecipientUserId == null)
            .OrderByDescending(x => x.CreatedAt)
            .Take(50)
            .Select(x => new AdminNotificationDto(x.Id, x.Title, x.Message, x.TargetUrl, x.CreatedAt))
            .ToListAsync();

        return Ok(notifications);
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        await db.AdminNotifications
            .Where(x => !x.IsRead && x.RecipientUserId == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.IsRead, true));

        return NoContent();
    }
}

public sealed record AdminNotificationDto(Guid Id, string Title, string Message, string TargetUrl, DateTime CreatedAt);
