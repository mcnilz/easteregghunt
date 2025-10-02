using EasterEggHunt.Domain.Entities;
using FluentAssertions;
using NUnit.Framework;

namespace EasterEggHunt.Domain.Tests.Entities;

/// <summary>
/// Tests für die AdminUser Entity
/// </summary>
[TestFixture]
public class AdminUserTests
{
    private const string ValidUsername = "admin";
    private const string ValidEmail = "admin@company.com";
    private const string ValidPasswordHash = "hashed_password_123";

    [Test]
    public void Constructor_WithValidParameters_ShouldCreateAdminUser()
    {
        // Act
        var adminUser = new AdminUser(ValidUsername, ValidEmail, ValidPasswordHash);

        // Assert
        adminUser.Username.Should().Be(ValidUsername);
        adminUser.Email.Should().Be(ValidEmail);
        adminUser.PasswordHash.Should().Be(ValidPasswordHash);
        adminUser.IsActive.Should().BeTrue();
        adminUser.LastLogin.Should().BeNull();
        adminUser.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public void Constructor_WithNullUsername_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new AdminUser(null!, ValidEmail, ValidPasswordHash);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("username");
    }

    [Test]
    public void Constructor_WithNullEmail_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new AdminUser(ValidUsername, null!, ValidPasswordHash);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("email");
    }

    [Test]
    public void Constructor_WithNullPasswordHash_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new AdminUser(ValidUsername, ValidEmail, null!);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("passwordHash");
    }

    [Test]
    public void UpdateLastLogin_ShouldSetLastLoginToCurrentTime()
    {
        // Arrange
        var adminUser = new AdminUser(ValidUsername, ValidEmail, ValidPasswordHash);

        // Act
        adminUser.UpdateLastLogin();

        // Assert
        adminUser.LastLogin.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var adminUser = new AdminUser(ValidUsername, ValidEmail, ValidPasswordHash);
        adminUser.Deactivate(); // Erst deaktivieren

        // Act
        adminUser.Activate();

        // Assert
        adminUser.IsActive.Should().BeTrue();
    }

    [Test]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var adminUser = new AdminUser(ValidUsername, ValidEmail, ValidPasswordHash);

        // Act
        adminUser.Deactivate();

        // Assert
        adminUser.IsActive.Should().BeFalse();
    }

    [Test]
    public void UpdatePassword_WithValidPasswordHash_ShouldUpdatePassword()
    {
        // Arrange
        var adminUser = new AdminUser(ValidUsername, ValidEmail, ValidPasswordHash);
        var newPasswordHash = "new_hashed_password_456";

        // Act
        adminUser.UpdatePassword(newPasswordHash);

        // Assert
        adminUser.PasswordHash.Should().Be(newPasswordHash);
    }

    [Test]
    public void UpdatePassword_WithNullPasswordHash_ShouldThrowArgumentNullException()
    {
        // Arrange
        var adminUser = new AdminUser(ValidUsername, ValidEmail, ValidPasswordHash);

        // Act & Assert
        var action = () => adminUser.UpdatePassword(null!);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("newPasswordHash");
    }
}
