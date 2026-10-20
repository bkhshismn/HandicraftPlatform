namespace Handicraft.Application.Contracts.Persistence;

public interface IUserCourseCategoryRepository
{
    Task<bool> HasAccessAsync(Guid userId, string category);
}
