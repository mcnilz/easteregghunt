using System.ComponentModel.DataAnnotations;

namespace EasterEggHunterApi.Abstractions.Models.Campaign;

/// <summary>
/// Request-Model für Kampagnen-Aktualisierung
/// </summary>
public class UpdateCampaignRequest
{
    /// <summary>
    /// Neuer Name der Kampagne
    /// </summary>
    [Required(ErrorMessage = "Name ist erforderlich")]
    [StringLength(100, ErrorMessage = "Name darf maximal 100 Zeichen haben")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Neue Beschreibung der Kampagne
    /// </summary>
    [Required(ErrorMessage = "Beschreibung ist erforderlich")]
    [StringLength(500, ErrorMessage = "Beschreibung darf maximal 500 Zeichen haben")]
    public string Description { get; set; } = string.Empty;
}
