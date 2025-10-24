using System.ComponentModel.DataAnnotations;

namespace EasterEggHunterApi.Abstractions.Models.Campaign;

/// <summary>
/// Request-Model für Kampagnen-Erstellung
/// </summary>
public class CreateCampaignRequest
{
    /// <summary>
    /// Name der Kampagne
    /// </summary>
    [Required(ErrorMessage = "Name ist erforderlich")]
    [StringLength(100, ErrorMessage = "Name darf maximal 100 Zeichen haben")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Beschreibung der Kampagne
    /// </summary>
    [Required(ErrorMessage = "Beschreibung ist erforderlich")]
    [StringLength(500, ErrorMessage = "Beschreibung darf maximal 500 Zeichen haben")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Erstellt von (Admin-Name)
    /// </summary>
    [Required(ErrorMessage = "Erstellt von ist erforderlich")]
    [StringLength(50, ErrorMessage = "Erstellt von darf maximal 50 Zeichen haben")]
    public string CreatedBy { get; set; } = string.Empty;
}
