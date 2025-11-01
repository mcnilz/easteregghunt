namespace EasterEggHunt.Domain.Models;

/// <summary>
/// DTO für System-Übersichtsstatistiken
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
    /// Gesamtanzahl der QR-Codes
    /// </summary>
    public int TotalQrCodes { get; set; }

    /// <summary>
    /// Gesamtanzahl der Benutzer
    /// </summary>
    public int TotalUsers { get; set; }

    /// <summary>
    /// Gesamtanzahl der Funde
    /// </summary>
    public int TotalFinds { get; set; }

    /// <summary>
    /// Anzahl der abgeschlossenen Funde
    /// </summary>
    public int CompletedFinds { get; set; }

    /// <summary>
    /// Zeitpunkt der Generierung der Statistiken
    /// </summary>
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// DTO für Kampagnen-Statistiken
/// </summary>
public class CampaignStatistics
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
    /// Gesamtanzahl der Funde
    /// </summary>
    public int TotalFinds { get; set; }

    /// <summary>
    /// Anzahl der einzigartigen Finder
    /// </summary>
    public int UniqueFinders { get; set; }

    /// <summary>
    /// Abschlussrate (Funde pro QR-Code)
    /// </summary>
    public double CompletionRate { get; set; }

    /// <summary>
    /// Zeitpunkt der Generierung der Statistiken
    /// </summary>
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// DTO für Benutzer-Statistiken
/// </summary>
public class UserStatistics
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
    /// Gesamtanzahl der Funde
    /// </summary>
    public int TotalFinds { get; set; }

    /// <summary>
    /// Anzahl der einzigartigen gefundenen QR-Codes
    /// </summary>
    public int UniqueQrCodesFound { get; set; }

    /// <summary>
    /// Datum des ersten Fundes
    /// </summary>
    public DateTime? FirstFindDate { get; set; }

    /// <summary>
    /// Datum des letzten Fundes
    /// </summary>
    public DateTime? LastFindDate { get; set; }

    /// <summary>
    /// Zeitpunkt der Generierung der Statistiken
    /// </summary>
    public DateTime GeneratedAt { get; set; }
}

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
    public string QrCodeTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gesamtanzahl der Funde
    /// </summary>
    public int TotalFinds { get; set; }

    /// <summary>
    /// Anzahl der einzigartigen Finder
    /// </summary>
    public int UniqueFinders { get; set; }

    /// <summary>
    /// Datum des ersten Fundes
    /// </summary>
    public DateTime? FirstFindDate { get; set; }

    /// <summary>
    /// Datum des letzten Fundes
    /// </summary>
    public DateTime? LastFindDate { get; set; }

    /// <summary>
    /// Liste der letzten Funde
    /// </summary>
    public IReadOnlyList<QrCodeStatisticsRecentFind> RecentFinds { get; set; } = new List<QrCodeStatisticsRecentFind>();

    /// <summary>
    /// Zeitpunkt der Generierung der Statistiken
    /// </summary>
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// DTO für QR-Code Statistiken - Recent Find
/// </summary>
public class QrCodeStatisticsRecentFind
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
    /// QR-Code Statistiken
    /// </summary>
    public IReadOnlyList<QrCodeStatisticsDto> QrCodeStatistics { get; set; } = new List<QrCodeStatisticsDto>();

    /// <summary>
    /// Zeitpunkt der Generierung der Statistiken
    /// </summary>
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// DTO für Top-Performer-Statistiken
/// </summary>
public class TopPerformersStatistics
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
/// DTO für zeitbasierte Statistiken (Funde pro Zeitraum)
/// </summary>
public class TimeSeriesStatistics
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
/// DTO für zeitbasierte Übersichtsstatistiken
/// </summary>
public class TimeBasedStatistics
{
    /// <summary>
    /// Statistiken gruppiert nach Tagen
    /// </summary>
    public IReadOnlyList<TimeSeriesStatistics> DailyStatistics { get; set; } = new List<TimeSeriesStatistics>();

    /// <summary>
    /// Statistiken gruppiert nach Wochen
    /// </summary>
    public IReadOnlyList<TimeSeriesStatistics> WeeklyStatistics { get; set; } = new List<TimeSeriesStatistics>();

    /// <summary>
    /// Statistiken gruppiert nach Monaten
    /// </summary>
    public IReadOnlyList<TimeSeriesStatistics> MonthlyStatistics { get; set; } = new List<TimeSeriesStatistics>();

    /// <summary>
    /// Zeitpunkt der Generierung der Statistiken
    /// </summary>
    public DateTime GeneratedAt { get; set; }
}
