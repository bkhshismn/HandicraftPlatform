using Handicraft.Application.Services.Dtos;

namespace Handicraft.Application.Contracts.Services;

public interface ICourseService
{
    Task<IEnumerable<CourseDto>> GetAllAsync();
    Task<IEnumerable<CourseDto>> GetPublishedAsync();
    Task<CourseDto?> GetByIdAsync(Guid id);
    Task<CourseDto> CreateAsync(CourseCreateDto dto);
    Task<CourseDto?> SetPublishedAsync(Guid id, bool isPublished);
}
