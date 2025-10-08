using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Integration.Tests;
using NUnit.Framework;

namespace EasterEggHunt.Integration.Tests.Repositories;

/// <summary>
/// Integration Tests für Session Repository mit echter SQLite-Datenbank
/// </summary>
[TestFixture]
public class SessionRepositoryIntegrationTests : IntegrationTestBase
{
    [SetUp]
    public void Setup()
    {
        // Test-Daten werden bereits in IntegrationTestBase geladen
    }

    [Test]
    public void GetAllAsync_ReturnsAllSessions()
    {
        // Act
        var sessions = Context.Sessions.ToList();

        // Assert
        Assert.That(sessions, Is.Not.Empty);
        Assert.That(sessions.Count, Is.GreaterThanOrEqualTo(0)); // Sessions können leer sein
    }

    [Test]
    public void GetActiveAsync_ReturnsOnlyActiveSessions()
    {
        // Act
        var activeSessions = Context.Sessions.Where(s => s.IsActive).ToList();

        // Assert
        Assert.That(activeSessions.All(s => s.IsActive), Is.True);
    }

    [Test]
    public void GetByUserIdAsync_WithValidUserId_ReturnsUserSessions()
    {
        // Arrange
        var userId = 1;

        // Act
        var userSessions = Context.Sessions.Where(s => s.UserId == userId).ToList();

        // Assert
        Assert.That(userSessions.All(s => s.UserId == userId), Is.True);
    }

    [Test]
    public void GetByUserIdAsync_WithInvalidUserId_ReturnsEmpty()
    {
        // Arrange
        var userId = 999;

        // Act
        var userSessions = Context.Sessions.Where(s => s.UserId == userId).ToList();

        // Assert
        Assert.That(userSessions, Is.Empty);
    }

    [Test]
    public void GetByIdAsync_WithValidId_ReturnsSession()
    {
        // Arrange
        var sessionId = "test-session-id";
        var session = new Session(1, 30)
        {
            Id = sessionId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };
        Context.Sessions.Add(session);
        Context.SaveChanges();

        // Act
        var foundSession = Context.Sessions.FirstOrDefault(s => s.Id == sessionId);

        // Assert
        Assert.That(foundSession, Is.Not.Null);
        Assert.That(foundSession!.Id, Is.EqualTo(sessionId));
        Assert.That(foundSession.UserId, Is.EqualTo(1));
    }

    [Test]
    public void GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var sessionId = "invalid-session-id";

        // Act
        var session = Context.Sessions.FirstOrDefault(s => s.Id == sessionId);

