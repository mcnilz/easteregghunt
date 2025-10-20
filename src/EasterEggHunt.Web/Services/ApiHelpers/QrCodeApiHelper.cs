using System.Text;
using System.Text.Json;
using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Web.Models;

namespace EasterEggHunt.Web.Services.ApiHelpers;

/// <summary>
/// Interne Helper-Klasse für QR-Code API-Operationen
/// </summary>
internal class QrCodeApiHelper
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public QrCodeApiHelper(HttpClient httpClient, ILogger logger, JsonSerializerOptions jsonOptions)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonOptions = jsonOptions ?? throw new ArgumentNullException(nameof(jsonOptions));
    }

    internal async Task<IEnumerable<QrCode>> GetQrCodesByCampaignIdAsync(int campaignId)
    {
        try
        {
            _logger.LogDebug("API-Aufruf: GET /api/qrcodes/campaign/{CampaignId}", campaignId);
            var response = await _httpClient.GetAsync(new Uri($"/api/qrcodes/campaign/{campaignId}", UriKind.Relative));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<QrCode>>(content, _jsonOptions) ?? Enumerable.Empty<QrCode>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der QR-Codes für Kampagne {CampaignId}", campaignId);
            throw;
        }
    }

    internal async Task<QrCode?> GetQrCodeByIdAsync(int id)
    {
        try
        {
            _logger.LogDebug("API-Aufruf: GET /api/qrcodes/{QrCodeId}", id);
            var response = await _httpClient.GetAsync(new Uri($"/api/qrcodes/{id}", UriKind.Relative));

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<QrCode>(content, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen des QR-Codes mit ID {QrCodeId}", id);
            throw;
        }
    }

    internal async Task<QrCode?> GetQrCodeByCodeAsync(string code)
    {
        try
        {
            _logger.LogDebug("API-Aufruf: GET /api/qrcodes/code/{Code}", code);
            var response = await _httpClient.GetAsync(new Uri($"/api/qrcodes/code/{Uri.EscapeDataString(code)}", UriKind.Relative));

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<QrCode>(content, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen des QR-Codes mit Code {Code}", code);
            throw;
        }
    }

    internal async Task<QrCode> CreateQrCodeAsync(CreateQrCodeRequest request)
    {
        try
        {
            _logger.LogDebug("API-Aufruf: POST /api/qrcodes");
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(new Uri("/api/qrcodes", UriKind.Relative), content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<QrCode>(responseContent, _jsonOptions) ?? throw new InvalidOperationException("API gab keinen QR-Code zurück");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Erstellen des QR-Codes {QrCodeTitle}", request.Title);
            throw;
        }
    }

    internal async Task UpdateQrCodeAsync(UpdateQrCodeRequest request)
    {
        try
        {
            _logger.LogDebug("API-Aufruf: PUT /api/qrcodes/{QrCodeId}", request.Id);
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(new Uri($"/api/qrcodes/{request.Id}", UriKind.Relative), content);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Aktualisieren des QR-Codes {QrCodeId}", request.Id);
            throw;
        }
    }

    internal async Task DeleteQrCodeAsync(int id)
    {
        try
        {
            _logger.LogDebug("API-Aufruf: DELETE /api/qrcodes/{QrCodeId}", id);
            var response = await _httpClient.DeleteAsync(new Uri($"/api/qrcodes/{id}", UriKind.Relative));
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Löschen des QR-Codes {QrCodeId}", id);
            throw;
        }
    }
}
