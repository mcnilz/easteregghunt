using EasterEggHunt.Application.Services;
using EasterEggHunt.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace EasterEggHunt.Api.Controllers;

/// <summary>
/// API Controller für Statistik-Operationen
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StatisticsController : ControllerBase
{
    private readonly IStatisticsService _statisticsService;
    private readonly ILogger<StatisticsController> _logger;

    public StatisticsController(
        IStatisticsService statisticsService,
        ILogger<StatisticsController> logger)
    {
        _statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Ruft allgemeine System-Statistiken ab
    /// </summary>
    /// <returns>System-Statistiken</returns>
    [HttpGet("overview")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SystemOverviewStatistics>> GetSystemOverview()
    {
        try
        {
            var statistics = await _statisticsService.GetSystemOverviewAsync();
            return Ok(statistics);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der System-Übersichtsstatistiken");
            return StatusCode(StatusCodes.Status500InternalServerError, "Fehler beim Abrufen der Statistiken");
        }
    }

    /// <summary>
    /// Ruft Statistiken für eine spezifische Kampagne ab
    /// </summary>
    /// <param name="campaignId">Kampagnen-ID</param>
    /// <returns>Kampagnen-Statistiken</returns>
    [HttpGet("campaign/{campaignId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CampaignStatistics>> GetCampaignStatistics(int campaignId)
    {
        try
        {
            var statistics = await _statisticsService.GetCampaignStatisticsAsync(campaignId);
            return Ok(statistics);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Kampagne {CampaignId} nicht gefunden", campaignId);
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der Kampagnen-Statistiken für {CampaignId}", campaignId);
            return StatusCode(StatusCodes.Status500InternalServerError, "Fehler beim Abrufen der Statistiken");
        }
    }

    /// <summary>
    /// Ruft Statistiken für einen spezifischen Benutzer ab
    /// </summary>
    /// <param name="userId">Benutzer-ID</param>
    /// <returns>Benutzer-Statistiken</returns>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserStatistics>> GetUserStatistics(int userId)
    {
        try
        {
            var statistics = await _statisticsService.GetUserStatisticsAsync(userId);
            return Ok(statistics);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Benutzer {UserId} nicht gefunden", userId);
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der Benutzer-Statistiken für {UserId}", userId);
            return StatusCode(StatusCodes.Status500InternalServerError, "Fehler beim Abrufen der Statistiken");
        }
    }

    /// <summary>
    /// Ruft Statistiken für einen spezifischen QR-Code ab
    /// </summary>
    /// <param name="qrCodeId">QR-Code-ID</param>
    /// <returns>QR-Code-Statistiken</returns>
    [HttpGet("qrcode/{qrCodeId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<QrCodeStatisticsDto>> GetQrCodeStatistics(int qrCodeId)
    {
        try
        {
            var statistics = await _statisticsService.GetQrCodeStatisticsAsync(qrCodeId);
            return Ok(statistics);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "QR-Code {QrCodeId} nicht gefunden", qrCodeId);
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der QR-Code-Statistiken für {QrCodeId}", qrCodeId);
            return StatusCode(StatusCodes.Status500InternalServerError, "Fehler beim Abrufen der Statistiken");
        }
    }

    /// <summary>
    /// Ruft QR-Code-Statistiken für eine Kampagne ab
    /// </summary>
    /// <param name="campaignId">Kampagnen-ID</param>
    /// <returns>Kampagnen-QR-Code-Statistiken</returns>
    [HttpGet("campaign/{campaignId}/qrcodes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CampaignQrCodeStatisticsDto>> GetCampaignQrCodeStatistics(int campaignId)
    {
        try
        {
            var statistics = await _statisticsService.GetCampaignQrCodeStatisticsAsync(campaignId);
            return Ok(statistics);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Kampagne {CampaignId} nicht gefunden", campaignId);
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der Kampagnen-QR-Code-Statistiken für {CampaignId}", campaignId);
            return StatusCode(StatusCodes.Status500InternalServerError, "Fehler beim Abrufen der Statistiken");
        }
    }

    /// <summary>
    /// Ruft Top-Performer-Statistiken ab
    /// </summary>
    /// <returns>Top-Performer-Statistiken</returns>
    [HttpGet("top-performers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TopPerformersStatistics>> GetTopPerformers()
    {
        try
        {
            var statistics = await _statisticsService.GetTopPerformersAsync();
            return Ok(statistics);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der Top-Performer-Statistiken");
            return StatusCode(StatusCodes.Status500InternalServerError, "Fehler beim Abrufen der Statistiken");
        }
    }

    /// <summary>
    /// Ruft zeitbasierte Statistiken ab
    /// </summary>
    /// <param name="startDate">Startdatum (optional, Format: yyyy-MM-dd)</param>
    /// <param name="endDate">Enddatum (optional, Format: yyyy-MM-dd)</param>
    /// <returns>Zeitbasierte Statistiken gruppiert nach Tag, Woche und Monat</returns>
    [HttpGet("time-based")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TimeBasedStatistics>> GetTimeBasedStatistics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            {
                return BadRequest("Startdatum darf nicht nach Enddatum liegen");
            }

            var statistics = await _statisticsService.GetTimeBasedStatisticsAsync(startDate, endDate);
            return Ok(statistics);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Ungültige Parameter für zeitbasierte Statistiken");
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der zeitbasierten Statistiken");
            return StatusCode(StatusCodes.Status500InternalServerError, "Fehler beim Abrufen der Statistiken");
        }
    }
}
