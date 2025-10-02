using EasterEggHunt.Domain.Entities;
using FluentAssertions;
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
        user.Name.Should().Be(ValidName);
        user.IsActive.Should().BeTrue();
        user.FirstSeen.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        user.LastSeen.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        user.Finds.Should().NotBeNull().And.BeEmpty();
        user.Sessions.Should().NotBeNull().And.BeEmpty();
    }

    [Test]
    public void Constructor_WithNullName_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new User(null!);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("name");
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
        user.LastSeen.Should().BeAfter(originalLastSeen);
        user.LastSeen.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
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
        user.IsActive.Should().BeTrue();
        user.LastSeen.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var user = new User(ValidName);

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
        user.LastSeen.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public void Finds_ShouldBeReadOnlyCollection()
    {
        // Arrange
        var user = new User(ValidName);

        // Act & Assert
        user.Finds.Should().NotBeNull();
        user.Finds.Should().BeAssignableTo<ICollection<Find>>();
    }

    [Test]
    public void Sessions_ShouldBeReadOnlyCollection()
    {
        // Arrange
        var user = new User(ValidName);

        // Act & Assert
        user.Sessions.Should().NotBeNull();
        user.Sessions.Should().BeAssignableTo<ICollection<Session>>();
    }
}
