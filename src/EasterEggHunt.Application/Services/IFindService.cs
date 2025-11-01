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

    /// <summary>
    /// Ruft einen bestehenden Fund für einen QR-Code und Benutzer ab
    /// </summary>
    /// <param name="qrCodeId">QR-Code-ID</param>
    /// <param name="userId">Benutzer-ID</param>
    /// <returns>Erster Fund oder null wenn nicht gefunden</returns>
    Task<Find?> GetExistingFindAsync(int qrCodeId, int userId);

    /// <summary>
    /// Prüft, ob ein Benutzer bereits einen QR-Code gefunden hat
    /// </summary>
    /// <param name="qrCodeId">QR-Code-ID</param>
    /// <param name="userId">Benutzer-ID</param>
    /// <returns>True wenn bereits gefunden</returns>
    Task<bool> HasUserFoundQrCodeAsync(int qrCodeId, int userId);

    /// <summary>
    /// Aggregierte Kampagnen-Fundzahlen (gesamt, einzigartige Finder)
    /// </summary>
    Task<(int totalFinds, int uniqueFinders)> GetCampaignFindsAggregateAsync(int campaignId);

    /// <summary>
    /// Anzahl einzigartiger QR-Codes, die ein Benutzer gefunden hat
    /// </summary>
    Task<int> GetUniqueQrCodesCountByUserIdAsync(int userId);

    /// <summary>
    /// Benutzer-Funde für Kampagne, optional limitiert, FoundAt DESC
    /// </summary>
    Task<IEnumerable<Find>> GetFindsByUserAndCampaignAsync(int userId, int campaignId, int? take = null);

    /// <summary>
    /// Ruft Funde gruppiert nach Tag ab
    /// </summary>
    /// <param name="startDate">Startdatum (optional)</param>
    /// <param name="endDate">Enddatum (optional)</param>
    /// <returns>Funde gruppiert nach Tag</returns>
    Task<IEnumerable<(DateTime Date, int Count, int UniqueFinders, int UniqueQrCodes)>> GetDailyStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Ruft Funde gruppiert nach Woche ab
    /// </summary>
    /// <param name="startDate">Startdatum (optional)</param>
    /// <param name="endDate">Enddatum (optional)</param>
    /// <returns>Funde gruppiert nach Woche</returns>
    Task<IEnumerable<(DateTime WeekStart, int Count, int UniqueFinders, int UniqueQrCodes)>> GetWeeklyStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Ruft Funde gruppiert nach Monat ab
    /// </summary>
    /// <param name="startDate">Startdatum (optional)</param>
    /// <param name="endDate">Enddatum (optional)</param>
    /// <returns>Funde gruppiert nach Monat</returns>
    Task<IEnumerable<(DateTime MonthStart, int Count, int UniqueFinders, int UniqueQrCodes)>> GetMonthlyStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
}
