using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Handicraft.Application.Services;
using Handicraft.Application.Services.Dtos;
using Handicraft.Application.Contracts.Persistence;
using Handicraft.Domain.Entities;
using Moq;
using Xunit;

namespace Handicraft.Application.Tests.Tests;

public class UserDeviceServiceTests
{
    private IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<Handicraft.Application.Services.MappingProfile>());
        return config.CreateMapper();
    }

    [Fact]
    public async Task GetDevicesByUserAsync_ReturnsMapped()
    {
        var mockRepo = new Mock<IUserDeviceRepository>();
        var devices = new List<UserDevice>
        {
            new UserDevice { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), DeviceId = "dev1", DeviceName = "d", IpAddress = "127.0.0.1", LastSeenAt = DateTime.UtcNow, IsActive = true, CreatedAt = DateTime.UtcNow }
        };
        mockRepo.Setup(r => r.GetDevicesByUserAsync(It.IsAny<Guid>())).ReturnsAsync(devices);

        var svc = new UserDeviceService(mockRepo.Object, CreateMapper());
        var res = await svc.GetDevicesByUserAsync(Guid.NewGuid());
        Assert.Single(res);
    }
}
