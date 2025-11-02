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

    /// <summary>
    /// Lädt zeitbasierte Statistiken
    /// </summary>
    /// <param name="startDate">Startdatum (optional)</param>
    /// <param name="endDate">Enddatum (optional)</param>
    /// <returns>Zeitbasierte Statistiken</returns>
    Task<Models.TimeBasedStatisticsViewModel> GetTimeBasedStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Lädt System-Statistiken für die Statistiken-Übersichtsseite
    /// </summary>
    /// <returns>System-Statistiken mit Top Found QR-Codes und Unfound QR-Codes</returns>
    Task<StatisticsViewModel> GetStatisticsAsync();

    /// <summary>
    /// Lädt Fund-Historie mit Filtern
    /// </summary>
    /// <param name="startDate">Startdatum (optional)</param>
    /// <param name="endDate">Enddatum (optional)</param>
    /// <param name="userId">Benutzer-ID (optional)</param>
    /// <param name="qrCodeId">QR-Code-ID (optional)</param>
    /// <param name="campaignId">Kampagnen-ID (optional)</param>
    /// <param name="skip">Anzahl zu überspringender Einträge (optional, Standard: 0)</param>
    /// <param name="take">Anzahl abzurufender Einträge (optional, Standard: 50)</param>
    /// <param name="sortBy">Sortierungsfeld (optional, Standard: "FoundAt")</param>
    /// <param name="sortDirection">Sortierungsrichtung (optional, Standard: "desc")</param>
    /// <returns>Fund-Historie ViewModel</returns>
    Task<FindHistoryViewModel> GetFindHistoryAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? userId = null,
        int? qrCodeId = null,
        int? campaignId = null,
        int skip = 0,
        int take = 50,
        string sortBy = "FoundAt",
        string sortDirection = "desc");
}
