using System.ComponentModel.DataAnnotations;
using EasterEggHunt.Application.Requests;
using EasterEggHunt.Application.Services;
using EasterEggHunt.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EasterEggHunt.Api.Controllers;

/// <summary>
/// API Controller für QrCode-Operationen
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class QrCodesController : ControllerBase
{
    private readonly IQrCodeService _qrCodeService;
    private readonly ILogger<QrCodesController> _logger;

    public QrCodesController(IQrCodeService qrCodeService, ILogger<QrCodesController> logger)
    {
        _qrCodeService = qrCodeService ?? throw new ArgumentNullException(nameof(qrCodeService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Ruft alle QR-Codes für eine Kampagne ab
    /// </summary>
    /// <param name="campaignId">Kampagnen-ID</param>
    /// <returns>Liste aller QR-Codes der Kampagne</returns>
    [HttpGet("campaign/{campaignId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<QrCode>>> GetQrCodesByCampaign(int campaignId)
    {
        try
        {
            var qrCodes = await _qrCodeService.GetQrCodesByCampaignIdAsync(campaignId);
            return Ok(qrCodes);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der QR-Codes für Kampagne {CampaignId}", campaignId);
            return StatusCode(500, "Interner Serverfehler");
        }
    }

    /// <summary>
    /// Ruft einen QR-Code anhand der ID ab
    /// </summary>
    /// <param name="id">QR-Code-ID</param>
    /// <returns>QR-Code oder 404 wenn nicht gefunden</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<QrCode>> GetQrCode(int id)
    {
        try
        {
            var qrCode = await _qrCodeService.GetQrCodeByIdAsync(id);
            if (qrCode == null)
            {
                return NotFound($"QR-Code mit ID {id} nicht gefunden");
            }
            return Ok(qrCode);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen des QR-Codes {QrCodeId}", id);
            return StatusCode(500, "Interner Serverfehler");
        }
    }

    /// <summary>
    /// Erstellt einen neuen QR-Code
    /// </summary>
    /// <param name="request">QR-Code-Erstellungsdaten</param>
    /// <returns>Erstellter QR-Code</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<QrCode>> CreateQrCode([FromBody] CreateQrCodeApiRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createRequest = new CreateQrCodeRequest
            {
                CampaignId = request.CampaignId,
                Title = request.Title,
                Description = request.Description ?? string.Empty,
                InternalNotes = request.InternalNote ?? string.Empty
            };
            var qrCode = await _qrCodeService.CreateQrCodeAsync(createRequest);

            return CreatedAtAction(nameof(GetQrCode), new { id = qrCode.Id }, qrCode);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Ungültige Argumente beim Erstellen des QR-Codes");
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Erstellen des QR-Codes");
            return StatusCode(500, "Interner Serverfehler");
        }
    }

    /// <summary>
    /// Aktualisiert einen bestehenden QR-Code
    /// </summary>
    /// <param name="id">QR-Code-ID</param>
    /// <param name="request">Aktualisierungsdaten</param>
    /// <returns>Erfolgsstatus</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> UpdateQrCode(int id, [FromBody] UpdateQrCodeApiRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updateRequest = new UpdateQrCodeRequest
            {
                Id = id,
                Title = request.Title,
                Description = request.Description ?? string.Empty,
                InternalNotes = request.InternalNote ?? string.Empty
            };
            var success = await _qrCodeService.UpdateQrCodeAsync(updateRequest);
            if (!success)
            {
                return NotFound($"QR-Code mit ID {id} nicht gefunden");
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Ungültige Argumente beim Aktualisieren des QR-Codes {QrCodeId}", id);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Aktualisieren des QR-Codes {QrCodeId}", id);
            return StatusCode(500, "Interner Serverfehler");
        }
    }

    /// <summary>
    /// Setzt die Sortierreihenfolge für einen QR-Code
    /// </summary>
    /// <param name="id">QR-Code-ID</param>
    /// <param name="request">Sortierreihenfolge-Daten</param>
    /// <returns>Erfolgsstatus</returns>
    [HttpPut("{id}/sort-order")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> SetSortOrder(int id, [FromBody] SetSortOrderRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _qrCodeService.SetQrCodeSortOrderAsync(id, request.SortOrder);
            if (!success)
            {
                return NotFound($"QR-Code mit ID {id} nicht gefunden");
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Ungültige Argumente beim Setzen der Sortierreihenfolge für QR-Code {QrCodeId}", id);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Setzen der Sortierreihenfolge für QR-Code {QrCodeId}", id);
            return StatusCode(500, "Interner Serverfehler");
        }
    }

    /// <summary>
    /// Aktiviert einen QR-Code
    /// </summary>
    /// <param name="id">QR-Code-ID</param>
    /// <returns>Erfolgsstatus</returns>
    [HttpPost("{id}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ActivateQrCode(int id)
    {
        try
        {
            var success = await _qrCodeService.ActivateQrCodeAsync(id);
            if (!success)
            {
                return NotFound($"QR-Code mit ID {id} nicht gefunden");
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Aktivieren des QR-Codes {QrCodeId}", id);
            return StatusCode(500, "Interner Serverfehler");
        }
    }

    /// <summary>
    /// Deaktiviert einen QR-Code
    /// </summary>
    /// <param name="id">QR-Code-ID</param>
    /// <returns>Erfolgsstatus</returns>
    [HttpPost("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeactivateQrCode(int id)
    {
        try
        {
            var success = await _qrCodeService.DeactivateQrCodeAsync(id);
            if (!success)
            {
                return NotFound($"QR-Code mit ID {id} nicht gefunden");
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Deaktivieren des QR-Codes {QrCodeId}", id);
            return StatusCode(500, "Interner Serverfehler");
        }
    }

    /// <summary>
    /// Löscht einen QR-Code
    /// </summary>
    /// <param name="id">QR-Code-ID</param>
    /// <returns>Erfolgsstatus</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteQrCode(int id)
    {
        try
        {
            var success = await _qrCodeService.DeleteQrCodeAsync(id);
            if (!success)
            {
                return NotFound($"QR-Code mit ID {id} nicht gefunden");
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Löschen des QR-Codes {QrCodeId}", id);
            return StatusCode(500, "Interner Serverfehler");
        }
    }

    /// <summary>
    /// Ruft einen QR-Code anhand des Codes ab
    /// </summary>
    /// <param name="code">Code des QR-Codes</param>
    /// <returns>QR-Code oder 404 wenn nicht gefunden</returns>
    [HttpGet("by-code/{code}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<QrCode>> GetQrCodeByCode(string code)
    {
        try
        {
            var qrCode = await _qrCodeService.GetQrCodeByCodeAsync(code);
            if (qrCode == null)
            {
                return NotFound($"QR-Code mit Code '{code}' nicht gefunden");
            }
            return Ok(qrCode);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen des QR-Codes mit Code {Code}", code);
            return StatusCode(500, "Interner Serverfehler");
        }
    }

}

/// <summary>
/// Request-Model für QR-Code-Erstellung
/// </summary>
public class CreateQrCodeApiRequest
{
    /// <summary>
    /// ID der zugehörigen Kampagne
    /// </summary>
    [Required(ErrorMessage = "Kampagnen-ID ist erforderlich")]
    [Range(1, int.MaxValue, ErrorMessage = "Kampagnen-ID muss größer als 0 sein")]
    public int CampaignId { get; set; }

    /// <summary>
    /// Titel des QR-Codes
    /// </summary>
    [Required(ErrorMessage = "Titel ist erforderlich")]
    [StringLength(100, ErrorMessage = "Titel darf maximal 100 Zeichen haben")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Beschreibung des QR-Codes
    /// </summary>
    [StringLength(500, ErrorMessage = "Beschreibung darf maximal 500 Zeichen haben")]
    public string? Description { get; set; }

    /// <summary>
    /// Interne Notiz für Administratoren
    /// </summary>
    [Required(ErrorMessage = "Interne Notiz ist erforderlich")]
    [StringLength(500, ErrorMessage = "Interne Notiz darf maximal 500 Zeichen haben")]
    public string InternalNote { get; set; } = string.Empty;
}

/// <summary>
/// Request-Model für QR-Code-Aktualisierung
/// </summary>
public class UpdateQrCodeApiRequest
{
    /// <summary>
    /// Neuer Titel des QR-Codes
    /// </summary>
    [Required(ErrorMessage = "Titel ist erforderlich")]
    [StringLength(100, ErrorMessage = "Titel darf maximal 100 Zeichen haben")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Neue Beschreibung des QR-Codes
    /// </summary>
    [StringLength(500, ErrorMessage = "Beschreibung darf maximal 500 Zeichen haben")]
    public string? Description { get; set; }

    /// <summary>
    /// Neue interne Notiz
    /// </summary>
    [Required(ErrorMessage = "Interne Notiz ist erforderlich")]
    [StringLength(500, ErrorMessage = "Interne Notiz darf maximal 500 Zeichen haben")]
    public string InternalNote { get; set; } = string.Empty;
}

/// <summary>
/// Request-Model für Sortierreihenfolge-Änderung
/// </summary>
public class SetSortOrderRequest
{
    /// <summary>
    /// Neue Sortierreihenfolge
    /// </summary>
    [Required(ErrorMessage = "Sortierreihenfolge ist erforderlich")]
    [Range(0, int.MaxValue, ErrorMessage = "Sortierreihenfolge muss größer oder gleich 0 sein")]
    public int SortOrder { get; set; }
}
