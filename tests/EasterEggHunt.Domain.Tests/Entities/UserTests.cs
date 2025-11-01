using EasterEggHunt.Domain.Entities;

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

    [Test]
    public void Constructor_WithEmptyString_ShouldCreateUser()
    {
        // Act
        var user = new User(string.Empty);

        // Assert
        Assert.That(user.Name, Is.EqualTo(string.Empty));
        Assert.That(user.IsActive, Is.True);
    }

    [Test]
    public void Constructor_WithWhitespaceOnly_ShouldCreateUser()
    {
        // Act
        var user = new User("   ");

        // Assert
        Assert.That(user.Name, Is.EqualTo("   "));
        Assert.That(user.IsActive, Is.True);
    }

    [Test]
    public void Constructor_WithSingleCharacter_ShouldCreateUser()
    {
        // Act
        var user = new User("A");

        // Assert
        Assert.That(user.Name, Is.EqualTo("A"));
        Assert.That(user.IsActive, Is.True);
    }

    [Test]
    public void Constructor_WithSpecialCharacters_ShouldCreateUser()
    {
        // Arrange
        var specialName = "Max-Müller_ÜberÖsterreich@Test";

        // Act
        var user = new User(specialName);

        // Assert
        Assert.That(user.Name, Is.EqualTo(specialName));
        Assert.That(user.IsActive, Is.True);
    }

    [Test]
    public void Constructor_WithUnicodeCharacters_ShouldCreateUser()
    {
        // Arrange
        var unicodeName = "François Müller 中文 🎉";

        // Act
        var user = new User(unicodeName);

        // Assert
        Assert.That(user.Name, Is.EqualTo(unicodeName));
        Assert.That(user.IsActive, Is.True);
    }

    [Test]
    public void Constructor_FirstSeenShouldBeSetCorrectly()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var user = new User(ValidName);
        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.That(user.FirstSeen, Is.GreaterThanOrEqualTo(beforeCreation));
        Assert.That(user.FirstSeen, Is.LessThanOrEqualTo(afterCreation));
    }

    [Test]
    public void Constructor_LastSeenShouldMatchFirstSeen()
    {
        // Act
        var user = new User(ValidName);

        // Assert
        Assert.That(user.LastSeen, Is.EqualTo(user.FirstSeen).Within(TimeSpan.FromSeconds(1)));
    }

    [Test]
    public void UpdateLastSeen_MultipleCalls_ShouldUpdateTimestamp()
    {
        // Arrange
        var user = new User(ValidName);
        var timestamps = new List<DateTime>();

        // Act
        for (int i = 0; i < 3; i++)
        {
            Thread.Sleep(10);
            user.UpdateLastSeen();
            timestamps.Add(user.LastSeen);
        }

        // Assert
        Assert.That(timestamps[0], Is.LessThan(timestamps[1]));
        Assert.That(timestamps[1], Is.LessThan(timestamps[2]));
    }

    [Test]
    public void UpdateLastSeen_ShouldNotAffectFirstSeen()
    {
        // Arrange
        var user = new User(ValidName);
        var originalFirstSeen = user.FirstSeen;

        // Act
        Thread.Sleep(10);
        user.UpdateLastSeen();

        // Assert
        Assert.That(user.FirstSeen, Is.EqualTo(originalFirstSeen));
        Assert.That(user.LastSeen, Is.GreaterThan(originalFirstSeen));
    }

    [Test]
    public void Activate_AlreadyActive_ShouldRemainActive()
    {
        // Arrange
        var user = new User(ValidName);

        // Act
        user.Activate();

        // Assert
        Assert.That(user.IsActive, Is.True);
    }

    [Test]
    public void Activate_MultipleCalls_ShouldUpdateLastSeen()
    {
        // Arrange
        var user = new User(ValidName);
        user.Deactivate();
        var firstLastSeen = user.LastSeen;

        // Act
        Thread.Sleep(10);
        user.Activate();
        var secondLastSeen = user.LastSeen;

        // Assert
        Assert.That(secondLastSeen, Is.GreaterThan(firstLastSeen));
    }

    [Test]
    public void Deactivate_AlreadyInactive_ShouldRemainInactive()
    {
        // Arrange
        var user = new User(ValidName);
        user.Deactivate();

        // Act
        user.Deactivate();

        // Assert
        Assert.That(user.IsActive, Is.False);
    }

    [Test]
    public void Deactivate_MultipleCalls_ShouldUpdateLastSeen()
    {
        // Arrange
        var user = new User(ValidName);
        var firstLastSeen = user.LastSeen;

        // Act
        Thread.Sleep(10);
        user.Deactivate();
        var secondLastSeen = user.LastSeen;

        // Assert
        Assert.That(secondLastSeen, Is.GreaterThan(firstLastSeen));
    }

    [Test]
    public void Finds_CollectionShouldNotBeNullAfterConstruction()
    {
        // Arrange & Act
        var user = new User(ValidName);

        // Assert
        Assert.That(user.Finds, Is.Not.Null);
        Assert.That(user.Finds.Count, Is.EqualTo(0));
    }

    [Test]
    public void Sessions_CollectionShouldNotBeNullAfterConstruction()
    {
        // Arrange & Act
        var user = new User(ValidName);

        // Assert
        Assert.That(user.Sessions, Is.Not.Null);
        Assert.That(user.Sessions.Count, Is.EqualTo(0));
    }

    [Test]
    public void Constructor_WithVeryLongName_ShouldCreateUser()
    {
        // Arrange - Create a name that is 200 characters long
        var longName = new string('A', 200);

        // Act
        var user = new User(longName);

        // Assert
        Assert.That(user.Name, Is.EqualTo(longName));
        Assert.That(user.Name.Length, Is.EqualTo(200));
        Assert.That(user.IsActive, Is.True);
    }

    [Test]
    public void Constructor_WithNameContainingNewlines_ShouldCreateUser()
    {
        // Arrange
        var nameWithNewlines = "Max\nMustermann\r\nTest";

        // Act
        var user = new User(nameWithNewlines);

        // Assert
        Assert.That(user.Name, Is.EqualTo(nameWithNewlines));
        Assert.That(user.IsActive, Is.True);
    }
}
