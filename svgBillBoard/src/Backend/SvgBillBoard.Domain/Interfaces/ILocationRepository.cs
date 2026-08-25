using SvgBillBoard.Domain.Entities;

namespace SvgBillBoard.Domain.Interfaces;

public interface ILocationRepository
{
    Task<Location?> GetByIdAsync(Guid id);

    Task<List<Location>> GetByOrganizationIdAsync(
        Guid organizationId);

    Task<bool> ExistsByCodeAsync(
        Guid organizationId,
        string code);

    Task AddAsync(Location location);

    Task UpdateAsync(Location location);

    Task SaveChangesAsync();
}