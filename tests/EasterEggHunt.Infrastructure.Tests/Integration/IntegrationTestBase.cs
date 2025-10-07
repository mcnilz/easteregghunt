using System.IO;
using EasterEggHunt.Domain.Entities; // Hinzugefügt für SeedTestDataAsync
using EasterEggHunt.Domain.Repositories;
using EasterEggHunt.Infrastructure.Data;
using EasterEggHunt.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EasterEggHunt.Infrastructure.Tests.Integration;

/// <summary>
/// Basis-Klasse für Integration Tests mit echter SQLite-Datenbank
/// </summary>
public abstract class IntegrationTestBase : IDisposable
{
    private bool _disposed = false;
    protected ServiceProvider ServiceProvider { get; }
    protected EasterEggHuntDbContext Context { get; }
    protected ICampaignRepository CampaignRepository { get; }
    protected IQrCodeRepository QrCodeRepository { get; }
    protected IUserRepository UserRepository { get; }
    protected IFindRepository FindRepository { get; }
    protected ISessionRepository SessionRepository { get; }
    protected IAdminUserRepository AdminUserRepository { get; }

    /// <summary>
    /// Initialisiert eine neue Instanz der IntegrationTestBase-Klasse
    /// </summary>
    protected IntegrationTestBase()
    {
        // Service Collection für Tests erstellen
        var services = new ServiceCollection();

        // SQLite-Datenbank für Tests konfigurieren (echte Datei für bessere Kompatibilität)
        var testDbPath = Path.GetTempFileName();
        services.AddDbContext<EasterEggHuntDbContext>(options =>
        {
            options.UseSqlite($"Data Source={testDbPath}");
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        });

        // Repository Services registrieren
        services.AddRepositories();

        // Logging für Tests deaktivieren
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));

        // Service Provider erstellen
        ServiceProvider = services.BuildServiceProvider();

        // Services aus DI Container abrufen
        Context = ServiceProvider.GetRequiredService<EasterEggHuntDbContext>();
        CampaignRepository = ServiceProvider.GetRequiredService<ICampaignRepository>();
        QrCodeRepository = ServiceProvider.GetRequiredService<IQrCodeRepository>();
        UserRepository = ServiceProvider.GetRequiredService<IUserRepository>();
        FindRepository = ServiceProvider.GetRequiredService<IFindRepository>();
        SessionRepository = ServiceProvider.GetRequiredService<ISessionRepository>();
        AdminUserRepository = ServiceProvider.GetRequiredService<IAdminUserRepository>();

        // Datenbank synchron erstellen und Migrationen anwenden
        Context.Database.EnsureCreated();
    }

    /// <summary>
    /// Setzt die Datenbank für jeden Test zurück
    /// </summary>
    protected async Task ResetDatabaseAsync()
    {
        // Alle Entitäten aus dem Change Tracker entfernen
        Context.ChangeTracker.Clear();

        // Alle Tabellen leeren
        await Context.Database.ExecuteSqlRawAsync("DELETE FROM Finds");
        await Context.Database.ExecuteSqlRawAsync("DELETE FROM Sessions");
        await Context.Database.ExecuteSqlRawAsync("DELETE FROM Users");
        await Context.Database.ExecuteSqlRawAsync("DELETE FROM QrCodes");
        await Context.Database.ExecuteSqlRawAsync("DELETE FROM Campaigns");
        await Context.Database.ExecuteSqlRawAsync("DELETE FROM AdminUsers");
    }

    /// <summary>
    /// Fügt Test-Daten zur Datenbank hinzu
    /// </summary>
    protected async Task SeedTestDataAsync()
    {
        // Test-Kampagne erstellen
        var campaign = new EasterEggHunt.Domain.Entities.Campaign(
            "Test Kampagne 2025",
            "Eine Test-Kampagne für Integration Tests",
            "TestAdmin");

        await CampaignRepository.AddAsync(campaign);

        // Test-QR-Codes erstellen
        var qrCode1 = new EasterEggHunt.Domain.Entities.QrCode(
            campaign.Id,
            "QR Code 1",
            "Beschreibung für QR Code 1",
            "Interner Hinweis für QR Code 1");

        var qrCode2 = new EasterEggHunt.Domain.Entities.QrCode(
            campaign.Id,
            "QR Code 2",
            "Beschreibung für QR Code 2",
            "Interner Hinweis für QR Code 2");

        await QrCodeRepository.AddAsync(qrCode1);
        await QrCodeRepository.AddAsync(qrCode2);

        // Test-Benutzer erstellen
        var user1 = new EasterEggHunt.Domain.Entities.User("Max Mustermann");
        var user2 = new EasterEggHunt.Domain.Entities.User("Anna Schmidt");

        await UserRepository.AddAsync(user1);
        await UserRepository.AddAsync(user2);

        // Test-Funde erstellen
        var find1 = new EasterEggHunt.Domain.Entities.Find(
            qrCode1.Id,
            user1.Id,
            "192.168.1.100",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

        var find2 = new EasterEggHunt.Domain.Entities.Find(
            qrCode1.Id,
            user2.Id,
            "192.168.1.101",
            "Mozilla/5.0 (iPhone; CPU iPhone OS 15_0 like Mac OS X)");

        await FindRepository.AddAsync(find1);
        await FindRepository.AddAsync(find2);

        // Test-Session erstellen
        var session = new EasterEggHunt.Domain.Entities.Session(user1.Id, 7);
        await SessionRepository.AddAsync(session);

        // Test-Admin-Benutzer erstellen
        var adminUser = new EasterEggHunt.Domain.Entities.AdminUser(
            "admin",
            "hashedpassword",
            "admin@test.com");

        await AdminUserRepository.AddAsync(adminUser);
    }

    /// <summary>
    /// Gibt alle Ressourcen frei
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Gibt alle Ressourcen frei
    /// </summary>
    /// <param name="disposing">True wenn verwaltete Ressourcen freigegeben werden sollen</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            Context?.Dispose();
            ServiceProvider?.Dispose();
            _disposed = true;
        }
    }
}
