using EasterEggHunt.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace EasterEggHunt.Web.Controllers;

/// <summary>
/// MVC Controller für Employee-Interface
/// TODO: Auf API-Client umstellen (wie AdminController)
/// </summary>
public class EmployeeController : Controller
{
    private readonly ILogger<EmployeeController> _logger;

    public EmployeeController(ILogger<EmployeeController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Employee Dashboard - Übersicht der verfügbaren Kampagnen
    /// </summary>
    /// <returns>Dashboard View</returns>
    public IActionResult Index()
    {
        _logger.LogInformation("Employee Dashboard wird geladen");

        // TODO: Implementierung über API-Client
        return View("ComingSoon");
    }

    /// <summary>
    /// QR-Code scannen - GET
    /// </summary>
    /// <returns>QR-Code scannen View</returns>
    public IActionResult ScanQrCode()
    {
        // TODO: Implementierung über API-Client
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
        // TODO: Implementierung über API-Client
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
