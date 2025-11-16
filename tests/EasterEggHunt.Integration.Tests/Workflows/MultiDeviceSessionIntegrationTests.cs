using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Infrastructure.Data;
using EasterEggHunt.Infrastructure.Repositories;
using EasterEggHunt.Integration.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace EasterEggHunt.Integration.Tests.Workflows;

/// <summary>
/// Integration Tests für Multi-Device-Session-Szenarien
/// Testet, ob verschiedene Geräte verschiedene Sessions erhalten und
/// ob Sessions isoliert zwischen Geräten sind
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.Self)]
public sealed class MultiDeviceSessionIntegrationTests : IDisposable
{
    private TestWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;
    private EasterEggHuntDbContext _context = null!;
    private SessionRepository _sessionRepository = null!;

    [SetUp]
    public async Task Setup()
    {
        _factory = new TestWebApplicationFactory();
        await _factory.SeedTestDataAsync();
        _client = _factory.CreateClient();

        // DbContext und Repository für direkte Datenbank-Zugriffe
        var scope = _factory.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<EasterEggHuntDbContext>();
        _sessionRepository = new SessionRepository(_context);
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
    public async Task User_CanHaveMultipleSessions_ForDifferentDevices()
    {
        // Arrange
        var user = new User("Test User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act - Erstelle verschiedene Sessions für denselben User (simuliert verschiedene Geräte)
        var session1 = new Session(user.Id, 30);
        var session2 = new Session(user.Id, 30);
        var session3 = new Session(user.Id, 30);

        await _sessionRepository.AddAsync(session1);
        await _sessionRepository.SaveChangesAsync();
        await _sessionRepository.AddAsync(session2);
        await _sessionRepository.SaveChangesAsync();
        await _sessionRepository.AddAsync(session3);
        await _sessionRepository.SaveChangesAsync();

        // Assert
        var allSessions = await _sessionRepository.GetByUserIdAsync(user.Id);
        Assert.That(allSessions.Count(), Is.EqualTo(3), "User sollte 3 Sessions haben");

        // Alle Sessions sollten verschiedene IDs haben
        var sessionIds = allSessions.Select(s => s.Id).ToList();
        Assert.That(sessionIds, Is.Unique, "Jede Session sollte eine eindeutige ID haben");
    }

    [Test]
    public async Task Sessions_ShouldBeIsolated_BySessionId()
    {
        // Arrange
        var user = new User("Test User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var session1 = new Session(user.Id, 30);
        var session2 = new Session(user.Id, 30);

        await _sessionRepository.AddAsync(session1);
        await _sessionRepository.SaveChangesAsync();
        await _sessionRepository.AddAsync(session2);
        await _context.SaveChangesAsync();

        // Act - Aktualisiere Daten in Session1
        session1.UpdateData("{\"device\": \"mobile\", \"data\": \"session1-data\"}");
        await _sessionRepository.UpdateAsync(session1);
        await _sessionRepository.SaveChangesAsync();

        // Assert - Session2 sollte unverändert sein
        var retrievedSession2 = await _sessionRepository.GetByIdAsync(session2.Id);
        Assert.That(retrievedSession2, Is.Not.Null, "Session2 sollte existieren");
        Assert.That(retrievedSession2!.Data, Is.EqualTo("{}"), "Session2-Daten sollten unverändert sein");
        Assert.That(retrievedSession2.Data, Is.Not.EqualTo(session1.Data),
            "Session2-Daten sollten sich von Session1-Daten unterscheiden");
    }

    [Test]
    public async Task Sessions_ShouldHaveUniqueIds_EvenForSameUser()
    {
        // Arrange
        var user = new User("Test User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act - Erstelle mehrere Sessions
        var sessions = new List<Session>();
        for (int i = 0; i < 5; i++)
        {
            var session = new Session(user.Id, 30);
            sessions.Add(session);
            await _sessionRepository.AddAsync(session);
            await _sessionRepository.SaveChangesAsync();
        }

        // Assert
        var allSessions = await _sessionRepository.GetByUserIdAsync(user.Id);
        var sessionIds = allSessions.Select(s => s.Id).ToList();

        Assert.That(sessionIds.Count, Is.EqualTo(5), "Es sollten 5 Sessions vorhanden sein");
        Assert.That(sessionIds, Is.Unique, "Alle Session-IDs sollten eindeutig sein");

        // Prüfe, dass keine IDs dupliziert sind
        var distinctIds = sessionIds.Distinct().ToList();
        Assert.That(distinctIds.Count, Is.EqualTo(sessionIds.Count),
            "Alle Session-IDs sollten eindeutig sein (keine Duplikate)");
    }

    [Test]
    public async Task Sessions_FromDifferentDevices_ShouldBeIndependent()
    {
        // Arrange
        var user = new User("Test User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Simuliere 3 verschiedene Geräte
        var device1Session = new Session(user.Id, 30);
        var device2Session = new Session(user.Id, 30);
        var device3Session = new Session(user.Id, 30);

        await _sessionRepository.AddAsync(device1Session);
        await _sessionRepository.SaveChangesAsync();
        await _sessionRepository.AddAsync(device2Session);
        await _sessionRepository.SaveChangesAsync();
        await _sessionRepository.AddAsync(device3Session);
        await _sessionRepository.SaveChangesAsync();

        // Act - Verlängere nur Session von Device 1
        device1Session.Extend(7);
        await _sessionRepository.UpdateAsync(device1Session);
        await _sessionRepository.SaveChangesAsync();

        // Assert - Nur Device 1 Session sollte verlängert sein
        var retrievedDevice1 = await _sessionRepository.GetByIdAsync(device1Session.Id);
        var retrievedDevice2 = await _sessionRepository.GetByIdAsync(device2Session.Id);
        var retrievedDevice3 = await _sessionRepository.GetByIdAsync(device3Session.Id);

        Assert.That(retrievedDevice1, Is.Not.Null, "Device1 Session sollte existieren");
        Assert.That(retrievedDevice2, Is.Not.Null, "Device2 Session sollte existieren");
        Assert.That(retrievedDevice3, Is.Not.Null, "Device3 Session sollte existieren");

        // Device1 sollte verlängert sein (37 Tage statt 30)
        var device1Expiration = (retrievedDevice1!.ExpiresAt - retrievedDevice1.CreatedAt).Days;
        var device2Expiration = (retrievedDevice2!.ExpiresAt - retrievedDevice2.CreatedAt).Days;
        var device3Expiration = (retrievedDevice3!.ExpiresAt - retrievedDevice3.CreatedAt).Days;

        Assert.That(device1Expiration, Is.GreaterThan(30),
            "Device1 Session sollte verlängert sein");
        Assert.That(device2Expiration, Is.EqualTo(30),
            "Device2 Session sollte nicht verlängert sein");
        Assert.That(device3Expiration, Is.EqualTo(30),
            "Device3 Session sollte nicht verlängert sein");
    }

    [Test]
    public async Task Sessions_FromDifferentDevices_CanBeDeactivatedIndependently()
    {
        // Arrange
        var user = new User("Test User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var device1Session = new Session(user.Id, 30);
        var device2Session = new Session(user.Id, 30);

        await _sessionRepository.AddAsync(device1Session);
        await _sessionRepository.SaveChangesAsync();
        await _sessionRepository.AddAsync(device2Session);
        await _sessionRepository.SaveChangesAsync();

        // Act - Deaktiviere nur Device1 Session
        device1Session.Deactivate();
        await _sessionRepository.UpdateAsync(device1Session);
        await _sessionRepository.SaveChangesAsync();

        // Assert
        var retrievedDevice1 = await _sessionRepository.GetByIdAsync(device1Session.Id);
        var retrievedDevice2 = await _sessionRepository.GetByIdAsync(device2Session.Id);

        Assert.That(retrievedDevice1, Is.Not.Null, "Device1 Session sollte existieren");
        Assert.That(retrievedDevice1!.IsActive, Is.False,
            "Device1 Session sollte deaktiviert sein");
        Assert.That(retrievedDevice1.IsValid(), Is.False,
            "Device1 Session sollte ungültig sein");

        Assert.That(retrievedDevice2, Is.Not.Null, "Device2 Session sollte existieren");
        Assert.That(retrievedDevice2!.IsActive, Is.True,
            "Device2 Session sollte noch aktiv sein");
        Assert.That(retrievedDevice2.IsValid(), Is.True,
            "Device2 Session sollte noch gültig sein");
    }

    [Test]
    public async Task GetByUserIdAsync_ShouldReturnAllSessionsForUser_FromAllDevices()
    {
        // Arrange
        var user1 = new User("User 1");
        var user2 = new User("User 2");
        await _context.Users.AddRangeAsync(user1, user2);
        await _context.SaveChangesAsync();

        // Erstelle Sessions für User1 (3 Geräte)
        var user1Session1 = new Session(user1.Id, 30);
        var user1Session2 = new Session(user1.Id, 30);
        var user1Session3 = new Session(user1.Id, 30);

        // Erstelle Sessions für User2 (2 Geräte)
        var user2Session1 = new Session(user2.Id, 30);
        var user2Session2 = new Session(user2.Id, 30);

        await _sessionRepository.AddAsync(user1Session1);
        await _sessionRepository.AddAsync(user1Session2);
        await _sessionRepository.AddAsync(user1Session3);
        await _sessionRepository.AddAsync(user2Session1);
        await _sessionRepository.AddAsync(user2Session2);
        await _sessionRepository.SaveChangesAsync();

        // Act
        var user1Sessions = await _sessionRepository.GetByUserIdAsync(user1.Id);
        var user2Sessions = await _sessionRepository.GetByUserIdAsync(user2.Id);

        // Assert
        Assert.That(user1Sessions.Count(), Is.EqualTo(3),
            "User1 sollte 3 Sessions haben (3 Geräte)");
        Assert.That(user2Sessions.Count(), Is.EqualTo(2),
            "User2 sollte 2 Sessions haben (2 Geräte)");

        // Alle Sessions sollten zum richtigen User gehören
        Assert.That(user1Sessions.All(s => s.UserId == user1.Id), Is.True,
            "Alle User1-Sessions sollten zu User1 gehören");
        Assert.That(user2Sessions.All(s => s.UserId == user2.Id), Is.True,
            "Alle User2-Sessions sollten zu User2 gehören");
    }
}
