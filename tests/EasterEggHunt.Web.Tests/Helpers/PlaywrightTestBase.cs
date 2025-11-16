using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace EasterEggHunt.Web.Tests.Helpers;

/// <summary>
/// Basis-Klasse für alle Playwright-Tests
/// Stellt gemeinsame Setup/Teardown-Logik bereit
/// </summary>
[TestFixture]
[Category("Playwright")]
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Disposed in TearDown method")]
[SuppressMessage("Design", "CA2213:Disposable fields should be disposed", Justification = "Disposed in TearDown method")]
public abstract class PlaywrightTestBase : PageTest
{
    protected ApiApplicationTestHost ApiHost { get; private set; } = null!;
    protected WebApplicationTestHost WebHost { get; private set; } = null!;
    protected IBrowserContext BrowserContext { get; private set; } = null!;
    private IPlaywright? _ownedPlaywright;
    private IBrowser? _ownedBrowser;
    private bool _useOwnedBrowser;

    [SuppressMessage("Design", "CA1056:URI properties should not be strings", Justification = "Playwright BaseURL requires string")]
    protected string BaseUrl { get; private set; } = null!;

    [SetUp]
    public virtual async Task SetUpAsync()
    {
        // Prüfe, ob dieser Test im sichtbaren Browser laufen soll (NUnit Category "Headed")
        _useOwnedBrowser = HasCategory("Headed");

        // Starte zuerst die API
        ApiHost = new ApiApplicationTestHost();
        await ApiHost.StartAsync();

        // Warte, bis die API vollständig bereit ist
        await EnsureApiServerReadyAsync();

        // Starte dann das Web-Projekt mit der API-URL
        WebHost = new WebApplicationTestHost();
        await WebHost.StartAsync(ApiHost.ServerUrl);

        // Warte, bis der Web-Server vollständig bereit ist
        await EnsureWebServerReadyAsync();

        // Hole die Web-Server-URL
        BaseUrl = WebHost.ServerUrl.ToString().TrimEnd('/');

        // Erstelle einen neuen BrowserContext mit der BaseURL
        var contextOptions = new BrowserNewContextOptions
        {
            BaseURL = BaseUrl,
            ViewportSize = new ViewportSize { Width = 1280, Height = 720 },
        };

        if (_useOwnedBrowser)
        {
            // Eigene Playwright/Browser-Instanz starten, um Headed-Modus zu erzwingen
            _ownedPlaywright = await Microsoft.Playwright.Playwright.CreateAsync();

            // Optional: SlowMo via Umgebungsvariable steuern (Standard 100ms)
            var slowMoMs = 0;
            var slowMoEnv = Environment.GetEnvironmentVariable("PW_SLOWMO");
            if (!string.IsNullOrWhiteSpace(slowMoEnv) && int.TryParse(slowMoEnv, out var parsed))
            {
                slowMoMs = Math.Max(0, parsed);
            }

            _ownedBrowser = await _ownedPlaywright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false,
                SlowMo = slowMoMs > 0 ? slowMoMs : 100
            });

