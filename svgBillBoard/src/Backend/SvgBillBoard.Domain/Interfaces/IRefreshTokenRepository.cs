using SvgBillBoard.Domain.Entities;

namespace SvgBillBoard.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(
        string tokenHash);

    Task AddAsync(RefreshToken refreshToken);

    Task SaveChangesAsync();
}