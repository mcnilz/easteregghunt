namespace EasterEggHunterApi.Abstractions.Models.Auth;

/// <summary>
/// Response DTO für erfolgreichen Login
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// ID des Administrators
    /// </summary>
    public int AdminId { get; set; }

    /// <summary>
    /// Benutzername
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// E-Mail-Adresse
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Zeitpunkt des letzten Logins
    /// </summary>
    public DateTime LastLogin { get; set; }

    /// <summary>
    /// Ob der Administrator aktiv ist
    /// </summary>
    public bool IsActive { get; set; }
}
