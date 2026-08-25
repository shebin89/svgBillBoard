using SvgBillBoard.Application.DTOs.Devices;

namespace SvgBillBoard.Application.Abstractions.Services;

public interface IDeviceAuthenticationService
{
    Task<DeviceLoginResponse> LoginAsync(
        DeviceLoginRequest request);
}