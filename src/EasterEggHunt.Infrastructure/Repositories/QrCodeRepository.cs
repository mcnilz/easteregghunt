using Microsoft.EntityFrameworkCore;
using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Domain.Repositories;
using EasterEggHunt.Infrastructure.Data;

namespace EasterEggHunt.Infrastructure.Repositories;

/// <summary>
/// Repository Implementation für QrCode-Entitäten
/// </summary>
public class QrCodeRepository : IQrCodeRepository
{
    private readonly EasterEggHuntDbContext _context;

    /// <summary>
    /// Initialisiert eine neue Instanz der QrCodeRepository-Klasse
    /// </summary>
    /// <param name="context">DbContext für Datenbankzugriff</param>
    public QrCodeRepository(EasterEggHuntDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<QrCode>> GetAllAsync()
    {
        return await _context.QrCodes
            .Include(q => q.Campaign)
            .Include(q => q.Finds)
            .OrderBy(q => q.SortOrder)
            .ThenBy(q => q.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<QrCode>> GetByCampaignIdAsync(int campaignId)
    {
        return await _context.QrCodes
            .Where(q => q.CampaignId == campaignId)
            .Include(q => q.Campaign)
            .Include(q => q.Finds)
            .OrderBy(q => q.SortOrder)
            .ThenBy(q => q.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<QrCode>> GetActiveByCampaignIdAsync(int campaignId)
    {
        return await _context.QrCodes
            .Where(q => q.CampaignId == campaignId && q.IsActive)
            .Include(q => q.Campaign)
            .Include(q => q.Finds)
            .OrderBy(q => q.SortOrder)
            .ThenBy(q => q.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<QrCode?> GetByIdAsync(int id)
    {
        return await _context.QrCodes
            .Include(q => q.Campaign)
            .Include(q => q.Finds)
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    /// <inheritdoc />
    public async Task<QrCode?> GetByUniqueUrlAsync(Uri uniqueUrl)
    {
        ArgumentNullException.ThrowIfNull(uniqueUrl);

        return await _context.QrCodes
            .Include(q => q.Campaign)
            .Include(q => q.Finds)
            .FirstOrDefaultAsync(q => q.UniqueUrl == uniqueUrl);
    }

    /// <inheritdoc />
    public async Task<QrCode> AddAsync(QrCode qrCode)
    {
        ArgumentNullException.ThrowIfNull(qrCode);

        _context.QrCodes.Add(qrCode);
        await _context.SaveChangesAsync();
        return qrCode;
    }

    /// <inheritdoc />
    public async Task<QrCode> UpdateAsync(QrCode qrCode)
    {
        ArgumentNullException.ThrowIfNull(qrCode);

        _context.QrCodes.Update(qrCode);
        await _context.SaveChangesAsync();
        return qrCode;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id)
    {
        var qrCode = await _context.QrCodes.FindAsync(id);
        if (qrCode == null)
        {
            return false;
        }

        _context.QrCodes.Remove(qrCode);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.QrCodes.AnyAsync(q => q.Id == id);
    }

    /// <inheritdoc />
    public async Task<bool> UniqueUrlExistsAsync(Uri uniqueUrl)
    {
        ArgumentNullException.ThrowIfNull(uniqueUrl);

        return await _context.QrCodes.AnyAsync(q => q.UniqueUrl == uniqueUrl);
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
