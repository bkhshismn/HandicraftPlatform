using Handicraft.Application.Contracts.Persistence;
using Handicraft.Application.Contracts.Services;
using Handicraft.Application.Services.Dtos;
using Handicraft.Domain.Entities;

namespace Handicraft.Application.Services;

public class CourseLessonService : ICourseLessonService
{
    private readonly ICourseLessonRepository _repository;


    public CourseLessonService(
        ICourseLessonRepository repository)
    {
        _repository = repository;
    }


    public async Task<CourseLessonDto> CreateAsync(
        Guid sectionId,
        CourseLessonCreateDto dto)
    {
        var lesson = new CourseLesson
        {
            Id = Guid.NewGuid(),
            SectionId = sectionId,
            Title = dto.Title,
            Description = dto.Description,
            DurationSeconds = dto.DurationSeconds,
            IsFreePreview = dto.IsFreePreview,
            OrderIndex = dto.OrderIndex,
            CreatedAt = DateTime.UtcNow
        };


        var result =
            await _repository.AddAsync(lesson);


        return new CourseLessonDto
        {
            Id = result.Id,
            Title = result.Title,
            Description = result.Description,
            DurationSeconds = result.DurationSeconds,
            IsFreePreview = result.IsFreePreview,
            OrderIndex = result.OrderIndex,
            HasVideo = false
        };
    }
}