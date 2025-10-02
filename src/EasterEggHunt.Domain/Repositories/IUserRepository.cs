using EasterEggHunt.Domain.Entities;

namespace EasterEggHunt.Domain.Repositories;

/// <summary>
/// Repository Interface für User-Entitäten
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Ruft alle Benutzer ab
    /// </summary>
    /// <returns>Liste aller Benutzer</returns>
    Task<IEnumerable<User>> GetAllAsync();

    /// <summary>
    /// Ruft alle aktiven Benutzer ab
    /// </summary>
    /// <returns>Liste aller aktiven Benutzer</returns>
    Task<IEnumerable<User>> GetActiveAsync();

    /// <summary>
    /// Ruft einen Benutzer anhand der ID ab
    /// </summary>
    /// <param name="id">Benutzer-ID</param>
    /// <returns>Benutzer oder null wenn nicht gefunden</returns>
    Task<User?> GetByIdAsync(int id);

    /// <summary>
    /// Ruft einen Benutzer anhand des Namens ab
    /// </summary>
    /// <param name="name">Benutzername</param>
    /// <returns>Benutzer oder null wenn nicht gefunden</returns>
    Task<User?> GetByNameAsync(string name);

    /// <summary>
    /// Fügt einen neuen Benutzer hinzu
    /// </summary>
    /// <param name="user">Benutzer zum Hinzufügen</param>
    /// <returns>Hinzugefügter Benutzer</returns>
    Task<User> AddAsync(User user);

    /// <summary>
    /// Aktualisiert einen bestehenden Benutzer
    /// </summary>
    /// <param name="user">Benutzer zum Aktualisieren</param>
    /// <returns>Aktualisierter Benutzer</returns>
    Task<User> UpdateAsync(User user);

    /// <summary>
    /// Löscht einen Benutzer
    /// </summary>
    /// <param name="id">ID des zu löschenden Benutzers</param>
    /// <returns>True wenn erfolgreich gelöscht</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Prüft, ob ein Benutzer existiert
    /// </summary>
    /// <param name="id">Benutzer-ID</param>
    /// <returns>True wenn der Benutzer existiert</returns>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Prüft, ob ein Benutzername bereits verwendet wird
    /// </summary>
    /// <param name="name">Benutzername zum Prüfen</param>
    /// <returns>True wenn der Name bereits verwendet wird</returns>
    Task<bool> NameExistsAsync(string name);

    /// <summary>
    /// Speichert alle ausstehenden Änderungen
    /// </summary>
    /// <returns>Anzahl der betroffenen Datensätze</returns>
    Task<int> SaveChangesAsync();
}
