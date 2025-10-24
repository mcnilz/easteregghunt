namespace EasterEggHunterApi.Abstractions.Models.User;

/// <summary>
/// Response-Model für Benutzername-Prüfung
/// </summary>
public class CheckUserNameResponse
{
    /// <summary>
    /// Gibt an, ob der Name bereits existiert
    /// </summary>
    public bool Exists { get; set; }

    /// <summary>
    /// Der geprüfte Name
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
