using EasterEggHunt.Application.Services;
using EasterEggHunt.Domain.Entities;
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
}

