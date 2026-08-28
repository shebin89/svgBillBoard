using SvgBillBoard.Application.Abstractions.Services;
using SvgBillBoard.Application.DTOs.Devices;
using SvgBillBoard.Domain.Entities;
using SvgBillBoard.Domain.Interfaces;

namespace SvgBillBoard.Application.Services;

public class DevicePlaybackService : IDevicePlaybackService
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IPlaylistAssignmentRepository _assignmentRepository;
    private readonly IPlaylistScheduleRepository _scheduleRepository;

    public DevicePlaybackService(
        IDeviceRepository deviceRepository,
        IPlaylistAssignmentRepository assignmentRepository,
        IPlaylistScheduleRepository scheduleRepository)
    {
        _deviceRepository = deviceRepository;
        _assignmentRepository = assignmentRepository;
        _scheduleRepository = scheduleRepository;
    }

    public async Task<DevicePlaybackResponse> GetPlaybackAsync(
    Guid deviceId)
    {
        var device =
            await _deviceRepository.GetByIdAsync(deviceId);

        if (device == null)
        {
            throw new InvalidOperationException(
                "Device was not found.");
        }

        if (device.Status != 1)
        {
            throw new InvalidOperationException(
                "Device is inactive.");
        }

        var now = DateTime.UtcNow;

        var schedules =
            await _scheduleRepository
                .GetActiveSchedulesAsync(
                    device.LocationId);

        var activeSchedule =
            schedules
                .Where(x => IsScheduleActive(x, now))
                .OrderByDescending(x => x.Priority)
                .FirstOrDefault();

        if (activeSchedule != null)
        {
            return BuildResponse(
                device,
                activeSchedule.Playlist.Id,
                activeSchedule.Playlist.Name,
                activeSchedule.Playlist.Items);
        }

        // Fall back to the normal location assignment
        var assignment =
            await _assignmentRepository
                .GetByLocationIdAsync(
                    device.LocationId);

        if (assignment == null)
        {
            return EmptyResponse(device);
        }

        if (assignment.StartDate.HasValue &&
            assignment.StartDate.Value > now)
        {
            return EmptyResponse(device);
        }

        if (assignment.EndDate.HasValue &&
            assignment.EndDate.Value < now)
        {
            return EmptyResponse(device);
        }

        return BuildResponse(
            device,
            assignment.Playlist.Id,
            assignment.Playlist.Name,
            assignment.Playlist.Items);
    }

    private static bool IsScheduleActive(
    PlaylistSchedule schedule,
    DateTime now)
    {
        if (schedule.Status != 1)
        {
            return false;
        }

        if (schedule.StartDate.HasValue &&
            now.Date < schedule.StartDate.Value.Date)
        {
            return false;
        }

        if (schedule.EndDate.HasValue &&
            now.Date > schedule.EndDate.Value.Date)
        {
            return false;
        }

        var dayBit =
            1 << (int)now.DayOfWeek;

        if ((schedule.DaysOfWeek & dayBit) == 0)
        {
            return false;
        }

        var currentTime =
            now.TimeOfDay;

        if (schedule.StartTime.HasValue &&
            currentTime < schedule.StartTime.Value)
        {
            return false;
        }

        if (schedule.EndTime.HasValue &&
            currentTime >= schedule.EndTime.Value)
        {
            return false;
        }

        return true;
    }
    private static DevicePlaybackResponse BuildResponse(
    Device device,
    Guid playlistId,
    string playlistName,
    IEnumerable<PlaylistItem> items)
    {
        return new DevicePlaybackResponse
        {
            DeviceId = device.Id,

            LocationId = device.LocationId,

            DeviceName = device.Name,

            PlaybackVersion = device.PlaybackVersion,

            PlaylistId = playlistId,

            PlaylistName = playlistName,

            Items = items
        .OrderBy(x => x.DisplayOrder)
        .Select(x => new DevicePlaybackItemResponse
        {
            Id = x.Id,

            MediaId = x.MediaId,

            MediaName = x.Media.Name,

            FileUrl = x.Media.FileUrl,

            ContentType = x.Media.ContentType,

            DisplayOrder = x.DisplayOrder,

            DurationSeconds = x.DurationSeconds
        })
        .ToList()
        };
    }

    private static DevicePlaybackResponse EmptyResponse(
        Device device)
    {
        return new DevicePlaybackResponse
        {
            DeviceId = device.Id,

            LocationId = device.LocationId,

            DeviceName = device.Name,

            PlaybackVersion = device.PlaybackVersion,

            Items = new List<DevicePlaybackItemResponse>()
        };
    }
}