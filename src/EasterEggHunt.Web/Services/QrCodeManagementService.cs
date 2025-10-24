using EasterEggHunt.Domain.Entities;
using EasterEggHunterApi.Abstractions.Models.QrCode;

namespace EasterEggHunt.Web.Services;

/// <summary>
/// Service für QR-Code-Management im Admin-Bereich
/// </summary>
public class QrCodeManagementService : IQrCodeManagementService
{
    private readonly IEasterEggHuntApiClient _apiClient;
    private readonly ILogger<QrCodeManagementService> _logger;

    public QrCodeManagementService(
        IEasterEggHuntApiClient apiClient,
        ILogger<QrCodeManagementService> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Lädt QR-Codes für eine Kampagne
    /// </summary>
    /// <param name="campaignId">Kampagnen-ID</param>
    /// <returns>Liste der QR-Codes</returns>
    public async Task<IEnumerable<QrCode>> GetQrCodesByCampaignAsync(int campaignId)
    {
        try
        {
            _logger.LogInformation("Lade QR-Codes für Kampagne {CampaignId}", campaignId);
            return await _apiClient.GetQrCodesByCampaignIdAsync(campaignId);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden der QR-Codes für Kampagne {CampaignId}", campaignId);
            throw;
        }
    }

    /// <summary>
    /// Lädt einen QR-Code anhand der ID
    /// </summary>
    /// <param name="id">QR-Code-ID</param>
    /// <returns>QR-Code oder null</returns>
    public async Task<QrCode?> GetQrCodeByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Lade QR-Code mit ID {QrCodeId}", id);
            return await _apiClient.GetQrCodeByIdAsync(id);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden des QR-Codes {QrCodeId}", id);
            throw;
        }
    }

    /// <summary>
    /// Erstellt einen neuen QR-Code
    /// </summary>
    /// <param name="request">QR-Code-Erstellungsanfrage</param>
    /// <returns>Erstellter QR-Code</returns>
    public async Task<QrCode> CreateQrCodeAsync(CreateQrCodeRequest request)
    {
        try
        {
            _logger.LogInformation("Erstelle neuen QR-Code: {QrCodeTitle}", request.Title);
            return await _apiClient.CreateQrCodeAsync(request);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Erstellen des QR-Codes: {QrCodeTitle}", request.Title);
            throw;
        }
    }

    /// <summary>
    /// Aktualisiert einen bestehenden QR-Code
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request">QR-Code-Update-Anfrage</param>
    /// <returns>Task</returns>
    public async Task UpdateQrCodeAsync(int id, UpdateQrCodeRequest request)
    {
        try
        {
            _logger.LogInformation("Aktualisiere QR-Code {QrCodeId}: {QrCodeTitle}", id, request.Title);
            await _apiClient.UpdateQrCodeAsync(id, request);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Aktualisieren des QR-Codes {QrCodeId}", id);
            throw;
        }
    }

    /// <summary>
    /// Löscht einen QR-Code
    /// </summary>
    /// <param name="id">QR-Code-ID</param>
    /// <returns>Task</returns>
    public async Task DeleteQrCodeAsync(int id)
    {
        try
        {
            _logger.LogInformation("Lösche QR-Code {QrCodeId}", id);
            await _apiClient.DeleteQrCodeAsync(id);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Löschen des QR-Codes {QrCodeId}", id);
            throw;
        }
    }

    /// <summary>
    /// Sortiert QR-Codes neu
    /// </summary>
    /// <param name="campaignId">Kampagnen-ID</param>
    /// <param name="qrCodeIds">Sortierte Liste der QR-Code-IDs</param>
    /// <returns>Task</returns>
    public async Task ReorderQrCodesAsync(int campaignId, IEnumerable<int> qrCodeIds)
    {
        try
        {
            _logger.LogInformation("Sortiere QR-Codes für Kampagne {CampaignId} neu", campaignId);

            var qrCodeIdsList = qrCodeIds.ToList();
            for (int i = 0; i < qrCodeIdsList.Count; i++)
            {
                var qrCodeId = qrCodeIdsList[i];
                var qrCode = await _apiClient.GetQrCodeByIdAsync(qrCodeId);
                if (qrCode != null)
                {
                    var updateRequest = new UpdateQrCodeRequest
                    {

                        Title = qrCode.Title,
                        Description = qrCode.Description,
                        InternalNotes = qrCode.InternalNotes,
                    };
                    await _apiClient.UpdateQrCodeAsync(qrCodeId, updateRequest);
                }
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Neusortieren der QR-Codes für Kampagne {CampaignId}", campaignId);
            throw;
        }
    }

    /// <summary>
    /// Validiert QR-Code-Daten
    /// </summary>
    /// <param name="request">QR-Code-Anfrage</param>
    /// <returns>True wenn gültig</returns>
    public Task<bool> ValidateQrCodeDataAsync(CreateQrCodeRequest request)
    {
        // Basic validation, more complex validation might be in API
        return Task.FromResult(!string.IsNullOrWhiteSpace(request.Title) &&
                              !string.IsNullOrWhiteSpace(request.InternalNotes));
    }

    /// <summary>
    /// Lädt QR-Code mit Statistiken
    /// </summary>
    /// <param name="id">QR-Code-ID</param>
    /// <returns>QR-Code mit Statistiken oder null</returns>
    public async Task<QrCode?> GetQrCodeWithStatisticsAsync(int id)
    {
        try
        {
            _logger.LogInformation("Lade QR-Code {QrCodeId} mit Statistiken", id);
            var qrCode = await _apiClient.GetQrCodeByIdAsync(id);
            if (qrCode != null)
            {
                var finds = await _apiClient.GetFindsByQrCodeIdAsync(id);
                // Note: Statistiken werden nicht direkt an QrCode angehängt, da es eine separate Entität ist
                // Die Statistiken werden separat geladen und in der View kombiniert
            }
            return qrCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden des QR-Codes {QrCodeId} mit Statistiken", id);
            throw;
        }
    }

    /// <summary>
    /// Lädt QR-Code mit Funden
    /// </summary>
    /// <param name="id">QR-Code-ID</param>
    /// <returns>QR-Code mit Funden oder null</returns>
    public async Task<QrCode?> GetQrCodeWithFindsAsync(int id)
    {
        try
        {
            _logger.LogInformation("Lade QR-Code {QrCodeId} mit Funden", id);
            var qrCode = await _apiClient.GetQrCodeByIdAsync(id);
            if (qrCode != null)
            {
                var finds = await _apiClient.GetFindsByQrCodeIdAsync(id);
                // Note: Funde werden nicht direkt an QrCode angehängt, da es eine separate Entität ist
                // Die Funde werden separat geladen und in der View kombiniert
            }
            return qrCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Laden des QR-Codes {QrCodeId} mit Funden", id);
            throw;
        }
    }
}
