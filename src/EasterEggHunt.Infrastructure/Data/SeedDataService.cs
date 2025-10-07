using EasterEggHunt.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EasterEggHunt.Infrastructure.Data;

/// <summary>
/// Service für das Seeding von Entwicklungsdaten
/// </summary>
public class SeedDataService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SeedDataService> _logger;

    /// <summary>
    /// Konstruktor für Dependency Injection
    /// </summary>
    /// <param name="serviceProvider">Service Provider für DbContext</param>
    /// <param name="logger">Logger für Seed-Operationen</param>
    public SeedDataService(IServiceProvider serviceProvider, ILogger<SeedDataService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Startet den Seed-Service
    /// </summary>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>Task</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EasterEggHuntDbContext>();

        try
        {
            // Prüfen ob bereits Daten vorhanden sind
            var hasData = await context.Campaigns.AnyAsync(cancellationToken);
            if (hasData)
            {
                _logger.LogInformation("Datenbank enthält bereits Daten. Seed wird übersprungen.");
                return;
            }

            _logger.LogInformation("Starte Seed-Prozess für Entwicklungsdaten...");

            // Seed-Daten erstellen
            await SeedDevelopmentDataAsync(context, cancellationToken);

            _logger.LogInformation("Seed-Prozess erfolgreich abgeschlossen.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Seeding der Entwicklungsdaten");
            throw;
        }
    }

    /// <summary>
    /// Stoppt den Seed-Service
    /// </summary>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>Task</returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Erstellt Entwicklungsdaten für die Datenbank
    /// </summary>
    /// <param name="context">DbContext</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    private async Task SeedDevelopmentDataAsync(EasterEggHuntDbContext context, CancellationToken cancellationToken)
    {
        await CreateAdminUserAsync(context, cancellationToken);
        var campaigns = await CreateTestCampaignsAsync(context, cancellationToken);
        await CreateQrCodesAsync(context, campaigns, cancellationToken);
        var testUsers = await CreateTestUsersAsync(context, cancellationToken);
        await CreateTestFindsAsync(context, testUsers, cancellationToken);
        await CreateTestSessionsAsync(context, testUsers, cancellationToken);

        LogSeedSummary();
    }

    /// <summary>
    /// Erstellt den Admin-Benutzer
    /// </summary>
    private async Task CreateAdminUserAsync(EasterEggHuntDbContext context, CancellationToken cancellationToken)
    {
        var adminUser = new AdminUser(
            username: "admin",
            passwordHash: BCrypt.Net.BCrypt.HashPassword("admin123"),
            email: "admin@easteregghunt.local"
        );

        context.AdminUsers.Add(adminUser);
        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Admin-Benutzer erstellt: admin/admin123");
    }

    /// <summary>
    /// Erstellt Test-Kampagnen
    /// </summary>
    private async Task<Campaign[]> CreateTestCampaignsAsync(EasterEggHuntDbContext context, CancellationToken cancellationToken)
    {
        var campaigns = new[]
        {
            new Campaign("Ostern 2025", "Jährliche Ostereier-Suche im Büro", "admin"),
            new Campaign("Team Building", "QR-Code Rallye für neue Mitarbeiter", "admin"),
            new Campaign("Weihnachts-Special", "Weihnachtliche Schatzsuche", "admin")
        };

        context.Campaigns.AddRange(campaigns);
        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("3 Test-Kampagnen erstellt");

        return campaigns;
    }

    /// <summary>
    /// Erstellt QR-Codes für die Kampagnen
    /// </summary>
    private async Task CreateQrCodesAsync(EasterEggHuntDbContext context, Campaign[] campaigns, CancellationToken cancellationToken)
    {
        var easterQrCodes = CreateEasterQrCodes(campaigns[0]);
        var teamBuildingQrCodes = CreateTeamBuildingQrCodes(campaigns[1]);

        context.QrCodes.AddRange(easterQrCodes);
        context.QrCodes.AddRange(teamBuildingQrCodes);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("15 QR-Codes erstellt");
    }

    /// <summary>
    /// Erstellt QR-Codes für die Ostern-Kampagne
    /// </summary>
    private static QrCode[] CreateEasterQrCodes(Campaign easterCampaign)
    {
        return new[]
        {
            new QrCode(easterCampaign.Id, "Küche", "Versteck in der Küche", "Hinter dem Kaffeeautomaten"),
            new QrCode(easterCampaign.Id, "Meeting Room", "Versteck im Meeting Room", "Unter dem Konferenztisch"),
            new QrCode(easterCampaign.Id, "Rezeption", "Versteck an der Rezeption", "In der Pflanze links vom Eingang"),
            new QrCode(easterCampaign.Id, "Büro 101", "Versteck in Büro 101", "Hinter dem Monitor von Max"),
            new QrCode(easterCampaign.Id, "Parkplatz", "Versteck auf dem Parkplatz", "Unter dem Auto von der Chefin"),
            new QrCode(easterCampaign.Id, "Kantine", "Versteck in der Kantine", "In der Salatschüssel"),
            new QrCode(easterCampaign.Id, "Toilette", "Versteck in der Toilette", "Hinter dem Spiegel"),
            new QrCode(easterCampaign.Id, "Kopierraum", "Versteck im Kopierraum", "Im Papierkorb"),
            new QrCode(easterCampaign.Id, "Balkon", "Versteck auf dem Balkon", "Unter dem Blumentopf"),
            new QrCode(easterCampaign.Id, "Serverraum", "Versteck im Serverraum", "Neben dem Router")
        };
    }

    /// <summary>
    /// Erstellt QR-Codes für die Team Building-Kampagne
    /// </summary>
    private static QrCode[] CreateTeamBuildingQrCodes(Campaign teamBuildingCampaign)
    {
        return new[]
        {
            new QrCode(teamBuildingCampaign.Id, "Eingang", "Willkommen im Unternehmen!", "Eingangsbereich"),
            new QrCode(teamBuildingCampaign.Id, "HR-Abteilung", "Hier arbeitet das Personalteam", "HR-Büro"),
            new QrCode(teamBuildingCampaign.Id, "IT-Abteilung", "Die Technik-Experten", "IT-Büro"),
            new QrCode(teamBuildingCampaign.Id, "Marketing", "Kreative Köpfe", "Marketing-Büro"),
            new QrCode(teamBuildingCampaign.Id, "Vertrieb", "Die Verkaufsprofis", "Vertriebs-Büro")
        };
    }

    /// <summary>
    /// Erstellt Test-Benutzer
    /// </summary>
    private async Task<User[]> CreateTestUsersAsync(EasterEggHuntDbContext context, CancellationToken cancellationToken)
    {
        var testUsers = new[]
        {
            new User("Max Mustermann"),
            new User("Anna Schmidt"),
            new User("Tom Weber"),
            new User("Lisa Müller"),
            new User("Peter Klein")
        };

        context.Users.AddRange(testUsers);
        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("5 Test-Benutzer erstellt");

        return testUsers;
    }

    /// <summary>
    /// Erstellt Test-Funde für Demo-Zwecke
    /// </summary>
    private async Task CreateTestFindsAsync(EasterEggHuntDbContext context, User[] testUsers, CancellationToken cancellationToken)
    {
        var qrCodes = await context.QrCodes.ToListAsync(cancellationToken);
        var finds = new List<Find>();

        // Max hat einige QR-Codes gefunden
        var maxFinds = qrCodes.Take(3).Select(qrCode =>
            new Find(qrCode.Id, testUsers[0].Id, "192.168.1.100", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"));
        finds.AddRange(maxFinds);

        // Anna hat auch einige gefunden
        var annaFinds = qrCodes.Skip(2).Take(4).Select(qrCode =>
            new Find(qrCode.Id, testUsers[1].Id, "192.168.1.101", "Mozilla/5.0 (iPhone; CPU iPhone OS 15_0 like Mac OS X)"));
        finds.AddRange(annaFinds);

        // Tom hat alle gefunden
        var tomFinds = qrCodes.Select(qrCode =>
            new Find(qrCode.Id, testUsers[2].Id, "192.168.1.102", "Mozilla/5.0 (Android 12; Mobile; rv:68.0) Gecko/68.0 Firefox/88.0"));
        finds.AddRange(tomFinds);

        context.Finds.AddRange(finds);
        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("{FindCount} Test-Funde erstellt", finds.Count);
    }

    /// <summary>
    /// Erstellt Test-Sessions
    /// </summary>
    private async Task CreateTestSessionsAsync(EasterEggHuntDbContext context, User[] testUsers, CancellationToken cancellationToken)
    {
        var sessions = new[]
        {
            new Session(testUsers[0].Id, 30),
            new Session(testUsers[1].Id, 30),
            new Session(testUsers[2].Id, 30)
        };

        context.Sessions.AddRange(sessions);
        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("3 aktive Sessions erstellt");
    }

    /// <summary>
    /// Loggt eine Zusammenfassung der erstellten Seed-Daten
    /// </summary>
    private void LogSeedSummary()
    {
        _logger.LogInformation("Entwicklungsdaten erfolgreich erstellt:");
        _logger.LogInformation("- 1 Admin-Benutzer (admin/admin123)");
        _logger.LogInformation("- 3 Kampagnen");
        _logger.LogInformation("- 15 QR-Codes");
        _logger.LogInformation("- 5 Test-Benutzer");
        _logger.LogInformation("- 17 Funde");
        _logger.LogInformation("- 3 aktive Sessions");
    }
}
