using Handicraft.Application.Contracts.Persistence;
using Handicraft.Domain.Entities;
using Handicraft.Infrastructure.Persistence;
using Handicraft.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Handicraft.Infrastructure.Repositories;

public class CourseEpisodeRepository : GenericRepository<CourseEpisode>, ICourseEpisodeRepository
{
    public CourseEpisodeRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<CourseEpisode>> GetEpisodesByCourseIdAsync(Guid courseId)
    {
        return await _context.CourseEpisodes
            .Where(e => e.CourseId == courseId)
            .OrderBy(e => e.Order)
            .ToListAsync();
    }

    public async Task<CourseEpisode?> GetEpisodeByOrderAsync(Guid courseId, int order)
    {
        return await _context.CourseEpisodes
            .FirstOrDefaultAsync(e => e.CourseId == courseId && e.Order == order);
    }

    public async Task<CourseEpisode?> GetEpisodeWithCourseAsync(Guid episodeId)
    {
        return await _context.CourseEpisodes
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == episodeId);
    }
}
