using EasterEggHunt.Domain.Entities;

namespace EasterEggHunt.Web.Models;

/// <summary>
/// ViewModel für Kampagnen-Übersicht im Employee Interface
/// </summary>
public class CampaignOverviewViewModel
{
    /// <summary>
    /// Die Kampagne
    /// </summary>
    public Campaign Campaign { get; set; } = null!;

    /// <summary>
    /// Gesamtanzahl der QR-Codes
    /// </summary>
    public int TotalQrCodes { get; set; }

    /// <summary>
    /// Gesamtanzahl der Funde
    /// </summary>
    public int TotalFinds { get; set; }
}

/// <summary>
/// ViewModel für QR-Code Scanning
/// </summary>
public class ScanQrCodeViewModel
{
    /// <summary>
    /// Der zu scannende QR-Code
    /// </summary>
    public string QrCode { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel für Benutzer-Profil
/// </summary>
public class UserProfileViewModel
{
    /// <summary>
    /// Der Benutzer
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Anzahl der Funde des Benutzers
    /// </summary>
    public int FindCount { get; set; }

    /// <summary>
    /// Letzte Funde des Benutzers
    /// </summary>
    public IReadOnlyList<Find> RecentFinds { get; }

    /// <summary>
    /// Konstruktor für UserProfileViewModel
    /// </summary>
    /// <param name="recentFinds">Liste der letzten Funde</param>
    public UserProfileViewModel(IEnumerable<Find> recentFinds)
    {
        RecentFinds = recentFinds.ToList();
    }
}

/// <summary>
/// ViewModel für Kampagnen-Details im Employee Interface
/// </summary>
public class EmployeeCampaignDetailsViewModel
{
    /// <summary>
    /// Die Kampagne
    /// </summary>
    public Campaign Campaign { get; set; } = null!;

    /// <summary>
    /// Liste der QR-Codes der Kampagne
    /// </summary>
    public IReadOnlyList<QrCode> QrCodes { get; }

    /// <summary>
    /// Gesamtanzahl der Funde
    /// </summary>
    public int TotalFinds { get; set; }

    /// <summary>
    /// Konstruktor für EmployeeCampaignDetailsViewModel
    /// </summary>
    /// <param name="qrCodes">Liste der QR-Codes</param>
    public EmployeeCampaignDetailsViewModel(IEnumerable<QrCode> qrCodes)
    {
        QrCodes = qrCodes.ToList();
    }
}

/// <summary>
/// ViewModel für Leaderboard-Eintrag
/// </summary>
public class UserLeaderboardEntry
{
    /// <summary>
    /// Der Benutzer
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Anzahl der Funde des Benutzers
    /// </summary>
    public int FindCount { get; set; }
}

/// <summary>
/// ViewModel für das Leaderboard
/// </summary>
public class LeaderboardViewModel
{
    /// <summary>
    /// Liste der Leaderboard-Einträge
    /// </summary>
    public IReadOnlyList<UserLeaderboardEntry> Entries { get; }

    /// <summary>
    /// Konstruktor für LeaderboardViewModel
    /// </summary>
    /// <param name="entries">Liste der Leaderboard-Einträge</param>
    public LeaderboardViewModel(IEnumerable<UserLeaderboardEntry> entries)
    {
        Entries = entries.ToList();
    }
}
