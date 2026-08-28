using SvgBillBoard.Application.DTOs.Devices;

namespace SvgBillBoard.Application.Abstractions.Services;

public interface IDeviceStatusNotifier
{
    Task NotifyStatusChangedAsync(
        DeviceStatusChangedResponse status);
}