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

    Task<bool> UpdateHeartbeatAsync(Guid deviceId);
}