using EasterEggHunt.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EasterEggHunt.Integration.Tests.Helpers;

/// <summary>
/// WebApplicationFactory für Controller Integration Tests mit eigener SQLite-Datenbank
/// </summary>
public class ControllerTestWebApplicationFactory : TestWebApplicationFactoryBase
{
    private readonly string _databasePath;

    public ControllerTestWebApplicationFactory()
    {
        // Eindeutige Test-Datenbank für jeden Test
        _databasePath = Path.Combine(Path.GetTempPath(), $"easteregghunt_controller_test_{Guid.NewGuid()}.db");

        // Lösche die Datenbank, falls sie bereits existiert
        if (File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        // Setze den ConnectionString über die Konfiguration
        builder.UseSetting("ConnectionStrings:DefaultConnection", $"Data Source={_databasePath}");

        // Entferne die ursprüngliche DbContext-Konfiguration
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<EasterEggHuntDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Füge Test-DbContext hinzu
            services.AddDbContext<EasterEggHuntDbContext>(options =>
            {
                options.UseSqlite($"Data Source={_databasePath}");
                options.EnableSensitiveDataLogging(false);
                options.EnableDetailedErrors(false);
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Test-Datenbank löschen
            if (File.Exists(_databasePath))
            {
                try
                {
                    File.Delete(_databasePath);
                }
                catch (IOException)
                {
                    // Ignoriere Löschfehler
                }
            }
        }
        base.Dispose(disposing);
    }
}
