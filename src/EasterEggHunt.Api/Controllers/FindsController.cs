using EasterEggHunt.Application.Services;
using EasterEggHunt.Domain.Entities;
using EasterEggHunterApi.Abstractions.Models;
using Microsoft.AspNetCore.Mvc;

namespace EasterEggHunt.Api.Controllers;

/// <summary>
/// API Controller für Find-Operationen
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FindsController : ControllerBase
{
    private readonly IFindService _findService;
    private readonly ILogger<FindsController> _logger;

    public FindsController(IFindService findService, ILogger<FindsController> logger)
    {
        _findService = findService ?? throw new ArgumentNullException(nameof(findService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Ruft alle Funde für einen QR-Code ab
    /// </summary>
    /// <param name="qrCodeId">QR-Code-ID</param>
    /// <returns>Liste aller Funde für den QR-Code</returns>
    [HttpGet("qrcode/{qrCodeId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Find>>> GetFindsByQrCodeId(int qrCodeId)
    {
        try
        {
            var finds = await _findService.GetFindsByQrCodeIdAsync(qrCodeId);
            return Ok(finds);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der Funde für QR-Code {QrCodeId}", qrCodeId);
            return StatusCode(500, "Interner Serverfehler");
        }
    }

    /// <summary>
    /// Ruft die Anzahl der Funde für einen Benutzer ab
    /// </summary>
    /// <param name="userId">Benutzer-ID</param>
    /// <returns>Anzahl der Funde</returns>
    [HttpGet("user/{userId}/count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<int>> GetFindCountByUserId(int userId)
    {
        try
        {
            var count = await _findService.GetFindCountByUserIdAsync(userId);
            return Ok(count);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der Fund-Anzahl für Benutzer {UserId}", userId);
            return StatusCode(500, "Interner Serverfehler");
        }
    }

    /// <summary>
    /// Ruft alle Funde für einen Benutzer ab
    /// </summary>
    /// <param name="userId">Benutzer-ID</param>
    /// <returns>Liste aller Funde des Benutzers</returns>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Find>>> GetFindsByUserId(int userId)
    {
        try
        {
            var finds = await _findService.GetFindsByUserIdAsync(userId);
            return Ok(finds);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der Funde für Benutzer {UserId}", userId);
            return StatusCode(500, "Interner Serverfehler");
        }
    }

    /// <summary>
    /// Ruft Funde eines Benutzers für eine Kampagne ab, optional limitiert
    /// </summary>
    [HttpGet("user/{userId}/by-campaign")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Find>>> GetFindsByUserAndCampaign(int userId, [FromQuery] int campaignId, [FromQuery] int? take)
    {
        try
        {
            if (campaignId <= 0)
            {
                return BadRequest("campaignId ist erforderlich");
            }

            var finds = await _findService.GetFindsByUserAndCampaignAsync(userId, campaignId, take);
            return Ok(finds);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der Funde für Benutzer {UserId} und Kampagne {CampaignId}", userId, campaignId);
            return StatusCode(500, "Interner Serverfehler");
        }
    }

    /// <summary>
    /// Registriert einen neuen Fund
    /// </summary>
    /// <param name="request">Fund-Registrierungsdaten</param>
    /// <returns>Registrierter Fund</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Find>> RegisterFind([FromBody] RegisterFindRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var find = await _findService.RegisterFindAsync(
                request.QrCodeId,
                request.UserId,
                request.IpAddress,
                request.UserAgent);

            return CreatedAtAction(nameof(GetFindsByUserId), new { userId = request.UserId }, find);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Ungültige Argumente beim Registrieren des Funds");
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Registrieren des Funds");
            return StatusCode(500, "Interner Serverfehler");
        }
    }

    /// <summary>
    /// Prüft, ob ein Benutzer bereits einen QR-Code gefunden hat
    /// </summary>
    /// <param name="qrCodeId">QR-Code-ID</param>
    /// <param name="userId">Benutzer-ID</param>
    /// <returns>Erster Fund oder null wenn nicht gefunden</returns>
    [HttpGet("check")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Find?>> CheckExistingFind([FromQuery] int qrCodeId, [FromQuery] int userId)
    {
        try
        {
            var existingFind = await _findService.GetExistingFindAsync(qrCodeId, userId);

            // Explizit null zurückgeben, damit es korrekt als JSON null serialisiert wird
            if (existingFind == null)
            {
                return Ok((Find?)null);
            }

            return Ok(existingFind);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Prüfen des bestehenden Funds für QR-Code {QrCodeId} und Benutzer {UserId}", qrCodeId, userId);
            return StatusCode(500, "Interner Serverfehler");
        }
    }
}
