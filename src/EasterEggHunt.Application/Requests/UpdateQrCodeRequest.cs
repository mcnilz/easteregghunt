using System.ComponentModel.DataAnnotations;

namespace EasterEggHunt.Application.Requests;

/// <summary>
/// Request für das Aktualisieren eines QR-Codes
/// </summary>
public class UpdateQrCodeRequest
{
    /// <summary>
    /// ID des QR-Codes
    /// </summary>
    [Required]
    public int Id { get; set; }

    /// <summary>
    /// Titel des QR-Codes
    /// </summary>
    [Required(ErrorMessage = "Titel ist erforderlich")]
    [StringLength(100, ErrorMessage = "Titel darf maximal 100 Zeichen lang sein")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Beschreibung des QR-Codes
    /// </summary>
    [StringLength(500, ErrorMessage = "Beschreibung darf maximal 500 Zeichen lang sein")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Interne Notizen (nur für Admins sichtbar)
    /// </summary>
    [StringLength(1000, ErrorMessage = "Interne Notizen dürfen maximal 1000 Zeichen lang sein")]
    public string InternalNotes { get; set; } = string.Empty;
}
