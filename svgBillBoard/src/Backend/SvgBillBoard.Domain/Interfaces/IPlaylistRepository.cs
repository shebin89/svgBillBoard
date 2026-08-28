using SvgBillBoard.Domain.Entities;

namespace SvgBillBoard.Domain.Interfaces;

public interface IPlaylistRepository
{
    Task<Playlist?> GetByIdAsync(Guid id);

    Task<Playlist?> GetByIdWithItemsAsync(Guid id);

    Task<List<Playlist>> GetByOrganizationIdAsync(
        Guid organizationId);

    Task AddAsync(Playlist playlist);

    Task UpdateAsync(Playlist playlist);

    Task DeleteAsync(Playlist playlist);

    Task AddItemAsync(PlaylistItem item);

    Task<PlaylistItem?> GetItemByIdAsync(Guid itemId);

    Task DeleteItemAsync(PlaylistItem item);
    Task<List<PlaylistItem>> GetItemsByPlaylistIdAsync(
    Guid playlistId);
    Task SaveChangesAsync();
}