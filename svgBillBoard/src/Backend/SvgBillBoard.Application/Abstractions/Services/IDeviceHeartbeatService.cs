using SvgBillBoard.Application.DTOs.Devices;

namespace SvgBillBoard.Application.Abstractions.Services;

public interface IDeviceHeartbeatService
{
    Task<DeviceHeartbeatResponse?> HeartbeatAsync(
        Guid deviceId);
}