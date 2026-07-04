using AutoMapper;
using AutoMapper;
using Handicraft.Application.Contracts.Persistence;
using Handicraft.Application.Services.Dtos;
using Handicraft.Domain.Entities;
using Handicraft.Application.Contracts.Services;

namespace Handicraft.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user == null ? null : _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto?> GetByPhoneAsync(string phone)
    {
        var user = await _userRepository.GetByPhoneAsync(phone);
        return user == null ? null : _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> CreateAsync(UserCreateDto dto)
    {
        var user = _mapper.Map<User>(dto);
        var created = await _userRepository.AddAsync(user);
        return _mapper.Map<UserDto>(created);
    }
}
