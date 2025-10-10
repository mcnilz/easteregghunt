namespace EasterEggHunt.Web.Models;

/// <summary>
/// ViewModel für QR-Code Statistiken
/// </summary>
public class QrCodeStatisticsViewModel
{
    /// <summary>
    /// QR-Code ID
    /// </summary>
    public int QrCodeId { get; set; }

    /// <summary>
    /// QR-Code Titel
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Kampagnen-ID
    /// </summary>
    public int CampaignId { get; set; }

    /// <summary>
    /// Kampagnen-Name
    /// </summary>
    public string CampaignName { get; set; } = string.Empty;

    /// <summary>
    /// Anzahl der Funde
    /// </summary>
    public int FindCount { get; set; }

    /// <summary>
    /// Ob der QR-Code bereits gefunden wurde
    /// </summary>
    public bool IsFound => FindCount > 0;

    /// <summary>
    /// Liste der Finder mit Details
    /// </summary>
    public IReadOnlyList<FinderInfoViewModel> Finders { get; set; } = new List<FinderInfoViewModel>();

    /// <summary>
    /// Zeitpunkt der Generierung der Statistiken
    /// </summary>
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// ViewModel für Finder-Informationen
/// </summary>
public class FinderInfoViewModel
{
    /// <summary>
    /// Benutzer-ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Benutzername
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Zeitpunkt des Fundes
    /// </summary>
    public DateTime FoundAt { get; set; }

    /// <summary>
    /// IP-Adresse des Benutzers
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel für Kampagnen-QR-Code Statistiken
/// </summary>
public class CampaignQrCodeStatisticsViewModel
{
    /// <summary>
    /// Kampagnen-ID
    /// </summary>
    public int CampaignId { get; set; }

    /// <summary>
    /// Kampagnen-Name
    /// </summary>
    public string CampaignName { get; set; } = string.Empty;

    /// <summary>
    /// Gesamtanzahl der QR-Codes
    /// </summary>
    public int TotalQrCodes { get; set; }

    /// <summary>
    /// Anzahl der gefundenen QR-Codes
    /// </summary>
    public int FoundQrCodes { get; set; }

    /// <summary>
    /// Anzahl der ungefunden QR-Codes
    /// </summary>
    public int UnfoundQrCodes => TotalQrCodes - FoundQrCodes;

    /// <summary>
    /// Gesamtanzahl der Funde
    /// </summary>
    public int TotalFinds { get; set; }

    /// <summary>
    /// QR-Code Statistiken
    /// </summary>
    public IReadOnlyList<QrCodeStatisticsViewModel> QrCodeStatistics { get; set; } = new List<QrCodeStatisticsViewModel>();

    /// <summary>
    /// Zeitpunkt der Generierung der Statistiken
    /// </summary>
    public DateTime GeneratedAt { get; set; }
}
