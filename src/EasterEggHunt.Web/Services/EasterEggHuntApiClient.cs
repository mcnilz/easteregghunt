using System.Text.Json;
using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Web.Models;
using EasterEggHunt.Web.Services.ApiHelpers;
using EasterEggHunterApi.Abstractions.Models.Auth;
using EasterEggHunterApi.Abstractions.Models.Campaign;
using EasterEggHunterApi.Abstractions.Models.QrCode;

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
    Task UpdateCampaignAsync(int id, UpdateCampaignRequest request);
    Task DeleteCampaignAsync(int id);

    // QR-Code Operations
    Task<IEnumerable<QrCode>> GetQrCodesByCampaignIdAsync(int campaignId);
    Task<QrCode?> GetQrCodeByIdAsync(int id);
    Task<QrCode?> GetQrCodeByCodeAsync(string code);
    Task<QrCode> CreateQrCodeAsync(CreateQrCodeRequest request);
    Task UpdateQrCodeAsync(int id, UpdateQrCodeRequest request);
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
    Task<Models.TopPerformersStatisticsViewModel> GetTopPerformersAsync();
    Task<Models.TimeBasedStatisticsViewModel> GetTimeBasedStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
}

/// <summary>
/// HTTP-Client Service für EasterEggHunt API - Facade Pattern mit internen Helper-Klassen
/// </summary>
public class EasterEggHuntApiClient : IEasterEggHuntApiClient
{
    private readonly CampaignApiHelper _campaignHelper;
    private readonly QrCodeApiHelper _qrCodeHelper;
    private readonly UserApiHelper _userHelper;
    private readonly FindApiHelper _findHelper;
    private readonly StatisticsApiHelper _statisticsHelper;
    private readonly AuthApiHelper _authHelper;

    public EasterEggHuntApiClient(HttpClient httpClient, ILogger<EasterEggHuntApiClient> logger)
    {
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        _campaignHelper = new CampaignApiHelper(httpClient, logger, jsonOptions);
        _qrCodeHelper = new QrCodeApiHelper(httpClient, logger, jsonOptions);
        _userHelper = new UserApiHelper(httpClient, logger, jsonOptions);
        _findHelper = new FindApiHelper(httpClient, logger, jsonOptions);
        _statisticsHelper = new StatisticsApiHelper(httpClient, logger, jsonOptions);
        _authHelper = new AuthApiHelper(httpClient, logger, jsonOptions);
    }

    #region Campaign Operations

    public async Task<IEnumerable<Campaign>> GetActiveCampaignsAsync()
        => await _campaignHelper.GetActiveCampaignsAsync();

    public async Task<Campaign?> GetCampaignByIdAsync(int id)
        => await _campaignHelper.GetCampaignByIdAsync(id);

    public async Task<Campaign> CreateCampaignAsync(string name, string description, string createdBy)
        => await _campaignHelper.CreateCampaignAsync(name, description, createdBy);

    public async Task UpdateCampaignAsync(int id, UpdateCampaignRequest request)
        => await _campaignHelper.UpdateCampaignAsync(id, request);

    public async Task DeleteCampaignAsync(int id)
        => await _campaignHelper.DeleteCampaignAsync(id);

    #endregion

    #region QR-Code Operations

    public async Task<IEnumerable<QrCode>> GetQrCodesByCampaignIdAsync(int campaignId)
        => await _qrCodeHelper.GetQrCodesByCampaignIdAsync(campaignId);

    public async Task<QrCode?> GetQrCodeByIdAsync(int id)
        => await _qrCodeHelper.GetQrCodeByIdAsync(id);

    public async Task<QrCode?> GetQrCodeByCodeAsync(string code)
        => await _qrCodeHelper.GetQrCodeByCodeAsync(code);

    public async Task<QrCode> CreateQrCodeAsync(CreateQrCodeRequest request)
        => await _qrCodeHelper.CreateQrCodeAsync(request);

    public async Task UpdateQrCodeAsync(int id, UpdateQrCodeRequest request)
        => await _qrCodeHelper.UpdateQrCodeAsync(id, request);

    public async Task DeleteQrCodeAsync(int id)
        => await _qrCodeHelper.DeleteQrCodeAsync(id);

    #endregion

    #region User Operations

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
        => await _userHelper.GetActiveUsersAsync();

    public async Task<User> RegisterEmployeeAsync(string name)
        => await _userHelper.RegisterEmployeeAsync(name);

    public async Task<bool> CheckUserNameExistsAsync(string name)
        => await _userHelper.CheckUserNameExistsAsync(name);

    #endregion

    #region Find Operations

    public async Task<IEnumerable<Find>> GetFindsByQrCodeIdAsync(int qrCodeId)
        => await _findHelper.GetFindsByQrCodeIdAsync(qrCodeId);

    public async Task<int> GetFindCountByUserIdAsync(int userId)
        => await _findHelper.GetFindCountByUserIdAsync(userId);

    public async Task<Find> RegisterFindAsync(int qrCodeId, int userId, string ipAddress, string userAgent)
        => await _findHelper.RegisterFindAsync(qrCodeId, userId, ipAddress, userAgent);

    public async Task<Find?> GetExistingFindAsync(int qrCodeId, int userId)
        => await _findHelper.GetExistingFindAsync(qrCodeId, userId);

    public async Task<IEnumerable<Find>> GetFindsByUserIdAsync(int userId)
        => await _findHelper.GetFindsByUserIdAsync(userId);

    public async Task<IEnumerable<Find>> GetFindsByUserAndCampaignAsync(int userId, int campaignId, int? take = null)
        => await _findHelper.GetFindsByUserAndCampaignAsync(userId, campaignId, take);

    public async Task<UserStatistics> GetUserStatisticsAsync(int userId)
        => await _findHelper.GetUserStatisticsAsync(userId);

    #endregion

    #region Authentication Operations

    public async Task<LoginResponse?> LoginAsync(string username, string password, bool rememberMe = false)
        => await _authHelper.LoginAsync(username, password, rememberMe);

    public async Task LogoutAsync()
        => await _authHelper.LogoutAsync();

    #endregion

    #region Statistics Operations

    public async Task<QrCodeStatisticsViewModel> GetQrCodeStatisticsAsync(int qrCodeId)
        => await _statisticsHelper.GetQrCodeStatisticsAsync(qrCodeId);

    public async Task<CampaignQrCodeStatisticsViewModel> GetCampaignQrCodeStatisticsAsync(int campaignId)
        => await _statisticsHelper.GetCampaignQrCodeStatisticsAsync(campaignId);

    public async Task<Models.TopPerformersStatisticsViewModel> GetTopPerformersAsync()
        => await _statisticsHelper.GetTopPerformersAsync();

    public async Task<Models.TimeBasedStatisticsViewModel> GetTimeBasedStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        => await _statisticsHelper.GetTimeBasedStatisticsAsync(startDate, endDate);

    #endregion
}
