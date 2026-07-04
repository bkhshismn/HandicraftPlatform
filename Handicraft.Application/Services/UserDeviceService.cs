using AutoMapper;
using Handicraft.Application.Contracts.Persistence;
using Handicraft.Application.Services.Dtos;
using Handicraft.Domain.Entities;

namespace Handicraft.Application.Services;

public class UserDeviceService : IUserDeviceService
{
    private readonly IUserDeviceRepository _repo;
    private readonly IMapper _mapper;

    public UserDeviceService(IUserDeviceRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UserDeviceDto>> GetDevicesByUserAsync(Guid userId)
    {
        var devices = await _repo.GetDevicesByUserAsync(userId);
        return devices.Select(d => _mapper.Map<UserDeviceDto>(d)).ToList();
    }

    public async Task<UserDeviceDto?> GetByTokenAsync(string deviceToken)
    {
        var d = await _repo.GetDeviceByTokenAsync(deviceToken);
        return d == null ? null : _mapper.Map<UserDeviceDto>(d);
    }

    public async Task<int> GetActiveDeviceCountAsync(Guid userId)
    {
        return await _repo.GetActiveDeviceCountAsync(userId);
    }

    public async Task<UserDeviceDto> CreateAsync(UserDeviceCreateDto dto)
    {
        var d = _mapper.Map<UserDevice>(dto);
        d.CreatedAt = DateTime.UtcNow;
        var created = await _repo.AddAsync(d);
        return _mapper.Map<UserDeviceDto>(created);
    }
}
