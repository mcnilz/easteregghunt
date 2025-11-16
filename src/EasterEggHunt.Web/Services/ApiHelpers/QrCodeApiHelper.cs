using System.Net;
using System.Text.Json;
using EasterEggHunt.Domain.Entities;
using EasterEggHunterApi.Abstractions.Models.QrCode;

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
        _logger.LogDebug("API-Aufruf: GET /api/qrcodes/campaign/{CampaignId}", campaignId);
        var result = await _httpClient.GetFromJsonAsync<IEnumerable<QrCode>>(
            new Uri($"/api/qrcodes/campaign/{campaignId}", UriKind.Relative), _jsonOptions);
        return result ?? Enumerable.Empty<QrCode>();
    }

    internal async Task<QrCode?> GetQrCodeByIdAsync(int id)
    {
        _logger.LogDebug("API-Aufruf: GET /api/qrcodes/{QrCodeId}", id);
        var response = await _httpClient.GetAsync(new Uri($"/api/qrcodes/{id}", UriKind.Relative));

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<QrCode>(_jsonOptions);
        return result;
    }

    internal async Task<QrCode?> GetQrCodeByCodeAsync(string code)
    {
        _logger.LogDebug("API-Aufruf: GET /api/qrcodes/by-code/{Code}", code);
        var response = await _httpClient.GetAsync(new Uri($"/api/qrcodes/by-code/{Uri.EscapeDataString(code)}", UriKind.Relative));

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<QrCode>(_jsonOptions);
        return result;
    }

    internal async Task<QrCode> CreateQrCodeAsync(CreateQrCodeRequest request)
    {
        _logger.LogDebug("API-Aufruf: POST /api/qrcodes");
        var response = await _httpClient.PostAsJsonAsync(
            new Uri("/api/qrcodes", UriKind.Relative), request, _jsonOptions);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<QrCode>(_jsonOptions);
        return result ?? throw new InvalidOperationException("API gab keinen QR-Code zurück");
    }

    internal async Task UpdateQrCodeAsync(int id, UpdateQrCodeRequest request)
    {
        _logger.LogDebug("API-Aufruf: PUT /api/qrcodes/{QrCodeId}", id);
        var response = await _httpClient.PutAsJsonAsync(
            new Uri($"/api/qrcodes/{id}", UriKind.Relative), request, _jsonOptions);
        response.EnsureSuccessStatusCode();
    }

    internal async Task DeleteQrCodeAsync(int id)
    {
        _logger.LogDebug("API-Aufruf: DELETE /api/qrcodes/{QrCodeId}", id);
        var response = await _httpClient.DeleteAsync(new Uri($"/api/qrcodes/{id}", UriKind.Relative));
        response.EnsureSuccessStatusCode();
    }
}
