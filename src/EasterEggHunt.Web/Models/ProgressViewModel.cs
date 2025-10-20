using System;
using System.Collections.Generic;
using EasterEggHunt.Domain.Entities;

namespace EasterEggHunt.Web.Models;

public sealed class ProgressViewModel
{
    public string UserName { get; set; } = string.Empty;
    public string CampaignName { get; set; } = string.Empty;
    public int TotalQrCodes { get; set; }
    public int UniqueFound { get; set; }
    public int Remaining => Math.Max(0, TotalQrCodes - UniqueFound);
    public int ProgressPercent => TotalQrCodes > 0 ? (int)((double)UniqueFound / TotalQrCodes * 100) : 0;
    public bool IsCompleted => TotalQrCodes > 0 && UniqueFound >= TotalQrCodes;

    public IReadOnlyList<ProgressRecentFindItem> RecentFinds { get; set; } = Array.Empty<ProgressRecentFindItem>();
}


