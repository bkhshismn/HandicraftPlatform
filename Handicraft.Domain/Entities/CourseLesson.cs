namespace Handicraft.Domain.Entities;

public class CourseLesson : BaseEntity
{
    public Guid SectionId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int DurationSeconds { get; set; }

    public bool IsFreePreview { get; set; }

    public int OrderIndex { get; set; }


    // Video link

    public CourseEpisode? Episode { get; set; }


    public CourseSection Section { get; set; } = null!;
}