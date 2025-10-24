using EasterEggHunt.Domain.Entities;
using NUnit.Framework;

namespace EasterEggHunt.Integration.Tests.Repositories;

/// <summary>
/// Integration Tests für Campaign Repository mit echter SQLite-Datenbank
/// </summary>
[TestFixture]
public class CampaignRepositoryIntegrationTests : IntegrationTestBase
{
    [SetUp]
    public void Setup()
    {
        // Test-Daten werden bereits in IntegrationTestBase geladen
    }

    [Test]
    public void GetAllAsync_ReturnsAllCampaigns()
    {
        // Act
        var campaigns = Context.Campaigns.ToList();

        // Assert
        Assert.That(campaigns, Is.Not.Empty);
        Assert.That(campaigns.Count, Is.GreaterThanOrEqualTo(1));
    }

    [Test]
    public void GetActiveAsync_ReturnsOnlyActiveCampaigns()
    {
        // Act
        var activeCampaigns = Context.Campaigns.Where(c => c.IsActive).ToList();

        // Assert
        Assert.That(activeCampaigns, Is.Not.Empty);
        Assert.That(activeCampaigns.All(c => c.IsActive), Is.True);
    }

    [Test]
    public void GetByIdAsync_WithValidId_ReturnsCampaign()
    {
        // Arrange
        var campaignId = 1;

        // Act
        var campaign = Context.Campaigns.FirstOrDefault(c => c.Id == campaignId);

        // Assert
        Assert.That(campaign, Is.Not.Null);
        Assert.That(campaign!.Id, Is.EqualTo(campaignId));
        Assert.That(campaign.Name, Is.EqualTo("Test Kampagne"));
    }

    [Test]
    public void GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var campaignId = 999;

        // Act
        var campaign = Context.Campaigns.FirstOrDefault(c => c.Id == campaignId);

        // Assert
        Assert.That(campaign, Is.Null);
    }

    [Test]
    public async Task AddAsync_WithValidData_CreatesCampaign()
    {
        // Arrange
        var uniqueId = Random.Shared.Next(1000, 9999);
        var newCampaign = new Campaign("Test Campaign", "Test Description", "Test Admin")
        {
            Id = uniqueId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        Context.Campaigns.Add(newCampaign);
        await Context.SaveChangesAsync();

        // Assert
        var createdCampaign = Context.Campaigns.FirstOrDefault(c => c.Id == uniqueId);
        Assert.That(createdCampaign, Is.Not.Null);
        Assert.That(createdCampaign!.Name, Is.EqualTo("Test Campaign"));
        Assert.That(createdCampaign.Description, Is.EqualTo("Test Description"));
        Assert.That(createdCampaign.CreatedBy, Is.EqualTo("Test Admin"));
    }

    [Test]
    public async Task UpdateAsync_WithValidData_UpdatesCampaign()
    {
        // Arrange
        var uniqueId = Random.Shared.Next(1000, 9999);
        var campaign = new Campaign("Test Campaign", "Test Description", "Test Admin")
        {
            Id = uniqueId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        Context.Campaigns.Add(campaign);
        await Context.SaveChangesAsync();

        // Act
        campaign.Update("Updated Campaign", "Updated Description");
        await Context.SaveChangesAsync();

        // Assert
        var updatedCampaign = Context.Campaigns.FirstOrDefault(c => c.Id == uniqueId);
        Assert.That(updatedCampaign, Is.Not.Null);
        Assert.That(updatedCampaign!.Name, Is.EqualTo("Updated Campaign"));
        Assert.That(updatedCampaign.Description, Is.EqualTo("Updated Description"));
    }

    [Test]
    public async Task DeleteAsync_WithValidId_DeletesCampaign()
    {
        // Arrange
        var uniqueId = Random.Shared.Next(1000, 9999);
        var campaign = new Campaign("Test Campaign", "Test Description", "Test Admin")
        {
            Id = uniqueId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        Context.Campaigns.Add(campaign);
        await Context.SaveChangesAsync();

        // Act
        Context.Campaigns.Remove(campaign);
        await Context.SaveChangesAsync();

        // Assert
        var deletedCampaign = Context.Campaigns.FirstOrDefault(c => c.Id == uniqueId);
        Assert.That(deletedCampaign, Is.Null);
    }

    [Test]
    public void ExistsAsync_WithValidId_ReturnsTrue()
    {
        // Arrange
        var campaignId = 1;

        // Act
        var exists = Context.Campaigns.Any(c => c.Id == campaignId);

        // Assert
        Assert.That(exists, Is.True);
    }

    [Test]
    public void ExistsAsync_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var campaignId = 999;

        // Act
        var exists = Context.Campaigns.Any(c => c.Id == campaignId);

        // Assert
        Assert.That(exists, Is.False);
    }

    [Test]
    public void Campaign_WithValidData_HasCorrectProperties()
    {
        // Arrange & Act
        var campaign = Context.Campaigns.FirstOrDefault(c => c.Id == 1);

        // Assert
        Assert.That(campaign, Is.Not.Null);
        Assert.That(campaign!.Name, Is.EqualTo("Test Kampagne"));
        Assert.That(campaign.Description, Is.EqualTo("Test Beschreibung"));
        Assert.That(campaign.CreatedBy, Is.EqualTo("Test Admin"));
        Assert.That(campaign.IsActive, Is.True);
        Assert.That(campaign.CreatedAt, Is.Not.EqualTo(DateTime.MinValue));
    }

    [Test]
    public async Task ActivateCampaign_WithValidId_ActivatesCampaign()
    {
        // Arrange
        var uniqueId = Random.Shared.Next(1000, 9999);
        var campaign = new Campaign("Test Campaign", "Test Description", "Test Admin")
        {
            Id = uniqueId,
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };
        Context.Campaigns.Add(campaign);
        await Context.SaveChangesAsync();

        // Act
        campaign.Activate();
        await Context.SaveChangesAsync();

        // Assert
        var activatedCampaign = Context.Campaigns.FirstOrDefault(c => c.Id == uniqueId);
        Assert.That(activatedCampaign, Is.Not.Null);
        Assert.That(activatedCampaign!.IsActive, Is.True);
    }

    [Test]
    public async Task DeactivateCampaign_WithValidId_DeactivatesCampaign()
    {
        // Arrange
        var uniqueId = Random.Shared.Next(1000, 9999);
        var campaign = new Campaign("Test Campaign", "Test Description", "Test Admin")
        {
            Id = uniqueId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        Context.Campaigns.Add(campaign);
        await Context.SaveChangesAsync();

        // Act
        campaign.Deactivate();
        await Context.SaveChangesAsync();

        // Assert
        var deactivatedCampaign = Context.Campaigns.FirstOrDefault(c => c.Id == uniqueId);
        Assert.That(deactivatedCampaign, Is.Not.Null);
        Assert.That(deactivatedCampaign!.IsActive, Is.False);
    }
}
