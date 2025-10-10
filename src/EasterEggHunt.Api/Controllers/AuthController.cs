using EasterEggHunt.Api.Models;
using EasterEggHunt.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace EasterEggHunt.Api.Controllers;

/// <summary>
/// API Controller für Authentifizierungs-Operationen
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Authentifiziert einen Administrator
    /// </summary>
    /// <param name="request">Login-Anfrage</param>
    /// <returns>Admin-Daten bei erfolgreichem Login</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Login-Versuch für Benutzer: {Username}", request.Username);

            var adminUser = await _authService.AuthenticateAdminAsync(request.Username, request.Password);

            if (adminUser == null)
            {
                _logger.LogWarning("Fehlgeschlagener Login-Versuch für Benutzer: {Username}", request.Username);
                return Unauthorized("Ungültige Anmeldedaten");
            }

            // Check if user is active
            if (!adminUser.IsActive)
            {
                _logger.LogWarning("Login-Versuch für inaktiven Benutzer: {Username}", request.Username);
                return Unauthorized("Benutzerkonto ist deaktiviert");
            }

            _logger.LogInformation("Erfolgreicher Login für Benutzer: {Username} (ID: {AdminId})",
                request.Username, adminUser.Id);

            var response = new LoginResponse
            {
                AdminId = adminUser.Id,
                Username = adminUser.Username,
                Email = adminUser.Email,
                LastLogin = adminUser.LastLogin ?? DateTime.UtcNow,
                IsActive = adminUser.IsActive
            };

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Ungültige Argumente beim Login für Benutzer: {Username}", request.Username);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Login für Benutzer: {Username}", request.Username);
            return StatusCode(500, "Interner Serverfehler");
        }
    }

    /// <summary>
    /// Validiert eine bestehende Session
    /// </summary>
    /// <returns>Session-Status</returns>
    [HttpGet("validate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Validate()
    {
        // Diese Methode kann später erweitert werden für Session-Validierung
        // Aktuell gibt sie nur den Status zurück
        return Ok(new { Valid = true, Timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Logout-Endpoint (für API-Konsistenz)
    /// </summary>
    /// <returns>Erfolgsstatus</returns>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Logout()
    {
        _logger.LogInformation("Logout-Anfrage empfangen");

        // Session-Invalidierung wird im Web-Layer gehandhabt
        // Dieser Endpoint ist für API-Konsistenz vorhanden
        return Ok(new { Message = "Logout erfolgreich", Timestamp = DateTime.UtcNow });
    }
}
