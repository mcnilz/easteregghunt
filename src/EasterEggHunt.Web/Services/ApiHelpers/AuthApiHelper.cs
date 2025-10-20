using System.Text;
using System.Text.Json;
using EasterEggHunt.Web.Models;

namespace EasterEggHunt.Web.Services.ApiHelpers;

/// <summary>
/// Interne Helper-Klasse für Authentication API-Operationen
/// </summary>
internal class AuthApiHelper
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuthApiHelper(HttpClient httpClient, ILogger logger, JsonSerializerOptions jsonOptions)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonOptions = jsonOptions ?? throw new ArgumentNullException(nameof(jsonOptions));
    }

    internal async Task<LoginResponse?> LoginAsync(string username, string password, bool rememberMe = false)
    {
        try
        {
            _logger.LogDebug("API-Aufruf: POST /api/auth/login");
            var request = new
            {
                Username = username,
                Password = password,
                RememberMe = rememberMe
            };

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(new Uri("/api/auth/login", UriKind.Relative), content);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<LoginResponse>(responseContent, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Login für Benutzer {Username}", username);
            throw;
        }
    }

    internal async Task LogoutAsync()
    {
        try
        {
            _logger.LogDebug("API-Aufruf: POST /api/auth/logout");
            var response = await _httpClient.PostAsync(new Uri("/api/auth/logout", UriKind.Relative), null);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Logout");
            throw;
        }
    }
}
