using System.ComponentModel.DataAnnotations;

namespace EasterEggHunterApi.Abstractions.Models.Auth;

/// <summary>
/// Request DTO für Admin-Login
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Benutzername des Administrators
    /// </summary>
    [Required(ErrorMessage = "Benutzername ist erforderlich")]
    [StringLength(50, ErrorMessage = "Benutzername darf maximal 50 Zeichen haben")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Passwort des Administrators
    /// </summary>
    [Required(ErrorMessage = "Passwort ist erforderlich")]
    [StringLength(100, ErrorMessage = "Passwort darf maximal 100 Zeichen haben")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Ob die Session länger gespeichert werden soll
    /// </summary>
    public bool RememberMe { get; set; }
}
