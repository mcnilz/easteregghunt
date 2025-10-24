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
}
