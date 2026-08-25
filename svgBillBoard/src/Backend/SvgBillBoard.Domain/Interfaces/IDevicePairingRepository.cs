using SvgBillBoard.Domain.Entities;

namespace SvgBillBoard.Domain.Interfaces;

public interface IDevicePairingRepository
{
    Task<DevicePairing?> GetByCodeAsync(
        string pairingCode);

    Task AddAsync(
        DevicePairing pairing);

    Task UpdateAsync(
        DevicePairing pairing);

    Task SaveChangesAsync();
}