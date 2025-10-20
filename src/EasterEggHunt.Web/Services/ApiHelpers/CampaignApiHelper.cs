using System.Text;
using System.Text.Json;
using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Web.Models;

namespace EasterEggHunt.Web.Services.ApiHelpers;

/// <summary>
/// Interne Helper-Klasse für Campaign API-Operationen
/// </summary>
internal class CampaignApiHelper
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public CampaignApiHelper(HttpClient httpClient, ILogger logger, JsonSerializerOptions jsonOptions)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonOptions = jsonOptions ?? throw new ArgumentNullException(nameof(jsonOptions));
    }

    internal async Task<IEnumerable<Campaign>> GetActiveCampaignsAsync()
    {
        try
        {
            _logger.LogDebug("API-Aufruf: GET /api/campaigns/active");
            var response = await _httpClient.GetAsync(new Uri("/api/campaigns/active", UriKind.Relative));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<Campaign>>(content, _jsonOptions) ?? Enumerable.Empty<Campaign>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der aktiven Kampagnen");
            throw;
        }
    }

    internal async Task<Campaign?> GetCampaignByIdAsync(int id)
    {
        try
        {
            _logger.LogDebug("API-Aufruf: GET /api/campaigns/{CampaignId}", id);
            var response = await _httpClient.GetAsync(new Uri($"/api/campaigns/{id}", UriKind.Relative));

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Campaign>(content, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der Kampagne mit ID {CampaignId}", id);
            throw;
        }
    }

    internal async Task<Campaign> CreateCampaignAsync(string name, string description, string createdBy)
    {
        try
        {
            _logger.LogDebug("API-Aufruf: POST /api/campaigns");
            var request = new CreateCampaignRequest
            {
                Name = name,
                Description = description,
                CreatedBy = createdBy
            };

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(new Uri("/api/campaigns", UriKind.Relative), content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Campaign>(responseContent, _jsonOptions) ?? throw new InvalidOperationException("API gab keine Kampagne zurück");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Erstellen der Kampagne {CampaignName}", name);
            throw;
        }
    }

    internal async Task UpdateCampaignAsync(UpdateCampaignRequest request)
    {
        try
        {
            _logger.LogDebug("API-Aufruf: PUT /api/campaigns/{CampaignId}", request.Id);
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(new Uri($"/api/campaigns/{request.Id}", UriKind.Relative), content);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Aktualisieren der Kampagne {CampaignId}", request.Id);
            throw;
        }
    }

    internal async Task DeleteCampaignAsync(int id)
    {
        try
        {
            _logger.LogDebug("API-Aufruf: DELETE /api/campaigns/{CampaignId}", id);
            var response = await _httpClient.DeleteAsync(new Uri($"/api/campaigns/{id}", UriKind.Relative));
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Löschen der Kampagne {CampaignId}", id);
            throw;
        }
    }
}
