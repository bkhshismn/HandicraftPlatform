using Handicraft.Application.Services.Dtos;

namespace Handicraft.Application.Contracts.Services;

public interface ICourseEnrollmentService
{
    Task<IEnumerable<CourseEnrollmentDto>> GetEnrollmentsByUserAsync(Guid userId);
    Task<IEnumerable<CourseEnrollmentDto>> GetEnrollmentsByCourseAsync(Guid courseId);
    Task<CourseEnrollmentDto> CreateAsync(CourseEnrollmentCreateDto dto);
}
