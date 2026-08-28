using Microsoft.EntityFrameworkCore;
using SvgBillBoard.Domain.Entities;
using SvgBillBoard.Domain.Interfaces;
using SvgBillBoard.Infrastructure.Persistence;

namespace SvgBillBoard.Infrastructure.Repositories;

public class DeviceRepository : IDeviceRepository
{
    private readonly ApplicationDbContext _context;

    public DeviceRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Device?> GetByIdAsync(Guid id)
    {
        return await _context.Devices
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Device>>
        GetByOrganizationIdAsync(
            Guid organizationId)
    {
        return await _context.Devices
            .AsNoTracking()
            .Where(x =>
                x.OrganizationId == organizationId)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<bool> ExistsByIdentifierAsync(
        string deviceIdentifier)
    {
        return await _context.Devices
            .AnyAsync(x =>
                x.DeviceIdentifier ==
                deviceIdentifier);
    }

    public async Task<bool> ExistsByCodeAsync(
        Guid organizationId,
        string deviceCode)
    {
        return await _context.Devices
            .AnyAsync(x =>
                x.OrganizationId == organizationId &&
                x.DeviceCode == deviceCode);
    }

    public async Task AddAsync(Device device)
    {
        await _context.Devices.AddAsync(device);
    }

    public Task UpdateAsync(Device device)
    {
        _context.Devices.Update(device);

        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<Device?> UpdateHeartbeatAsync(Guid deviceId)
    {
        var device = await _context.Devices
            .FirstOrDefaultAsync(x =>
                x.Id == deviceId &&
                x.Status == 1);

        if (device == null)
        {
            return null;
        }

        var now = DateTime.UtcNow;

        device.LastHeartbeatAt = now;
        device.LastSeenAt = now;
        device.IsOnline = true;
        device.LastOnlineAt = now;
        device.UpdatedAt = now;

        await _context.SaveChangesAsync();

        return device;
    }

    public async Task<List<Device>>GetOnlineDevicesAsync()
    {
        return await _context.Devices
            .Where(x => x.IsOnline)
            .ToListAsync();
    }

    public async Task<List<Device>> GetByLocationIdAsync(
    Guid locationId)
    {
        return await _context.Devices
            .Where(x => x.LocationId == locationId)
            .ToListAsync();
    }
}