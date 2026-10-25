namespace Handicraft.Application.Services.Dtos;

public sealed class ChatMessageDto
{
    public Guid Id { get; set; }

    public Guid ConversationId { get; set; }

    public string SenderRole { get; set; } = "User";

    public string SenderName { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}

public sealed class ChatConversationDto
{
    public Guid ConversationId { get; set; }

    public string SenderName { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public bool IsGuest { get; set; }

    public string LastMessage { get; set; } = string.Empty;

    public string LastSenderRole { get; set; } = "User";

    public DateTime LastAt { get; set; }

    public int MessageCount { get; set; }
}
