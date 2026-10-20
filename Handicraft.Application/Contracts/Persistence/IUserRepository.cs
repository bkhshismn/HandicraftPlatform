using Handicraft.Domain.Entities;

namespace Handicraft.Application.Contracts.Persistence;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByPhoneAsync(string phone);
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> IsUsernameUniqueAsync(string username);
    Task<bool> IsPhoneUniqueAsync(string phone);
}
