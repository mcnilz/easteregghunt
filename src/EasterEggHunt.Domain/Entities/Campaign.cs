namespace EasterEggHunt.Domain.Entities;

/// <summary>
/// Repräsentiert eine Easter Egg Hunt Kampagne
/// </summary>
public class Campaign
{
    /// <summary>
    /// Eindeutige ID der Kampagne
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name der Kampagne (wird öffentlich angezeigt)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Beschreibung der Kampagne
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Zeitpunkt der Erstellung
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Zeitpunkt der letzten Aktualisierung
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gibt an, ob die Kampagne aktiv ist
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Name des Erstellers der Kampagne
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Liste aller QR-Codes in dieser Kampagne
    /// </summary>
    public virtual ICollection<QrCode> QrCodes { get; } = new List<QrCode>();

    /// <summary>
    /// Konstruktor für Entity Framework
    /// </summary>
    public Campaign()
    {
    }

    /// <summary>
    /// Erstellt eine neue Kampagne
    /// </summary>
    /// <param name="name">Name der Kampagne</param>
    /// <param name="description">Beschreibung der Kampagne</param>
    /// <param name="createdBy">Ersteller der Kampagne</param>
    public Campaign(string name, string description, string createdBy)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy));
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    /// <summary>
    /// Aktiviert die Kampagne
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deaktiviert die Kampagne
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Aktualisiert die Kampagnen-Details
    /// </summary>
    /// <param name="name">Neuer Name</param>
    /// <param name="description">Neue Beschreibung</param>
    public void Update(string name, string description)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        UpdatedAt = DateTime.UtcNow;
    }
}

