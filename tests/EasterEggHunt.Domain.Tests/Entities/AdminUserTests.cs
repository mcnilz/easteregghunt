using EasterEggHunt.Domain.Entities;

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
        Assert.That(adminUser.Username, Is.EqualTo(ValidUsername));
        Assert.That(adminUser.Email, Is.EqualTo(ValidEmail));
        Assert.That(adminUser.PasswordHash, Is.EqualTo(ValidPasswordHash));
        Assert.That(adminUser.IsActive, Is.True);
        Assert.That(adminUser.LastLogin, Is.Null);
        Assert.That(adminUser.CreatedAt, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(5)));
    }

    [Test]
    public void Constructor_WithNullUsername_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new AdminUser(null!, ValidEmail, ValidPasswordHash));
        Assert.That(ex.ParamName, Is.EqualTo("username"));
    }

    [Test]
    public void Constructor_WithNullEmail_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new AdminUser(ValidUsername, null!, ValidPasswordHash));
        Assert.That(ex.ParamName, Is.EqualTo("email"));
    }

    [Test]
    public void Constructor_WithNullPasswordHash_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new AdminUser(ValidUsername, ValidEmail, null!));
        Assert.That(ex.ParamName, Is.EqualTo("passwordHash"));
    }

    [Test]
    public void UpdateLastLogin_ShouldSetLastLoginToCurrentTime()
    {
        // Arrange
        var adminUser = new AdminUser(ValidUsername, ValidEmail, ValidPasswordHash);

        // Act
        adminUser.UpdateLastLogin();

        // Assert
        Assert.That(adminUser.LastLogin, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(5)));
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
        Assert.That(adminUser.IsActive, Is.True);
    }

    [Test]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var adminUser = new AdminUser(ValidUsername, ValidEmail, ValidPasswordHash);

        // Act
        adminUser.Deactivate();

        // Assert
        Assert.That(adminUser.IsActive, Is.False);
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
        Assert.That(adminUser.PasswordHash, Is.EqualTo(newPasswordHash));
    }

    [Test]
    public void UpdatePassword_WithNullPasswordHash_ShouldThrowArgumentNullException()
    {
        // Arrange
        var adminUser = new AdminUser(ValidUsername, ValidEmail, ValidPasswordHash);

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => adminUser.UpdatePassword(null!));
        Assert.That(ex.ParamName, Is.EqualTo("newPasswordHash"));
    }
}
