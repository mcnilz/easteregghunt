using Microsoft.Playwright;

namespace EasterEggHunt.Web.Tests.Helpers;

/// <summary>
/// Helper-Klasse für Navigation in Playwright-Tests
/// </summary>
public static class NavigationHelper
{
    /// <summary>
    /// Navigiert zum Admin-Dashboard
    /// </summary>
    /// <param name="page">Playwright Page</param>
    /// <returns>Task</returns>
    public static async Task NavigateToAdminDashboardAsync(IPage page)
    {
        await page.GotoAsync("/Admin", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForLoadStateAsync();
    }

    /// <summary>
    /// Navigiert zur Campaigns-Übersicht
    /// </summary>
    /// <param name="page">Playwright Page</param>
    /// <returns>Task</returns>
    public static async Task NavigateToCampaignsAsync(IPage page)
    {
        await page.GotoAsync("/Admin/Campaigns", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForLoadStateAsync();
    }

    /// <summary>
    /// Navigiert zur Campaign-Erstellung
    /// </summary>
    /// <param name="page">Playwright Page</param>
    /// <returns>Task</returns>
    public static async Task NavigateToCreateCampaignAsync(IPage page)
    {
        await page.GotoAsync("/Admin/CreateCampaign", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForLoadStateAsync();
    }

    /// <summary>
    /// Navigiert zu QR-Codes für eine Kampagne
    /// </summary>
    /// <param name="page">Playwright Page</param>
    /// <param name="campaignId">Kampagnen-ID</param>
    /// <returns>Task</returns>
    public static async Task NavigateToQrCodesAsync(IPage page, int campaignId)
    {
        await page.GotoAsync($"/Admin/QrCodes/{campaignId}", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForLoadStateAsync();
    }

    /// <summary>
    /// Navigiert zur QR-Code-Erstellung für eine Kampagne
    /// </summary>
    /// <param name="page">Playwright Page</param>
    /// <param name="campaignId">Kampagnen-ID</param>
    /// <returns>Task</returns>
    public static async Task NavigateToCreateQrCodeAsync(IPage page, int campaignId)
    {
        await page.GotoAsync($"/Admin/CreateQrCode?campaignId={campaignId}", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForLoadStateAsync();
    }

    /// <summary>
    /// Navigiert zu einem QR-Code (Mitarbeiter-View)
    /// </summary>
    /// <param name="page">Playwright Page</param>
    /// <param name="qrCodeUrl">QR-Code URL (z.B. "ABC123")</param>
    /// <returns>Task</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054:URI parameters should not be strings", Justification = "QR-Code URL is a relative path segment, not a full URI")]
    public static async Task NavigateToQrCodeAsync(IPage page, string qrCodeUrl)
    {
        await page.GotoAsync($"/qr/{qrCodeUrl}", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForLoadStateAsync();
    }

    /// <summary>
    /// Navigiert zur Admin-Statistics-Seite
    /// </summary>
    /// <param name="page">Playwright Page</param>
    /// <returns>Task</returns>
    public static async Task NavigateToStatisticsAsync(IPage page)
    {
        await page.GotoAsync("/Admin/Statistics", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForLoadStateAsync();
    }

    public static Task NavigateToQrCodeAsync(IPage page, Uri qrCodeUrl)
    {
        throw new NotImplementedException();
    }
}

