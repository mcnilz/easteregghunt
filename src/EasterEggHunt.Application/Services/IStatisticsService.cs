using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Domain.Models;

namespace EasterEggHunt.Application.Services;

/// <summary>
/// Service für komplexe Statistik-Operationen und -Aggregationen
/// </summary>
public interface IStatisticsService
{
    /// <summary>
    /// Ruft allgemeine System-Statistiken ab
    /// </summary>
    /// <returns>System-Statistiken</returns>
    Task<SystemOverviewStatistics> GetSystemOverviewAsync();

    /// <summary>
    /// Ruft detaillierte Kampagnen-Statistiken ab
    /// </summary>
    /// <param name="campaignId">Kampagnen-ID</param>
    /// <returns>Kampagnen-Statistiken</returns>
    Task<CampaignStatistics> GetCampaignStatisticsAsync(int campaignId);

    /// <summary>
    /// Ruft detaillierte Benutzer-Statistiken ab
    /// </summary>
    /// <param name="userId">Benutzer-ID</param>
    /// <returns>Benutzer-Statistiken</returns>
    Task<UserStatistics> GetUserStatisticsAsync(int userId);

    /// <summary>
    /// Ruft detaillierte QR-Code-Statistiken ab
    /// </summary>
    /// <param name="qrCodeId">QR-Code-ID</param>
    /// <returns>QR-Code-Statistiken</returns>
    Task<QrCodeStatisticsDto> GetQrCodeStatisticsAsync(int qrCodeId);

    /// <summary>
    /// Ruft Kampagnen-QR-Code-Statistiken ab
    /// </summary>
    /// <param name="campaignId">Kampagnen-ID</param>
    /// <returns>Kampagnen-QR-Code-Statistiken</returns>
    Task<CampaignQrCodeStatisticsDto> GetCampaignQrCodeStatisticsAsync(int campaignId);

    /// <summary>
    /// Ruft Top-Performer-Statistiken ab
    /// </summary>
    /// <returns>Top-Performer-Statistiken</returns>
    Task<TopPerformersStatistics> GetTopPerformersAsync();

    /// <summary>
    /// Ruft zeitbasierte Statistiken ab
    /// </summary>
    /// <param name="startDate">Startdatum (optional)</param>
    /// <param name="endDate">Enddatum (optional)</param>
    /// <returns>Zeitbasierte Statistiken gruppiert nach Tag, Woche und Monat</returns>
    Task<TimeBasedStatistics> GetTimeBasedStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Ruft Fund-Historie mit Filtern ab
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
    /// <returns>Gefilterte und paginierte Liste von Funden mit Gesamtanzahl</returns>
    Task<(IEnumerable<Find> Finds, int TotalCount)> GetFindHistoryAsync(
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
