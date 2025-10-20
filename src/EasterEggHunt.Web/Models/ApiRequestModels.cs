using System.ComponentModel.DataAnnotations;

namespace EasterEggHunt.Web.Models;

/// <summary>
/// Request-Model für QR-Code-Erstellung
/// </summary>
public class CreateQrCodeRequest
{
    /// <summary>
    /// ID der zugehörigen Kampagne
    /// </summary>
    [Required]
    public int CampaignId { get; set; }

    /// <summary>
    /// Titel des QR-Codes
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Beschreibung des QR-Codes
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Interne Notiz für Administratoren
    /// </summary>
    [Required]
    [StringLength(500)]
    public string InternalNotes { get; set; } = string.Empty;
}

/// <summary>
/// Request-Model für QR-Code-Aktualisierung
/// </summary>
public class UpdateQrCodeRequest
{
    /// <summary>
    /// ID des QR-Codes
    /// </summary>
    [Required]
    public int Id { get; set; }

    /// <summary>
    /// ID der zugehörigen Kampagne
    /// </summary>
    [Required]
    public int CampaignId { get; set; }

    /// <summary>
    /// Neuer Titel des QR-Codes
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Neue Beschreibung des QR-Codes
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Neue interne Notiz
    /// </summary>
    [Required]
    [StringLength(500)]
    public string InternalNotes { get; set; } = string.Empty;

    /// <summary>
    /// Sortierreihenfolge
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Aktiv-Status
    /// </summary>
    public bool IsActive { get; set; }
}

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

/// <summary>
/// Request-Model für Kampagnen-Erstellung
/// </summary>
public class CreateCampaignRequest
{
    /// <summary>
    /// Name der Kampagne
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Beschreibung der Kampagne
    /// </summary>
    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Ersteller der Kampagne
    /// </summary>
    [Required]
    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// Request-Model für Kampagnen-Aktualisierung
/// </summary>
public class UpdateCampaignRequest
{
    /// <summary>
    /// ID der Kampagne
    /// </summary>
    [Required]
    public int Id { get; set; }

    /// <summary>
    /// Neuer Name der Kampagne
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Neue Beschreibung der Kampagne
    /// </summary>
    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Aktiv-Status der Kampagne
    /// </summary>
    public bool IsActive { get; set; }
}
