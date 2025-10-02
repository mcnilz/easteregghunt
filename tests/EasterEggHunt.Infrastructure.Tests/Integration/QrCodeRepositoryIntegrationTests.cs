using NUnit.Framework;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using EasterEggHunt.Domain.Entities;
using System.Linq;

namespace EasterEggHunt.Infrastructure.Tests.Integration;

[TestFixture]
[Category("Integration")]
public class QrCodeRepositoryIntegrationTests : IntegrationTestBase
{
    private Campaign _testCampaign;

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
        var qrCode = new QrCode(_testCampaign.Id, "Test Title", "Test Note");

        // Act
        await QrCodeRepository.AddAsync(qrCode);
        await QrCodeRepository.SaveChangesAsync();

        // Assert
        var retrievedQrCode = await QrCodeRepository.GetByIdAsync(qrCode.Id);
        retrievedQrCode.Should().NotBeNull();
        retrievedQrCode!.Title.Should().Be("Test Title");
        retrievedQrCode.InternalNote.Should().Be("Test Note");
        retrievedQrCode.CampaignId.Should().Be(_testCampaign.Id);
    }

    [Test]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnQrCode()
    {
        // Arrange
        var qrCode = new QrCode(_testCampaign.Id, "Existing Title", "Existing Note");
        await QrCodeRepository.AddAsync(qrCode);
        await QrCodeRepository.SaveChangesAsync();

        // Act
        var retrievedQrCode = await QrCodeRepository.GetByIdAsync(qrCode.Id);

        // Assert
        retrievedQrCode.Should().NotBeNull();
        retrievedQrCode!.Id.Should().Be(qrCode.Id);
    }

    [Test]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Act
        var retrievedQrCode = await QrCodeRepository.GetByIdAsync(999);

        // Assert
        retrievedQrCode.Should().BeNull();
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnAllQrCodes()
    {
        // Arrange
        var qrCode1 = new QrCode(_testCampaign.Id, "Title 1", "Note 1");
        var qrCode2 = new QrCode(_testCampaign.Id, "Title 2", "Note 2");
        await QrCodeRepository.AddAsync(qrCode1);
        await QrCodeRepository.AddAsync(qrCode2);
        await QrCodeRepository.SaveChangesAsync();

        // Act
        var qrCodes = await QrCodeRepository.GetAllAsync();

        // Assert
        qrCodes.Should().NotBeNull();
        qrCodes.Should().HaveCount(2);
        qrCodes.Should().Contain(q => q.Id == qrCode1.Id);
        qrCodes.Should().Contain(q => q.Id == qrCode2.Id);
    }

    [Test]
    public async Task GetByCampaignIdAsync_ShouldReturnQrCodesForCampaign()
    {
        // Arrange
        var campaign2 = new Campaign("Campaign 2", "Description 2", "Creator 2");
        await CampaignRepository.AddAsync(campaign2);
        await CampaignRepository.SaveChangesAsync();

        var qrCode1 = new QrCode(_testCampaign.Id, "Title 1", "Note 1");
        var qrCode2 = new QrCode(campaign2.Id, "Title 2", "Note 2");
        await QrCodeRepository.AddAsync(qrCode1);
        await QrCodeRepository.AddAsync(qrCode2);
        await QrCodeRepository.SaveChangesAsync();

        // Act
        var qrCodes = await QrCodeRepository.GetByCampaignIdAsync(_testCampaign.Id);

        // Assert
        qrCodes.Should().NotBeNull();
        qrCodes.Should().HaveCount(1);
        qrCodes.Should().Contain(q => q.Id == qrCode1.Id);
        qrCodes.Should().NotContain(q => q.Id == qrCode2.Id);
    }

    [Test]
    public async Task UpdateAsync_WithValidQrCode_ShouldUpdateQrCode()
    {
        // Arrange
        var qrCode = new QrCode(_testCampaign.Id, "Original Title", "Original Note");
        await QrCodeRepository.AddAsync(qrCode);
        await QrCodeRepository.SaveChangesAsync();

        qrCode.Update("Updated Title", "Updated Note");

        // Act
        await QrCodeRepository.UpdateAsync(qrCode);
        await QrCodeRepository.SaveChangesAsync();

        // Assert
        var updatedQrCode = await QrCodeRepository.GetByIdAsync(qrCode.Id);
        updatedQrCode.Should().NotBeNull();
        updatedQrCode!.Title.Should().Be("Updated Title");
        updatedQrCode.InternalNote.Should().Be("Updated Note");
    }

    [Test]
    public async Task DeleteAsync_WithExistingId_ShouldDeleteQrCode()
    {
        // Arrange
        var qrCode = new QrCode(_testCampaign.Id, "To Be Deleted", "Delete Note");
        await QrCodeRepository.AddAsync(qrCode);
        await QrCodeRepository.SaveChangesAsync();

        // Act
        var result = await QrCodeRepository.DeleteAsync(qrCode.Id);
        await QrCodeRepository.SaveChangesAsync();

        // Assert
        result.Should().BeTrue();
        var deletedQrCode = await QrCodeRepository.GetByIdAsync(qrCode.Id);
        deletedQrCode.Should().BeNull();
    }

    [Test]
    public async Task DeleteAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Act
        var result = await QrCodeRepository.DeleteAsync(999);
        await QrCodeRepository.SaveChangesAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
    {
        // Arrange
        var qrCode = new QrCode(_testCampaign.Id, "Check Exists", "Exists Note");
        await QrCodeRepository.AddAsync(qrCode);
        await QrCodeRepository.SaveChangesAsync();

        // Act
        var exists = await QrCodeRepository.ExistsAsync(qrCode.Id);

        // Assert
        exists.Should().BeTrue();
    }

    [Test]
    public async Task ExistsAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Act
        var exists = await QrCodeRepository.ExistsAsync(999);

        // Assert
        exists.Should().BeFalse();
    }

    [Test]
    public async Task SaveChangesAsync_ShouldSaveChanges()
    {
        // Arrange
        var qrCode = new QrCode(_testCampaign.Id, "Save Test", "Save Note");
        await QrCodeRepository.AddAsync(qrCode);

        // Act
        await QrCodeRepository.SaveChangesAsync();

        // Assert
        var savedQrCode = await QrCodeRepository.GetByIdAsync(qrCode.Id);
        savedQrCode.Should().NotBeNull();
    }

    [Test]
    public async Task QrCode_WithFinds_ShouldMaintainRelationship()
    {
        // Arrange
        var qrCode = new QrCode(_testCampaign.Id, "Title with Finds", "Note with Finds");
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
        retrievedQrCode.Should().NotBeNull();
        retrievedQrCode!.Finds.Should().HaveCount(1);
        retrievedQrCode.Finds.Should().Contain(f => f.Id == find.Id);
    }

    [Test]
    public async Task QrCode_UniqueUrl_ShouldBeGenerated()
    {
        // Arrange
        var qrCode = new QrCode(_testCampaign.Id, "URL Test", "URL Note");

        // Act
        await QrCodeRepository.AddAsync(qrCode);
        await QrCodeRepository.SaveChangesAsync();

        // Assert
        var retrievedQrCode = await QrCodeRepository.GetByIdAsync(qrCode.Id);
        retrievedQrCode.Should().NotBeNull();
        retrievedQrCode!.UniqueUrl.Should().NotBeNull();
        retrievedQrCode.UniqueUrl.ToString().Should().StartWith("https://easteregghunt.local/qr/");
    }
}
