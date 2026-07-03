namespace Handicraft.Domain.Entities;

public class CourseEpisode : BaseEntity
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty; // HLS URL (m3u8)
    public int Order { get; set; }
    public int DurationSeconds { get; set; }
    public bool IsFree { get; set; } = false; // اپیزود رایگان برای پیش‌نمایش

    // Navigation Property
    public Course Course { get; set; } = null!;
}
