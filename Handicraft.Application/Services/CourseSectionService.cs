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


        var result = await _repository.AddAsync(section);


        return new CourseSectionDto
        {
            Id = result.Id,
            CourseId = result.CourseId,
            Title = result.Title,
            OrderIndex = result.OrderIndex
        };
    }
}