using EasterEggHunt.Domain.Entities;

namespace EasterEggHunt.Application.Services;

/// <summary>
/// Service Interface für Campaign-Operationen
/// </summary>
public interface ICampaignService
{
    /// <summary>
    /// Ruft alle aktiven Kampagnen ab
    /// </summary>
    /// <returns>Liste aller aktiven Kampagnen</returns>
    Task<IEnumerable<Campaign>> GetActiveCampaignsAsync();

    /// <summary>
    /// Ruft eine Kampagne anhand der ID ab
    /// </summary>
    /// <param name="id">Kampagnen-ID</param>
    /// <returns>Kampagne oder null wenn nicht gefunden</returns>
    Task<Campaign?> GetCampaignByIdAsync(int id);

    /// <summary>
    /// Erstellt eine neue Kampagne
    /// </summary>
    /// <param name="name">Kampagnenname</param>
    /// <param name="description">Kampagnenbeschreibung</param>
    /// <param name="createdBy">Erstellt von</param>
    /// <returns>Erstellte Kampagne</returns>
    Task<Campaign> CreateCampaignAsync(string name, string description, string createdBy);

    /// <summary>
    /// Aktualisiert eine bestehende Kampagne
    /// </summary>
    /// <param name="id">Kampagnen-ID</param>
    /// <param name="name">Neuer Name</param>
    /// <param name="description">Neue Beschreibung</param>
    /// <returns>True wenn erfolgreich aktualisiert</returns>
    Task<bool> UpdateCampaignAsync(int id, string name, string description);

    /// <summary>
    /// Deaktiviert eine Kampagne
    /// </summary>
    /// <param name="id">Kampagnen-ID</param>
    /// <returns>True wenn erfolgreich deaktiviert</returns>
    Task<bool> DeactivateCampaignAsync(int id);

    /// <summary>
    /// Aktiviert eine Kampagne
    /// </summary>
    /// <param name="id">Kampagnen-ID</param>
    /// <returns>True wenn erfolgreich aktiviert</returns>
    Task<bool> ActivateCampaignAsync(int id);
}
