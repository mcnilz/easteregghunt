using System.Diagnostics.CodeAnalysis;
using EasterEggHunt.Web.Tests.Helpers;
using EasterEggHunt.Web.Tests.PageObjects;
using Microsoft.Playwright;
using NUnit.Framework;

namespace EasterEggHunt.Web.Tests.Frontend.Admin;

/// <summary>
/// Tests für QR-Code-Management innerhalb des Admin-Bereichs
/// </summary>
[TestFixture]
[Category("Playwright")]
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Disposed in TearDown method")]
[SuppressMessage("Design", "CA2213:Disposable fields should be disposed", Justification = "Disposed in TearDown method")]
public sealed class QrCodeManagementTests : PlaywrightTestBase
{
    [Test]
    public async Task CreateCampaignAndNavigateToQrCodes_ShouldSucceed()
    {
        // Arrange
        var page = await NewPageAsync();
        var loginPage = new AdminLoginPage(page);
        var campaignPage = new CampaignManagementPage(page);

        // Login als Admin
        await loginPage.LoginAsync(LoginHelper.DefaultAdminUsername, LoginHelper.DefaultAdminPassword);
        Assert.That(page.Url, Does.Contain("/Admin"), "Login sollte erfolgreich sein.");

        // Campaign erstellen
        var campaignName = $"E2E Test Campaign {DateTime.Now:yyyyMMddHHmmss}";
        var campaignDescription = "E2E Test Description";

        var campaignId = await campaignPage.CreateCampaignAsync(campaignName, campaignDescription);
        Assert.That(await campaignPage.HasSuccessMessageAsync(), Is.True, "Nach Campaign-Erstellung sollte Success-Message angezeigt werden.");

        // Zur Campaigns-Liste navigieren und Campaign finden
        await campaignPage.NavigateAsync();
        Assert.That(await campaignPage.HasCampaignAsync(campaignName), Is.True,
            $"Campaign '{campaignName}' sollte in der Liste erscheinen.");

        // Zu den Kampagnendetails per UI klicken
        await campaignPage.ClickCampaignAsync(campaignName);
        await page.WaitForSelectorAsync("h1");

        // Von den Details zu den QR-Codes per UI (Button/Link) navigieren
        await ClickAndWaitAsync(
            page,
            page.GetByRole(AriaRole.Link, new() { Name = "QR-Codes verwalten" }),
            expectedUrlPattern: $"**/Admin/QrCodes/{campaignId}**",
            waitForSelector: "h2:has-text('QR-Codes')");

        // Prüfe, ob wir auf der QR-Codes-Seite sind (charakteristische Überschrift vorhanden)
        Assert.That(page.Url, Does.Contain($"/Admin/QrCodes/{campaignId}"), "Nach UI-Navigation sollte die QR-Codes-Seite geladen sein.");
    }
}
