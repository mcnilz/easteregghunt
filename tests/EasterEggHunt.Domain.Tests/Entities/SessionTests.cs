using EasterEggHunt.Domain.Entities;

namespace EasterEggHunt.Domain.Tests.Entities;

/// <summary>
/// Tests für die Session Entity
/// </summary>
[TestFixture]
public class SessionTests
{
    private const int ValidUserId = 1;
    private const int DefaultExpirationDays = 30;

    [Test]
    public void Constructor_WithValidUserId_ShouldCreateSession()
    {
        // Act
        var session = new Session(ValidUserId);

        // Assert
        Assert.That(session.UserId, Is.EqualTo(ValidUserId));
        Assert.That(session.IsActive, Is.True);
        Assert.That(session.Data, Is.EqualTo("{}"));
        Assert.That(session.Id, Is.Not.Null.And.Not.Empty);
        Assert.That(session.CreatedAt, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(5)));
        Assert.That(session.ExpiresAt, Is.EqualTo(DateTime.UtcNow.AddDays(DefaultExpirationDays)).Within(TimeSpan.FromSeconds(5)));
    }

    [Test]
    public void Constructor_WithCustomExpirationDays_ShouldSetCorrectExpiration()
    {
        // Arrange
        var customExpirationDays = 7;

        // Act
        var session = new Session(ValidUserId, customExpirationDays);

        // Assert
        Assert.That(session.ExpiresAt, Is.EqualTo(DateTime.UtcNow.AddDays(customExpirationDays)).Within(TimeSpan.FromSeconds(5)));
    }

    [Test]
    public void Constructor_ShouldGenerateUniqueId()
    {
        // Act
        var session1 = new Session(ValidUserId);
        var session2 = new Session(ValidUserId);

        // Assert
        Assert.That(session1.Id, Is.Not.EqualTo(session2.Id));
        Assert.That(session1.Id, Is.Not.Null.And.Not.Empty);
        Assert.That(session2.Id, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void IsValid_WithActiveAndNotExpiredSession_ShouldReturnTrue()
    {
        // Arrange
        var session = new Session(ValidUserId, 30); // 30 Tage Gültigkeit

        // Act
        var isValid = session.IsValid();

        // Assert
        Assert.That(isValid, Is.True);
    }

    [Test]
    public void IsValid_WithInactiveSession_ShouldReturnFalse()
    {
        // Arrange
        var session = new Session(ValidUserId);
        session.Deactivate();

        // Act
        var isValid = session.IsValid();

        // Assert
        Assert.That(isValid, Is.False);
    }

    [Test]
    public void IsValid_WithExpiredSession_ShouldReturnFalse()
    {
        // Arrange
        var session = new Session(ValidUserId, -1); // Bereits abgelaufen

        // Act
        var isValid = session.IsValid();

        // Assert
        Assert.That(isValid, Is.False);
    }

    [Test]
    public void Extend_ShouldExtendExpirationDate()
    {
        // Arrange
        var session = new Session(ValidUserId, 7);
        var originalExpiration = session.ExpiresAt;
        var extensionDays = 14;

        // Act
        session.Extend(extensionDays);

        // Assert
        Assert.That(session.ExpiresAt, Is.GreaterThan(originalExpiration));
        Assert.That(session.ExpiresAt, Is.EqualTo(DateTime.UtcNow.AddDays(7 + extensionDays)).Within(TimeSpan.FromSeconds(5)));
    }

    [Test]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var session = new Session(ValidUserId);

        // Act
        session.Deactivate();

        // Assert
        Assert.That(session.IsActive, Is.False);
    }

    [Test]
    public void UpdateData_WithValidData_ShouldUpdateData()
    {
        // Arrange
        var session = new Session(ValidUserId);
        var newData = "{\"theme\": \"dark\", \"language\": \"de\"}";

        // Act
        session.UpdateData(newData);

        // Assert
        Assert.That(session.Data, Is.EqualTo(newData));
    }

    [Test]
    public void UpdateData_WithNullData_ShouldThrowArgumentNullException()
    {
        // Arrange
        var session = new Session(ValidUserId);

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => session.UpdateData(null!));
        Assert.That(ex.ParamName, Is.EqualTo("data"));
    }

    [Test]
    public void Constructor_WithZeroUserId_ShouldCreateSession()
    {
        // Act
        var session = new Session(0);

        // Assert
        Assert.That(session.UserId, Is.EqualTo(0));
        Assert.That(session.IsActive, Is.True);
    }

    [Test]
    public void Constructor_WithNegativeUserId_ShouldCreateSession()
    {
        // Act
        var session = new Session(-1);

        // Assert
        Assert.That(session.UserId, Is.EqualTo(-1));
        Assert.That(session.IsActive, Is.True);
    }

    [Test]
    public void Constructor_WithZeroExpirationDays_ShouldCreateSession()
    {
        // Act
        var session = new Session(ValidUserId, 0);

        // Assert
        Assert.That(session.ExpiresAt, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(5)));
    }

    [Test]
    public void Constructor_WithNegativeExpirationDays_ShouldCreateExpiredSession()
    {
        // Act
        var session = new Session(ValidUserId, -1);

        // Assert
        Assert.That(session.ExpiresAt, Is.LessThan(DateTime.UtcNow));
        Assert.That(session.IsValid(), Is.False);
    }

    [Test]
    public void Constructor_WithLargeExpirationDays_ShouldCreateSession()
    {
        // Act
        var session = new Session(ValidUserId, 365);

        // Assert
        Assert.That(session.ExpiresAt, Is.EqualTo(DateTime.UtcNow.AddDays(365)).Within(TimeSpan.FromSeconds(5)));
    }

    [Test]
    public void Constructor_CreatedAtShouldBeSetCorrectly()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var session = new Session(ValidUserId);
        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.That(session.CreatedAt, Is.GreaterThanOrEqualTo(beforeCreation));
        Assert.That(session.CreatedAt, Is.LessThanOrEqualTo(afterCreation));
    }

    [Test]
    public void Constructor_CreatedAtShouldBeUtcTime()
    {
        // Act
        var session = new Session(ValidUserId);

        // Assert
        Assert.That(session.CreatedAt.Kind, Is.EqualTo(DateTimeKind.Utc));
        Assert.That(session.ExpiresAt.Kind, Is.EqualTo(DateTimeKind.Utc));
    }

    [Test]
    public void Constructor_DataShouldBeEmptyJsonObject()
    {
        // Act
        var session = new Session(ValidUserId);

        // Assert
        Assert.That(session.Data, Is.EqualTo("{}"));
    }

    [Test]
    public void Constructor_IdShouldBeGuid()
    {
        // Act
        var session = new Session(ValidUserId);

        // Assert
        Assert.That(Guid.TryParse(session.Id, out _), Is.True);
    }

    [Test]
    public void IsValid_WithActiveAndExpiredSession_ShouldReturnFalse()
    {
        // Arrange - Session die bereits abgelaufen ist
        var session = new Session(ValidUserId, -1);

        // Act
        var isValid = session.IsValid();

        // Assert
        Assert.That(isValid, Is.False);
    }

    [Test]
    public void IsValid_WithInactiveAndNotExpiredSession_ShouldReturnFalse()
    {
        // Arrange
        var session = new Session(ValidUserId, 30);
        session.Deactivate();

        // Act
        var isValid = session.IsValid();

        // Assert
        Assert.That(isValid, Is.False);
    }

    [Test]
    public void Extend_WithZeroDays_ShouldNotChangeExpiration()
    {
        // Arrange
        var session = new Session(ValidUserId, 7);
        var originalExpiration = session.ExpiresAt;

        // Act
        session.Extend(0);

        // Assert
        Assert.That(session.ExpiresAt, Is.EqualTo(originalExpiration).Within(TimeSpan.FromSeconds(5)));
    }

    [Test]
    public void Extend_WithNegativeDays_ShouldShortenExpiration()
    {
        // Arrange
        var session = new Session(ValidUserId, 30);
        var originalExpiration = session.ExpiresAt;

        // Act
        session.Extend(-10);

        // Assert
        Assert.That(session.ExpiresAt, Is.EqualTo(originalExpiration.AddDays(-10)).Within(TimeSpan.FromSeconds(5)));
    }

    [Test]
    public void Extend_MultipleTimes_ShouldAccumulateDays()
    {
        // Arrange
        var session = new Session(ValidUserId, 7);

        // Act
        session.Extend(5);
        session.Extend(3);

        // Assert
        Assert.That(session.ExpiresAt, Is.EqualTo(DateTime.UtcNow.AddDays(7 + 5 + 3)).Within(TimeSpan.FromSeconds(5)));
    }

    [Test]
    public void Deactivate_AlreadyInactive_ShouldRemainInactive()
    {
        // Arrange
        var session = new Session(ValidUserId);
        session.Deactivate();

        // Act
        session.Deactivate();

        // Assert
        Assert.That(session.IsActive, Is.False);
    }

    [Test]
    public void Deactivate_ShouldNotAffectExpiration()
    {
        // Arrange
        var session = new Session(ValidUserId, 30);
        var originalExpiration = session.ExpiresAt;

        // Act
        session.Deactivate();

        // Assert
        Assert.That(session.ExpiresAt, Is.EqualTo(originalExpiration));
    }

    [Test]
    public void UpdateData_WithEmptyString_ShouldUpdateData()
    {
        // Arrange
        var session = new Session(ValidUserId);

        // Act
        session.UpdateData(string.Empty);

        // Assert
        Assert.That(session.Data, Is.EqualTo(string.Empty));
    }

    [Test]
    public void UpdateData_WithValidJson_ShouldUpdateData()
    {
        // Arrange
        var session = new Session(ValidUserId);
        var jsonData = "{\"key\":\"value\",\"number\":123}";

        // Act
        session.UpdateData(jsonData);

        // Assert
        Assert.That(session.Data, Is.EqualTo(jsonData));
    }

    [Test]
    public void UpdateData_WithInvalidJsonString_ShouldUpdateData()
    {
        // Arrange - Domain Entity validiert nicht JSON, das sollte Service-Ebene tun
        var session = new Session(ValidUserId);
        var invalidJson = "not valid json";

        // Act
        session.UpdateData(invalidJson);

        // Assert
        Assert.That(session.Data, Is.EqualTo(invalidJson));
    }

    [Test]
    public void UpdateData_WithVeryLongJson_ShouldUpdateData()
    {
        // Arrange
        var session = new Session(ValidUserId);
        var longJson = "{\"data\":\"" + new string('A', 1000) + "\"}";

        // Act
        session.UpdateData(longJson);

        // Assert
        Assert.That(session.Data, Is.EqualTo(longJson));
    }

    [Test]
    public void UpdateData_MultipleTimes_ShouldUpdateData()
    {
        // Arrange
        var session = new Session(ValidUserId);

        // Act
        session.UpdateData("{\"first\":1}");
        session.UpdateData("{\"second\":2}");

        // Assert
        Assert.That(session.Data, Is.EqualTo("{\"second\":2}"));
    }

    [Test]
    public void Extend_WithMaxIntValue_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var session = new Session(ValidUserId, 30);

        // Act & Assert - int.MaxValue Tage führt zu DateTime Overflow
        Assert.Throws<ArgumentOutOfRangeException>(() => session.Extend(int.MaxValue));
    }

    [Test]
    public void Extend_WithLargeButValidValue_ShouldExtendExpiration()
    {
        // Arrange
        var session = new Session(ValidUserId, 30);
        var originalExpiration = session.ExpiresAt;

        // Act - 10000 Tage ist immer noch ein gültiges DateTime
        session.Extend(10000);

        // Assert
        Assert.That(session.ExpiresAt, Is.GreaterThan(originalExpiration));
    }

    [Test]
    public void Constructor_WithMaxIntUserId_ShouldCreateSession()
    {
        // Act
        var session = new Session(int.MaxValue);

        // Assert
        Assert.That(session.UserId, Is.EqualTo(int.MaxValue));
        Assert.That(session.IsActive, Is.True);
    }

    [Test]
    public void Constructor_WithMinIntUserId_ShouldCreateSession()
    {
        // Act
        var session = new Session(int.MinValue);

        // Assert
        Assert.That(session.UserId, Is.EqualTo(int.MinValue));
        Assert.That(session.IsActive, Is.True);
    }

    #region Parameterless Constructor Tests (Entity Framework)

    [Test]
    public void ParameterlessConstructor_ShouldCreateDefaultSession()
    {
        // Act
        var session = new Session();

        // Assert
        Assert.That(session.Id, Is.EqualTo(string.Empty));
        Assert.That(session.UserId, Is.EqualTo(0));
        Assert.That(session.CreatedAt, Is.EqualTo(DateTime.MinValue));
        Assert.That(session.ExpiresAt, Is.EqualTo(DateTime.MinValue));
        Assert.That(session.Data, Is.EqualTo(string.Empty));
        Assert.That(session.IsActive, Is.False);
    }

    [Test]
    public void ParameterlessConstructor_ShouldAllowSettingProperties()
    {
        // Arrange
        var session = new Session();

        // Act
        session.Id = "test-session-id";
        session.UserId = 123;
        session.CreatedAt = DateTime.UtcNow;
        session.ExpiresAt = DateTime.UtcNow.AddDays(30);
        session.Data = "{\"key\":\"value\"}";
        session.IsActive = true;

        // Assert
        Assert.That(session.Id, Is.EqualTo("test-session-id"));
        Assert.That(session.UserId, Is.EqualTo(123));
        Assert.That(session.Data, Is.EqualTo("{\"key\":\"value\"}"));
        Assert.That(session.IsActive, Is.True);
        Assert.That(session.CreatedAt, Is.Not.EqualTo(DateTime.MinValue));
        Assert.That(session.ExpiresAt, Is.Not.EqualTo(DateTime.MinValue));
    }

    #endregion
}
