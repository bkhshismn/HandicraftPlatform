using Handicraft.Application.Contracts.Services;
using Handicraft.Application.Services.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Handicraft.Api.Controllers;

[ApiController]
[Route("api/lessons/{lessonId:guid}/progress")]
[Authorize]
public class CourseLessonProgressController : ControllerBase
{
    private readonly ICourseLessonProgressService _progressService;

    public CourseLessonProgressController(
        ICourseLessonProgressService progressService)
    {
        _progressService = progressService;
    }

    // ============================================================
    // Get Progress
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> GetProgress(
        Guid lessonId)
    {
        try
        {
            var progress =
                await _progressService.GetAsync(
                    User,
                    lessonId);

            if (progress is null)
            {
                return NotFound(new
                {
                    message = "درس پیدا نشد."
                });
            }

            return Ok(progress);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    // ============================================================
    // Update Progress
    // ============================================================

    [HttpPut]
    public async Task<IActionResult> UpdateProgress(
        Guid lessonId,
        [FromBody] UpdateCourseLessonProgressDto dto)
    {
        try
        {
            var progress =
                await _progressService.UpdateAsync(
                    User,
                    lessonId,
                    dto);

            return Ok(progress);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpGet("~/api/lessons/{lessonId:guid}/episodes/{episodeId:guid}/progress")]
    public async Task<IActionResult> GetEpisodeProgress(Guid lessonId, Guid episodeId)
    {
        try
        {
            var progress = await _progressService.GetEpisodeAsync(User, lessonId, episodeId);
            return progress is null ? NotFound(new { message = "ویدیو پیدا نشد." }) : Ok(progress);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPut("~/api/lessons/{lessonId:guid}/episodes/{episodeId:guid}/progress")]
    public async Task<IActionResult> UpdateEpisodeProgress(
        Guid lessonId,
        Guid episodeId,
        [FromBody] UpdateCourseLessonProgressDto dto)
    {
        try
        {
            return Ok(await _progressService.UpdateEpisodeAsync(User, lessonId, episodeId, dto));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
