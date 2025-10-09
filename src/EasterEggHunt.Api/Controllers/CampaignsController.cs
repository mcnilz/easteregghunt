using System.ComponentModel.DataAnnotations;
using EasterEggHunt.Application.Services;
using EasterEggHunt.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EasterEggHunt.Api.Controllers;

/// <summary>
/// API Controller für Campaign-Operationen
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CampaignsController : ControllerBase
{
    private readonly ICampaignService _campaignService;
    private readonly ILogger<CampaignsController> _logger;

    public CampaignsController(ICampaignService campaignService, ILogger<CampaignsController> logger)
    {
        _campaignService = campaignService ?? throw new ArgumentNullException(nameof(campaignService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Ruft alle aktiven Kampagnen ab
    /// </summary>
    /// <returns>Liste aller aktiven Kampagnen</returns>
    [HttpGet("active")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Campaign>>> GetActiveCampaigns()
    {
        try
        {
            var campaigns = await _campaignService.GetActiveCampaignsAsync();
            return Ok(campaigns);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der aktiven Kampagnen");
            return StatusCode(500, "Interner Serverfehler");
        }
    }

    /// <summary>
    /// Ruft eine Kampagne anhand der ID ab
    /// </summary>
    /// <param name="id">Kampagnen-ID</param>
    /// <returns>Kampagne oder 404 wenn nicht gefunden</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Campaign>> GetCampaign(int id)
    {
        try
        {
            var campaign = await _campaignService.GetCampaignByIdAsync(id);
            if (campaign == null)
            {
                return NotFound($"Kampagne mit ID {id} nicht gefunden");
            }
            return Ok(campaign);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der Kampagne {CampaignId}", id);
            return StatusCode(500, "Interner Serverfehler");
        }
    }

    /// <summary>
    /// Erstellt eine neue Kampagne
    /// </summary>
    /// <param name="request">Kampagnen-Erstellungsdaten</param>
    /// <returns>Erstellte Kampagne</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Campaign>> CreateCampaign([FromBody] CreateCampaignRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var campaign = await _campaignService.CreateCampaignAsync(
                request.Name,
                request.Description,
                request.CreatedBy);

            return CreatedAtAction(nameof(GetCampaign), new { id = campaign.Id }, campaign);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Ungültige Argumente beim Erstellen der Kampagne");
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Erstellen der Kampagne");
            return StatusCode(500, "Interner Serverfehler");
        }
    }

    /// <summary>
    /// Aktualisiert eine bestehende Kampagne
    /// </summary>
    /// <param name="id">Kampagnen-ID</param>
    /// <param name="request">Aktualisierungsdaten</param>
    /// <returns>Erfolgsstatus</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> UpdateCampaign(int id, [FromBody] UpdateCampaignRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _campaignService.UpdateCampaignAsync(id, request.Name, request.Description);
            if (!success)
            {
                return NotFound($"Kampagne mit ID {id} nicht gefunden");
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Ungültige Argumente beim Aktualisieren der Kampagne {CampaignId}", id);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Aktualisieren der Kampagne {CampaignId}", id);
            return StatusCode(500, "Interner Serverfehler");
        }
    }

    /// <summary>
    /// Aktiviert eine Kampagne
    /// </summary>
    /// <param name="id">Kampagnen-ID</param>
    /// <returns>Erfolgsstatus</returns>
    [HttpPost("{id}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ActivateCampaign(int id)
    {
        try
        {
            var success = await _campaignService.ActivateCampaignAsync(id);
            if (!success)
            {
                return NotFound($"Kampagne mit ID {id} nicht gefunden");
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Aktivieren der Kampagne {CampaignId}", id);
            return StatusCode(500, "Interner Serverfehler");
        }
    }

    /// <summary>
    /// Deaktiviert eine Kampagne
    /// </summary>
    /// <param name="id">Kampagnen-ID</param>
    /// <returns>Erfolgsstatus</returns>
    [HttpPost("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeactivateCampaign(int id)
    {
        try
        {
            var success = await _campaignService.DeactivateCampaignAsync(id);
            if (!success)
            {
                return NotFound($"Kampagne mit ID {id} nicht gefunden");
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Deaktivieren der Kampagne {CampaignId}", id);
            return StatusCode(500, "Interner Serverfehler");
        }
    }
}

/// <summary>
/// Request-Model für Kampagnen-Erstellung
/// </summary>
public class CreateCampaignRequest
{
    /// <summary>
    /// Name der Kampagne
    /// </summary>
    [Required(ErrorMessage = "Name ist erforderlich")]
    [StringLength(100, ErrorMessage = "Name darf maximal 100 Zeichen haben")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Beschreibung der Kampagne
    /// </summary>
    [Required(ErrorMessage = "Beschreibung ist erforderlich")]
    [StringLength(500, ErrorMessage = "Beschreibung darf maximal 500 Zeichen haben")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Erstellt von (Admin-Name)
    /// </summary>
    [Required(ErrorMessage = "Erstellt von ist erforderlich")]
    [StringLength(50, ErrorMessage = "Erstellt von darf maximal 50 Zeichen haben")]
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// Request-Model für Kampagnen-Aktualisierung
/// </summary>
public class UpdateCampaignRequest
{
    /// <summary>
    /// Neuer Name der Kampagne
    /// </summary>
    [Required(ErrorMessage = "Name ist erforderlich")]
    [StringLength(100, ErrorMessage = "Name darf maximal 100 Zeichen haben")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Neue Beschreibung der Kampagne
    /// </summary>
    [Required(ErrorMessage = "Beschreibung ist erforderlich")]
    [StringLength(500, ErrorMessage = "Beschreibung darf maximal 500 Zeichen haben")]
    public string Description { get; set; } = string.Empty;
}
