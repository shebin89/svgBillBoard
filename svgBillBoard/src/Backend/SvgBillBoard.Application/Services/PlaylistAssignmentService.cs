using SvgBillBoard.Application.Abstractions.Services;
using SvgBillBoard.Application.DTOs.PlaylistAssignments;
using SvgBillBoard.Domain.Entities;
using SvgBillBoard.Domain.Interfaces;

namespace SvgBillBoard.Application.Services;

public class PlaylistAssignmentService
    : IPlaylistAssignmentService
{
    private readonly IPlaylistAssignmentRepository _assignmentRepository;
    private readonly IPlaylistRepository _playlistRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly IPlaybackVersionService _playbackVersionService;

    public PlaylistAssignmentService(
        IPlaylistAssignmentRepository assignmentRepository,
        IPlaylistRepository playlistRepository,
        ILocationRepository locationRepository,
        IPlaybackVersionService playbackVersionService)
    {
        _assignmentRepository = assignmentRepository;
        _playlistRepository = playlistRepository;
        _locationRepository = locationRepository;
        _playbackVersionService = playbackVersionService;
    }

    public async Task<PlaylistAssignmentResponse>
        CreateAsync(
            Guid organizationId,
            CreatePlaylistAssignmentRequest request)
    {
        if (request.PlaylistId == Guid.Empty)
        {
            throw new ArgumentException(
                "PlaylistId is required.");
        }

        if (request.LocationId == Guid.Empty)
        {
            throw new ArgumentException(
                "LocationId is required.");
        }

        if (request.StartDate.HasValue &&
            request.EndDate.HasValue &&
            request.EndDate < request.StartDate)
        {
            throw new ArgumentException(
                "End date cannot be earlier than start date.");
        }

        var playlist =
            await _playlistRepository
                .GetByIdAsync(request.PlaylistId);

        if (playlist == null ||
            playlist.OrganizationId != organizationId)
        {
            throw new InvalidOperationException(
                "Playlist was not found.");
        }

        var location =
            await _locationRepository
                .GetByIdAsync(request.LocationId);

        if (location == null ||
            location.OrganizationId != organizationId)
        {
            throw new InvalidOperationException(
                "Location was not found.");
        }

        var existing =
            await _assignmentRepository
                .GetByLocationIdAsync(
                    request.LocationId);

        if (existing != null)
        {
            throw new InvalidOperationException(
                "A playlist is already assigned to this location.");
        }

        var assignment =
            new PlaylistAssignment
            {
                Id = Guid.NewGuid(),

                OrganizationId =
                    organizationId,

                PlaylistId =
                    request.PlaylistId,

                LocationId =
                    request.LocationId,

                StartDate =
                    request.StartDate,

                EndDate =
                    request.EndDate,

                Status = 1,

                CreatedAt =
                    DateTime.UtcNow,

                UpdatedAt =
                    DateTime.UtcNow
            };

        await _assignmentRepository.AddAsync(assignment);

        await _assignmentRepository.SaveChangesAsync();

        await _playbackVersionService.IncrementForLocationAsync(assignment.LocationId);

        return Map(
            assignment,
            playlist.Name,
            location.Name);
    }

    public async Task<List<PlaylistAssignmentResponse>>
        GetAllAsync(
            Guid organizationId)
    {
        var assignments =
            await _assignmentRepository
                .GetByOrganizationIdAsync(
                    organizationId);

        return assignments
            .Select(x => Map(
                x,
                x.Playlist.Name,
                x.Location.Name))
            .ToList();
    }

    public async Task<PlaylistAssignmentResponse?>
        GetByIdAsync(
            Guid organizationId,
            Guid id)
    {
        var assignment =
            await _assignmentRepository
                .GetByIdAsync(id);

        if (assignment == null ||
            assignment.OrganizationId != organizationId)
        {
            return null;
        }

        return Map(
            assignment,
            assignment.Playlist.Name,
            assignment.Location.Name);
    }

    public async Task<bool> DeleteAsync(
        Guid organizationId,
        Guid id)
    {
        var assignment =
            await _assignmentRepository
                .GetByIdAsync(id);

        if (assignment == null ||
            assignment.OrganizationId != organizationId)
        {
            return false;
        }

        await _assignmentRepository.DeleteAsync(assignment);

        await _assignmentRepository.SaveChangesAsync();

        await _playbackVersionService.IncrementForLocationAsync(assignment.LocationId);
        return true;
    }

    private static PlaylistAssignmentResponse Map(
        PlaylistAssignment assignment,
        string playlistName,
        string locationName)
    {
        return new PlaylistAssignmentResponse
        {
            Id = assignment.Id,

            OrganizationId =
                assignment.OrganizationId,

            PlaylistId =
                assignment.PlaylistId,

            PlaylistName =
                playlistName,

            LocationId =
                assignment.LocationId,

            LocationName =
                locationName,

            StartDate =
                assignment.StartDate,

            EndDate =
                assignment.EndDate,

            Status =
                assignment.Status,

            CreatedAt =
                assignment.CreatedAt,

            UpdatedAt =
                assignment.UpdatedAt
        };
    }
}