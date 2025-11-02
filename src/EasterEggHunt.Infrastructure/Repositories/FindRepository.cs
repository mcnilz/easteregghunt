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

    /// <summary>
    /// Ruft Funde gruppiert nach Tag ab
    /// </summary>
    /// <param name="startDate">Startdatum (optional)</param>
    /// <param name="endDate">Enddatum (optional)</param>
    /// <returns>Funde gruppiert nach Tag</returns>
    public async Task<IEnumerable<(DateTime Date, int Count, int UniqueFinders, int UniqueQrCodes)>> GetDailyStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Finds.AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(f => f.FoundAt >= startDate.Value.Date);
        }

        if (endDate.HasValue)
        {
            query = query.Where(f => f.FoundAt < endDate.Value.Date.AddDays(1));
        }

        return await query
            .GroupBy(f => f.FoundAt.Date)
            .Select(g => new
            {
                Date = g.Key,
                Count = g.Count(),
                UniqueFinders = g.Select(f => f.UserId).Distinct().Count(),
                UniqueQrCodes = g.Select(f => f.QrCodeId).Distinct().Count()
            })
            .OrderBy(g => g.Date)
            .Select(g => new ValueTuple<DateTime, int, int, int>(g.Date, g.Count, g.UniqueFinders, g.UniqueQrCodes))
            .ToListAsync();
    }

    /// <summary>
    /// Ruft Funde gruppiert nach Woche ab
    /// </summary>
    /// <param name="startDate">Startdatum (optional)</param>
    /// <param name="endDate">Enddatum (optional)</param>
    /// <returns>Funde gruppiert nach Woche</returns>
    public async Task<IEnumerable<(DateTime WeekStart, int Count, int UniqueFinders, int UniqueQrCodes)>> GetWeeklyStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Finds.AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(f => f.FoundAt >= startDate.Value.Date);
        }

        if (endDate.HasValue)
        {
            query = query.Where(f => f.FoundAt < endDate.Value.Date.AddDays(1));
        }

        // Daten in Memory laden, da ISOWeek Methoden nicht von EF Core übersetzt werden können
        var finds = await query.ToListAsync();

        if (finds.Count == 0)
        {
            return new List<(DateTime WeekStart, int Count, int UniqueFinders, int UniqueQrCodes)>();
        }

        var result = new List<(DateTime WeekStart, int Count, int UniqueFinders, int UniqueQrCodes)>();

        var grouped = finds
            .GroupBy(f =>
            {
                var date = f.FoundAt;
                var year = date.Year;
                int week;
                try
                {
                    week = System.Globalization.ISOWeek.GetWeekOfYear(date);
                }
#pragma warning disable CA1031 // Do not catch general exception types - needed for defensive programming
                catch (Exception)
                {
                    // Fallback: Berechne Woche manuell falls ISOWeek fehlschlägt
                    var jan1 = new DateTime(year, 1, 1);
                    var daysOffset = (int)jan1.DayOfWeek;
                    var firstMonday = jan1.AddDays(-((daysOffset + 6) % 7));
                    var diff = (date - firstMonday).Days;
                    week = (diff / 7) + 1;
                }
#pragma warning restore CA1031

                return new { Year = year, Week = week };
            })
            .Select(g => new
            {
                Year = g.Key.Year,
                Week = g.Key.Week,
                Count = g.Count(),
                UniqueFinders = g.Select(f => f.UserId).Distinct().Count(),
                UniqueQrCodes = g.Select(f => f.QrCodeId).Distinct().Count()
            })
            .ToList();

        foreach (var g in grouped)
        {
            DateTime weekStart;
            try
            {
                weekStart = System.Globalization.ISOWeek.ToDateTime(g.Year, g.Week, DayOfWeek.Monday);
            }
            catch (ArgumentOutOfRangeException)
            {
                // Fallback für ungültige ISO-Wochen (z.B. Woche 53 in Jahren mit 52 Wochen)
                // Berechne den Montag der Woche manuell
                var jan1 = new DateTime(g.Year, 1, 1);
                var daysOffset = (int)jan1.DayOfWeek;
                var firstMonday = jan1.AddDays(-((daysOffset + 6) % 7));
                weekStart = firstMonday.AddDays((g.Week - 1) * 7);
            }

            result.Add((weekStart, g.Count, g.UniqueFinders, g.UniqueQrCodes));
        }

        return result.OrderBy(t => t.WeekStart).ToList();
    }

    /// <summary>
    /// Ruft Funde gruppiert nach Monat ab
    /// </summary>
    /// <param name="startDate">Startdatum (optional)</param>
    /// <param name="endDate">Enddatum (optional)</param>
    /// <returns>Funde gruppiert nach Monat</returns>
    public async Task<IEnumerable<(DateTime MonthStart, int Count, int UniqueFinders, int UniqueQrCodes)>> GetMonthlyStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Finds.AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(f => f.FoundAt >= startDate.Value.Date);
        }

        if (endDate.HasValue)
        {
            query = query.Where(f => f.FoundAt < endDate.Value.Date.AddDays(1));
        }

        // Daten in Memory laden, da DateTime-Konstruktor nicht von EF Core übersetzt werden kann
        var finds = await query.ToListAsync();

        if (finds.Count == 0)
        {
            return new List<(DateTime MonthStart, int Count, int UniqueFinders, int UniqueQrCodes)>();
        }

        var grouped = finds
            .GroupBy(f => new
            {
                Year = f.FoundAt.Year,
                Month = f.FoundAt.Month
            })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Count = g.Count(),
                UniqueFinders = g.Select(f => f.UserId).Distinct().Count(),
                UniqueQrCodes = g.Select(f => f.QrCodeId).Distinct().Count()
            })
            .ToList();

        return grouped
            .Select(g => new ValueTuple<DateTime, int, int, int>(
                new DateTime(g.Year, g.Month, 1), // DateTime-Konstruktor in Memory
                g.Count,
                g.UniqueFinders,
                g.UniqueQrCodes
            ))
            .OrderBy(t => t.Item1)
            .ToList();
    }
}
