using System.ComponentModel.DataAnnotations;
using EasterEggHunt.Domain.Entities;

namespace EasterEggHunt.Web.Models;

/// <summary>
/// ViewModel für das Admin Dashboard
/// </summary>
public class AdminDashboardViewModel
{
    /// <summary>
    /// Liste aller Kampagnen
    /// </summary>
    public IReadOnlyList<Campaign> Campaigns { get; }

    /// <summary>
    /// Gesamtanzahl der Benutzer
    /// </summary>
    public int TotalUsers { get; set; }

    /// <summary>
    /// Anzahl der aktiven Kampagnen
    /// </summary>
    public int ActiveCampaigns { get; set; }

    /// <summary>
    /// Gesamtanzahl der QR-Codes
    /// </summary>
    public int TotalQrCodes { get; set; }

    /// <summary>
    /// Gesamtanzahl der Funde
    /// </summary>
    public int TotalFinds { get; set; }

    /// <summary>
    /// Anzahl der aktiven QR-Codes
    /// </summary>
    public int ActiveQrCodes { get; set; }

    /// <summary>
    /// Letzte Aktivitäten (QR-Code Funde)
    /// </summary>
    public IReadOnlyList<RecentActivityViewModel> RecentActivities { get; set; } = new List<RecentActivityViewModel>();

    /// <summary>
    /// Konstruktor für AdminDashboardViewModel
    /// </summary>
    /// <param name="campaigns">Liste der Kampagnen</param>
    public AdminDashboardViewModel(IEnumerable<Campaign> campaigns)
    {
        Campaigns = campaigns.ToList();
    }
}

/// <summary>
/// ViewModel für Kampagnen-Details
/// </summary>
public class CampaignDetailsViewModel
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
    /// Anzahl der einzigartigen Finder
    /// </summary>
    public int UniqueFinders { get; set; }

    /// <summary>
    /// QR-Code Statistiken für die Kampagne
    /// </summary>
    public CampaignQrCodeStatisticsViewModel? Statistics { get; set; }

    /// <summary>
    /// Konstruktor für CampaignDetailsViewModel
    /// </summary>
    /// <param name="qrCodes">Liste der QR-Codes</param>
    public CampaignDetailsViewModel(IEnumerable<QrCode> qrCodes)
    {
        QrCodes = qrCodes.ToList();
    }
}

/// <summary>
/// ViewModel für das Erstellen einer neuen Kampagne
/// </summary>
public class CreateCampaignViewModel
{
    /// <summary>
    /// Name der Kampagne
    /// </summary>
    [Required(ErrorMessage = "Name ist erforderlich")]
    [StringLength(100, ErrorMessage = "Name darf maximal 100 Zeichen haben")]
    [Display(Name = "Kampagnenname")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Beschreibung der Kampagne
    /// </summary>
    [Required(ErrorMessage = "Beschreibung ist erforderlich")]
    [StringLength(500, ErrorMessage = "Beschreibung darf maximal 500 Zeichen haben")]
    [Display(Name = "Beschreibung")]
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel für Benutzer-Übersicht
/// </summary>
public class UserViewModel
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
/// ViewModel für Statistiken
/// </summary>
public class StatisticsViewModel
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
    /// Anzahl der aktiven Benutzer
    /// </summary>
    public int ActiveUsers { get; set; }

    /// <summary>
    /// Gesamtanzahl der QR-Codes
    /// </summary>
    public int TotalQrCodes { get; set; }

    /// <summary>
    /// Gesamtanzahl der Funde
    /// </summary>
    public int TotalFinds { get; set; }

    /// <summary>
    /// Top 10 meist gefundene QR-Codes
    /// </summary>
    public IReadOnlyList<QrCodeStatisticsViewModel> TopFoundQrCodes { get; set; } = new List<QrCodeStatisticsViewModel>();

    /// <summary>
    /// Ungefundene QR-Codes (gruppiert nach Kampagne)
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyList<QrCodeStatisticsViewModel>> UnfoundQrCodesByCampaign { get; set; } = new Dictionary<string, IReadOnlyList<QrCodeStatisticsViewModel>>();
}

/// <summary>
/// ViewModel für QR-Code erstellen
/// </summary>
public class CreateQrCodeViewModel
{
    /// <summary>
    /// Kampagnen-ID
    /// </summary>
    public int CampaignId { get; set; }

    /// <summary>
    /// Name der Kampagne
    /// </summary>
    public string CampaignName { get; set; } = string.Empty;

