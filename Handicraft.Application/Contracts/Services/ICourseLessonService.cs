using Handicraft.Application.Services.Dtos;

namespace Handicraft.Application.Contracts.Services;

public interface ICourseLessonService
{
    Task<CourseLessonDto> CreateAsync(
        Guid sectionId,
        CourseLessonCreateDto dto);

    Task<IEnumerable<CourseLessonDto>> GetBySectionIdAsync(
        Guid sectionId);
}