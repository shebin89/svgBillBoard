using Microsoft.EntityFrameworkCore;
using SvgBillBoard.Domain.Entities;
using SvgBillBoard.Domain.Interfaces;
using SvgBillBoard.Infrastructure.Persistence;

namespace SvgBillBoard.Infrastructure.Repositories;

public class MediaRepository : IMediaRepository
{
    private readonly ApplicationDbContext _context;

    public MediaRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Media?> GetByIdAsync(
        Guid id)
    {
        return await _context.Media
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Media>>
        GetByOrganizationIdAsync(
            Guid organizationId)
    {
        return await _context.Media
            .Where(x =>
                x.OrganizationId == organizationId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(
        Media media)
    {
        await _context.Media.AddAsync(media);
    }

    public Task UpdateAsync(
        Media media)
    {
        _context.Media.Update(media);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(
        Media media)
    {
        _context.Media.Remove(media);

        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}