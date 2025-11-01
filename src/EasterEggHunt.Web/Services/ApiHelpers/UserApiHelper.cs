using System.Text;
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
        try
        {
            _logger.LogDebug("API-Aufruf: GET /api/users/active");
            var response = await _httpClient.GetAsync(new Uri("/api/users/active", UriKind.Relative));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<User>>(content, _jsonOptions) ?? Enumerable.Empty<User>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der aktiven Benutzer");
            throw;
        }
    }

    internal async Task<User> RegisterEmployeeAsync(string name)
    {
        try
        {
            _logger.LogDebug("API-Aufruf: POST /api/users");
            var request = new { Name = name };
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(new Uri("/api/users", UriKind.Relative), content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<User>(responseContent, _jsonOptions) ?? throw new InvalidOperationException("API gab keinen Benutzer zurück");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Registrieren des Mitarbeiters {EmployeeName}", name);
            throw;
        }
    }

    internal async Task<bool> CheckUserNameExistsAsync(string name)
    {
        try
        {
            _logger.LogDebug("API-Aufruf: POST /api/users/check-name");
            var request = new { Name = name };
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(new Uri("/api/users/check-name", UriKind.Relative), content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObj = JsonSerializer.Deserialize<CheckUserNameResponse>(responseContent, _jsonOptions);
            return responseObj?.Exists ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Prüfen des Benutzernamens {UserName}", name);
            throw;
        }
    }
}
