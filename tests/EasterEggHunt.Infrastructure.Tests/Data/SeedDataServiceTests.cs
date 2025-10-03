using EasterEggHunt.Infrastructure.Data;
using FluentAssertions;
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

        adminUsers.Should().HaveCount(1);
        adminUsers[0].Username.Should().Be("admin");
        adminUsers[0].Email.Should().Be("admin@easteregghunt.local");

        campaigns.Should().HaveCount(3);
        campaigns.Should().Contain(c => c.Name == "Ostern 2025");
        campaigns.Should().Contain(c => c.Name == "Team Building");
        campaigns.Should().Contain(c => c.Name == "Weihnachts-Special");

        qrCodes.Should().HaveCount(15); // 10 für Ostern + 5 für Team Building
        qrCodes.Should().Contain(q => q.Title == "Küche");
        qrCodes.Should().Contain(q => q.Title == "Eingang");

        users.Should().HaveCount(5);
        users.Should().Contain(u => u.Name == "Max Mustermann");
        users.Should().Contain(u => u.Name == "Anna Schmidt");
        users.Should().Contain(u => u.Name == "Tom Weber");

        finds.Should().NotBeEmpty();
        finds.Should().HaveCountGreaterThan(0);

        sessions.Should().HaveCount(3);
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
        campaigns.Should().HaveCount(1);
        campaigns[0].Name.Should().Be("Existing");

        var adminUsers = await _context.AdminUsers.ToListAsync();
        adminUsers.Should().BeEmpty();
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

        easterCampaign.QrCodes.Should().HaveCount(10);
        easterCampaign.QrCodes.Should().AllSatisfy(qr => 
            qr.CampaignId.Should().Be(easterCampaign.Id));

        var qrCodeWithFinds = await _context.QrCodes
            .Include(q => q.Finds)
            .ThenInclude(f => f.User)
            .FirstAsync(q => q.Finds.Any());

        qrCodeWithFinds.Finds.Should().NotBeEmpty();
        qrCodeWithFinds.Finds.Should().AllSatisfy(find =>
        {
            find.QrCodeId.Should().Be(qrCodeWithFinds.Id);
            find.User.Should().NotBeNull();
        });
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

        sessions.Should().HaveCount(3);
        sessions.Should().AllSatisfy(session =>
        {
            session.User.Should().NotBeNull();
            session.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
            session.CreatedAt.Should().BeBefore(DateTime.UtcNow);
        });
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
        service.Should().NotBeNull();
    }
}
