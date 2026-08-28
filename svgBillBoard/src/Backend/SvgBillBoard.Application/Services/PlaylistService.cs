using SvgBillBoard.Application.Abstractions.Services;
using SvgBillBoard.Application.DTOs.Playlists;
using SvgBillBoard.Domain.Entities;
using SvgBillBoard.Domain.Interfaces;

namespace SvgBillBoard.Application.Services;

public class PlaylistService : IPlaylistService
{
    private readonly IPlaylistRepository _playlistRepository;
    private readonly IMediaRepository _mediaRepository;
    private readonly IPlaybackVersionService _playbackVersionService;

    public PlaylistService(
        IPlaylistRepository playlistRepository,
        IMediaRepository mediaRepository,
        IPlaybackVersionService playbackVersionService)
    {
        _playlistRepository = playlistRepository;
        _mediaRepository = mediaRepository;
        _playbackVersionService = playbackVersionService;
    }

    public async Task<PlaylistResponse> CreateAsync(
        Guid organizationId,
        CreatePlaylistRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException(
                "Playlist name is required.");
        }

        var playlist = new Playlist
        {
            Id = Guid.NewGuid(),

            OrganizationId = organizationId,

            Name = request.Name.Trim(),

            Description =
                string.IsNullOrWhiteSpace(request.Description)
                    ? null
                    : request.Description.Trim(),

            Status = 1,

            CreatedAt = DateTime.UtcNow,

            UpdatedAt = DateTime.UtcNow
        };

        await _playlistRepository.AddAsync(playlist);

        await _playlistRepository.SaveChangesAsync();

