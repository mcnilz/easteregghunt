using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Web.Models;

namespace EasterEggHunt.Web.Services;

/// <summary>
/// Service für Statistik-Anzeige im Admin-Bereich
/// </summary>
public class StatisticsDisplayService : IStatisticsDisplayService
{
    private readonly IEasterEggHuntApiClient _apiClient;
    private readonly ILogger<StatisticsDisplayService> _logger;

    public StatisticsDisplayService(
        IEasterEggHuntApiClient apiClient,
        ILogger<StatisticsDisplayService> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Lädt Kampagnen-Statistiken
    /// </summary>
    /// <param name="campaignId">Kampagnen-ID</param>
    /// <returns>Kampagnen-Statistiken</returns>
    public async Task<CampaignQrCodeStatisticsViewModel> GetCampaignStatisticsAsync(int campaignId)
    {
        try
        {
            _logger.LogInformation("Lade Statistiken für Kampagne {CampaignId}", campaignId);
            return await _apiClient.GetCampaignQrCodeStatisticsAsync(campaignId);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der Kampagnen-Statistiken {CampaignId}", campaignId);
            throw;
        }
    }

    /// <summary>
    /// Lädt QR-Code-Statistiken
    /// </summary>
    /// <param name="qrCodeId">QR-Code-ID</param>
    /// <returns>QR-Code-Statistiken</returns>
    public async Task<QrCodeStatisticsViewModel> GetQrCodeStatisticsAsync(int qrCodeId)
    {
        try
        {
            _logger.LogInformation("Lade Statistiken für QR-Code {QrCodeId}", qrCodeId);
            return await _apiClient.GetQrCodeStatisticsAsync(qrCodeId);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der QR-Code-Statistiken {QrCodeId}", qrCodeId);
            throw;
        }
    }

    /// <summary>
    /// Erstellt Dashboard-Statistiken
    /// </summary>
    /// <returns>Dashboard-Statistiken</returns>
    public async Task<AdminDashboardViewModel> BuildDashboardStatisticsAsync()
    {
        try
        {
            _logger.LogInformation("Erstelle Dashboard-Statistiken");

            var campaigns = await _apiClient.GetActiveCampaignsAsync();
            var users = await _apiClient.GetActiveUsersAsync();

            // QR-Code Statistiken berechnen
            var allQrCodes = new List<QrCode>();
            var allFinds = new List<Find>();
            var recentActivities = new List<RecentActivityViewModel>();

            foreach (var campaign in campaigns)
            {
                var qrCodes = await _apiClient.GetQrCodesByCampaignIdAsync(campaign.Id);
                allQrCodes.AddRange(qrCodes);

                // Letzte Funde für diese Kampagne sammeln
                foreach (var qrCode in qrCodes)
                {
                    var finds = await _apiClient.GetFindsByQrCodeIdAsync(qrCode.Id);
                    allFinds.AddRange(finds);

                    // Recent Activities für die letzten 10 Funde
                    var recentFinds = finds
                        .OrderByDescending(f => f.FoundAt)
                        .Take(10)
                        .Select(f => new RecentActivityViewModel
                        {
                            UserName = f.User?.Name ?? "Unbekannter Benutzer",
                            QrCodeTitle = qrCode.Title,
                            CampaignName = campaign.Name,
                            FoundAt = f.FoundAt,
                            IpAddress = f.IpAddress
                        });

                    recentActivities.AddRange(recentFinds);
                }
            }

            // Recent Activities sortieren und auf 10 begrenzen
            recentActivities = recentActivities
                .OrderByDescending(a => a.FoundAt)
                .Take(10)
                .ToList();

            return new AdminDashboardViewModel(campaigns)
            {
                TotalUsers = users.Count(),
                ActiveCampaigns = campaigns.Count(c => c.IsActive),
                TotalQrCodes = allQrCodes.Count,
                ActiveQrCodes = allQrCodes.Count(q => q.IsActive),
                TotalFinds = allFinds.Count,
                RecentActivities = recentActivities
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Erstellen der Dashboard-Statistiken");
            throw;
        }
    }

    /// <summary>
    /// Lädt System-Übersichtsstatistiken
    /// </summary>
    /// <returns>System-Statistiken</returns>
    public async Task<Models.SystemOverviewStatistics> GetSystemOverviewAsync()
    {
        try
        {
            _logger.LogInformation("Lade System-Übersichtsstatistiken");

            var campaigns = await _apiClient.GetActiveCampaignsAsync();
            var users = await _apiClient.GetActiveUsersAsync();

            var totalQrCodes = 0;
            var totalFinds = 0;

            foreach (var campaign in campaigns)
            {
                var qrCodes = await _apiClient.GetQrCodesByCampaignIdAsync(campaign.Id);
                totalQrCodes += qrCodes.Count();

                foreach (var qrCode in qrCodes)
                {
                    var finds = await _apiClient.GetFindsByQrCodeIdAsync(qrCode.Id);
                    totalFinds += finds.Count();
                }
            }

            return new Models.SystemOverviewStatistics
            {
                TotalCampaigns = campaigns.Count(),
                ActiveCampaigns = campaigns.Count(c => c.IsActive),
                TotalUsers = users.Count(),
                TotalQrCodes = totalQrCodes,
                TotalFinds = totalFinds
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der System-Übersichtsstatistiken");
            throw;
        }
    }

    /// <summary>
    /// Erstellt Dashboard-Statistiken für Admin
    /// </summary>
    /// <returns>Dashboard-Statistiken</returns>
    public async Task<AdminDashboardViewModel> GetAdminDashboardStatisticsAsync()
    {
        return await BuildDashboardStatisticsAsync();
    }

    /// <summary>
    /// Lädt Benutzer-Statistiken
    /// </summary>
    /// <returns>Benutzer-Statistiken</returns>
    public async Task<IEnumerable<Models.UserStatistics>> GetUserStatisticsAsync()
    {
        try
        {
            _logger.LogInformation("Lade Benutzer-Statistiken");

            var users = await _apiClient.GetActiveUsersAsync();
            var userStatistics = new List<Models.UserStatistics>();

            foreach (var user in users)
            {
                var userStats = await _apiClient.GetUserStatisticsAsync(user.Id);
                userStatistics.Add(userStats);
            }

            return userStatistics;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der Benutzer-Statistiken");
            throw;
        }
    }

    /// <summary>
    /// Lädt Top-Performer-Statistiken
    /// </summary>
    /// <returns>Top-Performer-Statistiken</returns>
    public async Task<Models.TopPerformersStatisticsViewModel> GetTopPerformersAsync()
    {
        try
        {
            _logger.LogInformation("Lade Top-Performer-Statistiken");
            return await _apiClient.GetTopPerformersAsync();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der Top-Performer-Statistiken");
            throw;
        }
    }

}
