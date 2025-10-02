using EasterEggHunt.Domain.Entities;

namespace EasterEggHunt.Domain.Repositories;

/// <summary>
/// Repository Interface für Find-Entitäten
/// </summary>
public interface IFindRepository
{
    /// <summary>
    /// Ruft alle Funde ab
    /// </summary>
    /// <returns>Liste aller Funde</returns>
    Task<IEnumerable<Find>> GetAllAsync();

    /// <summary>
    /// Ruft alle Funde eines Benutzers ab
    /// </summary>
    /// <param name="userId">Benutzer-ID</param>
    /// <returns>Liste der Funde des Benutzers</returns>
    Task<IEnumerable<Find>> GetByUserIdAsync(int userId);

    /// <summary>
    /// Ruft alle Funde eines QR-Codes ab
    /// </summary>
    /// <param name="qrCodeId">QR-Code-ID</param>
    /// <returns>Liste der Funde des QR-Codes</returns>
    Task<IEnumerable<Find>> GetByQrCodeIdAsync(int qrCodeId);

    /// <summary>
    /// Ruft alle Funde einer Kampagne ab
    /// </summary>
    /// <param name="campaignId">Kampagnen-ID</param>
    /// <returns>Liste der Funde der Kampagne</returns>
    Task<IEnumerable<Find>> GetByCampaignIdAsync(int campaignId);

    /// <summary>
    /// Ruft einen Fund anhand der ID ab
    /// </summary>
    /// <param name="id">Fund-ID</param>
    /// <returns>Fund oder null wenn nicht gefunden</returns>
    Task<Find?> GetByIdAsync(int id);

    /// <summary>
    /// Prüft, ob ein Benutzer einen bestimmten QR-Code bereits gefunden hat
    /// </summary>
    /// <param name="userId">Benutzer-ID</param>
    /// <param name="qrCodeId">QR-Code-ID</param>
    /// <returns>True wenn der Benutzer den QR-Code bereits gefunden hat</returns>
    Task<bool> UserHasFoundQrCodeAsync(int userId, int qrCodeId);

    /// <summary>
    /// Fügt einen neuen Fund hinzu
    /// </summary>
    /// <param name="find">Fund zum Hinzufügen</param>
    /// <returns>Hinzugefügter Fund</returns>
    Task<Find> AddAsync(Find find);

    /// <summary>
    /// Aktualisiert einen bestehenden Fund
    /// </summary>
    /// <param name="find">Fund zum Aktualisieren</param>
    /// <returns>Aktualisierter Fund</returns>
    Task<Find> UpdateAsync(Find find);

    /// <summary>
    /// Löscht einen Fund
    /// </summary>
    /// <param name="id">ID des zu löschenden Funds</param>
    /// <returns>True wenn erfolgreich gelöscht</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Prüft, ob ein Fund existiert
    /// </summary>
    /// <param name="id">Fund-ID</param>
    /// <returns>True wenn der Fund existiert</returns>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Speichert alle ausstehenden Änderungen
    /// </summary>
    /// <returns>Anzahl der betroffenen Datensätze</returns>
    Task<int> SaveChangesAsync();
}
