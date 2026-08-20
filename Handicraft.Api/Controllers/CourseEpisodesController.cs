using Handicraft.Application.Contracts.Services;
using Handicraft.Application.Services.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Handicraft.Api.Controllers;

[ApiController]
[Route("api/admin/lessons/{lessonId:guid}/video")]
public class CourseEpisodesController : ControllerBase
{
    private readonly ICourseEpisodeService _episodeService;

    public CourseEpisodesController(
        ICourseEpisodeService episodeService)
    {
        _episodeService = episodeService;
    }

    // ============================================================
    // Upload Episode
    // ============================================================

    [HttpPost]
    [Authorize(Roles = "Admin,Instructor")]
    [RequestSizeLimit(2_000_000_000)]
    [RequestFormLimits(
        MultipartBodyLengthLimit = 2_000_000_000,
        ValueLengthLimit = int.MaxValue,
        MultipartHeadersLengthLimit = int.MaxValue)]
    public async Task<IActionResult> UploadEpisode(
        Guid lessonId,
        [FromForm] CourseEpisodeUploadDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (dto.VideoFile is null ||
            dto.VideoFile.Length == 0)
        {
            return BadRequest(new
            {
                message = "فایل ویدئو ارسال نشده است."
            });
        }

        var result =
            await _episodeService.CreateAndUploadAsync(
                lessonId,
                dto);

        return Ok(new
        {
            id = result.Id,
            title = result.Title,
            description = result.Description,
            videoUrl = result.VideoUrl,
            order = result.Order,
            durationSeconds = result.DurationSeconds,
            isFree = result.IsFree,
            status = result.Status.ToString()
        });
    }

    // ============================================================
    // Get Processing Status
    // ============================================================

    [HttpGet("{episodeId:guid}/status")]
    [Authorize]
    public async Task<IActionResult> GetStatus(
        Guid lessonId,
        Guid episodeId)
    {
        var status =
            await _episodeService.GetProcessingStatusAsync(
                episodeId);

        if (status is null)
            return NotFound();

        return Ok(status);
    }

    // ============================================================
    // Stream HLS
    //
    // IMPORTANT:
    // This endpoint intentionally does NOT have [Authorize].
    //
    // Access is controlled by UserHasAccessAsync:
    //
    // Anonymous + Free Episode       => allowed
    // Authenticated + Free Episode   => allowed
    // Admin                           => allowed
    // Authenticated + Enrolled        => allowed
    // Authenticated + Not Enrolled    => denied
    // Anonymous + Paid Episode        => denied
    // ============================================================

    [HttpGet("{episodeId:guid}/stream/{**path}")]
    [AllowAnonymous]
    public async Task<IActionResult> StreamHls(
     Guid lessonId,
     Guid episodeId,
     string path)
    {
        Console.WriteLine();
        Console.WriteLine("========== HLS REQUEST ==========");
        Console.WriteLine($"LessonId  : {lessonId}");
        Console.WriteLine($"EpisodeId : {episodeId}");
        Console.WriteLine($"Path      : {path}");
        Console.WriteLine($"User      : {User.Identity?.Name}");
        Console.WriteLine($"Auth      : {User.Identity?.IsAuthenticated}");
        Console.WriteLine($"Admin     : {User.IsInRole("Admin")}");
        Console.WriteLine($"Instructor: {User.IsInRole("Instructor")}");

        // --------------------------------------------------------
        // Validate path
        // --------------------------------------------------------

        if (string.IsNullOrWhiteSpace(path))
        {
            return BadRequest(new
            {
                message = "مسیر فایل ویدئو مشخص نشده است."
            });
        }

        // --------------------------------------------------------
        // Access control
        //
        // This handles BOTH authenticated and anonymous users.
        // --------------------------------------------------------

        var hasAccess =
            await _episodeService.UserHasAccessAsync(
                User,
                lessonId,
                episodeId);

        if (!hasAccess)
        {
            Console.WriteLine(
                "[HLS] ACCESS DENIED");

            if (User.Identity?.IsAuthenticated != true)
            {
                return Unauthorized();
            }

            return Forbid();
        }

        Console.WriteLine(
            "[HLS] ACCESS GRANTED");

        // --------------------------------------------------------
        // Resolve physical file
        // --------------------------------------------------------

        var fullPath =
            await _episodeService.ResolveHlsFilePathAsync(
                lessonId,
                episodeId,
                path);

        if (string.IsNullOrWhiteSpace(fullPath))
        {
            Console.WriteLine(
                "[HLS] File path could not be resolved.");

            return NotFound();
        }

        if (!System.IO.File.Exists(fullPath))
        {
            Console.WriteLine(
                $"[HLS] File does not exist: {fullPath}");

            return NotFound();
        }

        Console.WriteLine(
            $"[HLS] Serving: {fullPath}");

        // --------------------------------------------------------
        // Content type
        // --------------------------------------------------------

        var contentType =
            GetHlsContentType(path);

        // --------------------------------------------------------
        // PhysicalFile
        // --------------------------------------------------------

        return PhysicalFile(
            fullPath,
            contentType,
            enableRangeProcessing: true);
    }

    // ============================================================
    // Get Episodes By Lesson
    // ============================================================

    [HttpGet("~/api/lessons/{lessonId:guid}/episodes")]
    [Authorize]
    public async Task<IActionResult> GetEpisodes(
        Guid lessonId)
    {
        var episodes =
            await _episodeService
                .GetEpisodesByLessonIdAsync(lessonId);

        return Ok(episodes);
    }

    // ============================================================
    // HLS Content Types
    // ============================================================

    private static string GetHlsContentType(
        string path)
    {
        if (path.EndsWith(
                ".m3u8",
                StringComparison.OrdinalIgnoreCase))
        {
            return "application/vnd.apple.mpegurl";
        }

        if (path.EndsWith(
                ".ts",
                StringComparison.OrdinalIgnoreCase))
        {
            return "video/mp2t";
        }

        if (path.EndsWith(
                ".m4s",
                StringComparison.OrdinalIgnoreCase))
        {
            return "video/iso.segment";
        }

        if (path.EndsWith(
                ".mp4",
                StringComparison.OrdinalIgnoreCase))
        {
            return "video/mp4";
        }

        if (path.EndsWith(
                ".aac",
                StringComparison.OrdinalIgnoreCase))
        {
            return "audio/aac";
        }

        if (path.EndsWith(
                ".vtt",
                StringComparison.OrdinalIgnoreCase))
        {
            return "text/vtt";
        }

        return "application/octet-stream";
    }
}