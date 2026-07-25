using Handicraft.Domain.Entities;

namespace Handicraft.Application.Contracts.Persistence;

public interface ICourseRepository : IGenericRepository<Course>
{
    Task<IReadOnlyList<Course>> GetAllWithEpisodesAsync();
    Task<Course?> GetByIdWithEpisodesAsync(Guid id);
    Task<IReadOnlyList<Course>> GetPublishedCoursesAsync();
    Task<List<Course>> GetAllWithContentAsync();
    Task<Course?> GetByIdWithContentAsync(Guid id);
}
