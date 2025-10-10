using System.ComponentModel.DataAnnotations;
using EasterEggHunt.Application.Services;
using EasterEggHunt.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EasterEggHunt.Api.Controllers;

/// <summary>
/// API Controller für User-Operationen
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Prüft, ob ein Benutzername bereits existiert
    /// </summary>
    /// <param name="name">Benutzername zum Prüfen</param>
    /// <returns>Objekt mit exists-Flag und Name</returns>
    [HttpGet("check-name/{name}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CheckUserNameResponse>> CheckUserName(string name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("Benutzername darf nicht leer sein");
            }

            var exists = await _userService.UserNameExistsAsync(name);
            return Ok(new CheckUserNameResponse { Exists = exists, Name = name });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Ungültiger Benutzername: {UserName}", name);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Prüfen des Benutzernamens {UserName}", name);
            return StatusCode(500, "Interner Serverfehler");
        }
    }

    /// <summary>
    /// Ruft alle aktiven Benutzer ab
    /// </summary>
    /// <returns>Liste aller aktiven Benutzer</returns>
    [HttpGet("active")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<User>>> GetActiveUsers()
    {
        try
        {
            var users = await _userService.GetActiveUsersAsync();
            return Ok(users);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der aktiven Benutzer");
            return StatusCode(500, "Interner Serverfehler");
        }
    }

    /// <summary>
    /// Ruft einen Benutzer anhand der ID ab
    /// </summary>
    /// <param name="id">Benutzer-ID</param>
    /// <returns>Benutzer oder 404 wenn nicht gefunden</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<User>> GetUserById(int id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound($"Benutzer mit ID {id} nicht gefunden");
            }
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen des Benutzers {UserId}", id);
            return StatusCode(500, "Interner Serverfehler");
        }
    }

    /// <summary>
    /// Erstellt einen neuen Benutzer
    /// </summary>
    /// <param name="request">Benutzer-Erstellungsdaten</param>
    /// <returns>Erstellter Benutzer</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<User>> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userService.CreateUserAsync(request.Name);
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Ungültige Argumente beim Erstellen des Benutzers");
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Erstellen des Benutzers");
            return StatusCode(500, "Interner Serverfehler");
        }
    }

    /// <summary>
    /// Aktualisiert den letzten Besuch eines Benutzers
    /// </summary>
    /// <param name="id">Benutzer-ID</param>
    /// <returns>Erfolgsstatus</returns>
    [HttpPut("{id}/last-seen")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> UpdateLastSeen(int id)
    {
        try
        {
            var success = await _userService.UpdateUserLastSeenAsync(id);
            if (!success)
            {
                return NotFound($"Benutzer mit ID {id} nicht gefunden");
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Aktualisieren des letzten Besuchs für Benutzer {UserId}", id);
            return StatusCode(500, "Interner Serverfehler");
        }
    }

    /// <summary>
    /// Deaktiviert einen Benutzer
    /// </summary>
    /// <param name="id">Benutzer-ID</param>
    /// <returns>Erfolgsstatus</returns>
    [HttpPost("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeactivateUser(int id)
    {
        try
        {
            var success = await _userService.DeactivateUserAsync(id);
            if (!success)
            {
                return NotFound($"Benutzer mit ID {id} nicht gefunden");
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Deaktivieren des Benutzers {UserId}", id);
            return StatusCode(500, "Interner Serverfehler");
        }
    }
}

/// <summary>
/// Request-Model für Benutzer-Erstellung
/// </summary>
public class CreateUserRequest
{
    /// <summary>
    /// Name des Benutzers
    /// </summary>
    [Required(ErrorMessage = "Name ist erforderlich")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name muss zwischen 2 und 100 Zeichen haben")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Response-Model für Benutzername-Prüfung
/// </summary>
public class CheckUserNameResponse
{
    /// <summary>
    /// Gibt an, ob der Name bereits existiert
    /// </summary>
    public bool Exists { get; set; }

    /// <summary>
    /// Der geprüfte Name
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
