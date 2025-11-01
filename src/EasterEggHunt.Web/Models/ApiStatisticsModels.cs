namespace EasterEggHunt.Web.Models;

/// <summary>
/// API DTO für QR-Code Statistiken
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
/// API DTO für Finder-Informationen
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
/// API DTO für Kampagnen-QR-Code Statistiken
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

/// <summary>
/// API DTO für Top-Performer-Statistiken
/// </summary>
public class TopPerformersStatisticsViewModel
{
    /// <summary>
    /// Top-Performer nach Gesamtanzahl der Funde
    /// </summary>
    public IReadOnlyList<UserStatistics> TopByTotalFinds { get; set; } = new List<UserStatistics>();

    /// <summary>
    /// Top-Performer nach einzigartigen QR-Codes
    /// </summary>
    public IReadOnlyList<UserStatistics> TopByUniqueQrCodes { get; set; } = new List<UserStatistics>();

    /// <summary>
    /// Benutzer mit der neuesten Aktivität
    /// </summary>
    public IReadOnlyList<UserStatistics> MostRecentActivity { get; set; } = new List<UserStatistics>();

    /// <summary>
    /// Zeitpunkt der Generierung der Statistiken
    /// </summary>
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// API DTO für zeitbasierte Statistiken (Funde pro Zeitraum)
/// </summary>
public class TimeSeriesStatisticsViewModel
{
    /// <summary>
    /// Zeitpunkt des Zeitraums (Tag/Woche/Monat)
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Anzahl der Funde in diesem Zeitraum
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Anzahl der einzigartigen Finder in diesem Zeitraum
    /// </summary>
    public int UniqueFinders { get; set; }

    /// <summary>
    /// Anzahl der einzigartigen QR-Codes gefunden in diesem Zeitraum
    /// </summary>
    public int UniqueQrCodes { get; set; }
}

/// <summary>
/// API DTO für zeitbasierte Übersichtsstatistiken
/// </summary>
public class TimeBasedStatisticsViewModel
{
    /// <summary>
    /// Statistiken gruppiert nach Tagen
    /// </summary>
    public IReadOnlyList<TimeSeriesStatisticsViewModel> DailyStatistics { get; set; } = new List<TimeSeriesStatisticsViewModel>();

    /// <summary>
    /// Statistiken gruppiert nach Wochen
    /// </summary>
    public IReadOnlyList<TimeSeriesStatisticsViewModel> WeeklyStatistics { get; set; } = new List<TimeSeriesStatisticsViewModel>();

    /// <summary>
    /// Statistiken gruppiert nach Monaten
    /// </summary>
    public IReadOnlyList<TimeSeriesStatisticsViewModel> MonthlyStatistics { get; set; } = new List<TimeSeriesStatisticsViewModel>();

    /// <summary>
    /// Zeitpunkt der Generierung der Statistiken
    /// </summary>
    public DateTime GeneratedAt { get; set; }
}
