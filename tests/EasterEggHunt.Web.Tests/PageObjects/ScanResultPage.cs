using Microsoft.Playwright;

namespace EasterEggHunt.Web.Tests.PageObjects;

/// <summary>
/// Page-Object für die ScanResult-Seite des Employees
/// </summary>
public sealed class ScanResultPage
{
    private readonly IPage _page;

    public ScanResultPage(IPage page)
    {
        _page = page;
    }

    /// <summary>
    /// Klickt auf den Button/Link "Weiter suchen" und wartet auf Navigation zum Employee-Dashboard
    /// </summary>
    public async Task ClickContinueSearchAsync()
    {
        await _page.GetByRole(AriaRole.Link, new() { Name = "Weiter suchen" }).ClickAsync();
        await _page.WaitForURLAsync("**/Employee/Index**");
    }

    /// <summary>
    /// Klickt auf den Button/Link "Mein Fortschritt" und wartet auf die Progress-Seite
    /// </summary>
    public async Task ClickMyProgressAsync()
    {
        await _page.GetByRole(AriaRole.Link, new() { Name = "Mein Fortschritt" }).ClickAsync();
        await _page.WaitForURLAsync("**/Employee/Progress**");
    }
}
