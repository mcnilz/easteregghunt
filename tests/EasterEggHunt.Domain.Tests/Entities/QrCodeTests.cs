using EasterEggHunt.Domain.Entities;
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
    private const string ValidDescription = "Ein versteckter QR-Code";
    private const string ValidInternalNotes = "QR-Code ist unter dem Schreibtisch von Max versteckt";

    [Test]
    public void Constructor_WithValidParameters_ShouldCreateQrCode()
    {
        // Act
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidDescription, ValidInternalNotes);

        // Assert
        Assert.That(qrCode.CampaignId, Is.EqualTo(ValidCampaignId));
        Assert.That(qrCode.Title, Is.EqualTo(ValidTitle));
        Assert.That(qrCode.Description, Is.EqualTo(ValidDescription));
        Assert.That(qrCode.InternalNotes, Is.EqualTo(ValidInternalNotes));
        Assert.That(qrCode.IsActive, Is.True);
        Assert.That(qrCode.SortOrder, Is.EqualTo(0));
        Assert.That(qrCode.CreatedAt, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(5)));
        Assert.That(qrCode.UpdatedAt, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(5)));
        Assert.That(qrCode.UniqueUrl, Is.Not.Null);
        Assert.That(qrCode.UniqueUrl, Is.InstanceOf<Uri>());
        Assert.That(qrCode.Finds, Is.Not.Null);
        Assert.That(qrCode.Finds, Is.Empty);
    }

    [Test]
    public void Constructor_WithNullTitle_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new QrCode(ValidCampaignId, null!, ValidDescription, ValidInternalNotes));
        Assert.That(ex.ParamName, Is.EqualTo("title"));
    }

    [Test]
    public void Constructor_WithNullInternalNote_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new QrCode(ValidCampaignId, ValidTitle, ValidDescription, null!));
        Assert.That(ex.ParamName, Is.EqualTo("internalNotes"));
    }

    [Test]
    public void Constructor_ShouldGenerateUniqueUrl()
    {
        // Act
        var qrCode1 = new QrCode(ValidCampaignId, ValidTitle, ValidDescription, ValidInternalNotes);
        var qrCode2 = new QrCode(ValidCampaignId, ValidTitle, ValidDescription, ValidInternalNotes);

        // Assert
        Assert.That(qrCode1.UniqueUrl, Is.Not.EqualTo(qrCode2.UniqueUrl));
        Assert.That(qrCode1.UniqueUrl.ToString(), Does.StartWith("https://easteregghunt.local/qr/"));
        Assert.That(qrCode2.UniqueUrl.ToString(), Does.StartWith("https://easteregghunt.local/qr/"));
    }

    [Test]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidDescription, ValidInternalNotes);
        qrCode.Deactivate(); // Erst deaktivieren

        // Act
        qrCode.Activate();

        // Assert
        Assert.That(qrCode.IsActive, Is.True);
        Assert.That(qrCode.UpdatedAt, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(5)));
    }

    [Test]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidDescription, ValidInternalNotes);

        // Act
        qrCode.Deactivate();

        // Assert
        Assert.That(qrCode.IsActive, Is.False);
        Assert.That(qrCode.UpdatedAt, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(5)));
    }

    [Test]
    public void Update_WithValidParameters_ShouldUpdateProperties()
    {
        // Arrange
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidDescription, ValidInternalNotes);
        var newTitle = "Neuer Titel";
        var newNote = "Neue interne Notiz";

        // Act
        qrCode.Update(newTitle, ValidDescription, newNote);

        // Assert
        Assert.That(qrCode.Title, Is.EqualTo(newTitle));
        Assert.That(qrCode.InternalNotes, Is.EqualTo(newNote));
        Assert.That(qrCode.UpdatedAt, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(5)));
    }

    [Test]
    public void Update_WithNullTitle_ShouldThrowArgumentNullException()
    {
        // Arrange
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidDescription, ValidInternalNotes);

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => qrCode.Update(null!, ValidDescription, ValidInternalNotes));
        Assert.That(ex.ParamName, Is.EqualTo("title"));
    }

    [Test]
    public void Update_WithNullInternalNote_ShouldThrowArgumentNullException()
    {
        // Arrange
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidDescription, ValidInternalNotes);

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => qrCode.Update(ValidTitle, ValidDescription, null!));
        Assert.That(ex.ParamName, Is.EqualTo("internalNotes"));
    }

    [Test]
    public void SetSortOrder_ShouldUpdateSortOrder()
    {
        // Arrange
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidDescription, ValidInternalNotes);
        var newSortOrder = 5;

        // Act
        qrCode.SetSortOrder(newSortOrder);

        // Assert
        Assert.That(qrCode.SortOrder, Is.EqualTo(newSortOrder));
        Assert.That(qrCode.UpdatedAt, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(5)));
    }

    [Test]
    public void Finds_ShouldBeReadOnlyCollection()
    {
        // Arrange
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidDescription, ValidInternalNotes);

        // Act & Assert
        Assert.That(qrCode.Finds, Is.Not.Null);
        Assert.That(qrCode.Finds, Is.InstanceOf<ICollection<Find>>());
    }
}