    /// <summary>
    /// Titel des QR-Codes
    /// </summary>
    [Required(ErrorMessage = "Titel ist erforderlich")]
    [StringLength(100, ErrorMessage = "Titel darf maximal 100 Zeichen lang sein")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Beschreibung des QR-Codes
    /// </summary>
    [StringLength(500, ErrorMessage = "Beschreibung darf maximal 500 Zeichen lang sein")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Interne Notizen (nur für Admins sichtbar)
    /// </summary>
    [StringLength(1000, ErrorMessage = "Interne Notizen dürfen maximal 1000 Zeichen lang sein")]
    public string InternalNotes { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel für QR-Code bearbeiten
/// </summary>
public class EditQrCodeViewModel
{
    /// <summary>
    /// QR-Code-ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Kampagnen-ID
    /// </summary>
    public int CampaignId { get; set; }

    /// <summary>
    /// Titel des QR-Codes
    /// </summary>
    [Required(ErrorMessage = "Titel ist erforderlich")]
    [StringLength(100, ErrorMessage = "Titel darf maximal 100 Zeichen lang sein")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Beschreibung des QR-Codes
    /// </summary>
    [StringLength(500, ErrorMessage = "Beschreibung darf maximal 500 Zeichen lang sein")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Interne Notizen (nur für Admins sichtbar)
    /// </summary>
    [StringLength(1000, ErrorMessage = "Interne Notizen dürfen maximal 1000 Zeichen lang sein")]
    public string InternalNotes { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel für QR-Code löschen
/// </summary>
public class DeleteQrCodeViewModel
{
    /// <summary>
    /// QR-Code-ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Kampagnen-ID
    /// </summary>
    public int CampaignId { get; set; }

    /// <summary>
    /// Name der Kampagne
    /// </summary>
    public string CampaignName { get; set; } = string.Empty;

    /// <summary>
    /// Titel des QR-Codes
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Beschreibung des QR-Codes
    /// </summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel für letzte Aktivitäten
/// </summary>
public class RecentActivityViewModel
{
    /// <summary>
    /// Benutzername
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// QR-Code Titel
    /// </summary>
    public string QrCodeTitle { get; set; } = string.Empty;

    /// <summary>
    /// Kampagnenname
    /// </summary>
    public string CampaignName { get; set; } = string.Empty;

    /// <summary>
    /// Zeitpunkt des Fundes
    /// </summary>
    public DateTime FoundAt { get; set; }

    /// <summary>
    /// IP-Adresse des Fundes
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel für QR-Code Druckansicht
/// </summary>
public class PrintQrCodesViewModel
{
    /// <summary>
    /// Die Kampagne
    /// </summary>
    public Campaign Campaign { get; set; } = null!;

    /// <summary>
    /// Liste der zu druckenden QR-Codes
    /// </summary>
    public IReadOnlyList<QrCode> QrCodes { get; set; } = new List<QrCode>();

    /// <summary>
    /// Größe der QR-Codes in Pixeln
    /// </summary>
    public int Size { get; set; } = 200;

    /// <summary>
    /// Ob Titel angezeigt werden sollen
    /// </summary>
    public bool ShowTitles { get; set; } = true;
}

/// <summary>
/// ViewModel für Fund-Historie mit Filtern
/// </summary>
public class FindHistoryViewModel
{
    /// <summary>
    /// Liste der Funde
    /// </summary>
    public IReadOnlyList<Find> Finds { get; set; } = new List<Find>();

    /// <summary>
    /// Gesamtanzahl der gefilterten Funde
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Anzahl übersprungener Einträge
    /// </summary>
    public int Skip { get; set; }

    /// <summary>
    /// Anzahl abgerufener Einträge
    /// </summary>
    public int Take { get; set; }

    /// <summary>
    /// Filter: Startdatum
    /// </summary>
    [Display(Name = "Von")]
    [DataType(DataType.Date)]
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Filter: Enddatum
    /// </summary>
    [Display(Name = "Bis")]
    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Filter: Benutzer-ID
    /// </summary>
    [Display(Name = "Benutzer")]
    public int? UserId { get; set; }

    /// <summary>
    /// Filter: QR-Code-ID
    /// </summary>
    [Display(Name = "QR-Code")]
    public int? QrCodeId { get; set; }

    /// <summary>
    /// Filter: Kampagnen-ID
    /// </summary>
    [Display(Name = "Kampagne")]
    public int? CampaignId { get; set; }

    /// <summary>
    /// Sortierungsfeld
    /// </summary>
    public string SortBy { get; set; } = "FoundAt";

    /// <summary>
    /// Sortierungsrichtung
    /// </summary>
    public string SortDirection { get; set; } = "desc";

    /// <summary>
    /// Liste aller Benutzer für Dropdown
    /// </summary>
    public IReadOnlyList<User> AllUsers { get; set; } = new List<User>();

    /// <summary>
    /// Liste aller QR-Codes für Dropdown
    /// </summary>
    public IReadOnlyList<QrCode> AllQrCodes { get; set; } = new List<QrCode>();

    /// <summary>
    /// Liste aller Kampagnen für Dropdown
    /// </summary>
    public IReadOnlyList<Campaign> AllCampaigns { get; set; } = new List<Campaign>();

    /// <summary>
    /// Berechnet die aktuelle Seite basierend auf Skip und Take
    /// </summary>
    public int CurrentPage => (Skip / Take) + 1;

    /// <summary>
    /// Berechnet die Gesamtanzahl der Seiten
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / Take);
}
