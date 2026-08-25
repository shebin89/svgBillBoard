using SvgBillBoard.Domain.Entities;

namespace SvgBillBoard.Domain.Interfaces;

public interface IDeviceCredentialRepository
{
    Task<DeviceCredential?> GetByTokenHashAsync(
        string tokenHash);

    Task<DeviceCredential?> GetByDeviceIdAsync(
        Guid deviceId);

    Task AddAsync(
        DeviceCredential credential);

    Task UpdateAsync(
        DeviceCredential credential);

    Task SaveChangesAsync();
}