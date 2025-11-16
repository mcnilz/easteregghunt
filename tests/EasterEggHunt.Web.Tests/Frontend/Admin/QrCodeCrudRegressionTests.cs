using System.Diagnostics.CodeAnalysis;
using EasterEggHunt.Web.Tests.Helpers;
using EasterEggHunt.Web.Tests.PageObjects;
using NUnit.Framework;

namespace EasterEggHunt.Web.Tests.Frontend.Admin;

/// <summary>
/// E2E-Regressionstests für QR-Code Edit/Delete Flows im Admin-Bereich
/// </summary>
[TestFixture]
[Category("Playwright")]
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Disposed in TearDown method")]
[SuppressMessage("Design", "CA2213:Disposable fields should be disposed", Justification = "Disposed in TearDown method")]
public sealed class QrCodeCrudRegressionTests : PlaywrightTestBase
{
    [Test]
    public async Task EditQrCode_ShouldUpdateValues_AndShowSuccess()
    {
        // Arrange
        var page = await NewPageAsync();
        var loginPage = new AdminLoginPage(page);
        var campaignPage = new CampaignManagementPage(page);
        var qrPage = new QrCodeManagementPage(page);

        // Login
        await loginPage.LoginAsync(LoginHelper.DefaultAdminUsername, LoginHelper.DefaultAdminPassword);
        Assert.That(page.Url, Does.Contain("/Admin"));

        // Campaign erstellen
        var campaignName = $"E2E Crud Campaign {DateTime.Now:yyyyMMddHHmmss}";
        var campaignId = await campaignPage.CreateCampaignAsync(campaignName, "Crud Regression Tests");
        Assert.That(await campaignPage.HasSuccessMessageAsync(), Is.True);

        // QR-Code erstellen
        var originalTitle = $"QR {DateTime.Now:HHmmss}";
        await qrPage.CreateQrCodeAsync(campaignId, originalTitle, "Desc", "Notes");

        // Zur Liste navigieren und prüfen
        await qrPage.NavigateAsync(campaignId);
        Assert.That(await qrPage.HasQrCodeAsync(originalTitle), Is.True, "Erstellter QR-Code sollte gelistet sein.");

        // Edit öffnen, ändern und speichern
        await qrPage.ClickEditForAsync(originalTitle);
        var newTitle = originalTitle + " Updated";
        await qrPage.FillEditFormAsync(newTitle, "New Desc", "New Notes");
        await qrPage.SubmitEditAsync(campaignId);

        // Assert: Success und neuer Titel sichtbar
        Assert.That(await qrPage.HasSuccessMessageAsync(), Is.True, "Nach dem Edit sollte eine Success-Message erscheinen.");
        Assert.That(await qrPage.HasQrCodeAsync(newTitle), Is.True, "Aktualisierter QR-Code sollte in der Liste mit neuem Titel erscheinen.");
    }

    [Test]
    public async Task DeleteQrCode_ShouldRemoveFromList_AndShowSuccess()
    {
        // Arrange
        var page = await NewPageAsync();
        var loginPage = new AdminLoginPage(page);
        var campaignPage = new CampaignManagementPage(page);
        var qrPage = new QrCodeManagementPage(page);

        // Login
        await loginPage.LoginAsync(LoginHelper.DefaultAdminUsername, LoginHelper.DefaultAdminPassword);
        Assert.That(page.Url, Does.Contain("/Admin"));

        // Campaign erstellen
        var campaignName = $"E2E Crud Campaign {DateTime.Now:yyyyMMddHHmmss}";
        var campaignId = await campaignPage.CreateCampaignAsync(campaignName, "Crud Regression Delete");
        Assert.That(await campaignPage.HasSuccessMessageAsync(), Is.True);

        // QR-Code erstellen
        var title = $"QR-Del {DateTime.Now:HHmmss}";
        await qrPage.CreateQrCodeAsync(campaignId, title, "Desc", "Notes");

        // Zur Liste navigieren und prüfen
        await qrPage.NavigateAsync(campaignId);
        Assert.That(await qrPage.HasQrCodeAsync(title), Is.True, "Erstellter QR-Code sollte gelistet sein.");

        // Delete öffnen und bestätigen
        await qrPage.ClickDeleteForAsync(title);
        await qrPage.ConfirmDeleteAsync(campaignId);

        // Assert: Success und Eintrag nicht mehr in der Liste
        Assert.That(await qrPage.HasSuccessMessageAsync(), Is.True, "Nach dem Delete sollte eine Success-Message erscheinen.");
        Assert.That(await qrPage.HasQrCodeAsync(title), Is.False, "Gelöschter QR-Code sollte nicht mehr gelistet sein.");
    }
}
