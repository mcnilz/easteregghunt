using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Integration.Tests;
using NUnit.Framework;

namespace EasterEggHunt.Integration.Tests.Repositories;

/// <summary>
/// Integration Tests für Find Repository mit echter SQLite-Datenbank
/// </summary>
[TestFixture]
public class FindRepositoryIntegrationTests : IntegrationTestBase
{
    [SetUp]
    public void Setup()
    {
        // Test-Daten werden bereits in IntegrationTestBase geladen
    }

    [Test]
    public void GetAllAsync_ReturnsAllFinds()
    {
        // Act
        var finds = Context.Finds.ToList();

        // Assert
        Assert.That(finds, Is.Not.Empty);
        Assert.That(finds.Count, Is.GreaterThanOrEqualTo(2)); // Mindestens 2 Funde in Test-Daten
    }

    [Test]
    public void GetByUserIdAsync_WithValidUserId_ReturnsUserFinds()
    {
        // Arrange
        var userId = 1;

        // Act
        var userFinds = Context.Finds.Where(f => f.UserId == userId).ToList();

        // Assert
        Assert.That(userFinds, Is.Not.Empty);
        Assert.That(userFinds.All(f => f.UserId == userId), Is.True);
    }

    [Test]
    public void GetByUserIdAsync_WithInvalidUserId_ReturnsEmpty()
    {
        // Arrange
        var userId = 999;

        // Act
        var userFinds = Context.Finds.Where(f => f.UserId == userId).ToList();

        // Assert
        Assert.That(userFinds, Is.Empty);
    }

    [Test]
    public void GetByQrCodeIdAsync_WithValidQrCodeId_ReturnsQrCodeFinds()
    {
        // Arrange
        var qrCodeId = 1;

        // Act
        var qrCodeFinds = Context.Finds.Where(f => f.QrCodeId == qrCodeId).ToList();

        // Assert
        Assert.That(qrCodeFinds, Is.Not.Empty);
        Assert.That(qrCodeFinds.All(f => f.QrCodeId == qrCodeId), Is.True);
    }

    [Test]
    public void GetByQrCodeIdAsync_WithInvalidQrCodeId_ReturnsEmpty()
    {
        // Arrange
        var qrCodeId = 999;

        // Act
        var qrCodeFinds = Context.Finds.Where(f => f.QrCodeId == qrCodeId).ToList();

        // Assert
        Assert.That(qrCodeFinds, Is.Empty);
    }

    [Test]
    public void GetByCampaignIdAsync_WithValidCampaignId_ReturnsCampaignFinds()
    {
        // Arrange
        var campaignId = 1;

        // Act
        var campaignFinds = Context.Finds
            .Where(f => Context.QrCodes.Any(q => q.Id == f.QrCodeId && q.CampaignId == campaignId))
            .ToList();

        // Assert
        Assert.That(campaignFinds, Is.Not.Empty);
    }

    [Test]
    public void GetByIdAsync_WithValidId_ReturnsFind()
    {
        // Arrange
        var findId = 1;

        // Act
        var find = Context.Finds.FirstOrDefault(f => f.Id == findId);

        // Assert
        Assert.That(find, Is.Not.Null);
        Assert.That(find!.Id, Is.EqualTo(findId));
        Assert.That(find.UserId, Is.EqualTo(1));
        Assert.That(find.QrCodeId, Is.EqualTo(1));
    }

    [Test]
    public void GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var findId = 999;

        // Act
        var find = Context.Finds.FirstOrDefault(f => f.Id == findId);

