using SvgBillBoard.Domain.Entities;

namespace SvgBillBoard.Domain.Interfaces;

public interface IDeviceRepository
{
    Task<Device?> GetByIdAsync(Guid id);

    Task<List<Device>> GetByOrganizationIdAsync(
        Guid organizationId);

    Task<bool> ExistsByIdentifierAsync(
        string deviceIdentifier);

    Task<bool> ExistsByCodeAsync(
        Guid organizationId,
        string deviceCode);

    Task AddAsync(Device device);

    Task UpdateAsync(Device device);

    Task SaveChangesAsync();

    Task<Device?> UpdateHeartbeatAsync(Guid deviceId);

    Task<List<Device>> GetOnlineDevicesAsync();

    Task<List<Device>> GetByLocationIdAsync(Guid locationId);
}