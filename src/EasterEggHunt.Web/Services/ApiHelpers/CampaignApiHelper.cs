using System.Net.Http.Json;
using System.Text.Json;
using EasterEggHunt.Domain.Entities;
using EasterEggHunterApi.Abstractions.Models.Campaign;

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
        _logger.LogDebug("API-Aufruf: GET /api/campaigns/active");
        var result = await _httpClient.GetFromJsonAsync<IEnumerable<Campaign>>(
            new Uri("/api/campaigns/active", UriKind.Relative), _jsonOptions);
        return result ?? Enumerable.Empty<Campaign>();
    }

    internal async Task<Campaign?> GetCampaignByIdAsync(int id)
    {
        _logger.LogDebug("API-Aufruf: GET /api/campaigns/{CampaignId}", id);
        var response = await _httpClient.GetAsync(new Uri($"/api/campaigns/{id}", UriKind.Relative));

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<Campaign>(_jsonOptions);
        return result;
    }

    internal async Task<Campaign> CreateCampaignAsync(string name, string description, string createdBy)
    {
        _logger.LogDebug("API-Aufruf: POST /api/campaigns");
        var request = new CreateCampaignRequest
        {
            Name = name,
            Description = description,
            CreatedBy = createdBy
        };

        var response = await _httpClient.PostAsJsonAsync(
            new Uri("/api/campaigns", UriKind.Relative), request, _jsonOptions);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<Campaign>(_jsonOptions);
        return result ?? throw new InvalidOperationException("API gab keine Kampagne zurück");
    }

    internal async Task UpdateCampaignAsync(int id, UpdateCampaignRequest request)
    {
        _logger.LogDebug("API-Aufruf: PUT /api/campaigns/{CampaignId}", id);
        var response = await _httpClient.PutAsJsonAsync(
            new Uri($"/api/campaigns/{id}", UriKind.Relative), request, _jsonOptions);
        response.EnsureSuccessStatusCode();
    }

    internal async Task DeleteCampaignAsync(int id)
    {
        _logger.LogDebug("API-Aufruf: DELETE /api/campaigns/{CampaignId}", id);
        var response = await _httpClient.DeleteAsync(new Uri($"/api/campaigns/{id}", UriKind.Relative));
        response.EnsureSuccessStatusCode();
    }
}
