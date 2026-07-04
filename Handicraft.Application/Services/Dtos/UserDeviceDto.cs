namespace Handicraft.Application.Services.Dtos;

public record UserDeviceDto(
    Guid Id,
    Guid UserId,
    string DeviceId,
    string DeviceName,
    string IpAddress,
    DateTime LastSeenAt,
    bool IsActive
);
