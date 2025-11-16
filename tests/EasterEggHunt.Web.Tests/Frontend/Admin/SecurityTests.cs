using System.Diagnostics.CodeAnalysis;
using EasterEggHunt.Web.Tests.Helpers;
using Microsoft.Playwright;
using NUnit.Framework;

namespace EasterEggHunt.Web.Tests.Frontend.Admin;

/// <summary>
/// Sicherheits- und Zugriffstests für Admin-Bereich
/// </summary>
[TestFixture]
[Category("Playwright")]
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Disposed in TearDown method")]
[SuppressMessage("Design", "CA2213:Disposable fields should be disposed", Justification = "Disposed in TearDown method")]
public sealed class SecurityTests : PlaywrightTestBase
{
    [Test]
    public async Task AccessProtectedPage_ShouldRedirectToLogin_WhenNotAuthenticated()
    {
        // Arrange
        var page = await NewPageAsync();

        // Act: Versuche auf geschützte Admin-Seite zuzugreifen ohne Login
        await page.GotoAsync("/Admin/Campaigns", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // Assert: Sollte zur Login-Seite weitergeleitet werden
        await page.WaitForURLAsync("**/Auth/Login**", new PageWaitForURLOptions { Timeout = 5000 });
        Assert.That(page.Url, Does.Contain("/Auth/Login"), "Bei unauthentifiziertem Zugriff sollte zur Login-Seite weitergeleitet werden.");
    }
}
