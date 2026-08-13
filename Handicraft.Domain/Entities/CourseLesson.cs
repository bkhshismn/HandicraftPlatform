namespace Handicraft.Domain.Entities;

public class CourseLesson : BaseEntity
{
    public Guid SectionId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int DurationSeconds { get; set; }

    public bool IsFreePreview { get; set; }

    public int OrderIndex { get; set; }

    // A lesson can have multiple episodes/videos.
    public ICollection<CourseEpisode> Episodes { get; set; }
        = new List<CourseEpisode>();

    public CourseSection Section { get; set; } = null!;
}