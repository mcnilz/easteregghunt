using System.Diagnostics.CodeAnalysis;
using EasterEggHunt.Web.Tests.Helpers;
using EasterEggHunt.Web.Tests.PageObjects;
using Microsoft.Playwright;
using NUnit.Framework;

namespace EasterEggHunt.Web.Tests.Frontend.Admin;

/// <summary>
/// E2E-Tests für die Druckansicht der QR-Codes einer Kampagne
/// </summary>
[TestFixture]
[Category("Playwright")]
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Disposed in TearDown method")]
[SuppressMessage("Design", "CA2213:Disposable fields should be disposed", Justification = "Disposed in TearDown method")]
public sealed class PrintLayoutTests : PlaywrightTestBase
{
    [Test]
    public async Task PrintQrCodes_ShouldRender_WithMultipleEntries()
    {
        // Arrange
        var page = await NewPageAsync();
        var loginPage = new AdminLoginPage(page);
        var campaignPage = new CampaignManagementPage(page);
        var qrPage = new QrCodeManagementPage(page);

        // Login
        await loginPage.LoginAsync(LoginHelper.DefaultAdminUsername, LoginHelper.DefaultAdminPassword);
        await page.WaitForURLAsync("**/Admin**", new PageWaitForURLOptions { Timeout = 20000 });

        // Campaign erstellen
        var campaignName = $"E2E Print Campaign {DateTime.Now:yyyyMMddHHmmss}";
        var campaignId = await campaignPage.CreateCampaignAsync(campaignName, "Print Layout Tests");
        Assert.That(await campaignPage.HasSuccessMessageAsync(), Is.True);

        // Mind. 3 QR-Codes anlegen
        for (int i = 1; i <= 3; i++)
        {
            await qrPage.CreateQrCodeAsync(campaignId, $"QR-Print-{i}", "Print Desc", "Print Notes");
        }

        // Act: Drucklayout öffnen
        await page.GotoAsync($"/Admin/PrintQrCodes/{campaignId}", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // Assert: Seite rendert ohne Fehler und enthält erwartete Inhalte
        // Titelzeile enthält den Kampagnennamen, und Einleitungstext ist vorhanden
        await Expect(page.Locator("h1")).ToContainTextAsync(campaignName);
        await Expect(page.Locator(".print-header p").First).ToContainTextAsync("QR-Codes zum Drucken");

        // Es sollten mindestens 3 QR-Code Blöcke vorhanden sein
        var items = page.Locator(".qr-code-item");
        await Expect(items).ToHaveCountAsync(3);

        // Stichprobe: Ein Titeltext vorhanden
        await Expect(page.Locator(".qr-code-title")).ToContainTextAsync(new[] { "QR-Print-1" });

        // Prüfe, dass das Script für QR-Code Rendering vorhanden ist (qrcode.min.js Referenz)
        var hasQrScript = await page.EvaluateAsync<bool>(@"() => Array.from(document.scripts).some(s => (s.src||'').includes('qrcode'))");
        Assert.That(hasQrScript, Is.True, "qrcode.min.js sollte eingebunden sein.");
    }
}
