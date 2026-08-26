using Handicraft.Application.Contracts.Persistence;
using Handicraft.Application.Contracts.Services;
using Handicraft.Application.Services.Dtos;
using Handicraft.Domain.Entities;

namespace Handicraft.Application.Services;

public class CourseSectionService : ICourseSectionService
{
    private readonly ICourseSectionRepository _repository;

    public CourseSectionService(
        ICourseSectionRepository repository)
    {
        _repository = repository;
    }

    // ============================================================
    // Create Section
    // ============================================================

    public async Task<CourseSectionDto> CreateAsync(
        Guid courseId,
        CourseSectionCreateDto dto)
    {
        var section = new CourseSection
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Title = dto.Title,
            OrderIndex = dto.OrderIndex,
            CreatedAt = DateTime.UtcNow
        };

        var result =
            await _repository.AddAsync(section);

        return new CourseSectionDto
        {
            Id = result.Id,
            CourseId = result.CourseId,
            Title = result.Title,
            OrderIndex = result.OrderIndex
        };
    }

    // ============================================================
    // Get Sections By Course
    // ============================================================

    public async Task<IEnumerable<CourseSectionDto>>
        GetByCourseIdAsync(Guid courseId)
    {
        var sections =
            await _repository.GetByCourseIdAsync(courseId);

        return sections
            .OrderBy(s => s.OrderIndex)
            .Select(s => new CourseSectionDto
            {
                Id = s.Id,
                CourseId = s.CourseId,
                Title = s.Title,
                OrderIndex = s.OrderIndex
            })
            .ToList();
    }
}