using Handicraft.Application.Contracts.Persistence;
using Handicraft.Domain.Entities;
using Handicraft.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Handicraft.Infrastructure.Repositories;

public class CourseLessonProgressRepository
    : GenericRepository<CourseLessonProgress>,
      ICourseLessonProgressRepository
{
    public CourseLessonProgressRepository(
        AppDbContext context)
        : base(context)
    {
    }

    public async Task<CourseLessonProgress?> GetByUserAndLessonAsync(
        Guid userId,
        Guid lessonId)
    {
        return await _context.CourseLessonProgresses
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.LessonId == lessonId);
    }

    public async Task<bool> ExistsForUserAndLessonAsync(
        Guid userId,
        Guid lessonId)
    {
        return await _context.CourseLessonProgresses
            .AnyAsync(x =>
                x.UserId == userId &&
                x.LessonId == lessonId);
    }
}