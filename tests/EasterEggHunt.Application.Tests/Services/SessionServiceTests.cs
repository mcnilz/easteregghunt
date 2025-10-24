using EasterEggHunt.Application.Services;
using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

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

    #region Constructor Tests

    [Test]
    public void SessionService_Constructor_WithNullSessionRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SessionService(
            null!,
            _mockUserRepository.Object,
            _mockLogger.Object));
    }

    [Test]
    public void SessionService_Constructor_WithNullUserRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SessionService(
            _mockSessionRepository.Object,
            null!,
            _mockLogger.Object));
    }

    [Test]
    public void SessionService_Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SessionService(
            _mockSessionRepository.Object,
            _mockUserRepository.Object,
            null!));
    }

    [Test]
    public void SessionService_Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Act
        var service = new SessionService(
            _mockSessionRepository.Object,
            _mockUserRepository.Object,
            _mockLogger.Object);

        // Assert
        Assert.That(service, Is.Not.Null);
    }

    #endregion

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
            .ReturnsAsync((Session s) => s);
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
    public void CreateSessionAsync_WithNonExistentUser_ThrowsArgumentException()
    {
        // Arrange
        var userId = 999;
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _sessionService.CreateSessionAsync(userId));
        Assert.That(ex!.Message, Does.Contain($"Benutzer mit ID {userId} nicht gefunden"));
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
    public void GetSessionByIdAsync_WithEmptyId_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _sessionService.GetSessionByIdAsync(""));
    }

    [Test]
    public void GetSessionByIdAsync_WithWhitespaceId_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _sessionService.GetSessionByIdAsync("   "));
    }

    [Test]
    public void GetSessionByIdAsync_WithNullId_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _sessionService.GetSessionByIdAsync(null!));
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
    public void ExtendSessionAsync_WithEmptyId_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _sessionService.ExtendSessionAsync("", 7));
    }

    [Test]
    public void ExtendSessionAsync_WithWhitespaceId_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _sessionService.ExtendSessionAsync("   ", 7));
    }

    [Test]
    public void ExtendSessionAsync_WithNullId_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _sessionService.ExtendSessionAsync(null!, 7));
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
    public void DeactivateSessionAsync_WithEmptyId_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _sessionService.DeactivateSessionAsync(""));
    }

    [Test]
    public void DeactivateSessionAsync_WithWhitespaceId_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _sessionService.DeactivateSessionAsync("   "));
    }

    [Test]
    public void DeactivateSessionAsync_WithNullId_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _sessionService.DeactivateSessionAsync(null!));
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
    public void UpdateSessionDataAsync_WithEmptyId_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _sessionService.UpdateSessionDataAsync("", "data"));
    }

    [Test]
    public void UpdateSessionDataAsync_WithWhitespaceId_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _sessionService.UpdateSessionDataAsync("   ", "data"));
    }

    [Test]
    public void UpdateSessionDataAsync_WithNullId_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _sessionService.UpdateSessionDataAsync(null!, "data"));
    }

    [Test]
    public void UpdateSessionDataAsync_WithEmptyData_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _sessionService.UpdateSessionDataAsync("session-id", ""));
    }

    [Test]
    public void UpdateSessionDataAsync_WithWhitespaceData_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _sessionService.UpdateSessionDataAsync("session-id", "   "));
    }

    [Test]
    public void UpdateSessionDataAsync_WithNullData_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _sessionService.UpdateSessionDataAsync("session-id", null!));
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

    [Test]
    public void ValidateSessionAsync_WithEmptyId_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _sessionService.ValidateSessionAsync(""));
    }

    [Test]
    public void ValidateSessionAsync_WithWhitespaceId_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _sessionService.ValidateSessionAsync("   "));
    }

    [Test]
    public void ValidateSessionAsync_WithNullId_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _sessionService.ValidateSessionAsync(null!));
    }
}
