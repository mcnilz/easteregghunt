using EasterEggHunt.Web.Models;

namespace EasterEggHunt.Web.Services;

/// <summary>
/// Service für Statistik-Anzeige im Admin-Bereich
/// </summary>
public interface IStatisticsDisplayService
{
    /// <summary>
    /// Lädt Kampagnen-Statistiken
    /// </summary>
    /// <param name="campaignId">Kampagnen-ID</param>
    /// <returns>Kampagnen-Statistiken</returns>
    Task<CampaignQrCodeStatisticsViewModel> GetCampaignStatisticsAsync(int campaignId);

    /// <summary>
    /// Lädt QR-Code-Statistiken
    /// </summary>
    /// <param name="qrCodeId">QR-Code-ID</param>
    /// <returns>QR-Code-Statistiken</returns>
    Task<QrCodeStatisticsViewModel> GetQrCodeStatisticsAsync(int qrCodeId);

    /// <summary>
    /// Erstellt Dashboard-Statistiken
    /// </summary>
    /// <returns>Dashboard-Statistiken</returns>
    Task<AdminDashboardViewModel> BuildDashboardStatisticsAsync();

    /// <summary>
    /// Lädt System-Übersichtsstatistiken
    /// </summary>
    /// <returns>System-Statistiken</returns>
    Task<Models.SystemOverviewStatistics> GetSystemOverviewAsync();

    /// <summary>
    /// Lädt Top-Performer-Statistiken
    /// </summary>
    /// <returns>Top-Performer-Statistiken</returns>
    Task<Models.TopPerformersStatisticsViewModel> GetTopPerformersAsync();
}
