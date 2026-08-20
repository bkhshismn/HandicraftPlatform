using Handicraft.Application.Contracts.Persistence;
using Handicraft.Domain.Entities;
using Handicraft.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Handicraft.Infrastructure.Repositories;

public class CourseEnrollmentRepository : GenericRepository<CourseEnrollment>, ICourseEnrollmentRepository
{
    public CourseEnrollmentRepository(AppDbContext context) : base(context)
    {
    }

    // Interface implementation: returns IEnumerable as declared in ICourseEnrollmentRepository
    public async Task<IEnumerable<CourseEnrollment>> GetEnrollmentsByUserAsync(Guid userId)
    {
        return await _context.CourseEnrollments
            .Include(e => e.Course)
            .Where(e => e.UserId == userId)
            .ToListAsync();
    }

    // Interface implementation: get enrollments for a specific course
    public async Task<IEnumerable<CourseEnrollment>> GetEnrollmentsByCourseAsync(Guid courseId)
    {
        return await _context.CourseEnrollments
            .Include(e => e.User)
            .Where(e => e.CourseId == courseId)
            .ToListAsync();
    }

    // Additional convenience method (kept) — returns single enrollment if exists
    public async Task<CourseEnrollment?> GetEnrollmentAsync(Guid userId, Guid courseId)
    {
        return await _context.CourseEnrollments
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);
    }

    // Check if a user is enrolled in a course
    public async Task<bool> IsUserEnrolledAsync(
    Guid userId,
    Guid courseId)
    {
        return await _context.CourseEnrollments
            .AnyAsync(e =>
                e.UserId == userId &&
                e.CourseId == courseId &&
                e.IsActive);
    }
}
