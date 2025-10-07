using EasterEggHunt.Application.Services;
using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace EasterEggHunt.Application.Tests.Services;

/// <summary>
/// Tests für AuthService
/// </summary>
[TestFixture]
public class AuthServiceTests
{
    private Mock<IAdminUserRepository>? _mockRepository;
    private Mock<ILogger<AuthService>>? _mockLogger;
    private AuthService? _authService;

    [SetUp]
    public void Setup()
    {
        _mockRepository = new Mock<IAdminUserRepository>();
        _mockLogger = new Mock<ILogger<AuthService>>();
        _authService = new AuthService(_mockRepository.Object, _mockLogger.Object);
    }

    [Test]
    public async Task AuthenticateAdminAsync_ValidCredentials_ReturnsAdminUser()
    {
        // Arrange
        var username = "admin";
        var password = "password123";
        var hashedPassword = _authService.HashPassword(password);
        var adminUser = new AdminUser(username, "admin@test.com", hashedPassword);

        _mockRepository.Setup(r => r.GetByUsernameAsync(username))
            .ReturnsAsync(adminUser);
        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<AdminUser>()))
            .ReturnsAsync(adminUser);

        // Act
        var result = await _authService.AuthenticateAdminAsync(username, password);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Username, Is.EqualTo(username));
        Assert.That(result.LastLogin, Is.Not.Null);
        _mockRepository.Verify(r => r.SaveAsync(It.IsAny<AdminUser>()), Times.Once);
    }

    [Test]
    public async Task AuthenticateAdminAsync_InvalidUsername_ReturnsNull()
    {
        // Arrange
        var username = "nonexistent";
        var password = "password123";

        _mockRepository.Setup(r => r.GetByUsernameAsync(username))
            .ReturnsAsync((AdminUser?)null);

        // Act
        var result = await _authService.AuthenticateAdminAsync(username, password);

        // Assert
        Assert.That(result, Is.Null);
        _mockRepository.Verify(r => r.SaveAsync(It.IsAny<AdminUser>()), Times.Never);
    }

    [Test]
    public async Task AuthenticateAdminAsync_InvalidPassword_ReturnsNull()
    {
        // Arrange
        var username = "admin";
        var password = "wrongpassword";
        var hashedPassword = _authService.HashPassword("correctpassword");
        var adminUser = new AdminUser(username, "admin@test.com", hashedPassword);

        _mockRepository.Setup(r => r.GetByUsernameAsync(username))
            .ReturnsAsync(adminUser);

        // Act
        var result = await _authService.AuthenticateAdminAsync(username, password);

        // Assert
        Assert.That(result, Is.Null);
        _mockRepository.Verify(r => r.SaveAsync(It.IsAny<AdminUser>()), Times.Never);
    }

    [Test]
    public async Task AuthenticateAdminAsync_InactiveUser_ReturnsNull()
    {
        // Arrange
        var username = "admin";
        var password = "password123";
        var hashedPassword = _authService.HashPassword(password);
        var adminUser = new AdminUser(username, "admin@test.com", hashedPassword);
        adminUser.Deactivate();

        _mockRepository.Setup(r => r.GetByUsernameAsync(username))
            .ReturnsAsync(adminUser);

        // Act
        var result = await _authService.AuthenticateAdminAsync(username, password);

        // Assert
        Assert.That(result, Is.Null);
        _mockRepository.Verify(r => r.SaveAsync(It.IsAny<AdminUser>()), Times.Never);
    }

    [Test]
    public async Task CreateAdminAsync_ValidData_ReturnsCreatedAdmin()
    {
        // Arrange
        var username = "newadmin";
        var email = "newadmin@test.com";
        var password = "password123";

        _mockRepository.Setup(r => r.GetByUsernameAsync(username))
            .ReturnsAsync((AdminUser?)null);
        _mockRepository.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync((AdminUser?)null);
        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<AdminUser>()))
            .ReturnsAsync((AdminUser admin) => admin);

        // Act
        var result = await _authService.CreateAdminAsync(username, email, password);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Username, Is.EqualTo(username));
        Assert.That(result.Email, Is.EqualTo(email));
        Assert.That(result.IsActive, Is.True);
        Assert.That(result.CreatedAt, Is.Not.EqualTo(DateTime.MinValue));
        _mockRepository.Verify(r => r.SaveAsync(It.IsAny<AdminUser>()), Times.Once);
    }

    [Test]
    public async Task CreateAdminAsync_UsernameExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var username = "existingadmin";
        var email = "newadmin@test.com";
        var password = "password123";
        var existingAdmin = new AdminUser(username, "existing@test.com", "hash");

        _mockRepository.Setup(r => r.GetByUsernameAsync(username))
            .ReturnsAsync(existingAdmin);

        // Act & Assert
        try
        {
            await _authService.CreateAdminAsync(username, email, password);
            Assert.Fail("Expected InvalidOperationException was not thrown");
        }
        catch (InvalidOperationException ex)
        {
            Assert.That(ex.Message, Does.Contain("already exists"));
        }
    }

    [Test]
    public async Task CreateAdminAsync_EmailExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var username = "newadmin";
        var email = "existing@test.com";
        var password = "password123";
        var existingAdmin = new AdminUser("existing", email, "hash");

        _mockRepository.Setup(r => r.GetByUsernameAsync(username))
            .ReturnsAsync((AdminUser?)null);
        _mockRepository.Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync(existingAdmin);

        // Act & Assert
        try
        {
            await _authService.CreateAdminAsync(username, email, password);
            Assert.Fail("Expected InvalidOperationException was not thrown");
        }
        catch (InvalidOperationException ex)
        {
            Assert.That(ex.Message, Does.Contain("already exists"));
        }
    }

    [Test]
    public async Task ChangePasswordAsync_ValidData_ReturnsTrue()
    {
        // Arrange
        var adminId = 1;
        var currentPassword = "oldpassword";
        var newPassword = "newpassword";
        var hashedCurrentPassword = _authService.HashPassword(currentPassword);
        var adminUser = new AdminUser("admin", "admin@test.com", hashedCurrentPassword);

        _mockRepository.Setup(r => r.GetByIdAsync(adminId))
            .ReturnsAsync(adminUser);
        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<AdminUser>()))
            .ReturnsAsync(adminUser);

        // Act
        var result = await _authService.ChangePasswordAsync(adminId, currentPassword, newPassword);

        // Assert
        Assert.That(result, Is.True);
        _mockRepository.Verify(r => r.SaveAsync(It.IsAny<AdminUser>()), Times.Once);
    }

    [Test]
    public async Task ChangePasswordAsync_InvalidCurrentPassword_ReturnsFalse()
    {
        // Arrange
        var adminId = 1;
        var currentPassword = "wrongpassword";
        var newPassword = "newpassword";
        var hashedPassword = _authService.HashPassword("correctpassword");
        var adminUser = new AdminUser("admin", "admin@test.com", hashedPassword);

        _mockRepository.Setup(r => r.GetByIdAsync(adminId))
            .ReturnsAsync(adminUser);

        // Act
        var result = await _authService.ChangePasswordAsync(adminId, currentPassword, newPassword);

        // Assert
        Assert.That(result, Is.False);
        _mockRepository.Verify(r => r.SaveAsync(It.IsAny<AdminUser>()), Times.Never);
    }

    [Test]
    public void HashPassword_ValidPassword_ReturnsHash()
    {
        // Arrange
        var password = "testpassword";

        // Act
        var hash = _authService.HashPassword(password);

        // Assert
        Assert.That(hash, Is.Not.Null);
        Assert.That(hash, Is.Not.EqualTo(password));
        Assert.That(hash.Length, Is.GreaterThan(0));
    }

    [Test]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = "testpassword";
        var hash = _authService.HashPassword(password);

        // Act
        var result = _authService.VerifyPassword(password, hash);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void VerifyPassword_IncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var password = "testpassword";
        var wrongPassword = "wrongpassword";
        var hash = _authService.HashPassword(password);

        // Act
        var result = _authService.VerifyPassword(wrongPassword, hash);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void HashPassword_NullPassword_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _authService.HashPassword(null!));
    }

    [Test]
    public void HashPassword_EmptyPassword_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _authService.HashPassword(""));
    }

    [Test]
    public void VerifyPassword_NullPassword_ReturnsFalse()
    {
        // Arrange
        var hash = "somehash";

        // Act
        var result = _authService.VerifyPassword(null!, hash);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void VerifyPassword_NullHash_ReturnsFalse()
    {
        // Arrange
        var password = "testpassword";

        // Act
        var result = _authService.VerifyPassword(password, null!);

        // Assert
        Assert.That(result, Is.False);
    }
}
