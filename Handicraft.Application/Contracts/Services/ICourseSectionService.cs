using Handicraft.Application.Services.Dtos;

namespace Handicraft.Application.Contracts.Services;

public interface ICourseSectionService
{
    Task<CourseSectionDto> CreateAsync(
        Guid courseId,
        CourseSectionCreateDto dto);

    Task<IEnumerable<CourseSectionDto>> GetByCourseIdAsync(
        Guid courseId);
}