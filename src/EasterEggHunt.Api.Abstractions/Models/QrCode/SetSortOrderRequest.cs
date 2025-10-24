using System.ComponentModel.DataAnnotations;

namespace EasterEggHunterApi.Abstractions.Models.QrCode;

/// <summary>
/// Request-Model für Sortierreihenfolge-Änderung
/// </summary>
public class SetSortOrderRequest
{
    /// <summary>
    /// Neue Sortierreihenfolge
    /// </summary>
    [Required(ErrorMessage = "Sortierreihenfolge ist erforderlich")]
    [Range(0, int.MaxValue, ErrorMessage = "Sortierreihenfolge muss größer oder gleich 0 sein")]
    public int SortOrder { get; set; }
}
