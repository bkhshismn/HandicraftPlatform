namespace Handicraft.Api.Models;

public sealed record ProfileDto(
    Guid Id,
    string Name,
    string Phone,
    string Role,
    bool IsActive,
    DateTime CreatedAt);
