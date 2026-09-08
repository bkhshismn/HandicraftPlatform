using Handicraft.Application.Contracts.Services;
using Handicraft.Application.Services.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Handicraft.Api.Controllers;

[ApiController]
[Route("api/admin/courses/{courseId:guid}/sections")]
[Authorize(Roles = "Admin,Instructor")]
public class CourseSectionsController : ControllerBase
{
    private readonly ICourseSectionService _service;

    public CourseSectionsController(
        ICourseSectionService service)
    {
        _service = service;
    }

    // ============================================================
    // Get Sections By Course
    // ============================================================

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CourseSectionDto>>>
        GetByCourse(Guid courseId)
    {
        var result =
            await _service.GetByCourseIdAsync(courseId);

        return Ok(result);
    }

    // ============================================================
    // Create Section
    // ============================================================

    [HttpPost]
    public async Task<ActionResult<CourseSectionDto>>
        Create(
            Guid courseId,
            [FromBody] CourseSectionCreateDto dto)
    {
        var result =
            await _service.CreateAsync(courseId, dto);

        return Ok(result);
    }
}
