using Microsoft.EntityFrameworkCore;
using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Domain.Repositories;
using EasterEggHunt.Infrastructure.Data;

namespace EasterEggHunt.Infrastructure.Repositories;

/// <summary>
/// Repository Implementation für AdminUser-Entitäten
/// </summary>
public class AdminUserRepository : IAdminUserRepository
{
    private readonly EasterEggHuntDbContext _context;

    /// <summary>
    /// Initialisiert eine neue Instanz der AdminUserRepository-Klasse
    /// </summary>
    /// <param name="context">DbContext für Datenbankzugriff</param>
    public AdminUserRepository(EasterEggHuntDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AdminUser>> GetAllAsync()
    {
        return await _context.AdminUsers
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AdminUser>> GetActiveAsync()
    {
        return await _context.AdminUsers
            .Where(a => a.IsActive)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<AdminUser?> GetByIdAsync(int id)
    {
        return await _context.AdminUsers
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    /// <inheritdoc />
    public async Task<AdminUser?> GetByUsernameAsync(string username)
    {
        ArgumentException.ThrowIfNullOrEmpty(username);

        return await _context.AdminUsers
            .FirstOrDefaultAsync(a => a.Username == username);
    }

    /// <inheritdoc />
    public async Task<AdminUser?> GetByEmailAsync(string email)
    {
        ArgumentException.ThrowIfNullOrEmpty(email);

        return await _context.AdminUsers
            .FirstOrDefaultAsync(a => a.Email == email);
    }

    /// <inheritdoc />
    public async Task<AdminUser> AddAsync(AdminUser adminUser)
    {
        ArgumentNullException.ThrowIfNull(adminUser);

        _context.AdminUsers.Add(adminUser);
        await _context.SaveChangesAsync();
        return adminUser;
    }

    /// <inheritdoc />
    public async Task<AdminUser> UpdateAsync(AdminUser adminUser)
    {
        ArgumentNullException.ThrowIfNull(adminUser);

        _context.AdminUsers.Update(adminUser);
        await _context.SaveChangesAsync();
        return adminUser;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id)
    {
        var adminUser = await _context.AdminUsers.FindAsync(id);
        if (adminUser == null)
        {
            return false;
        }

        _context.AdminUsers.Remove(adminUser);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.AdminUsers.AnyAsync(a => a.Id == id);
    }

    /// <inheritdoc />
    public async Task<bool> UsernameExistsAsync(string username)
    {
        ArgumentException.ThrowIfNullOrEmpty(username);

        return await _context.AdminUsers.AnyAsync(a => a.Username == username);
    }

    /// <inheritdoc />
    public async Task<bool> EmailExistsAsync(string email)
    {
        ArgumentException.ThrowIfNullOrEmpty(email);

        return await _context.AdminUsers.AnyAsync(a => a.Email == email);
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
