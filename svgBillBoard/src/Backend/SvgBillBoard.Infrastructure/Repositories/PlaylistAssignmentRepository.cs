using Microsoft.EntityFrameworkCore;
using SvgBillBoard.Domain.Entities;
using SvgBillBoard.Domain.Interfaces;
using SvgBillBoard.Infrastructure.Persistence;

namespace SvgBillBoard.Infrastructure.Repositories;

public class PlaylistAssignmentRepository
    : IPlaylistAssignmentRepository
{
    private readonly ApplicationDbContext _context;

    public PlaylistAssignmentRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PlaylistAssignment?> GetByIdAsync(
        Guid id)
    {
        return await _context.PlaylistAssignments
            .Include(x => x.Playlist)
            .Include(x => x.Location)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<PlaylistAssignment>>GetByOrganizationIdAsync(Guid organizationId)
    {
        return await _context.PlaylistAssignments
            .Include(x => x.Playlist)
            .Include(x => x.Location)
            .Where(x =>
                x.OrganizationId == organizationId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<PlaylistAssignment?>GetByLocationIdAsync(Guid locationId)
    {
        return await _context.PlaylistAssignments
            .Include(x => x.Playlist)
                .ThenInclude(x => x.Items)
                .ThenInclude(x => x.Media)
            .FirstOrDefaultAsync(x =>
                x.LocationId == locationId &&
                x.Status == 1);
    }

    public async Task AddAsync(
        PlaylistAssignment assignment)
    {
        await _context.PlaylistAssignments
            .AddAsync(assignment);
    }

    public Task DeleteAsync(
        PlaylistAssignment assignment)
    {
        _context.PlaylistAssignments
            .Remove(assignment);

        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<List<PlaylistAssignment>>GetByPlaylistIdAsync(Guid playlistId)
    {
        return await _context.PlaylistAssignments
            .Where(x =>
                x.PlaylistId == playlistId &&
                x.Status == 1)
            .ToListAsync();
    }
}