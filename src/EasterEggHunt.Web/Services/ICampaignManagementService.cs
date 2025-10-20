using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Web.Models;

namespace EasterEggHunt.Web.Services;

/// <summary>
/// Service für Kampagnen-Management im Admin-Bereich
/// </summary>
public interface ICampaignManagementService
{
    /// <summary>
    /// Lädt alle aktiven Kampagnen
    /// </summary>
    /// <returns>Liste der aktiven Kampagnen</returns>
    Task<IEnumerable<Campaign>> GetActiveCampaignsAsync();

    /// <summary>
    /// Lädt eine Kampagne anhand der ID
    /// </summary>
    /// <param name="id">Kampagnen-ID</param>
    /// <returns>Kampagne oder null</returns>
    Task<Campaign?> GetCampaignByIdAsync(int id);

    /// <summary>
    /// Erstellt eine neue Kampagne
    /// </summary>
    /// <param name="request">Kampagnen-Erstellungsanfrage</param>
    /// <returns>Erstellte Kampagne</returns>
    Task<Campaign> CreateCampaignAsync(CreateCampaignRequest request);

    /// <summary>
    /// Aktualisiert eine bestehende Kampagne
    /// </summary>
    /// <param name="request">Kampagnen-Update-Anfrage</param>
    /// <returns>Task</returns>
    Task UpdateCampaignAsync(UpdateCampaignRequest request);

    /// <summary>
    /// Löscht eine Kampagne
    /// </summary>
    /// <param name="id">Kampagnen-ID</param>
    /// <returns>Task</returns>
    Task DeleteCampaignAsync(int id);

    /// <summary>
    /// Validiert Kampagnen-Daten
    /// </summary>
    /// <param name="request">Kampagnen-Anfrage</param>
    /// <returns>True wenn gültig</returns>
    Task<bool> ValidateCampaignDataAsync(CreateCampaignRequest request);

    /// <summary>
    /// Lädt Kampagne mit QR-Codes
    /// </summary>
    /// <param name="id">Kampagnen-ID</param>
    /// <returns>Kampagne mit QR-Codes oder null</returns>
    Task<Campaign?> GetCampaignWithQrCodesAsync(int id);

    /// <summary>
    /// Lädt Kampagne mit Statistiken
    /// </summary>
    /// <param name="id">Kampagnen-ID</param>
    /// <returns>Kampagne mit Statistiken oder null</returns>
    Task<Campaign?> GetCampaignWithStatisticsAsync(int id);
}
