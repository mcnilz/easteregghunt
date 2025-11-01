using EasterEggHunt.Domain.Entities;

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
        _testQrCode = new QrCode(_testCampaign.Id, "Test QR Code", "Test Description", "Test Note");
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
        Assert.That(retrievedFind, Is.Not.Null);
        Assert.That(retrievedFind!.QrCodeId, Is.EqualTo(_testQrCode.Id));
        Assert.That(retrievedFind.UserId, Is.EqualTo(_testUser.Id));
        Assert.That(retrievedFind.IpAddress, Is.EqualTo("127.0.0.1"));
        Assert.That(retrievedFind.UserAgent, Is.EqualTo("Test User Agent"));
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
        Assert.That(retrievedFind, Is.Not.Null);
        Assert.That(retrievedFind!.Id, Is.EqualTo(find.Id));
    }

    [Test]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Act
        var retrievedFind = await FindRepository.GetByIdAsync(999);

        // Assert
        Assert.That(retrievedFind, Is.Null);
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
        Assert.That(finds, Is.Not.Null);
        Assert.That(finds.Count(), Is.EqualTo(2));
        Assert.That(finds, Has.Some.Matches<Find>(f => f.Id == find1.Id));
        Assert.That(finds, Has.Some.Matches<Find>(f => f.Id == find2.Id));
    }

    [Test]
    public async Task GetByQrCodeIdAsync_ShouldReturnFindsForQrCode()
    {
        // Arrange
        var qrCode2 = new QrCode(_testCampaign.Id, "QR Code 2", "Description 2", "Note 2");
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
        Assert.That(finds, Is.Not.Null);
        Assert.That(finds.Count(), Is.EqualTo(1));
        Assert.That(finds, Has.Some.Matches<Find>(f => f.Id == find1.Id));
        Assert.That(finds, Has.None.Matches<Find>(f => f.Id == find2.Id));
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
        Assert.That(finds, Is.Not.Null);
        Assert.That(finds.Count(), Is.EqualTo(1));
        Assert.That(finds, Has.Some.Matches<Find>(f => f.Id == find1.Id));
        Assert.That(finds, Has.None.Matches<Find>(f => f.Id == find2.Id));
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
        Assert.That(updatedFind, Is.Not.Null);
        Assert.That(updatedFind!.UserAgent, Is.EqualTo("Updated User Agent"));
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
        Assert.That(result, Is.True);
        var deletedFind = await FindRepository.GetByIdAsync(find.Id);
        Assert.That(deletedFind, Is.Null);
    }

    [Test]
    public async Task DeleteAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Act
        var result = await FindRepository.DeleteAsync(999);
        await FindRepository.SaveChangesAsync();

        // Assert
        Assert.That(result, Is.False);
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
        Assert.That(exists, Is.True);
    }

    [Test]
    public async Task ExistsAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Act
        var exists = await FindRepository.ExistsAsync(999);

        // Assert
        Assert.That(exists, Is.False);
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
        Assert.That(savedFind, Is.Not.Null);
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
        Assert.That(hasFound, Is.True);
    }

    [Test]
    public async Task UserHasFoundQrCodeAsync_WithNonExistingFind_ShouldReturnFalse()
    {
        // Act
        var hasFound = await FindRepository.UserHasFoundQrCodeAsync(_testUser.Id, _testQrCode.Id);

        // Assert
        Assert.That(hasFound, Is.False);
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
        Assert.That(count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetCountByUserIdAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var qrCode2 = new QrCode(_testCampaign.Id, "QR Code 2", "Description 2", "Note 2");
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
        Assert.That(count, Is.EqualTo(2));
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
        Assert.That(retrievedFind, Is.Not.Null);
        Assert.That(retrievedFind!.QrCode, Is.Not.Null);
        Assert.That(retrievedFind.QrCode.Id, Is.EqualTo(_testQrCode.Id));
        Assert.That(retrievedFind.User, Is.Not.Null);
        Assert.That(retrievedFind.User.Id, Is.EqualTo(_testUser.Id));
    }

    #region Additional Query Methods Tests

    [Test]
    public async Task GetFirstByUserAndQrAsync_WithExistingFinds_ShouldReturnFirstFind()
    {
        // Arrange
        var find1 = new Find(_testQrCode.Id, _testUser.Id, "127.0.0.1", "User Agent 1");
        await FindRepository.AddAsync(find1);
        await FindRepository.SaveChangesAsync();

        Thread.Sleep(10); // Ensure different timestamps

        var find2 = new Find(_testQrCode.Id, _testUser.Id, "192.168.1.1", "User Agent 2");
        await FindRepository.AddAsync(find2);
        await FindRepository.SaveChangesAsync();

        // Act
        var firstFind = await FindRepository.GetFirstByUserAndQrAsync(_testUser.Id, _testQrCode.Id);

        // Assert
        Assert.That(firstFind, Is.Not.Null);
        Assert.That(firstFind!.Id, Is.EqualTo(find1.Id));
        Assert.That(firstFind.FoundAt, Is.LessThanOrEqualTo(find2.FoundAt));
    }

    [Test]
    public async Task GetFirstByUserAndQrAsync_WithNonExistingFinds_ShouldReturnNull()
    {
        // Act
        var firstFind = await FindRepository.GetFirstByUserAndQrAsync(999, 999);

        // Assert
        Assert.That(firstFind, Is.Null);
    }

    [Test]
    public async Task GetCampaignFindsAggregateAsync_ShouldReturnTotalFindsAndUniqueFinders()
    {
        // Arrange
        var user2 = new User("User 2");
        await UserRepository.AddAsync(user2);
        await UserRepository.SaveChangesAsync();

        var qrCode2 = new QrCode(_testCampaign.Id, "QR Code 2", "Description 2", "Note 2");
        await QrCodeRepository.AddAsync(qrCode2);
        await QrCodeRepository.SaveChangesAsync();

        var find1 = new Find(_testQrCode.Id, _testUser.Id, "127.0.0.1", "User Agent 1");
        var find2 = new Find(qrCode2.Id, _testUser.Id, "192.168.1.1", "User Agent 2");
        var find3 = new Find(_testQrCode.Id, user2.Id, "192.168.1.2", "User Agent 3");
        await FindRepository.AddAsync(find1);
        await FindRepository.AddAsync(find2);
        await FindRepository.AddAsync(find3);
        await FindRepository.SaveChangesAsync();

        // Act
        var (totalFinds, uniqueFinders) = await FindRepository.GetCampaignFindsAggregateAsync(_testCampaign.Id);

        // Assert
        Assert.That(totalFinds, Is.EqualTo(3));
        Assert.That(uniqueFinders, Is.EqualTo(2)); // Two different users
    }

    [Test]
    public async Task GetUniqueQrCodesCountByUserIdAsync_ShouldReturnUniqueCount()
    {
        // Arrange
        var qrCode2 = new QrCode(_testCampaign.Id, "QR Code 2", "Description 2", "Note 2");
        await QrCodeRepository.AddAsync(qrCode2);
        await QrCodeRepository.SaveChangesAsync();

        var find1 = new Find(_testQrCode.Id, _testUser.Id, "127.0.0.1", "User Agent 1");
        var find2 = new Find(qrCode2.Id, _testUser.Id, "192.168.1.1", "User Agent 2");
        var find3 = new Find(_testQrCode.Id, _testUser.Id, "192.168.1.2", "User Agent 3"); // Same QR code
        await FindRepository.AddAsync(find1);
        await FindRepository.AddAsync(find2);
        await FindRepository.AddAsync(find3);
        await FindRepository.SaveChangesAsync();

        // Act
        var uniqueCount = await FindRepository.GetUniqueQrCodesCountByUserIdAsync(_testUser.Id);

        // Assert
        Assert.That(uniqueCount, Is.EqualTo(2)); // Two unique QR codes
    }

    [Test]
    public async Task GetByUserAndCampaignAsync_ShouldReturnFindsForUserAndCampaign()
    {
        // Arrange
        var campaign2 = new Campaign("Campaign 2", "Description 2", "Admin");
        await CampaignRepository.AddAsync(campaign2);
        await CampaignRepository.SaveChangesAsync();

        var qrCode2 = new QrCode(campaign2.Id, "QR Code 2", "Description 2", "Note 2");
        await QrCodeRepository.AddAsync(qrCode2);
        await QrCodeRepository.SaveChangesAsync();

        var find1 = new Find(_testQrCode.Id, _testUser.Id, "127.0.0.1", "User Agent 1");
        var find2 = new Find(qrCode2.Id, _testUser.Id, "192.168.1.1", "User Agent 2"); // Different campaign
        await FindRepository.AddAsync(find1);
        await FindRepository.AddAsync(find2);
        await FindRepository.SaveChangesAsync();

        // Act
        var finds = await FindRepository.GetByUserAndCampaignAsync(_testUser.Id, _testCampaign.Id);

        // Assert
        Assert.That(finds, Is.Not.Null);
        Assert.That(finds.Count(), Is.EqualTo(1));
        Assert.That(finds.First().Id, Is.EqualTo(find1.Id));
    }

    [Test]
    public async Task GetByUserAndCampaignAsync_WithTakeParameter_ShouldLimitResults()
    {
        // Arrange
        var qrCode2 = new QrCode(_testCampaign.Id, "QR Code 2", "Description 2", "Note 2");
        await QrCodeRepository.AddAsync(qrCode2);
        await QrCodeRepository.SaveChangesAsync();

        var find1 = new Find(_testQrCode.Id, _testUser.Id, "127.0.0.1", "User Agent 1");
        var find2 = new Find(qrCode2.Id, _testUser.Id, "192.168.1.1", "User Agent 2");
        var find3 = new Find(_testQrCode.Id, _testUser.Id, "192.168.1.2", "User Agent 3");
        await FindRepository.AddAsync(find1);
        await FindRepository.AddAsync(find2);
        await FindRepository.AddAsync(find3);
        await FindRepository.SaveChangesAsync();

        // Act
        var finds = await FindRepository.GetByUserAndCampaignAsync(_testUser.Id, _testCampaign.Id, take: 2);

        // Assert
        Assert.That(finds, Is.Not.Null);
        Assert.That(finds.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task GetByUserAndCampaignAsync_WithNullTake_ShouldReturnAllResults()
    {
        // Arrange
        var qrCode2 = new QrCode(_testCampaign.Id, "QR Code 2", "Description 2", "Note 2");
        await QrCodeRepository.AddAsync(qrCode2);
        await QrCodeRepository.SaveChangesAsync();

        var find1 = new Find(_testQrCode.Id, _testUser.Id, "127.0.0.1", "User Agent 1");
        var find2 = new Find(qrCode2.Id, _testUser.Id, "192.168.1.1", "User Agent 2");
        await FindRepository.AddAsync(find1);
        await FindRepository.AddAsync(find2);
        await FindRepository.SaveChangesAsync();

        // Act
        var finds = await FindRepository.GetByUserAndCampaignAsync(_testUser.Id, _testCampaign.Id, take: null);

        // Assert
        Assert.That(finds, Is.Not.Null);
        Assert.That(finds.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task GetByCampaignIdAsync_ShouldReturnFindsForCampaign()
    {
        // Arrange
        var campaign2 = new Campaign("Campaign 2", "Description 2", "Admin");
        await CampaignRepository.AddAsync(campaign2);
        await CampaignRepository.SaveChangesAsync();

        var qrCode2 = new QrCode(campaign2.Id, "QR Code 2", "Description 2", "Note 2");
        await QrCodeRepository.AddAsync(qrCode2);
        await QrCodeRepository.SaveChangesAsync();

        var find1 = new Find(_testQrCode.Id, _testUser.Id, "127.0.0.1", "User Agent 1");
        var find2 = new Find(qrCode2.Id, _testUser.Id, "192.168.1.1", "User Agent 2"); // Different campaign
        await FindRepository.AddAsync(find1);
        await FindRepository.AddAsync(find2);
        await FindRepository.SaveChangesAsync();

        // Act
        var finds = await FindRepository.GetByCampaignIdAsync(_testCampaign.Id);

        // Assert
        Assert.That(finds, Is.Not.Null);
        Assert.That(finds.Count(), Is.EqualTo(1));
        Assert.That(finds.First().Id, Is.EqualTo(find1.Id));
    }

    [Test]
    public async Task GetByCampaignIdAsync_WithMultipleQrCodes_ShouldReturnAllFinds()
    {
        // Arrange
        var qrCode2 = new QrCode(_testCampaign.Id, "QR Code 2", "Description 2", "Note 2");
        await QrCodeRepository.AddAsync(qrCode2);
        await QrCodeRepository.SaveChangesAsync();

        var find1 = new Find(_testQrCode.Id, _testUser.Id, "127.0.0.1", "User Agent 1");
        var find2 = new Find(qrCode2.Id, _testUser.Id, "192.168.1.1", "User Agent 2");
        await FindRepository.AddAsync(find1);
        await FindRepository.AddAsync(find2);
        await FindRepository.SaveChangesAsync();

        // Act
        var finds = await FindRepository.GetByCampaignIdAsync(_testCampaign.Id);

        // Assert
        Assert.That(finds, Is.Not.Null);
        Assert.That(finds.Count(), Is.EqualTo(2));
    }

    #endregion
}
