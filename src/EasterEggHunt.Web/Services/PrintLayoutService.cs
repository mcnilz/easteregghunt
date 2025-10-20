using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Web.Models;

namespace EasterEggHunt.Web.Services;

/// <summary>
/// Service für Print-Layout im Admin-Bereich
/// </summary>
public class PrintLayoutService : IPrintLayoutService
{
    private readonly IEasterEggHuntApiClient _apiClient;
    private readonly ILogger<PrintLayoutService> _logger;

    public PrintLayoutService(
        IEasterEggHuntApiClient apiClient,
        ILogger<PrintLayoutService> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Bereitet Druckdaten für QR-Codes vor
    /// </summary>
    /// <param name="campaignId">Kampagnen-ID</param>
    /// <returns>Print-Layout-Daten</returns>
    public async Task<PrintLayoutViewModel> PreparePrintDataAsync(int campaignId)
    {
        try
        {
            _logger.LogInformation("Bereite Druckdaten für Kampagne {CampaignId} vor", campaignId);

            var campaign = await _apiClient.GetCampaignByIdAsync(campaignId);
            if (campaign == null)
            {
                throw new ArgumentException($"Kampagne mit ID {campaignId} nicht gefunden");
            }

            var qrCodes = await _apiClient.GetQrCodesByCampaignIdAsync(campaignId);
            var printQrCodes = await FormatQrCodesForPrintAsync(qrCodes);

            return new PrintLayoutViewModel
            {
                Campaign = campaign,
                QrCodes = printQrCodes.ToList(),
                PrintDate = DateTime.UtcNow
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Fehler beim Vorbereiten der Druckdaten für Kampagne {CampaignId}", campaignId);
            throw;
        }
    }

    /// <summary>
    /// Formatiert QR-Codes für den Druck
    /// </summary>
    /// <param name="qrCodes">QR-Codes zum Drucken</param>
    /// <returns>Formatierte QR-Code-Daten</returns>
    public Task<IEnumerable<PrintQrCodeItem>> FormatQrCodesForPrintAsync(IEnumerable<QrCode> qrCodes)
    {
        try
        {
            _logger.LogInformation("Formatiere {Count} QR-Codes für den Druck", qrCodes.Count());

            var printItems = new List<PrintQrCodeItem>();

            foreach (var qrCode in qrCodes.OrderBy(q => q.SortOrder))
            {
                var printUrl = GeneratePrintUrl(qrCode);

                printItems.Add(new PrintQrCodeItem
                {
                    Id = qrCode.Id,
                    Title = qrCode.Title,
                    Description = qrCode.Description,
                    Code = qrCode.Code,
                    PrintUrl = printUrl,
                    SortOrder = qrCode.SortOrder,
                    IsActive = qrCode.IsActive
                });
            }

            return Task.FromResult<IEnumerable<PrintQrCodeItem>>(printItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Formatieren der QR-Codes für den Druck");
            throw;
        }
    }

    /// <summary>
    /// Generiert Print-URL für QR-Codes
    /// </summary>
    /// <param name="qrCode">QR-Code</param>
    /// <returns>Print-URL</returns>
    public Uri GeneratePrintUrl(QrCode qrCode)
    {
        // Generiere URL für QR-Code-Scanning
        // Diese URL wird in den QR-Code eingebettet
        return new Uri($"https://localhost:7002/employee/scan/{qrCode.Code}");
    }

    /// <summary>
    /// Bereitet Print-Layout für Kampagne vor
    /// </summary>
    /// <param name="campaignId">Kampagnen-ID</param>
    /// <returns>Print-Layout-Daten</returns>
    public async Task<PrintLayoutViewModel> PreparePrintLayoutAsync(int campaignId)
    {
        return await PreparePrintDataAsync(campaignId);
    }
}
