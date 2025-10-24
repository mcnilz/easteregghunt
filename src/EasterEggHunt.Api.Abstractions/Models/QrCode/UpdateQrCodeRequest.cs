using System.ComponentModel.DataAnnotations;

namespace EasterEggHunterApi.Abstractions.Models.QrCode;

/// <summary>
/// Request-Model für QR-Code-Aktualisierung
/// </summary>
public class UpdateQrCodeRequest
{
    /// <summary>
    /// Neuer Titel des QR-Codes
    /// </summary>
    [Required(ErrorMessage = "Titel ist erforderlich")]
    [StringLength(100, ErrorMessage = "Titel darf maximal 100 Zeichen haben")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Neue Beschreibung des QR-Codes
    /// </summary>
    [StringLength(500, ErrorMessage = "Beschreibung darf maximal 500 Zeichen haben")]
    public string? Description { get; set; }

    /// <summary>
    /// Neue interne Notiz
    /// </summary>
    [Required(ErrorMessage = "Interne Notiz ist erforderlich")]
    [StringLength(500, ErrorMessage = "Interne Notiz darf maximal 500 Zeichen haben")]
    public string InternalNotes { get; set; } = string.Empty;
}
