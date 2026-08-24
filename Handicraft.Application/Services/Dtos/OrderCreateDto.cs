namespace Handicraft.Application.Services.Dtos;

public record OrderCreateDto(
    string Type,
    string Description,
    string? ImageUrl
);