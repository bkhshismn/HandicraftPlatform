namespace Handicraft.Domain.Entities;

public sealed class UserCourseCategory
{
    public Guid UserId { get; set; }
    public string Category { get; set; } = string.Empty;
    public User User { get; set; } = null!;
}
