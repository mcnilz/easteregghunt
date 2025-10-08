using EasterEggHunt.Application.Services;
using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace EasterEggHunt.Application.Tests.Services;

/// <summary>
/// Tests für SessionService
/// </summary>
[TestFixture]
public class SessionServiceTests
{
    private Mock<ISessionRepository> _mockSessionRepository = null!;
    private Mock<IUserRepository> _mockUserRepository = null!;
    private Mock<ILogger<SessionService>> _mockLogger = null!;
    private SessionService _sessionService = null!;

    [SetUp]
    public void Setup()
    {
        _mockSessionRepository = new Mock<ISessionRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockLogger = new Mock<ILogger<SessionService>>();

        _sessionService = new SessionService(
            _mockSessionRepository.Object,
            _mockUserRepository.Object,
            _mockLogger.Object);
    }

    [Test]
    public async Task CreateSessionAsync_WithValidUserId_ReturnsSession()
    {
        // Arrange
        var userId = 1;
        var user = new User("Test User") { Id = userId };
        var session = new Session(userId, 30);

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mockSessionRepository.Setup(x => x.AddAsync(It.IsAny<Session>()))
            .Callback<Session>(s => s.Id = "test-session-id");
        _mockSessionRepository.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _sessionService.CreateSessionAsync(userId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserId, Is.EqualTo(userId));
        Assert.That(result.ExpiresAt, Is.GreaterThan(DateTime.UtcNow));
        _mockSessionRepository.Verify(x => x.AddAsync(It.IsAny<Session>()), Times.Once);
        _mockSessionRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task GetSessionByIdAsync_WithValidId_ReturnsSession()
    {
        // Arrange
        var sessionId = "test-session-id";
        var session = new Session(1, 30) { Id = sessionId };

        _mockSessionRepository.Setup(x => x.GetByIdAsync(sessionId))
            .ReturnsAsync(session);

        // Act
        var result = await _sessionService.GetSessionByIdAsync(sessionId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(sessionId));
    }

    [Test]
    public async Task GetSessionByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var sessionId = "invalid-session-id";
        _mockSessionRepository.Setup(x => x.GetByIdAsync(sessionId))
            .ReturnsAsync((Session?)null);

        // Act
        var result = await _sessionService.GetSessionByIdAsync(sessionId);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetSessionsByUserIdAsync_ReturnsSessions()
    {
        // Arrange
        var userId = 1;
        var sessions = new List<Session>
        {
            new Session(userId, 30) { Id = "session-1" },
            new Session(userId, 30) { Id = "session-2" }
        };

        _mockSessionRepository.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(sessions);

        // Act
        var result = await _sessionService.GetSessionsByUserIdAsync(userId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task ExtendSessionAsync_WithValidId_ReturnsTrue()
    {
        // Arrange
        var sessionId = "test-session-id";
        var session = new Session(1, 30) { Id = sessionId };
        var extensionDays = 7;

        _mockSessionRepository.Setup(x => x.GetByIdAsync(sessionId))
            .ReturnsAsync(session);
        _mockSessionRepository.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _sessionService.ExtendSessionAsync(sessionId, extensionDays);

        // Assert
        Assert.That(result, Is.True);
        _mockSessionRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task ExtendSessionAsync_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var sessionId = "invalid-session-id";
        _mockSessionRepository.Setup(x => x.GetByIdAsync(sessionId))
            .ReturnsAsync((Session?)null);

        // Act
        var result = await _sessionService.ExtendSessionAsync(sessionId, 7);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task DeactivateSessionAsync_WithValidId_ReturnsTrue()
    {
        // Arrange
        var sessionId = "test-session-id";
        var session = new Session(1, 30) { Id = sessionId };

        _mockSessionRepository.Setup(x => x.GetByIdAsync(sessionId))
            .ReturnsAsync(session);
        _mockSessionRepository.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _sessionService.DeactivateSessionAsync(sessionId);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(session.IsActive, Is.False);
        _mockSessionRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task DeactivateSessionAsync_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var sessionId = "invalid-session-id";
        _mockSessionRepository.Setup(x => x.GetByIdAsync(sessionId))
            .ReturnsAsync((Session?)null);

        // Act
        var result = await _sessionService.DeactivateSessionAsync(sessionId);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task UpdateSessionDataAsync_WithValidId_ReturnsTrue()
    {
        // Arrange
        var sessionId = "test-session-id";
        var session = new Session(1, 30) { Id = sessionId };
        var data = "{\"key\": \"value\"}";

        _mockSessionRepository.Setup(x => x.GetByIdAsync(sessionId))
            .ReturnsAsync(session);
        _mockSessionRepository.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _sessionService.UpdateSessionDataAsync(sessionId, data);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(session.Data, Is.EqualTo(data));
        _mockSessionRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task UpdateSessionDataAsync_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var sessionId = "invalid-session-id";
        _mockSessionRepository.Setup(x => x.GetByIdAsync(sessionId))
            .ReturnsAsync((Session?)null);

        // Act
        var result = await _sessionService.UpdateSessionDataAsync(sessionId, "data");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ValidateSessionAsync_WithValidSession_ReturnsTrue()
    {
        // Arrange
        var sessionId = "test-session-id";
        var session = new Session(1, 30) { Id = sessionId };

        _mockSessionRepository.Setup(x => x.GetByIdAsync(sessionId))
            .ReturnsAsync(session);

        // Act
        var result = await _sessionService.ValidateSessionAsync(sessionId);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task ValidateSessionAsync_WithInvalidSession_ReturnsFalse()
    {
        // Arrange
        var sessionId = "invalid-session-id";
        _mockSessionRepository.Setup(x => x.GetByIdAsync(sessionId))
            .ReturnsAsync((Session?)null);

        // Act
        var result = await _sessionService.ValidateSessionAsync(sessionId);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ValidateSessionAsync_WithExpiredSession_ReturnsFalse()
    {
        // Arrange
        var sessionId = "expired-session-id";
        var session = new Session(1, -1) { Id = sessionId }; // Expired session

        _mockSessionRepository.Setup(x => x.GetByIdAsync(sessionId))
            .ReturnsAsync(session);

        // Act
        var result = await _sessionService.ValidateSessionAsync(sessionId);

        // Assert
        Assert.That(result, Is.False);
    }
}
