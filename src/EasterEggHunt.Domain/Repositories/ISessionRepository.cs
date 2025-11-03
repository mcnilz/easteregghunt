using EasterEggHunt.Domain.Entities;

namespace EasterEggHunt.Domain.Repositories;

/// <summary>
/// Repository Interface für Session-Entitäten
/// </summary>
public interface ISessionRepository
{
    /// <summary>
    /// Ruft alle Sessions ab
    /// </summary>
    /// <returns>Liste aller Sessions</returns>
    Task<IEnumerable<Session>> GetAllAsync();

    /// <summary>
    /// Ruft alle aktiven Sessions ab
    /// </summary>
    /// <returns>Liste aller aktiven Sessions</returns>
    Task<IEnumerable<Session>> GetActiveAsync();

    /// <summary>
    /// Ruft alle Sessions eines Benutzers ab
    /// </summary>
    /// <param name="userId">Benutzer-ID</param>
    /// <returns>Liste der Sessions des Benutzers</returns>
    Task<IEnumerable<Session>> GetByUserIdAsync(int userId);

    /// <summary>
    /// Ruft eine Session anhand der ID ab
    /// </summary>
    /// <param name="id">Session-ID</param>
    /// <returns>Session oder null wenn nicht gefunden</returns>
    Task<Session?> GetByIdAsync(string id);

    /// <summary>
    /// Ruft eine aktive Session eines Benutzers ab
    /// </summary>
    /// <param name="userId">Benutzer-ID</param>
    /// <returns>Aktive Session oder null wenn keine gefunden</returns>
    Task<Session?> GetActiveByUserIdAsync(int userId);

    /// <summary>
    /// Fügt eine neue Session hinzu
    /// </summary>
    /// <param name="session">Session zum Hinzufügen</param>
    /// <returns>Hinzugefügte Session</returns>
    Task<Session> AddAsync(Session session);

    /// <summary>
    /// Aktualisiert eine bestehende Session
    /// </summary>
    /// <param name="session">Session zum Aktualisieren</param>
    /// <returns>Aktualisierte Session</returns>
    Task<Session> UpdateAsync(Session session);

    /// <summary>
    /// Löscht eine Session
    /// </summary>
    /// <param name="id">ID der zu löschenden Session</param>
    /// <returns>True wenn erfolgreich gelöscht</returns>
    Task<bool> DeleteAsync(string id);

    /// <summary>
    /// Deaktiviert alle Sessions eines Benutzers
    /// </summary>
    /// <param name="userId">Benutzer-ID</param>
    /// <returns>Anzahl der deaktivierten Sessions</returns>
    Task<int> DeactivateAllByUserIdAsync(int userId);

    /// <summary>
    /// Löscht alle Sessions eines Benutzers (GDPR-Compliance)
    /// </summary>
    /// <param name="userId">Benutzer-ID</param>
    /// <returns>Anzahl der gelöschten Sessions</returns>
    Task<int> DeleteAllByUserIdAsync(int userId);

    /// <summary>
    /// Löscht abgelaufene Sessions
    /// </summary>
    /// <returns>Anzahl der gelöschten Sessions</returns>
    Task<int> DeleteExpiredAsync();

    /// <summary>
    /// Prüft, ob eine Session existiert
    /// </summary>
    /// <param name="id">Session-ID</param>
    /// <returns>True wenn die Session existiert</returns>
    Task<bool> ExistsAsync(string id);

    /// <summary>
    /// Speichert alle ausstehenden Änderungen
    /// </summary>
    /// <returns>Anzahl der betroffenen Datensätze</returns>
    Task<int> SaveChangesAsync();
}
