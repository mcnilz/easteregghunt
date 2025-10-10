using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace EasterEggHunt.Application.Services;

/// <summary>
/// Service für User-Operationen
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<User?> GetUserByIdAsync(int id)
    {
        _logger.LogInformation("Abrufen des Benutzers mit ID {UserId}", id);
        return await _userRepository.GetByIdAsync(id);
    }

    /// <inheritdoc />
    public async Task<User> CreateUserAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Benutzername darf nicht leer sein", nameof(name));

        // Prüfen ob Name bereits existiert
        var nameExists = await _userRepository.NameExistsAsync(name);
        if (nameExists)
        {
            _logger.LogWarning("Benutzername bereits vergeben: {UserName}", name);
            throw new InvalidOperationException($"Benutzername '{name}' ist bereits vergeben");
        }

        _logger.LogInformation("Erstellen eines neuen Benutzers: {UserName}", name);

        var user = new User(name);
        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("Benutzer erfolgreich erstellt mit ID {UserId}", user.Id);
        return user;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateUserLastSeenAsync(int id)
    {
        _logger.LogInformation("Aktualisieren des letzten Besuchs für Benutzer {UserId}", id);

        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            _logger.LogWarning("Benutzer mit ID {UserId} nicht gefunden", id);
            return false;
        }

        user.UpdateLastSeen();
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("Letzter Besuch für Benutzer {UserId} erfolgreich aktualisiert", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        _logger.LogInformation("Abrufen aller aktiven Benutzer");
        return await _userRepository.GetActiveAsync();
    }

    /// <inheritdoc />
    public async Task<bool> DeactivateUserAsync(int id)
    {
        _logger.LogInformation("Deaktivieren des Benutzers mit ID {UserId}", id);

        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            _logger.LogWarning("Benutzer mit ID {UserId} nicht gefunden", id);
            return false;
        }

        user.Deactivate();
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("Benutzer mit ID {UserId} erfolgreich deaktiviert", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> UserNameExistsAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Benutzername darf nicht leer sein", nameof(name));

        _logger.LogInformation("Prüfen ob Benutzername existiert: {UserName}", name);
        return await _userRepository.NameExistsAsync(name);
    }
}
