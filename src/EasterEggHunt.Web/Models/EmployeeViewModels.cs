using System.ComponentModel.DataAnnotations;
using EasterEggHunt.Domain.Entities;

namespace EasterEggHunt.Web.Models;

/// <summary>
/// ViewModel für Mitarbeiter-Registrierung
/// </summary>
public class EmployeeRegistrationViewModel
{
    /// <summary>
    /// Name des Mitarbeiters
    /// </summary>
    [Required(ErrorMessage = "Name ist erforderlich")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name muss zwischen 2 und 100 Zeichen haben")]
    [RegularExpression(@"^[a-zA-ZäöüÄÖÜß\s\-\.]+$", ErrorMessage = "Name darf nur Buchstaben, Leerzeichen, Bindestriche und Punkte enthalten")]
    [Display(Name = "Dein Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// QR-Code URL für Weiterleitung nach Registrierung
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:URI-like properties should not be strings", Justification = "MVC Model Binding requires string for query parameters")]
    public string? QrCodeUrl { get; set; }
}

/// <summary>
/// Session-Informationen für Mitarbeiter
/// </summary>
public class EmployeeSessionInfo
{
    /// <summary>
    /// Benutzer-ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Name des Mitarbeiters
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Registrierungszeitpunkt
    /// </summary>
    public DateTime RegisteredAt { get; set; }
}

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
/// ViewModel für QR-Code Scan-Ergebnis
/// </summary>
public class ScanResultViewModel
{
    /// <summary>
    /// Der gescannte QR-Code
    /// </summary>
    public QrCode QrCode { get; set; } = null!;

    /// <summary>
    /// Der aktuelle Fund
    /// </summary>
    public Find CurrentFind { get; set; } = null!;

    /// <summary>
    /// Der erste Fund dieses QR-Codes durch diesen User (falls bereits gefunden)
    /// </summary>
    public Find? PreviousFind { get; set; }

    /// <summary>
    /// Gibt an, ob dies der erste Fund dieses QR-Codes durch diesen User ist
    /// </summary>
    public bool IsFirstFind => PreviousFind == null;

    /// <summary>
    /// Gesamtanzahl der Funde dieses Users
    /// </summary>
    public int UserTotalFinds { get; set; }

    /// <summary>
    /// Gesamtanzahl der QR-Codes in der Kampagne
    /// </summary>
    public int CampaignTotalQrCodes { get; set; }

    /// <summary>
    /// Fortschritt in Prozent (0-100)
    /// </summary>
    public int ProgressPercentage { get; set; }
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
