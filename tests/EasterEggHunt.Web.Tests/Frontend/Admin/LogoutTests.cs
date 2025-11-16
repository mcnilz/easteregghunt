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
        var navbar = new AdminNavbar(page);

        // Login als Admin
        await loginPage.LoginAsync(LoginHelper.DefaultAdminUsername, LoginHelper.DefaultAdminPassword);
        await page.WaitForURLAsync("**/Admin**", new PageWaitForURLOptions { Timeout = 20000 });

        // Act: Logout über UI (Dropdown → Abmelden‑Button, POST)
        await navbar.LogoutAsync();

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

        // Logout per UI (Dropdown → Abmelden)
        var navbar = new AdminNavbar(page);
        await navbar.LogoutAsync();

        // Act: Versuche erneut auf Admin zuzugreifen
        await page.GotoAsync("/Admin", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        // Assert: Redirect auf Login
        await page.WaitForURLAsync("**/Auth/Login**", new PageWaitForURLOptions { Timeout = 20000 });
        Assert.That(page.Url, Does.Contain("/Auth/Login"));
    }
}
