using Handicraft.Application.Services.Dtos;

namespace Handicraft.Application.Services;

public interface IUserDeviceService
{
    Task<IEnumerable<UserDeviceDto>> GetDevicesByUserAsync(Guid userId);
    Task<UserDeviceDto?> GetByTokenAsync(string deviceToken);
    Task<int> GetActiveDeviceCountAsync(Guid userId);
    Task<UserDeviceDto> CreateAsync(UserDeviceCreateDto dto);
}
