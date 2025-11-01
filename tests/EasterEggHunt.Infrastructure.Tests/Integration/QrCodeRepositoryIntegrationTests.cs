using EasterEggHunt.Domain.Entities;

namespace EasterEggHunt.Infrastructure.Tests.Integration;

[TestFixture]
[Category("Integration")]
public class QrCodeRepositoryIntegrationTests : IntegrationTestBase
{
    private Campaign _testCampaign = null!;

    [SetUp]
    public async Task SetUp()
    {
        await ResetDatabaseAsync();

        // Test-Kampagne erstellen
        _testCampaign = new Campaign("Test Campaign", "Test Description", "Test Creator");
        await CampaignRepository.AddAsync(_testCampaign);
        await CampaignRepository.SaveChangesAsync();
    }

    [Test]
    public async Task AddAsync_WithValidQrCode_ShouldAddQrCode()
    {
        // Arrange
        var qrCode = new QrCode(_testCampaign.Id, "Test Title", "Test Description", "Test Note");

        // Act
        await QrCodeRepository.AddAsync(qrCode);
        await QrCodeRepository.SaveChangesAsync();

        // Assert
        var retrievedQrCode = await QrCodeRepository.GetByIdAsync(qrCode.Id);
        Assert.That(retrievedQrCode, Is.Not.Null);
        Assert.That(retrievedQrCode!.Title, Is.EqualTo("Test Title"));
        Assert.That(retrievedQrCode.InternalNotes, Is.EqualTo("Test Note"));
        Assert.That(retrievedQrCode.CampaignId, Is.EqualTo(_testCampaign.Id));
    }

    [Test]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnQrCode()
    {
        // Arrange
        var qrCode = new QrCode(_testCampaign.Id, "Existing Title", "Existing Description", "Existing Note");
        await QrCodeRepository.AddAsync(qrCode);
        await QrCodeRepository.SaveChangesAsync();

        // Act
        var retrievedQrCode = await QrCodeRepository.GetByIdAsync(qrCode.Id);

        // Assert
        Assert.That(retrievedQrCode, Is.Not.Null);
        Assert.That(retrievedQrCode!.Id, Is.EqualTo(qrCode.Id));
    }

    [Test]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Act
        var retrievedQrCode = await QrCodeRepository.GetByIdAsync(999);

        // Assert
        Assert.That(retrievedQrCode, Is.Null);
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnAllQrCodes()
    {
        // Arrange
        var qrCode1 = new QrCode(_testCampaign.Id, "Title 1", "Description 1", "Note 1");
        var qrCode2 = new QrCode(_testCampaign.Id, "Title 2", "Description 2", "Note 2");
        await QrCodeRepository.AddAsync(qrCode1);
        await QrCodeRepository.AddAsync(qrCode2);
        await QrCodeRepository.SaveChangesAsync();

        // Act
        var qrCodes = await QrCodeRepository.GetAllAsync();

        // Assert
        Assert.That(qrCodes, Is.Not.Null);
        Assert.That(qrCodes.Count(), Is.EqualTo(2));
        Assert.That(qrCodes, Has.Some.Matches<QrCode>(q => q.Id == qrCode1.Id));
        Assert.That(qrCodes, Has.Some.Matches<QrCode>(q => q.Id == qrCode2.Id));
    }

    [Test]
    public async Task GetByCampaignIdAsync_ShouldReturnQrCodesForCampaign()
    {
        // Arrange
        var campaign2 = new Campaign("Campaign 2", "Description 2", "Creator 2");
        await CampaignRepository.AddAsync(campaign2);
        await CampaignRepository.SaveChangesAsync();

        var qrCode1 = new QrCode(_testCampaign.Id, "Title 1", "Description 1", "Note 1");
        var qrCode2 = new QrCode(campaign2.Id, "Title 2", "Description 2", "Note 2");
        await QrCodeRepository.AddAsync(qrCode1);
        await QrCodeRepository.AddAsync(qrCode2);
        await QrCodeRepository.SaveChangesAsync();

        // Act
        var qrCodes = await QrCodeRepository.GetByCampaignIdAsync(_testCampaign.Id);

        // Assert
        Assert.That(qrCodes, Is.Not.Null);
        Assert.That(qrCodes.Count(), Is.EqualTo(1));
        Assert.That(qrCodes, Has.Some.Matches<QrCode>(q => q.Id == qrCode1.Id));
        Assert.That(qrCodes, Has.None.Matches<QrCode>(q => q.Id == qrCode2.Id));
    }

