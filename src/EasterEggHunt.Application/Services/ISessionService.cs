using EasterEggHunt.Domain.Entities;

namespace EasterEggHunt.Application.Services;

/// <summary>
/// Service Interface für Session-Operationen
/// </summary>
public interface ISessionService
{
    /// <summary>
    /// Erstellt eine neue Session für einen Benutzer
    /// </summary>
    /// <param name="userId">Benutzer-ID</param>
    /// <param name="expirationDays">Ablaufzeit in Tagen (Standard: 30)</param>
    /// <returns>Erstellte Session</returns>
    Task<Session> CreateSessionAsync(int userId, int expirationDays = 30);

    /// <summary>
    /// Ruft eine Session anhand der ID ab
    /// </summary>
    /// <param name="id">Session-ID</param>
    /// <returns>Session oder null wenn nicht gefunden</returns>
    Task<Session?> GetSessionByIdAsync(string id);

    /// <summary>
    /// Ruft alle Sessions für einen Benutzer ab
    /// </summary>
    /// <param name="userId">Benutzer-ID</param>
    /// <returns>Liste aller Sessions des Benutzers</returns>
    Task<IEnumerable<Session>> GetSessionsByUserIdAsync(int userId);

    /// <summary>
    /// Verlängert eine Session
    /// </summary>
    /// <param name="id">Session-ID</param>
    /// <param name="extensionDays">Verlängerung in Tagen</param>
    /// <returns>True wenn erfolgreich verlängert</returns>
    Task<bool> ExtendSessionAsync(string id, int extensionDays);

    /// <summary>
    /// Deaktiviert eine Session
    /// </summary>
    /// <param name="id">Session-ID</param>
    /// <returns>True wenn erfolgreich deaktiviert</returns>
    Task<bool> DeactivateSessionAsync(string id);

    /// <summary>
    /// Aktualisiert Session-Daten
    /// </summary>
    /// <param name="id">Session-ID</param>
    /// <param name="data">Neue Session-Daten (JSON)</param>
    /// <returns>True wenn erfolgreich aktualisiert</returns>
    Task<bool> UpdateSessionDataAsync(string id, string data);

    /// <summary>
    /// Validiert eine Session
    /// </summary>
    /// <param name="id">Session-ID</param>
    /// <returns>True wenn Session gültig ist</returns>
    Task<bool> ValidateSessionAsync(string id);
}
