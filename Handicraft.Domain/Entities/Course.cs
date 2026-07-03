namespace Handicraft.Domain.Entities;

public class Course : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CoverImageUrl { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsPublished { get; set; } = false;
    public int DurationMinutes { get; set; } // مجموع زمان اپیزودها

    // Navigation Properties
    public ICollection<CourseEpisode> Episodes { get; set; } = new List<CourseEpisode>();
    public ICollection<CourseEnrollment> Enrollments { get; set; } = new List<CourseEnrollment>();
}
