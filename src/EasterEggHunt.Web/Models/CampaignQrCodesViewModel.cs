using EasterEggHunt.Domain.Entities;

namespace EasterEggHunt.Web.Models;

/// <summary>
/// ViewModel für Kampagnen-QR-Codes-Übersicht
/// </summary>
public class CampaignQrCodesViewModel
{
    /// <summary>
    /// Kampagne
    /// </summary>
    public Campaign Campaign { get; set; } = null!;

    /// <summary>
    /// QR-Codes der Kampagne
    /// </summary>
    public IReadOnlyList<QrCode> QrCodes { get; set; } = new List<QrCode>();
}
