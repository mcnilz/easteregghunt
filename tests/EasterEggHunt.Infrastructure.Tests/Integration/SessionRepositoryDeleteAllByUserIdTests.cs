using EasterEggHunt.Domain.Entities;

namespace EasterEggHunt.Infrastructure.Tests.Integration;

/// <summary>
/// Integration Tests für SessionRepository.DeleteAllByUserIdAsync (GDPR)
/// </summary>
[TestFixture]
[Category("Integration")]
public class SessionRepositoryDeleteAllByUserIdTests : IntegrationTestBase
{
    private User _testUser = null!;
    private User _otherUser = null!;

    [SetUp]
    public async Task SetUp()
    {
        await ResetDatabaseAsync();

        // Test-Benutzer erstellen
        _testUser = new User("Test User");
        _otherUser = new User("Other User");
        await UserRepository.AddAsync(_testUser);
        await UserRepository.AddAsync(_otherUser);
        await UserRepository.SaveChangesAsync();
    }

    [Test]
    public async Task DeleteAllByUserIdAsync_ShouldDeleteAllSessionsForUser()
    {
        // Arrange
        var session1 = new Session(_testUser.Id, 30);
        var session2 = new Session(_testUser.Id, 30);
        var session3 = new Session(_testUser.Id, 30);
        var otherUserSession = new Session(_otherUser.Id, 30);

        await SessionRepository.AddAsync(session1);
        await SessionRepository.AddAsync(session2);
        await SessionRepository.AddAsync(session3);
        await SessionRepository.AddAsync(otherUserSession);
        await SessionRepository.SaveChangesAsync();

        // Act
        var deletedCount = await SessionRepository.DeleteAllByUserIdAsync(_testUser.Id);

        // Assert
        Assert.That(deletedCount, Is.EqualTo(3), "Alle 3 Sessions des Test-Users sollten gelöscht sein");

        // Prüfe, dass Test-User-Sessions gelöscht wurden
        var remainingTestUserSessions = await SessionRepository.GetByUserIdAsync(_testUser.Id);
        Assert.That(remainingTestUserSessions, Is.Empty, "Keine Sessions des Test-Users sollten vorhanden sein");

        // Prüfe, dass Other-User-Session noch existiert
        var otherUserSessions = await SessionRepository.GetByUserIdAsync(_otherUser.Id);
        Assert.That(otherUserSessions.Count(), Is.EqualTo(1), "Other-User-Session sollte noch existieren");
    }

    [Test]
    public async Task DeleteAllByUserIdAsync_ShouldDeleteActiveAndInactiveSessions()
    {
        // Arrange
        var activeSession = new Session(_testUser.Id, 30) { IsActive = true };
        var inactiveSession = new Session(_testUser.Id, 30) { IsActive = false };

        await SessionRepository.AddAsync(activeSession);
        await SessionRepository.AddAsync(inactiveSession);
        await SessionRepository.SaveChangesAsync();

        // Act
        var deletedCount = await SessionRepository.DeleteAllByUserIdAsync(_testUser.Id);

        // Assert
        Assert.That(deletedCount, Is.EqualTo(2), "Beide Sessions sollten gelöscht sein (aktiv und inaktiv)");

        var remainingSessions = await SessionRepository.GetByUserIdAsync(_testUser.Id);
        Assert.That(remainingSessions, Is.Empty, "Keine Sessions sollten vorhanden sein");
    }

    [Test]
    public async Task DeleteAllByUserIdAsync_WithNoSessions_ShouldReturnZero()
    {
        // Act
        var deletedCount = await SessionRepository.DeleteAllByUserIdAsync(_testUser.Id);

        // Assert
        Assert.That(deletedCount, Is.EqualTo(0), "Keine Sessions sollten gelöscht werden wenn keine vorhanden sind");
    }
}


