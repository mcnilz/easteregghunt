using EasterEggHunt.Application.Services;
using EasterEggHunt.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace EasterEggHunt.Web.Controllers;

/// <summary>
/// MVC Controller für Employee-Interface
/// </summary>
public class EmployeeController : Controller
{
    private readonly ICampaignService _campaignService;
    private readonly IQrCodeService _qrCodeService;
    private readonly IUserService _userService;
    private readonly IFindService _findService;
    private readonly ILogger<EmployeeController> _logger;

    public EmployeeController(
        ICampaignService campaignService,
        IQrCodeService qrCodeService,
        IUserService userService,
        IFindService findService,
        ILogger<EmployeeController> logger)
    {
        _campaignService = campaignService ?? throw new ArgumentNullException(nameof(campaignService));
        _qrCodeService = qrCodeService ?? throw new ArgumentNullException(nameof(qrCodeService));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _findService = findService ?? throw new ArgumentNullException(nameof(findService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Employee Dashboard - Übersicht der verfügbaren Kampagnen
    /// </summary>
    /// <returns>Dashboard View</returns>
    public async Task<IActionResult> Index()
    {
        try
        {
            _logger.LogInformation("Employee Dashboard wird geladen");

            var campaigns = await _campaignService.GetActiveCampaignsAsync();
            var campaignViewModels = new List<CampaignOverviewViewModel>();

            foreach (var campaign in campaigns)
            {
                var qrCodes = await _qrCodeService.GetQrCodesByCampaignIdAsync(campaign.Id);
                var totalFinds = 0;

                foreach (var qrCode in qrCodes)
                {
                    var finds = await _findService.GetFindsByQrCodeIdAsync(qrCode.Id);
                    totalFinds += finds.Count();
                }

                campaignViewModels.Add(new CampaignOverviewViewModel
                {
                    Campaign = campaign,
                    TotalQrCodes = qrCodes.Count(),
                    TotalFinds = totalFinds
                });
            }

            return View(campaignViewModels);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden des Employee Dashboards");
            return View("Error");
        }
    }

    /// <summary>
    /// QR-Code scannen - GET
    /// </summary>
    /// <returns>QR-Code scannen View</returns>
    public IActionResult ScanQrCode()
    {
        return View(new ScanQrCodeViewModel());
    }

    /// <summary>
    /// QR-Code scannen - POST
    /// </summary>
    /// <param name="model">QR-Code Daten</param>
    /// <returns>Scan-Ergebnis</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ScanQrCode(ScanQrCodeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            _logger.LogInformation("QR-Code wird gescannt: {QrCode}", model.QrCode);

            // QR-Code scanning logic can be implemented in future sprints
            // This would typically involve:
            // 1. Validating the QR-Code
            // 2. Finding the corresponding QrCode entity
            // 3. Recording the find
            // 4. Returning appropriate feedback

            ModelState.AddModelError("", "QR-Code Scanning ist noch nicht implementiert.");
            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Scannen des QR-Codes {QrCode}", model.QrCode);
            ModelState.AddModelError("", "Fehler beim Scannen des QR-Codes. Bitte versuchen Sie es erneut.");
            return View(model);
        }
    }

    /// <summary>
    /// Benutzer-Profil anzeigen
    /// </summary>
    /// <param name="userId">Benutzer-ID</param>
    /// <returns>Benutzer-Profil View</returns>
    public async Task<IActionResult> Profile(int userId)
    {
        try
        {
            _logger.LogInformation("Benutzer-Profil wird geladen für ID {UserId}", userId);

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var findCount = await _findService.GetFindCountByUserIdAsync(userId);
            var recentFinds = await _findService.GetFindsByUserIdAsync(userId);

            var viewModel = new UserProfileViewModel(recentFinds.Take(10))
            {
                User = user,
                FindCount = findCount
            };

            return View(viewModel);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden des Benutzer-Profils für ID {UserId}", userId);
            return View("Error");
        }
    }

    /// <summary>
    /// Kampagnen-Details für Employee anzeigen
    /// </summary>
    /// <param name="id">Kampagnen-ID</param>
    /// <returns>Kampagnen-Details View</returns>
    public async Task<IActionResult> CampaignDetails(int id)
    {
        try
        {
            _logger.LogInformation("Kampagnen-Details werden geladen für ID {CampaignId}", id);

            var campaign = await _campaignService.GetCampaignByIdAsync(id);
            if (campaign == null)
            {
                return NotFound();
            }

            var qrCodes = await _qrCodeService.GetQrCodesByCampaignIdAsync(id);
            var totalFinds = 0;

            foreach (var qrCode in qrCodes)
            {
                var finds = await _findService.GetFindsByQrCodeIdAsync(qrCode.Id);
                totalFinds += finds.Count();
            }

            var viewModel = new EmployeeCampaignDetailsViewModel(qrCodes)
            {
                Campaign = campaign,
                TotalFinds = totalFinds
            };

            return View(viewModel);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der Kampagnen-Details für ID {CampaignId}", id);
            return View("Error");
        }
    }

    /// <summary>
    /// Leaderboard anzeigen
    /// </summary>
    /// <returns>Leaderboard View</returns>
    public async Task<IActionResult> Leaderboard()
    {
        try
        {
            _logger.LogInformation("Leaderboard wird geladen");

            var users = await _userService.GetActiveUsersAsync();
            var leaderboard = new List<UserLeaderboardEntry>();

            foreach (var user in users)
            {
                var findCount = await _findService.GetFindCountByUserIdAsync(user.Id);
                leaderboard.Add(new UserLeaderboardEntry
                {
                    User = user,
                    FindCount = findCount
                });
            }

            var viewModel = new LeaderboardViewModel(leaderboard.OrderByDescending(e => e.FindCount));

            return View(viewModel);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden des Leaderboards");
            return View("Error");
        }
    }
}
