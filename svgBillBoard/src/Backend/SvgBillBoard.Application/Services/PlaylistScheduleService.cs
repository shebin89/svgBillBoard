using SvgBillBoard.Application.Abstractions.Services;
using SvgBillBoard.Application.DTOs.PlaylistSchedules;
using SvgBillBoard.Domain.Entities;
using SvgBillBoard.Domain.Interfaces;

namespace SvgBillBoard.Application.Services;

public class PlaylistScheduleService
    : IPlaylistScheduleService
{
    private readonly IPlaylistScheduleRepository _scheduleRepository;
    private readonly IPlaylistRepository _playlistRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly IPlaybackVersionService _playbackVersionService;

    public PlaylistScheduleService(
        IPlaylistScheduleRepository scheduleRepository,
        IPlaylistRepository playlistRepository,
        ILocationRepository locationRepository,
        IPlaybackVersionService playbackVersionService)
    {
        _scheduleRepository = scheduleRepository;
        _playlistRepository = playlistRepository;
        _locationRepository = locationRepository;
        _playbackVersionService = playbackVersionService;
    }

    public async Task<PlaylistScheduleResponse> CreateAsync(
        Guid organizationId,
        CreatePlaylistScheduleRequest request)
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
            request.EndDate.Value < request.StartDate.Value)
        {
            throw new ArgumentException(
                "End date cannot be earlier than start date.");
        }

        if (request.StartTime.HasValue &&
            request.EndTime.HasValue &&
            request.EndTime.Value <= request.StartTime.Value)
        {
            throw new ArgumentException(
                "End time must be later than start time.");
        }

        if (request.DaysOfWeek < 1 ||
            request.DaysOfWeek > 127)
        {
            throw new ArgumentException(
                "DaysOfWeek must be between 1 and 127.");
        }

        if (request.Priority < 0)
        {
            throw new ArgumentException(
                "Priority cannot be negative.");
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

        var schedule = new PlaylistSchedule
        {
            Id = Guid.NewGuid(),

            OrganizationId = organizationId,

            PlaylistId = request.PlaylistId,

            LocationId = request.LocationId,

            StartDate = request.StartDate,

            EndDate = request.EndDate,

            StartTime = request.StartTime,

            EndTime = request.EndTime,

            DaysOfWeek = request.DaysOfWeek,

            Priority = request.Priority,

            Status = 1,

            CreatedAt = DateTime.UtcNow,

            UpdatedAt = DateTime.UtcNow
        };

        await _scheduleRepository.AddAsync(schedule);

        await _scheduleRepository.SaveChangesAsync();

        await _playbackVersionService.IncrementForLocationAsync(schedule.LocationId);
        
        return Map(
            schedule,
            playlist.Name,
            location.Name);
    }

    public async Task<List<PlaylistScheduleResponse>>
        GetAllAsync(Guid organizationId)
    {
        var schedules =
            await _scheduleRepository
                .GetByOrganizationIdAsync(
                    organizationId);

        return schedules
            .Select(x => Map(
                x,
                x.Playlist.Name,
                x.Location.Name))
            .ToList();
    }

    public async Task<PlaylistScheduleResponse?>
        GetByIdAsync(
            Guid organizationId,
            Guid id)
    {
        var schedule =
            await _scheduleRepository
                .GetByIdAsync(id);

        if (schedule == null ||
            schedule.OrganizationId != organizationId)
        {
            return null;
        }

        return Map(
            schedule,
            schedule.Playlist.Name,
            schedule.Location.Name);
    }

    public async Task<bool> DeleteAsync(
    Guid organizationId,
    Guid id)
    {
        var schedule =
            await _scheduleRepository
                .GetByIdAsync(id);

        if (schedule == null ||
            schedule.OrganizationId != organizationId)
        {
            return false;
        }

        var locationId =
            schedule.LocationId;

        await _scheduleRepository
            .DeleteAsync(schedule);

        await _scheduleRepository
            .SaveChangesAsync();

        await _playbackVersionService
            .IncrementForLocationAsync(
                locationId);

        return true;
    }

    private static PlaylistScheduleResponse Map(
        PlaylistSchedule schedule,
        string playlistName,
        string locationName)
    {
        return new PlaylistScheduleResponse
        {
            Id = schedule.Id,

            OrganizationId =
                schedule.OrganizationId,

            PlaylistId =
                schedule.PlaylistId,

            PlaylistName =
                playlistName,

            LocationId =
                schedule.LocationId,

            LocationName =
                locationName,

            StartDate =
                schedule.StartDate,

            EndDate =
                schedule.EndDate,

            StartTime =
                schedule.StartTime,

            EndTime =
                schedule.EndTime,

            DaysOfWeek =
                schedule.DaysOfWeek,

            Priority =
                schedule.Priority,

            Status =
                schedule.Status,

            CreatedAt =
                schedule.CreatedAt,

            UpdatedAt =
                schedule.UpdatedAt
        };
    }
}