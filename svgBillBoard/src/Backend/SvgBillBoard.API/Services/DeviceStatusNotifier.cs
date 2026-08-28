using Microsoft.AspNetCore.SignalR;
using SvgBillBoard.API.Hubs;
using SvgBillBoard.Application.Abstractions.Services;
using SvgBillBoard.Application.DTOs.Devices;

namespace SvgBillBoard.API.Services;

public class DeviceStatusNotifier
    : IDeviceStatusNotifier
{
    private readonly IHubContext<DeviceStatusHub>
        _hubContext;

    public DeviceStatusNotifier(
        IHubContext<DeviceStatusHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyStatusChangedAsync(
        DeviceStatusChangedResponse status)
    {
        await _hubContext.Clients
            .Group(
                $"organization:{status.OrganizationId}")
            .SendAsync(
                "deviceStatusChanged",
                status);
    }
}