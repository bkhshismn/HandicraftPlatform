namespace Handicraft.Domain.Entities;

public sealed class CourseCategory
{
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
