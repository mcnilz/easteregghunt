using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Infrastructure.Data;
using EasterEggHunt.Integration.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace EasterEggHunt.Integration.Tests.Workflows;

/// <summary>
/// Integration Tests für Session-Timeout-Verhalten
/// Testet, ob Sessions korrekt ablaufen und validiert werden
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.Self)]
public sealed class SessionTimeoutIntegrationTests : IDisposable
{
    private TestWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;
    private EasterEggHuntDbContext _context = null!;

    [SetUp]
    public async Task Setup()
    {
        _factory = new TestWebApplicationFactory();
        await _factory.SeedTestDataAsync();
        _client = _factory.CreateClient();

        // DbContext für direkte Datenbank-Zugriffe
        var scope = _factory.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<EasterEggHuntDbContext>();
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
        _context?.Dispose();
        _factory?.Dispose();
    }

    public void Dispose()
    {
        TearDown();
        GC.SuppressFinalize(this);
    }

    [Test]
    public async Task Session_WhenExpired_ShouldBeInvalid()
    {
        // Arrange
        var user = new User("Test User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Erstelle eine bereits abgelaufene Session
        var expiredSession = new Session(user.Id, 30)
        {
            CreatedAt = DateTime.UtcNow.AddDays(-31), // Vor 31 Tagen erstellt
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // Vor 1 Tag abgelaufen
            IsActive = true
        };

        await _context.Sessions.AddAsync(expiredSession);
        await _context.SaveChangesAsync();

        // Act
        var isValid = expiredSession.IsValid();

        // Assert
        Assert.That(isValid, Is.False, "Abgelaufene Session sollte ungültig sein");
        Assert.That(expiredSession.ExpiresAt, Is.LessThan(DateTime.UtcNow),
            "ExpiresAt sollte in der Vergangenheit sein");
    }

    [Test]
    public async Task Session_WhenValid_ShouldBeValid()
    {
        // Arrange
        var user = new User("Test User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Erstelle eine gültige Session (läuft in 30 Tagen ab)
        var validSession = new Session(user.Id, 30);

        await _context.Sessions.AddAsync(validSession);
        await _context.SaveChangesAsync();

        // Act
        var isValid = validSession.IsValid();

        // Assert
        Assert.That(isValid, Is.True, "Gültige Session sollte valid sein");
        Assert.That(validSession.ExpiresAt, Is.GreaterThan(DateTime.UtcNow),
            "ExpiresAt sollte in der Zukunft sein");
        Assert.That(validSession.IsActive, Is.True, "Session sollte aktiv sein");
    }

    [Test]
    public async Task Session_Extend_ShouldIncreaseExpiration()
    {
        // Arrange
        var user = new User("Test User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var session = new Session(user.Id, 30);
        var originalExpiration = session.ExpiresAt;

        await _context.Sessions.AddAsync(session);
        await _context.SaveChangesAsync();

        // Act
        session.Extend(7); // Verlängere um 7 Tage
        await _context.SaveChangesAsync();

        // Assert
        var newExpiration = session.ExpiresAt;
        var extensionDays = (newExpiration - originalExpiration).Days;

        Assert.That(newExpiration, Is.GreaterThan(originalExpiration),
            "Expiration sollte erhöht worden sein");
        Assert.That(extensionDays, Is.EqualTo(7),
            "Session sollte um 7 Tage verlängert worden sein");
    }

    [Test]
    public async Task Session_WithCustomExpiration_ShouldExpireAfterSpecifiedDays()
    {
        // Arrange
        var user = new User("Test User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var expirationDays = 7;
        var session = new Session(user.Id, expirationDays);

        await _context.Sessions.AddAsync(session);
        await _context.SaveChangesAsync();

        // Act
        var expirationTime = session.ExpiresAt - session.CreatedAt;

        // Assert
        Assert.That(expirationTime.Days, Is.EqualTo(expirationDays),
            $"Session sollte nach {expirationDays} Tagen ablaufen");
    }

    [Test]
    public async Task Session_Deactivate_ShouldBecomeInvalid()
    {
        // Arrange
        var user = new User("Test User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var session = new Session(user.Id, 30);
        await _context.Sessions.AddAsync(session);
        await _context.SaveChangesAsync();

        // Act
        session.Deactivate();
        await _context.SaveChangesAsync();

        // Assert
        Assert.That(session.IsActive, Is.False, "Session sollte inaktiv sein");
        Assert.That(session.IsValid(), Is.False,
            "Deaktivierte Session sollte ungültig sein, auch wenn noch nicht abgelaufen");
    }
}
