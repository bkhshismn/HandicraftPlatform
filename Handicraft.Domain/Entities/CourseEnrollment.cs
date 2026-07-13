using Handicraft.Domain.Entities;

public class CourseEnrollment : BaseEntity
{
    public Guid UserId { get; set; }

    public Guid CourseId { get; set; }


    public decimal PaidAmount { get; set; }


    public DateTime EnrolledAt { get; set; }


    public decimal ProgressPercent { get; set; }


    public User User { get; set; } = null!;

    public Course Course { get; set; } = null!;

    public bool IsActive { get; set; } = true;
}