    [Test]
    public async Task UpdateAsync_WithValidQrCode_ShouldUpdateQrCode()
    {
        // Arrange
        var qrCode = new QrCode(_testCampaign.Id, "Original Title", "Original Description", "Original Note");
        await QrCodeRepository.AddAsync(qrCode);
        await QrCodeRepository.SaveChangesAsync();

        qrCode.Update("Updated Title", "Updated Description", "Updated Note");

        // Act
        await QrCodeRepository.UpdateAsync(qrCode);
        await QrCodeRepository.SaveChangesAsync();

        // Assert
        var updatedQrCode = await QrCodeRepository.GetByIdAsync(qrCode.Id);
        Assert.That(updatedQrCode, Is.Not.Null);
        Assert.That(updatedQrCode!.Title, Is.EqualTo("Updated Title"));
        Assert.That(updatedQrCode.InternalNotes, Is.EqualTo("Updated Note"));
    }

    [Test]
    public async Task DeleteAsync_WithExistingId_ShouldDeleteQrCode()
    {
        // Arrange
        var qrCode = new QrCode(_testCampaign.Id, "To Be Deleted", "Delete Description", "Delete Note");
        await QrCodeRepository.AddAsync(qrCode);
        await QrCodeRepository.SaveChangesAsync();

        // Act
        var result = await QrCodeRepository.DeleteAsync(qrCode.Id);
        await QrCodeRepository.SaveChangesAsync();

        // Assert
        Assert.That(result, Is.True);
        var deletedQrCode = await QrCodeRepository.GetByIdAsync(qrCode.Id);
        Assert.That(deletedQrCode, Is.Null);
    }

    [Test]
    public async Task DeleteAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Act
        var result = await QrCodeRepository.DeleteAsync(999);
        await QrCodeRepository.SaveChangesAsync();

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
    {
        // Arrange
        var qrCode = new QrCode(_testCampaign.Id, "Check Exists", "Exists Description", "Exists Note");
        await QrCodeRepository.AddAsync(qrCode);
        await QrCodeRepository.SaveChangesAsync();

        // Act
        var exists = await QrCodeRepository.ExistsAsync(qrCode.Id);

        // Assert
        Assert.That(exists, Is.True);
    }

    [Test]
    public async Task ExistsAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Act
        var exists = await QrCodeRepository.ExistsAsync(999);

