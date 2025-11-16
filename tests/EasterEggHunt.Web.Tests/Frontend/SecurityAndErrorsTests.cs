using System.Diagnostics.CodeAnalysis;
using EasterEggHunt.Web.Tests.Helpers;
using Microsoft.Playwright;
using NUnit.Framework;

namespace EasterEggHunt.Web.Tests.Frontend;

/// <summary>
/// E2E-Tests für Unauthorized-Redirects und 404-Fehlerseiten
/// </summary>
[TestFixture]
[Category("Playwright")]
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Disposed in TearDown method")]
[SuppressMessage("Design", "CA2213:Disposable fields should be disposed", Justification = "Disposed in TearDown method")]
public sealed class SecurityAndErrorsTests : PlaywrightTestBase
{
    [Test]
    public async Task AdminRoute_WithoutLogin_ShouldRedirectToLogin()
    {
        // Arrange
        var page = await NewPageAsync();

        // Act
        await page.GotoAsync("/Admin", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        // Assert: Redirect zur Login-Seite
        await page.WaitForURLAsync("**/Auth/Login**", new PageWaitForURLOptions { Timeout = 20000 });
        Assert.That(page.Url, Does.Contain("/Auth/Login"));
    }

    [Test]
    [Category("AllowHttpErrors")] // Bewusster 404-Test: globaler 4xx/5xx-Guard soll hier nicht greifen
    public async Task NonExistingRoute_ShouldReturnNotFoundView()
    {
        // Arrange
        var page = await NewPageAsync();
        var path = $"/definitely-not-found-{DateTime.UtcNow:yyyyMMddHHmmssfff}";

        // Act
        var response = await page.GotoAsync(path, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // Assert (robust):
        //  - Bevorzugt Status 404
        //  - Alternativ: Seite enthält typische 404-Hinweise
        if (response is not null)
        {
            var status = response.Status;
            if (status == 404)
            {
                Assert.Pass("404-Status erhalten.");
                return;
            }
        }

        // Fallback auf Inhaltsprüfung
        var bodyText = await page.InnerTextAsync("body");
        Assert.That(bodyText, Does.Contain("404").Or.Contain("Not Found").IgnoreCase,
            "Nicht vorhandene Route sollte eine 404-Information anzeigen.");
    }
}
