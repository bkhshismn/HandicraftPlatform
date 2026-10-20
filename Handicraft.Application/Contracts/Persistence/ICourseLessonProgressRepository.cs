using Handicraft.Domain.Entities;

namespace Handicraft.Application.Contracts.Persistence;

public interface ICourseLessonProgressRepository
    : IGenericRepository<CourseLessonProgress>
{
    Task<CourseLessonProgress?> GetByUserAndLessonAsync(
        Guid userId,
        Guid lessonId);

    Task<CourseLessonProgress?> GetByUserAndLessonAndEpisodeAsync(
        Guid userId,
        Guid lessonId,
        Guid episodeId);

    Task<bool> ExistsForUserAndLessonAsync(
        Guid userId,
        Guid lessonId);
}
