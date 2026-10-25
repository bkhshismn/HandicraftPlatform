namespace Handicraft.Application.Services.Dtos;

public record AdminUpdateOrderDto(
    decimal? EstimatedPrice,
    string Status,
    string? Type = null,
    string? Description = null
);
