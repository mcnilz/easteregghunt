using EasterEggHunt.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;

namespace EasterEggHunt.Integration.Tests.Helpers;

/// <summary>
/// Basis-WebApplicationFactory für alle Integration Tests mit vorkonfiguriertem Logging
/// </summary>
public abstract class TestWebApplicationFactoryBase : WebApplicationFactory<IApiMarker>, IDisposable
{
    private bool _disposed;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Logging komplett stumm schalten für alle Tests
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.SetMinimumLevel(LogLevel.Critical);
            logging.AddFilter("Microsoft", LogLevel.None);
            logging.AddFilter("System", LogLevel.None);
            logging.AddFilter("EasterEggHunt", LogLevel.None);
        });

        // Basis-Konfiguration für Tests
        builder.UseSetting("EasterEggHunt:Database:SeedData", "false");
        builder.UseSetting("EasterEggHunt:Database:AutoMigrate", "true");
        builder.UseSetting("Cors:AllowedOrigins", "*");
        builder.UseSetting("Cors:AllowedMethods", "*");
        builder.UseSetting("Cors:AllowedHeaders", "*");
    }

    protected override void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _disposed = true;
        }
        base.Dispose(disposing);
    }

    public new void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
