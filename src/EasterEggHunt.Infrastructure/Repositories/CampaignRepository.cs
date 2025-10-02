using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Domain.Repositories;
using EasterEggHunt.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EasterEggHunt.Infrastructure.Repositories;

/// <summary>
/// Repository Implementation für Campaign-Entitäten
/// </summary>
public class CampaignRepository : ICampaignRepository
{
    private readonly EasterEggHuntDbContext _context;

    /// <summary>
    /// Initialisiert eine neue Instanz der CampaignRepository-Klasse
    /// </summary>
    /// <param name="context">DbContext für Datenbankzugriff</param>
    public CampaignRepository(EasterEggHuntDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Campaign>> GetAllAsync()
    {
        return await _context.Campaigns
            .Include(c => c.QrCodes)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Campaign>> GetActiveAsync()
    {
        return await _context.Campaigns
            .Where(c => c.IsActive)
            .Include(c => c.QrCodes.Where(q => q.IsActive))
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Campaign?> GetByIdAsync(int id)
    {
        return await _context.Campaigns
            .Include(c => c.QrCodes)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    /// <inheritdoc />
    public async Task<Campaign> AddAsync(Campaign campaign)
    {
        ArgumentNullException.ThrowIfNull(campaign);

        _context.Campaigns.Add(campaign);
        await _context.SaveChangesAsync();
        return campaign;
    }

    /// <inheritdoc />
    public async Task<Campaign> UpdateAsync(Campaign campaign)
    {
        ArgumentNullException.ThrowIfNull(campaign);

        _context.Campaigns.Update(campaign);
        await _context.SaveChangesAsync();
        return campaign;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id)
    {
        var campaign = await _context.Campaigns.FindAsync(id);
        if (campaign == null)
        {
            return false;
        }

        _context.Campaigns.Remove(campaign);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Campaigns.AnyAsync(c => c.Id == id);
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
