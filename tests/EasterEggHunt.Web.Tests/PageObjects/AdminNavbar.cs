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
        var navbar = _page.Locator("header nav");
        await navbar.GetByRole(AriaRole.Link, new() { Name = "Dashboard", Exact = true }).ClickAsync();
        await Task.WhenAll(
            _page.WaitForURLAsync("**/Admin**", new PageWaitForURLOptions { Timeout = 20000 }),
            _page.WaitForSelectorAsync("h1:has-text('Admin Dashboard')", new PageWaitForSelectorOptions { Timeout = 20000 })
        );
    }

    public async Task GoToStatisticsAsync()
    {
        var navbar = _page.Locator("header nav");
        await navbar.GetByRole(AriaRole.Link, new() { Name = "Statistiken", Exact = true }).ClickAsync();
        await Task.WhenAll(
            _page.WaitForURLAsync("**/Admin/Statistics**", new PageWaitForURLOptions { Timeout = 20000 }),
            _page.WaitForSelectorAsync("h1:has-text('System-Statistiken')", new PageWaitForSelectorOptions { Timeout = 20000 })
        );
    }

    public async Task GoToLeaderboardAsync()
    {
        var navbar = _page.Locator("header nav");
        await navbar.GetByRole(AriaRole.Link, new() { Name = "Rangliste", Exact = true }).ClickAsync();
        await Task.WhenAll(
            _page.WaitForURLAsync("**/Admin/Leaderboard**", new PageWaitForURLOptions { Timeout = 20000 }),
            _page.WaitForSelectorAsync("h1:has-text('Teilnehmer-Rangliste')", new PageWaitForSelectorOptions { Timeout = 20000 })
        );
    }

    public async Task GoToTimeBasedStatisticsAsync()
    {
        var navbar = _page.Locator("header nav");
        await navbar.GetByRole(AriaRole.Link, new() { Name = "Zeitbasierte Statistiken", Exact = true }).ClickAsync();
        await Task.WhenAll(
            _page.WaitForURLAsync("**/Admin/TimeBasedStatistics**", new PageWaitForURLOptions { Timeout = 20000 }),
            _page.WaitForSelectorAsync("h1:has-text('Zeitbasierte Statistiken')", new PageWaitForSelectorOptions { Timeout = 20000 })
        );
    }

    public async Task GoToFindHistoryAsync()
    {
        var navbar = _page.Locator("header nav");
        await navbar.GetByRole(AriaRole.Link, new() { Name = "Fund-Historie", Exact = true }).ClickAsync();
        await Task.WhenAll(
            _page.WaitForURLAsync("**/Admin/FindHistory**", new PageWaitForURLOptions { Timeout = 20000 }),
            _page.WaitForSelectorAsync("h1:has-text('Fund-Historie')", new PageWaitForSelectorOptions { Timeout = 20000 })
        );
    }

    public async Task GoToUsersAsync()
    {
        var navbar = _page.Locator("header nav");
        await navbar.GetByRole(AriaRole.Link, new() { Name = "Benutzer", Exact = true }).ClickAsync();
        await Task.WhenAll(
            _page.WaitForURLAsync("**/Admin/Users**", new PageWaitForURLOptions { Timeout = 20000 }),
            _page.WaitForSelectorAsync("h1:has-text('Benutzer-Übersicht')", new PageWaitForSelectorOptions { Timeout = 20000 })
        );
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
