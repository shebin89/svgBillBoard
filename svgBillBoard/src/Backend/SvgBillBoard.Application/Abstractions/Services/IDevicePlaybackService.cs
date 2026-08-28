using SvgBillBoard.Application.DTOs.Devices;

namespace SvgBillBoard.Application.Abstractions.Services;

public interface IDevicePlaybackService
{
    Task<DevicePlaybackResponse> GetPlaybackAsync(
        Guid deviceId);
}