using EasterEggHunt.Application.Services;
using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace EasterEggHunt.Application.Tests.Services;

/// <summary>
/// Unit Tests für GdprService
/// Testet GDPR-Compliance-Operationen (Recht auf Löschung)
/// </summary>
[TestFixture]
public sealed class GdprServiceTests
{
    private Mock<ISessionRepository> _mockSessionRepository = null!;
    private Mock<IUserRepository> _mockUserRepository = null!;
    private Mock<IFindRepository> _mockFindRepository = null!;
    private Mock<ILogger<GdprService>> _mockLogger = null!;
    private GdprService _gdprService = null!;

    [SetUp]
    public void Setup()
    {
        _mockSessionRepository = new Mock<ISessionRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockFindRepository = new Mock<IFindRepository>();
        _mockLogger = new Mock<ILogger<GdprService>>();

        _gdprService = new GdprService(
            _mockSessionRepository.Object,
            _mockUserRepository.Object,
            _mockFindRepository.Object,
            _mockLogger.Object);
    }

    #region Constructor Tests

    [Test]
    public void GdprService_Constructor_WithNullSessionRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GdprService(
            null!,
            _mockUserRepository.Object,
            _mockFindRepository.Object,
            _mockLogger.Object));
    }

    [Test]
    public void GdprService_Constructor_WithNullUserRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GdprService(
            _mockSessionRepository.Object,
            null!,
            _mockFindRepository.Object,
            _mockLogger.Object));
    }

    [Test]
    public void GdprService_Constructor_WithNullFindRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GdprService(
            _mockSessionRepository.Object,
            _mockUserRepository.Object,
            null!,
            _mockLogger.Object));
    }

    [Test]
    public void GdprService_Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GdprService(
            _mockSessionRepository.Object,
            _mockUserRepository.Object,
            _mockFindRepository.Object,
            null!));
    }

    #endregion

    #region DeleteUserDataAsync Tests

    [Test]
    public async Task DeleteUserDataAsync_WithExistingUser_ShouldDeleteAllSessions()
    {
        // Arrange
        var userId = 1;
        var user = new User("Test User") { Id = userId };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mockSessionRepository.Setup(x => x.DeleteAllByUserIdAsync(userId))
            .ReturnsAsync(5); // 5 Sessions gelöscht
        _mockUserRepository.Setup(x => x.DeleteAsync(userId))
            .ReturnsAsync(true);

        // Act
        var result = await _gdprService.DeleteUserDataAsync(userId, deleteFinds: false);

        // Assert
        Assert.That(result.DeletedSessions, Is.EqualTo(5));
        Assert.That(result.UserDeleted, Is.True);
        Assert.That(result.DeletedFinds, Is.EqualTo(0));

        _mockSessionRepository.Verify(x => x.DeleteAllByUserIdAsync(userId), Times.Once);
        _mockUserRepository.Verify(x => x.DeleteAsync(userId), Times.Once);
        _mockFindRepository.Verify(x => x.GetByUserIdAsync(userId), Times.Never);
    }

    [Test]
    public async Task DeleteUserDataAsync_WithDeleteFindsTrue_ShouldDeleteFindsAndSessions()
    {
        // Arrange
        var userId = 1;
        var user = new User("Test User") { Id = userId };
        var finds = new List<Find>
        {
            new Find(1, userId, "127.0.0.1", "User Agent 1") { Id = 1 },
            new Find(2, userId, "127.0.0.1", "User Agent 2") { Id = 2 }
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mockSessionRepository.Setup(x => x.DeleteAllByUserIdAsync(userId))
            .ReturnsAsync(3); // 3 Sessions gelöscht
        _mockFindRepository.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(finds);
        _mockFindRepository.Setup(x => x.DeleteAsync(It.IsAny<int>()))
            .ReturnsAsync(true);
        _mockUserRepository.Setup(x => x.DeleteAsync(userId))
            .ReturnsAsync(true);

        // Act
        var result = await _gdprService.DeleteUserDataAsync(userId, deleteFinds: true);

        // Assert
        Assert.That(result.DeletedSessions, Is.EqualTo(3));
        Assert.That(result.DeletedFinds, Is.EqualTo(2));
        Assert.That(result.UserDeleted, Is.True);
        Assert.That(result.TotalDeleted, Is.EqualTo(6)); // 3 Sessions + 2 Finds + 1 User

        _mockSessionRepository.Verify(x => x.DeleteAllByUserIdAsync(userId), Times.Once);
        _mockFindRepository.Verify(x => x.GetByUserIdAsync(userId), Times.Once);
        _mockFindRepository.Verify(x => x.DeleteAsync(1), Times.Once);
        _mockFindRepository.Verify(x => x.DeleteAsync(2), Times.Once);
        _mockUserRepository.Verify(x => x.DeleteAsync(userId), Times.Once);
    }

    [Test]
    public async Task DeleteUserDataAsync_WithNonExistingUser_ShouldReturnZero()
    {
        // Arrange
        var userId = 999;
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _gdprService.DeleteUserDataAsync(userId, deleteFinds: false);

        // Assert
        Assert.That(result.DeletedSessions, Is.EqualTo(0));
        Assert.That(result.DeletedFinds, Is.EqualTo(0));
        Assert.That(result.UserDeleted, Is.False);
        Assert.That(result.TotalDeleted, Is.EqualTo(0));

        _mockSessionRepository.Verify(x => x.DeleteAllByUserIdAsync(userId), Times.Never);
        _mockUserRepository.Verify(x => x.DeleteAsync(userId), Times.Never);
    }

    #endregion

    #region AnonymizeUserDataAsync Tests

    [Test]
    public async Task AnonymizeUserDataAsync_WithExistingUser_ShouldAnonymizeUser()
    {
        // Arrange
        var userId = 1;
        var user = new User("Test User") { Id = userId };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mockSessionRepository.Setup(x => x.DeleteAllByUserIdAsync(userId))
            .ReturnsAsync(3);
        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);
        _mockUserRepository.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _gdprService.AnonymizeUserDataAsync(userId);

        // Assert
        Assert.That(result, Is.True);

        _mockSessionRepository.Verify(x => x.DeleteAllByUserIdAsync(userId), Times.Once);
        _mockUserRepository.Verify(x => x.UpdateAsync(It.Is<User>(u =>
            u.Id == userId &&
            u.Name.StartsWith("Anonymized_User_"))), Times.Once);
        _mockUserRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task AnonymizeUserDataAsync_WithNonExistingUser_ShouldReturnFalse()
    {
        // Arrange
        var userId = 999;
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _gdprService.AnonymizeUserDataAsync(userId);

        // Assert
        Assert.That(result, Is.False);

        _mockSessionRepository.Verify(x => x.DeleteAllByUserIdAsync(userId), Times.Never);
        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Test]
    public async Task AnonymizeUserDataAsync_ShouldCreateUniqueAnonymizedName()
    {
        // Arrange
        var userId = 1;
        var user = new User("Test User") { Id = userId };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mockSessionRepository.Setup(x => x.DeleteAllByUserIdAsync(userId))
            .ReturnsAsync(0);
        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => u);
        _mockUserRepository.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _gdprService.AnonymizeUserDataAsync(userId);

        // Assert
        Assert.That(result, Is.True);

        // Prüfe dass anonymisierter Name eindeutig ist
        _mockUserRepository.Verify(x => x.UpdateAsync(It.Is<User>(u =>
            u.Name.Contains("Anonymized_User_") &&
            u.Name.Contains(userId.ToString(System.Globalization.CultureInfo.InvariantCulture)) &&
            u.Name.Length > "Anonymized_User_1_".Length)), Times.Once);
    }

    #endregion
}
