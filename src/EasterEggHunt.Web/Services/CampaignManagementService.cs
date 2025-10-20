using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Web.Models;

namespace EasterEggHunt.Web.Services;

/// <summary>
/// Service für Kampagnen-Management im Admin-Bereich
/// </summary>
public class CampaignManagementService : ICampaignManagementService
{
    private readonly IEasterEggHuntApiClient _apiClient;
    private readonly ILogger<CampaignManagementService> _logger;

    public CampaignManagementService(
        IEasterEggHuntApiClient apiClient,
        ILogger<CampaignManagementService> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Lädt alle aktiven Kampagnen
    /// </summary>
    /// <returns>Liste der aktiven Kampagnen</returns>
    public async Task<IEnumerable<Campaign>> GetActiveCampaignsAsync()
    {
        try
        {
            _logger.LogInformation("Lade aktive Kampagnen");
            return await _apiClient.GetActiveCampaignsAsync();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der aktiven Kampagnen");
            throw;
        }
    }

    /// <summary>
    /// Lädt eine Kampagne anhand der ID
    /// </summary>
    /// <param name="id">Kampagnen-ID</param>
    /// <returns>Kampagne oder null</returns>
    public async Task<Campaign?> GetCampaignByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Lade Kampagne mit ID {CampaignId}", id);
            return await _apiClient.GetCampaignByIdAsync(id);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der Kampagne {CampaignId}", id);
            throw;
        }
    }

    /// <summary>
    /// Erstellt eine neue Kampagne
    /// </summary>
    /// <param name="request">Kampagnen-Erstellungsanfrage</param>
    /// <returns>Erstellte Kampagne</returns>
    public async Task<Campaign> CreateCampaignAsync(CreateCampaignRequest request)
    {
        try
        {
            _logger.LogInformation("Erstelle neue Kampagne: {CampaignName}", request.Name);

            if (!await ValidateCampaignDataAsync(request))
            {
                throw new ArgumentException("Ungültige Kampagnen-Daten");
            }

            return await _apiClient.CreateCampaignAsync(request.Name, request.Description, request.CreatedBy);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Erstellen der Kampagne: {CampaignName}", request.Name);
            throw;
        }
    }

    /// <summary>
    /// Aktualisiert eine bestehende Kampagne
    /// </summary>
    /// <param name="request">Kampagnen-Update-Anfrage</param>
    /// <returns>Task</returns>
    public async Task UpdateCampaignAsync(UpdateCampaignRequest request)
    {
        try
        {
            _logger.LogInformation("Aktualisiere Kampagne {CampaignId}: {CampaignName}", request.Id, request.Name);
            await _apiClient.UpdateCampaignAsync(request);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Aktualisieren der Kampagne {CampaignId}", request.Id);
            throw;
        }
    }

    /// <summary>
    /// Löscht eine Kampagne
    /// </summary>
    /// <param name="id">Kampagnen-ID</param>
    /// <returns>Task</returns>
    public async Task DeleteCampaignAsync(int id)
    {
        try
        {
            _logger.LogInformation("Lösche Kampagne {CampaignId}", id);
            await _apiClient.DeleteCampaignAsync(id);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Löschen der Kampagne {CampaignId}", id);
            throw;
        }
    }

    /// <summary>
    /// Validiert Kampagnen-Daten
    /// </summary>
    /// <param name="request">Kampagnen-Anfrage</param>
    /// <returns>True wenn gültig</returns>
    public async Task<bool> ValidateCampaignDataAsync(CreateCampaignRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            _logger.LogWarning("Kampagnen-Name ist leer");
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            _logger.LogWarning("Kampagnen-Beschreibung ist leer");
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.CreatedBy))
        {
            _logger.LogWarning("Kampagnen-Ersteller ist leer");
            return false;
        }

        // Prüfe ob Name bereits existiert
        var existingCampaigns = await _apiClient.GetActiveCampaignsAsync();
        if (existingCampaigns.Any(c => c.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogWarning("Kampagnen-Name '{CampaignName}' existiert bereits", request.Name);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Lädt Kampagne mit QR-Codes
    /// </summary>
    /// <param name="id">Kampagnen-ID</param>
    /// <returns>Kampagne mit QR-Codes oder null</returns>
    public async Task<Campaign?> GetCampaignWithQrCodesAsync(int id)
    {
        try
        {
            _logger.LogInformation("Lade Kampagne {CampaignId} mit QR-Codes", id);
            var campaign = await _apiClient.GetCampaignByIdAsync(id);
            if (campaign != null)
            {
                var qrCodes = await _apiClient.GetQrCodesByCampaignIdAsync(id);
                // Note: QR-Codes werden nicht direkt an Campaign angehängt, da es eine separate Entität ist
                // Die QR-Codes werden separat geladen und in der View kombiniert
            }
            return campaign;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der Kampagne {CampaignId} mit QR-Codes", id);
            throw;
        }
    }

    /// <summary>
    /// Lädt Kampagne mit Statistiken
    /// </summary>
    /// <param name="id">Kampagnen-ID</param>
    /// <returns>Kampagne mit Statistiken oder null</returns>
    public async Task<Campaign?> GetCampaignWithStatisticsAsync(int id)
    {
        try
        {
            _logger.LogInformation("Lade Kampagne {CampaignId} mit Statistiken", id);
            var campaign = await _apiClient.GetCampaignByIdAsync(id);
            if (campaign != null)
            {
                var qrCodes = await _apiClient.GetQrCodesByCampaignIdAsync(id);
                // Note: Statistiken werden nicht direkt an Campaign angehängt, da es eine separate Entität ist
                // Die Statistiken werden separat geladen und in der View kombiniert
            }
            return campaign;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der Kampagne {CampaignId} mit Statistiken", id);
            throw;
        }
    }
}
