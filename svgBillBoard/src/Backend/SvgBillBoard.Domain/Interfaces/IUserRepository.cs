using SvgBillBoard.Domain.Entities;

namespace SvgBillBoard.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);

    Task<User?> GetByEmailAsync(
        Guid organizationId,
        string email);

    Task<bool> ExistsByEmailAsync(
        Guid organizationId,
        string email);

    Task AddAsync(User user);

    Task SaveChangesAsync();
}