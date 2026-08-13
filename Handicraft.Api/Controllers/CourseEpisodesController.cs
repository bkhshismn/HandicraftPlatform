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


    [HttpPost]
    [Authorize(Roles = "Admin,Instructor")]
    [RequestSizeLimit(2_000_000_000)]
    public async Task<IActionResult> UploadEpisode(
    Guid lessonId,
    [FromForm] CourseEpisodeUploadDto dto)
    {
        Console.WriteLine("========== UPLOAD DEBUG ==========");
        Console.WriteLine($"LessonId: {lessonId}");
        Console.WriteLine($"Title: {dto.Title}");
        Console.WriteLine($"Description: {dto.Description}");
        Console.WriteLine($"Order: {dto.Order}");
        Console.WriteLine($"IsFree: {dto.IsFree}");
        Console.WriteLine($"VideoFile null: {dto.VideoFile == null}");
        Console.WriteLine($"VideoFile name: {dto.VideoFile?.FileName}");
        Console.WriteLine($"VideoFile length: {dto.VideoFile?.Length}");
        Console.WriteLine($"VideoFile content type: {dto.VideoFile?.ContentType}");

        if (!ModelState.IsValid)
        {
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

        Console.WriteLine("========== CALLING SERVICE ==========");

        var result =
            await _episodeService.CreateAndUploadAsync(
                lessonId,
                dto);

        Console.WriteLine("========== UPLOAD SERVICE SUCCESS ==========");

        return Ok(result);
    }


    [HttpGet("{episodeId:guid}/status")]
    public async Task<IActionResult> GetStatus(
        Guid episodeId)
    {
        var status =
            await _episodeService.GetProcessingStatusAsync(
                episodeId);

        if (status == null)
            return NotFound();

        return Ok(status);
    }


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
            return Forbid();


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
            path.EndsWith(".m3u8")
                ? "application/vnd.apple.mpegurl"
                : "video/mp2t";


        return PhysicalFile(
            fullPath,
            contentType);
    }

    [HttpGet("~/api/lessons/{lessonId:guid}/episodes")]
    [Authorize]
    public async Task<IActionResult> GetEpisodes(Guid lessonId)
    {
        var episodes =
            await _episodeService.GetEpisodesByLessonIdAsync(lessonId);

        return Ok(episodes);
    }

}