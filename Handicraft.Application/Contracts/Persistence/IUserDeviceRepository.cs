using Handicraft.Domain.Entities;

namespace Handicraft.Application.Contracts.Persistence;

public interface IUserDeviceRepository : IGenericRepository<UserDevice>
{
    Task<IReadOnlyList<UserDevice>> GetDevicesByUserAsync(Guid userId);
    Task<UserDevice?> GetDeviceByTokenAsync(string deviceToken);
    Task<int> GetActiveDeviceCountAsync(Guid userId);
}
