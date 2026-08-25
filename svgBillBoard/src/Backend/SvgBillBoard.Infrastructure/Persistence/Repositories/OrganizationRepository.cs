using Microsoft.EntityFrameworkCore;
using SvgBillBoard.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SvgBillBoard.Infrastructure.Persistence.Repositories
{
    public class OrganizationRepository
    {
        private readonly ApplicationDbContext _context;

        public OrganizationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Organization?> GetByIdAsync(Guid id)
        {
            return await _context.Organizations
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Organization>> GetAllAsync()
        {
            return await _context.Organizations
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> ExistsByCodeAsync(string code)
        {
            return await _context.Organizations
                .AnyAsync(x => x.Code == code);
        }

        public async Task AddAsync(Organization organization)
        {
            await _context.Organizations.AddAsync(organization);
        }

        public Task UpdateAsync(Organization organization)
        {
            _context.Organizations.Update(organization);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
