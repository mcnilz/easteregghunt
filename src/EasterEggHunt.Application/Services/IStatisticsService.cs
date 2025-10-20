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
}
