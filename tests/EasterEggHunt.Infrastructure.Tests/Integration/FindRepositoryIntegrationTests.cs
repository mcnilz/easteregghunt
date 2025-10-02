using System;
using System.Linq;
using System.Threading.Tasks;
using EasterEggHunt.Domain.Entities;
using FluentAssertions;
using NUnit.Framework;

namespace EasterEggHunt.Infrastructure.Tests.Integration;

[TestFixture]
[Category("Integration")]
public class FindRepositoryIntegrationTests : IntegrationTestBase
{
    private Campaign _testCampaign = null!;
    private User _testUser = null!;
    private QrCode _testQrCode = null!;

    [SetUp]
    public async Task SetUp()
    {
        await ResetDatabaseAsync();

        // Test-Kampagne erstellen
        _testCampaign = new Campaign("Test Campaign", "Test Description", "Test Creator");
        await CampaignRepository.AddAsync(_testCampaign);
        await CampaignRepository.SaveChangesAsync();

        // Test-Benutzer erstellen
        _testUser = new User("Test User");
        await UserRepository.AddAsync(_testUser);
        await UserRepository.SaveChangesAsync();

        // Test-QR-Code erstellen
        _testQrCode = new QrCode(_testCampaign.Id, "Test QR Code", "Test Note");
        await QrCodeRepository.AddAsync(_testQrCode);
        await QrCodeRepository.SaveChangesAsync();
    }

    [Test]
    public async Task AddAsync_WithValidFind_ShouldAddFind()
    {
        // Arrange
        var find = new Find(_testQrCode.Id, _testUser.Id, "127.0.0.1", "Test User Agent");

        // Act
        await FindRepository.AddAsync(find);
        await FindRepository.SaveChangesAsync();

        // Assert
        var retrievedFind = await FindRepository.GetByIdAsync(find.Id);
        retrievedFind.Should().NotBeNull();
        retrievedFind!.QrCodeId.Should().Be(_testQrCode.Id);
        retrievedFind.UserId.Should().Be(_testUser.Id);
        retrievedFind.IpAddress.Should().Be("127.0.0.1");
        retrievedFind.UserAgent.Should().Be("Test User Agent");
    }

    [Test]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnFind()
    {
        // Arrange
        var find = new Find(_testQrCode.Id, _testUser.Id, "127.0.0.1", "Test User Agent");
        await FindRepository.AddAsync(find);
        await FindRepository.SaveChangesAsync();

        // Act
        var retrievedFind = await FindRepository.GetByIdAsync(find.Id);

        // Assert
        retrievedFind.Should().NotBeNull();
        retrievedFind!.Id.Should().Be(find.Id);
    }

    [Test]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Act
        var retrievedFind = await FindRepository.GetByIdAsync(999);

        // Assert
        retrievedFind.Should().BeNull();
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnAllFinds()
    {
        // Arrange
        var find1 = new Find(_testQrCode.Id, _testUser.Id, "127.0.0.1", "User Agent 1");
        var find2 = new Find(_testQrCode.Id, _testUser.Id, "192.168.1.1", "User Agent 2");
        await FindRepository.AddAsync(find1);
        await FindRepository.AddAsync(find2);
        await FindRepository.SaveChangesAsync();

        // Act
        var finds = await FindRepository.GetAllAsync();

        // Assert
        finds.Should().NotBeNull();
        finds.Should().HaveCount(2);
        finds.Should().Contain(f => f.Id == find1.Id);
        finds.Should().Contain(f => f.Id == find2.Id);
    }

    [Test]
    public async Task GetByQrCodeIdAsync_ShouldReturnFindsForQrCode()
    {
        // Arrange
        var qrCode2 = new QrCode(_testCampaign.Id, "QR Code 2", "Note 2");
        await QrCodeRepository.AddAsync(qrCode2);
        await QrCodeRepository.SaveChangesAsync();

        var find1 = new Find(_testQrCode.Id, _testUser.Id, "127.0.0.1", "User Agent 1");
        var find2 = new Find(qrCode2.Id, _testUser.Id, "192.168.1.1", "User Agent 2");
        await FindRepository.AddAsync(find1);
        await FindRepository.AddAsync(find2);
        await FindRepository.SaveChangesAsync();

        // Act
        var finds = await FindRepository.GetByQrCodeIdAsync(_testQrCode.Id);

        // Assert
        finds.Should().NotBeNull();
        finds.Should().HaveCount(1);
        finds.Should().Contain(f => f.Id == find1.Id);
        finds.Should().NotContain(f => f.Id == find2.Id);
    }

    [Test]
    public async Task GetByUserIdAsync_ShouldReturnFindsForUser()
    {
        // Arrange
        var user2 = new User("User 2");
        await UserRepository.AddAsync(user2);
        await UserRepository.SaveChangesAsync();

        var find1 = new Find(_testQrCode.Id, _testUser.Id, "127.0.0.1", "User Agent 1");
        var find2 = new Find(_testQrCode.Id, user2.Id, "192.168.1.1", "User Agent 2");
        await FindRepository.AddAsync(find1);
        await FindRepository.AddAsync(find2);
        await FindRepository.SaveChangesAsync();

        // Act
        var finds = await FindRepository.GetByUserIdAsync(_testUser.Id);

        // Assert
        finds.Should().NotBeNull();
        finds.Should().HaveCount(1);
        finds.Should().Contain(f => f.Id == find1.Id);
        finds.Should().NotContain(f => f.Id == find2.Id);
    }

