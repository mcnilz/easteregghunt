using System.Globalization;
using System.Security.Claims;
using EasterEggHunt.Web.Models;
using EasterEggHunt.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace EasterEggHunt.Web.Controllers;

/// <summary>
/// Controller für Authentifizierungs-Operationen
/// </summary>
public class AuthController : Controller
{
    private readonly IEasterEggHuntApiClient _apiClient;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IEasterEggHuntApiClient apiClient, ILogger<AuthController> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Zeigt die Login-Seite an
    /// </summary>
    /// <param name="returnUrl">URL zur Weiterleitung nach Login</param>
    /// <returns>Login-View</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:URI-like parameters should not be strings", Justification = "URL wird als String für Redirect verwendet")]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        // Wenn bereits eingeloggt, zur ursprünglich angeforderte Seite weiterleiten
        if (User.Identity?.IsAuthenticated == true)
        {
            return Redirect(returnUrl ?? "/Admin");
        }

        var model = new LoginViewModel
        {
            ReturnUrl = returnUrl
        };

        return View(model);
    }

    /// <summary>
    /// Verarbeitet das Login-Formular
    /// </summary>
    /// <param name="model">Login-Daten</param>
    /// <returns>Redirect oder Login-View mit Fehlern</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            _logger.LogInformation("Login-Versuch für Benutzer: {Username}", model.Username);

            // API-Aufruf für Authentifizierung
            var loginResponse = await _apiClient.LoginAsync(model.Username, model.Password, model.RememberMe);

            if (loginResponse == null)
            {
                _logger.LogWarning("Fehlgeschlagener Login-Versuch für Benutzer: {Username}", model.Username);
                ModelState.AddModelError(string.Empty, "Ungültige Anmeldedaten");
                return View(model);
            }

            // Claims für Authentication erstellen
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, loginResponse.AdminId.ToString(CultureInfo.InvariantCulture)),
                new(ClaimTypes.Name, loginResponse.Username),
                new(ClaimTypes.Email, loginResponse.Email),
                new("AdminId", loginResponse.AdminId.ToString(CultureInfo.InvariantCulture)),
                new("LastLogin", loginResponse.LastLogin.ToString("O", CultureInfo.InvariantCulture))
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Authentication Properties konfigurieren
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8),
                AllowRefresh = true
            };

            // Session-Daten speichern
            HttpContext.Session.SetString("AdminId", loginResponse.AdminId.ToString(CultureInfo.InvariantCulture));
            HttpContext.Session.SetString("Username", loginResponse.Username);
            HttpContext.Session.SetString("Email", loginResponse.Email);

            // Authentication Cookie setzen
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProperties);

            _logger.LogInformation("Erfolgreicher Login für Benutzer: {Username} (ID: {AdminId})",
                model.Username, loginResponse.AdminId);

            // Zur ursprünglich angeforderte Seite oder Admin-Dashboard weiterleiten
            var returnUrl = model.ReturnUrl ?? "/Admin";

            // Sicherheitsprüfung: Nur relative URLs erlauben
            if (!Url.IsLocalUrl(returnUrl))
            {
                returnUrl = "/Admin";
            }

            return Redirect(returnUrl);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP-Fehler beim Login für Benutzer: {Username}", model.Username);
            ModelState.AddModelError(string.Empty, "Verbindungsfehler. Bitte versuchen Sie es erneut.");
            return View(model);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout beim Login für Benutzer: {Username}", model.Username);
            ModelState.AddModelError(string.Empty, "Zeitüberschreitung. Bitte versuchen Sie es erneut.");
            return View(model);
        }
    }

    /// <summary>
    /// Verarbeitet den Logout
    /// </summary>
    /// <returns>Redirect zur Login-Seite</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var username = User.Identity?.Name ?? "Unbekannt";
            _logger.LogInformation("Logout für Benutzer: {Username}", username);

            // Session-Daten löschen
            HttpContext.Session.Clear();

            // API-Logout aufrufen (optional)
            try
            {
                await _apiClient.LogoutAsync();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "HTTP-Fehler beim API-Logout für Benutzer: {Username}", username);
                // Logout trotzdem fortsetzen
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogWarning(ex, "Timeout beim API-Logout für Benutzer: {Username}", username);
                // Logout trotzdem fortsetzen
            }

            // Authentication Cookie löschen
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(nameof(Login));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP-Fehler beim Logout");
            return RedirectToAction(nameof(Login));
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout beim Logout");
            return RedirectToAction(nameof(Login));
        }
    }

    /// <summary>
    /// Zeigt die Access Denied Seite an
    /// </summary>
    /// <returns>Access Denied View</returns>
    [HttpGet]
    public IActionResult AccessDenied()
    {
        var model = new AccessDeniedViewModel
        {
            RequestedUrl = Request.Query["ReturnUrl"].FirstOrDefault()
        };

        return View(model);
    }
}
