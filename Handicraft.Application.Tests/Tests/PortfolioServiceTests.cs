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

public class PortfolioServiceTests
{
    private IMapper CreateMapper()
    {
        var config = new MapperConfiguration(
            cfg => cfg.AddProfile<Handicraft.Application.Services.MappingProfile>(),
            NullLoggerFactory.Instance);

        return config.CreateMapper();
    }

    [Fact]
    public async Task GetPublishedAsync_ReturnsMapped()
    {
        var mockRepo = new Mock<IPortfolioItemRepository>();

        var items = new List<PortfolioItem>
        {
            new PortfolioItem
            {
                Id = Guid.NewGuid(),
                Title = "P1",
                Category = "C",
                Description = "d",
                IsVisible = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        mockRepo
            .Setup(r => r.GetPublishedItemsAsync())
            .ReturnsAsync(items);

        var svc = new PortfolioService(
            mockRepo.Object,
            CreateMapper());

        var result = await svc.GetPublishedAsync();

        Assert.Single(result);
    }
}
