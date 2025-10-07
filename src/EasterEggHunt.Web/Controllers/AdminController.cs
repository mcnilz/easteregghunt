using EasterEggHunt.Application.Services;
using EasterEggHunt.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace EasterEggHunt.Web.Controllers;

/// <summary>
/// MVC Controller für Admin-Interface
/// </summary>
public class AdminController : Controller
{
    private readonly ICampaignService _campaignService;
    private readonly IQrCodeService _qrCodeService;
    private readonly IUserService _userService;
    private readonly IFindService _findService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        ICampaignService campaignService,
        IQrCodeService qrCodeService,
        IUserService userService,
        IFindService findService,
        ILogger<AdminController> logger)
    {
        _campaignService = campaignService ?? throw new ArgumentNullException(nameof(campaignService));
        _qrCodeService = qrCodeService ?? throw new ArgumentNullException(nameof(qrCodeService));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _findService = findService ?? throw new ArgumentNullException(nameof(findService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Admin Dashboard - Übersicht aller Kampagnen
    /// </summary>
    /// <returns>Dashboard View</returns>
    public async Task<IActionResult> Index()
    {
        try
        {
            _logger.LogInformation("Admin Dashboard wird geladen");

            var campaigns = await _campaignService.GetActiveCampaignsAsync();
            var users = await _userService.GetActiveUsersAsync();

            var viewModel = new AdminDashboardViewModel(campaigns)
            {
                TotalUsers = users.Count(),
                ActiveCampaigns = campaigns.Count(c => c.IsActive)
            };

            return View(viewModel);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden des Admin Dashboards");
            return View("Error");
        }
    }

    /// <summary>
    /// Kampagnen-Details anzeigen
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

            var viewModel = new CampaignDetailsViewModel(qrCodes)
            {
                Campaign = campaign,
                TotalFinds = totalFinds,
                UniqueFinders = uniqueFinders.Count
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
    /// Neue Kampagne erstellen - GET
    /// </summary>
    /// <returns>Kampagne erstellen View</returns>
    public IActionResult CreateCampaign()
    {
        return View(new CreateCampaignViewModel());
    }

    /// <summary>
    /// Neue Kampagne erstellen - POST
    /// </summary>
    /// <param name="model">Kampagnen-Daten</param>
    /// <returns>Redirect oder View mit Fehlern</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCampaign(CreateCampaignViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            _logger.LogInformation("Neue Kampagne wird erstellt: {CampaignName}", model.Name);

            var campaign = await _campaignService.CreateCampaignAsync(
                model.Name,
                model.Description,
                "Admin"); // TODO: Get from authenticated user

            return RedirectToAction(nameof(CampaignDetails), new { id = campaign.Id });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Erstellen der Kampagne {CampaignName}", model.Name);
            ModelState.AddModelError("", "Fehler beim Erstellen der Kampagne. Bitte versuchen Sie es erneut.");
            return View(model);
        }
    }

    /// <summary>
    /// Benutzer-Übersicht anzeigen
    /// </summary>
    /// <returns>Benutzer-Übersicht View</returns>
    public async Task<IActionResult> Users()
    {
        try
        {
            _logger.LogInformation("Benutzer-Übersicht wird geladen");

            var users = await _userService.GetActiveUsersAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var findCount = await _findService.GetFindCountByUserIdAsync(user.Id);
                userViewModels.Add(new UserViewModel
                {
                    User = user,
                    FindCount = findCount
                });
            }

            return View(userViewModels);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der Benutzer-Übersicht");
            return View("Error");
        }
    }

    /// <summary>
    /// Statistiken anzeigen
    /// </summary>
    /// <returns>Statistiken View</returns>
    public async Task<IActionResult> Statistics()
    {
        try
        {
            _logger.LogInformation("Statistiken werden geladen");

            var campaigns = await _campaignService.GetActiveCampaignsAsync();
            var users = await _userService.GetActiveUsersAsync();
            var totalFinds = 0;
            var totalQrCodes = 0;

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

            var viewModel = new StatisticsViewModel
            {
                TotalCampaigns = campaigns.Count(),
                ActiveCampaigns = campaigns.Count(c => c.IsActive),
                TotalUsers = users.Count(),
                ActiveUsers = users.Count(u => u.IsActive),
                TotalQrCodes = totalQrCodes,
                TotalFinds = totalFinds
            };

            return View(viewModel);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der Statistiken");
            return View("Error");
        }
    }
}
