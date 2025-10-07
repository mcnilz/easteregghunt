using EasterEggHunt.Domain.Entities;

namespace EasterEggHunt.Domain.Repositories;

/// <summary>
/// Repository Interface für AdminUser-Entitäten
/// </summary>
public interface IAdminUserRepository
{
    /// <summary>
    /// Ruft alle Admin-Benutzer ab
    /// </summary>
    /// <returns>Liste aller Admin-Benutzer</returns>
    Task<IEnumerable<AdminUser>> GetAllAsync();

    /// <summary>
    /// Ruft alle aktiven Admin-Benutzer ab
    /// </summary>
    /// <returns>Liste aller aktiven Admin-Benutzer</returns>
    Task<IEnumerable<AdminUser>> GetActiveAsync();

    /// <summary>
    /// Ruft einen Admin-Benutzer anhand der ID ab
    /// </summary>
    /// <param name="id">Admin-Benutzer-ID</param>
    /// <returns>Admin-Benutzer oder null wenn nicht gefunden</returns>
    Task<AdminUser?> GetByIdAsync(int id);

    /// <summary>
    /// Ruft einen Admin-Benutzer anhand des Benutzernamens ab
    /// </summary>
    /// <param name="username">Benutzername</param>
    /// <returns>Admin-Benutzer oder null wenn nicht gefunden</returns>
    Task<AdminUser?> GetByUsernameAsync(string username);

    /// <summary>
    /// Ruft einen Admin-Benutzer anhand der E-Mail-Adresse ab
    /// </summary>
    /// <param name="email">E-Mail-Adresse</param>
    /// <returns>Admin-Benutzer oder null wenn nicht gefunden</returns>
    Task<AdminUser?> GetByEmailAsync(string email);

    /// <summary>
    /// Fügt einen neuen Admin-Benutzer hinzu
    /// </summary>
    /// <param name="adminUser">Admin-Benutzer zum Hinzufügen</param>
    /// <returns>Hinzugefügter Admin-Benutzer</returns>
    Task<AdminUser> AddAsync(AdminUser adminUser);

    /// <summary>
    /// Aktualisiert einen bestehenden Admin-Benutzer
    /// </summary>
    /// <param name="adminUser">Admin-Benutzer zum Aktualisieren</param>
    /// <returns>Aktualisierter Admin-Benutzer</returns>
    Task<AdminUser> UpdateAsync(AdminUser adminUser);

    /// <summary>
    /// Löscht einen Admin-Benutzer
    /// </summary>
    /// <param name="id">ID des zu löschenden Admin-Benutzers</param>
    /// <returns>True wenn erfolgreich gelöscht</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Prüft, ob ein Admin-Benutzer existiert
    /// </summary>
    /// <param name="id">Admin-Benutzer-ID</param>
    /// <returns>True wenn der Admin-Benutzer existiert</returns>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Prüft, ob ein Benutzername bereits verwendet wird
    /// </summary>
    /// <param name="username">Benutzername zum Prüfen</param>
    /// <returns>True wenn der Benutzername bereits verwendet wird</returns>
    Task<bool> UsernameExistsAsync(string username);

    /// <summary>
    /// Prüft, ob eine E-Mail-Adresse bereits verwendet wird
    /// </summary>
    /// <param name="email">E-Mail-Adresse zum Prüfen</param>
    /// <returns>True wenn die E-Mail-Adresse bereits verwendet wird</returns>
    Task<bool> EmailExistsAsync(string email);

    /// <summary>
    /// Speichert alle ausstehenden Änderungen
    /// </summary>
    /// <returns>Anzahl der betroffenen Datensätze</returns>
    Task<int> SaveChangesAsync();

    /// <summary>
    /// Speichert einen Admin-Benutzer (Add oder Update)
    /// </summary>
    /// <param name="adminUser">Admin-Benutzer zum Speichern</param>
    /// <returns>Gespeicherter Admin-Benutzer</returns>
    Task<AdminUser> SaveAsync(AdminUser adminUser);
}
