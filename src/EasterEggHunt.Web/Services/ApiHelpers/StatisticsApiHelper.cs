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
        _logger.LogDebug("API-Aufruf: GET /api/statistics/qrcode/{QrCodeId}", qrCodeId);
        var result = await _httpClient.GetFromJsonAsync<QrCodeStatisticsViewModel>(
            new Uri($"/api/statistics/qrcode/{qrCodeId}", UriKind.Relative), _jsonOptions);
        return result ?? throw new InvalidOperationException("API gab keine QR-Code-Statistiken zurück");
    }

    internal async Task<CampaignQrCodeStatisticsViewModel> GetCampaignQrCodeStatisticsAsync(int campaignId)
    {
        _logger.LogDebug("API-Aufruf: GET /api/statistics/campaign/{CampaignId}", campaignId);
        var result = await _httpClient.GetFromJsonAsync<CampaignQrCodeStatisticsViewModel>(
            new Uri($"/api/statistics/campaign/{campaignId}", UriKind.Relative), _jsonOptions);
        return result ?? throw new InvalidOperationException("API gab keine Kampagnen-Statistiken zurück");
    }

    internal async Task<TopPerformersStatisticsViewModel> GetTopPerformersAsync()
    {
        _logger.LogDebug("API-Aufruf: GET /api/statistics/top-performers");
        var result = await _httpClient.GetFromJsonAsync<TopPerformersStatisticsViewModel>(
            new Uri("/api/statistics/top-performers", UriKind.Relative), _jsonOptions);
        return result ?? throw new InvalidOperationException("API gab keine Top-Performer-Statistiken zurück");
    }

    internal async Task<TimeBasedStatisticsViewModel> GetTimeBasedStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var url = "/api/statistics/time-based";
        var queryParams = new List<string>();
        if (startDate.HasValue)
        {
            queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
        }
        if (endDate.HasValue)
        {
            queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");
        }
        if (queryParams.Any())
        {
            url += "?" + string.Join("&", queryParams);
        }

        _logger.LogDebug("API-Aufruf: GET {Url}", url);
        var result = await _httpClient.GetFromJsonAsync<TimeBasedStatisticsViewModel>(
            new Uri(url, UriKind.Relative), _jsonOptions);
        return result ?? throw new InvalidOperationException("API gab keine zeitbasierten Statistiken zurück");
    }

    internal async Task<FindHistoryResponseViewModel> GetFindHistoryAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? userId = null,
        int? qrCodeId = null,
        int? campaignId = null,
        int skip = 0,
        int take = 50,
        string sortBy = "FoundAt",
        string sortDirection = "desc")
    {
        var url = "/api/statistics/find-history";
        var queryParams = new List<string>();

        if (startDate.HasValue)
        {
            queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
        }
        if (endDate.HasValue)
        {
            queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");
        }
        if (userId.HasValue)
        {
            queryParams.Add($"userId={userId.Value}");
        }
        if (qrCodeId.HasValue)
        {
            queryParams.Add($"qrCodeId={qrCodeId.Value}");
        }
        if (campaignId.HasValue)
        {
            queryParams.Add($"campaignId={campaignId.Value}");
        }
        if (skip > 0)
        {
            queryParams.Add($"skip={skip}");
        }
        if (take != 50)
        {
            queryParams.Add($"take={take}");
        }
        if (sortBy != "FoundAt")
        {
            queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");
        }
        if (sortDirection != "desc")
        {
            queryParams.Add($"sortDirection={Uri.EscapeDataString(sortDirection)}");
        }

        if (queryParams.Any())
        {
            url += "?" + string.Join("&", queryParams);
        }

        _logger.LogDebug("API-Aufruf: GET {Url}", url);
        var result = await _httpClient.GetFromJsonAsync<FindHistoryResponseViewModel>(
            new Uri(url, UriKind.Relative), _jsonOptions);
        return result ?? throw new InvalidOperationException("API gab keine Fund-Historie zurück");
    }
}
