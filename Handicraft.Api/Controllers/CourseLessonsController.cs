using Handicraft.Application.Contracts.Services;
using Handicraft.Application.Services.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Handicraft.Api.Controllers;

[ApiController]
[Route("api/admin/sections/{sectionId:guid}/lessons")]
[Authorize(Roles = "Admin,Instructor")]
public class CourseLessonsController : ControllerBase
{
    private readonly ICourseLessonService _service;

    public CourseLessonsController(
        ICourseLessonService service)
    {
        _service = service;
    }

    // ============================================================
    // Get Lessons By Section
    // ============================================================

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CourseLessonDto>>>
        GetBySection(Guid sectionId)
    {
        var result =
            await _service.GetBySectionIdAsync(sectionId);

        return Ok(result);
    }

    // ============================================================
    // Create Lesson
    // ============================================================

    [HttpPost]
    public async Task<ActionResult<CourseLessonDto>>
        Create(
            Guid sectionId,
            [FromBody] CourseLessonCreateDto dto)
    {
        var result =
            await _service.CreateAsync(
                sectionId,
                dto);

        return Ok(result);
    }
}
