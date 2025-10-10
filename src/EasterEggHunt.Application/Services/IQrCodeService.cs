using EasterEggHunt.Application.Requests;
using EasterEggHunt.Domain.Entities;

namespace EasterEggHunt.Application.Services;

/// <summary>
/// Service Interface für QrCode-Operationen
/// </summary>
public interface IQrCodeService
{
    /// <summary>
    /// Ruft alle QR-Codes für eine Kampagne ab
    /// </summary>
    /// <param name="campaignId">Kampagnen-ID</param>
    /// <returns>Liste aller QR-Codes der Kampagne</returns>
    Task<IEnumerable<QrCode>> GetQrCodesByCampaignIdAsync(int campaignId);

    /// <summary>
    /// Ruft einen QR-Code anhand der ID ab
    /// </summary>
    /// <param name="id">QR-Code-ID</param>
    /// <returns>QR-Code oder null wenn nicht gefunden</returns>
    Task<QrCode?> GetQrCodeByIdAsync(int id);

    /// <summary>
    /// Erstellt einen neuen QR-Code
    /// </summary>
    /// <param name="request">QR-Code Erstellungsdaten</param>
    /// <returns>Erstellter QR-Code</returns>
    Task<QrCode> CreateQrCodeAsync(CreateQrCodeRequest request);

    /// <summary>
    /// Aktualisiert einen bestehenden QR-Code
    /// </summary>
    /// <param name="request">QR-Code Aktualisierungsdaten</param>
    /// <returns>True wenn erfolgreich aktualisiert</returns>
    Task<bool> UpdateQrCodeAsync(UpdateQrCodeRequest request);

    /// <summary>
    /// Löscht einen QR-Code
    /// </summary>
    /// <param name="id">QR-Code-ID</param>
    /// <returns>True wenn erfolgreich gelöscht</returns>
    Task<bool> DeleteQrCodeAsync(int id);

    /// <summary>
    /// Setzt die Sortierreihenfolge für einen QR-Code
    /// </summary>
    /// <param name="id">QR-Code-ID</param>
    /// <param name="sortOrder">Neue Sortierreihenfolge</param>
    /// <returns>True wenn erfolgreich aktualisiert</returns>
    Task<bool> SetQrCodeSortOrderAsync(int id, int sortOrder);

    /// <summary>
    /// Aktiviert einen QR-Code
    /// </summary>
    /// <param name="id">QR-Code-ID</param>
    /// <returns>True wenn erfolgreich aktiviert</returns>
    Task<bool> ActivateQrCodeAsync(int id);

    /// <summary>
    /// Deaktiviert einen QR-Code
    /// </summary>
    /// <param name="id">QR-Code-ID</param>
    /// <returns>True wenn erfolgreich deaktiviert</returns>
    Task<bool> DeactivateQrCodeAsync(int id);

    /// <summary>
    /// Ruft einen QR-Code anhand des Codes ab
    /// </summary>
    /// <param name="code">Code des QR-Codes</param>
    /// <returns>QR-Code oder null wenn nicht gefunden</returns>
    Task<QrCode?> GetQrCodeByCodeAsync(string code);
}
