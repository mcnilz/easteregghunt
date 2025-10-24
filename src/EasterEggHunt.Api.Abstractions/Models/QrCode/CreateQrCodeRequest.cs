using System.ComponentModel.DataAnnotations;

namespace EasterEggHunterApi.Abstractions.Models.QrCode;

/// <summary>
/// Request-Model für QR-Code-Erstellung
/// </summary>
public class CreateQrCodeRequest
{
    /// <summary>
    /// ID der zugehörigen Kampagne
    /// </summary>
    [Required(ErrorMessage = "Kampagnen-ID ist erforderlich")]
    [Range(1, int.MaxValue, ErrorMessage = "Kampagnen-ID muss größer als 0 sein")]
    public int CampaignId { get; set; }

    /// <summary>
    /// Titel des QR-Codes
    /// </summary>
    [Required(ErrorMessage = "Titel ist erforderlich")]
    [StringLength(100, ErrorMessage = "Titel darf maximal 100 Zeichen haben")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Beschreibung des QR-Codes
    /// </summary>
    [StringLength(500, ErrorMessage = "Beschreibung darf maximal 500 Zeichen haben")]
    public string? Description { get; set; }

    /// <summary>
    /// Interne Notiz für Administratoren
    /// </summary>
    [Required(ErrorMessage = "Interne Notiz ist erforderlich")]
    [StringLength(500, ErrorMessage = "Interne Notiz darf maximal 500 Zeichen haben")]
    public string InternalNotes { get; set; } = string.Empty;
}
