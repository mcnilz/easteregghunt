namespace EasterEggHunt.Domain.Entities;

/// <summary>
/// Repräsentiert einen Administrator-Benutzer
/// </summary>
public class AdminUser
{
    /// <summary>
    /// Eindeutige ID des Admin-Benutzers
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Benutzername für die Anmeldung
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gehashtes Passwort
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// E-Mail-Adresse des Administrators
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Zeitpunkt der Erstellung des Admin-Accounts
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Zeitpunkt der letzten Anmeldung
    /// </summary>
    public DateTime? LastLogin { get; set; }

    /// <summary>
    /// Gibt an, ob der Admin-Account aktiv ist
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Konstruktor für Entity Framework
    /// </summary>
    public AdminUser()
    {
    }

    /// <summary>
    /// Erstellt einen neuen Administrator
    /// </summary>
    /// <param name="username">Benutzername</param>
    /// <param name="email">E-Mail-Adresse</param>
    /// <param name="passwordHash">Gehashtes Passwort</param>
    public AdminUser(string username, string email, string passwordHash)
    {
        Username = username ?? throw new ArgumentNullException(nameof(username));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    /// <summary>
    /// Aktualisiert den Zeitpunkt der letzten Anmeldung
    /// </summary>
    public void UpdateLastLogin()
    {
        LastLogin = DateTime.UtcNow;
    }

    /// <summary>
    /// Deaktiviert den Admin-Account
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Aktiviert den Admin-Account
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Aktualisiert das Passwort
    /// </summary>
    /// <param name="newPasswordHash">Neues gehashtes Passwort</param>
    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash ?? throw new ArgumentNullException(nameof(newPasswordHash));
    }
}


