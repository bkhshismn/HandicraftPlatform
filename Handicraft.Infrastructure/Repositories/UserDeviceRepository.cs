using Handicraft.Application.Contracts.Persistence;
using Handicraft.Domain.Entities;
using Handicraft.Infrastructure.Persistence;
using Handicraft.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Handicraft.Infrastructure.Repositories;

public class UserDeviceRepository : GenericRepository<UserDevice>, IUserDeviceRepository
{
    public UserDeviceRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<UserDevice>> GetDevicesByUserAsync(Guid userId)
    {
        return await _context.UserDevices
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.LastSeenAt)
            .ToListAsync();
    }

    public async Task<UserDevice?> GetDeviceByTokenAsync(string deviceToken)
    {
        // The domain stores a `DeviceId` field (token/unique id). Match that here.
        return await _context.UserDevices
            .FirstOrDefaultAsync(d => d.DeviceId == deviceToken);
    }

    public async Task<int> GetActiveDeviceCountAsync(Guid userId)
    {
        return await _context.UserDevices
            .CountAsync(d => d.UserId == userId && d.IsActive);
    }

    // Keep existing convenience method (name changed to match domain) if callers need it
    public async Task<bool> IsDeviceRegisteredAsync(Guid userId, string deviceId)
    {
        return await _context.UserDevices
            .AnyAsync(d => d.UserId == userId && d.DeviceId == deviceId);
    }
}
