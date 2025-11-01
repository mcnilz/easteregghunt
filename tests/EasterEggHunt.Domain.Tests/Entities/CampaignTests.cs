using EasterEggHunt.Domain.Entities;

namespace EasterEggHunt.Domain.Tests.Entities;

/// <summary>
/// Tests für die Campaign Entity
/// </summary>
[TestFixture]
public class CampaignTests
{
    private const string ValidName = "Ostern 2024";
    private const string ValidDescription = "Jährliche Ostereiersuche im Büro";
    private const string ValidCreatedBy = "Admin User";

    [Test]
    public void Constructor_WithValidParameters_ShouldCreateCampaign()
    {
        // Act
        var campaign = new Campaign(ValidName, ValidDescription, ValidCreatedBy);

        // Assert
        Assert.That(campaign.Name, Is.EqualTo(ValidName));
        Assert.That(campaign.Description, Is.EqualTo(ValidDescription));
        Assert.That(campaign.CreatedBy, Is.EqualTo(ValidCreatedBy));
        Assert.That(campaign.IsActive, Is.True);
        Assert.That(campaign.CreatedAt, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(5)));
        Assert.That(campaign.UpdatedAt, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(5)));
        Assert.That(campaign.QrCodes, Is.Not.Null);
        Assert.That(campaign.QrCodes, Is.Empty);
    }

    [Test]
    public void Constructor_WithNullName_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new Campaign(null!, ValidDescription, ValidCreatedBy));
        Assert.That(ex.ParamName, Is.EqualTo("name"));
    }

    [Test]
    public void Constructor_WithNullDescription_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new Campaign(ValidName, null!, ValidCreatedBy));
        Assert.That(ex.ParamName, Is.EqualTo("description"));
    }

    [Test]
    public void Constructor_WithNullCreatedBy_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new Campaign(ValidName, ValidDescription, null!));
        Assert.That(ex.ParamName, Is.EqualTo("createdBy"));
    }

    [Test]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var campaign = new Campaign(ValidName, ValidDescription, ValidCreatedBy);
        campaign.Deactivate(); // Erst deaktivieren

        // Act
        campaign.Activate();

        // Assert
        Assert.That(campaign.IsActive, Is.True);
        Assert.That(campaign.UpdatedAt, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(5)));
    }

    [Test]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var campaign = new Campaign(ValidName, ValidDescription, ValidCreatedBy);

        // Act
        campaign.Deactivate();

        // Assert
        Assert.That(campaign.IsActive, Is.False);
        Assert.That(campaign.UpdatedAt, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(5)));
    }

    [Test]
    public void Update_WithValidParameters_ShouldUpdateProperties()
    {
        // Arrange
        var campaign = new Campaign(ValidName, ValidDescription, ValidCreatedBy);
        var newName = "Ostern 2025";
        var newDescription = "Neue Beschreibung";

        // Act
        campaign.Update(newName, newDescription);

        // Assert
        Assert.That(campaign.Name, Is.EqualTo(newName));
        Assert.That(campaign.Description, Is.EqualTo(newDescription));
        Assert.That(campaign.UpdatedAt, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(5)));
    }

    [Test]
    public void Update_WithNullName_ShouldThrowArgumentNullException()
    {
        // Arrange
        var campaign = new Campaign(ValidName, ValidDescription, ValidCreatedBy);

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => campaign.Update(null!, ValidDescription));
        Assert.That(ex.ParamName, Is.EqualTo("name"));
    }

    [Test]
    public void Update_WithNullDescription_ShouldThrowArgumentNullException()
    {
        // Arrange
        var campaign = new Campaign(ValidName, ValidDescription, ValidCreatedBy);

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => campaign.Update(ValidName, null!));
        Assert.That(ex.ParamName, Is.EqualTo("description"));
    }

    [Test]
    public void QrCodes_ShouldBeReadOnlyCollection()
    {
        // Arrange
        var campaign = new Campaign(ValidName, ValidDescription, ValidCreatedBy);

        // Act & Assert
        Assert.That(campaign.QrCodes, Is.Not.Null);
        // Die Collection sollte read-only sein (keine Add-Methode verfügbar)
        Assert.That(campaign.QrCodes, Is.InstanceOf<ICollection<QrCode>>());
    }

    [Test]
    public void Constructor_WithEmptyStrings_ShouldCreateCampaign()
    {
        // Act
        var campaign = new Campaign(string.Empty, string.Empty, string.Empty);

        // Assert
        Assert.That(campaign.Name, Is.EqualTo(string.Empty));
        Assert.That(campaign.Description, Is.EqualTo(string.Empty));
        Assert.That(campaign.CreatedBy, Is.EqualTo(string.Empty));
        Assert.That(campaign.IsActive, Is.True);
    }

    [Test]
    public void Constructor_WithSpecialCharacters_ShouldCreateCampaign()
    {
        // Arrange
        var specialName = "Kampagne-2024_Test@Ostern";
        var specialDescription = "Beschreibung mit Sonderzeichen: äöüÄÖÜß";
        var specialCreatedBy = "Admin-User_123";

        // Act
        var campaign = new Campaign(specialName, specialDescription, specialCreatedBy);

        // Assert
        Assert.That(campaign.Name, Is.EqualTo(specialName));
        Assert.That(campaign.Description, Is.EqualTo(specialDescription));
        Assert.That(campaign.CreatedBy, Is.EqualTo(specialCreatedBy));
    }

    [Test]
    public void Constructor_WithUnicodeCharacters_ShouldCreateCampaign()
    {
        // Arrange
        var unicodeName = "Kampagne 中文 🎉";
        var unicodeDescription = "Beschreibung avec émojis 🐰🥚";

        // Act
        var campaign = new Campaign(unicodeName, unicodeDescription, ValidCreatedBy);

        // Assert
        Assert.That(campaign.Name, Is.EqualTo(unicodeName));
        Assert.That(campaign.Description, Is.EqualTo(unicodeDescription));
    }

    [Test]
    public void Constructor_CreatedAtShouldBeSetCorrectly()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var campaign = new Campaign(ValidName, ValidDescription, ValidCreatedBy);
        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.That(campaign.CreatedAt, Is.GreaterThanOrEqualTo(beforeCreation));
        Assert.That(campaign.CreatedAt, Is.LessThanOrEqualTo(afterCreation));
    }

    [Test]
    public void Constructor_UpdatedAtShouldMatchCreatedAt()
    {
        // Act
        var campaign = new Campaign(ValidName, ValidDescription, ValidCreatedBy);

        // Assert
        Assert.That(campaign.UpdatedAt, Is.EqualTo(campaign.CreatedAt).Within(TimeSpan.FromSeconds(1)));
    }

    [Test]
    public void Activate_AlreadyActive_ShouldRemainActive()
    {
        // Arrange
        var campaign = new Campaign(ValidName, ValidDescription, ValidCreatedBy);

        // Act
        campaign.Activate();

        // Assert
        Assert.That(campaign.IsActive, Is.True);
    }

    [Test]
    public void Activate_ShouldUpdateUpdatedAt()
    {
        // Arrange
        var campaign = new Campaign(ValidName, ValidDescription, ValidCreatedBy);
        campaign.Deactivate();
        var originalUpdatedAt = campaign.UpdatedAt;

        // Act
        Thread.Sleep(10);
        campaign.Activate();

        // Assert
        Assert.That(campaign.UpdatedAt, Is.GreaterThan(originalUpdatedAt));
    }

    [Test]
    public void Deactivate_AlreadyInactive_ShouldRemainInactive()
    {
        // Arrange
        var campaign = new Campaign(ValidName, ValidDescription, ValidCreatedBy);
        campaign.Deactivate();

        // Act
        campaign.Deactivate();

        // Assert
        Assert.That(campaign.IsActive, Is.False);
    }

    [Test]
    public void Deactivate_ShouldUpdateUpdatedAt()
    {
        // Arrange
        var campaign = new Campaign(ValidName, ValidDescription, ValidCreatedBy);
        var originalUpdatedAt = campaign.UpdatedAt;

        // Act
        Thread.Sleep(10);
        campaign.Deactivate();

        // Assert
        Assert.That(campaign.UpdatedAt, Is.GreaterThan(originalUpdatedAt));
    }

    [Test]
    public void Update_ShouldUpdateNameAndDescription()
    {
        // Arrange
        var campaign = new Campaign(ValidName, ValidDescription, ValidCreatedBy);
        var newName = "Neuer Name";
        var newDescription = "Neue Beschreibung";

        // Act
        campaign.Update(newName, newDescription);

        // Assert
        Assert.That(campaign.Name, Is.EqualTo(newName));
        Assert.That(campaign.Description, Is.EqualTo(newDescription));
    }

    [Test]
    public void Update_ShouldNotAffectCreatedBy()
    {
        // Arrange
        var campaign = new Campaign(ValidName, ValidDescription, ValidCreatedBy);
        var originalCreatedBy = campaign.CreatedBy;

        // Act
        campaign.Update("Neuer Name", "Neue Beschreibung");

        // Assert
        Assert.That(campaign.CreatedBy, Is.EqualTo(originalCreatedBy));
    }

    [Test]
    public void Update_ShouldNotAffectCreatedAt()
    {
        // Arrange
        var campaign = new Campaign(ValidName, ValidDescription, ValidCreatedBy);
        var originalCreatedAt = campaign.CreatedAt;

        // Act
        Thread.Sleep(10);
        campaign.Update("Neuer Name", "Neue Beschreibung");

        // Assert
        Assert.That(campaign.CreatedAt, Is.EqualTo(originalCreatedAt));
    }

    [Test]
    public void Update_ShouldUpdateUpdatedAt()
    {
        // Arrange
        var campaign = new Campaign(ValidName, ValidDescription, ValidCreatedBy);
        var originalUpdatedAt = campaign.UpdatedAt;

        // Act
        Thread.Sleep(10);
        campaign.Update("Neuer Name", "Neue Beschreibung");

        // Assert
        Assert.That(campaign.UpdatedAt, Is.GreaterThan(originalUpdatedAt));
    }

    [Test]
    public void Update_WithEmptyStrings_ShouldUpdateProperties()
    {
        // Arrange
        var campaign = new Campaign(ValidName, ValidDescription, ValidCreatedBy);

        // Act
        campaign.Update(string.Empty, string.Empty);

        // Assert
        Assert.That(campaign.Name, Is.EqualTo(string.Empty));
        Assert.That(campaign.Description, Is.EqualTo(string.Empty));
    }

    [Test]
    public void Update_WithVeryLongStrings_ShouldUpdateProperties()
    {
        // Arrange
        var campaign = new Campaign(ValidName, ValidDescription, ValidCreatedBy);
        var longName = new string('A', 200);
        var longDescription = new string('B', 500);

        // Act
        campaign.Update(longName, longDescription);

        // Assert
        Assert.That(campaign.Name, Is.EqualTo(longName));
        Assert.That(campaign.Description, Is.EqualTo(longDescription));
    }

    [Test]
    public void QrCodes_CollectionShouldNotBeNullAfterConstruction()
    {
        // Arrange & Act
        var campaign = new Campaign(ValidName, ValidDescription, ValidCreatedBy);

        // Assert
        Assert.That(campaign.QrCodes, Is.Not.Null);
        Assert.That(campaign.QrCodes.Count, Is.EqualTo(0));
    }

    [Test]
    public void Constructor_WithSingleCharacterStrings_ShouldCreateCampaign()
    {
        // Act
        var campaign = new Campaign("A", "B", "C");

        // Assert
        Assert.That(campaign.Name, Is.EqualTo("A"));
        Assert.That(campaign.Description, Is.EqualTo("B"));
        Assert.That(campaign.CreatedBy, Is.EqualTo("C"));
        Assert.That(campaign.IsActive, Is.True);
    }
}
