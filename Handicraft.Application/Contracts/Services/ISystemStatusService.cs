using Handicraft.Application.Services.Dtos;

namespace Handicraft.Application.Contracts.Services;

public interface ISystemStatusService
{
    Task<SystemStatusDto> GetStatusAsync(
        CancellationToken cancellationToken = default);
}
