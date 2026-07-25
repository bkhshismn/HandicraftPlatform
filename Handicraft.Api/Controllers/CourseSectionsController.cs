using Handicraft.Application.Contracts.Services;
using Handicraft.Application.Services.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Handicraft.Api.Controllers;

[ApiController]
[Route("api/admin/courses/{courseId:guid}/sections")]
public class CourseSectionsController : ControllerBase
{
    private readonly ICourseSectionService _service;


    public CourseSectionsController(
        ICourseSectionService service)
    {
        _service = service;
    }


    [HttpPost]
    public async Task<IActionResult> Create(
        Guid courseId,
        [FromBody] CourseSectionCreateDto dto)
    {
        var result =
            await _service.CreateAsync(courseId, dto);

        return Ok(result);
    }
}