namespace Handicraft.Domain.Entities;

public class User : BaseEntity
{
    public string Phone { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Customer"; // Admin, Customer
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public ICollection<CourseEnrollment> Enrollments { get; set; } = new List<CourseEnrollment>();
    public ICollection<UserDevice> Devices { get; set; } = new List<UserDevice>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
