using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Domain.Models;
using Microsoft.Extensions.Logging;

namespace EasterEggHunt.Application.Services;

/// <summary>
/// Service für komplexe Statistik-Operationen und -Aggregationen
/// </summary>
public class StatisticsService : IStatisticsService
{
    private readonly ICampaignService _campaignService;
    private readonly IQrCodeService _qrCodeService;
    private readonly IUserService _userService;
    private readonly IFindService _findService;
    private readonly ILogger<StatisticsService> _logger;

    public StatisticsService(
        ICampaignService campaignService,
        IQrCodeService qrCodeService,
        IUserService userService,
        IFindService findService,
        ILogger<StatisticsService> logger)
    {
        _campaignService = campaignService ?? throw new ArgumentNullException(nameof(campaignService));
        _qrCodeService = qrCodeService ?? throw new ArgumentNullException(nameof(qrCodeService));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _findService = findService ?? throw new ArgumentNullException(nameof(findService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SystemOverviewStatistics> GetSystemOverviewAsync()
    {
        _logger.LogInformation("Abrufen der System-Übersichtsstatistiken");

        var campaigns = await _campaignService.GetActiveCampaignsAsync();
        var users = await _userService.GetActiveUsersAsync();

        var totalQrCodes = 0;
        var totalFinds = 0;

        foreach (var campaign in campaigns)
        {
            var qrCodes = await _qrCodeService.GetQrCodesByCampaignIdAsync(campaign.Id);
            totalQrCodes += qrCodes.Count();

            foreach (var qrCode in qrCodes)
            {
                var finds = await _findService.GetFindsByQrCodeIdAsync(qrCode.Id);
                totalFinds += finds.Count();
            }
        }

        return new SystemOverviewStatistics
        {
            TotalCampaigns = campaigns.Count(),
            TotalUsers = users.Count(),
            TotalQrCodes = totalQrCodes,
            TotalFinds = totalFinds,
            ActiveCampaigns = campaigns.Count(c => c.IsActive),
            CompletedFinds = totalFinds
        };
    }

    public async Task<CampaignStatistics> GetCampaignStatisticsAsync(int campaignId)
    {
        _logger.LogInformation("Abrufen der Kampagnen-Statistiken für Kampagne {CampaignId}", campaignId);

        var campaign = await _campaignService.GetCampaignByIdAsync(campaignId);
        if (campaign == null)
        {
            throw new ArgumentException($"Kampagne mit ID {campaignId} nicht gefunden", nameof(campaignId));
        }

        var qrCodes = await _qrCodeService.GetQrCodesByCampaignIdAsync(campaignId);
        var totalFinds = 0;
        var uniqueFinders = new HashSet<int>();

        foreach (var qrCode in qrCodes)
        {
            var finds = await _findService.GetFindsByQrCodeIdAsync(qrCode.Id);
            totalFinds += finds.Count();

            foreach (var find in finds)
            {
                uniqueFinders.Add(find.UserId);
            }
        }

        return new CampaignStatistics
        {
            CampaignId = campaignId,
            CampaignName = campaign.Name,
            TotalQrCodes = qrCodes.Count(),
            TotalFinds = totalFinds,
            UniqueFinders = uniqueFinders.Count,
            CompletionRate = qrCodes.Any() ? (double)totalFinds / qrCodes.Count() : 0
        };
    }

    public async Task<UserStatistics> GetUserStatisticsAsync(int userId)
    {
        _logger.LogInformation("Abrufen der Benutzer-Statistiken für Benutzer {UserId}", userId);

        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
        {
            throw new ArgumentException($"Benutzer mit ID {userId} nicht gefunden", nameof(userId));
        }

        var finds = await _findService.GetFindsByUserIdAsync(userId);
        var uniqueQrCodes = finds.Select(f => f.QrCodeId).Distinct().Count();

        return new UserStatistics
        {
            UserId = userId,
            UserName = user.Name,
            TotalFinds = finds.Count(),
            UniqueQrCodesFound = uniqueQrCodes,
            FirstFindDate = finds.Any() ? finds.Min(f => f.FoundAt) : null,
            LastFindDate = finds.Any() ? finds.Max(f => f.FoundAt) : null
        };
    }

    public async Task<QrCodeStatisticsDto> GetQrCodeStatisticsAsync(int qrCodeId)
    {
        _logger.LogInformation("Abrufen der QR-Code-Statistiken für QR-Code {QrCodeId}", qrCodeId);

        var qrCode = await _qrCodeService.GetQrCodeByIdAsync(qrCodeId);
        if (qrCode == null)
        {
            throw new ArgumentException($"QR-Code mit ID {qrCodeId} nicht gefunden", nameof(qrCodeId));
        }

        var finds = await _findService.GetFindsByQrCodeIdAsync(qrCodeId);
        var uniqueFinders = finds.Select(f => f.UserId).Distinct().Count();

        return new QrCodeStatisticsDto
        {
            QrCodeId = qrCodeId,
            QrCodeTitle = qrCode.Title,
            TotalFinds = finds.Count(),
            UniqueFinders = uniqueFinders,
            FirstFindDate = finds.Any() ? finds.Min(f => f.FoundAt) : null,
            LastFindDate = finds.Any() ? finds.Max(f => f.FoundAt) : null,
            RecentFinds = finds.OrderByDescending(f => f.FoundAt).Take(10).Select(f => new QrCodeStatisticsRecentFind
            {
                UserId = f.UserId,
                UserName = f.User?.Name ?? "Unbekannt",
                FoundAt = f.FoundAt,
                IpAddress = f.IpAddress
            }).ToList()
        };
    }

    public async Task<CampaignQrCodeStatisticsDto> GetCampaignQrCodeStatisticsAsync(int campaignId)
    {
        _logger.LogInformation("Abrufen der Kampagnen-QR-Code-Statistiken für Kampagne {CampaignId}", campaignId);

        var campaign = await _campaignService.GetCampaignByIdAsync(campaignId);
        if (campaign == null)
        {
            throw new ArgumentException($"Kampagne mit ID {campaignId} nicht gefunden", nameof(campaignId));
        }

        var qrCodes = await _qrCodeService.GetQrCodesByCampaignIdAsync(campaignId);
        var qrCodeStatistics = new List<QrCodeStatisticsDto>();

        foreach (var qrCode in qrCodes)
        {
            var finds = await _findService.GetFindsByQrCodeIdAsync(qrCode.Id);
            var uniqueFinders = finds.Select(f => f.UserId).Distinct().Count();

            qrCodeStatistics.Add(new QrCodeStatisticsDto
            {
                QrCodeId = qrCode.Id,
                QrCodeTitle = qrCode.Title,
                TotalFinds = finds.Count(),
                UniqueFinders = uniqueFinders,
                FirstFindDate = finds.Any() ? finds.Min(f => f.FoundAt) : null,
                LastFindDate = finds.Any() ? finds.Max(f => f.FoundAt) : null,
                RecentFinds = finds.OrderByDescending(f => f.FoundAt).Take(5).Select(f => new QrCodeStatisticsRecentFind
                {
                    UserId = f.UserId,
                    UserName = f.User?.Name ?? "Unbekannt",
                    FoundAt = f.FoundAt,
                    IpAddress = f.IpAddress
                }).ToList()
            });
        }

        return new CampaignQrCodeStatisticsDto
        {
            CampaignId = campaignId,
            CampaignName = campaign.Name,
            QrCodeStatistics = qrCodeStatistics.OrderByDescending(q => q.TotalFinds).ToList()
        };
    }

    public async Task<TopPerformersStatistics> GetTopPerformersAsync()
    {
        _logger.LogInformation("Abrufen der Top-Performer-Statistiken");

        var users = await _userService.GetActiveUsersAsync();
        var userStatistics = new List<UserStatistics>();

        foreach (var user in users)
        {
            var finds = await _findService.GetFindsByUserIdAsync(user.Id);
            var uniqueQrCodes = finds.Select(f => f.QrCodeId).Distinct().Count();

            userStatistics.Add(new UserStatistics
            {
                UserId = user.Id,
                UserName = user.Name,
                TotalFinds = finds.Count(),
                UniqueQrCodesFound = uniqueQrCodes,
                FirstFindDate = finds.Any() ? finds.Min(f => f.FoundAt) : null,
                LastFindDate = finds.Any() ? finds.Max(f => f.FoundAt) : null
            });
        }

        return new TopPerformersStatistics
        {
            TopByTotalFinds = userStatistics.OrderByDescending(u => u.TotalFinds).Take(10).ToList(),
            TopByUniqueQrCodes = userStatistics.OrderByDescending(u => u.UniqueQrCodesFound).Take(10).ToList(),
            MostRecentActivity = userStatistics
                .Where(u => u.LastFindDate.HasValue)
                .OrderByDescending(u => u.LastFindDate)
                .Take(10)
                .ToList()
        };
    }

    public async Task<TimeBasedStatistics> GetTimeBasedStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        _logger.LogInformation("Abrufen der zeitbasierten Statistiken (StartDate={StartDate}, EndDate={EndDate})", startDate, endDate);

        try
        {
            var dailyStats = await _findService.GetDailyStatisticsAsync(startDate, endDate);
            var weeklyStats = await _findService.GetWeeklyStatisticsAsync(startDate, endDate);
            var monthlyStats = await _findService.GetMonthlyStatisticsAsync(startDate, endDate);

            return new TimeBasedStatistics
            {
                DailyStatistics = dailyStats.Select(s => new TimeSeriesStatistics
                {
                    Date = s.Date,
                    Count = s.Count,
                    UniqueFinders = s.UniqueFinders,
                    UniqueQrCodes = s.UniqueQrCodes
                }).ToList(),
                WeeklyStatistics = weeklyStats.Select(s => new TimeSeriesStatistics
                {
                    Date = s.WeekStart,
                    Count = s.Count,
                    UniqueFinders = s.UniqueFinders,
                    UniqueQrCodes = s.UniqueQrCodes
                }).ToList(),
                MonthlyStatistics = monthlyStats.Select(s => new TimeSeriesStatistics
                {
                    Date = s.MonthStart,
                    Count = s.Count,
                    UniqueFinders = s.UniqueFinders,
                    UniqueQrCodes = s.UniqueQrCodes
                }).ToList(),
                GeneratedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der zeitbasierten Statistiken: {Message}", ex.Message);
            _logger.LogError(ex, "Stack Trace: {StackTrace}", ex.StackTrace);
            throw new InvalidOperationException($"Fehler beim Abrufen der zeitbasierten Statistiken: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<Find> Finds, int TotalCount)> GetFindHistoryAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? userId = null,
        int? qrCodeId = null,
        int? campaignId = null,
        int skip = 0,
        int take = 50,
        string sortBy = "FoundAt",
        string sortDirection = "desc")
    {
        _logger.LogInformation("Abrufen der Fund-Historie (StartDate={StartDate}, EndDate={EndDate}, UserId={UserId}, QrCodeId={QrCodeId}, CampaignId={CampaignId}, Skip={Skip}, Take={Take}, SortBy={SortBy}, SortDirection={SortDirection})",
            startDate, endDate, userId, qrCodeId, campaignId, skip, take, sortBy, sortDirection);

        var finds = await _findService.GetFindHistoryAsync(startDate, endDate, userId, qrCodeId, campaignId, skip, take, sortBy, sortDirection);
        var totalCount = await _findService.GetFindHistoryCountAsync(startDate, endDate, userId, qrCodeId, campaignId);

        return (finds, totalCount);
    }
}
