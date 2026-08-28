using SvgBillBoard.Application.DTOs.PlaylistSchedules;

namespace SvgBillBoard.Application.Abstractions.Services;

public interface IPlaylistScheduleService
{
    Task<PlaylistScheduleResponse> CreateAsync(
        Guid organizationId,
        CreatePlaylistScheduleRequest request);

    Task<List<PlaylistScheduleResponse>> GetAllAsync(
        Guid organizationId);

    Task<PlaylistScheduleResponse?> GetByIdAsync(
        Guid organizationId,
        Guid id);

    Task<bool> DeleteAsync(
        Guid organizationId,
        Guid id);
}