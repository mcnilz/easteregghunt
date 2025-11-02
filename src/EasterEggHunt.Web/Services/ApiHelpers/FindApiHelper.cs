using System.Net.Http.Json;
using System.Text.Json;
using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Web.Models;

namespace EasterEggHunt.Web.Services.ApiHelpers;

/// <summary>
/// Interne Helper-Klasse für Find API-Operationen
/// </summary>
internal class FindApiHelper
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public FindApiHelper(HttpClient httpClient, ILogger logger, JsonSerializerOptions jsonOptions)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonOptions = jsonOptions ?? throw new ArgumentNullException(nameof(jsonOptions));
    }

    internal async Task<IEnumerable<Find>> GetFindsByQrCodeIdAsync(int qrCodeId)
    {
        _logger.LogDebug("API-Aufruf: GET /api/finds/qrcode/{QrCodeId}", qrCodeId);
        var result = await _httpClient.GetFromJsonAsync<IEnumerable<Find>>(
            new Uri($"/api/finds/qrcode/{qrCodeId}", UriKind.Relative), _jsonOptions);
        return result ?? Enumerable.Empty<Find>();
    }

    internal async Task<int> GetFindCountByUserIdAsync(int userId)
    {
        _logger.LogDebug("API-Aufruf: GET /api/finds/user/{UserId}/count", userId);
        var result = await _httpClient.GetFromJsonAsync<int>(
            new Uri($"/api/finds/user/{userId}/count", UriKind.Relative), _jsonOptions);
        return result;
    }

    internal async Task<Find> RegisterFindAsync(int qrCodeId, int userId, string ipAddress, string userAgent)
    {
        _logger.LogDebug("API-Aufruf: POST /api/finds");
        var request = new
        {
            QrCodeId = qrCodeId,
            UserId = userId,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        var response = await _httpClient.PostAsJsonAsync(
            new Uri("/api/finds", UriKind.Relative), request, _jsonOptions);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<Find>(_jsonOptions);
        return result ?? throw new InvalidOperationException("API gab keinen Fund zurück");
    }

    internal async Task<Find?> GetExistingFindAsync(int qrCodeId, int userId)
    {
        _logger.LogDebug("API-Aufruf: GET /api/finds/check?qrCodeId={QrCodeId}&userId={UserId}", qrCodeId, userId);
        var response = await _httpClient.GetAsync(new Uri($"/api/finds/check?qrCodeId={qrCodeId}&userId={userId}", UriKind.Relative));

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        // Prüfen ob Content leer oder null ist
        var content = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(content) || content.Trim() == "null")
        {
            return null;
        }

        // Da der Stream bereits gelesen wurde, müssen wir JsonSerializer.Deserialize verwenden
        var result = JsonSerializer.Deserialize<Find>(content, _jsonOptions);
        return result;
    }

    internal async Task<IEnumerable<Find>> GetFindsByUserIdAsync(int userId)
    {
        _logger.LogDebug("API-Aufruf: GET /api/finds/user/{UserId}", userId);
        var result = await _httpClient.GetFromJsonAsync<IEnumerable<Find>>(
            new Uri($"/api/finds/user/{userId}", UriKind.Relative), _jsonOptions);
        return result ?? Enumerable.Empty<Find>();
    }

    internal async Task<IEnumerable<Find>> GetFindsByUserAndCampaignAsync(int userId, int campaignId, int? take = null)
    {
        var url = $"/api/finds/user/{userId}/by-campaign?campaignId={campaignId}";
        if (take.HasValue)
        {
            url += $"&take={take.Value}";
        }

        _logger.LogDebug("API-Aufruf: GET {Url}", url);
        var result = await _httpClient.GetFromJsonAsync<IEnumerable<Find>>(
            new Uri(url, UriKind.Relative), _jsonOptions);
        return result ?? Enumerable.Empty<Find>();
    }

    internal async Task<UserStatistics> GetUserStatisticsAsync(int userId)
    {
        _logger.LogDebug("API-Aufruf: GET /api/statistics/user/{UserId}", userId);
        var result = await _httpClient.GetFromJsonAsync<UserStatistics>(
            new Uri($"/api/statistics/user/{userId}", UriKind.Relative), _jsonOptions);
        return result ?? throw new InvalidOperationException("API gab keine Benutzer-Statistiken zurück");
    }
}
