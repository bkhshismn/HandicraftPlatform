namespace Handicraft.Application.Services.Dtos;

public record UserDeviceCreateDto(
    Guid UserId,
    string DeviceId,
    string DeviceName,
    string IpAddress
);