    [Test]
    public async Task UpdateAsync_WithValidFind_ShouldUpdateFind()
    {
        // Arrange
        var find = new Find(_testQrCode.Id, _testUser.Id, "127.0.0.1", "Original User Agent");
        await FindRepository.AddAsync(find);
        await FindRepository.SaveChangesAsync();

        find.UserAgent = "Updated User Agent";

        // Act
        await FindRepository.UpdateAsync(find);
        await FindRepository.SaveChangesAsync();

        // Assert
        var updatedFind = await FindRepository.GetByIdAsync(find.Id);
        updatedFind.Should().NotBeNull();
        updatedFind!.UserAgent.Should().Be("Updated User Agent");
    }

    [Test]
    public async Task DeleteAsync_WithExistingId_ShouldDeleteFind()
    {
        // Arrange
        var find = new Find(_testQrCode.Id, _testUser.Id, "127.0.0.1", "To Be Deleted");
        await FindRepository.AddAsync(find);
        await FindRepository.SaveChangesAsync();

        // Act
        var result = await FindRepository.DeleteAsync(find.Id);
        await FindRepository.SaveChangesAsync();

        // Assert
        result.Should().BeTrue();
        var deletedFind = await FindRepository.GetByIdAsync(find.Id);
        deletedFind.Should().BeNull();
    }

    [Test]
    public async Task DeleteAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Act
        var result = await FindRepository.DeleteAsync(999);
        await FindRepository.SaveChangesAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
    {
        // Arrange
        var find = new Find(_testQrCode.Id, _testUser.Id, "127.0.0.1", "Check Exists");
        await FindRepository.AddAsync(find);
        await FindRepository.SaveChangesAsync();

        // Act
        var exists = await FindRepository.ExistsAsync(find.Id);

        // Assert
        exists.Should().BeTrue();
    }

    [Test]
    public async Task ExistsAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Act
        var exists = await FindRepository.ExistsAsync(999);

        // Assert
        exists.Should().BeFalse();
    }

    [Test]
    public async Task SaveChangesAsync_ShouldSaveChanges()
    {
        // Arrange
        var find = new Find(_testQrCode.Id, _testUser.Id, "127.0.0.1", "Save Test");
        await FindRepository.AddAsync(find);

        // Act
        await FindRepository.SaveChangesAsync();

        // Assert
        var savedFind = await FindRepository.GetByIdAsync(find.Id);
        savedFind.Should().NotBeNull();
    }

    [Test]
    public async Task UserHasFoundQrCodeAsync_WithExistingFind_ShouldReturnTrue()
    {
        // Arrange
        var find = new Find(_testQrCode.Id, _testUser.Id, "127.0.0.1", "Test User Agent");
        await FindRepository.AddAsync(find);
        await FindRepository.SaveChangesAsync();

        // Act
        var hasFound = await FindRepository.UserHasFoundQrCodeAsync(_testUser.Id, _testQrCode.Id);

        // Assert
        hasFound.Should().BeTrue();
    }

    [Test]
    public async Task UserHasFoundQrCodeAsync_WithNonExistingFind_ShouldReturnFalse()
    {
        // Act
        var hasFound = await FindRepository.UserHasFoundQrCodeAsync(_testUser.Id, _testQrCode.Id);

        // Assert
        hasFound.Should().BeFalse();
    }

    [Test]
    public async Task GetCountByQrCodeIdAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var user2 = new User("User 2");
        await UserRepository.AddAsync(user2);
        await UserRepository.SaveChangesAsync();

        var find1 = new Find(_testQrCode.Id, _testUser.Id, "127.0.0.1", "User Agent 1");
        var find2 = new Find(_testQrCode.Id, user2.Id, "192.168.1.1", "User Agent 2");
        await FindRepository.AddAsync(find1);
        await FindRepository.AddAsync(find2);
        await FindRepository.SaveChangesAsync();

        // Act
        var finds = await FindRepository.GetByQrCodeIdAsync(_testQrCode.Id);
        var count = finds.Count();

        // Assert
        count.Should().Be(2);
    }

    [Test]
    public async Task GetCountByUserIdAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var qrCode2 = new QrCode(_testCampaign.Id, "QR Code 2", "Note 2");
        await QrCodeRepository.AddAsync(qrCode2);
        await QrCodeRepository.SaveChangesAsync();

        var find1 = new Find(_testQrCode.Id, _testUser.Id, "127.0.0.1", "User Agent 1");
        var find2 = new Find(qrCode2.Id, _testUser.Id, "192.168.1.1", "User Agent 2");
        await FindRepository.AddAsync(find1);
        await FindRepository.AddAsync(find2);
        await FindRepository.SaveChangesAsync();

        // Act
        var finds = await FindRepository.GetByUserIdAsync(_testUser.Id);
        var count = finds.Count();

        // Assert
        count.Should().Be(2);
    }

    [Test]
    public async Task Find_WithNavigationProperties_ShouldMaintainRelationships()
    {
        // Arrange
        var find = new Find(_testQrCode.Id, _testUser.Id, "127.0.0.1", "Test User Agent");
        await FindRepository.AddAsync(find);
        await FindRepository.SaveChangesAsync();

        // Act
        var retrievedFind = await FindRepository.GetByIdAsync(find.Id);

        // Assert
        retrievedFind.Should().NotBeNull();
        retrievedFind!.QrCode.Should().NotBeNull();
        retrievedFind.QrCode.Id.Should().Be(_testQrCode.Id);
        retrievedFind.User.Should().NotBeNull();
        retrievedFind.User.Id.Should().Be(_testUser.Id);
    }
}
