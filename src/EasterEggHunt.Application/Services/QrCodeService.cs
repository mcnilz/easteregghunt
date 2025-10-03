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
    public async Task<QrCode> CreateQrCodeAsync(int campaignId, string title, string internalNote)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("QR-Code Titel darf nicht leer sein", nameof(title));
        
        if (string.IsNullOrWhiteSpace(internalNote))
            throw new ArgumentException("Interne Notiz darf nicht leer sein", nameof(internalNote));

        // Prüfen ob Kampagne existiert
        var campaign = await _campaignRepository.GetByIdAsync(campaignId);
        if (campaign == null)
        {
            throw new ArgumentException($"Kampagne mit ID {campaignId} nicht gefunden", nameof(campaignId));
        }

        _logger.LogInformation("Erstellen eines neuen QR-Codes für Kampagne {CampaignId}: {Title}", campaignId, title);

        var qrCode = new QrCode(campaignId, title, internalNote);
        await _qrCodeRepository.AddAsync(qrCode);
        await _qrCodeRepository.SaveChangesAsync();

        _logger.LogInformation("QR-Code erfolgreich erstellt mit ID {QrCodeId}", qrCode.Id);
        return qrCode;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateQrCodeAsync(int id, string title, string internalNote)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("QR-Code Titel darf nicht leer sein", nameof(title));
        
        if (string.IsNullOrWhiteSpace(internalNote))
            throw new ArgumentException("Interne Notiz darf nicht leer sein", nameof(internalNote));

        _logger.LogInformation("Aktualisieren des QR-Codes mit ID {QrCodeId}", id);

        var qrCode = await _qrCodeRepository.GetByIdAsync(id);
        if (qrCode == null)
        {
            _logger.LogWarning("QR-Code mit ID {QrCodeId} nicht gefunden", id);
            return false;
        }

        qrCode.Update(title, internalNote);
        await _qrCodeRepository.SaveChangesAsync();

        _logger.LogInformation("QR-Code mit ID {QrCodeId} erfolgreich aktualisiert", id);
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
}
