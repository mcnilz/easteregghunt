using System.Text;
using System.Text.Json;
using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Web.Models;

namespace EasterEggHunt.Web.Services;

/// <summary>
/// Service für API-Kommunikation vom Web-Projekt
/// </summary>
public interface IEasterEggHuntApiClient
{
    // Campaign Operations
    Task<IEnumerable<Campaign>> GetActiveCampaignsAsync();
    Task<Campaign?> GetCampaignByIdAsync(int id);
    Task<Campaign> CreateCampaignAsync(string name, string description, string createdBy);

    // QR-Code Operations
    Task<IEnumerable<QrCode>> GetQrCodesByCampaignIdAsync(int campaignId);
    Task<QrCode?> GetQrCodeByIdAsync(int id);
    Task<QrCode> CreateQrCodeAsync(CreateQrCodeRequest request);
    Task UpdateQrCodeAsync(UpdateQrCodeRequest request);
    Task DeleteQrCodeAsync(int id);

    // User Operations
    Task<IEnumerable<User>> GetActiveUsersAsync();

    // Find Operations
    Task<IEnumerable<Find>> GetFindsByQrCodeIdAsync(int qrCodeId);
    Task<int> GetFindCountByUserIdAsync(int userId);

    // Authentication Operations
    Task<LoginResponse?> LoginAsync(string username, string password, bool rememberMe = false);
    Task LogoutAsync();
}

/// <summary>
/// HTTP-Client Service für EasterEggHunt API
/// </summary>
public class EasterEggHuntApiClient : IEasterEggHuntApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EasterEggHuntApiClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public EasterEggHuntApiClient(HttpClient httpClient, ILogger<EasterEggHuntApiClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    #region Campaign Operations

    public async Task<IEnumerable<Campaign>> GetActiveCampaignsAsync()
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

    public async Task<Campaign?> GetCampaignByIdAsync(int id)
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

    public async Task<Campaign> CreateCampaignAsync(string name, string description, string createdBy)
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

    #endregion

    #region QR-Code Operations

    public async Task<IEnumerable<QrCode>> GetQrCodesByCampaignIdAsync(int campaignId)
    {
        try
        {
            _logger.LogDebug("API-Aufruf: GET /api/qrcodes/campaign/{CampaignId}", campaignId);
            var response = await _httpClient.GetAsync(new Uri($"/api/qrcodes/campaign/{campaignId}", UriKind.Relative));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IEnumerable<QrCode>>(content, _jsonOptions) ?? Enumerable.Empty<QrCode>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der QR-Codes für Kampagne {CampaignId}", campaignId);
            throw;
        }
    }

    public async Task<QrCode?> GetQrCodeByIdAsync(int id)
    {
        try
        {
            _logger.LogDebug("API-Aufruf: GET /api/qrcodes/{QrCodeId}", id);
            var response = await _httpClient.GetAsync(new Uri($"/api/qrcodes/{id}", UriKind.Relative));

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<QrCode>(content, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen des QR-Codes mit ID {QrCodeId}", id);
            throw;
        }
    }

    public async Task<QrCode> CreateQrCodeAsync(CreateQrCodeRequest request)
    {
        try
        {
            _logger.LogDebug("API-Aufruf: POST /api/qrcodes");
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(new Uri("/api/qrcodes", UriKind.Relative), content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<QrCode>(responseContent, _jsonOptions) ?? throw new InvalidOperationException("API gab keinen QR-Code zurück");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Erstellen des QR-Codes für Kampagne {CampaignId}", request.CampaignId);
            throw;
        }
    }

    public async Task UpdateQrCodeAsync(UpdateQrCodeRequest request)
    {
        try
        {
            _logger.LogDebug("API-Aufruf: PUT /api/qrcodes/{QrCodeId}", request.Id);
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(new Uri($"/api/qrcodes/{request.Id}", UriKind.Relative), content);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException($"QR-Code mit ID {request.Id} nicht gefunden");
            }

            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Aktualisieren des QR-Codes mit ID {QrCodeId}", request.Id);
            throw;
        }
    }

    public async Task DeleteQrCodeAsync(int id)
    {
        try
        {
            _logger.LogDebug("API-Aufruf: DELETE /api/qrcodes/{QrCodeId}", id);
            var response = await _httpClient.DeleteAsync(new Uri($"/api/qrcodes/{id}", UriKind.Relative));

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException($"QR-Code mit ID {id} nicht gefunden");
            }

            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Löschen des QR-Codes mit ID {QrCodeId}", id);
            throw;
        }
    }

    #endregion

    #region User Operations

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
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

    #endregion

    #region Find Operations

    public async Task<IEnumerable<Find>> GetFindsByQrCodeIdAsync(int qrCodeId)
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

    public async Task<int> GetFindCountByUserIdAsync(int userId)
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

    #endregion

    #region Authentication Operations

    public async Task<LoginResponse?> LoginAsync(string username, string password, bool rememberMe = false)
    {
        try
        {
            _logger.LogDebug("API-Aufruf: POST /api/auth/login");
            var request = new LoginRequest
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
        catch (HttpRequestException ex) when (ex.Message.Contains("401", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Login fehlgeschlagen für Benutzer: {Username}", username);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Login für Benutzer: {Username}", username);
            throw;
        }
    }

    public async Task LogoutAsync()
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

    #endregion
}

/// <summary>
/// Request-Model für Kampagnen-Erstellung
/// </summary>
public class CreateCampaignRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
}
