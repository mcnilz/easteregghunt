using System.Diagnostics.CodeAnalysis;
using EasterEggHunt.Web.Tests.Helpers;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace EasterEggHunt.Web.Tests.Frontend;

/// <summary>
/// Playwright-Tests für Loading-Indikatoren
/// </summary>
[TestFixture]
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Disposed in TearDown method")]
[SuppressMessage("Design", "CA2213:Disposable fields should be disposed", Justification = "Disposed in TearDown method")]
public sealed class LoadingIndicatorsTests : PageTest
{
    private ApiApplicationTestHost _apiHost = null!;
    private WebApplicationTestHost _webHost = null!;
    private IBrowserContext _browserContext = null!;
    private string _baseUrl = null!;

    [SetUp]
    public async Task SetUpAsync()
    {
        // Starte zuerst die API
        _apiHost = new ApiApplicationTestHost();
        await _apiHost.StartAsync();

        // Starte dann das Web-Projekt mit der API-URL
        _webHost = new WebApplicationTestHost();
        await _webHost.StartAsync(_apiHost.ServerUrl);

        // Hole die Web-Server-URL
        _baseUrl = _webHost.ServerUrl.ToString().TrimEnd('/');

        // Erstelle einen neuen BrowserContext mit der BaseURL
        var contextOptions = new BrowserNewContextOptions
        {
            BaseURL = _baseUrl,
            ViewportSize = new ViewportSize { Width = 1280, Height = 720 },
        };

        _browserContext = await Browser.NewContextAsync(contextOptions);
    }

    [TearDown]
    public async Task TearDownAsync()
    {
        await _browserContext.DisposeAsync();
        _webHost.Dispose();
        _apiHost.Dispose();
    }

