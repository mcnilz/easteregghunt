using EasterEggHunt.Domain.Entities;

namespace EasterEggHunt.Domain.Repositories;

/// <summary>
/// Repository Interface für Find-Entitäten
/// </summary>
public interface IFindRepository
{
    /// <summary>
    /// Ruft alle Funde ab
    /// </summary>
    /// <returns>Liste aller Funde</returns>
    Task<IEnumerable<Find>> GetAllAsync();

    /// <summary>
    /// Ruft alle Funde eines Benutzers ab
    /// </summary>
    /// <param name="userId">Benutzer-ID</param>
    /// <returns>Liste der Funde des Benutzers</returns>
    Task<IEnumerable<Find>> GetByUserIdAsync(int userId);
    Task<int> GetCountByUserIdAsync(int userId);

    /// <summary>
    /// Ruft alle Funde eines QR-Codes ab
    /// </summary>
    /// <param name="qrCodeId">QR-Code-ID</param>
    /// <returns>Liste der Funde des QR-Codes</returns>
    Task<IEnumerable<Find>> GetByQrCodeIdAsync(int qrCodeId);
    Task<int> GetCountByQrCodeIdAsync(int qrCodeId);

    /// <summary>
    /// Ruft alle Funde einer Kampagne ab
    /// </summary>
    /// <param name="campaignId">Kampagnen-ID</param>
    /// <returns>Liste der Funde der Kampagne</returns>
    Task<IEnumerable<Find>> GetByCampaignIdAsync(int campaignId);

    /// <summary>
    /// Ruft einen Fund anhand der ID ab
    /// </summary>
    /// <param name="id">Fund-ID</param>
    /// <returns>Fund oder null wenn nicht gefunden</returns>
    Task<Find?> GetByIdAsync(int id);

    /// <summary>
    /// Prüft, ob ein Benutzer einen bestimmten QR-Code bereits gefunden hat
    /// </summary>
    /// <param name="userId">Benutzer-ID</param>
    /// <param name="qrCodeId">QR-Code-ID</param>
    /// <returns>True wenn der Benutzer den QR-Code bereits gefunden hat</returns>
    Task<bool> UserHasFoundQrCodeAsync(int userId, int qrCodeId);
    Task<Find?> GetFirstByUserAndQrAsync(int userId, int qrCodeId);

    /// <summary>
    /// Aggregierte Kennzahlen für eine Kampagne (Funde gesamt, einzigartige Finder)
    /// </summary>
    Task<(int totalFinds, int uniqueFinders)> GetCampaignFindsAggregateAsync(int campaignId);

    /// <summary>
    /// Anzahl einzigartiger QR-Codes, die ein Benutzer gefunden hat
    /// </summary>
    Task<int> GetUniqueQrCodesCountByUserIdAsync(int userId);

    /// <summary>
    /// Ruft Funde eines Benutzers für eine Kampagne ab, optional begrenzt
    /// </summary>
    Task<IEnumerable<Find>> GetByUserAndCampaignAsync(int userId, int campaignId, int? take = null);

    /// <summary>
    /// Fügt einen neuen Fund hinzu
    /// </summary>
    /// <param name="find">Fund zum Hinzufügen</param>
    /// <returns>Hinzugefügter Fund</returns>
    Task<Find> AddAsync(Find find);

    /// <summary>
    /// Aktualisiert einen bestehenden Fund
    /// </summary>
    /// <param name="find">Fund zum Aktualisieren</param>
    /// <returns>Aktualisierter Fund</returns>
    Task<Find> UpdateAsync(Find find);

    /// <summary>
    /// Löscht einen Fund
    /// </summary>
    /// <param name="id">ID des zu löschenden Funds</param>
    /// <returns>True wenn erfolgreich gelöscht</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Prüft, ob ein Fund existiert
    /// </summary>
    /// <param name="id">Fund-ID</param>
    /// <returns>True wenn der Fund existiert</returns>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Speichert alle ausstehenden Änderungen
    /// </summary>
    /// <returns>Anzahl der betroffenen Datensätze</returns>
    Task<int> SaveChangesAsync();

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

    /// <summary>
    /// Ruft Fund-Historie mit Filtern ab
    /// </summary>
    /// <param name="startDate">Startdatum (optional)</param>
    /// <param name="endDate">Enddatum (optional)</param>
    /// <param name="userId">Benutzer-ID (optional)</param>
    /// <param name="qrCodeId">QR-Code-ID (optional)</param>
    /// <param name="campaignId">Kampagnen-ID (optional)</param>
    /// <param name="skip">Anzahl zu überspringender Einträge (optional, Standard: 0)</param>
    /// <param name="take">Anzahl abzurufender Einträge (optional, Standard: 50)</param>
    /// <param name="sortBy">Sortierungsfeld (optional, Standard: "FoundAt")</param>
    /// <param name="sortDirection">Sortierungsrichtung (optional, Standard: "desc")</param>
    /// <returns>Gefilterte und paginierte Liste von Funden</returns>
    Task<IEnumerable<Find>> GetFindHistoryAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? userId = null,
        int? qrCodeId = null,
        int? campaignId = null,
        int skip = 0,
        int take = 50,
        string sortBy = "FoundAt",
        string sortDirection = "desc");

    /// <summary>
    /// Ruft die Gesamtanzahl der Funde mit den gleichen Filtern wie GetFindHistoryAsync ab
    /// </summary>
    /// <param name="startDate">Startdatum (optional)</param>
    /// <param name="endDate">Enddatum (optional)</param>
    /// <param name="userId">Benutzer-ID (optional)</param>
    /// <param name="qrCodeId">QR-Code-ID (optional)</param>
    /// <param name="campaignId">Kampagnen-ID (optional)</param>
    /// <returns>Gesamtanzahl der gefilterten Funde</returns>
    Task<int> GetFindHistoryCountAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? userId = null,
        int? qrCodeId = null,
        int? campaignId = null);
}
