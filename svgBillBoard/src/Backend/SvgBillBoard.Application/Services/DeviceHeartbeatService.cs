using SvgBillBoard.Application.Abstractions.Services;
using SvgBillBoard.Application.DTOs.Devices;
using SvgBillBoard.Domain.Interfaces;

namespace SvgBillBoard.Application.Services;

public class DeviceHeartbeatService
    : IDeviceHeartbeatService
{
    private readonly IDeviceRepository _deviceRepository;

    public DeviceHeartbeatService(
        IDeviceRepository deviceRepository)
    {
        _deviceRepository = deviceRepository;
    }

    public async Task<DeviceHeartbeatResponse?> HeartbeatAsync(
        Guid deviceId)
    {
        var updated =
            await _deviceRepository
                .UpdateHeartbeatAsync(deviceId);

        if (!updated)
        {
            return null;
        }

        return new DeviceHeartbeatResponse
        {
            DeviceId = deviceId,
            IsOnline = true,
            ServerTime = DateTime.UtcNow
        };
    }
}