        return Map(playlist);
    }

    public async Task<List<PlaylistResponse>> GetAllAsync(
        Guid organizationId)
    {
        var playlists =
            await _playlistRepository
                .GetByOrganizationIdAsync(
                    organizationId);

        var result =
            new List<PlaylistResponse>();

        foreach (var playlist in playlists)
        {
            var playlistWithItems =
                await _playlistRepository
                    .GetByIdWithItemsAsync(
                        playlist.Id);

            if (playlistWithItems != null)
            {
                result.Add(
                    Map(playlistWithItems));
            }
        }

        return result;
    }

    public async Task<PlaylistResponse?> GetByIdAsync(
        Guid organizationId,
        Guid id)
    {
        var playlist =
            await _playlistRepository
                .GetByIdWithItemsAsync(id);

        if (playlist == null ||
            playlist.OrganizationId != organizationId)
        {
            return null;
        }

        return Map(playlist);
    }

    public async Task<bool> DeleteAsync(
        Guid organizationId,
        Guid id)
    {
        var playlist =
            await _playlistRepository
                .GetByIdAsync(id);

        if (playlist == null ||
            playlist.OrganizationId != organizationId)
        {
            return false;
        }

        await _playlistRepository
            .DeleteAsync(playlist);

        await _playlistRepository
            .SaveChangesAsync();

        return true;
    }

    public async Task<PlaylistItemResponse> AddItemAsync(
        Guid organizationId,
        Guid playlistId,
        AddPlaylistItemRequest request)
    {
        if (request.MediaId == Guid.Empty)
        {
            throw new ArgumentException(
                "MediaId is required.");
        }

        if (request.DurationSeconds < 1)
        {
            throw new ArgumentException(
                "Duration must be at least 1 second.");
        }

        var playlist =
            await _playlistRepository
                .GetByIdAsync(playlistId);

        if (playlist == null ||
            playlist.OrganizationId != organizationId)
        {
            throw new InvalidOperationException(
                "Playlist was not found.");
        }

        var media =
            await _mediaRepository
                .GetByIdAsync(request.MediaId);

        if (media == null ||
            media.OrganizationId != organizationId)
        {
            throw new InvalidOperationException(
                "Media was not found.");
        }

        var item = new PlaylistItem
        {
            Id = Guid.NewGuid(),

            PlaylistId = playlistId,

            MediaId = media.Id,

            DisplayOrder =
                request.DisplayOrder,

            DurationSeconds =
                request.DurationSeconds,

            CreatedAt = DateTime.UtcNow
        };

        await _playlistRepository
            .AddItemAsync(item);

        await _playlistRepository
            .SaveChangesAsync();

        await _playbackVersionService
            .IncrementForPlaylistAsync(
                playlistId);

        return new PlaylistItemResponse
        {
            Id = item.Id,

            MediaId = media.Id,

            MediaName = media.Name,

            FileUrl = media.FileUrl,

            ContentType = media.ContentType,

            DisplayOrder =
                item.DisplayOrder,

            DurationSeconds =
                item.DurationSeconds
        };
    }

    public async Task<bool> DeleteItemAsync(
        Guid organizationId,
        Guid playlistId,
        Guid itemId)
    {
        var playlist =
            await _playlistRepository
                .GetByIdAsync(playlistId);

        if (playlist == null ||
            playlist.OrganizationId != organizationId)
        {
            return false;
        }

        var item =
            await _playlistRepository
                .GetItemByIdAsync(itemId);

        if (item == null ||
            item.PlaylistId != playlistId)
        {
            return false;
        }

        await _playlistRepository
            .DeleteItemAsync(item);

        await _playlistRepository
            .SaveChangesAsync();

        await _playbackVersionService
            .IncrementForPlaylistAsync(
                playlistId);

        return true;
    }

    public async Task<bool> ReorderItemsAsync(
        Guid organizationId,
        Guid playlistId,
        ReorderPlaylistItemsRequest request)
    {
        var playlist =
            await _playlistRepository
                .GetByIdAsync(playlistId);

        if (playlist == null ||
            playlist.OrganizationId != organizationId)
        {
            return false;
        }

        if (request.Items == null ||
            request.Items.Count == 0)
        {
            throw new ArgumentException(
                "At least one playlist item is required.");
        }

        var existingItems =
            await _playlistRepository
                .GetItemsByPlaylistIdAsync(
                    playlistId);

        var existingIds =
            existingItems
                .Select(x => x.Id)
                .ToHashSet();

        foreach (var requestedItem in request.Items)
        {
            if (!existingIds.Contains(
                    requestedItem.ItemId))
            {
                throw new InvalidOperationException(
                    $"Playlist item '{requestedItem.ItemId}' was not found.");
            }

            if (requestedItem.DisplayOrder < 1)
            {
                throw new ArgumentException(
                    "Display order must be greater than zero.");
            }
        }

        var duplicateOrders =
            request.Items
                .GroupBy(x => x.DisplayOrder)
                .Any(x => x.Count() > 1);

        if (duplicateOrders)
        {
            throw new ArgumentException(
                "Display orders must be unique.");
        }

        foreach (var requestedItem in request.Items)
        {
            var item =
                existingItems.First(
                    x => x.Id ==
                         requestedItem.ItemId);

            item.DisplayOrder =
                requestedItem.DisplayOrder;
        }

        await _playlistRepository
            .SaveChangesAsync();

        await _playbackVersionService
            .IncrementForPlaylistAsync(
                playlistId);

        return true;
    }

    private static PlaylistResponse Map(
        Playlist playlist)
    {
        return new PlaylistResponse
        {
            Id = playlist.Id,

            OrganizationId =
                playlist.OrganizationId,

            Name =
                playlist.Name,

            Description =
                playlist.Description,

            Status =
                playlist.Status,

            CreatedAt =
                playlist.CreatedAt,

            UpdatedAt =
                playlist.UpdatedAt,

            Items =
                playlist.Items
                    .OrderBy(x => x.DisplayOrder)
                    .Select(x =>
                        new PlaylistItemResponse
                        {
                            Id = x.Id,

                            MediaId =
                                x.MediaId,

                            MediaName =
                                x.Media.Name,

                            FileUrl =
                                x.Media.FileUrl,

                            ContentType =
                                x.Media.ContentType,

                            DisplayOrder =
                                x.DisplayOrder,

                            DurationSeconds =
                                x.DurationSeconds
                        })
                    .ToList()
        };
    }
}