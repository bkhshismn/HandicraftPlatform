using Handicraft.Application.Contracts.Persistence;
using Handicraft.Domain.Entities;
using Handicraft.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Handicraft.Infrastructure.Repositories;

public class CourseRepository : GenericRepository<Course>, ICourseRepository
{
    public CourseRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<Course>> GetAllWithEpisodesAsync()
    {
        return await _context.Courses
            .Include(c => c.Episodes)
            .ToListAsync();
    }

    public async Task<Course?> GetByIdWithEpisodesAsync(Guid id)
    {
        return await _context.Courses
            .Include(c => c.Episodes)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IReadOnlyList<Course>> GetPublishedCoursesAsync()
    {
        return await _context.Courses
            .Where(x => x.IsPublished)
            .Include(x => x.Sections)
                .ThenInclude(x => x.Lessons)
                    .ThenInclude(x => x.Episodes)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Course>> GetAllWithContentAsync()
    {
        return await _context.Courses
            .Include(x => x.Sections)
                .ThenInclude(x => x.Lessons)
                    .ThenInclude(x => x.Episodes)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Course?> GetByIdWithContentAsync(Guid id)
    {
        return await _context.Courses
            .Include(x => x.Sections)
                .ThenInclude(x => x.Lessons)
                    .ThenInclude(x => x.Episodes)
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}