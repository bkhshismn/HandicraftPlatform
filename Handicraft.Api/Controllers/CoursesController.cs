using Handicraft.Application.Contracts.Services;
using Handicraft.Application.Services.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Handicraft.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CourseDto>>> GetAll()
    {
        var courses = await _courseService.GetAllAsync();
        return Ok(courses);
    }

    [HttpGet("published")]
    public async Task<ActionResult<IEnumerable<CourseDto>>> GetPublished()
    {
        var courses = await _courseService.GetPublishedAsync();
        return Ok(courses);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CourseDto>> GetById(Guid id)
    {
        var course = await _courseService.GetByIdAsync(id);

        if (course == null)
            return NotFound();

        return Ok(course);
    }

    [HttpPost]
    public async Task<ActionResult<CourseDto>> Create(
        [FromBody] CourseCreateDto dto)
    {
        if (dto == null)
            return BadRequest();

        var created =
            await _courseService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            created);
    }

    [HttpPut("{id:guid}/publish")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CourseDto>> SetPublished(
        Guid id,
        [FromBody] SetCoursePublishedRequest request)
    {
        var course =
            await _courseService.SetPublishedAsync(
                id,
                request.IsPublished);

        if (course is null)
            return NotFound();

        return Ok(course);
    }
}

public sealed class SetCoursePublishedRequest
{
    public bool IsPublished { get; set; }
}