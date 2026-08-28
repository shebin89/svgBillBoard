using SvgBillBoard.Domain.Entities;

namespace SvgBillBoard.Domain.Interfaces;

public interface IPlaylistAssignmentRepository
{
    Task<PlaylistAssignment?> GetByIdAsync(Guid id);

    Task<List<PlaylistAssignment>>GetByOrganizationIdAsync(Guid organizationId);

    Task<PlaylistAssignment?> GetByLocationIdAsync(Guid locationId);

    Task AddAsync(PlaylistAssignment assignment);

    Task DeleteAsync(PlaylistAssignment assignment);

    Task SaveChangesAsync();

    Task<List<PlaylistAssignment>> GetByPlaylistIdAsync(Guid playlistId);
}