using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Domain.Repositories;
using EasterEggHunt.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EasterEggHunt.Infrastructure.Repositories;

/// <summary>
/// Repository Implementation für Find-Entitäten
/// </summary>
public class FindRepository : IFindRepository
{
    private readonly EasterEggHuntDbContext _context;

    /// <summary>
    /// Initialisiert eine neue Instanz der FindRepository-Klasse
    /// </summary>
    /// <param name="context">DbContext für Datenbankzugriff</param>
    public FindRepository(EasterEggHuntDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Find>> GetAllAsync()
    {
        return await _context.Finds
            .Include(f => f.QrCode)
            .ThenInclude(q => q!.Campaign)
            .Include(f => f.User)
            .OrderByDescending(f => f.FoundAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Find>> GetByUserIdAsync(int userId)
    {
        return await _context.Finds
            .Where(f => f.UserId == userId)
            .Include(f => f.QrCode)
            .ThenInclude(q => q!.Campaign)
            .Include(f => f.User)
            .OrderByDescending(f => f.FoundAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Find>> GetByQrCodeIdAsync(int qrCodeId)
    {
        return await _context.Finds
            .Where(f => f.QrCodeId == qrCodeId)
            .Include(f => f.QrCode)
            .ThenInclude(q => q!.Campaign)
            .Include(f => f.User)
            .OrderByDescending(f => f.FoundAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Find>> GetByCampaignIdAsync(int campaignId)
    {
        return await _context.Finds
            .Where(f => f.QrCode!.CampaignId == campaignId)
            .Include(f => f.QrCode)
            .ThenInclude(q => q!.Campaign)
            .Include(f => f.User)
            .OrderByDescending(f => f.FoundAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Find?> GetByIdAsync(int id)
    {
        return await _context.Finds
            .Include(f => f.QrCode)
            .ThenInclude(q => q!.Campaign)
            .Include(f => f.User)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    /// <inheritdoc />
    public async Task<bool> UserHasFoundQrCodeAsync(int userId, int qrCodeId)
    {
        return await _context.Finds
            .AnyAsync(f => f.UserId == userId && f.QrCodeId == qrCodeId);
    }

    /// <inheritdoc />
    public async Task<Find> AddAsync(Find find)
    {
        ArgumentNullException.ThrowIfNull(find);

        _context.Finds.Add(find);
        await _context.SaveChangesAsync();
        return find;
    }

    /// <inheritdoc />
    public async Task<Find> UpdateAsync(Find find)
    {
        ArgumentNullException.ThrowIfNull(find);

        _context.Finds.Update(find);
        await _context.SaveChangesAsync();
        return find;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id)
    {
        var find = await _context.Finds.FindAsync(id);
        if (find == null)
        {
            return false;
        }

        _context.Finds.Remove(find);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Finds.AnyAsync(f => f.Id == id);
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
