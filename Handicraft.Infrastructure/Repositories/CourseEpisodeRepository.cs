using Handicraft.Application.Contracts.Persistence;
using Handicraft.Domain.Entities;
using Handicraft.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Handicraft.Infrastructure.Repositories;

public class CourseEpisodeRepository : ICourseEpisodeRepository
{
    private readonly AppDbContext _context;

    public CourseEpisodeRepository(AppDbContext context)
    {
        _context = context;
    }


    public async Task<CourseEpisode> AddAsync(
        CourseEpisode entity)
    {
        await _context.CourseEpisodes.AddAsync(entity);
        await _context.SaveChangesAsync();

        return entity;
    }


    public async Task UpdateAsync(
        CourseEpisode entity)
    {
        _context.CourseEpisodes.Update(entity);
        await _context.SaveChangesAsync();
    }


    public async Task DeleteAsync(
        CourseEpisode entity)
    {
        _context.CourseEpisodes.Remove(entity);
        await _context.SaveChangesAsync();
    }


    public async Task<bool> ExistsAsync(
        Guid id)
    {
        return await _context.CourseEpisodes
            .AnyAsync(x => x.Id == id);
    }


    public async Task<IReadOnlyList<CourseEpisode>> GetAllAsync()
    {
        return await _context.CourseEpisodes
            .Include(x => x.Lesson)
            .ToListAsync();
    }


    public async Task<IReadOnlyList<CourseEpisode>> GetAsync(
     Expression<Func<CourseEpisode, bool>> predicate)
    {
        return await _context.CourseEpisodes
            .Where(predicate)
            .ToListAsync();
    }


    public async Task<CourseEpisode?> GetByIdAsync(
        Guid id)
    {
        return await _context.CourseEpisodes
            .Include(x => x.Lesson)
            .FirstOrDefaultAsync(x => x.Id == id);
    }


    // قبلاً CourseId بود
    // فعلاً برای سازگاری Interface نگه می‌داریم
    public async Task<IReadOnlyList<CourseEpisode>> GetEpisodesByCourseIdAsync(
        Guid courseId)
    {
        return await _context.CourseEpisodes
            .Include(x => x.Lesson)
                .ThenInclude(x => x.Section)
            .Where(x => x.Lesson.Section.CourseId == courseId)
            .OrderBy(x => x.Order)
            .ToListAsync();
    }


    public async Task<CourseEpisode?> GetEpisodeByOrderAsync(
        Guid courseId,
        int order)
    {
        return await _context.CourseEpisodes
            .Include(x => x.Lesson)
                .ThenInclude(x => x.Section)
            .FirstOrDefaultAsync(x =>
                x.Lesson.Section.CourseId == courseId &&
                x.Order == order);
    }


    public async Task<CourseEpisode?> GetEpisodeWithCourseAsync(
        Guid id)
    {
        return await _context.CourseEpisodes
            .Include(x => x.Lesson)
                .ThenInclude(x => x.Section)
                    .ThenInclude(x => x.Course)
            .FirstOrDefaultAsync(x => x.Id == id);
    }
    public async Task<IReadOnlyList<CourseEpisode>> GetEpisodesByLessonIdAsync(
    Guid lessonId)
    {
        return await _context.CourseEpisodes
            .Where(x => x.LessonId == lessonId)
            .OrderBy(x => x.Order)
            .ToListAsync();
    }
}