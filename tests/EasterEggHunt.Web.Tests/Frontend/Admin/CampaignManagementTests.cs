using System.Diagnostics.CodeAnalysis;
using EasterEggHunt.Web.Tests.Helpers;
using EasterEggHunt.Web.Tests.PageObjects;
using Microsoft.Playwright;
using NUnit.Framework;

namespace EasterEggHunt.Web.Tests.Frontend.Admin;

/// <summary>
/// Tests für Kampagnenverwaltung (Erstellung, Validierung)
/// </summary>
[TestFixture]
[Category("Playwright")]
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Disposed in TearDown method")]
[SuppressMessage("Design", "CA2213:Disposable fields should be disposed", Justification = "Disposed in TearDown method")]
public sealed class CampaignManagementTests : PlaywrightTestBase
{
    [Test]
    public async Task CreateCampaign_ShouldCreateAndDisplayCampaign()
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
    public async Task CreateCampaign_ShouldShowValidationError_WhenNameIsEmpty()
    {
        // Arrange
        var page = await NewPageAsync();
        var loginPage = new AdminLoginPage(page);
        var campaignPage = new CampaignManagementPage(page);

        // Login als Admin
        await loginPage.LoginAsync(LoginHelper.DefaultAdminUsername, LoginHelper.DefaultAdminPassword);
        Assert.That(page.Url, Does.Contain("/Admin"), "Login sollte erfolgreich sein.");

        // Act: Versuche Campaign ohne Name zu erstellen
        await campaignPage.NavigateToCreateAsync();
        await campaignPage.FillCreateFormAsync("", "Test Description");
        await page.ClickAsync("form[data-loading='true'] button[type='submit']");

        // Assert: Validierungsfehler sollte angezeigt werden
        await page.WaitForSelectorAsync(".text-danger", new PageWaitForSelectorOptions { Timeout = 5000 });
        var errorElement = await page.QuerySelectorAsync(".text-danger");
        Assert.That(errorElement, Is.Not.Null, "Validierungsfehler sollte angezeigt werden.");
    }
}
