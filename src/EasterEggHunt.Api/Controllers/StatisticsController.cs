using EasterEggHunt.Api.Models;
using EasterEggHunt.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace EasterEggHunt.Api.Controllers;

/// <summary>
/// API Controller für Statistik-Operationen
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StatisticsController : ControllerBase
{
    private readonly ICampaignService _campaignService;
    private readonly IQrCodeService _qrCodeService;
    private readonly IUserService _userService;
    private readonly IFindService _findService;
    private readonly ILogger<StatisticsController> _logger;

    public StatisticsController(
        ICampaignService campaignService,
        IQrCodeService qrCodeService,
        IUserService userService,
        IFindService findService,
        ILogger<StatisticsController> logger)
    {
        _campaignService = campaignService ?? throw new ArgumentNullException(nameof(campaignService));
        _qrCodeService = qrCodeService ?? throw new ArgumentNullException(nameof(qrCodeService));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _findService = findService ?? throw new ArgumentNullException(nameof(findService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Ruft allgemeine System-Statistiken ab
    /// </summary>
    /// <returns>System-Statistiken</returns>
    [HttpGet("overview")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SystemOverviewStatistics>> GetSystemOverview()
    {
        try
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
                    var findCount = await _findService.GetFindCountByQrCodeIdAsync(qrCode.Id);
                    totalFinds += findCount;
                }
            }

            var statistics = new SystemOverviewStatistics
            {
                TotalCampaigns = campaigns.Count(),
                ActiveCampaigns = campaigns.Count(c => c.IsActive),
                TotalQrCodes = totalQrCodes,
                TotalUsers = users.Count(),
                ActiveUsers = users.Count(u => u.IsActive),
                TotalFinds = totalFinds,
                GeneratedAt = DateTime.UtcNow
            };

            return Ok(statistics);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der System-Übersichtsstatistiken");
            return StatusCode(500, "Interner Serverfehler");
        }
    }

    /// <summary>
    /// Ruft Statistiken für eine spezifische Kampagne ab
    /// </summary>
    /// <param name="campaignId">Kampagnen-ID</param>
    /// <returns>Kampagnen-Statistiken</returns>
    [HttpGet("campaign/{campaignId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CampaignStatistics>> GetCampaignStatistics(int campaignId)
    {
        try
        {
            _logger.LogInformation("Abrufen der Statistiken für Kampagne {CampaignId}", campaignId);

            var campaign = await _campaignService.GetCampaignByIdAsync(campaignId);
            if (campaign == null)
            {
                return NotFound($"Kampagne mit ID {campaignId} nicht gefunden");
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

            var statistics = new CampaignStatistics
            {
                CampaignId = campaignId,
                CampaignName = campaign.Name,
                TotalQrCodes = qrCodes.Count(),
                ActiveQrCodes = qrCodes.Count(q => q.IsActive),
                TotalFinds = totalFinds,
                UniqueFinders = uniqueFinders.Count,
                GeneratedAt = DateTime.UtcNow
            };

            return Ok(statistics);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der Kampagnen-Statistiken für {CampaignId}", campaignId);
            return StatusCode(500, "Interner Serverfehler");
        }
    }

    /// <summary>
    /// Ruft Statistiken für einen spezifischen Benutzer ab
    /// </summary>
    /// <param name="userId">Benutzer-ID</param>
    /// <returns>Benutzer-Statistiken</returns>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserStatistics>> GetUserStatistics(int userId)
    {
        try
        {
            _logger.LogInformation("Abrufen der Statistiken für Benutzer {UserId}", userId);

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Benutzer mit ID {userId} nicht gefunden");
            }

            var finds = await _findService.GetFindsByUserIdAsync(userId);
            var totalFinds = finds.Count();
            var uniqueQrCodes = finds.Select(f => f.QrCodeId).Distinct().Count();

            var statistics = new UserStatistics
            {
                UserId = userId,
                UserName = user.Name,
                TotalFinds = totalFinds,
                UniqueQrCodesFound = uniqueQrCodes,
                FirstSeen = user.FirstSeen,
                LastSeen = user.LastSeen,
                IsActive = user.IsActive,
                GeneratedAt = DateTime.UtcNow
            };

            return Ok(statistics);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der Benutzer-Statistiken für {UserId}", userId);
            return StatusCode(500, "Interner Serverfehler");
        }
    }

    /// <summary>
    /// Ruft Statistiken für einen spezifischen QR-Code ab
    /// </summary>
    /// <param name="qrCodeId">QR-Code-ID</param>
    /// <returns>QR-Code Statistiken</returns>
    [HttpGet("qrcode/{qrCodeId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<QrCodeStatisticsDto>> GetQrCodeStatistics(int qrCodeId)
    {
        try
        {
            _logger.LogInformation("Abrufen der Statistiken für QR-Code {QrCodeId}", qrCodeId);

            var qrCode = await _qrCodeService.GetQrCodeByIdAsync(qrCodeId);
            if (qrCode == null)
            {
                return NotFound($"QR-Code mit ID {qrCodeId} nicht gefunden");
            }

            var campaign = await _campaignService.GetCampaignByIdAsync(qrCode.CampaignId);
            var finds = await _findService.GetFindsByQrCodeIdAsync(qrCodeId);

            var finders = finds
                .OrderByDescending(f => f.FoundAt)
                .Select(f => new FinderInfoDto
                {
                    UserId = f.UserId,
                    UserName = f.User?.Name ?? "Unbekannter Benutzer",
                    FoundAt = f.FoundAt,
                    IpAddress = f.IpAddress
                })
                .ToList();

            var statistics = new QrCodeStatisticsDto
            {
                QrCodeId = qrCode.Id,
                Title = qrCode.Title,
                CampaignId = qrCode.CampaignId,
                CampaignName = campaign?.Name ?? "Unbekannte Kampagne",
                FindCount = finds.Count(),
                Finders = finders,
                GeneratedAt = DateTime.UtcNow
            };

            return Ok(statistics);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der QR-Code-Statistiken für {QrCodeId}", qrCodeId);
            return StatusCode(500, "Interner Serverfehler");
        }
    }

    /// <summary>
    /// Ruft Statistiken für alle QR-Codes einer Kampagne ab
    /// </summary>
    /// <param name="campaignId">Kampagnen-ID</param>
    /// <returns>Kampagnen-QR-Code Statistiken</returns>
    [HttpGet("campaign/{campaignId}/qrcodes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CampaignQrCodeStatisticsDto>> GetCampaignQrCodeStatistics(int campaignId)
    {
        try
        {
            _logger.LogInformation("Abrufen der QR-Code-Statistiken für Kampagne {CampaignId}", campaignId);

            var campaign = await _campaignService.GetCampaignByIdAsync(campaignId);
            if (campaign == null)
            {
                return NotFound($"Kampagne mit ID {campaignId} nicht gefunden");
            }

            var qrCodes = await _qrCodeService.GetQrCodesByCampaignIdAsync(campaignId);
            var qrCodeStatistics = new List<QrCodeStatisticsDto>();
            var totalFinds = 0;
            var foundQrCodes = 0;

            foreach (var qrCode in qrCodes)
            {
                var finds = await _findService.GetFindsByQrCodeIdAsync(qrCode.Id);
                var findCount = finds.Count();
                totalFinds += findCount;

                if (findCount > 0)
                {
                    foundQrCodes++;
                }

                var finders = finds
                    .OrderByDescending(f => f.FoundAt)
                    .Select(f => new FinderInfoDto
                    {
                        UserId = f.UserId,
                        UserName = f.User?.Name ?? "Unbekannter Benutzer",
                        FoundAt = f.FoundAt,
                        IpAddress = f.IpAddress
                    })
                    .ToList();

                qrCodeStatistics.Add(new QrCodeStatisticsDto
                {
                    QrCodeId = qrCode.Id,
                    Title = qrCode.Title,
                    CampaignId = qrCode.CampaignId,
                    CampaignName = campaign.Name,
                    FindCount = findCount,
                    Finders = finders,
                    GeneratedAt = DateTime.UtcNow
                });
            }

            var statistics = new CampaignQrCodeStatisticsDto
            {
                CampaignId = campaignId,
                CampaignName = campaign.Name,
                TotalQrCodes = qrCodes.Count(),
                FoundQrCodes = foundQrCodes,
                TotalFinds = totalFinds,
                QrCodeStatistics = qrCodeStatistics.OrderByDescending(q => q.FindCount).ToList(),
                GeneratedAt = DateTime.UtcNow
            };

            return Ok(statistics);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der Kampagnen-QR-Code-Statistiken für {CampaignId}", campaignId);
            return StatusCode(500, "Interner Serverfehler");
        }
    }

    /// <summary>
    /// Ruft Top-Performer Statistiken ab
    /// </summary>
    /// <returns>Top-Performer Statistiken</returns>
    [HttpGet("top-performers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TopPerformersStatistics>> GetTopPerformers()
    {
        try
        {
            _logger.LogInformation("Abrufen der Top-Performer Statistiken");

            var users = await _userService.GetActiveUsersAsync();
            var userStats = new List<UserFindCount>();

            foreach (var user in users)
            {
                var findCount = await _findService.GetFindCountByUserIdAsync(user.Id);
                userStats.Add(new UserFindCount
                {
                    UserId = user.Id,
                    UserName = user.Name,
                    FindCount = findCount
                });
            }

            var topPerformers = userStats
                .OrderByDescending(u => u.FindCount)
                .Take(10)
                .ToList();

            var statistics = new TopPerformersStatistics(topPerformers)
            {
                GeneratedAt = DateTime.UtcNow
            };

            return Ok(statistics);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der Top-Performer Statistiken");
            return StatusCode(500, "Interner Serverfehler");
        }
    }
}

/// <summary>
/// Response-Model für System-Übersichtsstatistiken
/// </summary>
public class SystemOverviewStatistics
{
    /// <summary>
    /// Gesamtanzahl der Kampagnen
    /// </summary>
    public int TotalCampaigns { get; set; }

    /// <summary>
    /// Anzahl der aktiven Kampagnen
    /// </summary>
    public int ActiveCampaigns { get; set; }

    /// <summary>
    /// Gesamtanzahl der QR-Codes
    /// </summary>
    public int TotalQrCodes { get; set; }

    /// <summary>
    /// Gesamtanzahl der Benutzer
    /// </summary>
    public int TotalUsers { get; set; }

    /// <summary>
    /// Anzahl der aktiven Benutzer
    /// </summary>
    public int ActiveUsers { get; set; }

    /// <summary>
    /// Gesamtanzahl der Funde
    /// </summary>
    public int TotalFinds { get; set; }

    /// <summary>
    /// Zeitpunkt der Generierung
    /// </summary>
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// Response-Model für Kampagnen-Statistiken
/// </summary>
public class CampaignStatistics
{
    /// <summary>
    /// Kampagnen-ID
    /// </summary>
    public int CampaignId { get; set; }

    /// <summary>
    /// Name der Kampagne
    /// </summary>
    public string CampaignName { get; set; } = string.Empty;

    /// <summary>
    /// Gesamtanzahl der QR-Codes in der Kampagne
    /// </summary>
    public int TotalQrCodes { get; set; }

    /// <summary>
    /// Anzahl der aktiven QR-Codes in der Kampagne
    /// </summary>
    public int ActiveQrCodes { get; set; }

    /// <summary>
    /// Gesamtanzahl der Funde in der Kampagne
    /// </summary>
    public int TotalFinds { get; set; }

    /// <summary>
    /// Anzahl der einzigartigen Finder
    /// </summary>
    public int UniqueFinders { get; set; }

    /// <summary>
    /// Zeitpunkt der Generierung
    /// </summary>
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// Response-Model für Benutzer-Statistiken
/// </summary>
public class UserStatistics
{
    /// <summary>
    /// Benutzer-ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Name des Benutzers
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gesamtanzahl der Funde des Benutzers
    /// </summary>
    public int TotalFinds { get; set; }

    /// <summary>
    /// Anzahl der einzigartigen QR-Codes, die der Benutzer gefunden hat
    /// </summary>
    public int UniqueQrCodesFound { get; set; }

    /// <summary>
    /// Zeitpunkt der ersten Registrierung
    /// </summary>
    public DateTime FirstSeen { get; set; }

    /// <summary>
    /// Zeitpunkt der letzten Aktivität
    /// </summary>
    public DateTime LastSeen { get; set; }

    /// <summary>
    /// Gibt an, ob der Benutzer aktiv ist
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Zeitpunkt der Generierung
    /// </summary>
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// Response-Model für Top-Performer Statistiken
/// </summary>
public class TopPerformersStatistics
{
    /// <summary>
    /// Liste der Top-Performer
    /// </summary>
    public IReadOnlyList<UserFindCount> TopPerformers { get; }

    /// <summary>
    /// Zeitpunkt der Generierung
    /// </summary>
    public DateTime GeneratedAt { get; set; }

    /// <summary>
    /// Konstruktor für TopPerformersStatistics
    /// </summary>
    /// <param name="topPerformers">Liste der Top-Performer</param>
    public TopPerformersStatistics(IEnumerable<UserFindCount> topPerformers)
    {
        TopPerformers = topPerformers.ToList().AsReadOnly();
    }
}

/// <summary>
/// Response-Model für Benutzer-Fund-Anzahl
/// </summary>
public class UserFindCount
{
    /// <summary>
    /// Benutzer-ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Name des Benutzers
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Anzahl der Funde
    /// </summary>
    public int FindCount { get; set; }
}