            BrowserContext = await _ownedBrowser.NewContextAsync(contextOptions);
            TestContext.WriteLine("Playwright Headed-Modus aktiviert (via NUnit Category 'Headed').");
        }
        else
        {
            // Standard: von der PageTest-Infrastruktur bereitgestellter Browser
            BrowserContext = await Browser.NewContextAsync(contextOptions);
        }
    }

    /// <summary>
    /// Stellt sicher, dass der API-Server vollständig gestartet und bereit ist
    /// </summary>
    private async Task EnsureApiServerReadyAsync()
    {
        const int maxRetries = 20;
        const int retryDelay = 250;

        using var httpClient = new HttpClient();
        var healthUrl = new Uri(ApiHost.ServerUrl, "/health");

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                var response = await httpClient.GetAsync(healthUrl);
                if (response.IsSuccessStatusCode)
                {
                    TestContext.WriteLine($"API-Server ist bereit: {ApiHost.ServerUrl}");

                    // Teste auch einen kritischen Endpoint (z.B. Auth-Login)
                    var authTestUrl = new Uri(ApiHost.ServerUrl, "/api/auth/login");
                    using var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
                    var authTestResponse = await httpClient.PostAsync(authTestUrl, content);

                    // 404 ist hier OK, da wir nur prüfen wollen, ob der Endpoint existiert
                    // Unauthorized (401) oder BadRequest (400) bedeutet, dass der Endpoint existiert
                    if (authTestResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        TestContext.WriteLine($"Warnung: Auth-Endpoint gibt 404 zurück. Möglicherweise sind die Routen noch nicht vollständig initialisiert.");
                        // Warte noch etwas länger
                        if (i < maxRetries - 1)
                        {
                            await Task.Delay(retryDelay * 2);
                            continue;
                        }
                    }
                    else
                    {
                        TestContext.WriteLine($"API Auth-Endpoint ist erreichbar: Status {authTestResponse.StatusCode}");
                        return; // Server ist bereit
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                TestContext.WriteLine($"API-Server noch nicht bereit (Versuch {i + 1}/{maxRetries}): {ex.Message}");
            }
            catch (TaskCanceledException)
            {
                TestContext.WriteLine($"API Health-Check Timeout (Versuch {i + 1}/{maxRetries})");
            }

            if (i < maxRetries - 1)
            {
                await Task.Delay(retryDelay);
            }
        }

        throw new InvalidOperationException($"API-Server konnte nach {maxRetries} Versuchen nicht erreicht werden. URL: {ApiHost.ServerUrl}");
    }

    /// <summary>
    /// Stellt sicher, dass der Web-Server vollständig gestartet und bereit ist
    /// </summary>
    private async Task EnsureWebServerReadyAsync()
    {
        const int maxRetries = 20;
        const int retryDelay = 250;

        using var httpClient = new HttpClient();
        var homeUrl = new Uri(WebHost.ServerUrl, "/");

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                var response = await httpClient.GetAsync(homeUrl);
                if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Redirect)
                {
                    TestContext.WriteLine($"Web-Server ist bereit: {WebHost.ServerUrl}");

                    // Teste auch die Login-Seite, da diese definitiv existieren sollte
                    var loginUrl = new Uri(WebHost.ServerUrl, "/Auth/Login");
                    var loginResponse = await httpClient.GetAsync(loginUrl);
                    if (loginResponse.IsSuccessStatusCode)
                    {
                        TestContext.WriteLine($"Web Login-Seite ist erreichbar: Status {loginResponse.StatusCode}");
                        return; // Server ist bereit
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                TestContext.WriteLine($"Web-Server noch nicht bereit (Versuch {i + 1}/{maxRetries}): {ex.Message}");
            }
            catch (TaskCanceledException)
            {
                TestContext.WriteLine($"Web Health-Check Timeout (Versuch {i + 1}/{maxRetries})");
            }

            if (i < maxRetries - 1)
            {
                await Task.Delay(retryDelay);
            }
        }

        throw new InvalidOperationException($"Web-Server konnte nach {maxRetries} Versuchen nicht erreicht werden. URL: {WebHost.ServerUrl}");
    }

    [TearDown]
    public virtual async Task TearDownAsync()
    {
        await BrowserContext.DisposeAsync();
        if (_ownedBrowser is not null)
        {
            await _ownedBrowser.DisposeAsync();
            _ownedBrowser = null;
        }
        if (_ownedPlaywright is not null)
        {
            _ownedPlaywright.Dispose();
            _ownedPlaywright = null;
        }
        WebHost.Dispose();
        ApiHost.Dispose();
    }

    /// <summary>
    /// Erstellt eine neue Page für Tests
    /// </summary>
    protected async Task<IPage> NewPageAsync()
    {
        return await BrowserContext.NewPageAsync();
    }

    private static bool HasCategory(string categoryName)
    {
        var test = TestContext.CurrentContext?.Test;
        if (test is null) return false;
        var props = test.Properties;
        if (props is null) return false;
        var categories = props["Category"];
        if (categories is null) return false;
        foreach (var c in categories)
        {
            if (string.Equals(c?.ToString(), categoryName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
}

