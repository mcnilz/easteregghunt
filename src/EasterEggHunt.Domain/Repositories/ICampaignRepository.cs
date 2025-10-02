using EasterEggHunt.Domain.Entities;

namespace EasterEggHunt.Domain.Repositories;

/// <summary>
/// Repository Interface für Campaign-Entitäten
/// </summary>
public interface ICampaignRepository
{
    /// <summary>
    /// Ruft alle Kampagnen ab
    /// </summary>
    /// <returns>Liste aller Kampagnen</returns>
    Task<IEnumerable<Campaign>> GetAllAsync();

    /// <summary>
    /// Ruft alle aktiven Kampagnen ab
    /// </summary>
    /// <returns>Liste aller aktiven Kampagnen</returns>
    Task<IEnumerable<Campaign>> GetActiveAsync();

    /// <summary>
    /// Ruft eine Kampagne anhand der ID ab
    /// </summary>
    /// <param name="id">Kampagnen-ID</param>
    /// <returns>Kampagne oder null wenn nicht gefunden</returns>
    Task<Campaign?> GetByIdAsync(int id);

    /// <summary>
    /// Fügt eine neue Kampagne hinzu
    /// </summary>
    /// <param name="campaign">Kampagne zum Hinzufügen</param>
    /// <returns>Hinzugefügte Kampagne</returns>
    Task<Campaign> AddAsync(Campaign campaign);

    /// <summary>
    /// Aktualisiert eine bestehende Kampagne
    /// </summary>
    /// <param name="campaign">Kampagne zum Aktualisieren</param>
    /// <returns>Aktualisierte Kampagne</returns>
    Task<Campaign> UpdateAsync(Campaign campaign);

    /// <summary>
    /// Löscht eine Kampagne
    /// </summary>
    /// <param name="id">ID der zu löschenden Kampagne</param>
    /// <returns>True wenn erfolgreich gelöscht</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Prüft, ob eine Kampagne existiert
    /// </summary>
    /// <param name="id">Kampagnen-ID</param>
    /// <returns>True wenn die Kampagne existiert</returns>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Speichert alle ausstehenden Änderungen
    /// </summary>
    /// <returns>Anzahl der betroffenen Datensätze</returns>
    Task<int> SaveChangesAsync();
}
