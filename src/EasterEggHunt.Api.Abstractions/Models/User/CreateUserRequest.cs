using System.ComponentModel.DataAnnotations;

namespace EasterEggHunterApi.Abstractions.Models.User;

/// <summary>
/// Request-Model für Benutzer-Erstellung
/// </summary>
public class CreateUserRequest
{
    /// <summary>
    /// Name des Benutzers
    /// </summary>
    [Required(ErrorMessage = "Name ist erforderlich")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name muss zwischen 2 und 100 Zeichen haben")]
    public string Name { get; set; } = string.Empty;
}
