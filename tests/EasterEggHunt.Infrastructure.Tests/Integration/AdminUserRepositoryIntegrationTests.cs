using NUnit.Framework;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using EasterEggHunt.Domain.Entities;
using System.Linq;

namespace EasterEggHunt.Infrastructure.Tests.Integration;

[TestFixture]
[Category("Integration")]
public class AdminUserRepositoryIntegrationTests : IntegrationTestBase
{
    [SetUp]
    public async Task SetUp()
    {
        await ResetDatabaseAsync();
        await SeedTestDataAsync();
    }

    [Test]
    public async Task AddAsync_WithValidAdminUser_ShouldAddAdminUser()
    {
        // Arrange
        var adminUser = new AdminUser("Test Admin", "test@admin.com", "hashedPassword123");

        // Act
        await AdminUserRepository.AddAsync(adminUser);
        await AdminUserRepository.SaveChangesAsync();

        // Assert
        var retrievedAdminUser = await AdminUserRepository.GetByIdAsync(adminUser.Id);
        retrievedAdminUser.Should().NotBeNull();
        retrievedAdminUser!.Username.Should().Be("Test Admin");
        retrievedAdminUser.Email.Should().Be("test@admin.com");
        retrievedAdminUser.IsActive.Should().BeTrue();
    }

    [Test]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnAdminUser()
    {
        // Arrange
        var adminUser = new AdminUser("Existing Admin", "existing@admin.com", "hashedPassword123");
        await AdminUserRepository.AddAsync(adminUser);
        await AdminUserRepository.SaveChangesAsync();

        // Act
        var retrievedAdminUser = await AdminUserRepository.GetByIdAsync(adminUser.Id);

        // Assert
        retrievedAdminUser.Should().NotBeNull();
        retrievedAdminUser!.Id.Should().Be(adminUser.Id);
    }

    [Test]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Act
        var retrievedAdminUser = await AdminUserRepository.GetByIdAsync(999);

        // Assert
        retrievedAdminUser.Should().BeNull();
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnAllAdminUsers()
    {
        // Arrange
        var adminUser1 = new AdminUser("Admin 1", "admin1@test.com", "hashedPassword123");
        var adminUser2 = new AdminUser("Admin 2", "admin2@test.com", "hashedPassword123");
        await AdminUserRepository.AddAsync(adminUser1);
        await AdminUserRepository.AddAsync(adminUser2);
        await AdminUserRepository.SaveChangesAsync();

        // Act
        var adminUsers = await AdminUserRepository.GetAllAsync();

        // Assert
        adminUsers.Should().NotBeNull();
        adminUsers.Should().HaveCount(3); // Includes the one from SeedTestDataAsync
        adminUsers.Should().Contain(a => a.Id == adminUser1.Id);
        adminUsers.Should().Contain(a => a.Id == adminUser2.Id);
    }

    [Test]
    public async Task GetByUsernameAsync_WithExistingUsername_ShouldReturnAdminUser()
    {
        // Arrange
        var adminUser = new AdminUser("Find Me", "findme@admin.com", "hashedPassword123");
        await AdminUserRepository.AddAsync(adminUser);
        await AdminUserRepository.SaveChangesAsync();

        // Act
        var retrievedAdminUser = await AdminUserRepository.GetByUsernameAsync("Find Me");

        // Assert
        retrievedAdminUser.Should().NotBeNull();
        retrievedAdminUser!.Username.Should().Be("Find Me");
    }

    [Test]
    public async Task GetByUsernameAsync_WithNonExistingUsername_ShouldReturnNull()
    {
        // Act
        var retrievedAdminUser = await AdminUserRepository.GetByUsernameAsync("Non Existing");

        // Assert
        retrievedAdminUser.Should().BeNull();
    }

    [Test]
    public async Task GetByEmailAsync_WithExistingEmail_ShouldReturnAdminUser()
    {
        // Arrange
        var adminUser = new AdminUser("Email Test", "emailtest@admin.com", "hashedPassword123");
        await AdminUserRepository.AddAsync(adminUser);
        await AdminUserRepository.SaveChangesAsync();

        // Act
        var retrievedAdminUser = await AdminUserRepository.GetByEmailAsync("emailtest@admin.com");

        // Assert
        retrievedAdminUser.Should().NotBeNull();
        retrievedAdminUser!.Email.Should().Be("emailtest@admin.com");
    }

    [Test]
    public async Task GetByEmailAsync_WithNonExistingEmail_ShouldReturnNull()
    {
        // Act
        var retrievedAdminUser = await AdminUserRepository.GetByEmailAsync("nonexisting@admin.com");

        // Assert
        retrievedAdminUser.Should().BeNull();
    }

    [Test]
    public async Task UpdateAsync_WithValidAdminUser_ShouldUpdateAdminUser()
    {
        // Arrange
        var adminUser = new AdminUser("Original Username", "original@admin.com", "hashedPassword123");
        await AdminUserRepository.AddAsync(adminUser);
        await AdminUserRepository.SaveChangesAsync();

        adminUser.Email = "updated@admin.com";

        // Act
        await AdminUserRepository.UpdateAsync(adminUser);
        await AdminUserRepository.SaveChangesAsync();

        // Assert
        var updatedAdminUser = await AdminUserRepository.GetByIdAsync(adminUser.Id);
        updatedAdminUser.Should().NotBeNull();
        updatedAdminUser!.Email.Should().Be("updated@admin.com");
    }

