using Handicraft.Application.Services.Dtos;

namespace Handicraft.Application.Contracts.Services;

public interface IUserService
{
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<UserDto?> GetByPhoneAsync(string phone);
    Task<UserDto> CreateAsync(UserCreateDto dto);
}
