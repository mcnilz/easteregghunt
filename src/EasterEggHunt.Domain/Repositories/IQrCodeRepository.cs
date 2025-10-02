using EasterEggHunt.Domain.Entities;

namespace EasterEggHunt.Domain.Repositories;

/// <summary>
/// Repository Interface für QrCode-Entitäten
/// </summary>
public interface IQrCodeRepository
{
    /// <summary>
    /// Ruft alle QR-Codes ab
    /// </summary>
    /// <returns>Liste aller QR-Codes</returns>
    Task<IEnumerable<QrCode>> GetAllAsync();

    /// <summary>
    /// Ruft alle QR-Codes einer Kampagne ab
    /// </summary>
    /// <param name="campaignId">Kampagnen-ID</param>
    /// <returns>Liste der QR-Codes der Kampagne</returns>
    Task<IEnumerable<QrCode>> GetByCampaignIdAsync(int campaignId);

    /// <summary>
    /// Ruft alle aktiven QR-Codes einer Kampagne ab
    /// </summary>
    /// <param name="campaignId">Kampagnen-ID</param>
    /// <returns>Liste der aktiven QR-Codes der Kampagne</returns>
    Task<IEnumerable<QrCode>> GetActiveByCampaignIdAsync(int campaignId);

    /// <summary>
    /// Ruft einen QR-Code anhand der ID ab
    /// </summary>
    /// <param name="id">QR-Code-ID</param>
    /// <returns>QR-Code oder null wenn nicht gefunden</returns>
    Task<QrCode?> GetByIdAsync(int id);

    /// <summary>
    /// Ruft einen QR-Code anhand der Unique URL ab
    /// </summary>
    /// <param name="uniqueUrl">Unique URL des QR-Codes</param>
    /// <returns>QR-Code oder null wenn nicht gefunden</returns>
    Task<QrCode?> GetByUniqueUrlAsync(Uri uniqueUrl);

    /// <summary>
    /// Fügt einen neuen QR-Code hinzu
    /// </summary>
    /// <param name="qrCode">QR-Code zum Hinzufügen</param>
    /// <returns>Hinzugefügter QR-Code</returns>
    Task<QrCode> AddAsync(QrCode qrCode);

    /// <summary>
    /// Aktualisiert einen bestehenden QR-Code
    /// </summary>
    /// <param name="qrCode">QR-Code zum Aktualisieren</param>
    /// <returns>Aktualisierter QR-Code</returns>
    Task<QrCode> UpdateAsync(QrCode qrCode);

    /// <summary>
    /// Löscht einen QR-Code
    /// </summary>
    /// <param name="id">ID des zu löschenden QR-Codes</param>
    /// <returns>True wenn erfolgreich gelöscht</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Prüft, ob ein QR-Code existiert
    /// </summary>
    /// <param name="id">QR-Code-ID</param>
    /// <returns>True wenn der QR-Code existiert</returns>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Prüft, ob eine Unique URL bereits verwendet wird
    /// </summary>
    /// <param name="uniqueUrl">Unique URL zum Prüfen</param>
    /// <returns>True wenn die URL bereits verwendet wird</returns>
    Task<bool> UniqueUrlExistsAsync(Uri uniqueUrl);

    /// <summary>
    /// Speichert alle ausstehenden Änderungen
    /// </summary>
    /// <returns>Anzahl der betroffenen Datensätze</returns>
    Task<int> SaveChangesAsync();
}
