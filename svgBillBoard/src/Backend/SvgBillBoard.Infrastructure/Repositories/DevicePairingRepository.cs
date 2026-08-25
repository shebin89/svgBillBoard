using Microsoft.EntityFrameworkCore;
using SvgBillBoard.Domain.Entities;
using SvgBillBoard.Domain.Interfaces;
using SvgBillBoard.Infrastructure.Persistence;

namespace SvgBillBoard.Infrastructure.Repositories;

public class DevicePairingRepository
    : IDevicePairingRepository
{
    private readonly ApplicationDbContext _context;

    public DevicePairingRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DevicePairing?> GetByCodeAsync(
        string pairingCode)
    {
        return await _context.DevicePairings
            .FirstOrDefaultAsync(x =>
                x.PairingCode == pairingCode);
    }

    public async Task AddAsync(
        DevicePairing pairing)
    {
        await _context.DevicePairings.AddAsync(pairing);
    }

    public Task UpdateAsync(
        DevicePairing pairing)
    {
        _context.DevicePairings.Update(pairing);

        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}