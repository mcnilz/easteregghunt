using System.Collections.Generic;
using EasterEggHunt.Api.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EasterEggHunt.Web.Tests.Helpers;

/// <summary>
/// Test-Helper für die API-Anwendung
/// Startet einen echten Kestrel-Server für Playwright-Tests
/// </summary>
public sealed class ApiApplicationTestHost : IDisposable
{
    private WebApplication? _app;
    private Uri? _serverUrl;
    private string? _sqliteDbPath;

    /// <summary>
    /// Startet den API-Server
    /// </summary>
    /// <param name="baseUrl">Base URL für den Server (optional, standardmäßig zufälliger Port)</param>
    public async Task StartAsync(Uri? baseUrl = null)
    {
        var options = new WebApplicationOptions
        {
            EnvironmentName = "Test"
        };

        var builder = WebApplication.CreateBuilder(options);

        // Test-SQLite-DB unter dem Test-Projekt-Root vorbereiten
        // Projekt-Root (mit .sln) ermitteln
        var currentDir = new DirectoryInfo(AppContext.BaseDirectory);
        DirectoryInfo? solutionRoot = null;
        while (currentDir != null)
        {
            if (currentDir.GetFiles("*.sln").Length > 0)
            {
                solutionRoot = currentDir;
                break;
            }
            currentDir = currentDir.Parent;
        }
        if (solutionRoot == null)
        {
            throw new InvalidOperationException("Projekt-Root-Verzeichnis (mit .sln) konnte nicht gefunden werden!");
        }
        var testsRoot = Path.Combine(solutionRoot.FullName, "tests");
        var tempDir = Path.Combine(testsRoot, ".tmp");
        Directory.CreateDirectory(tempDir);
        _sqliteDbPath = Path.Combine(tempDir, $"egh-tests-{Guid.NewGuid():N}.db");
        var connectionString = $"Data Source={_sqliteDbPath}";

        // ConnectionString per In-Memory-Konfiguration setzen, bevor Services gebaut werden
        var dict = new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = connectionString
        };
        builder.Configuration.AddInMemoryCollection(dict);

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

        // Konfiguriere Services
        ApiApplicationHostBuilder.ConfigureServices(builder);

        // Baue die Anwendung
        _app = builder.Build();

        // Konfiguriere Pipeline
        ApiApplicationHostBuilder.ConfigurePipeline(_app);

        // Ensure DB created (Schema), bevor wir Requests bedienen
        using (var scope = _app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EasterEggHunt.Infrastructure.Data.EasterEggHuntDbContext>();
            await db.Database.EnsureCreatedAsync();
        }

        // Starte die Anwendung
        await _app.StartAsync();

        // Seed: In Testumgebung einen Admin-Benutzer anlegen, falls nicht vorhanden
        using (var scope = _app.Services.CreateScope())
        {
            try
            {
                var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
                if (env.IsEnvironment("Test"))
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApiApplicationTestHost>>();
                    var authService = scope.ServiceProvider.GetRequiredService<EasterEggHunt.Application.Services.IAuthService>();
                    var adminRepo = scope.ServiceProvider.GetRequiredService<EasterEggHunt.Domain.Repositories.IAdminUserRepository>();
                    var existing = await adminRepo.GetByUsernameAsync("admin");
                    if (existing == null)
                    {
                        await authService.CreateAdminAsync("admin", "admin@example.com", "admin123");
                        logger.LogInformation("Test-Adminbenutzer 'admin' wurde angelegt.");
                    }
                }
            }
            catch (Exception ex)
            {
                var log = scope.ServiceProvider.GetService<ILogger<ApiApplicationTestHost>>();
                log?.LogError(ex, "Seeding des Test-Admin-Benutzers ist fehlgeschlagen und wird abgebrochen.");
                throw;
            }
        }

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
    /// Stoppt den API-Server
    /// </summary>
    public async Task StopAsync()
    {
        if (_app != null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
            _app = null;
        }

        // Temporäre SQLite-DB-Datei aufräumen
        try
        {
            if (!string.IsNullOrWhiteSpace(_sqliteDbPath) && File.Exists(_sqliteDbPath))
            {
                File.Delete(_sqliteDbPath);
            }
        }
        catch (IOException)
        {
            // Aufräumfehler ignorieren (Datei evtl. gelockt)
        }
        catch (UnauthorizedAccessException)
        {
            // Aufräumfehler ignorieren (fehlende Berechtigung)
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

