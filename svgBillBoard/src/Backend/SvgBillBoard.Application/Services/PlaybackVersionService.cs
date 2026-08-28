using SvgBillBoard.Application.Abstractions.Services;
using SvgBillBoard.Domain.Interfaces;

public class PlaybackVersionService
    : IPlaybackVersionService
{
    private readonly IDeviceRepository _deviceRepository;

    private readonly IPlaylistAssignmentRepository
        _assignmentRepository;

    public PlaybackVersionService(
        IDeviceRepository deviceRepository,
        IPlaylistAssignmentRepository assignmentRepository)
    {
        _deviceRepository = deviceRepository;

        _assignmentRepository =
            assignmentRepository;
    }

    public async Task IncrementForLocationAsync(
        Guid locationId)
    {
        var devices =
            await _deviceRepository
                .GetByLocationIdAsync(locationId);

        foreach (var device in devices)
        {
            device.PlaybackVersion++;

            device.UpdatedAt =
                DateTime.UtcNow;

            await _deviceRepository
                .UpdateAsync(device);
        }

        await _deviceRepository
            .SaveChangesAsync();
    }

    public async Task IncrementForPlaylistAsync(
        Guid playlistId)
    {
        var assignments =
            await _assignmentRepository
                .GetByPlaylistIdAsync(playlistId);

        var locationIds =
            assignments
                .Select(x => x.LocationId)
                .Distinct()
                .ToList();

        foreach (var locationId in locationIds)
        {
            await IncrementForLocationAsync(
                locationId);
        }
    }
}