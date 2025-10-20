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

    public async Task<int> GetCountByUserIdAsync(int userId)
    {
        return await _context.Finds.CountAsync(f => f.UserId == userId);
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

    public async Task<int> GetCountByQrCodeIdAsync(int qrCodeId)
    {
        return await _context.Finds.CountAsync(f => f.QrCodeId == qrCodeId);
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

    public async Task<Find?> GetFirstByUserAndQrAsync(int userId, int qrCodeId)
    {
        return await _context.Finds
            .Where(f => f.UserId == userId && f.QrCodeId == qrCodeId)
            .OrderBy(f => f.FoundAt)
            .FirstOrDefaultAsync();
    }

    public async Task<(int totalFinds, int uniqueFinders)> GetCampaignFindsAggregateAsync(int campaignId)
    {
        var query = _context.Finds.Where(f => f.QrCode!.CampaignId == campaignId);
        var totalFinds = await query.CountAsync();
        var uniqueFinders = await query.Select(f => f.UserId).Distinct().CountAsync();
        return (totalFinds, uniqueFinders);
    }

    public async Task<int> GetUniqueQrCodesCountByUserIdAsync(int userId)
    {
        return await _context.Finds
            .Where(f => f.UserId == userId)
            .Select(f => f.QrCodeId)
            .Distinct()
            .CountAsync();
    }

    public async Task<IEnumerable<Find>> GetByUserAndCampaignAsync(int userId, int campaignId, int? take = null)
    {
        IOrderedQueryable<Find> query = _context.Finds
            .Where(f => f.UserId == userId && f.QrCode!.CampaignId == campaignId)
            .OrderByDescending(f => f.FoundAt);

        if (take.HasValue && take.Value > 0)
        {
            return await query
                .Take(take.Value)
                .Include(f => f.QrCode)
                .ThenInclude(q => q!.Campaign)
                .Include(f => f.User)
                .ToListAsync();
        }

        return await query
            .Include(f => f.QrCode)
            .ThenInclude(q => q!.Campaign)
            .Include(f => f.User)
            .ToListAsync();
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
