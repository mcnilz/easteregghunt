using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Web.Models;

namespace EasterEggHunt.Web.Services;

/// <summary>
/// Service für Print-Layout im Admin-Bereich
/// </summary>
public interface IPrintLayoutService
{
    /// <summary>
    /// Bereitet Druckdaten für QR-Codes vor
    /// </summary>
    /// <param name="campaignId">Kampagnen-ID</param>
    /// <returns>Print-Layout-Daten</returns>
    Task<PrintLayoutViewModel> PreparePrintDataAsync(int campaignId);

    /// <summary>
    /// Formatiert QR-Codes für den Druck
    /// </summary>
    /// <param name="qrCodes">QR-Codes zum Drucken</param>
    /// <returns>Formatierte QR-Code-Daten</returns>
    Task<IEnumerable<PrintQrCodeItem>> FormatQrCodesForPrintAsync(IEnumerable<QrCode> qrCodes);

    /// <summary>
    /// Generiert Print-URL für QR-Codes
    /// </summary>
    /// <param name="qrCode">QR-Code</param>
    /// <returns>Print-URL</returns>
    Uri GeneratePrintUrl(QrCode qrCode);

    /// <summary>
    /// Bereitet Print-Layout für Kampagne vor
    /// </summary>
    /// <param name="campaignId">Kampagnen-ID</param>
    /// <returns>Print-Layout-Daten</returns>
    Task<PrintLayoutViewModel> PreparePrintLayoutAsync(int campaignId);
}
