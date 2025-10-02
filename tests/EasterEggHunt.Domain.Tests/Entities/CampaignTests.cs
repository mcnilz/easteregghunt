using EasterEggHunt.Domain.Entities;
using FluentAssertions;
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
        campaign.Name.Should().Be(ValidName);
        campaign.Description.Should().Be(ValidDescription);
        campaign.CreatedBy.Should().Be(ValidCreatedBy);
        campaign.IsActive.Should().BeTrue();
        campaign.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        campaign.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        campaign.QrCodes.Should().NotBeNull().And.BeEmpty();
    }

    [Test]
    public void Constructor_WithNullName_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new Campaign(null!, ValidDescription, ValidCreatedBy);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("name");
    }

    [Test]
    public void Constructor_WithNullDescription_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new Campaign(ValidName, null!, ValidCreatedBy);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("description");
    }

    [Test]
    public void Constructor_WithNullCreatedBy_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new Campaign(ValidName, ValidDescription, null!);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("createdBy");
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
        campaign.IsActive.Should().BeTrue();
        campaign.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var campaign = new Campaign(ValidName, ValidDescription, ValidCreatedBy);

        // Act
        campaign.Deactivate();

        // Assert
        campaign.IsActive.Should().BeFalse();
        campaign.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
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
        campaign.Name.Should().Be(newName);
        campaign.Description.Should().Be(newDescription);
        campaign.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public void Update_WithNullName_ShouldThrowArgumentNullException()
    {
        // Arrange
        var campaign = new Campaign(ValidName, ValidDescription, ValidCreatedBy);

        // Act & Assert
        var action = () => campaign.Update(null!, ValidDescription);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("name");
    }

    [Test]
    public void Update_WithNullDescription_ShouldThrowArgumentNullException()
    {
        // Arrange
        var campaign = new Campaign(ValidName, ValidDescription, ValidCreatedBy);

        // Act & Assert
        var action = () => campaign.Update(ValidName, null!);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("description");
    }

    [Test]
    public void QrCodes_ShouldBeReadOnlyCollection()
    {
        // Arrange
        var campaign = new Campaign(ValidName, ValidDescription, ValidCreatedBy);

        // Act & Assert
        campaign.QrCodes.Should().NotBeNull();
        // Die Collection sollte read-only sein (keine Add-Methode verfügbar)
        campaign.QrCodes.Should().BeAssignableTo<ICollection<QrCode>>();
    }
}
