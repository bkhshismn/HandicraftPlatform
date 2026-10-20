using Microsoft.AspNetCore.Http;

namespace Handicraft.Application.Services.Dtos;

public class CourseEpisodeUploadDto
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int Order { get; set; }

    public bool IsFree { get; set; }

    public IFormFile VideoFile { get; set; } = null!;
}
