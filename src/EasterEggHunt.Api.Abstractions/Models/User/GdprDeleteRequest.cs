namespace EasterEggHunterApi.Abstractions.Models.User;

/// <summary>
/// Request-Modell für GDPR-Datenlöschung
/// </summary>
public class GdprDeleteRequest
{
    /// <summary>
    /// Benutzer-ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Wenn true, werden auch alle Funde des Benutzers gelöscht (Standard: false)
    /// Hinweis: Funde werden standardmäßig nicht gelöscht, um Statistiken zu erhalten
    /// </summary>
    public bool DeleteFinds { get; set; } = false;
}

/// <summary>
/// Response-Modell für GDPR-Datenlöschung
/// </summary>
public class GdprDeleteResponse
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
    public int TotalDeleted { get; set; }
}


