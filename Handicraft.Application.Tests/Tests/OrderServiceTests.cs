using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Handicraft.Application.Contracts.Persistence;
using Handicraft.Application.Services;
using Handicraft.Application.Services.Dtos;
using Handicraft.Domain.Entities;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Handicraft.Application.Tests.Tests;

public class OrderServiceTests
{
    private IMapper CreateMapper()
    {
        var config =
            new MapperConfiguration(
                cfg =>
                    cfg.AddProfile<
                        Handicraft.Application.Services.MappingProfile>(),
                NullLoggerFactory.Instance);

        return config.CreateMapper();
    }

    // ============================================================
    // Get Orders By User
    // ============================================================

    [Fact]
    public async Task GetOrdersByUserAsync_ReturnsMapped()
    {
        var mockRepo =
            new Mock<IOrderRepository>();

        var orders =
            new List<Order>
            {
                new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    Type = "T",
                    Description = "D",
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                }
            };

        mockRepo
            .Setup(r =>
                r.GetOrdersByUserAsync(
                    It.IsAny<Guid>()))
            .ReturnsAsync(orders);

        var svc =
            new OrderService(
                mockRepo.Object,
                CreateMapper());

        var result =
            await svc.GetOrdersByUserAsync(
                Guid.NewGuid());

        Assert.Single(result);
    }

    // ============================================================
    // Create Order
    // ============================================================

    [Fact]
    public async Task CreateAsync_CreatesPendingOrderForUser()
    {
        var mockRepo =
            new Mock<IOrderRepository>();

        mockRepo
            .Setup(r =>
                r.AddAsync(
                    It.IsAny<Order>()))
            .ReturnsAsync(
                (Order order) => order);

        var svc =
            new OrderService(
                mockRepo.Object,
                CreateMapper());

        var userId =
            Guid.NewGuid();

        var create =
            new OrderCreateDto(
                "T",
                "D",
                null);

        var created =
            await svc.CreateAsync(
                create,
                userId);

        // --------------------------------------------------------
        // Basic mapping
        // --------------------------------------------------------

        Assert.Equal(
            create.Type,
            created.Type);

        Assert.Equal(
            create.Description,
            created.Description);

        // --------------------------------------------------------
        // Security / business rules
        // --------------------------------------------------------

        Assert.Equal(
            userId,
            created.UserId);

        Assert.Equal(
            "Pending",
            created.Status);

        Assert.Null(
            created.EstimatedPrice);

        // --------------------------------------------------------
        // Repository
        // --------------------------------------------------------

        mockRepo.Verify(
            r =>
                r.AddAsync(
                    It.Is<Order>(
                        o =>
                            o.UserId == userId &&
                            o.Status == "Pending" &&
                            o.EstimatedPrice == null)),
            Times.Once);
    }
}
