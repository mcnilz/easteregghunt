using EasterEggHunt.Domain.Entities;

namespace EasterEggHunt.Application.Services;

/// <summary>
/// Service Interface für Find-Operationen
/// </summary>
public interface IFindService
{
    /// <summary>
    /// Registriert einen neuen Fund
    /// </summary>
    /// <param name="qrCodeId">QR-Code-ID</param>
    /// <param name="userId">Benutzer-ID</param>
    /// <param name="ipAddress">IP-Adresse</param>
    /// <param name="userAgent">User-Agent</param>
    /// <returns>Registrierter Fund</returns>
    Task<Find> RegisterFindAsync(int qrCodeId, int userId, string ipAddress, string userAgent);

    /// <summary>
    /// Ruft alle Funde für einen QR-Code ab
    /// </summary>
    /// <param name="qrCodeId">QR-Code-ID</param>
    /// <returns>Liste aller Funde für den QR-Code</returns>
    Task<IEnumerable<Find>> GetFindsByQrCodeIdAsync(int qrCodeId);

    /// <summary>
    /// Ruft alle Funde für einen Benutzer ab
    /// </summary>
    /// <param name="userId">Benutzer-ID</param>
    /// <returns>Liste aller Funde des Benutzers</returns>
    Task<IEnumerable<Find>> GetFindsByUserIdAsync(int userId);

    /// <summary>
    /// Ruft die Anzahl der Funde für einen QR-Code ab
    /// </summary>
    /// <param name="qrCodeId">QR-Code-ID</param>
    /// <returns>Anzahl der Funde</returns>
    Task<int> GetFindCountByQrCodeIdAsync(int qrCodeId);

    /// <summary>
    /// Ruft die Anzahl der Funde für einen Benutzer ab
    /// </summary>
    /// <param name="userId">Benutzer-ID</param>
    /// <returns>Anzahl der Funde</returns>
    Task<int> GetFindCountByUserIdAsync(int userId);
}
