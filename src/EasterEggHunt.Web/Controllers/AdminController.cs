using EasterEggHunt.Web.Models;
using EasterEggHunt.Web.Services;
using EasterEggHunterApi.Abstractions.Models.Campaign;
using EasterEggHunterApi.Abstractions.Models.QrCode;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

#pragma warning disable CA1031 // Do not catch general exception types
namespace EasterEggHunt.Web.Controllers;

/// <summary>
/// MVC Controller für Admin-Interface
/// </summary>
[Authorize]
public class AdminController : Controller
{
    private readonly ICampaignManagementService _campaignService;
    private readonly IQrCodeManagementService _qrCodeService;
    private readonly IStatisticsDisplayService _statisticsService;
    private readonly IPrintLayoutService _printService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        ICampaignManagementService campaignService,
        IQrCodeManagementService qrCodeService,
        IStatisticsDisplayService statisticsService,
        IPrintLayoutService printService,
        ILogger<AdminController> logger)
    {
        _campaignService = campaignService ?? throw new ArgumentNullException(nameof(campaignService));
        _qrCodeService = qrCodeService ?? throw new ArgumentNullException(nameof(qrCodeService));
        _statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
        _printService = printService ?? throw new ArgumentNullException(nameof(printService));
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
            var viewModel = await _statisticsService.BuildDashboardStatisticsAsync();
            return View(viewModel);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API-Verbindungsfehler beim Laden des Admin Dashboards");
            return View("Error");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "API-Timeout beim Laden des Admin Dashboards");
            return View("Error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unerwarteter Fehler beim Laden des Admin Dashboards");
            return View("Error");
        }
    }

    /// <summary>
    /// Kampagnen-Übersicht
    /// </summary>
    /// <returns>Kampagnen-Liste</returns>
    public async Task<IActionResult> Campaigns()
    {
        try
        {
            _logger.LogInformation("Lade Kampagnen-Übersicht");
            var campaigns = await _campaignService.GetActiveCampaignsAsync();
            return View(campaigns);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der Kampagnen");
            return View("Error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unerwarteter Fehler beim Laden der Kampagnen");
            return View("Error");
        }
    }

    /// <summary>
    /// Kampagne erstellen - GET
    /// </summary>
    /// <returns>Erstellungsformular</returns>
    public IActionResult CreateCampaign()
    {
        return View(new CreateCampaignRequest());
    }

    /// <summary>
    /// Kampagne erstellen - POST
    /// </summary>
    /// <param name="request">Kampagnen-Daten</param>
    /// <returns>Redirect oder View mit Fehlern</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCampaign(CreateCampaignRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            _logger.LogInformation("Erstelle neue Kampagne: {CampaignName}", request.Name);
            await _campaignService.CreateCampaignAsync(request);

            TempData["SuccessMessage"] = $"Kampagne '{request.Name}' wurde erfolgreich erstellt.";
            return RedirectToAction(nameof(Campaigns));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Erstellen der Kampagne: {CampaignName}", request.Name);
            ModelState.AddModelError("", "Fehler beim Erstellen der Kampagne. Bitte versuchen Sie es erneut.");
            return View(request);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Ungültige Daten für Kampagne: {CampaignName}", request.Name);
            ModelState.AddModelError("", ex.Message);
            return View(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unerwarteter Fehler beim Erstellen der Kampagne: {CampaignName}", request.Name);
            ModelState.AddModelError("", "Ein unerwarteter Fehler ist aufgetreten. Bitte versuchen Sie es erneut.");
            return View(request);
        }
    }

    /// <summary>
    /// Kampagne bearbeiten - GET
    /// </summary>
    /// <param name="id">Kampagnen-ID</param>
    /// <returns>Bearbeitungsformular</returns>
    public async Task<IActionResult> EditCampaign(int id)
    {
        try
        {
            var campaign = await _campaignService.GetCampaignByIdAsync(id);
            if (campaign == null)
            {
                return NotFound();
            }

            var request = new UpdateCampaignRequest
            {
                Name = campaign.Name,
                Description = campaign.Description,
            };

            return View(request);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der Kampagne {CampaignId}", id);
            return View("Error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unerwarteter Fehler beim Laden der Kampagne {CampaignId}", id);
            return View("Error");
        }
    }

    /// <summary>
    /// Kampagne bearbeiten - POST
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request">Kampagnen-Daten</param>
    /// <returns>Redirect oder View mit Fehlern</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCampaign(int id, UpdateCampaignRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            _logger.LogInformation("Aktualisiere Kampagne {CampaignId}: {CampaignName}", id, request.Name);
            await _campaignService.UpdateCampaignAsync(id, request);

            TempData["SuccessMessage"] = $"Kampagne '{request.Name}' wurde erfolgreich aktualisiert.";
            return RedirectToAction(nameof(Campaigns));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Aktualisieren der Kampagne {CampaignId}", id);
            ModelState.AddModelError("", "Fehler beim Aktualisieren der Kampagne. Bitte versuchen Sie es erneut.");
            return View(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unerwarteter Fehler beim Aktualisieren der Kampagne {CampaignId}", id);
            ModelState.AddModelError("", "Ein unerwarteter Fehler ist aufgetreten. Bitte versuchen Sie es erneut.");
            return View(request);
        }
    }

    /// <summary>
    /// Kampagne löschen - GET
    /// </summary>
    /// <param name="id">Kampagnen-ID</param>
    /// <returns>Bestätigungsansicht</returns>
    public async Task<IActionResult> DeleteCampaign(int id)
    {
        try
        {
            var campaign = await _campaignService.GetCampaignByIdAsync(id);
            if (campaign == null)
            {
                return NotFound();
            }

            return View(campaign);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der Kampagne {CampaignId}", id);
            return View("Error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unerwarteter Fehler beim Laden der Kampagne {CampaignId}", id);
            return View("Error");
        }
    }

    /// <summary>
    /// Kampagne löschen - POST
    /// </summary>
    /// <param name="id">Kampagnen-ID</param>
    /// <returns>Redirect</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("DeleteCampaign")]
    public async Task<IActionResult> DeleteCampaignConfirmed(int id)
    {
        try
        {
            _logger.LogInformation("Lösche Kampagne {CampaignId}", id);
            await _campaignService.DeleteCampaignAsync(id);

            TempData["SuccessMessage"] = "Kampagne wurde erfolgreich gelöscht.";
            return RedirectToAction(nameof(Campaigns));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Löschen der Kampagne {CampaignId}", id);
            TempData["ErrorMessage"] = "Fehler beim Löschen der Kampagne. Bitte versuchen Sie es erneut.";
            return RedirectToAction(nameof(Campaigns));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unerwarteter Fehler beim Löschen der Kampagne {CampaignId}", id);
            TempData["ErrorMessage"] = "Ein unerwarteter Fehler ist aufgetreten. Bitte versuchen Sie es erneut.";
            return RedirectToAction(nameof(Campaigns));
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
            _logger.LogInformation("Lade Kampagnen-Details für Kampagne {CampaignId}", id);

            // Kampagne laden
            var campaign = await _campaignService.GetCampaignByIdAsync(id);
            if (campaign == null)
            {
                return NotFound();
            }

            // QR-Codes für die Kampagne laden
            var qrCodes = await _qrCodeService.GetQrCodesByCampaignAsync(id);

            // Statistiken laden
            var statistics = await _statisticsService.GetCampaignStatisticsAsync(id);

            // ViewModel erstellen
            var viewModel = new Models.CampaignDetailsViewModel(qrCodes)
            {
                Campaign = campaign,
                TotalFinds = statistics?.TotalFinds ?? 0,
                UniqueFinders = 0, // TODO: Implementiere UniqueFinders in CampaignQrCodeStatisticsViewModel
                Statistics = statistics
            };

            return View(viewModel);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der Kampagnen-Details für Kampagne {CampaignId}", id);
            return View("Error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unerwarteter Fehler beim Laden der Kampagnen-Details für Kampagne {CampaignId}", id);
            return View("Error");
        }
    }

    /// <summary>
    /// QR-Codes für eine Kampagne anzeigen
    /// </summary>
    /// <param name="id">Kampagnen-ID</param>
    /// <returns>QR-Code-Liste</returns>
    public async Task<IActionResult> QrCodes(int id)
    {
        try
        {
            var campaign = await _campaignService.GetCampaignByIdAsync(id);
            if (campaign == null)
            {
                return NotFound();
            }

            var qrCodes = await _qrCodeService.GetQrCodesByCampaignAsync(id);

            var viewModel = new CampaignQrCodesViewModel
            {
                Campaign = campaign,
                QrCodes = qrCodes.ToList()
            };

            return View(viewModel);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der QR-Codes für Kampagne {CampaignId}", id);
            return View("Error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unerwarteter Fehler beim Laden der QR-Codes für Kampagne {CampaignId}", id);
            return View("Error");
        }
    }

    /// <summary>
    /// QR-Code erstellen - GET
    /// </summary>
    /// <param name="campaignId">Kampagnen-ID</param>
    /// <returns>Erstellungsformular</returns>
    public async Task<IActionResult> CreateQrCode(int campaignId)
    {
        try
        {
            var campaign = await _campaignService.GetCampaignByIdAsync(campaignId);
            if (campaign == null)
            {
                return NotFound();
            }

            var request = new CreateQrCodeRequest
            {
                CampaignId = campaignId
            };

            return View(request);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der Kampagne {CampaignId}", campaignId);
            return View("Error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unerwarteter Fehler beim Laden der Kampagne {CampaignId}", campaignId);
            return View("Error");
        }
    }

    /// <summary>
    /// QR-Code erstellen - POST
    /// </summary>
    /// <param name="request">QR-Code-Daten</param>
    /// <returns>Redirect oder View mit Fehlern</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateQrCode(CreateQrCodeRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            _logger.LogInformation("Erstelle neuen QR-Code: {QrCodeTitle}", request.Title);
            await _qrCodeService.CreateQrCodeAsync(request);

            TempData["SuccessMessage"] = $"QR-Code '{request.Title}' wurde erfolgreich erstellt.";
            return RedirectToAction(nameof(QrCodes), new { id = request.CampaignId });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Erstellen des QR-Codes: {QrCodeTitle}", request.Title);
            ModelState.AddModelError("", "Fehler beim Erstellen des QR-Codes. Bitte versuchen Sie es erneut.");
            return View(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unerwarteter Fehler beim Erstellen des QR-Codes: {QrCodeTitle}", request.Title);
            ModelState.AddModelError("", "Ein unerwarteter Fehler ist aufgetreten. Bitte versuchen Sie es erneut.");
            return View(request);
        }
    }

    /// <summary>
    /// QR-Code bearbeiten - GET
    /// </summary>
    /// <param name="id">QR-Code-ID</param>
    /// <returns>Bearbeitungsformular</returns>
    public async Task<IActionResult> EditQrCode(int id)
    {
        try
        {
            var qrCode = await _qrCodeService.GetQrCodeByIdAsync(id);
            if (qrCode == null)
            {
                return NotFound();
            }

            var request = new UpdateQrCodeRequest
            {
                Title = qrCode.Title,
                Description = qrCode.Description,
                InternalNotes = qrCode.InternalNotes,
            };

            return View(request);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden des QR-Codes {QrCodeId}", id);
            return View("Error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unerwarteter Fehler beim Laden des QR-Codes {QrCodeId}", id);
            return View("Error");
        }
    }

    /// <summary>
    /// QR-Code bearbeiten - POST
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request">QR-Code-Daten</param>
    /// <returns>Redirect oder View mit Fehlern</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditQrCode(int id, UpdateQrCodeRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var qrCode = await _qrCodeService.GetQrCodeByIdAsync(id);
            if (qrCode == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Aktualisiere QR-Code {QrCodeId}: {QrCodeTitle}", id, request.Title);
            await _qrCodeService.UpdateQrCodeAsync(id, request);

            TempData["SuccessMessage"] = $"QR-Code '{request.Title}' wurde erfolgreich aktualisiert.";
            return RedirectToAction(nameof(QrCodes), new { id = qrCode.CampaignId });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Aktualisieren des QR-Codes {QrCodeId}", id);
            ModelState.AddModelError("", "Fehler beim Aktualisieren des QR-Codes. Bitte versuchen Sie es erneut.");
            return View(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unerwarteter Fehler beim Aktualisieren des QR-Codes {QrCodeId}", id);
            ModelState.AddModelError("", "Ein unerwarteter Fehler ist aufgetreten. Bitte versuchen Sie es erneut.");
            return View(request);
        }
    }

    /// <summary>
    /// QR-Code löschen - GET
    /// </summary>
    /// <param name="id">QR-Code-ID</param>
    /// <returns>Bestätigungsansicht</returns>
    public async Task<IActionResult> DeleteQrCode(int id)
    {
        try
        {
            var qrCode = await _qrCodeService.GetQrCodeByIdAsync(id);
            if (qrCode == null)
            {
                return NotFound();
            }

            return View(qrCode);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden des QR-Codes {QrCodeId}", id);
            return View("Error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unerwarteter Fehler beim Laden des QR-Codes {QrCodeId}", id);
            return View("Error");
        }
    }

    /// <summary>
    /// QR-Code löschen - POST
    /// </summary>
    /// <param name="id">QR-Code-ID</param>
    /// <returns>Redirect</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("DeleteQrCode")]
    public async Task<IActionResult> DeleteQrCodeConfirmed(int id)
    {
        try
        {
            var qrCode = await _qrCodeService.GetQrCodeByIdAsync(id);
            if (qrCode == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Lösche QR-Code {QrCodeId}", id);
            await _qrCodeService.DeleteQrCodeAsync(id);

            TempData["SuccessMessage"] = "QR-Code wurde erfolgreich gelöscht.";
            return RedirectToAction(nameof(QrCodes), new { id = qrCode.CampaignId });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Löschen des QR-Codes {QrCodeId}", id);
            TempData["ErrorMessage"] = "Fehler beim Löschen des QR-Codes. Bitte versuchen Sie es erneut.";
            return RedirectToAction(nameof(QrCodes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unerwarteter Fehler beim Löschen des QR-Codes {QrCodeId}", id);
            TempData["ErrorMessage"] = "Ein unerwarteter Fehler ist aufgetreten. Bitte versuchen Sie es erneut.";
            return RedirectToAction(nameof(QrCodes));
        }
    }

    /// <summary>
    /// QR-Codes neu sortieren
    /// </summary>
    /// <param name="campaignId">Kampagnen-ID</param>
    /// <param name="qrCodeIds">Sortierte QR-Code-IDs</param>
    /// <returns>JSON-Ergebnis</returns>
    [HttpPost]
    public async Task<IActionResult> ReorderQrCodes(int campaignId, [FromBody] int[] qrCodeIds)
    {
        try
        {
            if (qrCodeIds == null || qrCodeIds.Length == 0)
            {
                return BadRequest("Keine QR-Code-IDs angegeben");
            }

            _logger.LogInformation("Sortiere QR-Codes für Kampagne {CampaignId} neu", campaignId);
            await _qrCodeService.ReorderQrCodesAsync(campaignId, qrCodeIds);

            return Json(new { success = true });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Neusortieren der QR-Codes für Kampagne {CampaignId}", campaignId);
            return Json(new { success = false, error = "Fehler beim Neusortieren der QR-Codes" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unerwarteter Fehler beim Neusortieren der QR-Codes für Kampagne {CampaignId}", campaignId);
            return Json(new { success = false, error = "Ein unerwarteter Fehler ist aufgetreten" });
        }
    }

    /// <summary>
    /// Print-Layout für QR-Codes
    /// </summary>
    /// <param name="id">Kampagnen-ID</param>
    /// <returns>Print-Layout-Ansicht</returns>
    public async Task<IActionResult> PrintQrCodes(int id)
    {
        try
        {
            var printData = await _printService.PreparePrintDataAsync(id);
            return View(printData);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der Print-Daten für Kampagne {CampaignId}", id);
            return View("Error");
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Kampagne {CampaignId} nicht gefunden", id);
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unerwarteter Fehler beim Laden der Print-Daten für Kampagne {CampaignId}", id);
            return View("Error");
        }
    }

    /// <summary>
    /// Kampagnen-Statistiken
    /// </summary>
    /// <param name="id">Kampagnen-ID</param>
    /// <returns>Statistik-Ansicht</returns>
    public async Task<IActionResult> CampaignStatistics(int id)
    {
        try
        {
            var statistics = await _statisticsService.GetCampaignStatisticsAsync(id);
            return View(statistics);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der Statistiken für Kampagne {CampaignId}", id);
            return View("Error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unerwarteter Fehler beim Laden der Statistiken für Kampagne {CampaignId}", id);
            return View("Error");
        }
    }

    /// <summary>
    /// QR-Code-Statistiken
    /// </summary>
    /// <param name="id">QR-Code-ID</param>
    /// <returns>Statistik-Ansicht</returns>
    public async Task<IActionResult> QrCodeStatistics(int id)
    {
        try
        {
            var statistics = await _statisticsService.GetQrCodeStatisticsAsync(id);
            return View(statistics);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der Statistiken für QR-Code {QrCodeId}", id);
            return View("Error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unerwarteter Fehler beim Laden der Statistiken für QR-Code {QrCodeId}", id);
            return View("Error");
        }
    }

    /// <summary>
    /// Teilnehmer-Rangliste
    /// </summary>
    /// <returns>Leaderboard-Ansicht</returns>
    public async Task<IActionResult> Leaderboard()
    {
        try
        {
            var topPerformers = await _statisticsService.GetTopPerformersAsync();
            return View(topPerformers);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der Teilnehmer-Rangliste");
            return View("Error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unerwarteter Fehler beim Laden der Teilnehmer-Rangliste");
            return View("Error");
        }
    }

    /// <summary>
    /// System-Statistiken Übersicht
    /// </summary>
    /// <returns>Statistik-Ansicht</returns>
    public async Task<IActionResult> Statistics()
    {
        try
        {
            _logger.LogInformation("Lade System-Statistiken");
            var statistics = await _statisticsService.GetStatisticsAsync();
            return View(statistics);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der System-Statistiken");
            return View("Error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unerwarteter Fehler beim Laden der System-Statistiken");
            return View("Error");
        }
    }

    /// <summary>
    /// Zeitbasierte Statistiken
    /// </summary>
    /// <param name="startDate">Startdatum (optional)</param>
    /// <param name="endDate">Enddatum (optional)</param>
    /// <returns>Zeitbasierte Statistiken-Ansicht</returns>
    public async Task<IActionResult> TimeBasedStatistics(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            {
                ModelState.AddModelError("", "Startdatum darf nicht nach Enddatum liegen");
                startDate = null;
                endDate = null;
            }

            var statistics = await _statisticsService.GetTimeBasedStatisticsAsync(startDate, endDate);

            // Filter-Parameter an View übergeben
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            return View(statistics);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der zeitbasierten Statistiken");
            return View("Error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unerwarteter Fehler beim Laden der zeitbasierten Statistiken");
            return View("Error");
        }
    }
}
