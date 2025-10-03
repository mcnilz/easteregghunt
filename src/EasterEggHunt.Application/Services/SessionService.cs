using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace EasterEggHunt.Application.Services;

/// <summary>
/// Service für Session-Operationen
/// </summary>
public class SessionService : ISessionService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<SessionService> _logger;

    public SessionService(
        ISessionRepository sessionRepository,
        IUserRepository userRepository,
        ILogger<SessionService> logger)
    {
        _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<Session> CreateSessionAsync(int userId, int expirationDays = 30)
    {
        // Prüfen ob Benutzer existiert
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ArgumentException($"Benutzer mit ID {userId} nicht gefunden", nameof(userId));
        }

        _logger.LogInformation("Erstellen einer neuen Session für Benutzer {UserId} mit {ExpirationDays} Tagen Gültigkeit", userId, expirationDays);

        var session = new Session(userId, expirationDays);
        await _sessionRepository.AddAsync(session);
        await _sessionRepository.SaveChangesAsync();

        _logger.LogInformation("Session erfolgreich erstellt mit ID {SessionId}", session.Id);
        return session;
    }

    /// <inheritdoc />
    public async Task<Session?> GetSessionByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Session-ID darf nicht leer sein", nameof(id));

        _logger.LogInformation("Abrufen der Session mit ID {SessionId}", id);
        return await _sessionRepository.GetByIdAsync(id);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Session>> GetSessionsByUserIdAsync(int userId)
    {
        _logger.LogInformation("Abrufen aller Sessions für Benutzer {UserId}", userId);
        return await _sessionRepository.GetByUserIdAsync(userId);
    }

    /// <inheritdoc />
    public async Task<bool> ExtendSessionAsync(string id, int extensionDays)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Session-ID darf nicht leer sein", nameof(id));

        _logger.LogInformation("Verlängern der Session {SessionId} um {ExtensionDays} Tage", id, extensionDays);

        var session = await _sessionRepository.GetByIdAsync(id);
        if (session == null)
        {
            _logger.LogWarning("Session mit ID {SessionId} nicht gefunden", id);
            return false;
        }

        session.Extend(extensionDays);
        await _sessionRepository.SaveChangesAsync();

        _logger.LogInformation("Session {SessionId} erfolgreich um {ExtensionDays} Tage verlängert", id, extensionDays);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeactivateSessionAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Session-ID darf nicht leer sein", nameof(id));

        _logger.LogInformation("Deaktivieren der Session {SessionId}", id);

        var session = await _sessionRepository.GetByIdAsync(id);
        if (session == null)
        {
            _logger.LogWarning("Session mit ID {SessionId} nicht gefunden", id);
            return false;
        }

        session.Deactivate();
        await _sessionRepository.SaveChangesAsync();

        _logger.LogInformation("Session {SessionId} erfolgreich deaktiviert", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateSessionDataAsync(string id, string data)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Session-ID darf nicht leer sein", nameof(id));
        
        if (string.IsNullOrWhiteSpace(data))
            throw new ArgumentException("Session-Daten dürfen nicht leer sein", nameof(data));

        _logger.LogInformation("Aktualisieren der Session-Daten für Session {SessionId}", id);

        var session = await _sessionRepository.GetByIdAsync(id);
        if (session == null)
        {
            _logger.LogWarning("Session mit ID {SessionId} nicht gefunden", id);
            return false;
        }

        session.UpdateData(data);
        await _sessionRepository.SaveChangesAsync();

        _logger.LogInformation("Session-Daten für Session {SessionId} erfolgreich aktualisiert", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ValidateSessionAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Session-ID darf nicht leer sein", nameof(id));

        _logger.LogInformation("Validieren der Session {SessionId}", id);

        var session = await _sessionRepository.GetByIdAsync(id);
        if (session == null)
        {
            _logger.LogWarning("Session mit ID {SessionId} nicht gefunden", id);
            return false;
        }

        var isValid = session.IsValid();
        _logger.LogInformation("Session {SessionId} ist {Status}", id, isValid ? "gültig" : "ungültig");
        return isValid;
    }
}
