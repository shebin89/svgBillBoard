namespace SvgBillBoard.Application.Abstractions.Services;

public interface IPlaybackVersionService
{
    Task IncrementForLocationAsync(
        Guid locationId);

    Task IncrementForPlaylistAsync(
        Guid playlistId);
}