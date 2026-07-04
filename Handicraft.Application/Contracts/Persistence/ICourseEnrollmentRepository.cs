using Handicraft.Domain.Entities;

namespace Handicraft.Application.Contracts.Persistence;

public interface ICourseEnrollmentRepository : IGenericRepository<CourseEnrollment>
{
    Task<IEnumerable<CourseEnrollment>> GetEnrollmentsByUserAsync(Guid userId);
    Task<IEnumerable<CourseEnrollment>> GetEnrollmentsByCourseAsync(Guid courseId);
    Task<bool> IsUserEnrolledAsync(Guid userId, Guid courseId);
}
