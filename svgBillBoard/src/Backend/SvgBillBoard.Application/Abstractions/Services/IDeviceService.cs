using SvgBillBoard.Application.DTOs.Devices;

namespace SvgBillBoard.Application.Abstractions.Services;

public interface IDeviceService
{
    Task<PairingResponse> GeneratePairingAsync(
        Guid organizationId,
        GeneratePairingRequest request);

    Task<DeviceAuthenticationResponse> PairDeviceAsync(
    PairDeviceRequest request);

    Task<List<DeviceResponse>> GetAllAsync(
        Guid organizationId);

    Task<DeviceResponse?> GetByIdAsync(
        Guid organizationId,
        Guid id);
}