using Handicraft.Application.Contracts.Persistence;
using Handicraft.Application.Contracts.Services;
using Handicraft.Application.Services.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Handicraft.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PortfolioController : ControllerBase
{
    private readonly IPortfolioService _service;

    public PortfolioController(IPortfolioService service)
    {
        _service = service;
    }

    [HttpGet("published")]
    public async Task<ActionResult<IEnumerable<PortfolioItemDto>>> GetPublished()
    {
        var items = await _service.GetPublishedAsync();
        return Ok(items);
    }

    [HttpGet("category/{category}")]
    public async Task<ActionResult<IEnumerable<PortfolioItemDto>>> GetByCategory(string category)
    {
        var items = await _service.GetByCategoryAsync(category);
        return Ok(items);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<ActionResult<PortfolioItemDto>> Create([FromBody] PortfolioCreateDto dto)
    {
        if (dto == null) return BadRequest();
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(null, created);
    }
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PortfolioItemDto>> GetById(Guid id)
    {
        var item = await _service.GetByIdAsync(id);

        if (item is null)
            return NotFound();

        if (!item.IsVisible && !User.IsInRole("Admin") && !User.IsInRole("Instructor"))
            return NotFound();

        return Ok(item);
    }
}
