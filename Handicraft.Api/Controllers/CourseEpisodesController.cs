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
        var result =
            await _episodeService.CreateAndUploadAsync(
                lessonId,
                dto);

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
}