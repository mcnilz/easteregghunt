using EasterEggHunt.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EasterEggHunt.Integration.Tests;

/// <summary>
/// Basis-Klasse für Integration Tests mit echter SQLite-Datenbank
/// </summary>
public abstract class IntegrationTestBase : IDisposable
{
    protected readonly EasterEggHuntDbContext Context;
    protected readonly IServiceProvider ServiceProvider;
    private readonly string _databasePath;

    protected IntegrationTestBase()
    {
        // Eindeutige Test-Datenbank für jeden Test
        _databasePath = Path.Combine(Path.GetTempPath(), $"easteregghunt_test_{Guid.NewGuid()}.db");

        // Service Provider für Tests erstellen
        var services = new ServiceCollection();
        // Globales Test-Logging drosseln (EF-Core ruhigstellen)
        services.AddLogging(b =>
        {
            b.ClearProviders();
            b.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Error);
            b.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
        });

        services.AddDbContext<EasterEggHuntDbContext>(options =>
        {
            options.UseSqlite($"Data Source={_databasePath}");
            // Test-Logs ruhig halten
            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(false);
        });

        ServiceProvider = services.BuildServiceProvider();
        Context = ServiceProvider.GetRequiredService<EasterEggHuntDbContext>();

        // Datenbank erstellen und migrieren
        Context.Database.EnsureCreated();

        // Test-Daten sofort laden
        SeedTestDataAsync().Wait();
    }

    /// <summary>
    /// Bereinigt die Test-Datenbank
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Bereinigt die Test-Datenbank
    /// </summary>
    /// <param name="disposing">True wenn verwaltete Ressourcen freigegeben werden sollen</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Context?.Dispose();
            if (ServiceProvider is IDisposable disposableServiceProvider)
            {
                disposableServiceProvider.Dispose();
            }

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
    }

    /// <summary>
    /// Erstellt Test-Daten in der Datenbank
    /// </summary>
    protected async Task SeedTestDataAsync()
    {
        // Prüfe ob Daten bereits vorhanden sind
        if (Context.Campaigns.Any())
            return;

        // Test-Kampagne erstellen
        var campaign = new EasterEggHunt.Domain.Entities.Campaign("Test Kampagne", "Test Beschreibung", "Test Admin")
        {
            Id = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        Context.Campaigns.Add(campaign);
        await Context.SaveChangesAsync();

        // Test-QR-Codes erstellen
        var qrCodes = new[]
        {
            new EasterEggHunt.Domain.Entities.QrCode(1, "QR Code 1", "Beschreibung 1", "Notiz 1")
            {
                Id = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new EasterEggHunt.Domain.Entities.QrCode(1, "QR Code 2", "Beschreibung 2", "Notiz 2")
            {
                Id = 2,
                IsActive = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };
        Context.QrCodes.AddRange(qrCodes);
        await Context.SaveChangesAsync();

        // Test-Benutzer erstellen
        var user = new EasterEggHunt.Domain.Entities.User("Test Benutzer")
        {
            Id = 1
        };
        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        // Test-Funde erstellen
        var finds = new[]
        {
            new EasterEggHunt.Domain.Entities.Find(1, 1, "127.0.0.1", "Test Agent")
            {
                Id = 1,
                FoundAt = DateTime.UtcNow
            },
            new EasterEggHunt.Domain.Entities.Find(2, 1, "127.0.0.1", "Test Agent")
            {
                Id = 2,
                FoundAt = DateTime.UtcNow.AddMinutes(-5)
            }
        };
        Context.Finds.AddRange(finds);
        await Context.SaveChangesAsync();
    }
}
