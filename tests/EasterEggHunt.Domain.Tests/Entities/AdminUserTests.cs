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

    [Test]
    public void Constructor_WithEmptyStrings_ShouldCreateAdminUser()
    {
        // Act
        var adminUser = new AdminUser(string.Empty, string.Empty, string.Empty);

        // Assert
        Assert.That(adminUser.Username, Is.EqualTo(string.Empty));
        Assert.That(adminUser.Email, Is.EqualTo(string.Empty));
        Assert.That(adminUser.PasswordHash, Is.EqualTo(string.Empty));
        Assert.That(adminUser.IsActive, Is.True);
    }

    [Test]
    public void Constructor_WithSpecialCharacters_ShouldCreateAdminUser()
    {
        // Arrange
        var specialUsername = "admin-user_123";
        var specialEmail = "admin.test+123@company-domain.com";
        var specialPasswordHash = "hash_with-special_chars@123";

        // Act
        var adminUser = new AdminUser(specialUsername, specialEmail, specialPasswordHash);

        // Assert
        Assert.That(adminUser.Username, Is.EqualTo(specialUsername));
        Assert.That(adminUser.Email, Is.EqualTo(specialEmail));
        Assert.That(adminUser.PasswordHash, Is.EqualTo(specialPasswordHash));
    }

    [Test]
    public void Constructor_WithUnicodeCharacters_ShouldCreateAdminUser()
    {
        // Arrange
        var unicodeUsername = "admin中文";
        var unicodeEmail = "admin@公司.com";

        // Act
        var adminUser = new AdminUser(unicodeUsername, unicodeEmail, ValidPasswordHash);

        // Assert
        Assert.That(adminUser.Username, Is.EqualTo(unicodeUsername));
        Assert.That(adminUser.Email, Is.EqualTo(unicodeEmail));
    }

    [Test]
    public void Constructor_CreatedAtShouldBeSetCorrectly()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var adminUser = new AdminUser(ValidUsername, ValidEmail, ValidPasswordHash);
        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.That(adminUser.CreatedAt, Is.GreaterThanOrEqualTo(beforeCreation));
        Assert.That(adminUser.CreatedAt, Is.LessThanOrEqualTo(afterCreation));
    }

    [Test]
    public void Constructor_CreatedAtShouldBeUtcTime()
    {
        // Act
        var adminUser = new AdminUser(ValidUsername, ValidEmail, ValidPasswordHash);

        // Assert
        Assert.That(adminUser.CreatedAt.Kind, Is.EqualTo(DateTimeKind.Utc));
    }

    [Test]
    public void Constructor_LastLoginShouldBeNullInitially()
    {
        // Act
        var adminUser = new AdminUser(ValidUsername, ValidEmail, ValidPasswordHash);

        // Assert
        Assert.That(adminUser.LastLogin, Is.Null);
    }

    [Test]
    public void UpdateLastLogin_ShouldSetLastLoginWithCorrectTimestamp()
    {
        // Arrange
        var adminUser = new AdminUser(ValidUsername, ValidEmail, ValidPasswordHash);
        var beforeUpdate = DateTime.UtcNow;

        // Act
        adminUser.UpdateLastLogin();
        var afterUpdate = DateTime.UtcNow;

        // Assert
        Assert.That(adminUser.LastLogin, Is.Not.Null);
        Assert.That(adminUser.LastLogin!.Value, Is.GreaterThanOrEqualTo(beforeUpdate));
        Assert.That(adminUser.LastLogin.Value, Is.LessThanOrEqualTo(afterUpdate));
    }

    [Test]
    public void UpdateLastLogin_ShouldSetLastLoginToUtcTime()
    {
        // Arrange
        var adminUser = new AdminUser(ValidUsername, ValidEmail, ValidPasswordHash);

        // Act
        adminUser.UpdateLastLogin();

        // Assert
        Assert.That(adminUser.LastLogin!.Value.Kind, Is.EqualTo(DateTimeKind.Utc));
    }

    [Test]
    public void UpdateLastLogin_MultipleTimes_ShouldUpdateLastLogin()
    {
        // Arrange
        var adminUser = new AdminUser(ValidUsername, ValidEmail, ValidPasswordHash);
        var timestamps = new List<DateTime>();

        // Act
        for (int i = 0; i < 3; i++)
        {
            Thread.Sleep(10);
            adminUser.UpdateLastLogin();
            timestamps.Add(adminUser.LastLogin!.Value);
        }

        // Assert
        Assert.That(timestamps[0], Is.LessThan(timestamps[1]));
        Assert.That(timestamps[1], Is.LessThan(timestamps[2]));
    }

    [Test]
    public void Activate_AlreadyActive_ShouldRemainActive()
    {
        // Arrange
        var adminUser = new AdminUser(ValidUsername, ValidEmail, ValidPasswordHash);

        // Act
        adminUser.Activate();

        // Assert
        Assert.That(adminUser.IsActive, Is.True);
    }

    [Test]
    public void Deactivate_AlreadyInactive_ShouldRemainInactive()
    {
        // Arrange
        var adminUser = new AdminUser(ValidUsername, ValidEmail, ValidPasswordHash);
        adminUser.Deactivate();

        // Act
        adminUser.Deactivate();

        // Assert
        Assert.That(adminUser.IsActive, Is.False);
    }

    [Test]
    public void UpdatePassword_WithEmptyString_ShouldUpdatePassword()
    {
        // Arrange
        var adminUser = new AdminUser(ValidUsername, ValidEmail, ValidPasswordHash);

        // Act
        adminUser.UpdatePassword(string.Empty);

        // Assert
        Assert.That(adminUser.PasswordHash, Is.EqualTo(string.Empty));
    }

    [Test]
    public void UpdatePassword_WithVeryLongHash_ShouldUpdatePassword()
    {
        // Arrange
        var adminUser = new AdminUser(ValidUsername, ValidEmail, ValidPasswordHash);
        var longHash = new string('A', 500) + "hash";

        // Act
        adminUser.UpdatePassword(longHash);

        // Assert
        Assert.That(adminUser.PasswordHash, Is.EqualTo(longHash));
    }

    [Test]
    public void UpdatePassword_ShouldNotAffectOtherProperties()
    {
        // Arrange
        var adminUser = new AdminUser(ValidUsername, ValidEmail, ValidPasswordHash);
        var originalUsername = adminUser.Username;
        var originalEmail = adminUser.Email;
        var originalIsActive = adminUser.IsActive;

        // Act
        adminUser.UpdatePassword("new_hash");

        // Assert
        Assert.That(adminUser.Username, Is.EqualTo(originalUsername));
        Assert.That(adminUser.Email, Is.EqualTo(originalEmail));
        Assert.That(adminUser.IsActive, Is.EqualTo(originalIsActive));
    }

    [Test]
    public void UpdatePassword_ShouldNotAffectCreatedAt()
    {
        // Arrange
        var adminUser = new AdminUser(ValidUsername, ValidEmail, ValidPasswordHash);
        var originalCreatedAt = adminUser.CreatedAt;

        // Act
        Thread.Sleep(10);
        adminUser.UpdatePassword("new_hash");

        // Assert
        Assert.That(adminUser.CreatedAt, Is.EqualTo(originalCreatedAt));
    }

    [Test]
    public void UpdatePassword_ShouldNotAffectLastLogin()
    {
        // Arrange
        var adminUser = new AdminUser(ValidUsername, ValidEmail, ValidPasswordHash);
        adminUser.UpdateLastLogin();
        var originalLastLogin = adminUser.LastLogin;

        // Act
        adminUser.UpdatePassword("new_hash");

        // Assert
        Assert.That(adminUser.LastLogin, Is.EqualTo(originalLastLogin));
    }

    [Test]
    public void Constructor_WithVeryLongStrings_ShouldCreateAdminUser()
    {
        // Arrange
        var longUsername = new string('A', 200);
        var longEmail = new string('B', 200) + "@company.com";
        var longHash = new string('C', 300);

        // Act
        var adminUser = new AdminUser(longUsername, longEmail, longHash);

        // Assert
        Assert.That(adminUser.Username, Is.EqualTo(longUsername));
        Assert.That(adminUser.Email, Is.EqualTo(longEmail));
        Assert.That(adminUser.PasswordHash, Is.EqualTo(longHash));
    }

    [Test]
    public void Constructor_WithWhitespaceStrings_ShouldCreateAdminUser()
    {
        // Act
        var adminUser = new AdminUser("   ", "   ", "   ");

        // Assert
        Assert.That(adminUser.Username, Is.EqualTo("   "));
        Assert.That(adminUser.Email, Is.EqualTo("   "));
        Assert.That(adminUser.PasswordHash, Is.EqualTo("   "));
    }

    [Test]
    public void UpdateLastLogin_ShouldNotAffectCreatedAt()
    {
        // Arrange
        var adminUser = new AdminUser(ValidUsername, ValidEmail, ValidPasswordHash);
        var originalCreatedAt = adminUser.CreatedAt;

        // Act
        Thread.Sleep(10);
        adminUser.UpdateLastLogin();

        // Assert
        Assert.That(adminUser.CreatedAt, Is.EqualTo(originalCreatedAt));
    }

    #region Parameterless Constructor Tests (Entity Framework)

    [Test]
    public void ParameterlessConstructor_ShouldCreateDefaultAdminUser()
    {
        // Act
        var adminUser = new AdminUser();

        // Assert
        Assert.That(adminUser.Username, Is.EqualTo(string.Empty));
        Assert.That(adminUser.Email, Is.EqualTo(string.Empty));
        Assert.That(adminUser.PasswordHash, Is.EqualTo(string.Empty));
        Assert.That(adminUser.CreatedAt, Is.EqualTo(DateTime.MinValue));
        Assert.That(adminUser.LastLogin, Is.Null);
        Assert.That(adminUser.IsActive, Is.False);
    }

    [Test]
    public void ParameterlessConstructor_ShouldAllowSettingProperties()
    {
        // Arrange
        var adminUser = new AdminUser();

        // Act
        adminUser.Username = "testadmin";
        adminUser.Email = "test@example.com";
        adminUser.PasswordHash = "hashedpassword123";
        adminUser.IsActive = true;
        adminUser.CreatedAt = DateTime.UtcNow;
        adminUser.LastLogin = DateTime.UtcNow;

        // Assert
        Assert.That(adminUser.Username, Is.EqualTo("testadmin"));
        Assert.That(adminUser.Email, Is.EqualTo("test@example.com"));
        Assert.That(adminUser.PasswordHash, Is.EqualTo("hashedpassword123"));
        Assert.That(adminUser.IsActive, Is.True);
        Assert.That(adminUser.CreatedAt, Is.Not.EqualTo(DateTime.MinValue));
        Assert.That(adminUser.LastLogin, Is.Not.Null);
    }

    #endregion
}
