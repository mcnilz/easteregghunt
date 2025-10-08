using EasterEggHunt.Application.Requests;
using EasterEggHunt.Application.Services;
using EasterEggHunt.Domain.Entities;
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

            // QR-Code Statistiken berechnen
            var allQrCodes = new List<QrCode>();
            var allFinds = new List<Find>();
            var recentActivities = new List<RecentActivityViewModel>();

            foreach (var campaign in campaigns)
            {
                var qrCodes = await _qrCodeService.GetQrCodesByCampaignIdAsync(campaign.Id);
                allQrCodes.AddRange(qrCodes);

                // Letzte Funde für diese Kampagne sammeln
                foreach (var qrCode in qrCodes)
                {
                    var finds = await _findService.GetFindsByQrCodeIdAsync(qrCode.Id);
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

            var viewModel = new AdminDashboardViewModel(campaigns)
            {
                TotalUsers = users.Count(),
                ActiveCampaigns = campaigns.Count(c => c.IsActive),
                TotalQrCodes = allQrCodes.Count,
                ActiveQrCodes = allQrCodes.Count(q => q.IsActive),
                TotalFinds = allFinds.Count,
                RecentActivities = recentActivities
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
                "Admin"); // In future sprints, get from authenticated user context

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

    #region QR-Code Management

    /// <summary>
    /// QR-Code erstellen - GET
    /// </summary>
    /// <param name="campaignId">Kampagnen-ID</param>
    /// <returns>QR-Code erstellen View</returns>
    public async Task<IActionResult> CreateQrCode(int campaignId)
    {
        try
        {
            _logger.LogInformation("QR-Code erstellen für Kampagne {CampaignId}", campaignId);

            var campaign = await _campaignService.GetCampaignByIdAsync(campaignId);
            if (campaign == null)
            {
                return NotFound();
            }

            var viewModel = new CreateQrCodeViewModel
            {
                CampaignId = campaignId,
                CampaignName = campaign.Name
            };

            return View(viewModel);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der QR-Code erstellen Seite für Kampagne {CampaignId}", campaignId);
            return View("Error");
        }
    }

    /// <summary>
    /// QR-Code erstellen - POST
    /// </summary>
    /// <param name="viewModel">QR-Code Daten</param>
    /// <returns>Redirect zur Kampagnen-Details</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateQrCode(CreateQrCodeViewModel viewModel)
    {
        try
        {
            _logger.LogInformation("QR-Code wird erstellt für Kampagne {CampaignId}", viewModel.CampaignId);

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var request = new CreateQrCodeRequest
            {
                CampaignId = viewModel.CampaignId,
                Title = viewModel.Title,
                Description = viewModel.Description,
                InternalNotes = viewModel.InternalNotes
            };

            await _qrCodeService.CreateQrCodeAsync(request);

            _logger.LogInformation("QR-Code erfolgreich erstellt für Kampagne {CampaignId}", viewModel.CampaignId);
            return RedirectToAction(nameof(CampaignDetails), new { id = viewModel.CampaignId });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Erstellen des QR-Codes für Kampagne {CampaignId}", viewModel.CampaignId);
            ModelState.AddModelError("", "Fehler beim Erstellen des QR-Codes. Bitte versuchen Sie es erneut.");
            return View(viewModel);
        }
    }

    /// <summary>
    /// QR-Code bearbeiten - GET
    /// </summary>
    /// <param name="id">QR-Code-ID</param>
    /// <returns>QR-Code bearbeiten View</returns>
    public async Task<IActionResult> EditQrCode(int id)
    {
        try
        {
            _logger.LogInformation("QR-Code bearbeiten für ID {QrCodeId}", id);

            var qrCode = await _qrCodeService.GetQrCodeByIdAsync(id);
            if (qrCode == null)
            {
                return NotFound();
            }

            var viewModel = new EditQrCodeViewModel
            {
                Id = qrCode.Id,
                CampaignId = qrCode.CampaignId,
                Title = qrCode.Title,
                Description = qrCode.Description,
                InternalNotes = qrCode.InternalNotes
            };

            return View(viewModel);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der QR-Code bearbeiten Seite für ID {QrCodeId}", id);
            return View("Error");
        }
    }

    /// <summary>
    /// QR-Code bearbeiten - POST
    /// </summary>
    /// <param name="viewModel">QR-Code Daten</param>
    /// <returns>Redirect zur Kampagnen-Details</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditQrCode(EditQrCodeViewModel viewModel)
    {
        try
        {
            _logger.LogInformation("QR-Code wird bearbeitet für ID {QrCodeId}", viewModel.Id);

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var request = new UpdateQrCodeRequest
            {
                Id = viewModel.Id,
                Title = viewModel.Title,
                Description = viewModel.Description,
                InternalNotes = viewModel.InternalNotes
            };

            await _qrCodeService.UpdateQrCodeAsync(request);

            _logger.LogInformation("QR-Code erfolgreich bearbeitet für ID {QrCodeId}", viewModel.Id);
            return RedirectToAction(nameof(CampaignDetails), new { id = viewModel.CampaignId });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Bearbeiten des QR-Codes für ID {QrCodeId}", viewModel.Id);
            ModelState.AddModelError("", "Fehler beim Bearbeiten des QR-Codes. Bitte versuchen Sie es erneut.");
            return View(viewModel);
        }
    }

    /// <summary>
    /// QR-Code löschen - GET
    /// </summary>
    /// <param name="id">QR-Code-ID</param>
    /// <returns>QR-Code löschen Bestätigung</returns>
    public async Task<IActionResult> DeleteQrCode(int id)
    {
        try
        {
            _logger.LogInformation("QR-Code löschen für ID {QrCodeId}", id);

            var qrCode = await _qrCodeService.GetQrCodeByIdAsync(id);
            if (qrCode == null)
            {
                return NotFound();
            }

            var campaign = await _campaignService.GetCampaignByIdAsync(qrCode.CampaignId);
            var viewModel = new DeleteQrCodeViewModel
            {
                Id = qrCode.Id,
                CampaignId = qrCode.CampaignId,
                CampaignName = campaign?.Name ?? "Unbekannte Kampagne",
                Title = qrCode.Title,
                Description = qrCode.Description
            };

            return View(viewModel);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der QR-Code löschen Seite für ID {QrCodeId}", id);
            return View("Error");
        }
    }

    /// <summary>
    /// QR-Code löschen - POST
    /// </summary>
    /// <param name="id">QR-Code-ID</param>
    /// <returns>Redirect zur Kampagnen-Details</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("DeleteQrCode")]
    public async Task<IActionResult> DeleteQrCodeConfirmed(int id)
    {
        try
        {
            _logger.LogInformation("QR-Code wird gelöscht für ID {QrCodeId}", id);

            var qrCode = await _qrCodeService.GetQrCodeByIdAsync(id);
            if (qrCode == null)
            {
                return NotFound();
            }

            var campaignId = qrCode.CampaignId;
            await _qrCodeService.DeleteQrCodeAsync(id);

            _logger.LogInformation("QR-Code erfolgreich gelöscht für ID {QrCodeId}", id);
            return RedirectToAction(nameof(CampaignDetails), new { id = campaignId });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Löschen des QR-Codes für ID {QrCodeId}", id);
            return View("Error");
        }
    }

    #endregion
}
