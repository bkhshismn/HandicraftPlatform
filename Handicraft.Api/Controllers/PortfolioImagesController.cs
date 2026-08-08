using Handicraft.Application.Contracts.Services;
using Handicraft.Application.Services.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Handicraft.Api.Controllers;

[ApiController]
[Route("api/portfolio")]
public class PortfolioImagesController : ControllerBase
{
    private readonly IPortfolioImageService _service;

    public PortfolioImagesController(
        IPortfolioImageService service)
    {
        _service = service;
    }

    [HttpGet("{portfolioId:guid}/images")]
    public async Task<ActionResult<IReadOnlyList<PortfolioImageDto>>>
        Get(Guid portfolioId)
    {
        var images =
            await _service.GetByPortfolioIdAsync(
                portfolioId);

        return Ok(images);
    }

    [HttpPost("{portfolioId:guid}/images")]
    public async Task<ActionResult<PortfolioImageDto>>
        Add(
            Guid portfolioId,
            [FromBody] PortfolioImageCreateDto dto)
    {
        var image =
            await _service.AddAsync(
                portfolioId,
                dto);

        return Ok(image);
    }

    [HttpDelete("images/{imageId:guid}")]
    public async Task<IActionResult>
        Delete(Guid imageId)
    {
        await _service.DeleteAsync(imageId);

        return NoContent();
    }

    [HttpPut("images/reorder")]
    public async Task<IActionResult>
        Reorder(
            [FromBody] PortfolioImageReorderDto dto)
    {
        await _service.ReorderAsync(dto);

        return NoContent();
    }
}