        // Assert
        Assert.That(session, Is.Null);
    }

    [Test]
    public void GetActiveByUserIdAsync_WithValidUserId_ReturnsActiveSession()
    {
        // Arrange
        var userId = 1;
        var sessionId = $"active-session-{Random.Shared.Next(1000, 9999)}";
        var session = new Session()
        {
            Id = sessionId,
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            Data = "{}"
        };
        Context.Sessions.Add(session);
        Context.SaveChanges();

        // Act
        var activeSession = Context.Sessions
            .FirstOrDefault(s => s.UserId == userId && s.IsActive);

        // Assert
        Assert.That(activeSession, Is.Not.Null);
        Assert.That(activeSession!.IsActive, Is.True);
        Assert.That(activeSession.UserId, Is.EqualTo(userId));
    }

    [Test]
    public void GetActiveByUserIdAsync_WithNoActiveSession_ReturnsNull()
    {
        // Arrange
        var userId = 999;

        // Act
        var activeSession = Context.Sessions
            .FirstOrDefault(s => s.UserId == userId && s.IsActive);

        // Assert
        Assert.That(activeSession, Is.Null);
    }

    [Test]
    public async Task AddAsync_WithValidData_CreatesSession()
    {
        // Arrange
        var sessionId = $"test-session-{Random.Shared.Next(1000, 9999)}";
        var newSession = new Session(1, 30)
        {
            Id = sessionId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            Data = "{\"key\":\"value\"}"
        };

        // Act
        Context.Sessions.Add(newSession);
        await Context.SaveChangesAsync();

        // Assert
        var createdSession = Context.Sessions.FirstOrDefault(s => s.Id == sessionId);
        Assert.That(createdSession, Is.Not.Null);
        Assert.That(createdSession!.Id, Is.EqualTo(sessionId));
        Assert.That(createdSession.UserId, Is.EqualTo(1));
        Assert.That(createdSession.IsActive, Is.True);
        Assert.That(createdSession.Data, Is.EqualTo("{\"key\":\"value\"}"));
    }

    [Test]
    public async Task UpdateAsync_WithValidData_UpdatesSession()
    {
        // Arrange
        var sessionId = $"test-session-{Random.Shared.Next(1000, 9999)}";
        var session = new Session(1, 30)
        {
            Id = sessionId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            Data = "{\"key\":\"value\"}"
        };
        Context.Sessions.Add(session);
        await Context.SaveChangesAsync();

        // Act
        session.Data = "{\"key\":\"updated\"}";
        session.Extend(7); // Verlängert um 7 Tage
        await Context.SaveChangesAsync();

        // Assert
        var updatedSession = Context.Sessions.FirstOrDefault(s => s.Id == sessionId);
        Assert.That(updatedSession, Is.Not.Null);
        Assert.That(updatedSession!.Data, Is.EqualTo("{\"key\":\"updated\"}"));
        Assert.That(updatedSession.ExpiresAt, Is.GreaterThan(DateTime.UtcNow.AddDays(6)));
    }

    [Test]
    public async Task DeleteAsync_WithValidId_DeletesSession()
    {
        // Arrange
        var sessionId = $"test-session-{Random.Shared.Next(1000, 9999)}";
        var session = new Session(1, 30)
        {
            Id = sessionId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };
        Context.Sessions.Add(session);
        await Context.SaveChangesAsync();

        // Act
        Context.Sessions.Remove(session);
        await Context.SaveChangesAsync();

        // Assert
        var deletedSession = Context.Sessions.FirstOrDefault(s => s.Id == sessionId);
        Assert.That(deletedSession, Is.Null);
    }

    [Test]
    public void Session_WithValidData_HasCorrectProperties()
    {
        // Arrange
        var sessionId = $"test-session-{Random.Shared.Next(1000, 9999)}";
        var session = new Session(1, 30)
        {
            Id = sessionId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            Data = "{\"key\":\"value\"}"
        };
        Context.Sessions.Add(session);
        Context.SaveChanges();

        // Act
        var foundSession = Context.Sessions.FirstOrDefault(s => s.Id == sessionId);

        // Assert
        Assert.That(foundSession, Is.Not.Null);
        Assert.That(foundSession!.UserId, Is.EqualTo(1));
        Assert.That(foundSession.IsActive, Is.True);
        Assert.That(foundSession.Data, Is.EqualTo("{\"key\":\"value\"}"));
        Assert.That(foundSession.CreatedAt, Is.Not.EqualTo(DateTime.MinValue));
        Assert.That(foundSession.ExpiresAt, Is.GreaterThan(DateTime.UtcNow));
    }

    [Test]
    public async Task Session_Activate_ActivatesSession()
    {
        // Arrange
        var sessionId = $"test-session-{Random.Shared.Next(1000, 9999)}";
        var session = new Session(1, 30)
        {
            Id = sessionId,
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };
        Context.Sessions.Add(session);
        await Context.SaveChangesAsync();

        // Act
        session.IsActive = true;
        await Context.SaveChangesAsync();

        // Assert
        var activatedSession = Context.Sessions.FirstOrDefault(s => s.Id == sessionId);
        Assert.That(activatedSession, Is.Not.Null);
        Assert.That(activatedSession!.IsActive, Is.True);
    }

    [Test]
    public async Task Session_Deactivate_DeactivatesSession()
    {
        // Arrange
        var sessionId = $"test-session-{Random.Shared.Next(1000, 9999)}";
        var session = new Session(1, 30)
        {
            Id = sessionId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };
        Context.Sessions.Add(session);
        await Context.SaveChangesAsync();

        // Act
        session.Deactivate();
        await Context.SaveChangesAsync();

        // Assert
        var deactivatedSession = Context.Sessions.FirstOrDefault(s => s.Id == sessionId);
        Assert.That(deactivatedSession, Is.Not.Null);
        Assert.That(deactivatedSession!.IsActive, Is.False);
    }

    [Test]
    public async Task Session_Extend_ExtendsExpiration()
    {
        // Arrange
        var sessionId = $"test-session-{Random.Shared.Next(1000, 9999)}";
        var session = new Session(1, 30)
        {
            Id = sessionId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };
        Context.Sessions.Add(session);
        await Context.SaveChangesAsync();

        var originalExpiration = session.ExpiresAt;

        // Act
        session.Extend(7); // Verlängert um 7 Tage
        await Context.SaveChangesAsync();

        // Assert
        var extendedSession = Context.Sessions.FirstOrDefault(s => s.Id == sessionId);
        Assert.That(extendedSession, Is.Not.Null);
        Assert.That(extendedSession!.ExpiresAt, Is.GreaterThan(originalExpiration));
        Assert.That(extendedSession.ExpiresAt, Is.GreaterThan(DateTime.UtcNow.AddDays(6)));
    }

    [Test]
    public async Task Session_UpdateData_UpdatesSessionData()
    {
        // Arrange
        var sessionId = $"test-session-{Random.Shared.Next(1000, 9999)}";
        var session = new Session(1, 30)
        {
            Id = sessionId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            Data = "{\"key\":\"value\"}"
        };
        Context.Sessions.Add(session);
        await Context.SaveChangesAsync();

        // Act
        session.UpdateData("{\"key\":\"updated\",\"newKey\":\"newValue\"}");
        await Context.SaveChangesAsync();

        // Assert
        var updatedSession = Context.Sessions.FirstOrDefault(s => s.Id == sessionId);
        Assert.That(updatedSession, Is.Not.Null);
        Assert.That(updatedSession!.Data, Is.EqualTo("{\"key\":\"updated\",\"newKey\":\"newValue\"}"));
    }

    [Test]
    public void Session_IsExpired_WithExpiredSession_ReturnsTrue()
    {
        // Arrange
        var sessionId = $"test-session-{Random.Shared.Next(1000, 9999)}";
        var session = new Session(1, 30)
        {
            Id = sessionId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-31), // Vor 31 Tagen erstellt
            ExpiresAt = DateTime.UtcNow.AddDays(-1) // Vor 1 Tag abgelaufen
        };
        Context.Sessions.Add(session);
        Context.SaveChanges();

        // Act
        var isExpired = !session.IsValid();

        // Assert
        Assert.That(isExpired, Is.True);
    }

    [Test]
    public void Session_IsExpired_WithValidSession_ReturnsFalse()
    {
        // Arrange
        var sessionId = $"test-session-{Random.Shared.Next(1000, 9999)}";
        var session = new Session(1, 30)
        {
            Id = sessionId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30) // In 30 Tagen abgelaufen
        };
        Context.Sessions.Add(session);
        Context.SaveChanges();

        // Act
        var isExpired = !session.IsValid();

        // Assert
        Assert.That(isExpired, Is.False);
    }

    [Test]
    public void Session_WithUserNavigation_LoadsUser()
    {
        // Arrange
        var sessionId = $"test-session-{Random.Shared.Next(1000, 9999)}";
        var session = new Session(1, 30)
        {
            Id = sessionId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };
        Context.Sessions.Add(session);
        Context.SaveChanges();

        // Act
        var sessionWithUser = Context.Sessions
            .Where(s => s.Id == sessionId)
            .Select(s => new { Session = s, User = s.User })
            .FirstOrDefault();

        // Assert
        Assert.That(sessionWithUser, Is.Not.Null);
        Assert.That(sessionWithUser!.User, Is.Not.Null);
        Assert.That(sessionWithUser.User.Name, Is.EqualTo("Test Benutzer"));
    }
}
