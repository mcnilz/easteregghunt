using EasterEggHunt.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace EasterEggHunt.Application.Services;

/// <summary>
/// Service für GDPR-Compliance-Operationen
/// Implementiert das Recht auf Löschung gemäß DSGVO
/// </summary>
public class GdprService : IGdprService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IFindRepository _findRepository;
    private readonly ILogger<GdprService> _logger;

    public GdprService(
        ISessionRepository sessionRepository,
        IUserRepository userRepository,
        IFindRepository findRepository,
        ILogger<GdprService> logger)
    {
        _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _findRepository = findRepository ?? throw new ArgumentNullException(nameof(findRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<GdprDeletionResult> DeleteUserDataAsync(int userId, bool deleteFinds = false)
    {
        _logger.LogInformation("GDPR-Datenlöschung für Benutzer {UserId} gestartet (Funde löschen: {DeleteFinds})", userId, deleteFinds);

        var result = new GdprDeletionResult();

        // Prüfe ob Benutzer existiert
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Benutzer mit ID {UserId} nicht gefunden für GDPR-Löschung", userId);
            return result; // Keine Daten gelöscht
        }

        // 1. Alle Sessions des Benutzers löschen
        var deletedSessions = await _sessionRepository.DeleteAllByUserIdAsync(userId);
        result.DeletedSessions = deletedSessions;
        _logger.LogInformation("GDPR: {Count} Session(s) für Benutzer {UserId} gelöscht", deletedSessions, userId);

        // 2. Optional: Alle Funde des Benutzers löschen
        if (deleteFinds)
        {
            var finds = await _findRepository.GetByUserIdAsync(userId);
            var findsList = finds.ToList();

            foreach (var find in findsList)
            {
                var deleted = await _findRepository.DeleteAsync(find.Id);
                if (deleted)
                {
                    result.DeletedFinds++;
                }
            }
            _logger.LogInformation("GDPR: {Count} Fund(e) für Benutzer {UserId} gelöscht", findsList.Count, userId);
        }
        else
        {
            _logger.LogInformation("GDPR: Funde werden nicht gelöscht (deleteFinds=false)");
        }

        // 3. Benutzer löschen
        var userDeleted = await _userRepository.DeleteAsync(userId);
        result.UserDeleted = userDeleted;

        if (userDeleted)
        {
            _logger.LogInformation("GDPR: Benutzer {UserId} erfolgreich gelöscht. Gesamt gelöscht: {Total} Datensätze",
                userId, result.TotalDeleted);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<bool> AnonymizeUserDataAsync(int userId)
    {
        _logger.LogInformation("GDPR-Anonymisierung für Benutzer {UserId} gestartet", userId);

        // Prüfe ob Benutzer existiert
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Benutzer mit ID {UserId} nicht gefunden für GDPR-Anonymisierung", userId);
            return false;
        }

        // 1. Alle Sessions löschen
        await _sessionRepository.DeleteAllByUserIdAsync(userId);
        _logger.LogInformation("GDPR: Alle Sessions für Benutzer {UserId} gelöscht", userId);

        // 2. Benutzername anonymisieren
        user.Name = $"Anonymized_User_{userId.ToString(System.Globalization.CultureInfo.InvariantCulture)}_{Guid.NewGuid().ToString("N")[..8]}";
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("GDPR: Benutzer {UserId} erfolgreich anonymisiert", userId);
        return true;
    }
}
