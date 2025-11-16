using System.Net;
using System.Text.Json;
using EasterEggHunterApi.Abstractions.Models.Auth;
using LoginRequest = EasterEggHunterApi.Abstractions.Models.Auth.LoginRequest;

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
            var request = new LoginRequest() { Username = username, Password = password, RememberMe = rememberMe };

            var response = await _httpClient.PostAsJsonAsync(
                new Uri("/api/auth/login", UriKind.Relative), request, _jsonOptions);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);
            return result;
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
