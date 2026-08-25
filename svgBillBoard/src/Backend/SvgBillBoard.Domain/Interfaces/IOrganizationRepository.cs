using SvgBillBoard.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SvgBillBoard.Domain.Interfaces
{
    public interface IOrganizationRepository
    {
        Task<Organization?> GetByIdAsync(Guid id);

        Task<List<Organization>> GetAllAsync();

        Task<bool> ExistsByCodeAsync(string code);

        Task AddAsync(Organization organization);

        Task UpdateAsync(Organization organization);

        Task SaveChangesAsync();
    }
}
