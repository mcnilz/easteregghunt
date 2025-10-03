using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace EasterEggHunt.Application.Services;

/// <summary>
/// Service für Find-Operationen
/// </summary>
public class FindService : IFindService
{
    private readonly IFindRepository _findRepository;
    private readonly IQrCodeRepository _qrCodeRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<FindService> _logger;

    public FindService(
        IFindRepository findRepository,
        IQrCodeRepository qrCodeRepository,
        IUserRepository userRepository,
        ILogger<FindService> logger)
    {
        _findRepository = findRepository ?? throw new ArgumentNullException(nameof(findRepository));
        _qrCodeRepository = qrCodeRepository ?? throw new ArgumentNullException(nameof(qrCodeRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<Find> RegisterFindAsync(int qrCodeId, int userId, string ipAddress, string userAgent)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            throw new ArgumentException("IP-Adresse darf nicht leer sein", nameof(ipAddress));
        
        if (string.IsNullOrWhiteSpace(userAgent))
            throw new ArgumentException("User-Agent darf nicht leer sein", nameof(userAgent));

        // Prüfen ob QR-Code existiert
        var qrCode = await _qrCodeRepository.GetByIdAsync(qrCodeId);
        if (qrCode == null)
        {
            throw new ArgumentException($"QR-Code mit ID {qrCodeId} nicht gefunden", nameof(qrCodeId));
        }

        // Prüfen ob Benutzer existiert
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ArgumentException($"Benutzer mit ID {userId} nicht gefunden", nameof(userId));
        }

        _logger.LogInformation("Registrieren eines neuen Funds: QR-Code {QrCodeId}, Benutzer {UserId}", qrCodeId, userId);

        var find = new Find(qrCodeId, userId, ipAddress, userAgent);
        await _findRepository.AddAsync(find);
        await _findRepository.SaveChangesAsync();

        _logger.LogInformation("Fund erfolgreich registriert mit ID {FindId}", find.Id);
        return find;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Find>> GetFindsByQrCodeIdAsync(int qrCodeId)
    {
        _logger.LogInformation("Abrufen aller Funde für QR-Code {QrCodeId}", qrCodeId);
        return await _findRepository.GetByQrCodeIdAsync(qrCodeId);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Find>> GetFindsByUserIdAsync(int userId)
    {
        _logger.LogInformation("Abrufen aller Funde für Benutzer {UserId}", userId);
        return await _findRepository.GetByUserIdAsync(userId);
    }

    /// <inheritdoc />
    public async Task<int> GetFindCountByQrCodeIdAsync(int qrCodeId)
    {
        _logger.LogInformation("Abrufen der Fundanzahl für QR-Code {QrCodeId}", qrCodeId);
        var finds = await _findRepository.GetByQrCodeIdAsync(qrCodeId);
        return finds.Count();
    }

    /// <inheritdoc />
    public async Task<int> GetFindCountByUserIdAsync(int userId)
    {
        _logger.LogInformation("Abrufen der Fundanzahl für Benutzer {UserId}", userId);
        var finds = await _findRepository.GetByUserIdAsync(userId);
        return finds.Count();
    }
}
