using NUnit.Framework;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using EasterEggHunt.Domain.Entities;
using System.Linq;

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
        retrievedSession.Should().NotBeNull();
        retrievedSession!.UserId.Should().Be(_testUser.Id);
        retrievedSession.IsActive.Should().BeTrue();
        retrievedSession.Data.Should().Be("{}");
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
        retrievedSession.Should().NotBeNull();
        retrievedSession!.Id.Should().Be(session.Id);
    }

    [Test]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Act
        var retrievedSession = await SessionRepository.GetByIdAsync("non-existing-id");

        // Assert
        retrievedSession.Should().BeNull();
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
        sessions.Should().NotBeNull();
        sessions.Should().HaveCount(2);
        sessions.Should().Contain(s => s.Id == session1.Id);
        sessions.Should().Contain(s => s.Id == session2.Id);
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
        sessions.Should().NotBeNull();
        sessions.Should().HaveCount(1);
        sessions.Should().Contain(s => s.Id == session1.Id);
        sessions.Should().NotContain(s => s.Id == session2.Id);
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
        updatedSession.Should().NotBeNull();
        updatedSession!.Data.Should().Be("{\"key\": \"value\"}");
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
        result.Should().BeTrue();
        var deletedSession = await SessionRepository.GetByIdAsync(session.Id);
        deletedSession.Should().BeNull();
    }

    [Test]
    public async Task DeleteAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Act
        var result = await SessionRepository.DeleteAsync("non-existing-id");
        await SessionRepository.SaveChangesAsync();

        // Assert
        result.Should().BeFalse();
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
        exists.Should().BeTrue();
    }

    [Test]
    public async Task ExistsAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Act
        var exists = await SessionRepository.ExistsAsync("non-existing-id");

        // Assert
        exists.Should().BeFalse();
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
        savedSession.Should().NotBeNull();
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
        activeSessions.Should().NotBeNull();
        activeSessions.Should().ContainSingle(s => s.Id == activeSession.Id);
        activeSessions.Should().NotContain(s => s.Id == inactiveSession.Id);
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
        activeSessions.Should().NotBeNull();
        activeSessions.Should().ContainSingle(s => s.Id == activeSession1.Id);
        activeSessions.Should().NotContain(s => s.Id == activeSession2.Id);
        activeSessions.Should().NotContain(s => s.Id == inactiveSession.Id);
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
        deletedCount.Should().BeGreaterThan(0);
        var remainingSessions = await SessionRepository.GetAllAsync();
        remainingSessions.Should().Contain(s => s.Id == validSession.Id);
        remainingSessions.Should().NotContain(s => s.Id == expiredSession.Id);
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
        retrievedSession.Should().NotBeNull();
        retrievedSession!.User.Should().NotBeNull();
        retrievedSession.User.Id.Should().Be(_testUser.Id);
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

        retrievedValidSession!.IsValid().Should().BeTrue();
        retrievedExpiredSession!.IsValid().Should().BeFalse();
        retrievedInactiveSession!.IsValid().Should().BeFalse();
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
        updatedSession.Should().NotBeNull();
        updatedSession!.ExpiresAt.Should().BeCloseTo(originalExpiration.AddDays(10), TimeSpan.FromMinutes(1));
    }
}
