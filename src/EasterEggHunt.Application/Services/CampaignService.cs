using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace EasterEggHunt.Application.Services;

/// <summary>
/// Service für Campaign-Operationen
/// </summary>
public class CampaignService : ICampaignService
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly ILogger<CampaignService> _logger;

    public CampaignService(ICampaignRepository campaignRepository, ILogger<CampaignService> logger)
    {
        _campaignRepository = campaignRepository ?? throw new ArgumentNullException(nameof(campaignRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Campaign>> GetActiveCampaignsAsync()
    {
        _logger.LogInformation("Abrufen aller aktiven Kampagnen");
        return await _campaignRepository.GetActiveAsync();
    }

    /// <inheritdoc />
    public async Task<Campaign?> GetCampaignByIdAsync(int id)
    {
        _logger.LogInformation("Abrufen der Kampagne mit ID {CampaignId}", id);
        return await _campaignRepository.GetByIdAsync(id);
    }

    /// <inheritdoc />
    public async Task<Campaign> CreateCampaignAsync(string name, string description, string createdBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Kampagnenname darf nicht leer sein", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Kampagnenbeschreibung darf nicht leer sein", nameof(description));

        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("Erstellt von darf nicht leer sein", nameof(createdBy));

        _logger.LogInformation("Erstellen einer neuen Kampagne: {CampaignName}", name);

        var campaign = new Campaign(name, description, createdBy);
        await _campaignRepository.AddAsync(campaign);
        await _campaignRepository.SaveChangesAsync();

        _logger.LogInformation("Kampagne erfolgreich erstellt mit ID {CampaignId}", campaign.Id);
        return campaign;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateCampaignAsync(int id, string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Kampagnenname darf nicht leer sein", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Kampagnenbeschreibung darf nicht leer sein", nameof(description));

        _logger.LogInformation("Aktualisieren der Kampagne mit ID {CampaignId}", id);

        var campaign = await _campaignRepository.GetByIdAsync(id);
        if (campaign == null)
        {
            _logger.LogWarning("Kampagne mit ID {CampaignId} nicht gefunden", id);
            return false;
        }

        campaign.Update(name, description);
        await _campaignRepository.SaveChangesAsync();

        _logger.LogInformation("Kampagne mit ID {CampaignId} erfolgreich aktualisiert", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeactivateCampaignAsync(int id)
    {
        _logger.LogInformation("Deaktivieren der Kampagne mit ID {CampaignId}", id);

        var campaign = await _campaignRepository.GetByIdAsync(id);
        if (campaign == null)
        {
            _logger.LogWarning("Kampagne mit ID {CampaignId} nicht gefunden", id);
            return false;
        }

        campaign.Deactivate();
        await _campaignRepository.SaveChangesAsync();

        _logger.LogInformation("Kampagne mit ID {CampaignId} erfolgreich deaktiviert", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ActivateCampaignAsync(int id)
    {
        _logger.LogInformation("Aktivieren der Kampagne mit ID {CampaignId}", id);

        var campaign = await _campaignRepository.GetByIdAsync(id);
        if (campaign == null)
        {
            _logger.LogWarning("Kampagne mit ID {CampaignId} nicht gefunden", id);
            return false;
        }

        campaign.Activate();
        await _campaignRepository.SaveChangesAsync();

        _logger.LogInformation("Kampagne mit ID {CampaignId} erfolgreich aktiviert", id);
        return true;
    }
}
