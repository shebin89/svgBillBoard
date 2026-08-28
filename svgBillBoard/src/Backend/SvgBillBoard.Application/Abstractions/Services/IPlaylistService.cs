using SvgBillBoard.Application.DTOs.Playlists;

namespace SvgBillBoard.Application.Abstractions.Services;

public interface IPlaylistService
{
    Task<PlaylistResponse> CreateAsync(
        Guid organizationId,
        CreatePlaylistRequest request);

    Task<List<PlaylistResponse>> GetAllAsync(
        Guid organizationId);

    Task<PlaylistResponse?> GetByIdAsync(
        Guid organizationId,
        Guid id);

    Task<bool> DeleteAsync(
        Guid organizationId,
        Guid id);

    Task<PlaylistItemResponse> AddItemAsync(
        Guid organizationId,
        Guid playlistId,
        AddPlaylistItemRequest request);

    Task<bool> DeleteItemAsync(
        Guid organizationId,
        Guid playlistId,
        Guid itemId);
    Task<bool> ReorderItemsAsync(
    Guid organizationId,
    Guid playlistId,
    ReorderPlaylistItemsRequest request);
}