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

        // Zu QR-Codes der Campaign navigieren
        await page.GotoAsync($"/Admin/QrCodes/{campaignId}", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // Prüfe, ob wir auf der QR-Codes-Seite sind
        Assert.That(page.Url, Does.Contain($"/Admin/QrCodes/{campaignId}"), "Nach Navigation sollte zur QR-Codes-Seite navigiert werden.");
    }
}
