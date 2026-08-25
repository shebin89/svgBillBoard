using SvgBillBoard.Domain.Entities;

namespace SvgBillBoard.Domain.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid id);

    Task<Role?> GetByCodeAsync(
        Guid organizationId,
        string code);

    Task AddAsync(Role role);

    Task SaveChangesAsync();
}