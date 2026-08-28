using Microsoft.EntityFrameworkCore;
using SvgBillBoard.Domain.Entities;
using SvgBillBoard.Domain.Interfaces;
using SvgBillBoard.Infrastructure.Persistence;

namespace SvgBillBoard.Infrastructure.Repositories;

public class PlaylistRepository : IPlaylistRepository
{
    private readonly ApplicationDbContext _context;

    public PlaylistRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Playlist?> GetByIdAsync(
        Guid id)
    {
        return await _context.Playlists
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Playlist?> GetByIdWithItemsAsync(
        Guid id)
    {
        return await _context.Playlists
            .Include(x => x.Items)
                .ThenInclude(x => x.Media)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Playlist>>
        GetByOrganizationIdAsync(
            Guid organizationId)
    {
        return await _context.Playlists
            .Where(x =>
                x.OrganizationId == organizationId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(
        Playlist playlist)
    {
        await _context.Playlists.AddAsync(playlist);
    }

    public Task UpdateAsync(
        Playlist playlist)
    {
        _context.Playlists.Update(playlist);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(
        Playlist playlist)
    {
        _context.Playlists.Remove(playlist);

        return Task.CompletedTask;
    }

    public async Task AddItemAsync(
        PlaylistItem item)
    {
        await _context.PlaylistItems.AddAsync(item);
    }

    public async Task<PlaylistItem?> GetItemByIdAsync(
        Guid itemId)
    {
        return await _context.PlaylistItems
            .FirstOrDefaultAsync(x => x.Id == itemId);
    }

    public Task DeleteItemAsync(
        PlaylistItem item)
    {
        _context.PlaylistItems.Remove(item);

        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<List<PlaylistItem>>GetItemsByPlaylistIdAsync(Guid playlistId)
    {
        return await _context.PlaylistItems
            .Where(x => x.PlaylistId == playlistId)
            .ToListAsync();
    }
}