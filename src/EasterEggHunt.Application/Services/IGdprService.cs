namespace EasterEggHunt.Application.Services;

/// <summary>
/// Service Interface für GDPR-Compliance-Operationen
/// </summary>
public interface IGdprService
{
    /// <summary>
    /// Löscht alle Benutzerdaten gemäß GDPR (Recht auf Löschung)
    /// Löscht alle Sessions des Benutzers
    /// </summary>
    /// <param name="userId">Benutzer-ID</param>
    /// <param name="deleteFinds">Wenn true, werden auch alle Funde des Benutzers gelöscht (Standard: false)</param>
    /// <returns>Anzahl der gelöschten Datensätze</returns>
    Task<GdprDeletionResult> DeleteUserDataAsync(int userId, bool deleteFinds = false);

    /// <summary>
    /// Anonymisiert Benutzerdaten gemäß GDPR
    /// Löscht Sessions, anonymisiert Benutzernamen
    /// </summary>
    /// <param name="userId">Benutzer-ID</param>
    /// <returns>True wenn erfolgreich anonymisiert</returns>
    Task<bool> AnonymizeUserDataAsync(int userId);
}

/// <summary>
/// Ergebnis einer GDPR-Datenlöschung
/// </summary>
public class GdprDeletionResult
{
    /// <summary>
    /// Anzahl der gelöschten Sessions
    /// </summary>
    public int DeletedSessions { get; set; }

    /// <summary>
    /// Anzahl der gelöschten Funde (optional)
    /// </summary>
    public int DeletedFinds { get; set; }

    /// <summary>
    /// Ob der Benutzer gelöscht wurde
    /// </summary>
    public bool UserDeleted { get; set; }

    /// <summary>
    /// Gesamtanzahl der gelöschten Datensätze
    /// </summary>
    public int TotalDeleted => DeletedSessions + DeletedFinds + (UserDeleted ? 1 : 0);
}

