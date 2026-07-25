using Handicraft.Domain.Entities;

namespace Handicraft.Application.Contracts.Persistence;

public interface ICourseLessonRepository
{
    Task<CourseLesson> AddAsync(CourseLesson lesson);

    Task<CourseLesson?> GetByIdAsync(Guid id);

    Task<IReadOnlyList<CourseLesson>> GetBySectionIdAsync(Guid sectionId);

    Task UpdateAsync(CourseLesson lesson);
}