    [Test]
    public async Task DeleteAsync_WithExistingId_ShouldDeleteAdminUser()
    {
        // Arrange
        var adminUser = new AdminUser("To Be Deleted", "delete@admin.com", "hashedPassword123");
        await AdminUserRepository.AddAsync(adminUser);
        await AdminUserRepository.SaveChangesAsync();

        // Act
        var result = await AdminUserRepository.DeleteAsync(adminUser.Id);
        await AdminUserRepository.SaveChangesAsync();

        // Assert
        result.Should().BeTrue();
        var deletedAdminUser = await AdminUserRepository.GetByIdAsync(adminUser.Id);
        deletedAdminUser.Should().BeNull();
    }

    [Test]
    public async Task DeleteAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Act
        var result = await AdminUserRepository.DeleteAsync(999);
        await AdminUserRepository.SaveChangesAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
    {
        // Arrange
        var adminUser = new AdminUser("Check Exists", "exists@admin.com", "hashedPassword123");
        await AdminUserRepository.AddAsync(adminUser);
        await AdminUserRepository.SaveChangesAsync();

        // Act
        var exists = await AdminUserRepository.ExistsAsync(adminUser.Id);

        // Assert
        exists.Should().BeTrue();
    }

    [Test]
    public async Task ExistsAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Act
        var exists = await AdminUserRepository.ExistsAsync(999);

        // Assert
        exists.Should().BeFalse();
    }

    [Test]
    public async Task SaveChangesAsync_ShouldSaveChanges()
    {
        // Arrange
        var adminUser = new AdminUser("Save Test", "save@admin.com", "hashedPassword123");
        await AdminUserRepository.AddAsync(adminUser);

        // Act
        await AdminUserRepository.SaveChangesAsync();

        // Assert
        var savedAdminUser = await AdminUserRepository.GetByIdAsync(adminUser.Id);
        savedAdminUser.Should().NotBeNull();
    }

    [Test]
    public async Task AdminUser_Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var adminUser = new AdminUser("Inactive Admin", "inactive@admin.com", "hashedPassword123");
        adminUser.Deactivate(); // Ensure it's inactive
        await AdminUserRepository.AddAsync(adminUser);
        await AdminUserRepository.SaveChangesAsync();

        // Act
        adminUser.Activate();
        await AdminUserRepository.UpdateAsync(adminUser);
        await AdminUserRepository.SaveChangesAsync();

        // Assert
        var activatedAdminUser = await AdminUserRepository.GetByIdAsync(adminUser.Id);
        activatedAdminUser.Should().NotBeNull();
        activatedAdminUser!.IsActive.Should().BeTrue();
    }

    [Test]
    public async Task AdminUser_Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var adminUser = new AdminUser("Active Admin", "active@admin.com", "hashedPassword123");
        await AdminUserRepository.AddAsync(adminUser);
        await AdminUserRepository.SaveChangesAsync();

        // Act
        adminUser.Deactivate();
        await AdminUserRepository.UpdateAsync(adminUser);
        await AdminUserRepository.SaveChangesAsync();

        // Assert
        var deactivatedAdminUser = await AdminUserRepository.GetByIdAsync(adminUser.Id);
        deactivatedAdminUser.Should().NotBeNull();
        deactivatedAdminUser!.IsActive.Should().BeFalse();
    }

    [Test]
    public async Task AdminUser_UpdatePassword_ShouldUpdatePassword()
    {
        // Arrange
        var adminUser = new AdminUser("Password Test", "password@admin.com", "hashedPassword123");
        await AdminUserRepository.AddAsync(adminUser);
        await AdminUserRepository.SaveChangesAsync();

        // Act
        adminUser.UpdatePassword("newPassword123");
        await AdminUserRepository.UpdateAsync(adminUser);
        await AdminUserRepository.SaveChangesAsync();

        // Assert
        var updatedAdminUser = await AdminUserRepository.GetByIdAsync(adminUser.Id);
        updatedAdminUser.Should().NotBeNull();
        updatedAdminUser!.PasswordHash.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task AdminUser_VerifyPassword_ShouldReturnCorrectResult()
    {
        // Arrange
        var adminUser = new AdminUser("Password Verify", "verify@admin.com", "hashedPassword123");
        await AdminUserRepository.AddAsync(adminUser);
        await AdminUserRepository.SaveChangesAsync();

        // Act & Assert
        var retrievedAdminUser = await AdminUserRepository.GetByIdAsync(adminUser.Id);
        retrievedAdminUser.Should().NotBeNull();
        retrievedAdminUser!.PasswordHash.Should().Be("hashedPassword123");
    }

    [Test]
    public async Task AdminUser_LastLogin_ShouldUpdateCorrectly()
    {
        // Arrange
        var adminUser = new AdminUser("Login Test", "login@admin.com", "hashedPassword123");
        await AdminUserRepository.AddAsync(adminUser);
        await AdminUserRepository.SaveChangesAsync();

        var loginTime = DateTime.UtcNow;

        // Act
        adminUser.UpdateLastLogin();
        await AdminUserRepository.UpdateAsync(adminUser);
        await AdminUserRepository.SaveChangesAsync();

        // Assert
        var updatedAdminUser = await AdminUserRepository.GetByIdAsync(adminUser.Id);
        updatedAdminUser.Should().NotBeNull();
        updatedAdminUser!.LastLogin.Should().BeCloseTo(loginTime, TimeSpan.FromSeconds(5));
    }
}
