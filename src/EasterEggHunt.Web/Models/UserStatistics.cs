using System;

namespace EasterEggHunt.Web.Models;

/// <summary>
/// DTO für Benutzer-Statistiken (entspricht API Response)
/// </summary>
public sealed class UserStatistics
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int TotalFinds { get; set; }
    public int UniqueQrCodesFound { get; set; }
    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }
    public bool IsActive { get; set; }
    public DateTime GeneratedAt { get; set; }
}


