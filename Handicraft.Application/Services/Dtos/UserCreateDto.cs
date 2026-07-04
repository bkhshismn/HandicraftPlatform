namespace Handicraft.Application.Services.Dtos;

public record UserCreateDto(
    string Phone,
    string Name,
    string PasswordHash,
    string Role,
    bool IsActive
);
