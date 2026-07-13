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

public class CourseEnrollmentServiceTests
{
    private IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<Handicraft.Application.Services.MappingProfile>());
        return config.CreateMapper();
    }

    [Fact]
    public async Task GetEnrollmentsByUserAsync_ReturnsMapped()
    {
        var mockRepo = new Mock<ICourseEnrollmentRepository>();
        var items = new List<CourseEnrollment>
        {
            new CourseEnrollment { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), CourseId = Guid.NewGuid(), PaidAmount = 10, EnrolledAt = DateTime.UtcNow, IsActive = true, CreatedAt = DateTime.UtcNow }
        };
        mockRepo.Setup(r => r.GetEnrollmentsByUserAsync(It.IsAny<Guid>())).ReturnsAsync(items);

        var svc = new CourseEnrollmentService(mockRepo.Object, CreateMapper());
        var res = await svc.GetEnrollmentsByUserAsync(Guid.NewGuid());
        Assert.Single(res);
    }

    [Fact]
    public async Task CreateAsync_CreatesAndReturns()
    {
        var mockRepo = new Mock<ICourseEnrollmentRepository>();
        mockRepo.Setup(r => r.AddAsync(It.IsAny<CourseEnrollment>())).ReturnsAsync((CourseEnrollment e) => e);

        var svc = new CourseEnrollmentService(mockRepo.Object, CreateMapper());
        var create = new CourseEnrollmentCreateDto(Guid.NewGuid(), Guid.NewGuid(), 20m, null);
        var created = await svc.CreateAsync(create);
        Assert.Equal(create.PaidAmount, created.PaidAmount);
        mockRepo.Verify(r => r.AddAsync(It.IsAny<CourseEnrollment>()), Times.Once);
    }
}
