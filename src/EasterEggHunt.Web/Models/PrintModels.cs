using EasterEggHunt.Domain.Entities;

namespace EasterEggHunt.Web.Models;

/// <summary>
/// ViewModel für Print-Layout
/// </summary>
public class PrintLayoutViewModel
{
    /// <summary>
    /// Kampagne für den Druck
    /// </summary>
    public Campaign Campaign { get; set; } = null!;

    /// <summary>
    /// QR-Codes zum Drucken
    /// </summary>
    public IReadOnlyList<PrintQrCodeItem> QrCodes { get; set; } = new List<PrintQrCodeItem>();

    /// <summary>
    /// Druckdatum
    /// </summary>
    public DateTime PrintDate { get; set; }
}

/// <summary>
/// Item für QR-Code-Druck
/// </summary>
public class PrintQrCodeItem
{
    /// <summary>
    /// QR-Code-ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Titel des QR-Codes
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Beschreibung des QR-Codes
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Code des QR-Codes
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Print-URL für den QR-Code
    /// </summary>
    public Uri PrintUrl { get; set; } = null!;

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
/// ViewModel für System-Übersichtsstatistiken
/// </summary>
public class SystemOverviewStatistics
{
    /// <summary>
    /// Gesamtanzahl der Kampagnen
    /// </summary>
    public int TotalCampaigns { get; set; }

    /// <summary>
    /// Anzahl der aktiven Kampagnen
    /// </summary>
    public int ActiveCampaigns { get; set; }

    /// <summary>
    /// Gesamtanzahl der Benutzer
    /// </summary>
    public int TotalUsers { get; set; }

    /// <summary>
    /// Gesamtanzahl der QR-Codes
    /// </summary>
    public int TotalQrCodes { get; set; }

    /// <summary>
    /// Gesamtanzahl der Funde
    /// </summary>
    public int TotalFinds { get; set; }
}
