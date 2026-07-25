using Handicraft.Domain.Entities;

namespace Handicraft.Application.Contracts.Persistence;

public interface ICourseSectionRepository
{
    Task<CourseSection> AddAsync(CourseSection section);

    Task<CourseSection?> GetByIdAsync(Guid id);

    Task<IReadOnlyList<CourseSection>> GetByCourseIdAsync(Guid courseId);

    Task UpdateAsync(CourseSection section);
}