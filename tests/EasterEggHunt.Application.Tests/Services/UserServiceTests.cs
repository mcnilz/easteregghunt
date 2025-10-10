using EasterEggHunt.Application.Services;
using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace EasterEggHunt.Application.Tests.Services;

[TestFixture]
public class UserServiceTests
{
    private Mock<IUserRepository> _mockRepository = null!;
    private Mock<ILogger<UserService>> _mockLogger = null!;
    private UserService _userService = null!;

    [SetUp]
    public void Setup()
    {
        _mockRepository = new Mock<IUserRepository>();
        _mockLogger = new Mock<ILogger<UserService>>();
        _userService = new UserService(_mockRepository.Object, _mockLogger.Object);
    }

    #region Constructor Tests

    [Test]
    public void UserService_Constructor_WithNullRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new UserService(null!, _mockLogger.Object));
    }

    [Test]
    public void UserService_Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new UserService(_mockRepository.Object, null!));
    }

    [Test]
    public void UserService_Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Act
        var service = new UserService(_mockRepository.Object, _mockLogger.Object);

        // Assert
        Assert.That(service, Is.Not.Null);
    }

    #endregion

    #region GetUserByIdAsync Tests

    [Test]
    public async Task GetUserByIdAsync_WithValidId_ReturnsUser()
    {
        // Arrange
        var userId = 1;
        var user = new User("testuser");

        _mockRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(user));
        _mockRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
    }

    [Test]
    public async Task GetUserByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var userId = 999;
        _mockRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        Assert.That(result, Is.Null);
        _mockRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
    }

    #endregion

    #region CreateUserAsync Tests

    [Test]
    public async Task CreateUserAsync_WithValidName_ReturnsCreatedUser()
    {
        // Arrange
        var userName = "testuser";
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User user) => user);
        _mockRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _userService.CreateUserAsync(userName);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo(userName));
        Assert.That(result.IsActive, Is.True);
        Assert.That(result.FirstSeen, Is.Not.EqualTo(DateTime.MinValue));
        Assert.That(result.LastSeen, Is.Not.EqualTo(DateTime.MinValue));

        _mockRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void CreateUserAsync_WithEmptyName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _userService.CreateUserAsync(""));
    }

    [Test]
    public void CreateUserAsync_WithWhitespaceName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _userService.CreateUserAsync("   "));
    }

    [Test]
    public void CreateUserAsync_WithNullName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _userService.CreateUserAsync(null!));
    }

    #endregion

    #region UpdateUserLastSeenAsync Tests

    [Test]
    public async Task UpdateUserLastSeenAsync_WithValidId_ReturnsTrue()
    {
        // Arrange
        var userId = 1;
        var user = new User("testuser");
        var originalLastSeen = user.LastSeen;

        _mockRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mockRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _userService.UpdateUserLastSeenAsync(userId);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(user.LastSeen, Is.GreaterThan(originalLastSeen));
        _mockRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task UpdateUserLastSeenAsync_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var userId = 999;
        _mockRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.UpdateUserLastSeenAsync(userId);

        // Assert
        Assert.That(result, Is.False);
        _mockRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    #endregion

    #region GetActiveUsersAsync Tests

    [Test]
    public async Task GetActiveUsersAsync_ReturnsActiveUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User("user1"),
            new User("user2")
        };

        _mockRepository.Setup(r => r.GetActiveAsync())
            .ReturnsAsync(users);

        // Act
        var result = await _userService.GetActiveUsersAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(2));
        _mockRepository.Verify(r => r.GetActiveAsync(), Times.Once);
    }

    [Test]
    public async Task GetActiveUsersAsync_WithNoActiveUsers_ReturnsEmptyCollection()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetActiveAsync())
            .ReturnsAsync(new List<User>());

        // Act
        var result = await _userService.GetActiveUsersAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(0));
        _mockRepository.Verify(r => r.GetActiveAsync(), Times.Once);
    }

    #endregion

    #region DeactivateUserAsync Tests

    [Test]
    public async Task DeactivateUserAsync_WithValidId_ReturnsTrue()
    {
        // Arrange
        var userId = 1;
        var user = new User("testuser");
        Assert.That(user.IsActive, Is.True); // Ensure user starts as active

        _mockRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mockRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _userService.DeactivateUserAsync(userId);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(user.IsActive, Is.False);
        _mockRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task DeactivateUserAsync_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var userId = 999;
        _mockRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.DeactivateUserAsync(userId);

        // Assert
        Assert.That(result, Is.False);
        _mockRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    #endregion

    #region UserNameExistsAsync Tests

    [Test]
    public async Task UserNameExistsAsync_WithExistingName_ReturnsTrue()
    {
        // Arrange
        var userName = "existinguser";
        _mockRepository.Setup(r => r.NameExistsAsync(userName))
            .ReturnsAsync(true);

        // Act
        var result = await _userService.UserNameExistsAsync(userName);

        // Assert
        Assert.That(result, Is.True);
        _mockRepository.Verify(r => r.NameExistsAsync(userName), Times.Once);
    }

    [Test]
    public async Task UserNameExistsAsync_WithNonExistingName_ReturnsFalse()
    {
        // Arrange
        var userName = "newuser";
        _mockRepository.Setup(r => r.NameExistsAsync(userName))
            .ReturnsAsync(false);

        // Act
        var result = await _userService.UserNameExistsAsync(userName);

        // Assert
        Assert.That(result, Is.False);
        _mockRepository.Verify(r => r.NameExistsAsync(userName), Times.Once);
    }

    [Test]
    public void UserNameExistsAsync_WithEmptyName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _userService.UserNameExistsAsync(""));
    }

    [Test]
    public void UserNameExistsAsync_WithWhitespaceName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _userService.UserNameExistsAsync("   "));
    }

    [Test]
    public void UserNameExistsAsync_WithNullName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _userService.UserNameExistsAsync(null!));
    }

    #endregion

    #region CreateUserAsync with Duplicate Name Tests

    [Test]
    public void CreateUserAsync_WithExistingName_ThrowsInvalidOperationException()
    {
        // Arrange
        var userName = "existinguser";
        _mockRepository.Setup(r => r.NameExistsAsync(userName))
            .ReturnsAsync(true);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _userService.CreateUserAsync(userName));
        Assert.That(ex!.Message, Does.Contain("bereits vergeben"));
        _mockRepository.Verify(r => r.NameExistsAsync(userName), Times.Once);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Test]
    public async Task CreateUserAsync_WithNonExistingName_CreatesUser()
    {
        // Arrange
        var userName = "newuser";
        _mockRepository.Setup(r => r.NameExistsAsync(userName))
            .ReturnsAsync(false);
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User user) => user);
        _mockRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _userService.CreateUserAsync(userName);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo(userName));
        _mockRepository.Verify(r => r.NameExistsAsync(userName), Times.Once);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    #endregion
}
