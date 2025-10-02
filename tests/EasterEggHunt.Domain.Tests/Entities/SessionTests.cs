using EasterEggHunt.Domain.Entities;
using FluentAssertions;
using NUnit.Framework;

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
        session.UserId.Should().Be(ValidUserId);
        session.IsActive.Should().BeTrue();
        session.Data.Should().Be("{}");
        session.Id.Should().NotBeNullOrEmpty();
        session.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        session.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(DefaultExpirationDays), TimeSpan.FromSeconds(5));
    }

    [Test]
    public void Constructor_WithCustomExpirationDays_ShouldSetCorrectExpiration()
    {
        // Arrange
        var customExpirationDays = 7;

        // Act
        var session = new Session(ValidUserId, customExpirationDays);

        // Assert
        session.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(customExpirationDays), TimeSpan.FromSeconds(5));
    }

    [Test]
    public void Constructor_ShouldGenerateUniqueId()
    {
        // Act
        var session1 = new Session(ValidUserId);
        var session2 = new Session(ValidUserId);

        // Assert
        session1.Id.Should().NotBe(session2.Id);
        session1.Id.Should().NotBeNullOrEmpty();
        session2.Id.Should().NotBeNullOrEmpty();
    }

    [Test]
    public void IsValid_WithActiveAndNotExpiredSession_ShouldReturnTrue()
    {
        // Arrange
        var session = new Session(ValidUserId, 30); // 30 Tage Gültigkeit

        // Act
        var isValid = session.IsValid();

        // Assert
        isValid.Should().BeTrue();
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
        isValid.Should().BeFalse();
    }

    [Test]
    public void IsValid_WithExpiredSession_ShouldReturnFalse()
    {
        // Arrange
        var session = new Session(ValidUserId, -1); // Bereits abgelaufen

        // Act
        var isValid = session.IsValid();

        // Assert
        isValid.Should().BeFalse();
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
        session.ExpiresAt.Should().BeAfter(originalExpiration);
        session.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(7 + extensionDays), TimeSpan.FromSeconds(5));
    }

    [Test]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var session = new Session(ValidUserId);

        // Act
        session.Deactivate();

        // Assert
        session.IsActive.Should().BeFalse();
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
        session.Data.Should().Be(newData);
    }

    [Test]
    public void UpdateData_WithNullData_ShouldThrowArgumentNullException()
    {
        // Arrange
        var session = new Session(ValidUserId);

        // Act & Assert
        var action = () => session.UpdateData(null!);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("data");
    }
}
