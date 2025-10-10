using System.Globalization;
using System.Security.Claims;
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
        if (User.Identity?.IsAuthenticated == true && User.Identity.AuthenticationType == "EmployeeScheme")
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
        if (!string.IsNullOrEmpty(qrCodeUrl) && Url.IsLocalUrl(qrCodeUrl))
        {
            return Redirect(qrCodeUrl);
        }

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
    /// QR-Code scannen - GET
    /// </summary>
    /// <param name="code">QR-Code Identifier</param>
    /// <returns>QR-Code scannen View oder Weiterleitung zur Registrierung</returns>
    public IActionResult ScanQrCode(string? code)
    {
        // Prüfen ob als Employee registriert
        if (User.Identity?.IsAuthenticated != true || User.Identity.AuthenticationType != "EmployeeScheme")
        {
            _logger.LogInformation("Nicht registrierter Benutzer versucht QR-Code zu scannen, Weiterleitung zur Registrierung");

            // Weiterleitung zur Registrierung mit QR-Code URL
            var qrCodeUrl = !string.IsNullOrEmpty(code)
                ? $"/Employee/ScanQrCode?code={Uri.EscapeDataString(code)}"
                : null;

            return RedirectToAction(nameof(Register), new { qrCodeUrl });
        }

        _logger.LogInformation("Registrierter Mitarbeiter scannt QR-Code: {Code}", code);

        // TODO: QR-Code Scan-Logik implementieren (Story 3.2)
        return View("ComingSoon");
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
        if (User.Identity?.IsAuthenticated != true || User.Identity.AuthenticationType != "EmployeeScheme")
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
}
