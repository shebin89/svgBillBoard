using Microsoft.EntityFrameworkCore;
using SvgBillBoard.Domain.Entities;
using SvgBillBoard.Domain.Interfaces;
using SvgBillBoard.Infrastructure.Persistence;

namespace SvgBillBoard.Infrastructure.Repositories;

public class LocationRepository : ILocationRepository
{
    private readonly ApplicationDbContext _context;

    public LocationRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Location?> GetByIdAsync(Guid id)
    {
        return await _context.Locations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Location>>
        GetByOrganizationIdAsync(
            Guid organizationId)
    {
        return await _context.Locations
            .AsNoTracking()
            .Where(x =>
                x.OrganizationId == organizationId)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<bool> ExistsByCodeAsync(
        Guid organizationId,
        string code)
    {
        return await _context.Locations
            .AnyAsync(x =>
                x.OrganizationId == organizationId &&
                x.Code == code);
    }

    public async Task AddAsync(Location location)
    {
        await _context.Locations.AddAsync(location);
    }

    public Task UpdateAsync(Location location)
    {
        _context.Locations.Update(location);

        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}