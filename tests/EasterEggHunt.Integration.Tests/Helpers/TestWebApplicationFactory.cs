using System.Text;
using System.Text.Json;
using EasterEggHunt.Api;
using EasterEggHunt.Api.Configuration;
using EasterEggHunt.Application;
using EasterEggHunt.Infrastructure;
using EasterEggHunt.Infrastructure.Configuration;
using EasterEggHunt.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EasterEggHunt.Integration.Tests.Helpers;

/// <summary>
/// WebApplicationFactory für Integration Tests mit echter SQLite-Datenbank
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<IApiMarker>, IDisposable
{
    private readonly string _databasePath;
    private bool _disposed;

    public TestWebApplicationFactory()
    {
        // Eindeutige Test-Datenbank für jeden Test
        _databasePath = Path.Combine(Path.GetTempPath(), $"easteregghunt_webapp_test_{Guid.NewGuid()}.db");

        // Lösche die Datenbank, falls sie bereits existiert
        if (File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Entferne die ursprüngliche DbContext-Konfiguration
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

            // Test-Logging komplett stumm schalten
            services.AddLogging(b =>
            {
                b.ClearProviders();
                b.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.None);
                b.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
                b.AddFilter("Microsoft.EntityFrameworkCore.Infrastructure", LogLevel.None);
                b.AddFilter("Microsoft.EntityFrameworkCore.Query", LogLevel.None);
                b.AddFilter("Microsoft.EntityFrameworkCore.Migrations", LogLevel.None);
                b.AddFilter("Microsoft.EntityFrameworkCore.Model", LogLevel.None);
                b.AddFilter("Microsoft.EntityFrameworkCore.Model.Validation", LogLevel.None);
                b.AddFilter("Microsoft.EntityFrameworkCore.Update", LogLevel.None);
                b.AddFilter("Microsoft.EntityFrameworkCore.ChangeTracking", LogLevel.None);
                b.AddFilter("Microsoft.EntityFrameworkCore.InMemory", LogLevel.None);
                b.AddFilter("Microsoft.EntityFrameworkCore.Sqlite", LogLevel.None);
            });
        });

        // Configure the application to use test settings
        builder.UseEnvironment("Test");
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={_databasePath}",
                ["EasterEggHunt:Database:SeedData"] = "false",
                ["EasterEggHunt:Database:AutoMigrate"] = "true",
                ["Cors:AllowedOrigins"] = "*",
                ["Cors:AllowedMethods"] = "*",
                ["Cors:AllowedHeaders"] = "*"
            });
        });
    }

    /// <summary>
    /// Erstellt Test-Daten in der Datenbank
    /// </summary>
    public async Task SeedTestDataAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EasterEggHuntDbContext>();

        // Prüfe ob Daten bereits vorhanden sind
        if (context.Campaigns.Any())
            return;

        // Test-Kampagne erstellen
        var campaign = new EasterEggHunt.Domain.Entities.Campaign("Test Kampagne", "Test Beschreibung", "Test Admin")
        {
            Id = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Campaigns.Add(campaign);

        // Test-QR-Codes erstellen
        var qrCodes = new[]
        {
            new EasterEggHunt.Domain.Entities.QrCode(1, "QR Code 1", "Beschreibung 1", "Notiz 1")
            {
                Id = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Code = "testcode1"
            },
            new EasterEggHunt.Domain.Entities.QrCode(1, "QR Code 2", "Beschreibung 2", "Notiz 2")
            {
                Id = 2,
                IsActive = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Code = "testcode2"
            },
            new EasterEggHunt.Domain.Entities.QrCode(1, "QR Code 3", "Beschreibung 3", "Notiz 3")
            {
                Id = 3,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Code = "testcode3"
            }
        };
        context.QrCodes.AddRange(qrCodes);

        // Test-Benutzer erstellen
        var user = new EasterEggHunt.Domain.Entities.User("Test Benutzer")
        {
            Id = 1,
            IsActive = true
        };
        context.Users.Add(user);

        // Test-Admin-Benutzer erstellen
        var adminUser = new EasterEggHunt.Domain.Entities.AdminUser("admin", "admin@test.com", BCrypt.Net.BCrypt.HashPassword("admin123"))
        {
            Id = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.AdminUsers.Add(adminUser);

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
        context.Finds.AddRange(finds);

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Erstellt einen HttpClient mit Admin-Authentication
    /// </summary>
    public async Task<HttpClient> CreateAuthenticatedAdminClientAsync()
    {
        var client = CreateClient();

        // Admin-Login durchführen
        var loginData = new
        {
            Username = "admin",
            Password = "admin123"
        };

        using var loginContent = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(loginData),
            System.Text.Encoding.UTF8,
            "application/json");

        var loginResponse = await client.PostAsync(new Uri("/api/auth/login", UriKind.Relative), loginContent);

        if (loginResponse.IsSuccessStatusCode)
        {
            // Cookies werden automatisch vom HttpClient verwaltet
            return client;
        }

        throw new InvalidOperationException($"Admin login failed: {loginResponse.StatusCode}");
    }

    /// <summary>
    /// Erstellt einen HttpClient mit Employee-Authentication
    /// </summary>
    public async Task<HttpClient> CreateAuthenticatedEmployeeClientAsync(string userName)
    {
        var client = CreateClient();

        // Employee registrieren/anmelden
        var registrationData = new
        {
            Name = userName
        };

        using var registrationContent = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(registrationData),
            System.Text.Encoding.UTF8,
            "application/json");

        var registrationResponse = await client.PostAsync(new Uri("/api/users", UriKind.Relative), registrationContent);

        if (registrationResponse.IsSuccessStatusCode)
        {
            // Cookies werden automatisch vom HttpClient verwaltet
            return client;
        }

        throw new InvalidOperationException($"Employee registration failed: {registrationResponse.StatusCode}");
    }

    /// <summary>
    /// Bereinigt die Test-Datenbank
    /// </summary>
    public new void Dispose()
    {
        if (!_disposed)
        {
            base.Dispose();

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

            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
