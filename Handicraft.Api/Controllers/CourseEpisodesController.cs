using Handicraft.Application.Contracts.Services;
using Handicraft.Application.Services.Dtos;
using Handicraft.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Handicraft.Api.Controllers
{
    [ApiController]
    [Route("api/courses/{courseId}/episodes")]
    public class CourseEpisodesController : ControllerBase
    {
        private readonly ICourseEpisodeService _episodeService;
        public CourseEpisodesController(ICourseEpisodeService episodeService)
        {
            _episodeService = episodeService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Instructor")]
        [RequestSizeLimit(2_000_000_000)] // 2GB مثلا
        public async Task<IActionResult> UploadEpisode(Guid courseId, [FromForm] CourseEpisodeUploadDto dto)
        {
            var result = await _episodeService.CreateAndUploadAsync(courseId, dto);
            return Ok(result);
        }

        [HttpGet("{episodeId}/status")]
        public async Task<IActionResult> GetStatus(Guid episodeId)
        {
            var status = await _episodeService.GetProcessingStatusAsync(episodeId);
            return Ok(status);
        }

        [HttpGet("{episodeId}/stream/{**path}")]
        [Authorize] // چک دسترسی کاربر به دوره
        public async Task<IActionResult> StreamHls(Guid courseId, Guid episodeId, string path)
        {
            // چک: آیا کاربر این دوره را خریده؟ یا IsFree است؟
            var hasAccess = await _episodeService.UserHasAccessAsync(User, courseId, episodeId);
            if (!hasAccess) return Forbid();

            var fullPath = await _episodeService.ResolveHlsFilePathAsync(courseId, episodeId, path);
            if (!System.IO.File.Exists(fullPath)) return NotFound();

            var contentType = path.EndsWith(".m3u8") ? "application/vnd.apple.mpegurl" : "video/MP2T";
            return PhysicalFile(fullPath, contentType);
        }
    }

}