        // Assert
        Assert.That(find, Is.Null);
    }

    [Test]
    public void UserHasFoundQrCodeAsync_WithValidIds_ReturnsTrue()
    {
        // Arrange
        var userId = 1;
        var qrCodeId = 1;

        // Act
        var hasFound = Context.Finds.Any(f => f.UserId == userId && f.QrCodeId == qrCodeId);

        // Assert
        Assert.That(hasFound, Is.True);
    }

    [Test]
    public void UserHasFoundQrCodeAsync_WithInvalidIds_ReturnsFalse()
    {
        // Arrange
        var userId = 999;
        var qrCodeId = 999;

        // Act
        var hasFound = Context.Finds.Any(f => f.UserId == userId && f.QrCodeId == qrCodeId);

        // Assert
        Assert.That(hasFound, Is.False);
    }

    [Test]
    public async Task AddAsync_WithValidData_CreatesFind()
    {
        // Arrange
        var uniqueId = Random.Shared.Next(1000, 9999);
        var newFind = new Find(1, 1, "127.0.0.1", "Test Agent")
        {
            Id = uniqueId,
            FoundAt = DateTime.UtcNow
        };

        // Act
        Context.Finds.Add(newFind);
        await Context.SaveChangesAsync();

        // Assert
        var createdFind = Context.Finds.FirstOrDefault(f => f.Id == uniqueId);
        Assert.That(createdFind, Is.Not.Null);
        Assert.That(createdFind!.QrCodeId, Is.EqualTo(1));
        Assert.That(createdFind.UserId, Is.EqualTo(1));
        Assert.That(createdFind.IpAddress, Is.EqualTo("127.0.0.1"));
        Assert.That(createdFind.UserAgent, Is.EqualTo("Test Agent"));
    }

    [Test]
    public async Task UpdateAsync_WithValidData_UpdatesFind()
    {
        // Arrange
        var uniqueId = Random.Shared.Next(1000, 9999);
        var find = new Find(1, 1, "127.0.0.1", "Test Agent")
        {
            Id = uniqueId,
            FoundAt = DateTime.UtcNow
        };
        Context.Finds.Add(find);
        await Context.SaveChangesAsync();

        // Act
        find.IpAddress = "192.168.1.1";
        find.UserAgent = "Updated Agent";
        await Context.SaveChangesAsync();

        // Assert
        var updatedFind = Context.Finds.FirstOrDefault(f => f.Id == uniqueId);
        Assert.That(updatedFind, Is.Not.Null);
        Assert.That(updatedFind!.IpAddress, Is.EqualTo("192.168.1.1"));
        Assert.That(updatedFind.UserAgent, Is.EqualTo("Updated Agent"));
    }

    [Test]
    public async Task DeleteAsync_WithValidId_DeletesFind()
    {
        // Arrange
        var uniqueId = Random.Shared.Next(1000, 9999);
        var find = new Find(1, 1, "127.0.0.1", "Test Agent")
        {
            Id = uniqueId,
            FoundAt = DateTime.UtcNow
        };
        Context.Finds.Add(find);
        await Context.SaveChangesAsync();

        // Act
        Context.Finds.Remove(find);
        await Context.SaveChangesAsync();

        // Assert
        var deletedFind = Context.Finds.FirstOrDefault(f => f.Id == uniqueId);
        Assert.That(deletedFind, Is.Null);
    }

    [Test]
    public void Find_WithValidData_HasCorrectProperties()
    {
        // Arrange & Act
        var find = Context.Finds.FirstOrDefault(f => f.Id == 1);

        // Assert
        Assert.That(find, Is.Not.Null);
        Assert.That(find!.QrCodeId, Is.EqualTo(1));
        Assert.That(find.UserId, Is.EqualTo(1));
        Assert.That(find.IpAddress, Is.EqualTo("127.0.0.1"));
        Assert.That(find.UserAgent, Is.EqualTo("Test Agent"));
        Assert.That(find.FoundAt, Is.Not.EqualTo(DateTime.MinValue));
    }

    [Test]
    public void Find_WithUserNavigation_LoadsUser()
    {
        // Arrange & Act
        var find = Context.Finds
            .Where(f => f.Id == 1)
            .Select(f => new { Find = f, User = f.User })
            .FirstOrDefault();

        // Assert
        Assert.That(find, Is.Not.Null);
        Assert.That(find!.User, Is.Not.Null);
        Assert.That(find.User.Name, Is.EqualTo("Test Benutzer"));
    }

    [Test]
    public void Find_WithQrCodeNavigation_LoadsQrCode()
    {
        // Arrange & Act
        var find = Context.Finds
            .Where(f => f.Id == 1)
            .Select(f => new { Find = f, QrCode = f.QrCode })
            .FirstOrDefault();

        // Assert
        Assert.That(find, Is.Not.Null);
        Assert.That(find!.QrCode, Is.Not.Null);
        Assert.That(find.QrCode.Title, Is.EqualTo("QR Code 1"));
    }

    [Test]
    public async Task Find_WithDuplicateUserAndQrCode_CanBeCreated()
    {
        // Arrange
        var uniqueId1 = Random.Shared.Next(1000, 9999);
        var uniqueId2 = Random.Shared.Next(1000, 9999);
        var find1 = new Find(1, 1, "127.0.0.1", "Agent 1")
        {
            Id = uniqueId1,
            FoundAt = DateTime.UtcNow.AddMinutes(-10)
        };
        var find2 = new Find(1, 1, "127.0.0.2", "Agent 2")
        {
            Id = uniqueId2,
            FoundAt = DateTime.UtcNow.AddMinutes(-5)
        };

        // Act
        Context.Finds.Add(find1);
        Context.Finds.Add(find2);
        await Context.SaveChangesAsync();

        // Assert
        var finds = Context.Finds.Where(f => f.QrCodeId == 1 && f.UserId == 1).ToList();
        Assert.That(finds.Count, Is.GreaterThanOrEqualTo(2));
    }
}
