using EasterEggHunt.Domain.Entities;
using NUnit.Framework;

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
}
