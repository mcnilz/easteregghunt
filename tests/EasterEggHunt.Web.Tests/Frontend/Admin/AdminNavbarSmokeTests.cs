using System.Diagnostics.CodeAnalysis;
using EasterEggHunt.Web.Tests.Helpers;
using EasterEggHunt.Web.Tests.PageObjects;
using Microsoft.Playwright;
using NUnit.Framework;

namespace EasterEggHunt.Web.Tests.Frontend.Admin;

/// <summary>
/// Smoke-Tests für die Admin-Navbar: Navigation ausschließlich über UI-Klicks.
/// Prüft, dass zentrale Admin-Seiten erreichbar sind und charakteristische UI-Elemente vorhanden sind.
/// </summary>
[TestFixture]
[Category("Playwright")]
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Disposed in TearDown method")]
[SuppressMessage("Design", "CA2213:Disposable fields should be disposed", Justification = "Disposed in TearDown method")]
public sealed class AdminNavbarSmokeTests : PlaywrightTestBase
{
    [Test]
    public async Task Navbar_Should_Navigate_To_All_Core_Admin_Pages()
    {
        // Arrange: Login als Admin
        var page = await NewPageAsync();
        var loginPage = new AdminLoginPage(page);
        await loginPage.LoginAsync(LoginHelper.DefaultAdminUsername, LoginHelper.DefaultAdminPassword);

        // Warte, bis wir auf einer Admin-Seite sind (Dashboard)
        await page.WaitForURLAsync("**/Admin**", new PageWaitForURLOptions { Timeout = 20000 });
        await page.WaitForSelectorAsync("h1:has-text('Admin Dashboard')");

        var navbar = page.Locator("header nav");

        // Act & Assert: Statistiken
        await ClickAndWaitAsync(
            page,
            navbar.GetByRole(AriaRole.Link, new() { Name = "Statistiken", Exact = true }),
            expectedUrlPattern: "**/Admin/Statistics**",
            waitForSelector: "h1:has-text('System-Statistiken')");

        // Rangliste
        await ClickAndWaitAsync(
            page,
            navbar.GetByRole(AriaRole.Link, new() { Name = "Rangliste", Exact = true }),
            expectedUrlPattern: "**/Admin/Leaderboard**",
            waitForSelector: "h1:has-text('Teilnehmer-Rangliste')");

        // Zeitbasierte Statistiken
        await ClickAndWaitAsync(
            page,
            navbar.GetByRole(AriaRole.Link, new() { Name = "Zeitbasierte Statistiken", Exact = true }),
            expectedUrlPattern: "**/Admin/TimeBasedStatistics**",
            waitForSelector: "h1:has-text('Zeitbasierte Statistiken')");

        // Fund-Historie
        await ClickAndWaitAsync(
            page,
            navbar.GetByRole(AriaRole.Link, new() { Name = "Fund-Historie", Exact = true }),
            expectedUrlPattern: "**/Admin/FindHistory**",
            waitForSelector: "h1:has-text('Fund-Historie')");

        // Benutzer
        await ClickAndWaitAsync(
            page,
            navbar.GetByRole(AriaRole.Link, new() { Name = "Benutzer", Exact = true }),
            expectedUrlPattern: "**/Admin/Users**",
            waitForSelector: "h1:has-text('Benutzer-Übersicht')");

        // Zurück zum Dashboard
        await ClickAndWaitAsync(
            page,
            navbar.GetByRole(AriaRole.Link, new() { Name = "Dashboard", Exact = true }),
            expectedUrlPattern: "**/Admin**",
            waitForSelector: "h1:has-text('Admin Dashboard')");
    }
}