        // Assert
        Assert.That(exists, Is.False);
    }

    [Test]
    public async Task SaveChangesAsync_ShouldSaveChanges()
    {
        // Arrange
        var qrCode = new QrCode(_testCampaign.Id, "Save Test", "Save Description", "Save Note");
        await QrCodeRepository.AddAsync(qrCode);

        // Act
        await QrCodeRepository.SaveChangesAsync();

        // Assert
        var savedQrCode = await QrCodeRepository.GetByIdAsync(qrCode.Id);
        Assert.That(savedQrCode, Is.Not.Null);
    }

    [Test]
    public async Task QrCode_WithFinds_ShouldMaintainRelationship()
    {
        // Arrange
        var qrCode = new QrCode(_testCampaign.Id, "Title with Finds", "Description with Finds", "Note with Finds");
        var user = new User("Test User");
        await UserRepository.AddAsync(user);
        await QrCodeRepository.AddAsync(qrCode);
        await QrCodeRepository.SaveChangesAsync();

        var find = new Find(qrCode.Id, user.Id, "127.0.0.1", "Test User Agent");
        await FindRepository.AddAsync(find);
        await FindRepository.SaveChangesAsync();

        // Act
        var retrievedQrCode = await QrCodeRepository.GetByIdAsync(qrCode.Id);

        // Assert
        Assert.That(retrievedQrCode, Is.Not.Null);
        Assert.That(retrievedQrCode!.Finds.Count(), Is.EqualTo(1));
        Assert.That(retrievedQrCode.Finds, Has.Some.Matches<Find>(f => f.Id == find.Id));
    }

    [Test]
    public async Task QrCode_UniqueUrl_ShouldBeGenerated()
    {
        // Arrange
        var qrCode = new QrCode(_testCampaign.Id, "URL Test", "URL Description", "URL Note");

        // Act
        await QrCodeRepository.AddAsync(qrCode);
        await QrCodeRepository.SaveChangesAsync();

        // Assert
        var retrievedQrCode = await QrCodeRepository.GetByIdAsync(qrCode.Id);
        Assert.That(retrievedQrCode, Is.Not.Null);
        Assert.That(retrievedQrCode!.Code, Is.Not.Null);
        Assert.That(retrievedQrCode.Code, Is.Not.Empty);
        Assert.That(retrievedQrCode.Code.Length, Is.EqualTo(12));
    }

    #region Additional Query Methods Tests

    [Test]
    public async Task GetActiveByCampaignIdAsync_ShouldReturnOnlyActiveQrCodes()
    {
        // Arrange
        var activeQrCode = new QrCode(_testCampaign.Id, "Active QR", "Description", "Note");
        var inactiveQrCode = new QrCode(_testCampaign.Id, "Inactive QR", "Description", "Note");
        inactiveQrCode.Deactivate();
        await QrCodeRepository.AddAsync(activeQrCode);
        await QrCodeRepository.AddAsync(inactiveQrCode);
        await QrCodeRepository.SaveChangesAsync();

        // Act
        var activeQrCodes = await QrCodeRepository.GetActiveByCampaignIdAsync(_testCampaign.Id);

        // Assert
        Assert.That(activeQrCodes, Is.Not.Null);
        Assert.That(activeQrCodes, Has.Some.Matches<QrCode>(q => q.Id == activeQrCode.Id));
        Assert.That(activeQrCodes, Has.None.Matches<QrCode>(q => q.Id == inactiveQrCode.Id));
    }

    [Test]
    public async Task GetByCodeAsync_WithExistingCode_ShouldReturnQrCode()
    {
        // Arrange
        var qrCode = new QrCode(_testCampaign.Id, "Code Test", "Description", "Note");
        await QrCodeRepository.AddAsync(qrCode);
        await QrCodeRepository.SaveChangesAsync();

        var retrievedQrCode = await QrCodeRepository.GetByIdAsync(qrCode.Id);
        var code = retrievedQrCode!.Code;

        // Act
        var foundQrCode = await QrCodeRepository.GetByCodeAsync(code);

        // Assert
        Assert.That(foundQrCode, Is.Not.Null);
        Assert.That(foundQrCode!.Id, Is.EqualTo(qrCode.Id));
        Assert.That(foundQrCode.Code, Is.EqualTo(code));
    }

    [Test]
    public async Task GetByCodeAsync_WithNonExistingCode_ShouldReturnNull()
    {
        // Act
        var foundQrCode = await QrCodeRepository.GetByCodeAsync("NONEXISTING");

        // Assert
        Assert.That(foundQrCode, Is.Null);
    }

    [Test]
    public async Task CodeExistsAsync_WithExistingCode_ShouldReturnTrue()
    {
        // Arrange
        var qrCode = new QrCode(_testCampaign.Id, "Exists Test", "Description", "Note");
        await QrCodeRepository.AddAsync(qrCode);
        await QrCodeRepository.SaveChangesAsync();

        var retrievedQrCode = await QrCodeRepository.GetByIdAsync(qrCode.Id);
        var code = retrievedQrCode!.Code;

        // Act
        var exists = await QrCodeRepository.CodeExistsAsync(code);

        // Assert
        Assert.That(exists, Is.True);
    }

    [Test]
    public async Task CodeExistsAsync_WithNonExistingCode_ShouldReturnFalse()
    {
        // Act
        var exists = await QrCodeRepository.CodeExistsAsync("NONEXISTING");

        // Assert
        Assert.That(exists, Is.False);
    }


    #endregion
}
