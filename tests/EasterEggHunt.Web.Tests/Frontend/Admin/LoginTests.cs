using System.Diagnostics.CodeAnalysis;
using EasterEggHunt.Web.Tests.Helpers;
using EasterEggHunt.Web.Tests.PageObjects;
using NUnit.Framework;

namespace EasterEggHunt.Web.Tests.Frontend.Admin;

/// <summary>
/// Tests rund um Admin-Login und Zugänge
/// </summary>
[TestFixture]
[Category("Playwright")]
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Disposed in TearDown method")]
[SuppressMessage("Design", "CA2213:Disposable fields should be disposed", Justification = "Disposed in TearDown method")]
public sealed class LoginTests : PlaywrightTestBase
{
    [Test]
    public async Task AdminLogin_ShouldRedirectToDashboard_WhenCredentialsAreCorrect()
    {
        // Arrange
        var page = await NewPageAsync();
        var loginPage = new AdminLoginPage(page);

        // Debug: Prüfe API-Verbindung
        TestContext.WriteLine($"API-URL: {ApiHost.ServerUrl}");
        TestContext.WriteLine($"Web-URL: {WebHost.ServerUrl}");

        // Act
        await loginPage.LoginAsync(LoginHelper.DefaultAdminUsername, LoginHelper.DefaultAdminPassword);

        // Debug: Prüfe aktuelle URL und Fehlermeldung
        TestContext.WriteLine($"Aktuelle URL nach Login: {page.Url}");
        var errorMessage = await loginPage.GetErrorMessageAsync();
        if (errorMessage != null)
        {
            TestContext.WriteLine($"Fehlermeldung: {errorMessage}");
        }

        // Assert
        Assert.That(page.Url, Does.Contain("/Admin"), "Nach erfolgreichem Login sollte zur Admin-Seite navigiert werden.");
        Assert.That(await loginPage.HasErrorMessageAsync(), Is.False, "Bei korrekten Credentials sollte keine Fehlermeldung angezeigt werden.");
    }

    [Test]
    public async Task AdminLogin_ShouldShowError_WhenCredentialsAreIncorrect()
    {
        // Arrange
        var page = await NewPageAsync();
        var loginPage = new AdminLoginPage(page);

        // Act
        await loginPage.LoginAsync("wronguser", "wrongpassword");

        // Assert
        Assert.That(await loginPage.HasErrorMessageAsync(), Is.True, "Bei falschen Credentials sollte eine Fehlermeldung angezeigt werden.");
        Assert.That(page.Url, Does.Contain("/Auth/Login"), "Bei falschen Credentials sollte auf der Login-Seite bleiben.");
    }
}
