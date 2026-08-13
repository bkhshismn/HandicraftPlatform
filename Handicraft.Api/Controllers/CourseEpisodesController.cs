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
        Console.WriteLine();
        Console.WriteLine("========================================");
        Console.WriteLine("========== UPLOAD EPISODE =============");
        Console.WriteLine("========================================");

        Console.WriteLine($"LessonId: {lessonId}");
        Console.WriteLine($"Content-Type: {Request.ContentType}");
        Console.WriteLine($"HasFormContentType: {Request.HasFormContentType}");

        // --------------------------------------------------------
        // Form Debug
        // --------------------------------------------------------

        try
        {
            Console.WriteLine(
                $"Form keys: {string.Join(", ", Request.Form.Keys)}");

            Console.WriteLine(
                $"Files count: {Request.Form.Files.Count}");

            foreach (var file in Request.Form.Files)
            {
                Console.WriteLine(
                    $"FILE: " +
                    $"name={file.Name}, " +
                    $"fileName={file.FileName}, " +
                    $"length={file.Length}, " +
                    $"contentType={file.ContentType}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"[FORM READ ERROR] {ex.Message}");
        }

        // --------------------------------------------------------
        // DTO Debug
        // --------------------------------------------------------

        Console.WriteLine();
        Console.WriteLine("========== DTO DEBUG ==========");

        Console.WriteLine(
            $"Title: {dto?.Title}");

        Console.WriteLine(
            $"Description: {dto?.Description}");

        Console.WriteLine(
            $"Order: {dto?.Order}");

        Console.WriteLine(
            $"IsFree: {dto?.IsFree}");

        Console.WriteLine(
            $"VideoFile null: {dto?.VideoFile == null}");

        Console.WriteLine(
            $"VideoFile name: {dto?.VideoFile?.FileName}");

        Console.WriteLine(
            $"VideoFile length: {dto?.VideoFile?.Length}");

        Console.WriteLine(
            $"VideoFile content type: {dto?.VideoFile?.ContentType}");

        // --------------------------------------------------------
        // Authentication Debug
        // --------------------------------------------------------

        Console.WriteLine();
        Console.WriteLine("========== AUTH DEBUG ==========");

        Console.WriteLine(
            $"IsAuthenticated: {User.Identity?.IsAuthenticated}");

        Console.WriteLine(
            $"Role Admin: {User.IsInRole("Admin")}");

        Console.WriteLine(
            $"Role Instructor: {User.IsInRole("Instructor")}");

        Console.WriteLine(
            $"Claims: {string.Join(
                " | ",
                User.Claims.Select(
                    c => $"{c.Type}={c.Value}"))}");

        // --------------------------------------------------------
        // Model Validation
        // --------------------------------------------------------

        if (!ModelState.IsValid)
        {
            Console.WriteLine();
            Console.WriteLine("========== MODEL STATE ERRORS ==========");

            foreach (var item in ModelState)
            {
                foreach (var error in item.Value.Errors)
                {
                    Console.WriteLine(
                        $"[{item.Key}] {error.ErrorMessage}");

                    if (error.Exception != null)
                    {
                        Console.WriteLine(
                            $"Exception: {error.Exception.Message}");
                    }
                }
            }

            return ValidationProblem(ModelState);
        }

        // --------------------------------------------------------
        // File Validation
        // --------------------------------------------------------

        if (dto?.VideoFile == null ||
            dto.VideoFile.Length == 0)
        {
            Console.WriteLine(
                "[ERROR] VideoFile is null or empty.");

            return BadRequest(new
            {
                message = "فایل ویدئو ارسال نشده است."
            });
        }

        // --------------------------------------------------------
        // Create Episode + Save Raw Video
        // --------------------------------------------------------

        Console.WriteLine();
        Console.WriteLine(
            "========== CALLING EPISODE SERVICE ==========");

        var result =
            await _episodeService.CreateAndUploadAsync(
                lessonId,
                dto);

        // --------------------------------------------------------
        // Result Debug
        // --------------------------------------------------------

        Console.WriteLine();
        Console.WriteLine(
            "========== EPISODE CREATED ==========");

        Console.WriteLine(
            $"Result null: {result is null}");

        if (result is null)
        {
            Console.WriteLine(
                "[ERROR] Episode service returned null.");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message = "اطلاعات ویدئو ایجاد نشد."
                });
        }

        Console.WriteLine(
            $"Id: {result.Id}");

        Console.WriteLine(
            $"Title: {result.Title}");

        Console.WriteLine(
            $"Description: {result.Description}");

        Console.WriteLine(
            $"VideoUrl: {result.VideoUrl}");

        Console.WriteLine(
            $"Order: {result.Order}");

        Console.WriteLine(
            $"DurationSeconds: {result.DurationSeconds}");

        Console.WriteLine(
            $"IsFree: {result.IsFree}");

        Console.WriteLine(
            $"Status: {result.Status}");

        // --------------------------------------------------------
        // Explicit JSON Response
        // --------------------------------------------------------

        var response = new
        {
            id = result.Id,
            title = result.Title,
            description = result.Description,
            videoUrl = result.VideoUrl,
            order = result.Order,
            durationSeconds = result.DurationSeconds,
            isFree = result.IsFree,
            status = result.Status.ToString()
        };

        Console.WriteLine();
        Console.WriteLine(
            "========== RESPONSE ==========");

        Console.WriteLine(
            $"Response Id: {response.id}");

        Console.WriteLine(
            $"Response Status: {response.status}");

        Console.WriteLine(
            "Returning HTTP 200 with JSON body.");

        Console.WriteLine(
            "========================================");

        return Ok(response);
    }

    // ============================================================
    // Get Processing Status
    // ============================================================

    [HttpGet("{episodeId:guid}/status")]
    public async Task<IActionResult> GetStatus(
        Guid episodeId)
    {
        var status =
            await _episodeService.GetProcessingStatusAsync(
                episodeId);

        if (status == null)
        {
            return NotFound();
        }

        return Ok(status);
    }

    // ============================================================
    // Stream HLS
    // ============================================================

    [HttpGet("{episodeId:guid}/stream/{**path}")]
    [Authorize]
    public async Task<IActionResult> StreamHls(
        Guid lessonId,
        Guid episodeId,
        string path)
    {
        var hasAccess =
            await _episodeService.UserHasAccessAsync(
                User,
                lessonId,
                episodeId);

        if (!hasAccess)
        {
            return Forbid();
        }

        var fullPath =
            await _episodeService.ResolveHlsFilePathAsync(
                lessonId,
                episodeId,
                path);

        if (string.IsNullOrEmpty(fullPath) ||
            !System.IO.File.Exists(fullPath))
        {
            return NotFound();
        }

        var contentType =
            path.EndsWith(
                ".m3u8",
                StringComparison.OrdinalIgnoreCase)
                ? "application/vnd.apple.mpegurl"
                : "video/mp2t";

        return PhysicalFile(
            fullPath,
            contentType);
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
}