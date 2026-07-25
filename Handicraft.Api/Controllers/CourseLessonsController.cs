using Handicraft.Application.Contracts.Services;
using Handicraft.Application.Services.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Handicraft.Api.Controllers;

[ApiController]
[Route("api/admin/sections/{sectionId:guid}/lessons")]
public class CourseLessonsController : ControllerBase
{
    private readonly ICourseLessonService _service;


    public CourseLessonsController(
        ICourseLessonService service)
    {
        _service = service;
    }


    [HttpPost]
    public async Task<IActionResult> Create(
        Guid sectionId,
        [FromBody] CourseLessonCreateDto dto)
    {
        var result =
            await _service.CreateAsync(sectionId, dto);

        return Ok(result);
    }
}