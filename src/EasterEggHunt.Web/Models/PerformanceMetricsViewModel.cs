namespace EasterEggHunt.Web.Models;

/// <summary>
/// ViewModel für Performance-Metriken
/// </summary>
public class PerformanceMetricsViewModel
{
    /// <summary>
    /// Durchschnittliche Response-Zeit aller Statistiken-Endpoints (in ms)
    /// </summary>
    public double AverageResponseTimeMs { get; set; }

    /// <summary>
    /// Anzahl der API-Calls in der letzten Minute
    /// </summary>
    public int ApiCallsLastMinute { get; set; }

    /// <summary>
    /// Langsamste Response-Zeit (in ms)
    /// </summary>
    public long SlowestResponseTimeMs { get; set; }

    /// <summary>
    /// Schnellste Response-Zeit (in ms)
    /// </summary>
    public long FastestResponseTimeMs { get; set; }

    /// <summary>
    /// Anzahl der langsamen Requests (> 1s) in der letzten Stunde
    /// </summary>
    public int SlowRequestsCount { get; set; }

    /// <summary>
    /// Zeitpunkt der letzten Aktualisierung
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

