using System.Diagnostics.CodeAnalysis;
using EasterEggHunt.Web.Tests.Helpers;
using Microsoft.Playwright;
using NUnit.Framework;

namespace EasterEggHunt.Web.Tests.Frontend.Employee;

/// <summary>
/// Smoke-Tests für die Employee-Navbar-Navigation per UI-Klicks (keine direkten Goto-Navigationen zwischen den Zielseiten).
/// Ziel: Frühe 404/500-Erkennung über globalen Guard in <see cref="PlaywrightTestBase"/>.
/// </summary>
[TestFixture]
[Category("Playwright")]
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Disposed in TearDown method")]
[SuppressMessage("Design", "CA2213:Disposable fields should be disposed", Justification = "Disposed in TearDown method")]
public sealed class EmployeeNavbarSmokeTests : PlaywrightTestBase
{
    [Test]
    public async Task Navbar_Should_Navigate_To_Campaigns_Scan_Leaderboard()
    {
        // Arrange
        var page = await NewPageAsync();
        Assert.Ignore("Employee-Navbar derzeit nicht stabil testbar: Employee-Views verwenden teils Shared 'Coming Soon' oder erfordern Auth. Test wird aktiviert, sobald die Employee-UI implementiert ist.");

        // Hinweis: Der folgende Code bleibt als Vorlage erhalten und wird aktiviert, sobald die Employee-UI bereit ist.
        // Startpunkt: Seite mit Employee-Layout (Scan QR), damit die Employee-Navbar vorhanden ist
        await page.GotoAsync("/Employee/ScanQrCode", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForSelectorAsync("h1:has-text('QR-Code scannen')");

        // Act & Assert: QR-Code scannen
        await ClickAndWaitAsync(
            page,
            page.GetByRole(AriaRole.Link, new() { Name = "QR-Code scannen", Exact = true }),
            expectedUrlPattern: "**/Employee/ScanQrCode**",
            waitForSelector: "h1:has-text('QR-Code scannen')");

        // Act & Assert: Leaderboard
        await ClickAndWaitAsync(
            page,
            page.GetByRole(AriaRole.Link, new() { Name = "Leaderboard", Exact = true }),
            expectedUrlPattern: "**/Employee/Leaderboard**",
            waitForSelector: "h1:has-text('Leaderboard')");

        // Act & Assert: Zurück zu Kampagnen (via Navbar)
        await ClickAndWaitAsync(
            page,
            page.GetByRole(AriaRole.Link, new() { Name = "Kampagnen", Exact = true }),
            expectedUrlPattern: "**/Employee/Index**",
            // Index zeigt derzeit eine Coming‑Soon‑Seite im Shared‑Layout
            waitForSelector: "h1.card-title:has-text('Coming Soon')");
    }
}
