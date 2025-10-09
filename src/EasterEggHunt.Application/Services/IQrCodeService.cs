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
    /// Generiert ein QR-Code Bild als Base64-String
    /// </summary>
    /// <param name="id">QR-Code-ID</param>
    /// <param name="size">Größe des QR-Codes in Pixeln (Standard: 200)</param>
    /// <returns>Base64-String des QR-Code Bildes</returns>
    Task<string?> GenerateQrCodeImageAsync(int id, int size = 200);

    /// <summary>
    /// Generiert QR-Code Bild für eine URL
    /// </summary>
    /// <param name="url">URL für den QR-Code</param>
    /// <param name="size">Größe des QR-Codes in Pixeln (Standard: 200)</param>
    /// <returns>Base64-String des QR-Code Bildes</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "URL wird als String verarbeitet")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1055:Uri return values should not be strings", Justification = "Base64-String wird zurückgegeben")]
    string GenerateQrCodeImageForUrl(string url, int size = 200);
}
