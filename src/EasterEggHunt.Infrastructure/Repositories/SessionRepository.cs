using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Domain.Repositories;
using EasterEggHunt.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EasterEggHunt.Infrastructure.Repositories;

/// <summary>
/// Repository Implementation für Session-Entitäten
/// </summary>
public class SessionRepository : ISessionRepository
{
    private readonly EasterEggHuntDbContext _context;

    /// <summary>
    /// Initialisiert eine neue Instanz der SessionRepository-Klasse
    /// </summary>
    /// <param name="context">DbContext für Datenbankzugriff</param>
    public SessionRepository(EasterEggHuntDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Session>> GetAllAsync()
    {
        return await _context.Sessions
            .Include(s => s.User)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Session>> GetActiveAsync()
    {
        return await _context.Sessions
            .Where(s => s.IsActive && s.ExpiresAt > DateTime.UtcNow)
            .Include(s => s.User)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Session>> GetByUserIdAsync(int userId)
    {
        return await _context.Sessions
            .Where(s => s.UserId == userId)
            .Include(s => s.User)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Session?> GetByIdAsync(string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        return await _context.Sessions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    /// <inheritdoc />
    public async Task<Session?> GetActiveByUserIdAsync(int userId)
    {
        return await _context.Sessions
            .Where(s => s.UserId == userId && s.IsActive && s.ExpiresAt > DateTime.UtcNow)
            .Include(s => s.User)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<Session> AddAsync(Session session)
    {
        ArgumentNullException.ThrowIfNull(session);

        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();
        return session;
    }

    /// <inheritdoc />
    public async Task<Session> UpdateAsync(Session session)
    {
        ArgumentNullException.ThrowIfNull(session);

        _context.Sessions.Update(session);
        await _context.SaveChangesAsync();
        return session;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        var session = await _context.Sessions.FindAsync(id);
        if (session == null)
        {
            return false;
        }

        _context.Sessions.Remove(session);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<int> DeactivateAllByUserIdAsync(int userId)
    {
        var sessions = await _context.Sessions
            .Where(s => s.UserId == userId && s.IsActive)
            .ToListAsync();

        foreach (var session in sessions)
        {
            session.Deactivate();
        }

        return await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<int> DeleteAllByUserIdAsync(int userId)
    {
        var sessions = await _context.Sessions
            .Where(s => s.UserId == userId)
            .ToListAsync();

        _context.Sessions.RemoveRange(sessions);
        return await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<int> DeleteExpiredAsync()
    {
        var expiredSessions = await _context.Sessions
            .Where(s => s.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync();

        _context.Sessions.RemoveRange(expiredSessions);
        return await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        return await _context.Sessions.AnyAsync(s => s.Id == id);
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
