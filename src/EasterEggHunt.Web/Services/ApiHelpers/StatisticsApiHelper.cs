using System.Text.Json;
using EasterEggHunt.Web.Models;

namespace EasterEggHunt.Web.Services.ApiHelpers;

/// <summary>
/// Interne Helper-Klasse für Statistics API-Operationen
/// </summary>
internal class StatisticsApiHelper
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public StatisticsApiHelper(HttpClient httpClient, ILogger logger, JsonSerializerOptions jsonOptions)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonOptions = jsonOptions ?? throw new ArgumentNullException(nameof(jsonOptions));
    }

    internal async Task<QrCodeStatisticsViewModel> GetQrCodeStatisticsAsync(int qrCodeId)
    {
        try
        {
            _logger.LogDebug("API-Aufruf: GET /api/statistics/qrcode/{QrCodeId}", qrCodeId);
            var response = await _httpClient.GetAsync(new Uri($"/api/statistics/qrcode/{qrCodeId}", UriKind.Relative));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<QrCodeStatisticsViewModel>(content, _jsonOptions) ?? throw new InvalidOperationException("API gab keine QR-Code-Statistiken zurück");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der QR-Code-Statistiken für {QrCodeId}", qrCodeId);
            throw;
        }
    }

    internal async Task<CampaignQrCodeStatisticsViewModel> GetCampaignQrCodeStatisticsAsync(int campaignId)
    {
        try
        {
            _logger.LogDebug("API-Aufruf: GET /api/statistics/campaign/{CampaignId}", campaignId);
            var response = await _httpClient.GetAsync(new Uri($"/api/statistics/campaign/{campaignId}", UriKind.Relative));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CampaignQrCodeStatisticsViewModel>(content, _jsonOptions) ?? throw new InvalidOperationException("API gab keine Kampagnen-Statistiken zurück");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der Kampagnen-Statistiken für {CampaignId}", campaignId);
            throw;
        }
    }

    internal async Task<Models.TopPerformersStatisticsViewModel> GetTopPerformersAsync()
    {
        try
        {
            _logger.LogDebug("API-Aufruf: GET /api/statistics/top-performers");
            var response = await _httpClient.GetAsync(new Uri("/api/statistics/top-performers", UriKind.Relative));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Models.TopPerformersStatisticsViewModel>(content, _jsonOptions) ?? throw new InvalidOperationException("API gab keine Top-Performer-Statistiken zurück");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der Top-Performer-Statistiken");
            throw;
        }
    }
}
