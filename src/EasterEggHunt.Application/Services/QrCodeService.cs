using EasterEggHunt.Application.Requests;
using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Domain.Repositories;
using Microsoft.Extensions.Logging;
using QRCoder;

namespace EasterEggHunt.Application.Services;

/// <summary>
/// Service für QrCode-Operationen
/// </summary>
public class QrCodeService : IQrCodeService
{
    private readonly IQrCodeRepository _qrCodeRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly ILogger<QrCodeService> _logger;

    public QrCodeService(
        IQrCodeRepository qrCodeRepository,
        ICampaignRepository campaignRepository,
        ILogger<QrCodeService> logger)
    {
        _qrCodeRepository = qrCodeRepository ?? throw new ArgumentNullException(nameof(qrCodeRepository));
        _campaignRepository = campaignRepository ?? throw new ArgumentNullException(nameof(campaignRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<QrCode>> GetQrCodesByCampaignIdAsync(int campaignId)
    {
        _logger.LogInformation("Abrufen aller QR-Codes für Kampagne {CampaignId}", campaignId);
        return await _qrCodeRepository.GetByCampaignIdAsync(campaignId);
    }

    /// <inheritdoc />
    public async Task<QrCode?> GetQrCodeByIdAsync(int id)
    {
        _logger.LogInformation("Abrufen des QR-Codes mit ID {QrCodeId}", id);
        return await _qrCodeRepository.GetByIdAsync(id);
    }

    /// <inheritdoc />
    public async Task<QrCode> CreateQrCodeAsync(CreateQrCodeRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("QR-Code Titel darf nicht leer sein", nameof(request));

        // Prüfen ob Kampagne existiert
        var campaign = await _campaignRepository.GetByIdAsync(request.CampaignId);
        if (campaign == null)
        {
            throw new ArgumentException($"Kampagne mit ID {request.CampaignId} nicht gefunden", nameof(request));
        }

        _logger.LogInformation("Erstellen eines neuen QR-Codes für Kampagne {CampaignId}: {Title}", request.CampaignId, request.Title);

        var qrCode = new QrCode(request.CampaignId, request.Title, request.Description, request.InternalNotes);
        await _qrCodeRepository.AddAsync(qrCode);
        await _qrCodeRepository.SaveChangesAsync();

        _logger.LogInformation("QR-Code erfolgreich erstellt mit ID {QrCodeId}", qrCode.Id);
        return qrCode;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateQrCodeAsync(UpdateQrCodeRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("QR-Code Titel darf nicht leer sein", nameof(request));

        _logger.LogInformation("Aktualisieren des QR-Codes mit ID {QrCodeId}", request.Id);

        var qrCode = await _qrCodeRepository.GetByIdAsync(request.Id);
        if (qrCode == null)
        {
            _logger.LogWarning("QR-Code mit ID {QrCodeId} nicht gefunden", request.Id);
            return false;
        }

        qrCode.Update(request.Title, request.Description, request.InternalNotes);
        await _qrCodeRepository.SaveChangesAsync();

        _logger.LogInformation("QR-Code mit ID {QrCodeId} erfolgreich aktualisiert", request.Id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> SetQrCodeSortOrderAsync(int id, int sortOrder)
    {
        _logger.LogInformation("Setzen der Sortierreihenfolge für QR-Code {QrCodeId} auf {SortOrder}", id, sortOrder);

        var qrCode = await _qrCodeRepository.GetByIdAsync(id);
        if (qrCode == null)
        {
            _logger.LogWarning("QR-Code mit ID {QrCodeId} nicht gefunden", id);
            return false;
        }

        qrCode.SetSortOrder(sortOrder);
        await _qrCodeRepository.SaveChangesAsync();

        _logger.LogInformation("Sortierreihenfolge für QR-Code {QrCodeId} erfolgreich auf {SortOrder} gesetzt", id, sortOrder);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ActivateQrCodeAsync(int id)
    {
        _logger.LogInformation("Aktivieren des QR-Codes mit ID {QrCodeId}", id);

        var qrCode = await _qrCodeRepository.GetByIdAsync(id);
        if (qrCode == null)
        {
            _logger.LogWarning("QR-Code mit ID {QrCodeId} nicht gefunden", id);
            return false;
        }

        qrCode.Activate();
        await _qrCodeRepository.SaveChangesAsync();

        _logger.LogInformation("QR-Code mit ID {QrCodeId} erfolgreich aktiviert", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeactivateQrCodeAsync(int id)
    {
        _logger.LogInformation("Deaktivieren des QR-Codes mit ID {QrCodeId}", id);

        var qrCode = await _qrCodeRepository.GetByIdAsync(id);
        if (qrCode == null)
        {
            _logger.LogWarning("QR-Code mit ID {QrCodeId} nicht gefunden", id);
            return false;
        }

        qrCode.Deactivate();
        await _qrCodeRepository.SaveChangesAsync();

        _logger.LogInformation("QR-Code mit ID {QrCodeId} erfolgreich deaktiviert", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteQrCodeAsync(int id)
    {
        _logger.LogInformation("Löschen des QR-Codes mit ID {QrCodeId}", id);

        var qrCode = await _qrCodeRepository.GetByIdAsync(id);
        if (qrCode == null)
        {
            _logger.LogWarning("QR-Code mit ID {QrCodeId} nicht gefunden", id);
            return false;
        }

        await _qrCodeRepository.DeleteAsync(id);
        await _qrCodeRepository.SaveChangesAsync();

        _logger.LogInformation("QR-Code mit ID {QrCodeId} erfolgreich gelöscht", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<string?> GenerateQrCodeImageAsync(int id, int size = 200)
    {
        _logger.LogInformation("Generieren des QR-Code Bildes für ID {QrCodeId} mit Größe {Size}", id, size);

        var qrCode = await _qrCodeRepository.GetByIdAsync(id);
        if (qrCode == null)
        {
            _logger.LogWarning("QR-Code mit ID {QrCodeId} nicht gefunden", id);
            return null;
        }

        return GenerateQrCodeImageForUrl(qrCode.UniqueUrl.ToString(), size);
    }

    /// <inheritdoc />
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1055:Uri return values should not be strings", Justification = "Base64-String wird zurückgegeben")]
    public string GenerateQrCodeImageForUrl(string url, int size = 200)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL darf nicht leer sein", nameof(url));
        }

        if (size <= 0 || size > 1000)
        {
            throw new ArgumentException("Größe muss zwischen 1 und 1000 Pixeln liegen", nameof(size));
        }

        try
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new Base64QRCode(qrCodeData);
            var base64String = qrCode.GetGraphic(size);

            _logger.LogDebug("QR-Code Bild erfolgreich generiert für URL {Url} mit Größe {Size}", url, size);
            return $"data:image/png;base64,{base64String}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Generieren des QR-Code Bildes für URL {Url}", url);
            throw new InvalidOperationException("Fehler beim Generieren des QR-Code Bildes", ex);
        }
    }
}
