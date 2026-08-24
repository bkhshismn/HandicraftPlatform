namespace Handicraft.Application.Services.Dtos;

public record AdminUpdateOrderDto(
    decimal? EstimatedPrice,
    string Status
);