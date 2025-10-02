namespace EasterEggHunt.Domain.Entities;

/// <summary>
/// Repräsentiert eine Benutzer-Session
/// </summary>
public class Session
{
    /// <summary>
    /// Eindeutige Session-ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// ID des zugehörigen Benutzers
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Zeitpunkt der Session-Erstellung
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Zeitpunkt des Session-Ablaufs
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Session-Daten als JSON
    /// </summary>
    public string Data { get; set; } = string.Empty;

    /// <summary>
    /// Gibt an, ob die Session aktiv ist
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Navigation Property zum Benutzer
    /// </summary>
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Konstruktor für Entity Framework
    /// </summary>
    public Session()
    {
    }

    /// <summary>
    /// Erstellt eine neue Session
    /// </summary>
    /// <param name="userId">ID des Benutzers</param>
    /// <param name="expirationDays">Anzahl Tage bis zum Ablauf</param>
    public Session(int userId, int expirationDays = 30)
    {
        Id = Guid.NewGuid().ToString();
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = DateTime.UtcNow.AddDays(expirationDays);
        IsActive = true;
        Data = "{}"; // Leeres JSON-Objekt
    }

    /// <summary>
    /// Prüft, ob die Session noch gültig ist
    /// </summary>
    /// <returns>True wenn die Session gültig ist</returns>
    public bool IsValid()
    {
        return IsActive && DateTime.UtcNow < ExpiresAt;
    }

    /// <summary>
    /// Verlängert die Session um die angegebenen Tage
    /// </summary>
    /// <param name="days">Anzahl Tage zur Verlängerung</param>
    public void Extend(int days)
    {
        ExpiresAt = DateTime.UtcNow.AddDays(days);
    }

    /// <summary>
    /// Deaktiviert die Session
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Aktualisiert die Session-Daten
    /// </summary>
    /// <param name="data">Neue Session-Daten als JSON</param>
    public void UpdateData(string data)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
    }
}


