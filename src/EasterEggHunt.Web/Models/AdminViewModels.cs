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
}
