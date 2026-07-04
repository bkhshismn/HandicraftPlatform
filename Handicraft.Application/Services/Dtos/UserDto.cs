namespace Handicraft.Application.Services.Dtos;

public record UserDto(
    Guid Id,
    string Phone,
    string Name,
    string Role,
    bool IsActive
);
