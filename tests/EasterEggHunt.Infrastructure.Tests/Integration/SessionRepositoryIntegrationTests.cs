using EasterEggHunt.Domain.Entities;

namespace EasterEggHunt.Infrastructure.Tests.Integration;

[TestFixture]
[Category("Integration")]
public class SessionRepositoryIntegrationTests : IntegrationTestBase
{
    private User _testUser = null!;

    [SetUp]
    public async Task SetUp()
    {
        await ResetDatabaseAsync();

        // Test-Benutzer erstellen
        _testUser = new User("Test User");
        await UserRepository.AddAsync(_testUser);
        await UserRepository.SaveChangesAsync();
    }

    [Test]
    public async Task AddAsync_WithValidSession_ShouldAddSession()
    {
        // Arrange
        var session = new Session(_testUser.Id, 30);

        // Act
        await SessionRepository.AddAsync(session);
        await SessionRepository.SaveChangesAsync();

        // Assert
        var retrievedSession = await SessionRepository.GetByIdAsync(session.Id);
        Assert.That(retrievedSession, Is.Not.Null);
        Assert.That(retrievedSession!.UserId, Is.EqualTo(_testUser.Id));
        Assert.That(retrievedSession.IsActive, Is.True);
        Assert.That(retrievedSession.Data, Is.EqualTo("{}"));
    }

    [Test]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnSession()
    {
        // Arrange
        var session = new Session(_testUser.Id, 30);
        await SessionRepository.AddAsync(session);
        await SessionRepository.SaveChangesAsync();

        // Act
        var retrievedSession = await SessionRepository.GetByIdAsync(session.Id);

        // Assert
        Assert.That(retrievedSession, Is.Not.Null);
        Assert.That(retrievedSession!.Id, Is.EqualTo(session.Id));
    }

    [Test]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Act
        var retrievedSession = await SessionRepository.GetByIdAsync("non-existing-id");

        // Assert
        Assert.That(retrievedSession, Is.Null);
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnAllSessions()
    {
        // Arrange
        var session1 = new Session(_testUser.Id, 30);
        var session2 = new Session(_testUser.Id, 60);
        await SessionRepository.AddAsync(session1);
        await SessionRepository.AddAsync(session2);
        await SessionRepository.SaveChangesAsync();

        // Act
        var sessions = await SessionRepository.GetAllAsync();

        // Assert
        Assert.That(sessions, Is.Not.Null);
        Assert.That(sessions.Count(), Is.EqualTo(2));
        Assert.That(sessions, Has.Some.Matches<Session>(s => s.Id == session1.Id));
        Assert.That(sessions, Has.Some.Matches<Session>(s => s.Id == session2.Id));
    }

    [Test]
    public async Task GetByUserIdAsync_ShouldReturnSessionsForUser()
    {
        // Arrange
        var user2 = new User("User 2");
        await UserRepository.AddAsync(user2);
        await UserRepository.SaveChangesAsync();

        var session1 = new Session(_testUser.Id, 30);
        var session2 = new Session(user2.Id, 30);
        await SessionRepository.AddAsync(session1);
        await SessionRepository.AddAsync(session2);
        await SessionRepository.SaveChangesAsync();

        // Act
        var sessions = await SessionRepository.GetByUserIdAsync(_testUser.Id);

        // Assert
        Assert.That(sessions, Is.Not.Null);
        Assert.That(sessions.Count(), Is.EqualTo(1));
        Assert.That(sessions, Has.Some.Matches<Session>(s => s.Id == session1.Id));
        Assert.That(sessions, Has.None.Matches<Session>(s => s.Id == session2.Id));
    }

    [Test]
    public async Task UpdateAsync_WithValidSession_ShouldUpdateSession()
    {
        // Arrange
        var session = new Session(_testUser.Id, 30);
        await SessionRepository.AddAsync(session);
        await SessionRepository.SaveChangesAsync();

        session.UpdateData("{\"key\": \"value\"}");

        // Act
        await SessionRepository.UpdateAsync(session);
        await SessionRepository.SaveChangesAsync();

        // Assert
        var updatedSession = await SessionRepository.GetByIdAsync(session.Id);
        Assert.That(updatedSession, Is.Not.Null);
        Assert.That(updatedSession!.Data, Is.EqualTo("{\"key\": \"value\"}"));
    }

    [Test]
    public async Task DeleteAsync_WithExistingId_ShouldDeleteSession()
    {
        // Arrange
        var session = new Session(_testUser.Id, 30);
        await SessionRepository.AddAsync(session);
        await SessionRepository.SaveChangesAsync();

        // Act
        var result = await SessionRepository.DeleteAsync(session.Id);
        await SessionRepository.SaveChangesAsync();

        // Assert
        Assert.That(result, Is.True);
        var deletedSession = await SessionRepository.GetByIdAsync(session.Id);
        Assert.That(deletedSession, Is.Null);
    }

    [Test]
    public async Task DeleteAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Act
        var result = await SessionRepository.DeleteAsync("non-existing-id");
        await SessionRepository.SaveChangesAsync();

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
    {
        // Arrange
        var session = new Session(_testUser.Id, 30);
        await SessionRepository.AddAsync(session);
        await SessionRepository.SaveChangesAsync();

        // Act
        var exists = await SessionRepository.ExistsAsync(session.Id);

        // Assert
        Assert.That(exists, Is.True);
    }

    [Test]
    public async Task ExistsAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Act
        var exists = await SessionRepository.ExistsAsync("non-existing-id");

