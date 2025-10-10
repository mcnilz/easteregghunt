using System.ComponentModel.DataAnnotations;

namespace EasterEggHunt.Api.Models;

/// <summary>
/// DTO für QR-Code Statistiken
/// </summary>
public class QrCodeStatisticsDto
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
    public IReadOnlyList<FinderInfoDto> Finders { get; set; } = new List<FinderInfoDto>();

    /// <summary>
    /// Zeitpunkt der Generierung der Statistiken
    /// </summary>
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// DTO für Finder-Informationen
/// </summary>
public class FinderInfoDto
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
/// DTO für Kampagnen-QR-Code Statistiken
/// </summary>
public class CampaignQrCodeStatisticsDto
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
    public IReadOnlyList<QrCodeStatisticsDto> QrCodeStatistics { get; set; } = new List<QrCodeStatisticsDto>();

    /// <summary>
    /// Zeitpunkt der Generierung der Statistiken
    /// </summary>
    public DateTime GeneratedAt { get; set; }
}
