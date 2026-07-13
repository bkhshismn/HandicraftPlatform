using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Handicraft.Application.Services;
using Handicraft.Application.Services.Dtos;
using Handicraft.Application.Contracts.Persistence;
using Handicraft.Domain.Entities;
using Moq;
using Xunit;

namespace Handicraft.Application.Tests.Tests;

public class CourseServiceTests
{
    [Fact]
    public async Task GetAllAsync_Returns_MappedDtos()
    {
        var mockRepo = new Mock<ICourseRepository>();
        var courses = new List<Course>
        {
            new Course { Id = Guid.NewGuid(), Title = "C1", Description = "D1", Price = 10, CoverImageUrl = "u", IsPublished = true, DurationMinutes = 60 }
        };
        mockRepo.Setup(r => r.GetAllWithEpisodesAsync()).ReturnsAsync(courses);

        var svc = new CourseService(mockRepo.Object);

        var result = await svc.GetAllAsync();

        Assert.NotNull(result);
        Assert.Collection(result, c => Assert.Equal("C1", c.Title));
    }

    [Fact]
    public async Task CreateAsync_Calls_Repository_And_ReturnsDto()
    {
        var mockRepo = new Mock<ICourseRepository>();
        mockRepo.Setup(r => r.AddAsync(It.IsAny<Course>())).ReturnsAsync((Course c) => c);

        var svc = new CourseService(mockRepo.Object);

        var dto = new CourseCreateDto("T","Desc","img",9.99m,false,100,null);
        var created = await svc.CreateAsync(dto);

        Assert.Equal(dto.Title, created.Title);
        mockRepo.Verify(r => r.AddAsync(It.IsAny<Course>()), Times.Once);
    }
}
