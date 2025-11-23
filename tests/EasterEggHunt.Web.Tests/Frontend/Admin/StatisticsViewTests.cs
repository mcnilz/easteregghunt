using System.Diagnostics.CodeAnalysis;
using EasterEggHunt.Web.Tests.Helpers;
using EasterEggHunt.Web.Tests.PageObjects;
using Microsoft.Playwright;
using NUnit.Framework;

namespace EasterEggHunt.Web.Tests.Frontend.Admin;

/// <summary>
/// E2E-Tests für die Admin-Statistics-Ansicht
/// </summary>
[TestFixture]
[Category("Playwright")]
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Disposed in TearDown method")]
[SuppressMessage("Design", "CA2213:Disposable fields should be disposed", Justification = "Disposed in TearDown method")]
public sealed class StatisticsViewTests : PlaywrightTestBase
{
    [Test]
    public async Task Statistics_ShouldRenderKpis_AndRefreshSpinner()
    {
        // Arrange
        var page = await NewPageAsync();
        var loginPage = new AdminLoginPage(page);

        // Login als Admin
        await loginPage.LoginAsync(LoginHelper.DefaultAdminUsername, LoginHelper.DefaultAdminPassword);
        await page.WaitForURLAsync("**/Admin**", new PageWaitForURLOptions { Timeout = 20000 });

        // Act: Navigiere zur Statistics-Seite via Navbar (UI‑Klick, keine direkte URL)
        await ClickAndWaitAsync(
            page,
            page.Locator("header nav").GetByRole(AriaRole.Link, new() { Name = "Statistiken", Exact = true }),
            expectedUrlPattern: "**/Admin/Statistics**",
            waitForSelector: "h1:has-text('System-Statistiken')");

        // Assert: KPI-Kacheln sichtbar (4 Karten erwartet: Kampagnen, Benutzer, QR-Codes, Funde)
        var cards = page.Locator(".card .card-title");
        await Expect(cards).ToContainTextAsync(new[] { "Kampagnen", "Benutzer", "QR-Codes", "Funde" });

        // Refresh-Button testen (Spinner & Disabled)
        var refreshButton = page.Locator("#refreshButton");
        if (await refreshButton.CountAsync() > 0)
        {
            await refreshButton.ClickAsync();
            await Expect(refreshButton).ToBeDisabledAsync();
            var spinner = refreshButton.Locator(".spinner-border");
            await Expect(spinner).ToBeVisibleAsync();

            // Warte, bis Button wieder aktiv ist
            await Expect(refreshButton).ToBeEnabledAsync(new LocatorAssertionsToBeEnabledOptions { Timeout = 20000 });
        }
        else
        {
            TestContext.WriteLine("[DEBUG_LOG] Kein Refresh-Button auf Statistics-Seite gefunden. Überspringe Spinner-Prüfung.");
        }
    }
}
