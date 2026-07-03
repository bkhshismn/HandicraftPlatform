namespace Handicraft.Domain.Entities;

public class UserDevice : BaseEntity
{
    public Guid UserId { get; set; }
    public string DeviceId { get; set; } = string.Empty; // UUID یا fingerprint
    public string DeviceName { get; set; } = string.Empty; // "iPhone 13", "Chrome on Windows"
    public string IpAddress { get; set; } = string.Empty;
    public DateTime LastSeenAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation Property
    public User User { get; set; } = null!;
}
