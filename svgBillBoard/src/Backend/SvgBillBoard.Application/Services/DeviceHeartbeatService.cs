using SvgBillBoard.Application.Abstractions.Services;
using SvgBillBoard.Application.DTOs.Devices;
using SvgBillBoard.Domain.Interfaces;

namespace SvgBillBoard.Application.Services;

public class DeviceHeartbeatService : IDeviceHeartbeatService
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IDeviceStatusNotifier _statusNotifier;

    public DeviceHeartbeatService(
        IDeviceRepository deviceRepository,
        IDeviceStatusNotifier statusNotifier)
    {
        _deviceRepository = deviceRepository;
        _statusNotifier = statusNotifier;
    }

    public async Task<DeviceHeartbeatResponse?> HeartbeatAsync(
        Guid deviceId)
    {
        var device =
            await _deviceRepository
                .UpdateHeartbeatAsync(deviceId);

        if (device == null)
        {
            return null;
        }

        var status = new DeviceStatusChangedResponse
        {
            DeviceId = device.Id,
            OrganizationId = device.OrganizationId,
            DeviceCode = device.DeviceCode,
            DeviceName = device.Name,
            IsOnline = device.IsOnline,
            ChangedAt = DateTime.UtcNow
        };

        await _statusNotifier
            .NotifyStatusChangedAsync(status);

        return new DeviceHeartbeatResponse
        {
            DeviceId = device.Id,
            IsOnline = device.IsOnline,
            ServerTime = DateTime.UtcNow
        };
    }
}