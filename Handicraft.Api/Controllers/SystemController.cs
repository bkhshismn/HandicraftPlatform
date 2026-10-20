using Handicraft.Application.Contracts.Services;
using Handicraft.Application.Services.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Handicraft.Api.Controllers;

[ApiController]
[Route("api/admin/system")]
[Authorize(Roles = "Admin")]
public class SystemController : ControllerBase
{
    private readonly ISystemStatusService _systemStatus;

    public SystemController(ISystemStatusService systemStatus)
    {
        _systemStatus = systemStatus;
    }

    // ============================================================
    // وضعیت منابع سرور (CPU / RAM / دیسک)
    // ============================================================

    [HttpGet("status")]
    public async Task<ActionResult<SystemStatusDto>> GetStatus(
        CancellationToken cancellationToken)
    {
        var status =
            await _systemStatus.GetStatusAsync(cancellationToken);

        return Ok(status);
    }
}
