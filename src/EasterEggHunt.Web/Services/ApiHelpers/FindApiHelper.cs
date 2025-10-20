using System.Text;
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
        try
        {
            _logger.LogDebug("API-Aufruf: GET /api/finds/qrcode/{QrCodeId}", qrCodeId);
            var response = await _httpClient.GetAsync(new Uri($"/api/finds/qrcode/{qrCodeId}", UriKind.Relative));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<Find>>(content, _jsonOptions) ?? Enumerable.Empty<Find>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der Funde für QR-Code {QrCodeId}", qrCodeId);
            throw;
        }
    }

    internal async Task<int> GetFindCountByUserIdAsync(int userId)
    {
        try
        {
            _logger.LogDebug("API-Aufruf: GET /api/finds/user/{UserId}/count", userId);
            var response = await _httpClient.GetAsync(new Uri($"/api/finds/user/{userId}/count", UriKind.Relative));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<int>(content, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der Fund-Anzahl für Benutzer {UserId}", userId);
            throw;
        }
    }

    internal async Task<Find> RegisterFindAsync(int qrCodeId, int userId, string ipAddress, string userAgent)
    {
        try
        {
            _logger.LogDebug("API-Aufruf: POST /api/finds");
            var request = new
            {
                QrCodeId = qrCodeId,
                UserId = userId,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(new Uri("/api/finds", UriKind.Relative), content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Find>(responseContent, _jsonOptions) ?? throw new InvalidOperationException("API gab keinen Fund zurück");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Registrieren des Funds für QR-Code {QrCodeId} und Benutzer {UserId}", qrCodeId, userId);
            throw;
        }
    }

    internal async Task<Find?> GetExistingFindAsync(int qrCodeId, int userId)
    {
        try
        {
            _logger.LogDebug("API-Aufruf: GET /api/finds/check?qrCodeId={QrCodeId}&userId={UserId}", qrCodeId, userId);
            var response = await _httpClient.GetAsync(new Uri($"/api/finds/check?qrCodeId={qrCodeId}&userId={userId}", UriKind.Relative));

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Find>(content, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen des bestehenden Funds für QR-Code {QrCodeId} und Benutzer {UserId}", qrCodeId, userId);
            throw;
        }
    }

    internal async Task<IEnumerable<Find>> GetFindsByUserIdAsync(int userId)
    {
        try
        {
            _logger.LogDebug("API-Aufruf: GET /api/finds/user/{UserId}", userId);
            var response = await _httpClient.GetAsync(new Uri($"/api/finds/user/{userId}", UriKind.Relative));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<Find>>(content, _jsonOptions) ?? Enumerable.Empty<Find>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der Funde für Benutzer {UserId}", userId);
            throw;
        }
    }

    internal async Task<IEnumerable<Find>> GetFindsByUserAndCampaignAsync(int userId, int campaignId, int? take = null)
    {
        try
        {
            var url = $"/api/finds/user/{userId}/campaign/{campaignId}";
            if (take.HasValue)
            {
                url += $"?take={take.Value}";
            }

            _logger.LogDebug("API-Aufruf: GET {Url}", url);
            var response = await _httpClient.GetAsync(new Uri(url, UriKind.Relative));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<Find>>(content, _jsonOptions) ?? Enumerable.Empty<Find>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der Funde für Benutzer {UserId} und Kampagne {CampaignId}", userId, campaignId);
            throw;
        }
    }

    internal async Task<UserStatistics> GetUserStatisticsAsync(int userId)
    {
        try
        {
            _logger.LogDebug("API-Aufruf: GET /api/finds/user/{UserId}/statistics", userId);
            var response = await _httpClient.GetAsync(new Uri($"/api/finds/user/{userId}/statistics", UriKind.Relative));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UserStatistics>(content, _jsonOptions) ?? throw new InvalidOperationException("API gab keine Benutzer-Statistiken zurück");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der Statistiken für Benutzer {UserId}", userId);
            throw;
        }
    }
}
