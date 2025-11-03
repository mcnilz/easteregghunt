using EasterEggHunt.Domain.Entities;
using NUnit.Framework;

namespace EasterEggHunt.Domain.Tests.Entities;

/// <summary>
/// Unit Tests für Session-Timeout-Verhalten
/// Testet die Timeout-Logik der Session-Entity
/// </summary>
[TestFixture]
public sealed class SessionTimeoutTests
{
    [Test]
    public void Session_WithDefaultExpiration_ShouldExpireAfter30Days()
    {
        // Arrange
        var userId = 1;
        var session = new Session(userId);

        // Act
        var expirationTime = session.ExpiresAt - session.CreatedAt;

        // Assert
        Assert.That(expirationTime.Days, Is.EqualTo(30), "Standard-Expiration sollte 30 Tage sein");
    }

    [Test]
    public void Session_WithCustomExpiration_ShouldExpireAfterSpecifiedDays()
    {
        // Arrange
        var userId = 1;
        var expirationDays = 7;
        var session = new Session(userId, expirationDays);

        // Act
        var expirationTime = session.ExpiresAt - session.CreatedAt;

        // Assert
        Assert.That(expirationTime.Days, Is.EqualTo(expirationDays),
            $"Expiration sollte {expirationDays} Tage sein");
    }

    [Test]
    public void Session_WhenValid_IsValidShouldReturnTrue()
    {
        // Arrange
        var session = new Session(1, 30);

        // Act
        var isValid = session.IsValid();

        // Assert
        Assert.That(isValid, Is.True, "Neue Session sollte gültig sein");
        Assert.That(session.IsActive, Is.True, "Session sollte aktiv sein");
    }

    [Test]
    public void Session_WhenExpired_IsValidShouldReturnFalse()
    {
        // Arrange
        var session = new Session(1, 30);
        // Setze ExpiresAt auf die Vergangenheit
        session.ExpiresAt = DateTime.UtcNow.AddDays(-1);

        // Act
        var isValid = session.IsValid();

        // Assert
        Assert.That(isValid, Is.False, "Abgelaufene Session sollte ungültig sein");
    }

    [Test]
    public void Session_WhenDeactivated_IsValidShouldReturnFalse()
    {
        // Arrange
        var session = new Session(1, 30);
        session.Deactivate();

        // Act
        var isValid = session.IsValid();

        // Assert
        Assert.That(isValid, Is.False, "Deaktivierte Session sollte ungültig sein");
        Assert.That(session.IsActive, Is.False, "Session sollte inaktiv sein");
    }

    [Test]
    public void Session_Extend_ShouldIncreaseExpiration()
    {
        // Arrange
        var session = new Session(1, 30);
        var originalExpiration = session.ExpiresAt;
        var extensionDays = 7;

        // Act
        session.Extend(extensionDays);

        // Assert
        var newExpirationTime = session.ExpiresAt - session.CreatedAt;
        Assert.That(newExpirationTime.Days, Is.GreaterThanOrEqualTo(30 + extensionDays),
            "Expiration sollte um die angegebenen Tage verlängert werden");
        Assert.That(session.ExpiresAt, Is.GreaterThan(originalExpiration),
            "ExpiresAt sollte nach der Verlängerung größer sein");
    }

    [Test]
    public void Session_ExtendWithZeroDays_ShouldNotChangeExpiration()
    {
        // Arrange
        var session = new Session(1, 30);
        var originalExpiration = session.ExpiresAt;

        // Act
        session.Extend(0);

        // Assert
        // Expiration sollte nahezu gleich sein (kleine Zeitunterschiede möglich)
        var timeDifference = (session.ExpiresAt - originalExpiration).TotalSeconds;
        Assert.That(Math.Abs(timeDifference), Is.LessThan(1),
            "Expiration sollte sich nicht ändern wenn 0 Tage verlängert wird");
    }
}
