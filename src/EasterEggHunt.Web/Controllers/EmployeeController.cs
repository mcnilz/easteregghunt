using System.Globalization;
using System.Security.Claims;
using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Web.Models;
using EasterEggHunt.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace EasterEggHunt.Web.Controllers;

/// <summary>
/// MVC Controller für Employee-Interface
/// </summary>
public class EmployeeController : Controller
{
    private readonly ILogger<EmployeeController> _logger;
    private readonly IEasterEggHuntApiClient _apiClient;

    public EmployeeController(ILogger<EmployeeController> logger, IEasterEggHuntApiClient apiClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    }

    /// <summary>
    /// Mitarbeiter-Registrierung - GET
    /// </summary>
    /// <param name="qrCodeUrl">Optional: QR-Code URL für Weiterleitung nach Registrierung</param>
    /// <returns>Registrierungsformular oder Weiterleitung wenn bereits registriert</returns>
    [HttpGet]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:URI-like parameters should not be strings", Justification = "MVC requires string for query parameters")]
    public IActionResult Register(string? qrCodeUrl)
    {
        // Prüfen ob bereits als Employee eingeloggt
        if (User.Identity?.IsAuthenticated == true && User.FindFirst("UserId") != null)
        {
            _logger.LogInformation("Mitarbeiter bereits registriert, Weiterleitung");
            return RedirectToQrCodeOrDashboard(qrCodeUrl);
        }

        _logger.LogInformation("Registrierungsformular wird angezeigt");

        var model = new EmployeeRegistrationViewModel
        {
            QrCodeUrl = qrCodeUrl
        };

        return View(model);
    }

