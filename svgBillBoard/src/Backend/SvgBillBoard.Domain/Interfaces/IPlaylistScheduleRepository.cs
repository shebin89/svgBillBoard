using SvgBillBoard.Domain.Entities;

namespace SvgBillBoard.Domain.Interfaces;

public interface IPlaylistScheduleRepository
{
    Task<PlaylistSchedule?> GetByIdAsync(Guid id);

    Task<List<PlaylistSchedule>> GetByOrganizationIdAsync(
        Guid organizationId);

    Task<List<PlaylistSchedule>> GetActiveSchedulesAsync(
        Guid locationId);

    Task AddAsync(PlaylistSchedule schedule);

    Task UpdateAsync(PlaylistSchedule schedule);

    Task DeleteAsync(PlaylistSchedule schedule);

    Task SaveChangesAsync();
}