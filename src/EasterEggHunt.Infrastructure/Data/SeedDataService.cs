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
        // 1. Admin-Benutzer erstellen
        var adminUser = new AdminUser(
            username: "admin",
            passwordHash: BCrypt.Net.BCrypt.HashPassword("admin123"),
            email: "admin@easteregghunt.local"
        );

        context.AdminUsers.Add(adminUser);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Admin-Benutzer erstellt: admin/admin123");

        // 2. Test-Kampagnen erstellen
        var campaigns = new[]
        {
            new Campaign("Ostern 2025", "Jährliche Ostereier-Suche im Büro", "admin"),
            new Campaign("Team Building", "QR-Code Rallye für neue Mitarbeiter", "admin"),
            new Campaign("Weihnachts-Special", "Weihnachtliche Schatzsuche", "admin")
        };

        context.Campaigns.AddRange(campaigns);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("3 Test-Kampagnen erstellt");

        // 3. QR-Codes für die erste Kampagne erstellen
        var easterCampaign = campaigns[0];
        var qrCodes = new[]
        {
            new QrCode(easterCampaign.Id, "Küche", "Hinter dem Kaffeeautomaten"),
            new QrCode(easterCampaign.Id, "Meeting Room", "Unter dem Konferenztisch"),
            new QrCode(easterCampaign.Id, "Rezeption", "In der Pflanze links vom Eingang"),
            new QrCode(easterCampaign.Id, "Büro 101", "Hinter dem Monitor von Max"),
            new QrCode(easterCampaign.Id, "Parkplatz", "Unter dem Auto von der Chefin"),
            new QrCode(easterCampaign.Id, "Kantine", "In der Salatschüssel"),
            new QrCode(easterCampaign.Id, "Toilette", "Hinter dem Spiegel"),
            new QrCode(easterCampaign.Id, "Kopierraum", "Im Papierkorb"),
            new QrCode(easterCampaign.Id, "Balkon", "Unter dem Blumentopf"),
            new QrCode(easterCampaign.Id, "Serverraum", "Neben dem Router")
        };

        context.QrCodes.AddRange(qrCodes);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("10 QR-Codes für Ostern 2025 erstellt");

        // 4. QR-Codes für Team Building Kampagne
        var teamBuildingCampaign = campaigns[1];
        var teamBuildingQrCodes = new[]
        {
            new QrCode(teamBuildingCampaign.Id, "Eingang", "Willkommen im Unternehmen!"),
            new QrCode(teamBuildingCampaign.Id, "HR-Abteilung", "Hier arbeitet das Personalteam"),
            new QrCode(teamBuildingCampaign.Id, "IT-Abteilung", "Die Technik-Experten"),
            new QrCode(teamBuildingCampaign.Id, "Marketing", "Kreative Köpfe"),
            new QrCode(teamBuildingCampaign.Id, "Vertrieb", "Die Verkaufsprofis")
        };

        context.QrCodes.AddRange(teamBuildingQrCodes);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("5 QR-Codes für Team Building erstellt");

        // 5. Test-Benutzer erstellen
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

        // 6. Einige Funde simulieren (für Demo-Zwecke)
        var random = new Random();
        var finds = new List<Find>();

        // Max hat einige QR-Codes gefunden
        var maxUser = testUsers[0];
        var maxFinds = qrCodes.Take(3).Select(qrCode => 
            new Find(qrCode.Id, maxUser.Id, "192.168.1.100", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"));
        finds.AddRange(maxFinds);

        // Anna hat auch einige gefunden
        var annaUser = testUsers[1];
        var annaFinds = qrCodes.Skip(2).Take(4).Select(qrCode => 
            new Find(qrCode.Id, annaUser.Id, "192.168.1.101", "Mozilla/5.0 (iPhone; CPU iPhone OS 15_0 like Mac OS X)"));
        finds.AddRange(annaFinds);

        // Tom hat alle gefunden
        var tomUser = testUsers[2];
        var tomFinds = qrCodes.Select(qrCode => 
            new Find(qrCode.Id, tomUser.Id, "192.168.1.102", "Mozilla/5.0 (Android 12; Mobile; rv:68.0) Gecko/68.0 Firefox/88.0"));
        finds.AddRange(tomFinds);

        context.Finds.AddRange(finds);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{FindCount} Test-Funde erstellt", finds.Count);

        // 7. Aktive Sessions für einige Benutzer
        var sessions = new[]
        {
            new Session(maxUser.Id, 30),
            new Session(annaUser.Id, 30),
            new Session(tomUser.Id, 30)
        };

        context.Sessions.AddRange(sessions);
        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("3 aktive Sessions erstellt");

        _logger.LogInformation("Entwicklungsdaten erfolgreich erstellt:");
        _logger.LogInformation("- 1 Admin-Benutzer (admin/admin123)");
        _logger.LogInformation("- 3 Kampagnen");
        _logger.LogInformation("- 15 QR-Codes");
        _logger.LogInformation("- 5 Test-Benutzer");
        _logger.LogInformation("- {FindCount} Funde", finds.Count);
        _logger.LogInformation("- 3 aktive Sessions");
    }
}
