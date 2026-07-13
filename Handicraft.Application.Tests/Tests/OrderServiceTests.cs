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

public class OrderServiceTests
{
    private IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<Handicraft.Application.Services.MappingProfile>());
        return config.CreateMapper();
    }

    [Fact]
    public async Task GetOrdersByUserAsync_ReturnsMapped()
    {
        var mockRepo = new Mock<IOrderRepository>();
        var orders = new List<Order>
        {
            new Order { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Type = "T", Description = "D", Status = "New", CreatedAt = DateTime.UtcNow }
        };
        mockRepo.Setup(r => r.GetOrdersByUserAsync(It.IsAny<Guid>())).ReturnsAsync(orders);

        var svc = new OrderService(mockRepo.Object, CreateMapper());
        var res = await svc.GetOrdersByUserAsync(Guid.NewGuid());
        Assert.Single(res);
    }

    [Fact]
    public async Task CreateAsync_CallsRepo()
    {
        var mockRepo = new Mock<IOrderRepository>();
        mockRepo.Setup(r => r.AddAsync(It.IsAny<Order>())).ReturnsAsync((Order o) => o);
        var svc = new OrderService(mockRepo.Object, CreateMapper());

        var create = new OrderCreateDto(Guid.NewGuid(), "T", "D", null, "New", null);
        var created = await svc.CreateAsync(create);
        Assert.Equal(create.Type, created.Type);
        mockRepo.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);
    }
}
