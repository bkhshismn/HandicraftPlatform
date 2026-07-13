namespace Handicraft.Domain.Entities;

public class Course : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? CoverImageUrl { get; set; }

    public decimal Price { get; set; }

    public bool IsFree { get; set; }

    public bool IsPublished { get; set; }

    public string Level { get; set; } = "Beginner";
    // Beginner / Intermediate / Advanced

    public string Category { get; set; } = string.Empty;

    public int DurationMinutes { get; set; }

    public Guid? InstructorId { get; set; }

    public User? Instructor { get; set; }


    public ICollection<CourseSection> Sections { get; set; }
        = new List<CourseSection>();

    public ICollection<CourseEnrollment> Enrollments { get; set; }
        = new List<CourseEnrollment>();
    public ICollection<CourseEpisode> Episodes { get; set; }
    = new List<CourseEpisode>();
}