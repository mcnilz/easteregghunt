namespace EasterEggHunt.Domain.Entities;

/// <summary>
/// Repräsentiert einen Mitarbeiter/Benutzer im System
/// </summary>
public class User
{
    /// <summary>
    /// Eindeutige ID des Benutzers
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name des Benutzers
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Zeitpunkt der ersten Registrierung
    /// </summary>
    public DateTime FirstSeen { get; set; }

    /// <summary>
    /// Zeitpunkt der letzten Aktivität
    /// </summary>
    public DateTime LastSeen { get; set; }

    /// <summary>
    /// Gibt an, ob der Benutzer aktiv ist
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Liste aller Funde des Benutzers
    /// </summary>
    public virtual ICollection<Find> Finds { get; } = new List<Find>();

    /// <summary>
    /// Liste aller Sessions des Benutzers
    /// </summary>
    public virtual ICollection<Session> Sessions { get; } = new List<Session>();

    /// <summary>
    /// Konstruktor für Entity Framework
    /// </summary>
    public User()
    {
    }

    /// <summary>
    /// Erstellt einen neuen Benutzer
    /// </summary>
    /// <param name="name">Name des Benutzers</param>
    public User(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        FirstSeen = DateTime.UtcNow;
        LastSeen = DateTime.UtcNow;
        IsActive = true;
    }

    /// <summary>
    /// Aktualisiert die letzte Aktivität des Benutzers
    /// </summary>
    public void UpdateLastSeen()
    {
        LastSeen = DateTime.UtcNow;
    }

    /// <summary>
    /// Deaktiviert den Benutzer
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdateLastSeen();
    }

    /// <summary>
    /// Aktiviert den Benutzer
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdateLastSeen();
    }
}

