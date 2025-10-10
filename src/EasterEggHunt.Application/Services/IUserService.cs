using EasterEggHunt.Domain.Entities;

namespace EasterEggHunt.Application.Services;

/// <summary>
/// Service Interface für User-Operationen
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Ruft einen Benutzer anhand der ID ab
    /// </summary>
    /// <param name="id">Benutzer-ID</param>
    /// <returns>Benutzer oder null wenn nicht gefunden</returns>
    Task<User?> GetUserByIdAsync(int id);

    /// <summary>
    /// Erstellt einen neuen Benutzer
    /// </summary>
    /// <param name="name">Benutzername</param>
    /// <returns>Erstellter Benutzer</returns>
    Task<User> CreateUserAsync(string name);

    /// <summary>
    /// Aktualisiert den letzten Besuch eines Benutzers
    /// </summary>
    /// <param name="id">Benutzer-ID</param>
    /// <returns>True wenn erfolgreich aktualisiert</returns>
    Task<bool> UpdateUserLastSeenAsync(int id);

    /// <summary>
    /// Ruft alle aktiven Benutzer ab
    /// </summary>
    /// <returns>Liste aller aktiven Benutzer</returns>
    Task<IEnumerable<User>> GetActiveUsersAsync();

    /// <summary>
    /// Deaktiviert einen Benutzer
    /// </summary>
    /// <param name="id">Benutzer-ID</param>
    /// <returns>True wenn erfolgreich deaktiviert</returns>
    Task<bool> DeactivateUserAsync(int id);

    /// <summary>
    /// Prüft, ob ein Benutzername bereits existiert
    /// </summary>
    /// <param name="name">Benutzername zum Prüfen</param>
    /// <returns>True wenn der Name bereits existiert</returns>
    Task<bool> UserNameExistsAsync(string name);
}
