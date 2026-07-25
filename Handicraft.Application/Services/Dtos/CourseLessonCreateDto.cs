namespace Handicraft.Application.Services.Dtos;

public class CourseLessonCreateDto
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int DurationSeconds { get; set; }

    public bool IsFreePreview { get; set; }

    public int OrderIndex { get; set; }
}