namespace Handicraft.Application.Services.Dtos;

public record OrderCreateDto(
    Guid UserId,
    string Type,
    string Description,
    string? ImageUrl,
    string Status,
    decimal? EstimatedPrice
);
