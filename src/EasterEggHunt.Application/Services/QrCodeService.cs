using EasterEggHunt.Application.Requests;
using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Domain.Repositories;
using Microsoft.Extensions.Logging;

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
    public async Task<QrCode?> GetQrCodeByUniqueUrlAsync(string uniqueUrl)
    {
        if (string.IsNullOrWhiteSpace(uniqueUrl))
        {
            _logger.LogWarning("UniqueUrl darf nicht leer sein");
            return null;
        }

        _logger.LogInformation("Abrufen des QR-Codes mit UniqueUrl {UniqueUrl}", uniqueUrl);

        // Konvertiere String zu Uri
        if (!Uri.TryCreate(uniqueUrl, UriKind.Absolute, out var uri))
        {
            _logger.LogWarning("Ungültige UniqueUrl: {UniqueUrl}", uniqueUrl);
            return null;
        }

        var qrCode = await _qrCodeRepository.GetByUniqueUrlAsync(uri);

        if (qrCode == null)
        {
            _logger.LogWarning("QR-Code mit UniqueUrl {UniqueUrl} nicht gefunden", uniqueUrl);
        }
        else
        {
            _logger.LogInformation("QR-Code mit UniqueUrl {UniqueUrl} gefunden: {Title}", uniqueUrl, qrCode.Title);
        }

        return qrCode;
    }
}
