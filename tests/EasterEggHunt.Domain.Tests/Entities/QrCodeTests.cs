using EasterEggHunt.Domain.Entities;

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
        Assert.That(qrCode.Code, Is.Not.Null);
        Assert.That(qrCode.Code, Is.Not.Empty);
        Assert.That(qrCode.Code.Length, Is.EqualTo(12));
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
    public void Constructor_ShouldGenerateUniqueCode()
    {
        // Act
        var qrCode1 = new QrCode(ValidCampaignId, ValidTitle, ValidDescription, ValidInternalNotes);
        var qrCode2 = new QrCode(ValidCampaignId, ValidTitle, ValidDescription, ValidInternalNotes);

        // Assert
        Assert.That(qrCode1.Code, Is.Not.EqualTo(qrCode2.Code));
        Assert.That(qrCode1.Code.Length, Is.EqualTo(12));
        Assert.That(qrCode2.Code.Length, Is.EqualTo(12));
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

    [Test]
    public void Constructor_WithEmptyStrings_ShouldCreateQrCode()
    {
        // Act
        var qrCode = new QrCode(ValidCampaignId, string.Empty, string.Empty, string.Empty);

        // Assert
        Assert.That(qrCode.Title, Is.EqualTo(string.Empty));
        Assert.That(qrCode.Description, Is.EqualTo(string.Empty));
        Assert.That(qrCode.InternalNotes, Is.EqualTo(string.Empty));
        Assert.That(qrCode.IsActive, Is.True);
        Assert.That(qrCode.Code, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void Constructor_WithSpecialCharacters_ShouldCreateQrCode()
    {
        // Arrange
        var specialTitle = "QR-Code_Test@Ostern-2024";
        var specialDescription = "Beschreibung mit Sonderzeichen: äöüÄÖÜß";
        var specialNotes = "Notizen: Test-123_456";

        // Act
        var qrCode = new QrCode(ValidCampaignId, specialTitle, specialDescription, specialNotes);

        // Assert
        Assert.That(qrCode.Title, Is.EqualTo(specialTitle));
        Assert.That(qrCode.Description, Is.EqualTo(specialDescription));
        Assert.That(qrCode.InternalNotes, Is.EqualTo(specialNotes));
    }

    [Test]
    public void Constructor_WithUnicodeCharacters_ShouldCreateQrCode()
    {
        // Arrange
        var unicodeTitle = "QR-Code 中文 🎉";
        var unicodeDescription = "Beschreibung avec émojis 🐰🥚";

        // Act
        var qrCode = new QrCode(ValidCampaignId, unicodeTitle, unicodeDescription, ValidInternalNotes);

        // Assert
        Assert.That(qrCode.Title, Is.EqualTo(unicodeTitle));
        Assert.That(qrCode.Description, Is.EqualTo(unicodeDescription));
    }

    [Test]
    public void Constructor_WithZeroCampaignId_ShouldCreateQrCode()
    {
        // Act
        var qrCode = new QrCode(0, ValidTitle, ValidDescription, ValidInternalNotes);

        // Assert
        Assert.That(qrCode.CampaignId, Is.EqualTo(0));
        Assert.That(qrCode.IsActive, Is.True);
    }

    [Test]
    public void Constructor_WithNegativeCampaignId_ShouldCreateQrCode()
    {
        // Act
        var qrCode = new QrCode(-1, ValidTitle, ValidDescription, ValidInternalNotes);

        // Assert
        Assert.That(qrCode.CampaignId, Is.EqualTo(-1));
        Assert.That(qrCode.IsActive, Is.True);
    }

    [Test]
    public void Constructor_CreatedAtShouldBeSetCorrectly()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidDescription, ValidInternalNotes);
        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.That(qrCode.CreatedAt, Is.GreaterThanOrEqualTo(beforeCreation));
        Assert.That(qrCode.CreatedAt, Is.LessThanOrEqualTo(afterCreation));
    }

    [Test]
    public void Constructor_UpdatedAtShouldMatchCreatedAt()
    {
        // Act
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidDescription, ValidInternalNotes);

        // Assert
        Assert.That(qrCode.UpdatedAt, Is.EqualTo(qrCode.CreatedAt).Within(TimeSpan.FromSeconds(1)));
    }

    [Test]
    public void Constructor_SortOrderShouldBeZero()
    {
        // Act
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidDescription, ValidInternalNotes);

        // Assert
        Assert.That(qrCode.SortOrder, Is.EqualTo(0));
    }

    [Test]
    public void Constructor_CodeShouldBeGenerated()
    {
        // Act
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidDescription, ValidInternalNotes);

        // Assert
        Assert.That(qrCode.Code, Is.Not.Null);
        Assert.That(qrCode.Code, Is.Not.Empty);
        Assert.That(qrCode.Code.Length, Is.EqualTo(12));
    }

    [Test]
    public void Constructor_CodeShouldContainOnlyHexCharacters()
    {
        // Act
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidDescription, ValidInternalNotes);

        // Assert
        Assert.That(qrCode.Code, Does.Match(@"^[0-9a-f]{12}$"));
    }

    [Test]
    public void Activate_AlreadyActive_ShouldRemainActive()
    {
        // Arrange
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidDescription, ValidInternalNotes);

        // Act
        qrCode.Activate();

        // Assert
        Assert.That(qrCode.IsActive, Is.True);
    }

    [Test]
    public void Activate_ShouldUpdateUpdatedAt()
    {
        // Arrange
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidDescription, ValidInternalNotes);
        qrCode.Deactivate();
        var originalUpdatedAt = qrCode.UpdatedAt;

        // Act
        Thread.Sleep(10);
        qrCode.Activate();

        // Assert
        Assert.That(qrCode.UpdatedAt, Is.GreaterThan(originalUpdatedAt));
    }

    [Test]
    public void Deactivate_AlreadyInactive_ShouldRemainInactive()
    {
        // Arrange
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidDescription, ValidInternalNotes);
        qrCode.Deactivate();

        // Act
        qrCode.Deactivate();

        // Assert
        Assert.That(qrCode.IsActive, Is.False);
    }

    [Test]
    public void Deactivate_ShouldUpdateUpdatedAt()
    {
        // Arrange
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidDescription, ValidInternalNotes);
        var originalUpdatedAt = qrCode.UpdatedAt;

        // Act
        Thread.Sleep(10);
        qrCode.Deactivate();

        // Assert
        Assert.That(qrCode.UpdatedAt, Is.GreaterThan(originalUpdatedAt));
    }

    [Test]
    public void Update_ShouldUpdateAllProperties()
    {
        // Arrange
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidDescription, ValidInternalNotes);
        var newTitle = "Neuer Titel";
        var newDescription = "Neue Beschreibung";
        var newNotes = "Neue Notizen";

        // Act
        qrCode.Update(newTitle, newDescription, newNotes);

        // Assert
        Assert.That(qrCode.Title, Is.EqualTo(newTitle));
        Assert.That(qrCode.Description, Is.EqualTo(newDescription));
        Assert.That(qrCode.InternalNotes, Is.EqualTo(newNotes));
    }

    [Test]
    public void Update_ShouldNotAffectCampaignId()
    {
        // Arrange
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidDescription, ValidInternalNotes);
        var originalCampaignId = qrCode.CampaignId;

        // Act
        qrCode.Update("Neuer Titel", "Neue Beschreibung", "Neue Notizen");

        // Assert
        Assert.That(qrCode.CampaignId, Is.EqualTo(originalCampaignId));
    }

    [Test]
    public void Update_ShouldNotAffectCreatedAt()
    {
        // Arrange
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidDescription, ValidInternalNotes);
        var originalCreatedAt = qrCode.CreatedAt;

        // Act
        Thread.Sleep(10);
        qrCode.Update("Neuer Titel", "Neue Beschreibung", "Neue Notizen");

        // Assert
        Assert.That(qrCode.CreatedAt, Is.EqualTo(originalCreatedAt));
    }

    [Test]
    public void Update_ShouldNotAffectCode()
    {
        // Arrange
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidDescription, ValidInternalNotes);
        var originalCode = qrCode.Code;

        // Act
        qrCode.Update("Neuer Titel", "Neue Beschreibung", "Neue Notizen");

        // Assert
        Assert.That(qrCode.Code, Is.EqualTo(originalCode));
    }

    [Test]
    public void Update_ShouldUpdateUpdatedAt()
    {
        // Arrange
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidDescription, ValidInternalNotes);
        var originalUpdatedAt = qrCode.UpdatedAt;

        // Act
        Thread.Sleep(10);
        qrCode.Update("Neuer Titel", "Neue Beschreibung", "Neue Notizen");

        // Assert
        Assert.That(qrCode.UpdatedAt, Is.GreaterThan(originalUpdatedAt));
    }

    [Test]
    public void Update_WithEmptyStrings_ShouldUpdateProperties()
    {
        // Arrange
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidDescription, ValidInternalNotes);

        // Act
        qrCode.Update(string.Empty, string.Empty, string.Empty);

        // Assert
        Assert.That(qrCode.Title, Is.EqualTo(string.Empty));
        Assert.That(qrCode.Description, Is.EqualTo(string.Empty));
        Assert.That(qrCode.InternalNotes, Is.EqualTo(string.Empty));
    }

    [Test]
    public void Update_WithVeryLongStrings_ShouldUpdateProperties()
    {
        // Arrange
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidDescription, ValidInternalNotes);
        var longTitle = new string('A', 200);
        var longDescription = new string('B', 500);
        var longNotes = new string('C', 1000);

        // Act
        qrCode.Update(longTitle, longDescription, longNotes);

        // Assert
        Assert.That(qrCode.Title, Is.EqualTo(longTitle));
        Assert.That(qrCode.Description, Is.EqualTo(longDescription));
        Assert.That(qrCode.InternalNotes, Is.EqualTo(longNotes));
    }

    #region Parameterless Constructor Tests (Entity Framework)

    [Test]
    public void ParameterlessConstructor_ShouldCreateDefaultQrCode()
    {
        // Act
        var qrCode = new QrCode();

        // Assert
        Assert.That(qrCode.Title, Is.EqualTo(string.Empty));
        Assert.That(qrCode.Description, Is.EqualTo(string.Empty));
        Assert.That(qrCode.InternalNotes, Is.EqualTo(string.Empty));
        Assert.That(qrCode.Code, Is.EqualTo(string.Empty));
        Assert.That(qrCode.CreatedAt, Is.EqualTo(DateTime.MinValue));
        Assert.That(qrCode.UpdatedAt, Is.EqualTo(DateTime.MinValue));
        Assert.That(qrCode.IsActive, Is.False);
        Assert.That(qrCode.SortOrder, Is.EqualTo(0));
        Assert.That(qrCode.Finds, Is.Not.Null);
    }

    [Test]
    public void ParameterlessConstructor_ShouldAllowSettingProperties()
    {
        // Arrange
        var qrCode = new QrCode();

        // Act
        qrCode.Title = "Test Title";
        qrCode.Description = "Test Description";
        qrCode.InternalNotes = "Test Notes";
        qrCode.Code = "TESTCODE123";
        qrCode.IsActive = true;
        qrCode.SortOrder = 5;
        qrCode.CreatedAt = DateTime.UtcNow;
        qrCode.UpdatedAt = DateTime.UtcNow;

        // Assert
        Assert.That(qrCode.Title, Is.EqualTo("Test Title"));
        Assert.That(qrCode.Description, Is.EqualTo("Test Description"));
        Assert.That(qrCode.InternalNotes, Is.EqualTo("Test Notes"));
        Assert.That(qrCode.Code, Is.EqualTo("TESTCODE123"));
        Assert.That(qrCode.IsActive, Is.True);
        Assert.That(qrCode.SortOrder, Is.EqualTo(5));
        Assert.That(qrCode.CreatedAt, Is.Not.EqualTo(DateTime.MinValue));
        Assert.That(qrCode.UpdatedAt, Is.Not.EqualTo(DateTime.MinValue));
    }

    #endregion

    [Test]
    public void SetSortOrder_WithNegativeValue_ShouldUpdateSortOrder()
    {
        // Arrange
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidDescription, ValidInternalNotes);

        // Act
        qrCode.SetSortOrder(-5);

        // Assert
        Assert.That(qrCode.SortOrder, Is.EqualTo(-5));
    }

    [Test]
    public void SetSortOrder_WithZero_ShouldUpdateSortOrder()
    {
        // Arrange
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidDescription, ValidInternalNotes);
        qrCode.SetSortOrder(10);

        // Act
        qrCode.SetSortOrder(0);

        // Assert
        Assert.That(qrCode.SortOrder, Is.EqualTo(0));
    }

    [Test]
    public void SetSortOrder_WithLargeValue_ShouldUpdateSortOrder()
    {
        // Arrange
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidDescription, ValidInternalNotes);

        // Act
        qrCode.SetSortOrder(int.MaxValue);

        // Assert
        Assert.That(qrCode.SortOrder, Is.EqualTo(int.MaxValue));
    }

    [Test]
    public void SetSortOrder_ShouldUpdateUpdatedAt()
    {
        // Arrange
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidDescription, ValidInternalNotes);
        var originalUpdatedAt = qrCode.UpdatedAt;

        // Act
        Thread.Sleep(10);
        qrCode.SetSortOrder(5);

        // Assert
        Assert.That(qrCode.UpdatedAt, Is.GreaterThan(originalUpdatedAt));
    }

    [Test]
    public void SetSortOrder_ShouldNotAffectOtherProperties()
    {
        // Arrange
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidDescription, ValidInternalNotes);
        var originalTitle = qrCode.Title;
        var originalDescription = qrCode.Description;
        var originalIsActive = qrCode.IsActive;

        // Act
        qrCode.SetSortOrder(5);

        // Assert
        Assert.That(qrCode.Title, Is.EqualTo(originalTitle));
        Assert.That(qrCode.Description, Is.EqualTo(originalDescription));
        Assert.That(qrCode.IsActive, Is.EqualTo(originalIsActive));
    }

    [Test]
    public void Finds_CollectionShouldNotBeNullAfterConstruction()
    {
        // Arrange & Act
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidDescription, ValidInternalNotes);

        // Assert
        Assert.That(qrCode.Finds, Is.Not.Null);
        Assert.That(qrCode.Finds.Count, Is.EqualTo(0));
    }

    [Test]
    public void Constructor_WithNullDescription_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new QrCode(ValidCampaignId, ValidTitle, null!, ValidInternalNotes));
        Assert.That(ex.ParamName, Is.EqualTo("description"));
    }

    [Test]
    public void Update_WithNullDescription_ShouldThrowArgumentNullException()
    {
        // Arrange
        var qrCode = new QrCode(ValidCampaignId, ValidTitle, ValidDescription, ValidInternalNotes);

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => qrCode.Update(ValidTitle, null!, ValidInternalNotes));
        Assert.That(ex.ParamName, Is.EqualTo("description"));
    }
}
