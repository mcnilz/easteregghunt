using EasterEggHunt.Domain.Entities;
using EasterEggHunterApi.Abstractions.Models.QrCode;

namespace EasterEggHunt.Web.Services;

/// <summary>
/// Service für QR-Code-Management im Admin-Bereich
/// </summary>
public interface IQrCodeManagementService
{
    /// <summary>
    /// Lädt QR-Codes für eine Kampagne
    /// </summary>
    /// <param name="campaignId">Kampagnen-ID</param>
    /// <returns>Liste der QR-Codes</returns>
    Task<IEnumerable<QrCode>> GetQrCodesByCampaignAsync(int campaignId);

    /// <summary>
    /// Lädt einen QR-Code anhand der ID
    /// </summary>
    /// <param name="id">QR-Code-ID</param>
    /// <returns>QR-Code oder null</returns>
    Task<QrCode?> GetQrCodeByIdAsync(int id);

    /// <summary>
    /// Erstellt einen neuen QR-Code
    /// </summary>
    /// <param name="request">QR-Code-Erstellungsanfrage</param>
    /// <returns>Erstellter QR-Code</returns>
    Task<QrCode> CreateQrCodeAsync(CreateQrCodeRequest request);

    /// <summary>
    /// Aktualisiert einen bestehenden QR-Code
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request">QR-Code-Update-Anfrage</param>
    /// <returns>Task</returns>
    Task UpdateQrCodeAsync(int id, UpdateQrCodeRequest request);

    /// <summary>
    /// Löscht einen QR-Code
    /// </summary>
    /// <param name="id">QR-Code-ID</param>
    /// <returns>Task</returns>
    Task DeleteQrCodeAsync(int id);

    /// <summary>
    /// Sortiert QR-Codes neu
    /// </summary>
    /// <param name="campaignId">Kampagnen-ID</param>
    /// <param name="qrCodeIds">Sortierte Liste der QR-Code-IDs</param>
    /// <returns>Task</returns>
    Task ReorderQrCodesAsync(int campaignId, IEnumerable<int> qrCodeIds);

    /// <summary>
    /// Validiert QR-Code-Daten
    /// </summary>
    /// <param name="request">QR-Code-Anfrage</param>
    /// <returns>True wenn gültig</returns>
    Task<bool> ValidateQrCodeDataAsync(CreateQrCodeRequest request);

    /// <summary>
    /// Lädt QR-Code mit Statistiken
    /// </summary>
    /// <param name="id">QR-Code-ID</param>
    /// <returns>QR-Code mit Statistiken oder null</returns>
    Task<QrCode?> GetQrCodeWithStatisticsAsync(int id);

    /// <summary>
    /// Lädt QR-Code mit Funden
    /// </summary>
    /// <param name="id">QR-Code-ID</param>
    /// <returns>QR-Code mit Funden oder null</returns>
    Task<QrCode?> GetQrCodeWithFindsAsync(int id);
}
