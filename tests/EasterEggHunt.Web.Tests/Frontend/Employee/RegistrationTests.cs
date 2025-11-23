using System.Diagnostics.CodeAnalysis;
using EasterEggHunt.Web.Tests.Helpers;
using EasterEggHunt.Web.Tests.PageObjects;
using Microsoft.Playwright;
using NUnit.Framework;

namespace EasterEggHunt.Web.Tests.Frontend.Employee;

/// <summary>
/// Tests für Mitarbeiter-Registrierung und Sichtbarkeit des Formulars
/// </summary>
[TestFixture]
[Category("Playwright")]
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Disposed in TearDown method")]
[SuppressMessage("Design", "CA2213:Disposable fields should be disposed", Justification = "Disposed in TearDown method")]
public sealed class RegistrationTests : PlaywrightTestBase
{
    [Test]
    public async Task ScanQrCode_ShouldShowRegistration_WhenNotRegistered()
    {
        // Arrange: Erstelle Campaign und QR-Code
        var page = await NewPageAsync();
        var loginPage = new AdminLoginPage(page);
        var campaignPage = new CampaignManagementPage(page);

        // Login als Admin
        await loginPage.LoginAsync(LoginHelper.DefaultAdminUsername, LoginHelper.DefaultAdminPassword);
        Assert.That(page.Url, Does.Contain("/Admin"), "Login sollte erfolgreich sein.");

        // Campaign erstellen
        var campaignName = $"E2E Employee Test Campaign {DateTime.Now:yyyyMMddHHmmss}";
        var campaignId = await campaignPage.CreateCampaignAsync(campaignName, "Test Description");

        // Zu QR-Codes navigieren: über UI statt direkter URL
        // 1) Zur Kampagnenliste
        await campaignPage.NavigateAsync();
        // 2) Kampagne in der Liste anklicken → Kampagnendetails
        await campaignPage.ClickCampaignAsync(campaignName);
        await page.WaitForSelectorAsync("h1");
        // 3) Von den Details per Button/Link „QR-Codes verwalten“ zur QR-Codes-Seite
        await ClickAndWaitAsync(
            page,
            page.GetByRole(AriaRole.Link, new() { Name = "QR-Codes verwalten" }),
            expectedUrlPattern: $"**/Admin/QrCodes/{campaignId}**",
            waitForSelector: "h2:has-text('QR-Codes')");

        // QR-Code erstellen
        var qrCodePage = new QrCodeManagementPage(page);
        var qrCodeTitle = $"E2E Employee Test QR {DateTime.Now:yyyyMMddHHmmss}";
        await qrCodePage.CreateQrCodeAsync(campaignId, qrCodeTitle, "Test QR Description");

        // QR-Code-URL (Test-URL)
        var qrCodeUrl = $"/qr/testcode{DateTime.Now:yyyyMMddHHmmss}";

        // Act: Navigiere zur QR-Code-URL (sollte zur Registrierung weiterleiten)
        await page.GotoAsync(qrCodeUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // Assert: Sollte auf Registrierungsseite sein
        var registrationPage = new EmployeeRegistrationPage(page);
        Assert.That(await registrationPage.IsFormVisibleAsync(), Is.True, "Registrierungsformular sollte sichtbar sein.");
    }

    [Test]
    public async Task Register_ShouldShowValidationError_WhenNameIsEmpty()
    {
        // Arrange
        var page = await NewPageAsync();
        var registrationPage = new EmployeeRegistrationPage(page);
        var qrCodeUrl = "/qr/testcode123";

        // Act: Navigiere zur Registrierung und versuche ohne Name zu registrieren
        await registrationPage.NavigateAsync(qrCodeUrl);
        await registrationPage.FillRegistrationFormAsync("");
        await page.ClickAsync("form[data-loading='true'] button[type='submit']");

        // Assert: Validierungsfehler sollte angezeigt werden
        await page.WaitForSelectorAsync(".text-danger", new PageWaitForSelectorOptions { Timeout = 5000 });
        var errorElement = await page.QuerySelectorAsync(".text-danger");
        Assert.That(errorElement, Is.Not.Null, "Validierungsfehler sollte angezeigt werden.");
    }
}
