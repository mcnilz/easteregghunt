using EasterEggHunt.Web.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;

namespace EasterEggHunt.Web.Tests.Helpers;

/// <summary>
/// Test-Helper für die Web-Anwendung
/// Startet einen echten Kestrel-Server für Playwright-Tests
/// </summary>
public sealed class WebApplicationTestHost : IDisposable
{
    private WebApplication? _app;
    private Uri? _serverUrl;

    /// <summary>
    /// Startet den Web-Server
    /// </summary>
    /// <param name="apiBaseUrl">API Base URL (erforderlich)</param>
    /// <param name="baseUrl">Base URL für den Web-Server (optional, standardmäßig zufälliger Port)</param>
    public async Task StartAsync(Uri apiBaseUrl, Uri? baseUrl = null)
    {
        if (apiBaseUrl == null)
        {
            throw new ArgumentNullException(nameof(apiBaseUrl));
        }

        // Setze Content Root Path auf das Web-Projekt-Verzeichnis
        // Finde das Projekt-Root-Verzeichnis durch Suchen nach der .sln-Datei
        var currentDir = new DirectoryInfo(AppContext.BaseDirectory);
        DirectoryInfo? projectRoot = null;
        while (currentDir != null)
        {
            if (currentDir.GetFiles("*.sln").Length > 0)
            {
                projectRoot = currentDir;
                break;
            }
            currentDir = currentDir.Parent;
        }

        if (projectRoot == null)
        {
            throw new InvalidOperationException("Projekt-Root-Verzeichnis konnte nicht gefunden werden!");
        }

        var webProjectPath = Path.Combine(projectRoot.FullName, "src", "EasterEggHunt.Web");

        var options = new WebApplicationOptions
        {
            ContentRootPath = webProjectPath,
            WebRootPath = Path.Combine(webProjectPath, "wwwroot"),
            EnvironmentName = "Test",
            ApplicationName = typeof(EasterEggHunt.Web.IWebMarker).Assembly.GetName().Name
        };

        var builder = WebApplication.CreateBuilder(options);

        // Konfiguriere Kestrel für echten HTTP-Server
        //builder.WebHost.UseKestrel();
        if (baseUrl != null)
        {
            builder.WebHost.UseUrls(baseUrl.ToString());
        }
        else
        {
            builder.WebHost.UseUrls("http://127.0.0.1:0"); // Zufälliger Port (127.0.0.1 statt localhost für dynamischen Port)
        }

        // Konfiguriere Services mit API-URL
        WebApplicationHostBuilder.ConfigureServices(builder, apiBaseUrl);

        // Baue die Anwendung
        _app = builder.Build();

        // Konfiguriere Pipeline
        WebApplicationHostBuilder.ConfigurePipeline(_app);

        // Starte die Anwendung
        await _app.StartAsync();

        // Warte kurz, damit der Server vollständig gestartet ist
        await Task.Delay(100);

        // Extrahiere die tatsächliche Server-URL
        var server = _app.Services.GetRequiredService<IServer>();
        var addresses = server.Features.Get<IServerAddressesFeature>();
        var urlString = addresses?.Addresses.FirstOrDefault();

        if (urlString == null)
        {
            throw new InvalidOperationException("Server-Adresse konnte nicht ermittelt werden!");
        }

        _serverUrl = new Uri(urlString);
    }

    /// <summary>
    /// Stoppt den Web-Server
    /// </summary>
    public async Task StopAsync()
    {
        if (_app != null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
            _app = null;
        }
    }

    /// <summary>
    /// Gibt die Server-URL zurück
    /// </summary>
    public Uri ServerUrl
    {
        get => _serverUrl ?? throw new InvalidOperationException("Server wurde nicht gestartet!");
    }

    /// <summary>
    /// Disposed den Host
    /// </summary>
    public void Dispose()
    {
        StopAsync().GetAwaiter().GetResult();
    }
}

