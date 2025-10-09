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
}

