using EasterEggHunt.Domain.Entities;
using FluentAssertions;
using NUnit.Framework;

namespace EasterEggHunt.Domain.Tests.Entities;

/// <summary>
/// Tests für die QrCode Entity
/// </summary>
[TestFixture]
public class QrCodeTests
{
    private const int ValidCampaignId = 1;
    private const string ValidTitle = "Versteck unter dem Schreibtisch";
    private const string ValidInternalNote = "QR-Code ist unter dem Schreibtisch von Max versteckt";

    [Test]
    public void Constructor_WithValidParameters_ShouldCreateQrCode()
    {
        // Act
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidInternalNote);

        // Assert
        qrCode.CampaignId.Should().Be(ValidCampaignId);
        qrCode.Title.Should().Be(ValidTitle);
        qrCode.InternalNote.Should().Be(ValidInternalNote);
        qrCode.IsActive.Should().BeTrue();
        qrCode.SortOrder.Should().Be(0);
        qrCode.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        qrCode.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        qrCode.UniqueUrl.Should().NotBeNull();
        qrCode.UniqueUrl.Should().BeOfType<Uri>();
        qrCode.Finds.Should().NotBeNull().And.BeEmpty();
    }

    [Test]
    public void Constructor_WithNullTitle_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new QrCode(ValidCampaignId, null!, ValidInternalNote);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("title");
    }

    [Test]
    public void Constructor_WithNullInternalNote_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new QrCode(ValidCampaignId, ValidTitle, null!);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("internalNote");
    }

    [Test]
    public void Constructor_ShouldGenerateUniqueUrl()
    {
        // Act
        var qrCode1 = new QrCode(ValidCampaignId, ValidTitle, ValidInternalNote);
        var qrCode2 = new QrCode(ValidCampaignId, ValidTitle, ValidInternalNote);

        // Assert
        qrCode1.UniqueUrl.Should().NotBe(qrCode2.UniqueUrl);
        qrCode1.UniqueUrl.ToString().Should().StartWith("https://easteregghunt.local/qr/");
        qrCode2.UniqueUrl.ToString().Should().StartWith("https://easteregghunt.local/qr/");
    }

    [Test]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidInternalNote);
        qrCode.Deactivate(); // Erst deaktivieren

        // Act
        qrCode.Activate();

        // Assert
        qrCode.IsActive.Should().BeTrue();
        qrCode.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidInternalNote);

        // Act
        qrCode.Deactivate();

        // Assert
        qrCode.IsActive.Should().BeFalse();
        qrCode.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public void Update_WithValidParameters_ShouldUpdateProperties()
    {
        // Arrange
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidInternalNote);
        var newTitle = "Neuer Titel";
        var newNote = "Neue interne Notiz";

        // Act
        qrCode.Update(newTitle, newNote);

        // Assert
        qrCode.Title.Should().Be(newTitle);
        qrCode.InternalNote.Should().Be(newNote);
        qrCode.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public void Update_WithNullTitle_ShouldThrowArgumentNullException()
    {
        // Arrange
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidInternalNote);

        // Act & Assert
        var action = () => qrCode.Update(null!, ValidInternalNote);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("title");
    }

    [Test]
    public void Update_WithNullInternalNote_ShouldThrowArgumentNullException()
    {
        // Arrange
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidInternalNote);

        // Act & Assert
        var action = () => qrCode.Update(ValidTitle, null!);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("internalNote");
    }

    [Test]
    public void SetSortOrder_ShouldUpdateSortOrder()
    {
        // Arrange
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidInternalNote);
        var newSortOrder = 5;

        // Act
        qrCode.SetSortOrder(newSortOrder);

        // Assert
        qrCode.SortOrder.Should().Be(newSortOrder);
        qrCode.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public void Finds_ShouldBeReadOnlyCollection()
    {
        // Arrange
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidInternalNote);

        // Act & Assert
        qrCode.Finds.Should().NotBeNull();
        qrCode.Finds.Should().BeAssignableTo<ICollection<Find>>();
    }
}
