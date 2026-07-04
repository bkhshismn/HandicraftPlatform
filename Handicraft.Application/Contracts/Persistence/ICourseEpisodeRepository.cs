using Handicraft.Domain.Entities;

namespace Handicraft.Application.Contracts.Persistence;

public interface ICourseEpisodeRepository : IGenericRepository<CourseEpisode>
{
    Task<IReadOnlyList<CourseEpisode>> GetEpisodesByCourseIdAsync(Guid courseId);
    Task<CourseEpisode?> GetEpisodeByOrderAsync(Guid courseId, int order);
    Task<CourseEpisode?> GetEpisodeWithCourseAsync(Guid episodeId);
}
