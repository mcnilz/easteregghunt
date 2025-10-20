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
    Task<QrCode?> GetQrCodeByCodeAsync(string code);
    Task<QrCode> CreateQrCodeAsync(CreateQrCodeRequest request);
    Task UpdateQrCodeAsync(UpdateQrCodeRequest request);
    Task DeleteQrCodeAsync(int id);

    // User Operations
    Task<IEnumerable<User>> GetActiveUsersAsync();
    Task<User> RegisterEmployeeAsync(string name);
    Task<bool> CheckUserNameExistsAsync(string name);

    // Find Operations
    Task<IEnumerable<Find>> GetFindsByQrCodeIdAsync(int qrCodeId);
    Task<int> GetFindCountByUserIdAsync(int userId);
    Task<Find> RegisterFindAsync(int qrCodeId, int userId, string ipAddress, string userAgent);
    Task<Find?> GetExistingFindAsync(int qrCodeId, int userId);

    // User progress
    Task<IEnumerable<Find>> GetFindsByUserIdAsync(int userId);
    Task<IEnumerable<Find>> GetFindsByUserAndCampaignAsync(int userId, int campaignId, int? take = null);
    Task<UserStatistics> GetUserStatisticsAsync(int userId);

    // Authentication Operations
    Task<LoginResponse?> LoginAsync(string username, string password, bool rememberMe = false);
    Task LogoutAsync();

    // Statistics Operations
    Task<QrCodeStatisticsViewModel> GetQrCodeStatisticsAsync(int qrCodeId);
    Task<CampaignQrCodeStatisticsViewModel> GetCampaignQrCodeStatisticsAsync(int campaignId);
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

    public async Task<QrCode?> GetQrCodeByCodeAsync(string code)
    {
        try
        {
            _logger.LogDebug("API-Aufruf: GET /api/qrcodes/by-code/{Code}", code);
            var response = await _httpClient.GetAsync(new Uri($"/api/qrcodes/by-code/{Uri.EscapeDataString(code)}", UriKind.Relative));

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
            _logger.LogError(ex, "Fehler beim Abrufen des QR-Codes mit Code {Code}", code);
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

    public async Task<User> RegisterEmployeeAsync(string name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name darf nicht leer sein", nameof(name));

            _logger.LogDebug("API-Aufruf: POST /api/users (Registrierung: {Name})", name);

            // Prüfen ob Name bereits existiert
            var nameExists = await CheckUserNameExistsAsync(name);
            if (nameExists)
            {
                throw new InvalidOperationException($"Benutzername '{name}' ist bereits vergeben");
            }

            var request = new { Name = name };
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(new Uri("/api/users", UriKind.Relative), content);

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Registrierung fehlgeschlagen: {errorContent}");
            }

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<User>(responseContent, _jsonOptions);

            if (user == null)
                throw new InvalidOperationException("Benutzer konnte nicht erstellt werden");

            _logger.LogInformation("Mitarbeiter erfolgreich registriert: {Name} (ID: {UserId})", name, user.Id);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler bei der Registrierung des Mitarbeiters {Name}", name);
            throw;
        }
    }

    public async Task<bool> CheckUserNameExistsAsync(string name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name darf nicht leer sein", nameof(name));

            _logger.LogDebug("API-Aufruf: GET /api/users/check-name/{Name}", name);
            var response = await _httpClient.GetAsync(new Uri($"/api/users/check-name/{Uri.EscapeDataString(name)}", UriKind.Relative));

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                return false;
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CheckUserNameResponse>(content, _jsonOptions);

            return result?.Exists ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Prüfen des Benutzernamens {Name}", name);
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

    public async Task<IEnumerable<Find>> GetFindsByUserIdAsync(int userId)
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

    public async Task<IEnumerable<Find>> GetFindsByUserAndCampaignAsync(int userId, int campaignId, int? take = null)
    {
        try
        {
            _logger.LogDebug("API-Aufruf: GET /api/finds/user/{UserId}/by-campaign?campaignId={CampaignId}&take={Take}", userId, campaignId, take);
            var query = $"/api/finds/user/{userId}/by-campaign?campaignId={campaignId}" + (take.HasValue ? $"&take={take.Value}" : string.Empty);
            var response = await _httpClient.GetAsync(new Uri(query, UriKind.Relative));
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

    public async Task<Find> RegisterFindAsync(int qrCodeId, int userId, string ipAddress, string userAgent)
    {
        try
        {
            _logger.LogDebug("API-Aufruf: POST /api/finds");
            var request = new RegisterFindRequest
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

    public async Task<Find?> GetExistingFindAsync(int qrCodeId, int userId)
    {
        try
        {
            _logger.LogDebug("API-Aufruf: GET /api/finds/check?qrCodeId={QrCodeId}&userId={UserId}", qrCodeId, userId);
            var response = await _httpClient.GetAsync(new Uri($"/api/finds/check?qrCodeId={qrCodeId}&userId={userId}", UriKind.Relative));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("API-Antwort: '{Content}'", content);

            // Prüfen ob die Antwort leer ist oder nur Whitespace
            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogDebug("Leere API-Antwort für QR-Code {QrCodeId} und User {UserId} - kein Fund vorhanden", qrCodeId, userId);
                return null;
            }

            // Prüfen ob die Antwort "null" ist (JSON null)
            if (content.Trim() == "null")
            {
                _logger.LogDebug("JSON null-Antwort für QR-Code {QrCodeId} und User {UserId} - kein Fund vorhanden", qrCodeId, userId);
                return null;
            }

            return JsonSerializer.Deserialize<Find?>(content, _jsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON-Deserialisierungsfehler beim Abrufen des bestehenden Funds für QR-Code {QrCodeId} und Benutzer {UserId}", qrCodeId, userId);
            return null; // Graceful fallback
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen des bestehenden Funds für QR-Code {QrCodeId} und Benutzer {UserId}", qrCodeId, userId);
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

    #region Statistics Operations

    public async Task<QrCodeStatisticsViewModel> GetQrCodeStatisticsAsync(int qrCodeId)
    {
        try
        {
            _logger.LogDebug("API-Aufruf: GET /api/statistics/qrcode/{QrCodeId}", qrCodeId);
            var response = await _httpClient.GetAsync(new Uri($"/api/statistics/qrcode/{qrCodeId}", UriKind.Relative));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var apiDto = JsonSerializer.Deserialize<QrCodeStatisticsDto>(content, _jsonOptions) ?? throw new InvalidOperationException("Deserialization failed");

            return new QrCodeStatisticsViewModel
            {
                QrCodeId = apiDto.QrCodeId,
                Title = apiDto.Title,
                CampaignId = apiDto.CampaignId,
                CampaignName = apiDto.CampaignName,
                FindCount = apiDto.FindCount,
                Finders = apiDto.Finders.Select(f => new FinderInfoViewModel
                {
                    UserId = f.UserId,
                    UserName = f.UserName,
                    FoundAt = f.FoundAt,
                    IpAddress = f.IpAddress
                }).ToList(),
                GeneratedAt = apiDto.GeneratedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der QR-Code-Statistiken für {QrCodeId}", qrCodeId);
            throw;
        }
    }

    public async Task<CampaignQrCodeStatisticsViewModel> GetCampaignQrCodeStatisticsAsync(int campaignId)
    {
        try
        {
            _logger.LogDebug("API-Aufruf: GET /api/statistics/campaign/{CampaignId}/qrcodes", campaignId);
            var response = await _httpClient.GetAsync(new Uri($"/api/statistics/campaign/{campaignId}/qrcodes", UriKind.Relative));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var apiDto = JsonSerializer.Deserialize<CampaignQrCodeStatisticsDto>(content, _jsonOptions) ?? throw new InvalidOperationException("Deserialization failed");

            return new CampaignQrCodeStatisticsViewModel
            {
                CampaignId = apiDto.CampaignId,
                CampaignName = apiDto.CampaignName,
                TotalQrCodes = apiDto.TotalQrCodes,
                FoundQrCodes = apiDto.FoundQrCodes,
                TotalFinds = apiDto.TotalFinds,
                QrCodeStatistics = apiDto.QrCodeStatistics.Select(q => new QrCodeStatisticsViewModel
                {
                    QrCodeId = q.QrCodeId,
                    Title = q.Title,
                    CampaignId = q.CampaignId,
                    CampaignName = q.CampaignName,
                    FindCount = q.FindCount,
                    Finders = q.Finders.Select(f => new FinderInfoViewModel
                    {
                        UserId = f.UserId,
                        UserName = f.UserName,
                        FoundAt = f.FoundAt,
                        IpAddress = f.IpAddress
                    }).ToList(),
                    GeneratedAt = q.GeneratedAt
                }).ToList(),
                GeneratedAt = apiDto.GeneratedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der Kampagnen-QR-Code-Statistiken für {CampaignId}", campaignId);
            throw;
        }
    }

    public async Task<UserStatistics> GetUserStatisticsAsync(int userId)
    {
        try
        {
            _logger.LogDebug("API-Aufruf: GET /api/statistics/user/{UserId}", userId);
            var response = await _httpClient.GetAsync(new Uri($"/api/statistics/user/{userId}", UriKind.Relative));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var stats = JsonSerializer.Deserialize<UserStatistics>(content, _jsonOptions) ?? throw new InvalidOperationException("Deserialization failed");
            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Abrufen der Benutzer-Statistiken für {UserId}", userId);
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

/// <summary>
/// Request-Model für Fund-Registrierung
/// </summary>
public class RegisterFindRequest
{
    public int QrCodeId { get; set; }
    public int UserId { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}
