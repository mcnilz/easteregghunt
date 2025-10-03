using EasterEggHunt.Domain.Entities;
using NUnit.Framework;

namespace EasterEggHunt.Domain.Tests.Entities;

/// <summary>
/// Tests für die User Entity
/// </summary>
[TestFixture]
public class UserTests
{
    private const string ValidName = "Max Mustermann";

    [Test]
    public void Constructor_WithValidName_ShouldCreateUser()
    {
        // Act
        var user = new User(ValidName);

        // Assert
        Assert.That(user.Name, Is.EqualTo(ValidName));
        Assert.That(user.IsActive, Is.True);
        Assert.That(user.FirstSeen, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(5)));
        Assert.That(user.LastSeen, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(5)));
        Assert.That(user.Finds, Is.Not.Null);
        Assert.That(user.Finds, Is.Empty);
        Assert.That(user.Sessions, Is.Not.Null);
        Assert.That(user.Sessions, Is.Empty);
    }

    [Test]
    public void Constructor_WithNullName_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new User(null!));
        Assert.That(ex.ParamName, Is.EqualTo("name"));
    }

    [Test]
    public void UpdateLastSeen_ShouldUpdateLastSeenTimestamp()
    {
        // Arrange
        var user = new User(ValidName);
        var originalLastSeen = user.LastSeen;

        // Warten, damit sich der Zeitstempel ändert
        Thread.Sleep(10);

        // Act
        user.UpdateLastSeen();

        // Assert
        Assert.That(user.LastSeen, Is.GreaterThan(originalLastSeen));
        Assert.That(user.LastSeen, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(5)));
    }

    [Test]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var user = new User(ValidName);
        user.Deactivate(); // Erst deaktivieren

        // Act
        user.Activate();

        // Assert
        Assert.That(user.IsActive, Is.True);
        Assert.That(user.LastSeen, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(5)));
    }

    [Test]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var user = new User(ValidName);

        // Act
        user.Deactivate();

        // Assert
        Assert.That(user.IsActive, Is.False);
        Assert.That(user.LastSeen, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(5)));
    }

    [Test]
    public void Finds_ShouldBeReadOnlyCollection()
    {
        // Arrange
        var user = new User(ValidName);

        // Act & Assert
        Assert.That(user.Finds, Is.Not.Null);
        Assert.That(user.Finds, Is.InstanceOf<ICollection<Find>>());
    }

    [Test]
    public void Sessions_ShouldBeReadOnlyCollection()
    {
        // Arrange
        var user = new User(ValidName);

        // Act & Assert
        Assert.That(user.Sessions, Is.Not.Null);
        Assert.That(user.Sessions, Is.InstanceOf<ICollection<Session>>());
    }
}