        // Assert
        Assert.That(exists, Is.False);
    }

    [Test]
    public async Task SaveChangesAsync_ShouldSaveChanges()
    {
        // Arrange
        var session = new Session(_testUser.Id, 30);
        await SessionRepository.AddAsync(session);

        // Act
        await SessionRepository.SaveChangesAsync();

        // Assert
        var savedSession = await SessionRepository.GetByIdAsync(session.Id);
        Assert.That(savedSession, Is.Not.Null);
    }

    [Test]
    public async Task GetActiveSessionsAsync_ShouldReturnOnlyActiveSessions()
    {
        // Arrange
        var activeSession = new Session(_testUser.Id, 30);
        var inactiveSession = new Session(_testUser.Id, 30);
        inactiveSession.Deactivate();

        await SessionRepository.AddAsync(activeSession);
        await SessionRepository.AddAsync(inactiveSession);
        await SessionRepository.SaveChangesAsync();

        // Act
        var activeSessions = await SessionRepository.GetActiveAsync();

        // Assert
        Assert.That(activeSessions, Is.Not.Null);
        Assert.That(activeSessions, Has.Exactly(1).Matches<Session>(s => s.Id == activeSession.Id));
        Assert.That(activeSessions, Has.None.Matches<Session>(s => s.Id == inactiveSession.Id));
    }

    [Test]
    public async Task GetActiveSessionsByUserIdAsync_ShouldReturnActiveSessionsForUser()
    {
        // Arrange
        var user2 = new User("User 2");
        await UserRepository.AddAsync(user2);
        await UserRepository.SaveChangesAsync();

        var activeSession1 = new Session(_testUser.Id, 30);
        var activeSession2 = new Session(user2.Id, 30);
        var inactiveSession = new Session(_testUser.Id, 30);
        inactiveSession.Deactivate();

        await SessionRepository.AddAsync(activeSession1);
        await SessionRepository.AddAsync(activeSession2);
        await SessionRepository.AddAsync(inactiveSession);
        await SessionRepository.SaveChangesAsync();

        // Act
        var activeSessions = await SessionRepository.GetByUserIdAsync(_testUser.Id);
        activeSessions = activeSessions.Where(s => s.IsActive);

        // Assert
        Assert.That(activeSessions, Is.Not.Null);
        Assert.That(activeSessions, Has.Exactly(1).Matches<Session>(s => s.Id == activeSession1.Id));
        Assert.That(activeSessions, Has.None.Matches<Session>(s => s.Id == activeSession2.Id));
        Assert.That(activeSessions, Has.None.Matches<Session>(s => s.Id == inactiveSession.Id));
    }

    [Test]
    public async Task DeleteExpiredAsync_ShouldDeleteExpiredSessions()
    {
        // Arrange
        var validSession = new Session(_testUser.Id, 30);
        var expiredSession = new Session(_testUser.Id, -1); // Expired session

        await SessionRepository.AddAsync(validSession);
        await SessionRepository.AddAsync(expiredSession);
        await SessionRepository.SaveChangesAsync();

        // Act
        var deletedCount = await SessionRepository.DeleteExpiredAsync();
        await SessionRepository.SaveChangesAsync();

        // Assert
        Assert.That(deletedCount, Is.GreaterThan(0));
        var remainingSessions = await SessionRepository.GetAllAsync();
        Assert.That(remainingSessions, Has.Some.Matches<Session>(s => s.Id == validSession.Id));
        Assert.That(remainingSessions, Has.None.Matches<Session>(s => s.Id == expiredSession.Id));
    }

    [Test]
    public async Task Session_WithNavigationProperty_ShouldMaintainRelationship()
    {
        // Arrange
        var session = new Session(_testUser.Id, 30);
        await SessionRepository.AddAsync(session);
        await SessionRepository.SaveChangesAsync();

        // Act
        var retrievedSession = await SessionRepository.GetByIdAsync(session.Id);

        // Assert
        Assert.That(retrievedSession, Is.Not.Null);
        Assert.That(retrievedSession!.User, Is.Not.Null);
        Assert.That(retrievedSession.User.Id, Is.EqualTo(_testUser.Id));
    }

    [Test]
    public async Task Session_IsValid_ShouldReturnCorrectValue()
    {
        // Arrange
        var validSession = new Session(_testUser.Id, 30);
        var expiredSession = new Session(_testUser.Id, -1);
        var inactiveSession = new Session(_testUser.Id, 30);
        inactiveSession.Deactivate();

        await SessionRepository.AddAsync(validSession);
        await SessionRepository.AddAsync(expiredSession);
        await SessionRepository.AddAsync(inactiveSession);
        await SessionRepository.SaveChangesAsync();

        // Act & Assert
        var retrievedValidSession = await SessionRepository.GetByIdAsync(validSession.Id);
        var retrievedExpiredSession = await SessionRepository.GetByIdAsync(expiredSession.Id);
        var retrievedInactiveSession = await SessionRepository.GetByIdAsync(inactiveSession.Id);

        Assert.That(retrievedValidSession!.IsValid(), Is.True);
        Assert.That(retrievedExpiredSession!.IsValid(), Is.False);
        Assert.That(retrievedInactiveSession!.IsValid(), Is.False);
    }

    [Test]
    public async Task Session_Extend_ShouldExtendExpiration()
    {
        // Arrange
        var session = new Session(_testUser.Id, 30);
        await SessionRepository.AddAsync(session);
        await SessionRepository.SaveChangesAsync();

        var originalExpiration = session.ExpiresAt;

        // Act
        session.Extend(10);
        await SessionRepository.UpdateAsync(session);
        await SessionRepository.SaveChangesAsync();

        // Assert
        var updatedSession = await SessionRepository.GetByIdAsync(session.Id);
        Assert.That(updatedSession, Is.Not.Null);
        Assert.That(updatedSession!.ExpiresAt, Is.EqualTo(originalExpiration.AddDays(10)).Within(TimeSpan.FromMinutes(1)));
    }
}
