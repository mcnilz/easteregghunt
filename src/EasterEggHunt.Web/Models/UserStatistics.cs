namespace EasterEggHunt.Web.Models;

/// <summary>
/// DTO für Benutzer-Statistiken (entspricht API Response)
/// </summary>
public sealed class UserStatistics
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int TotalFinds { get; set; }
    public int UniqueQrCodesFound { get; set; }

    /// <summary>
    /// Datum des ersten Fundes (nullable, entspricht Domain Model FirstFindDate)
    /// </summary>
    public DateTime? FirstFindDate { get; set; }

    /// <summary>
    /// Datum des letzten Fundes (nullable, entspricht Domain Model LastFindDate)
    /// </summary>
    public DateTime? LastFindDate { get; set; }

    /// <summary>
    /// Kompatibilität mit älteren Views (deprecated, verwende FirstFindDate)
    /// </summary>
    [Obsolete("Verwende FirstFindDate")]
    public DateTime FirstSeen => FirstFindDate ?? DateTime.MinValue;

    /// <summary>
    /// Kompatibilität mit älteren Views (deprecated, verwende LastFindDate)
    /// </summary>
    [Obsolete("Verwende LastFindDate")]
    public DateTime LastSeen => LastFindDate ?? DateTime.MinValue;

    public bool IsActive { get; set; }
    public DateTime GeneratedAt { get; set; }
}


