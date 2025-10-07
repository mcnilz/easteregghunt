using EasterEggHunt.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;

namespace EasterEggHunt.Infrastructure.Tests.Data;

/// <summary>
/// Tests für den SeedDataService
/// </summary>
[TestFixture]
[Category("Integration")]
public class SeedDataServiceTests
{
    private ServiceProvider _serviceProvider = null!;
    private EasterEggHuntDbContext _context = null!;
    private SeedDataService _seedService = null!;

    [SetUp]
    public void SetUp()
    {
        // File-based SQLite für Tests (bessere Kompatibilität)
        var testDbPath = Path.GetTempFileName();
        var services = new ServiceCollection();

        services.AddDbContext<EasterEggHuntDbContext>(options =>
        {
            options.UseSqlite($"Data Source={testDbPath}");
            options.EnableSensitiveDataLogging();
        });

        // Mock Logger
        var mockLogger = new Mock<ILogger<SeedDataService>>();
        services.AddSingleton(mockLogger.Object);

        // Mock Host Environment (Development)
        var mockEnvironment = new Mock<IHostEnvironment>();
        mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");
        services.AddSingleton(mockEnvironment.Object);

        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<EasterEggHuntDbContext>();
        _context.Database.EnsureCreated();

        _seedService = new SeedDataService(_serviceProvider, mockLogger.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context?.Dispose();
        _serviceProvider?.Dispose();
    }

    [Test]
    public async Task StartAsync_WithEmptyDatabase_ShouldSeedData()
    {
        // Act
        await _seedService.StartAsync(CancellationToken.None);

        // Assert
        var adminUsers = await _context.AdminUsers.ToListAsync();
        var campaigns = await _context.Campaigns.ToListAsync();
        var qrCodes = await _context.QrCodes.ToListAsync();
        var users = await _context.Users.ToListAsync();
        var finds = await _context.Finds.ToListAsync();
        var sessions = await _context.Sessions.ToListAsync();

        Assert.That(adminUsers, Has.Count.EqualTo(1));
        Assert.That(adminUsers[0].Username, Is.EqualTo("admin"));
        Assert.That(adminUsers[0].Email, Is.EqualTo("admin@easteregghunt.local"));

        Assert.That(campaigns, Has.Count.EqualTo(3));
        Assert.That(campaigns, Has.Some.Matches<Domain.Entities.Campaign>(c => c.Name == "Ostern 2025"));
        Assert.That(campaigns, Has.Some.Matches<Domain.Entities.Campaign>(c => c.Name == "Team Building"));
        Assert.That(campaigns, Has.Some.Matches<Domain.Entities.Campaign>(c => c.Name == "Weihnachts-Special"));

        Assert.That(qrCodes, Has.Count.EqualTo(15)); // 10 für Ostern + 5 für Team Building
        Assert.That(qrCodes, Has.Some.Matches<Domain.Entities.QrCode>(q => q.Title == "Küche"));
        Assert.That(qrCodes, Has.Some.Matches<Domain.Entities.QrCode>(q => q.Title == "Eingang"));

        Assert.That(users, Has.Count.EqualTo(5));
        Assert.That(users, Has.Some.Matches<Domain.Entities.User>(u => u.Name == "Max Mustermann"));
        Assert.That(users, Has.Some.Matches<Domain.Entities.User>(u => u.Name == "Anna Schmidt"));
        Assert.That(users, Has.Some.Matches<Domain.Entities.User>(u => u.Name == "Tom Weber"));

        Assert.That(finds, Is.Not.Empty);
        Assert.That(finds, Has.Count.GreaterThan(0));

        Assert.That(sessions, Has.Count.EqualTo(3));
    }

    [Test]
    public async Task StartAsync_WithExistingData_ShouldSkipSeeding()
    {
        // Arrange - Füge bereits Daten hinzu
        var existingCampaign = new EasterEggHunt.Domain.Entities.Campaign("Existing", "Test", "admin");
        _context.Campaigns.Add(existingCampaign);
        await _context.SaveChangesAsync();

        // Act
        await _seedService.StartAsync(CancellationToken.None);

        // Assert - Nur die ursprünglichen Daten sollten vorhanden sein
        var campaigns = await _context.Campaigns.ToListAsync();
        Assert.That(campaigns, Has.Count.EqualTo(1));
        Assert.That(campaigns[0].Name, Is.EqualTo("Existing"));

        var adminUsers = await _context.AdminUsers.ToListAsync();
        Assert.That(adminUsers, Is.Empty);
    }

    [Test]
    public async Task StartAsync_ShouldCreateValidRelationships()
    {
        // Act
        await _seedService.StartAsync(CancellationToken.None);

        // Assert - Prüfe Beziehungen
        var easterCampaign = await _context.Campaigns
            .Include(c => c.QrCodes)
            .FirstAsync(c => c.Name == "Ostern 2025");

        Assert.That(easterCampaign.QrCodes, Has.Count.EqualTo(10));
        Assert.That(easterCampaign.QrCodes, Has.All.Matches<Domain.Entities.QrCode>(qr =>
            qr.CampaignId == easterCampaign.Id));

        var qrCodeWithFinds = await _context.QrCodes
            .Include(q => q.Finds)
            .ThenInclude(f => f.User)
            .FirstAsync(q => q.Finds.Any());

        Assert.That(qrCodeWithFinds.Finds, Is.Not.Empty);
        Assert.That(qrCodeWithFinds.Finds, Has.All.Matches<Domain.Entities.Find>(find =>
            find.QrCodeId == qrCodeWithFinds.Id && find.User != null));
    }

    [Test]
    public async Task StartAsync_ShouldCreateValidSessions()
    {
        // Act
        await _seedService.StartAsync(CancellationToken.None);

        // Assert
        var sessions = await _context.Sessions
            .Include(s => s.User)
            .ToListAsync();

        Assert.That(sessions, Has.Count.EqualTo(3));
        Assert.That(sessions, Has.All.Matches<Domain.Entities.Session>(session =>
            session.User != null &&
            session.ExpiresAt > DateTime.UtcNow &&
            session.CreatedAt < DateTime.UtcNow));
    }

    [Test]
    public async Task StopAsync_ShouldCompleteSuccessfully()
    {
        // Act & Assert - Sollte keine Exception werfen
        await _seedService.StopAsync(CancellationToken.None);
    }

    [Test]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<SeedDataService>>();

        // Act
        var service = new SeedDataService(_serviceProvider, mockLogger.Object);

        // Assert
        Assert.That(service, Is.Not.Null);
    }
}
