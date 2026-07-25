using Handicraft.Application.Contracts.Persistence;
using Handicraft.Domain.Entities;
using Handicraft.Infrastructure.Persistence;
using Handicraft.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Handicraft.Infrastructure.Repositories;

public class CourseSectionRepository : ICourseSectionRepository
{
    private readonly AppDbContext _context;


    public CourseSectionRepository(AppDbContext context)
    {
        _context = context;
    }


    public async Task<CourseSection> AddAsync(
        CourseSection section)
    {
        await _context.CourseSections.AddAsync(section);
        await _context.SaveChangesAsync();

        return section;
    }


    public async Task<CourseSection?> GetByIdAsync(Guid id)
    {
        return await _context.CourseSections
            .FirstOrDefaultAsync(x => x.Id == id);
    }


    public async Task<IReadOnlyList<CourseSection>> GetByCourseIdAsync(
        Guid courseId)
    {
        return await _context.CourseSections
            .Where(x => x.CourseId == courseId)
            .OrderBy(x => x.OrderIndex)
            .ToListAsync();
    }


    public async Task UpdateAsync(
        CourseSection section)
    {
        _context.CourseSections.Update(section);
        await _context.SaveChangesAsync();
    }
}