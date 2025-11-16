using System.Diagnostics.CodeAnalysis;
using EasterEggHunt.Web.Tests.Helpers;
using EasterEggHunt.Web.Tests.PageObjects;
using Microsoft.Playwright;
using NUnit.Framework;

namespace EasterEggHunt.Web.Tests.Frontend.Admin;

/// <summary>
/// Tests für das Admin-Dashboard und allgemeine Erreichbarkeit nach Login
/// </summary>
[TestFixture]
[Category("Playwright")]
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Disposed in TearDown method")]
[SuppressMessage("Design", "CA2213:Disposable fields should be disposed", Justification = "Disposed in TearDown method")]
public sealed class DashboardTests : PlaywrightTestBase
{
    [Test]
    public async Task Dashboard_ShouldBeAccessible_AfterLogin()
    {
        // Arrange
        var page = await NewPageAsync();
        var loginPage = new AdminLoginPage(page);

        // Act: Login (sollte automatisch zum Dashboard weiterleiten)
        await loginPage.LoginAsync(LoginHelper.DefaultAdminUsername, LoginHelper.DefaultAdminPassword);

        // Warte explizit auf die Weiterleitung zum Dashboard
        await page.WaitForURLAsync("**/Admin**", new PageWaitForURLOptions { Timeout = 10000 });

        // Assert: Prüfe, ob wir auf einer Admin-Seite sind
        Assert.That(page.Url, Does.Contain("/Admin"), "Dashboard sollte nach Login zugänglich sein.");

        // Prüfe, ob Dashboard-Elemente vorhanden sind
        var dashboardContent = await page.QuerySelectorAsync("body");
        Assert.That(dashboardContent, Is.Not.Null, "Dashboard sollte Content haben.");
    }
}
