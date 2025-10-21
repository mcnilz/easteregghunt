using System.Text;
using System.Text.Json;
using EasterEggHunt.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
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
        // Globales Test-Logging komplett stumm schalten
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

        services.AddDbContext<EasterEggHuntDbContext>(options =>
        {
            options.UseSqlite($"Data Source={_databasePath}");
            // Test-Logs ruhig halten
            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(false);

            // Explizit LoggerFactory für EF Core konfigurieren - komplett stumm
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.ClearProviders();
                builder.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.None);
                builder.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
                builder.AddFilter("Microsoft.EntityFrameworkCore.Infrastructure", LogLevel.None);
                builder.AddFilter("Microsoft.EntityFrameworkCore.Query", LogLevel.None);
                builder.AddFilter("Microsoft.EntityFrameworkCore.Migrations", LogLevel.None);
                builder.AddFilter("Microsoft.EntityFrameworkCore.Model", LogLevel.None);
                builder.AddFilter("Microsoft.EntityFrameworkCore.Model.Validation", LogLevel.None);
                builder.AddFilter("Microsoft.EntityFrameworkCore.Update", LogLevel.None);
                builder.AddFilter("Microsoft.EntityFrameworkCore.ChangeTracking", LogLevel.None);
                builder.AddFilter("Microsoft.EntityFrameworkCore.InMemory", LogLevel.None);
                builder.AddFilter("Microsoft.EntityFrameworkCore.Sqlite", LogLevel.None);
            });
            options.UseLoggerFactory(loggerFactory);
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

    /// <summary>
    /// Erstellt einen HttpClient mit Admin-Authentication für Workflow-Tests
    /// </summary>
    protected static async Task<HttpClient> CreateAuthenticatedAdminClientAsync(WebApplicationFactory<EasterEggHunt.Api.IApiMarker> factory)
    {
        var client = factory.CreateClient();

        // Admin-Login durchführen
        var loginData = new
        {
            Username = "admin",
            Password = "admin123"
        };

        using var loginContent = new StringContent(
            JsonSerializer.Serialize(loginData),
            Encoding.UTF8,
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
    /// Erstellt einen HttpClient mit Employee-Authentication für Workflow-Tests
    /// </summary>
    protected static async Task<HttpClient> CreateAuthenticatedEmployeeClientAsync(WebApplicationFactory<EasterEggHunt.Api.IApiMarker> factory, string userName)
    {
        var client = factory.CreateClient();

        // Employee registrieren/anmelden
        var registrationData = new
        {
            Name = userName
        };

        using var registrationContent = new StringContent(
            JsonSerializer.Serialize(registrationData),
            Encoding.UTF8,
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
    /// Führt Admin-Login durch und gibt Session-Token zurück
    /// </summary>
    protected static async Task<string> LoginAsAdminAsync(HttpClient client)
    {
        var loginData = new
        {
            Username = "admin",
            Password = "admin123"
        };

        using var loginContent = new StringContent(
            JsonSerializer.Serialize(loginData),
            Encoding.UTF8,
            "application/json");

        var loginResponse = await client.PostAsync(new Uri("/api/auth/login", UriKind.Relative), loginContent);

        if (loginResponse.IsSuccessStatusCode)
        {
            var responseContent = await loginResponse.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<JsonElement>(responseContent);

            if (loginResult.TryGetProperty("token", out var tokenElement))
            {
                return tokenElement.GetString() ?? string.Empty;
            }
        }

        throw new InvalidOperationException($"Admin login failed: {loginResponse.StatusCode}");
    }

    /// <summary>
    /// Registriert einen Employee und gibt Session-Token zurück
    /// </summary>
    protected static async Task<string> RegisterEmployeeAsync(HttpClient client, string name)
    {
        var registrationData = new
        {
            Name = name
        };

        using var registrationContent = new StringContent(
            JsonSerializer.Serialize(registrationData),
            Encoding.UTF8,
            "application/json");

        var registrationResponse = await client.PostAsync(new Uri("/api/users", UriKind.Relative), registrationContent);

        if (registrationResponse.IsSuccessStatusCode)
        {
            var responseContent = await registrationResponse.Content.ReadAsStringAsync();
            var registrationResult = JsonSerializer.Deserialize<JsonElement>(responseContent);

            if (registrationResult.TryGetProperty("sessionId", out var sessionElement))
            {
                return sessionElement.GetString() ?? string.Empty;
            }
        }

        throw new InvalidOperationException($"Employee registration failed: {registrationResponse.StatusCode}");
    }
}
