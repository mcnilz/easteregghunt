using System.Diagnostics.CodeAnalysis;
using EasterEggHunt.Web.Tests.Helpers;
using EasterEggHunt.Web.Tests.PageObjects;
using Microsoft.Playwright;
using NUnit.Framework;

namespace EasterEggHunt.Web.Tests.Frontend;

/// <summary>
/// Playwright-Tests für kritische User-Workflows
/// Diese Tests stellen sicher, dass keine Features durch Änderungen kaputt gehen
/// </summary>
[TestFixture]
[Category("Playwright")]
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Disposed in TearDown method")]
[SuppressMessage("Design", "CA2213:Disposable fields should be disposed", Justification = "Disposed in TearDown method")]
public sealed class CriticalWorkflowsTests : PlaywrightTestBase
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

    [Test]
    public async Task AdminWorkflow_CreateCampaign_ShouldCreateAndDisplayCampaign()
    {
        // Arrange
        var page = await NewPageAsync();
        var loginPage = new AdminLoginPage(page);
        var campaignPage = new CampaignManagementPage(page);

        // Login als Admin
        await loginPage.LoginAsync(LoginHelper.DefaultAdminUsername, LoginHelper.DefaultAdminPassword);
        Assert.That(page.Url, Does.Contain("/Admin"), "Login sollte erfolgreich sein.");

        // Act: Campaign erstellen
        var campaignName = $"Test Campaign {DateTime.Now:yyyyMMddHHmmss}";
        var campaignDescription = "Test Description für Playwright-Test";

        await campaignPage.CreateCampaignAsync(campaignName, campaignDescription);

        // Assert: Campaign sollte in der Liste erscheinen
        await campaignPage.NavigateAsync();
        Assert.That(await campaignPage.HasCampaignAsync(campaignName), Is.True,
            $"Campaign '{campaignName}' sollte in der Liste erscheinen.");
    }

    [Test]
    public async Task AdminWorkflow_CreateCampaignAndQrCode_ShouldCompleteFullWorkflow()
    {
        // Arrange
        var page = await NewPageAsync();
        var loginPage = new AdminLoginPage(page);
        var campaignPage = new CampaignManagementPage(page);

        // Login als Admin
        await loginPage.LoginAsync(LoginHelper.DefaultAdminUsername, LoginHelper.DefaultAdminPassword);
        Assert.That(page.Url, Does.Contain("/Admin"), "Login sollte erfolgreich sein.");

        // Step 1: Campaign erstellen
        var campaignName = $"E2E Test Campaign {DateTime.Now:yyyyMMddHHmmss}";
        var campaignDescription = "E2E Test Description";

        await campaignPage.CreateCampaignAsync(campaignName, campaignDescription);
        Assert.That(await campaignPage.HasSuccessMessageAsync(), Is.True, "Nach Campaign-Erstellung sollte Success-Message angezeigt werden.");

        // Step 2: Zur Campaigns-Liste navigieren und Campaign finden
        await campaignPage.NavigateAsync();
        Assert.That(await campaignPage.HasCampaignAsync(campaignName), Is.True,
            $"Campaign '{campaignName}' sollte in der Liste erscheinen.");

        // Step 3: Zu QR-Codes der Campaign navigieren
        // TODO: QR-Code-Management-Page-Object erstellen und hier verwenden
        // Für jetzt: Direkte Navigation
        await page.ClickAsync($"text={campaignName}");
        await page.WaitForLoadStateAsync();

        // Prüfe, ob wir auf der Campaign-Details-Seite sind
        Assert.That(page.Url, Does.Contain("/Admin/"), "Nach Klick auf Campaign sollte zur Details-Seite navigiert werden.");
    }

    [Test]
    public async Task AdminDashboard_ShouldBeAccessible_AfterLogin()
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

