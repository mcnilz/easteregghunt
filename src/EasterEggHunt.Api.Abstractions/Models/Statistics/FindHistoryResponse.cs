using EasterEggHunt.Domain.Entities;

namespace EasterEggHunterApi.Abstractions.Models.Statistics;

/// <summary>
/// Response-Modell für Fund-Historie
/// </summary>
public class FindHistoryResponse
{
    /// <summary>
    /// Liste der Funde
    /// </summary>
    public IReadOnlyList<Find> Finds { get; set; } = new List<Find>();

    /// <summary>
    /// Gesamtanzahl der gefilterten Funde
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Anzahl übersprungener Einträge
    /// </summary>
    public int Skip { get; set; }

    /// <summary>
    /// Anzahl abgerufener Einträge
    /// </summary>
    public int Take { get; set; }
}

