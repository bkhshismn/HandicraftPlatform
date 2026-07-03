namespace Handicraft.Domain.Entities;

public class CourseEnrollment : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTime EnrolledAt { get; set; }
    public DateTime? ExpiresAt { get; set; } // اگر دسترسی محدود زمانی داره
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public User User { get; set; } = null!;
    public Course Course { get; set; } = null!;
}
