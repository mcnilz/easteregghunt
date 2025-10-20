using System;

namespace EasterEggHunt.Web.Models;

public sealed class ProgressRecentFindItem
{
    public int QrCodeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime FoundAt { get; set; }
}


