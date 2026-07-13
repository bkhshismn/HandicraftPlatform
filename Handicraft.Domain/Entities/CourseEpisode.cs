namespace Handicraft.Domain.Entities;

public enum VideoProcessingStatus
{
    Pending,
    Processing,
    Ready,
    Failed
}

public class CourseEpisode : BaseEntity
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty; // مسیر نهایی master.m3u8 پس از پردازش
    public int Order { get; set; }
    public int DurationSeconds { get; set; }
    public bool IsFree { get; set; } = false;

    // فیلدهای جدید برای پردازش ویدیو
    public string? RawVideoFileName { get; set; }
    public VideoProcessingStatus Status { get; set; } = VideoProcessingStatus.Pending;

    // Navigation Property
    public Course Course { get; set; } = null!;
}
