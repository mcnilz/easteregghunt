using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace EasterEggHunt.Web.Tests.Helpers;

/// <summary>
/// Playwright-Fixture, die den BrowserContext mit der WebApplicationFactory-BaseURL konfiguriert
/// </summary>
public class PlaywrightFixture
{
    public IBrowserContext Context { get; private set; } = null!;
    public WebApplicationFactory Factory { get; private set; } = null!;
    public Uri BaseUrl { get; private set; } = null!;

    public async Task InitializeAsync(IPlaywright playwright, IBrowser browser)
    {
        Factory = new WebApplicationFactory();
        BaseUrl = Factory.Server.BaseAddress ?? throw new InvalidOperationException("Server BaseAddress ist null!");

        var baseUrlString = BaseUrl.ToString().TrimEnd('/');

        // Erstelle BrowserContext mit der BaseURL aus WebApplicationFactory
        Context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = baseUrlString,
            ViewportSize = new ViewportSize { Width = 1280, Height = 720 },
        });
    }

    public void Dispose()
    {
        Context?.DisposeAsync().AsTask().Wait();
        Factory?.Dispose();
    }
}

