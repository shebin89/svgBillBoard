using SvgBillBoard.Application.DTOs.PlaylistAssignments;

namespace SvgBillBoard.Application.Abstractions.Services;

public interface IPlaylistAssignmentService
{
    Task<PlaylistAssignmentResponse> CreateAsync(
        Guid organizationId,
        CreatePlaylistAssignmentRequest request);

    Task<List<PlaylistAssignmentResponse>> GetAllAsync(
        Guid organizationId);

    Task<PlaylistAssignmentResponse?> GetByIdAsync(
        Guid organizationId,
        Guid id);

    Task<bool> DeleteAsync(
        Guid organizationId,
        Guid id);
}