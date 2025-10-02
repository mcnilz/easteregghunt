using Microsoft.EntityFrameworkCore;
using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Domain.Repositories;
using EasterEggHunt.Infrastructure.Data;

namespace EasterEggHunt.Infrastructure.Repositories;

/// <summary>
/// Repository Implementation für User-Entitäten
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly EasterEggHuntDbContext _context;

    /// <summary>
    /// Initialisiert eine neue Instanz der UserRepository-Klasse
    /// </summary>
    /// <param name="context">DbContext für Datenbankzugriff</param>
    public UserRepository(EasterEggHuntDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.Finds)
            .Include(u => u.Sessions)
            .OrderByDescending(u => u.FirstSeen)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<User>> GetActiveAsync()
    {
        return await _context.Users
            .Where(u => u.IsActive)
            .Include(u => u.Finds)
            .Include(u => u.Sessions)
            .OrderByDescending(u => u.FirstSeen)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .Include(u => u.Finds)
            .Include(u => u.Sessions)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    /// <inheritdoc />
    public async Task<User?> GetByNameAsync(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        return await _context.Users
            .Include(u => u.Finds)
            .Include(u => u.Sessions)
            .FirstOrDefaultAsync(u => u.Name == name);
    }

    /// <inheritdoc />
    public async Task<User> AddAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    /// <inheritdoc />
    public async Task<User> UpdateAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return false;
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Users.AnyAsync(u => u.Id == id);
    }

    /// <inheritdoc />
    public async Task<bool> NameExistsAsync(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        return await _context.Users.AnyAsync(u => u.Name == name);
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
