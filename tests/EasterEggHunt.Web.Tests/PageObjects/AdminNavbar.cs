using Microsoft.Playwright;

namespace EasterEggHunt.Web.Tests.PageObjects;

/// <summary>
/// Page-Object für die Admin Navbar Navigation (Shared/_Layout)
/// </summary>
public sealed class AdminNavbar
{
    private readonly IPage _page;

    public AdminNavbar(IPage page)
    {
        _page = page;
    }

    public async Task GoToDashboardAsync()
    {
        await _page.GetByRole(AriaRole.Link, new() { Name = "Dashboard" }).ClickAsync();
        await _page.WaitForURLAsync("**/Admin/Index**");
    }

    public async Task GoToStatisticsAsync()
    {
        await _page.GetByRole(AriaRole.Link, new() { Name = "Statistiken" }).ClickAsync();
        await _page.WaitForURLAsync("**/Admin/Statistics**");
    }

    public async Task GoToLeaderboardAsync()
    {
        await _page.GetByRole(AriaRole.Link, new() { Name = "Rangliste" }).ClickAsync();
        await _page.WaitForURLAsync("**/Admin/Leaderboard**");
    }

    public async Task GoToTimeBasedStatisticsAsync()
    {
        await _page.GetByRole(AriaRole.Link, new() { Name = "Zeitbasierte Statistiken" }).ClickAsync();
        await _page.WaitForURLAsync("**/Admin/TimeBasedStatistics**");
    }

    public async Task GoToFindHistoryAsync()
    {
        await _page.GetByRole(AriaRole.Link, new() { Name = "Fund-Historie" }).ClickAsync();
        await _page.WaitForURLAsync("**/Admin/FindHistory**");
    }

    public async Task GoToUsersAsync()
    {
        await _page.GetByRole(AriaRole.Link, new() { Name = "Benutzer" }).ClickAsync();
        await _page.WaitForURLAsync("**/Admin/Users**");
    }

    public async Task LogoutAsync()
    {
        // Dropdown öffnen (enthält das Logout-Formular)
        var dropdown = _page.Locator("#navbarDropdown");
        await dropdown.ClickAsync();

        // Auf den Dropdown-Button "Abmelden" klicken (POST-Formular)
        await _page.GetByRole(AriaRole.Button, new() { Name = "Abmelden" }).ClickAsync();

        // Erwartung: Redirect zur Login-Seite
        await _page.WaitForURLAsync("**/Auth/Login**", new PageWaitForURLOptions { Timeout = 20000 });
    }
}
