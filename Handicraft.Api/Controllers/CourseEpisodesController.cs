using Handicraft.Application.Contracts.Services;
using Handicraft.Application.Services.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

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
    [RequestSizeLimit(3_221_225_472)]
    [RequestFormLimits(
        MultipartBodyLengthLimit = 3_221_225_472,
        ValueLengthLimit = 16_384,
        MultipartHeadersLengthLimit = 32_768)]
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
    [Authorize(Roles = "Admin,Instructor")]
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
    
    [HttpDelete("{episodeId:guid}")]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> DeleteEpisode(
        Guid lessonId,
        Guid episodeId)
    {
        var deleted = await _episodeService.DeleteAsync(
            lessonId,
            episodeId);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpPut("{episodeId:guid}")]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<ActionResult<CourseEpisodeDto>> UpdateEpisode(
        Guid lessonId,
        Guid episodeId,
        [FromBody] CourseEpisodeUpdateDto dto)
    {
        var updated = await _episodeService.UpdateAsync(lessonId, episodeId, dto);
        if (updated is null)
            return NotFound();
        return Ok(updated);
    }

    // ============================================================
    // Stream HLS
    //
    // Free episode:
    //      Anonymous       => allowed
    //      Authenticated   => allowed
    //
    // Paid episode:
    //      Anonymous       => denied
    //      Authenticated + access => allowed
    //
    // IMPORTANT:
    // این endpoint AllowAnonymous است تا Free Preview بتواند
    // بدون لاگین هم پخش شود.
    //
    // برای درخواست‌های HLS با Bearer Token، ابتدا Header را
    // لاگ می‌کنیم تا مطمئن شویم HLS.js واقعاً Token را ارسال می‌کند.
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

        Console.WriteLine(
            $"LessonId  : {lessonId}");

        Console.WriteLine(
            $"EpisodeId : {episodeId}");

        Console.WriteLine(
            $"Path      : {path}");

        // --------------------------------------------------------
        // Authorization Header
        // --------------------------------------------------------

        var authHeader =
            Request.Headers.Authorization.ToString();

        Console.WriteLine(
            $"[HLS AUTH HEADER] " +
            $"{(string.IsNullOrWhiteSpace(authHeader)
                ? "EMPTY"
                : "PRESENT")}");

        if (!string.IsNullOrWhiteSpace(authHeader))
        {
            Console.WriteLine(
                $"[HLS AUTH HEADER FORMAT] " +
                $"{(authHeader.StartsWith(
                    "Bearer ",
                    StringComparison.OrdinalIgnoreCase)
                    ? "Bearer"
                    : "INVALID")}");
        }

        // --------------------------------------------------------
        // Current HttpContext User
        // --------------------------------------------------------

        Console.WriteLine(
            $"User      : {User.Identity?.Name}");

        Console.WriteLine(
            $"Auth      : {User.Identity?.IsAuthenticated}");

        Console.WriteLine(
            $"Admin     : {User.IsInRole("Admin")}");

        Console.WriteLine(
            $"Instructor: {User.IsInRole("Instructor")}");

        // --------------------------------------------------------
        // Validate path
        // --------------------------------------------------------

        if (string.IsNullOrWhiteSpace(path))
        {
            Console.WriteLine(
                "[HLS] Empty path.");

            return BadRequest(new
            {
                message = "مسیر فایل ویدئو مشخص نشده است."
            });
        }

        // --------------------------------------------------------
        // Access control
        // --------------------------------------------------------

        var hasAccess =
            await _episodeService.UserHasAccessAsync(
                User,
                lessonId,
                episodeId);

        Console.WriteLine(
            $"[HLS ACCESS RESULT] {hasAccess}");

        if (!hasAccess)
        {
            Console.WriteLine(
                "[HLS] ACCESS DENIED");

            if (User.Identity?.IsAuthenticated != true)
            {
                Console.WriteLine(
                    "[HLS] Anonymous user.");

                return Unauthorized();
            }

            Console.WriteLine(
                "[HLS] Authenticated user without access.");

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

        Console.WriteLine(
            $"[HLS] Resolved path: {fullPath}");

        // --------------------------------------------------------
        // Check file exists
        // --------------------------------------------------------

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

        Console.WriteLine(
            $"[HLS] Content-Type: {contentType}");

        // --------------------------------------------------------
        // Physical file
        // --------------------------------------------------------

        Response.Headers["Cache-Control"] =
            "no-store, no-cache, must-revalidate";

        Response.Headers["Pragma"] =
            "no-cache";

        Response.Headers["Expires"] =
            "0";

        // --------------------------------------------------------
        // iOS / Safari: HLS بومی هدر Authorization را همراه
        // سگمنت‌ها نمی‌فرستد؛ پس مسیرهای داخل پلی‌لیست را با
        // access_token بازنویسی می‌کنیم.
        // --------------------------------------------------------

        if (contentType == "application/vnd.apple.mpegurl")
        {
            var playlistToken = GetAccessToken();

            if (!string.IsNullOrWhiteSpace(playlistToken))
            {
                var playlist =
                    await System.IO.File.ReadAllTextAsync(fullPath);

                return Content(
                    RewritePlaylist(playlist, playlistToken),
                    contentType);
            }
        }

        return PhysicalFile(
            fullPath,
            contentType,
            enableRangeProcessing: true);
    }

    // ============================================================
    // Token helpers
    // ============================================================

    private string? GetAccessToken()
    {
        var header = Request.Headers.Authorization.ToString();

        if (header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return header["Bearer ".Length..].Trim();

        var query = Request.Query["access_token"].ToString();

        return string.IsNullOrWhiteSpace(query) ? null : query;
    }

    private static string RewritePlaylist(
        string playlist,
        string token)
    {
        var encoded = Uri.EscapeDataString(token);

        var builder = new StringBuilder();

        foreach (var rawLine in playlist.Split('\n'))
        {
            var line = rawLine.TrimEnd('\r');

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            {
                builder.Append(line).Append('\n');

                continue;
            }

            var separator = line.Contains('?') ? '&' : '?';

            builder
                .Append(line)
                .Append(separator)
                .Append("access_token=")
                .Append(encoded)
                .Append('\n');
        }

        return builder.ToString();
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
