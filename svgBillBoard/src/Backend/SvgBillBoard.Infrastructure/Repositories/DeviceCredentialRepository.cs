using Microsoft.EntityFrameworkCore;
using SvgBillBoard.Domain.Entities;
using SvgBillBoard.Domain.Interfaces;
using SvgBillBoard.Infrastructure.Persistence;

namespace SvgBillBoard.Infrastructure.Repositories;

public class DeviceCredentialRepository
    : IDeviceCredentialRepository
{
    private readonly ApplicationDbContext _context;

    public DeviceCredentialRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DeviceCredential?>
        GetByTokenHashAsync(string tokenHash)
    {
        return await _context.DeviceCredentials
            .Include(x => x.Device)
            .FirstOrDefaultAsync(x =>
                x.TokenHash == tokenHash);
    }

    public async Task<DeviceCredential?>
        GetByDeviceIdAsync(Guid deviceId)
    {
        return await _context.DeviceCredentials
            .FirstOrDefaultAsync(x =>
                x.DeviceId == deviceId &&
                x.RevokedAt == null);
    }

    public async Task AddAsync(
        DeviceCredential credential)
    {
        await _context.DeviceCredentials
            .AddAsync(credential);
    }

    public Task UpdateAsync(
        DeviceCredential credential)
    {
        _context.DeviceCredentials
            .Update(credential);

        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}