using Microsoft.EntityFrameworkCore;
using SvgBillBoard.Domain.Entities;
using SvgBillBoard.Domain.Interfaces;
using SvgBillBoard.Infrastructure.Persistence;

namespace SvgBillBoard.Infrastructure.Repositories;

public class PlaylistScheduleRepository
    : IPlaylistScheduleRepository
{
    private readonly ApplicationDbContext _context;

    public PlaylistScheduleRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PlaylistSchedule?> GetByIdAsync(
        Guid id)
    {
        return await _context.PlaylistSchedules
            .Include(x => x.Playlist)
            .Include(x => x.Location)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<PlaylistSchedule>>
        GetByOrganizationIdAsync(
            Guid organizationId)
    {
        return await _context.PlaylistSchedules
            .Include(x => x.Playlist)
            .Include(x => x.Location)
            .Where(x =>
                x.OrganizationId == organizationId)
            .OrderByDescending(x => x.Priority)
            .ThenBy(x => x.StartTime)
            .ToListAsync();
    }

    public async Task<List<PlaylistSchedule>>
        GetActiveSchedulesAsync(
            Guid locationId)
    {
        return await _context.PlaylistSchedules
            .Include(x => x.Playlist)
                .ThenInclude(x => x.Items)
                .ThenInclude(x => x.Media)
            .Where(x =>
                x.LocationId == locationId &&
                x.Status == 1)
            .OrderByDescending(x => x.Priority)
            .ToListAsync();
    }

    public async Task AddAsync(
        PlaylistSchedule schedule)
    {
        await _context.PlaylistSchedules
            .AddAsync(schedule);
    }

    public Task UpdateAsync(
        PlaylistSchedule schedule)
    {
        _context.PlaylistSchedules
            .Update(schedule);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(
        PlaylistSchedule schedule)
    {
        _context.PlaylistSchedules
            .Remove(schedule);

        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}