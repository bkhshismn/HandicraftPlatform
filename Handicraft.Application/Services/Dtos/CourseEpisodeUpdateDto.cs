namespace Handicraft.Application.Services.Dtos;

public sealed class CourseEpisodeUpdateDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; }
    public bool IsFree { get; set; }
}
