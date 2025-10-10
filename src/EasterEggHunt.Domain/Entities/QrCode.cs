namespace EasterEggHunt.Domain.Entities;

/// <summary>
/// Repräsentiert einen QR-Code in einer Kampagne
/// </summary>
public class QrCode
{
    /// <summary>
    /// Eindeutige ID des QR-Codes
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ID der zugehörigen Kampagne
    /// </summary>
    public int CampaignId { get; set; }

    /// <summary>
    /// Titel des QR-Codes (öffentlich sichtbar)
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Beschreibung des QR-Codes (öffentlich sichtbar)
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Interne Notizen für Administratoren
    /// </summary>
    public string InternalNotes { get; set; } = string.Empty;

    /// <summary>
    /// Eindeutiger Code für den QR-Code (ohne Domain)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Zeitpunkt der Erstellung
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Zeitpunkt der letzten Aktualisierung
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gibt an, ob der QR-Code aktiv ist
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Sortierreihenfolge für die Anzeige
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Navigation Property zur zugehörigen Kampagne
    /// </summary>
    public virtual Campaign Campaign { get; set; } = null!;

    /// <summary>
    /// Liste aller Funde dieses QR-Codes
    /// </summary>
    public virtual ICollection<Find> Finds { get; } = new List<Find>();

    /// <summary>
    /// Konstruktor für Entity Framework
    /// </summary>
    public QrCode()
    {
    }

    /// <summary>
    /// Erstellt einen neuen QR-Code
    /// </summary>
    /// <param name="campaignId">ID der zugehörigen Kampagne</param>
    /// <param name="title">Titel des QR-Codes</param>
    /// <param name="description">Beschreibung des QR-Codes</param>
    /// <param name="internalNotes">Interne Notizen</param>
    public QrCode(int campaignId, string title, string description, string internalNotes)
    {
        CampaignId = campaignId;
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        InternalNotes = internalNotes ?? throw new ArgumentNullException(nameof(internalNotes));
        Code = GenerateUniqueCode();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        IsActive = true;
        SortOrder = 0;
    }

    /// <summary>
    /// Aktiviert den QR-Code
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deaktiviert den QR-Code
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Aktualisiert die QR-Code Details
    /// </summary>
    /// <param name="title">Neuer Titel</param>
    /// <param name="description">Neue Beschreibung</param>
    /// <param name="internalNotes">Neue interne Notizen</param>
    public void Update(string title, string description, string internalNotes)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        InternalNotes = internalNotes ?? throw new ArgumentNullException(nameof(internalNotes));
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Setzt die Sortierreihenfolge
    /// </summary>
    /// <param name="sortOrder">Neue Sortierreihenfolge</param>
    public void SetSortOrder(int sortOrder)
    {
        SortOrder = sortOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Generiert einen eindeutigen Code für den QR-Code
    /// </summary>
    /// <returns>Eindeutiger Code</returns>
    private static string GenerateUniqueCode()
    {
        return Guid.NewGuid().ToString("N")[..12]; // 12 Zeichen für bessere Lesbarkeit
    }
}

