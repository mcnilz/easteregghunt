using System.Text.Json;
using EasterEggHunt.Domain.Entities;
using EasterEggHunterApi.Abstractions.Models.User;

namespace EasterEggHunt.Web.Services.ApiHelpers;

/// <summary>
/// Interne Helper-Klasse für User API-Operationen
/// </summary>
internal class UserApiHelper
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public UserApiHelper(HttpClient httpClient, ILogger logger, JsonSerializerOptions jsonOptions)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonOptions = jsonOptions ?? throw new ArgumentNullException(nameof(jsonOptions));
    }

    internal async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        _logger.LogDebug("API-Aufruf: GET /api/users/active");
        var result = await _httpClient.GetFromJsonAsync<IEnumerable<User>>(
            new Uri("/api/users/active", UriKind.Relative), _jsonOptions);
        return result ?? Enumerable.Empty<User>();
    }

    internal async Task<User> RegisterEmployeeAsync(string name)
    {
        _logger.LogDebug("API-Aufruf: POST /api/users");
        var request = new { Name = name };
        var response = await _httpClient.PostAsJsonAsync(
            new Uri("/api/users", UriKind.Relative), request, _jsonOptions);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<User>(_jsonOptions);
        return result ?? throw new InvalidOperationException("API gab keinen Benutzer zurück");
    }

    internal async Task<bool> CheckUserNameExistsAsync(string name)
    {
        _logger.LogDebug("API-Aufruf: POST /api/users/check-name");
        var request = new { Name = name };
        var response = await _httpClient.PostAsJsonAsync(
            new Uri("/api/users/check-name", UriKind.Relative), request, _jsonOptions);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CheckUserNameResponse>(_jsonOptions);
        return result?.Exists ?? false;
    }
}
