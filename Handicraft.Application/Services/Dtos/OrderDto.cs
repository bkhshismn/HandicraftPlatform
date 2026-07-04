namespace Handicraft.Application.Services.Dtos;

public record OrderDto(
    Guid Id,
    Guid UserId,
    string Type,
    string Description,
    string? ImageUrl,
    string Status,
    decimal? EstimatedPrice,
    DateTime CreatedAt
);
