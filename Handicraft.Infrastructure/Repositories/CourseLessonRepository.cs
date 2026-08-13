using Handicraft.Application.Contracts.Persistence;
using Handicraft.Domain.Entities;
using Handicraft.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Handicraft.Infrastructure.Repositories;

public class CourseLessonRepository : ICourseLessonRepository
{
    private readonly AppDbContext _context;

    public CourseLessonRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CourseLesson> AddAsync(
        CourseLesson lesson)
    {
        await _context.CourseLessons.AddAsync(lesson);
        await _context.SaveChangesAsync();

        return lesson;
    }

    public async Task<CourseLesson?> GetByIdAsync(
        Guid id)
    {
        return await _context.CourseLessons
            .Include(x => x.Episodes)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IReadOnlyList<CourseLesson>> GetBySectionIdAsync(
        Guid sectionId)
    {
        return await _context.CourseLessons
            .Where(x => x.SectionId == sectionId)
            .OrderBy(x => x.OrderIndex)
            .ToListAsync();
    }

    public async Task UpdateAsync(
        CourseLesson lesson)
    {
        _context.CourseLessons.Update(lesson);
        await _context.SaveChangesAsync();
    }
}