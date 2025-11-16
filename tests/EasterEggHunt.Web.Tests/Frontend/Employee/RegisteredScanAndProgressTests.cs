using System.Diagnostics.CodeAnalysis;
using EasterEggHunt.Web.Tests.Helpers;
using EasterEggHunt.Web.Tests.PageObjects;
using Microsoft.Playwright;
using NUnit.Framework;

namespace EasterEggHunt.Web.Tests.Frontend.Employee;

/// <summary>
/// E2E-Tests: Registrierter Employee scannt QR-Code und sieht Progress-View
/// </summary>
[TestFixture]
[Category("Playwright")]
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Disposed in TearDown method")]
[SuppressMessage("Design", "CA2213:Disposable fields should be disposed", Justification = "Disposed in TearDown method")]
public sealed class RegisteredScanAndProgressTests : PlaywrightTestBase
{
    [Test]
    public async Task RegisteredEmployee_ScanQrCode_Twice_ShowsFirstSuccess_ThenAlreadyFound_AndProgress()
    {
        // Arrange
        var page = await NewPageAsync();

        // Admin erstellt Kampagne und QR-Code
        var loginPage = new AdminLoginPage(page);
        var campaignPage = new CampaignManagementPage(page);
        var qrPage = new QrCodeManagementPage(page);

        await loginPage.LoginAsync(LoginHelper.DefaultAdminUsername, LoginHelper.DefaultAdminPassword);
        await page.WaitForURLAsync("**/Admin**", new PageWaitForURLOptions { Timeout = 20000 });

        var campaignName = $"E2E Emp Campaign {DateTime.Now:yyyyMMddHHmmss}";
        var campaignId = await campaignPage.CreateCampaignAsync(campaignName, "Employee Flow Tests");
        Assert.That(await campaignPage.HasSuccessMessageAsync(), Is.True);

        var qrTitle = $"Emp-QR-{DateTime.Now:HHmmss}";
        await qrPage.CreateQrCodeAsync(campaignId, qrTitle, "Emp Desc", "Emp Notes");

        // Zur QR-Liste wechseln und den Code auslesen
        await qrPage.NavigateAsync(campaignId);
        var row = page.Locator("table tbody tr").Filter(new LocatorFilterOptions { HasText = qrTitle });
        var codeLocator = row.Locator("code");
        var qrCodeValue = await codeLocator.InnerTextAsync();
        Assert.That(qrCodeValue, Is.Not.Null.And.Not.Empty, "QR-Code-Wert sollte vorhanden sein.");

        // Act 1: Als (noch) nicht registrierter Employee QR-Scan aufrufen → Redirect zur Registrierung
        await page.GotoAsync($"/qr/{qrCodeValue}", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForURLAsync("**/Employee/Register**", new PageWaitForURLOptions { Timeout = 20000 });

        // Registrierung durchführen
        var registrationPage = new EmployeeRegistrationPage(page);
        var employeeName = $"Emp-{DateTime.Now:mmss}";
        await registrationPage.RegisterAsync(employeeName);

        // Erwartung: Nach Registrierung wird das Scan-Ergebnis angezeigt (robust auf Inhalt warten)
        await Expect(page.Locator("h2")).ToContainTextAsync("QR-Code gefunden!");

        // Assert 1: ScanResult sichtbar, Titel und Erfolgskarte vorhanden
        await Expect(page.Locator("h2")).ToContainTextAsync("QR-Code gefunden!");
        await Expect(page.Locator("h3")).ToContainTextAsync(qrTitle);

        // Act 2: Erneut denselben QR scannen → Hinweis "Bereits gefunden!"
        await page.GotoAsync($"/qr/{qrCodeValue}", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // Assert 2: Info-Alert mit "Bereits gefunden!" sichtbar
        await Expect(page.Locator(".alert-info")).ToContainTextAsync("Bereits gefunden!");

        // Act 3: Progress-View aufrufen
        await page.GotoAsync("/Employee/Progress", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // Assert 3: Progress-Seite zeigt Überschrift und mindestens einen 'Letzte Funde' Eintrag mit dem Titel
        await Expect(page.Locator("h2")).ToContainTextAsync("Dein Fortschritt");
        var progressRows = page.Locator("table tbody tr");
        var rowCount = await progressRows.CountAsync();
        Assert.That(rowCount, Is.GreaterThan(0), "Progress sollte mindestens einen Eintrag auflisten.");
        await Expect(page.Locator("table tbody")).ToContainTextAsync(qrTitle);
    }
}
