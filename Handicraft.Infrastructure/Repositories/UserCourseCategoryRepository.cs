using Handicraft.Application.Contracts.Persistence;
using Handicraft.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Handicraft.Infrastructure.Repositories;

public sealed class UserCourseCategoryRepository : IUserCourseCategoryRepository
{
    private readonly AppDbContext _db;

    public UserCourseCategoryRepository(AppDbContext db) => _db = db;

    public Task<bool> HasAccessAsync(Guid userId, string category) =>
        _db.UserCourseCategories.AnyAsync(x => x.UserId == userId && x.Category == category);
}
