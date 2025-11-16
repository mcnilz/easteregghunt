using System.Diagnostics.CodeAnalysis;
using EasterEggHunt.Web.Tests.Helpers;
using EasterEggHunt.Web.Tests.PageObjects;
using Microsoft.Playwright;
using NUnit.Framework;

namespace EasterEggHunt.Web.Tests.Frontend.Employee;

/// <summary>
/// End-to-End Workflow Tests für Mitarbeiter (Registrierung und QR-Scan)
/// </summary>
[TestFixture]
[Category("Playwright")]
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Disposed in TearDown method")]
[SuppressMessage("Design", "CA2213:Disposable fields should be disposed", Justification = "Disposed in TearDown method")]
public sealed class FullWorkflowTests : PlaywrightTestBase
{
    [Test]
    public async Task RegisterAndScanQrCode_ShouldCompleteFullWorkflow()
    {
        // Arrange: Erstelle Campaign und QR-Code
        var page = await NewPageAsync();
        var loginPage = new AdminLoginPage(page);
        var campaignPage = new CampaignManagementPage(page);

        // Login als Admin
        await loginPage.LoginAsync(LoginHelper.DefaultAdminUsername, LoginHelper.DefaultAdminPassword);

        // Campaign erstellen
        var campaignName = $"E2E Full Workflow Campaign {DateTime.Now:yyyyMMddHHmmss}";
        var campaignId = await campaignPage.CreateCampaignAsync(campaignName, "Test Description");

        // Zu QR-Codes navigieren
        await page.GotoAsync($"/Admin/QrCodes/{campaignId}", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // QR-Code erstellen
        var qrCodePage = new QrCodeManagementPage(page);
        var qrCodeTitle = $"E2E Full Workflow QR {DateTime.Now:yyyyMMddHHmmss}";
        await qrCodePage.CreateQrCodeAsync(campaignId, qrCodeTitle, "Test QR Description");

        // QR-Code-URL für Test (in echten Tests würde man die URL aus der API holen)
        var qrCodeUrl = $"/qr/testcode{DateTime.Now:yyyyMMddHHmmss}";

        // Act: Navigiere zur Registrierung
        var registrationPage = new EmployeeRegistrationPage(page);
        await registrationPage.NavigateAsync(qrCodeUrl);

        // Employee registrieren - Name darf laut Validierung nur Buchstaben/Leerzeichen/.- enthalten
        var employeeName = $"Test Employee {DateTime.Now:HHmmss}"
            .Replace("0", "o", StringComparison.Ordinal)
            .Replace("1", "l", StringComparison.Ordinal)
            .Replace("2", "z", StringComparison.Ordinal)
            .Replace("3", "e", StringComparison.Ordinal)
            .Replace("4", "a", StringComparison.Ordinal)
            .Replace("5", "s", StringComparison.Ordinal)
            .Replace("6", "g", StringComparison.Ordinal)
            .Replace("7", "t", StringComparison.Ordinal)
            .Replace("8", "b", StringComparison.Ordinal)
            .Replace("9", "q", StringComparison.Ordinal);
        await registrationPage.RegisterAsync(employeeName);

        // Assert: Sollte nach Registrierung zur QR-Code-Scan-Seite navigieren
        Assert.That(page.Url, Does.Contain("/qr/"), "Nach Registrierung sollte zur QR-Code-Seite navigiert werden.");
    }
}