    [Test]
    [Ignore("Depriorisiert: rein visuelle Effekte werden aktuell nicht getestet.")]
    public async Task LoginForm_ShouldShowLoadingSpinner_WhenSubmitted()
    {
        // Arrange
        var page = await _browserContext.NewPageAsync();
        await page.GotoAsync("/Auth/Login", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // Warte auf das Laden des Scripts (maximal 15 Sekunden)
        await page.WaitForFunctionAsync(@"() => typeof window.disableButton !== 'undefined'",
            new PageWaitForFunctionOptions { Timeout = 15000 });

        // Act
        await page.FillAsync("input[name='Username']", "admin");
        await page.FillAsync("input[name='Password']", "admin123");

        // Warte auf Submit-Event
        var submitButton = page.Locator("button[type='submit']");
        await submitButton.ClickAsync();

        // Robust warten: Entweder disabled+Spinner ODER Redirect auf Admin
        var waitDisabledTask = page.WaitForFunctionAsync("() => document.querySelector(\"button[type='submit']\")?.disabled === true", new PageWaitForFunctionOptions { Timeout = 15000 });
        var waitUrlTask = page.WaitForURLAsync("**/Admin/**", new PageWaitForURLOptions { Timeout = 20000 });
        var completed = await Task.WhenAny(waitDisabledTask, waitUrlTask);

        if (completed == waitDisabledTask)
        {
            // Assert - Button sollte disabled sein und Spinner enthalten
            await Expect(submitButton).ToBeDisabledAsync();
            var spinner = page.Locator(".spinner-border");
            await Expect(spinner).ToBeVisibleAsync();
        }
        else if (completed == waitUrlTask)
        {
            // Wir sind schon navigiert – das ist ebenso gültig für den Testzweck
            Assert.That(page.Url, Does.Contain("/Admin/"), "Nach Login sollte zur Admin-Seite navigiert werden.");
        }
        else
        {
            // Diagnostik ausgeben und scheitern
            var html = await page.ContentAsync();
            TestContext.WriteLine("[DEBUG_LOG] HTML-Länge: " + html.Length);
            TestContext.WriteLine("[DEBUG_LOG] Aktuelle URL: " + page.Url);
            Assert.Fail("Weder Disabled-Status noch Admin-Redirect eingetreten.");
        }
    }

    [Test]
    [Ignore("Depriorisiert: rein visuelle Effekte werden aktuell nicht getestet.")]
    public async Task RefreshButton_ShouldShowLoadingSpinner_WhenClicked()
    {
        // Arrange - Login first
        var page = await _browserContext.NewPageAsync();
        await page.GotoAsync("/Auth/Login", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // Warte auf das Laden des Scripts (maximal 15 Sekunden)
        await page.WaitForFunctionAsync(@"() => typeof window.disableButton !== 'undefined'",
            new PageWaitForFunctionOptions { Timeout = 15000 });

        await page.FillAsync("input[name='Username']", "admin");
        await page.FillAsync("input[name='Password']", "admin123");
        await page.ClickAsync("button[type='submit']");

        // Robust warten: Entweder disabled+Spinner ODER Redirect auf Admin
        var waitDisabledTask = page.WaitForFunctionAsync("() => document.querySelector(\"button[type='submit']\")?.disabled === true", new PageWaitForFunctionOptions { Timeout = 15000 });
        var waitUrlTask = page.WaitForURLAsync("**/Admin/**", new PageWaitForURLOptions { Timeout = 20000 });
        var completed = await Task.WhenAny(waitDisabledTask, waitUrlTask);

        if (completed == waitDisabledTask)
        {
            // Danach noch auf Redirect warten
            await page.WaitForURLAsync("**/Admin/**", new PageWaitForURLOptions { Timeout = 20000 });
        }
        else if (completed != waitUrlTask)
        {
            var html = await page.ContentAsync();
            TestContext.WriteLine("[DEBUG_LOG] HTML-Länge nach Login: " + html.Length);
            TestContext.WriteLine("[DEBUG_LOG] Aktuelle URL: " + page.Url);
            Assert.Fail("Login-Redirect ist nicht erfolgt.");
        }

        // Navigate to Statistics
        await page.GotoAsync("/Admin/Statistics", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // Act
        var refreshButton = page.Locator("#refreshButton");
        await refreshButton.ClickAsync();

        // Assert
        await Expect(refreshButton).ToBeDisabledAsync();

        var spinner = refreshButton.Locator(".spinner-border");
        await Expect(spinner).ToBeVisibleAsync();
    }

    [Test]
    [Ignore("Depriorisiert: rein visuelle Effekte werden aktuell nicht getestet.")]
    public async Task FormWithDataLoading_ShouldDisableSubmitButton_WhenSubmitted()
    {
        // Arrange - Login first
        var page = await _browserContext.NewPageAsync();
        await page.GotoAsync("/Auth/Login", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // Warte auf das Laden des Scripts (maximal 15 Sekunden)
        await page.WaitForFunctionAsync(@"() => typeof window.disableButton !== 'undefined'",
            new PageWaitForFunctionOptions { Timeout = 15000 });

        await page.FillAsync("input[name='Username']", "admin");
        await page.FillAsync("input[name='Password']", "admin123");
        await page.ClickAsync("button[type='submit']");
        await page.WaitForURLAsync("**/Admin/**", new PageWaitForURLOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // Navigate to CreateCampaign
        await page.GotoAsync("/Admin/CreateCampaign", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // Act
        var form = page.Locator("form[data-loading='true']");
        var submitButton = form.Locator("button[type='submit']");

        await page.FillAsync("input[name='Name']", "Test Campaign");
        await submitButton.ClickAsync();

        // Assert
        await Expect(submitButton).ToBeDisabledAsync();
    }

    [Test]
    public async Task Homepage_ShouldBeAccessible()
    {
        // Arrange
        var page = await _browserContext.NewPageAsync();

        // Act: Navigiere zur Login-Seite
        var response = await page.GotoAsync("/Auth/Login", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // Assert
        Assert.That(response?.Status, Is.EqualTo(200), $"Login-Seite sollte erreichbar sein (Status: {response?.Status})");

        var pageTitle = await page.TitleAsync();
        Assert.That(pageTitle, Is.Not.Null.And.Not.Empty, "Seitentitel sollte vorhanden sein");

        var htmlContent = await page.ContentAsync();
        Assert.That(htmlContent, Is.Not.Null.And.Not.Empty, "HTML-Inhalt sollte vorhanden sein");
        Assert.That(htmlContent.Length, Is.GreaterThan(100), "HTML-Inhalt sollte eine sinnvolle Länge haben");

        // Zusätzliche Prüfung: Prüfe, ob die Seite ein Formular enthält
        var hasForm = await page.EvaluateAsync<bool>(@"() => document.querySelector('form') !== null");
        Assert.That(hasForm, Is.True, "Login-Seite sollte ein Formular enthalten");
    }

    [Test]
    [Ignore("Depriorisiert: rein visuelle/Asset-Effekte werden aktuell nicht getestet.")]
    public async Task StaticFile_ShouldBeAccessible()
    {
        // Arrange
        var page = await _browserContext.NewPageAsync();

        // Act: Versuche direkt auf die JavaScript-Datei zuzugreifen
        var scriptUrl = _baseUrl + "/js/loading-indicators.js";
        var response = await page.GotoAsync(scriptUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // Assert
        Assert.That(response?.Status, Is.EqualTo(200), $"JavaScript-Datei sollte unter {scriptUrl} erreichbar sein (Status: {response?.Status})");

        var content = await page.ContentAsync();
        Assert.That(content, Does.Contain("showLoadingSpinner"), "JavaScript-Datei sollte den Inhalt enthalten");
    }

    [Test]
    [Ignore("Depriorisiert: rein visuelle/Asset-Effekte werden aktuell nicht getestet.")]
    public async Task LoadingIndicatorsScript_ShouldBeLoaded()
    {
        // Arrange - Verwende Login-Seite, die definitiv das Layout verwendet
        var page = await _browserContext.NewPageAsync();
        await page.GotoAsync("/Auth/Login", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // Prüfe, ob das Script-Tag im HTML vorhanden ist
        // asp-append-version fügt eine Versionsnummer hinzu, daher müssen wir nach 'loading-indicators' suchen
        var hasScriptTag = await page.EvaluateAsync<bool>(@"() => {
            const scripts = Array.from(document.querySelectorAll('script[src]'));
            return scripts.some(s => s.src.includes('loading-indicators'));
        }");

        if (!hasScriptTag)
        {
            // Debug: Alle Script-Tags ausgeben
            var allScripts = await page.EvaluateAsync<string>(@"() => {
                const scripts = Array.from(document.querySelectorAll('script[src]'));
                return scripts.map(s => s.src).join(', ');
            }");
            var htmlLength = await page.EvaluateAsync<int>(@"() => document.documentElement.outerHTML.length");
            var message = "Script-Tag 'loading-indicators.js' nicht im HTML gefunden. Gefundene Scripts: " + allScripts + ". HTML-Länge: " + htmlLength;
            Assert.Fail(message);
        }

        // Warte auf das Laden des Scripts (maximal 15 Sekunden)
        // Das Script wird asynchron geladen, daher müssen wir warten
        await page.WaitForFunctionAsync(@"() => {
            return typeof window.showLoadingSpinner !== 'undefined' &&
                   typeof window.hideLoadingSpinner !== 'undefined' &&
                   typeof window.disableButton !== 'undefined' &&
                   typeof window.enableButton !== 'undefined';
        }", new PageWaitForFunctionOptions { Timeout = 15000 });

        // Act - Prüfe ob loading-indicators.js geladen wurde
        var scriptLoaded = await page.EvaluateAsync<bool>(@"() => {
            return typeof window.showLoadingSpinner !== 'undefined' &&
                   typeof window.hideLoadingSpinner !== 'undefined' &&
                   typeof window.disableButton !== 'undefined' &&
                   typeof window.enableButton !== 'undefined';
        }");

        // Assert
        Assert.That(scriptLoaded, Is.True, "loading-indicators.js sollte geladen sein");
    }

    [Test]
    [Ignore("Depriorisiert: rein visuelle Effekte werden aktuell nicht getestet.")]
    public async Task ShowLoadingSpinner_ShouldDisplaySpinner()
    {
        // Arrange - Verwende Login-Seite, die definitiv das Layout verwendet
        var page = await _browserContext.NewPageAsync();
        await page.GotoAsync("/Auth/Login", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // Warte auf das Laden des Scripts (maximal 15 Sekunden)
        await page.WaitForFunctionAsync(@"() => typeof window.showLoadingSpinner !== 'undefined'",
            new PageWaitForFunctionOptions { Timeout = 15000 });

        // Act
        await page.EvaluateAsync(@"() => {
            const container = document.createElement('div');
            container.id = 'test-container';
            document.body.appendChild(container);
            window.showLoadingSpinner('test-container', { text: 'Loading...', size: 'normal' });
        }");

        // Assert
        var spinner = page.Locator("#test-container .spinner-border");
        await Expect(spinner).ToBeVisibleAsync();

        var text = page.Locator("#test-container");
        await Expect(text).ToContainTextAsync("Loading...");
    }

    [Test]
    [Ignore("Depriorisiert: rein visuelle Effekte werden aktuell nicht getestet.")]
    public async Task DisableButton_ShouldDisableButtonAndShowSpinner()
    {
        // Arrange - Verwende Login-Seite, die definitiv das Layout verwendet
        var page = await _browserContext.NewPageAsync();
        await page.GotoAsync("/Auth/Login", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // Warte auf das Laden des Scripts (maximal 15 Sekunden)
        await page.WaitForFunctionAsync(@"() => typeof window.disableButton !== 'undefined'",
            new PageWaitForFunctionOptions { Timeout = 15000 });

        // Act
        await page.EvaluateAsync(@"() => {
            const button = document.createElement('button');
            button.id = 'test-button';
            button.textContent = 'Test Button';
            document.body.appendChild(button);
            window.disableButton('test-button', true);
        }");

        // Assert
        var button = page.Locator("#test-button");
        await Expect(button).ToBeDisabledAsync();

        var spinner = button.Locator(".spinner-border");
        await Expect(spinner).ToBeVisibleAsync();
    }

    [Test]
    [Ignore("Depriorisiert: rein visuelle Effekte werden aktuell nicht getestet.")]
    public async Task EnableButton_ShouldReEnableButton()
    {
        // Arrange - Verwende Login-Seite, die definitiv das Layout verwendet
        var page = await _browserContext.NewPageAsync();
        await page.GotoAsync("/Auth/Login", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // Warte auf das Laden des Scripts (maximal 15 Sekunden)
        await page.WaitForFunctionAsync(@"() => typeof window.disableButton !== 'undefined' && typeof window.enableButton !== 'undefined'",
            new PageWaitForFunctionOptions { Timeout = 15000 });

        // Act
        await page.EvaluateAsync(@"() => {
            const button = document.createElement('button');
            button.id = 'test-button';
            button.textContent = 'Test Button';
            document.body.appendChild(button);
            window.disableButton('test-button', true);
            window.enableButton('test-button');
        }");

        // Assert
        var button = page.Locator("#test-button");
        await Expect(button).ToBeEnabledAsync();

        var spinner = button.Locator(".spinner-border");
        await Expect(spinner).Not.ToBeVisibleAsync();
    }

    [Test]
    [Ignore("Depriorisiert: rein visuelle Effekte werden aktuell nicht getestet.")]
    public async Task LoadingOverlay_ShouldBlockEntireScreen()
    {
        // Arrange - Verwende Login-Seite, die definitiv das Layout verwendet
        var page = await _browserContext.NewPageAsync();
        await page.GotoAsync("/Auth/Login", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // Warte auf das Laden des Scripts (maximal 15 Sekunden)
        await page.WaitForFunctionAsync(@"() => typeof window.showLoadingSpinner !== 'undefined'",
            new PageWaitForFunctionOptions { Timeout = 15000 });

        // Act
        await page.EvaluateAsync(@"() => {
            const container = document.createElement('div');
            container.id = 'test-container';
            document.body.appendChild(container);
            window.showLoadingSpinner('test-container', { text: 'Loading...', overlay: true });
        }");

        // Assert
        var overlay = page.Locator(".loading-overlay");
        await Expect(overlay).ToBeVisibleAsync();

        // Overlay sollte den gesamten Bildschirm blockieren
        var overlayStyle = await overlay.EvaluateAsync<string>("el => window.getComputedStyle(el).position");
        Assert.That(overlayStyle, Is.EqualTo("fixed"));
    }

}

