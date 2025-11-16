using Microsoft.Playwright;

namespace EasterEggHunt.Web.Tests.PageObjects;

/// <summary>
/// Page-Object für die Employee Navbar Navigation
/// </summary>
public sealed class EmployeeNavbar
{
    private readonly IPage _page;

    public EmployeeNavbar(IPage page)
    {
        _page = page;
    }

    public async Task GoToCampaignsAsync()
    {
        await _page.GetByRole(AriaRole.Link, new() { Name = "Kampagnen" }).ClickAsync();
        await _page.WaitForURLAsync("**/Employee/Index**");
    }

    public async Task GoToScanAsync()
    {
        await _page.GetByRole(AriaRole.Link, new() { Name = "QR-Code scannen" }).ClickAsync();
        await _page.WaitForURLAsync("**/Employee/ScanQrCode**");
    }

    public async Task GoToLeaderboardAsync()
    {
        await _page.GetByRole(AriaRole.Link, new() { Name = "Leaderboard" }).ClickAsync();
        await _page.WaitForURLAsync("**/Employee/Leaderboard**");
    }
}
