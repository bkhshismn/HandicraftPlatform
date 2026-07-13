using System;
using System.Threading.Tasks;
using AutoMapper;
using Handicraft.Application.Services;
using Handicraft.Application.Services.Dtos;
using Handicraft.Application.Contracts.Persistence;
using Handicraft.Domain.Entities;
using Moq;
using Xunit;

namespace Handicraft.Application.Tests.Tests;

public class UserServiceTests
{
    private IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<Handicraft.Application.Services.MappingProfile>());
        return config.CreateMapper();
    }

    [Fact]
    public async Task GetByPhoneAsync_ReturnsMappedDto()
    {
        var mockRepo = new Mock<IUserRepository>();
        var user = new User { Id = Guid.NewGuid(), Phone = "09120000000", Name = "Saman", Role = "Admin", PasswordHash = "h" };
        mockRepo.Setup(r => r.GetByPhoneAsync(user.Phone)).ReturnsAsync(user);

        var mapper = CreateMapper();
        var svc = new UserService(mockRepo.Object, mapper);

        var dto = await svc.GetByPhoneAsync(user.Phone);
        Assert.NotNull(dto);
        Assert.Equal(user.Phone, dto.Phone);
    }

    [Fact]
    public async Task CreateAsync_CreatesAndReturnsDto()
    {
        var mockRepo = new Mock<IUserRepository>();
        mockRepo.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);

        var mapper = CreateMapper();
        var svc = new UserService(mockRepo.Object, mapper);

        var create = new UserCreateDto("09121112222", "Saman", "hash", "User", true);
        var created = await svc.CreateAsync(create);

        Assert.Equal(create.Phone, created.Phone);
        mockRepo.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
    }
}
