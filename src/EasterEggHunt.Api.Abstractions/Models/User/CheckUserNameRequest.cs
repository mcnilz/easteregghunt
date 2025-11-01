using System.ComponentModel.DataAnnotations;

namespace EasterEggHunterApi.Abstractions.Models.User;

/// <summary>
/// Request-Model für Benutzername-Prüfung
/// </summary>
public class CheckUserNameRequest
{
    /// <summary>
    /// Name des Benutzers zum Prüfen
    /// </summary>
    [Required(ErrorMessage = "Name ist erforderlich")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name muss zwischen 2 und 100 Zeichen haben")]
    public string Name { get; set; } = string.Empty;
}

