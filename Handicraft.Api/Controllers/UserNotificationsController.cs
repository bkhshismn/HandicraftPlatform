using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Handicraft.Infrastructure.Persistence.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Handicraft.Api.Controllers;

[ApiController]
[Route("api/user-notifications")]
[Authorize]
public sealed class UserNotificationsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUnread()
    {
        var userId = GetUserId();
        var items = await db.AdminNotifications.AsNoTracking()
            .Where(x => x.RecipientUserId == userId && !x.IsRead)
            .OrderByDescending(x => x.CreatedAt).Take(50)
            .Select(x => new UserNotificationDto(x.Id, x.Title, x.Message, x.TargetUrl, x.CreatedAt))
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = GetUserId();
        await db.AdminNotifications.Where(x => x.RecipientUserId == userId && !x.IsRead)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.IsRead, true));
        return NoContent();
    }

    private Guid GetUserId()
    {
        var value = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(value, out var id) ? id : throw new UnauthorizedAccessException();
    }
}

public sealed record UserNotificationDto(Guid Id, string Title, string Message, string TargetUrl, DateTime CreatedAt);
