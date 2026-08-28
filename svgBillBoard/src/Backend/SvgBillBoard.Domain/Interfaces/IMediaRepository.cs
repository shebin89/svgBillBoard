using SvgBillBoard.Domain.Entities;

namespace SvgBillBoard.Domain.Interfaces;

public interface IMediaRepository
{
    Task<Media?> GetByIdAsync(Guid id);

    Task<List<Media>> GetByOrganizationIdAsync(
        Guid organizationId);

    Task AddAsync(Media media);

    Task UpdateAsync(Media media);

    Task DeleteAsync(Media media);

    Task SaveChangesAsync();
}