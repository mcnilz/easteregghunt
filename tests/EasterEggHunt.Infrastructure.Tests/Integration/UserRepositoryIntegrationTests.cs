using System;
using System.Linq;
using System.Threading.Tasks;
using EasterEggHunt.Domain.Entities;
using NUnit.Framework;

namespace EasterEggHunt.Infrastructure.Tests.Integration;

[TestFixture]
[Category("Integration")]
public class UserRepositoryIntegrationTests : IntegrationTestBase
{
    [SetUp]
    public async Task SetUp()
    {
        await ResetDatabaseAsync();
    }

    [Test]
    public async Task AddAsync_WithValidUser_ShouldAddUser()
    {
        // Arrange
        var user = new User("Test User");

        // Act
        await UserRepository.AddAsync(user);
        await UserRepository.SaveChangesAsync();

        // Assert
        var retrievedUser = await UserRepository.GetByIdAsync(user.Id);
        Assert.That(retrievedUser, Is.Not.Null);
        Assert.That(retrievedUser!.Name, Is.EqualTo("Test User"));
        Assert.That(retrievedUser.IsActive, Is.True);
        Assert.That(retrievedUser.FirstSeen, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromMinutes(1)));
        Assert.That(retrievedUser.LastSeen, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromMinutes(1)));
    }

    [Test]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnUser()
    {
        // Arrange
        var user = new User("Existing User");
        await UserRepository.AddAsync(user);
        await UserRepository.SaveChangesAsync();

        // Act
        var retrievedUser = await UserRepository.GetByIdAsync(user.Id);

        // Assert
        Assert.That(retrievedUser, Is.Not.Null);
        Assert.That(retrievedUser!.Id, Is.EqualTo(user.Id));
        Assert.That(retrievedUser.Name, Is.EqualTo("Existing User"));
    }

    [Test]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Act
        var retrievedUser = await UserRepository.GetByIdAsync(999);

        // Assert
        Assert.That(retrievedUser, Is.Null);
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        // Arrange
        var user1 = new User("User 1");
        var user2 = new User("User 2");
        await UserRepository.AddAsync(user1);
        await UserRepository.AddAsync(user2);
        await UserRepository.SaveChangesAsync();

        // Act
        var users = await UserRepository.GetAllAsync();

        // Assert
        Assert.That(users, Is.Not.Null);
        Assert.That(users.Count(), Is.EqualTo(2));
        Assert.That(users, Has.Some.Matches<User>(u => u.Id == user1.Id));
        Assert.That(users, Has.Some.Matches<User>(u => u.Id == user2.Id));
    }

    [Test]
    public async Task GetByNameAsync_WithExistingName_ShouldReturnUser()
    {
        // Arrange
        var user = new User("Unique Name");
        await UserRepository.AddAsync(user);
        await UserRepository.SaveChangesAsync();

        // Act
        var retrievedUser = await UserRepository.GetByNameAsync("Unique Name");

        // Assert
        Assert.That(retrievedUser, Is.Not.Null);
        Assert.That(retrievedUser!.Id, Is.EqualTo(user.Id));
        Assert.That(retrievedUser.Name, Is.EqualTo("Unique Name"));
    }

    [Test]
    public async Task GetByNameAsync_WithNonExistingName_ShouldReturnNull()
    {
        // Act
        var retrievedUser = await UserRepository.GetByNameAsync("Non Existing Name");

        // Assert
        Assert.That(retrievedUser, Is.Null);
    }

    [Test]
    public async Task UpdateAsync_WithValidUser_ShouldUpdateUser()
    {
        // Arrange
        var user = new User("Original Name");
        await UserRepository.AddAsync(user);
        await UserRepository.SaveChangesAsync();

        user.UpdateLastSeen();

        // Act
        await UserRepository.UpdateAsync(user);
        await UserRepository.SaveChangesAsync();

        // Assert
        var updatedUser = await UserRepository.GetByIdAsync(user.Id);
        Assert.That(updatedUser, Is.Not.Null);
        Assert.That(updatedUser!.LastSeen, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromMinutes(1)));
    }

    [Test]
    public async Task DeleteAsync_WithExistingId_ShouldDeleteUser()
    {
        // Arrange
        var user = new User("To Be Deleted");
        await UserRepository.AddAsync(user);
        await UserRepository.SaveChangesAsync();

        // Act
        var result = await UserRepository.DeleteAsync(user.Id);
        await UserRepository.SaveChangesAsync();

        // Assert
        Assert.That(result, Is.True);
        var deletedUser = await UserRepository.GetByIdAsync(user.Id);
        Assert.That(deletedUser, Is.Null);
    }

    [Test]
    public async Task DeleteAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Act
        var result = await UserRepository.DeleteAsync(999);
        await UserRepository.SaveChangesAsync();

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
    {
        // Arrange
        var user = new User("Check Exists");
        await UserRepository.AddAsync(user);
        await UserRepository.SaveChangesAsync();

        // Act
        var exists = await UserRepository.ExistsAsync(user.Id);

        // Assert
        Assert.That(exists, Is.True);
    }

    [Test]
    public async Task ExistsAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Act
        var exists = await UserRepository.ExistsAsync(999);

        // Assert
        Assert.That(exists, Is.False);
    }

    [Test]
    public async Task SaveChangesAsync_ShouldSaveChanges()
    {
        // Arrange
        var user = new User("Save Test");
        await UserRepository.AddAsync(user);

        // Act
        await UserRepository.SaveChangesAsync();

        // Assert
        var savedUser = await UserRepository.GetByIdAsync(user.Id);
        Assert.That(savedUser, Is.Not.Null);
    }

    [Test]
    public async Task User_WithFinds_ShouldMaintainRelationship()
    {
        // Arrange
        var user = new User("User with Finds");
        var campaign = new Campaign("Test Campaign", "Test Description", "Test Creator");

        await CampaignRepository.AddAsync(campaign);
        await CampaignRepository.SaveChangesAsync();
        await UserRepository.AddAsync(user);
        await UserRepository.SaveChangesAsync();

        var qrCode = new QrCode(campaign.Id, "Test QR Code", "Test Description", "Test Note");
        await QrCodeRepository.AddAsync(qrCode);
        await QrCodeRepository.SaveChangesAsync();

        var find = new Find(qrCode.Id, user.Id, "127.0.0.1", "Test User Agent");
        await FindRepository.AddAsync(find);
        await FindRepository.SaveChangesAsync();

        // Act
        var retrievedUser = await UserRepository.GetByIdAsync(user.Id);

        // Assert
        Assert.That(retrievedUser, Is.Not.Null);
        Assert.That(retrievedUser!.Finds.Count(), Is.EqualTo(1));
        Assert.That(retrievedUser.Finds, Has.Some.Matches<Find>(f => f.Id == find.Id));
    }

    [Test]
    public async Task User_WithSessions_ShouldMaintainRelationship()
    {
        // Arrange
        var user = new User("User with Sessions");
        await UserRepository.AddAsync(user);
        await UserRepository.SaveChangesAsync();

        var session = new Session(user.Id, 30);
        await SessionRepository.AddAsync(session);
        await SessionRepository.SaveChangesAsync();

        // Act
        var retrievedUser = await UserRepository.GetByIdAsync(user.Id);

        // Assert
        Assert.That(retrievedUser, Is.Not.Null);
        Assert.That(retrievedUser!.Sessions.Count(), Is.EqualTo(1));
        Assert.That(retrievedUser.Sessions, Has.Some.Matches<Session>(s => s.Id == session.Id));
    }

    [Test]
    public async Task User_Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var user = new User("Test User");
        user.Deactivate(); // Erst deaktivieren
        await UserRepository.AddAsync(user);
        await UserRepository.SaveChangesAsync();

        // Act
        user.Activate();
        await UserRepository.UpdateAsync(user);
        await UserRepository.SaveChangesAsync();

        // Assert
        var retrievedUser = await UserRepository.GetByIdAsync(user.Id);
        Assert.That(retrievedUser, Is.Not.Null);
        Assert.That(retrievedUser!.IsActive, Is.True);
    }

    [Test]
    public async Task User_Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var user = new User("Test User");
        await UserRepository.AddAsync(user);
        await UserRepository.SaveChangesAsync();

        // Act
        user.Deactivate();
        await UserRepository.UpdateAsync(user);
        await UserRepository.SaveChangesAsync();

        // Assert
        var retrievedUser = await UserRepository.GetByIdAsync(user.Id);
        Assert.That(retrievedUser, Is.Not.Null);
        Assert.That(retrievedUser!.IsActive, Is.False);
    }
}
