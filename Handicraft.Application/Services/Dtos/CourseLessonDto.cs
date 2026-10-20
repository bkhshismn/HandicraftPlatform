using System;

namespace Handicraft.Application.Services.Dtos;

public class CourseLessonDto
{
    public Guid Id { get; set; }


    public string Title { get; set; } = string.Empty;


    public string? Description { get; set; }


    public int DurationSeconds { get; set; }


    public bool IsFreePreview { get; set; }


    public int OrderIndex { get; set; }


    // ارتباط با ویدئو
    public Guid? EpisodeId { get; set; }


    // اطلاعات لازم برای Player
    public string? VideoStatus { get; set; }

    public string? ThumbnailUrl { get; set; }
    public bool HasVideo { get; set; }
    public List<CourseEpisodeDto> Episodes { get; set; } = [];
}
