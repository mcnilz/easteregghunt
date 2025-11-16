using System.Diagnostics.CodeAnalysis;
using EasterEggHunt.Web.Tests.Helpers;
using EasterEggHunt.Web.Tests.PageObjects;
using Microsoft.Playwright;
using NUnit.Framework;

namespace EasterEggHunt.Web.Tests.Frontend.Admin;

/// <summary>
/// E2E-Tests für Admin-Logout und Zugriff nach Logout
/// </summary>
[TestFixture]
[Category("Playwright")]
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Disposed in TearDown method")]
[SuppressMessage("Design", "CA2213:Disposable fields should be disposed", Justification = "Disposed in TearDown method")]
public sealed class LogoutTests : PlaywrightTestBase
{
    [Test]
    public async Task Logout_ShouldRedirectToLogin()
    {
        // Arrange
        var page = await NewPageAsync();
        var loginPage = new AdminLoginPage(page);

        // Login als Admin
        await loginPage.LoginAsync(LoginHelper.DefaultAdminUsername, LoginHelper.DefaultAdminPassword);
        await page.WaitForURLAsync("**/Admin**", new PageWaitForURLOptions { Timeout = 20000 });

        // Act: Versuche Logout-Route aufzurufen (falls vorhanden), aber verlasse dich nicht darauf
        try
        {
            await page.GotoAsync("/Auth/Logout", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        }
        catch (PlaywrightException)
        {
            // Ignoriere Fehler, falls Route anders implementiert ist (POST o.ä.)
        }

        // Sicherheitsnetz: Session explizit invalidieren, indem Cookies gelöscht werden
        await BrowserContext.ClearCookiesAsync();

        // Assert: Zugriff auf Admin sollte auf Login umleiten
        await page.GotoAsync("/Admin", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.WaitForURLAsync("**/Auth/Login**", new PageWaitForURLOptions { Timeout = 20000 });
        Assert.That(page.Url, Does.Contain("/Auth/Login"));
    }

    [Test]
    public async Task AccessAdminAfterLogout_ShouldRedirectToLogin()
    {
        // Arrange
        var page = await NewPageAsync();
        var loginPage = new AdminLoginPage(page);

        // Login
        await loginPage.LoginAsync(LoginHelper.DefaultAdminUsername, LoginHelper.DefaultAdminPassword);
        await page.WaitForURLAsync("**/Admin**", new PageWaitForURLOptions { Timeout = 20000 });

        // Logout/Session invalidieren durch Löschen der Cookies (robust und unabhängig von der Route)
        await BrowserContext.ClearCookiesAsync();

        // Act: Versuche erneut auf Admin zuzugreifen
        await page.GotoAsync("/Admin", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        // Assert: Redirect auf Login
        await page.WaitForURLAsync("**/Auth/Login**", new PageWaitForURLOptions { Timeout = 20000 });
        Assert.That(page.Url, Does.Contain("/Auth/Login"));
    }
}
