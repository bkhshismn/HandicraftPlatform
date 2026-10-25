namespace Handicraft.Domain.Entities;

public sealed class AdminNotification : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string TargetUrl { get; set; } = "/admin";
    public bool IsRead { get; set; }
    public Guid? RecipientUserId { get; set; }
}