    /// <summary>
    /// Mitarbeiter-Registrierung - POST
    /// </summary>
    /// <param name="model">Registrierungsdaten</param>
    /// <returns>Weiterleitung nach erfolgreicher Registrierung oder Formular mit Fehlern</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(EmployeeRegistrationViewModel model)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Registrierung fehlgeschlagen: Ungültige Eingaben");
            return View(model);
        }

        try
        {
            _logger.LogInformation("Registrierungsversuch für: {Name}", model.Name);

            // Prüfen ob Name bereits existiert
            var nameExists = await _apiClient.CheckUserNameExistsAsync(model.Name);
            if (nameExists)
            {
                _logger.LogWarning("Registrierung fehlgeschlagen: Name bereits vergeben - {Name}", model.Name);
                ModelState.AddModelError(nameof(model.Name),
                    "Dieser Name ist bereits vergeben. Bitte wähle einen anderen Namen.");
                return View(model);
            }

            // Benutzer registrieren
            var user = await _apiClient.RegisterEmployeeAsync(model.Name);

            // Claims für Employee Authentication erstellen
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture)),
                new(ClaimTypes.Name, user.Name),
                new("UserId", user.Id.ToString(CultureInfo.InvariantCulture)),
                new("RegisteredAt", user.FirstSeen.ToString("O", CultureInfo.InvariantCulture))
            };

            var identity = new ClaimsIdentity(claims, "EmployeeScheme");
            var principal = new ClaimsPrincipal(identity);

            var authProps = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30),
                AllowRefresh = true
            };

            // Employee Cookie setzen
            await HttpContext.SignInAsync("EmployeeScheme", principal, authProps);

            // Session-Daten speichern
            HttpContext.Session.SetInt32("EmployeeUserId", user.Id);
            HttpContext.Session.SetString("EmployeeName", user.Name);
            HttpContext.Session.SetString("EmployeeRegisteredAt", user.FirstSeen.ToString("O", CultureInfo.InvariantCulture));

            _logger.LogInformation("Mitarbeiter erfolgreich registriert: {Name} (ID: {UserId})", user.Name, user.Id);

            TempData["SuccessMessage"] = $"Willkommen {user.Name}! Du bist jetzt registriert.";

            return RedirectToQrCodeOrDashboard(model.QrCodeUrl);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler bei der Registrierung für {Name}", model.Name);
            ModelState.AddModelError(string.Empty, "Ein Fehler ist aufgetreten. Bitte versuche es erneut.");
            return View(model);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP-Fehler bei der Registrierung für {Name}", model.Name);
            ModelState.AddModelError(string.Empty, "Verbindungsfehler. Bitte versuche es erneut.");
            return View(model);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout bei der Registrierung für {Name}", model.Name);
            ModelState.AddModelError(string.Empty, "Zeitüberschreitung. Bitte versuche es erneut.");
            return View(model);
        }
    }

    /// <summary>
    /// Hilfsmethode für Weiterleitung nach Registrierung
    /// </summary>
    /// <param name="qrCodeUrl">Optional: QR-Code URL</param>
    /// <returns>Redirect zur QR-Code URL oder Dashboard</returns>
    private IActionResult RedirectToQrCodeOrDashboard(string? qrCodeUrl)
    {
        if (!string.IsNullOrEmpty(qrCodeUrl))
        {
            // Prüfe ob URL mit / beginnt (relative URL) oder IsLocalUrl
            if (qrCodeUrl.StartsWith("/", StringComparison.Ordinal) || Url.IsLocalUrl(qrCodeUrl))
            {
                _logger.LogInformation("Weiterleitung zu QR-Code URL: {QrCodeUrl}", qrCodeUrl);
                return Redirect(qrCodeUrl);
            }

            _logger.LogWarning("Ungültige QR-Code URL ignoriert: {QrCodeUrl}", qrCodeUrl);
        }

        _logger.LogInformation("Weiterleitung zum Dashboard");
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Employee Dashboard - Übersicht der verfügbaren Kampagnen
    /// </summary>
    /// <returns>Dashboard View</returns>
    public IActionResult Index()
    {
        _logger.LogInformation("Employee Dashboard wird geladen");

        // TODO: Implementierung über API-Client (Story 3.3)
        return View("ComingSoon");
    }

    /// <summary>
    /// Fortschritts-Seite des eingeloggten Mitarbeiters
    /// </summary>
    /// <returns>Progress View</returns>
    public async Task<IActionResult> Progress()
    {
        // Auth prüfen
        if (User.Identity?.IsAuthenticated != true || User.FindFirst("UserId") == null)
        {
            _logger.LogWarning("Nicht registrierter Benutzer ruft Progress auf");
            return RedirectToAction(nameof(Register));
        }

        var userId = GetUserIdFromClaims();
        if (!userId.HasValue)
        {
            _logger.LogError("User-ID konnte nicht aus Claims extrahiert werden (Progress)");
            return RedirectToAction(nameof(Register));
        }

        // Aktive Kampagnen laden
        var activeCampaigns = await _apiClient.GetActiveCampaignsAsync();
        if (!activeCampaigns.Any())
        {
            _logger.LogWarning("Keine aktiven Kampagnen gefunden (Progress)");
            return View("NoCampaign");
        }

        // Alle User-Funde abrufen
        var userFinds = await _apiClient.GetFindsByUserIdAsync(userId.Value);

        // Kampagne anhand des letzten gefundenen QR-Codes bestimmen
        Campaign? campaign;
        if (userFinds.Any())
        {
            var lastFind = userFinds.OrderByDescending(f => f.FoundAt).First();
            var lastQrCode = await _apiClient.GetQrCodeByIdAsync(lastFind.QrCodeId);
            if (lastQrCode != null)
            {
                // Versuche aktive Kampagne zu nutzen, ansonsten lade spezifische Kampagne
                campaign = activeCampaigns.FirstOrDefault(c => c.Id == lastQrCode.CampaignId)
                           ?? await _apiClient.GetCampaignByIdAsync(lastQrCode.CampaignId);
            }
            else
            {
                _logger.LogWarning("Letzter Fund verweist auf unbekannten QR-Code {QrCodeId}. Fallback auf erste aktive Kampagne.", lastFind.QrCodeId);
                campaign = activeCampaigns.First();
            }
        }
        else
        {
            // Kein Fund vorhanden: Fallback auf erste aktive Kampagne
            campaign = activeCampaigns.First();
        }

        // QR-Codes der ermittelten Kampagne laden
        var campaignQrCodes = await _apiClient.GetQrCodesByCampaignIdAsync(campaign!.Id);

        // Auf Kampagnen-QR-Codes filtern und eindeutige QR-Codes zählen
        var campaignQrCodeIds = campaignQrCodes.Select(q => q.Id).ToHashSet();
        var findsInCampaign = userFinds.Where(f => campaignQrCodeIds.Contains(f.QrCodeId)).ToList();
        var uniqueFound = findsInCampaign.Select(f => f.QrCodeId).Distinct().Count();

        // Zuletzt gefundene Einträge inkl. Titel
        var qrCodeIdToTitle = campaignQrCodes.ToDictionary(q => q.Id, q => q.Title);
        var recent = findsInCampaign
            .OrderByDescending(f => f.FoundAt)
            .Take(10)
            .Select(f => new ProgressRecentFindItem
            {
                QrCodeId = f.QrCodeId,
                Title = qrCodeIdToTitle.TryGetValue(f.QrCodeId, out var t) ? t : $"QR-{f.QrCodeId}",
                FoundAt = f.FoundAt
            })
            .ToList();

        // Optional: Benutzername laden
        var userStats = await _apiClient.GetUserStatisticsAsync(userId.Value);

        var vm = new ProgressViewModel
        {
            UserName = userStats.UserName,
            CampaignName = campaign.Name,
            TotalQrCodes = campaignQrCodes.Count(),
            UniqueFound = uniqueFound,
            RecentFinds = recent
        };

        return View("Progress", vm);
    }

    /// <summary>
    /// QR-Code scannen - GET
    /// </summary>
    /// <param name="code">QR-Code Identifier</param>
    /// <returns>QR-Code scannen View oder Weiterleitung zur Registrierung</returns>
    public async Task<IActionResult> ScanQrCode(string? code)
    {
        // Prüfen ob als Employee registriert
        // Das AuthenticationType ist "Cookies", aber wir prüfen auf die Claims
        if (User.Identity?.IsAuthenticated != true || User.FindFirst("UserId") == null)
        {
            _logger.LogInformation("Nicht registrierter Benutzer versucht QR-Code zu scannen, Weiterleitung zur Registrierung");

            // Weiterleitung zur Registrierung mit QR-Code URL
            var qrCodeUrl = !string.IsNullOrEmpty(code)
                ? $"/qr/{Uri.EscapeDataString(code)}"
                : null;

            return RedirectToAction(nameof(Register), new { qrCodeUrl });
        }

        if (string.IsNullOrEmpty(code))
        {
            _logger.LogWarning("QR-Code Parameter ist leer");
            return View("InvalidQrCode");
        }

        try
        {
            _logger.LogInformation("Registrierter Mitarbeiter scannt QR-Code: {Code}", code);

            // QR-Code per Code abrufen
            var qrCode = await _apiClient.GetQrCodeByCodeAsync(code);
            if (qrCode == null)
            {
                _logger.LogWarning("QR-Code mit Code '{Code}' nicht gefunden", code);
                return View("InvalidQrCode");
            }

            // Prüfen ob QR-Code zu einer aktiven Kampagne gehört
            var activeCampaigns = await _apiClient.GetActiveCampaignsAsync();
            if (!activeCampaigns.Any())
            {
                _logger.LogWarning("Keine aktiven Kampagnen gefunden");
                return View("NoCampaign");
            }

            var qrCodeCampaign = activeCampaigns.FirstOrDefault(c => c.Id == qrCode.CampaignId);
            if (qrCodeCampaign == null)
            {
                _logger.LogWarning("QR-Code gehört nicht zu einer aktiven Kampagne. QR-Code CampaignId: {QrCodeCampaignId}, Active CampaignIds: {ActiveCampaignIds}",
                    qrCode.CampaignId, string.Join(", ", activeCampaigns.Select(c => c.Id)));
                return View("InvalidQrCode");
            }

            // User-ID aus Claims extrahieren
            var userId = GetUserIdFromClaims();
            if (userId == null)
            {
                _logger.LogError("User-ID konnte nicht aus Claims extrahiert werden");
                return RedirectToAction(nameof(Register));
            }

            // Prüfen ob User bereits gefunden hat
            var previousFind = await _apiClient.GetExistingFindAsync(qrCode.Id, userId.Value);

            // Fund nur registrieren wenn es der erste Fund ist
            Find? currentFind = null;
            if (previousFind == null)
            {
                // Erster Fund - registrieren
                var ipAddress = GetClientIpAddress();
                var userAgent = GetUserAgent();
                currentFind = await _apiClient.RegisterFindAsync(qrCode.Id, userId.Value, ipAddress, userAgent);
                _logger.LogInformation("Neuer Fund registriert für QR-Code {QrCodeId} und User {UserId}", qrCode.Id, userId.Value);
            }
            else
            {
                // Bereits gefunden - keinen neuen Fund registrieren
                currentFind = previousFind;
                _logger.LogInformation("QR-Code {QrCodeId} bereits von User {UserId} gefunden - kein neuer Fund", qrCode.Id, userId.Value);
            }

            // User-Fortschritt berechnen
            var userTotalFinds = await _apiClient.GetFindCountByUserIdAsync(userId.Value);
            var campaignQrCodes = await _apiClient.GetQrCodesByCampaignIdAsync(qrCodeCampaign.Id);
            var campaignTotalQrCodes = campaignQrCodes.Count();
            var progressPercentage = campaignTotalQrCodes > 0 ? (int)((double)userTotalFinds / campaignTotalQrCodes * 100) : 0;

            // ScanResultViewModel erstellen
            var viewModel = new ScanResultViewModel
            {
                QrCode = qrCode,
                CurrentFind = currentFind,
                PreviousFind = previousFind,
                IsFirstFind = previousFind == null,
                UserTotalFinds = userTotalFinds,
                CampaignTotalQrCodes = campaignTotalQrCodes,
                ProgressPercentage = progressPercentage
            };

            _logger.LogInformation("QR-Code erfolgreich gescannt: {QrCodeTitle} durch User {UserId}", qrCode.Title, userId);
            return View("ScanResult", viewModel);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP-Fehler beim Scannen des QR-Codes: {Code}", code);
            return View("InvalidQrCode");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Ungültige Operation beim Scannen des QR-Codes: {Code}", code);
            return View("InvalidQrCode");
        }
        catch (Exception ex) when (!(ex is HttpRequestException || ex is InvalidOperationException))
        {
            _logger.LogError(ex, "Unerwarteter Fehler beim Scannen des QR-Codes: {Code}", code);
            return View("InvalidQrCode");
        }
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
        // Prüfen ob als Employee registriert
        if (User.Identity?.IsAuthenticated != true || User.FindFirst("UserId") == null)
        {
            _logger.LogWarning("Nicht registrierter Benutzer versucht QR-Code zu scannen");
            return RedirectToAction(nameof(Register));
        }

        // TODO: QR-Code Scan-Logik implementieren (Story 3.2)
        return View("ComingSoon");
    }

    /// <summary>
    /// Benutzer-Profil anzeigen
    /// </summary>
    /// <param name="userId">Benutzer-ID</param>
    /// <returns>Benutzer-Profil View</returns>
    public IActionResult Profile(int userId)
    {
        // TODO: Implementierung über API-Client
        return View("ComingSoon");
    }

    /// <summary>
    /// Kampagnen-Details für Employee anzeigen
    /// </summary>
    /// <param name="id">Kampagnen-ID</param>
    /// <returns>Kampagnen-Details View</returns>
    public IActionResult CampaignDetails(int id)
    {
        // TODO: Implementierung über API-Client
        return View("ComingSoon");
    }

    /// <summary>
    /// Leaderboard anzeigen
    /// </summary>
    /// <returns>Leaderboard View</returns>
    public IActionResult Leaderboard()
    {
        // TODO: Implementierung über API-Client
        return View("ComingSoon");
    }

    #region Hilfsmethoden

    /// <summary>
    /// Extrahiert die User-ID aus den Claims
    /// </summary>
    /// <returns>User-ID oder null wenn nicht gefunden</returns>
    private int? GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return null;
    }

    /// <summary>
    /// Ermittelt die Client-IP-Adresse
    /// </summary>
    /// <returns>IP-Adresse</returns>
    private string GetClientIpAddress()
    {
        // Prüfe X-Forwarded-For Header (für Load Balancer/Proxy)
        var forwardedFor = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var ip = forwardedFor.Split(',')[0].Trim();
            if (!string.IsNullOrEmpty(ip))
                return ip;
        }

        // Prüfe X-Real-IP Header
        var realIp = HttpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
            return realIp;

        // Fallback auf RemoteIpAddress
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    /// <summary>
    /// Ermittelt den User-Agent
    /// </summary>
    /// <returns>User-Agent String</returns>
    private string GetUserAgent()
    {
        return HttpContext.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown";
    }

    #endregion
}
