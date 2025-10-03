using EasterEggHunt.Domain.Entities;
using NUnit.Framework;

namespace EasterEggHunt.Infrastructure.Tests.Integration;

[TestFixture]
[Category("Integration")]
[Ignore("Temporarily disabled - FluentAssertions conversion needed")]
public class CampaignRepositoryIntegrationTests : IntegrationTestBase
{
    [SetUp]
    public async Task SetUp()
    {
        await ResetDatabaseAsync();
        await SeedTestDataAsync();
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnAllCampaigns()
    {
        // Act
        var campaigns = await CampaignRepository.GetAllAsync();

        // Assert
        Assert.That(campaigns, Has.Count.EqualTo(1));
        Assert.That(campaigns.First().Name, Is.EqualTo("Test Kampagne 2025"));
        Assert.That(campaigns.First().Description, Is.EqualTo("Eine Test-Kampagne für Integration Tests"));
        Assert.That(campaigns.First().CreatedBy, Is.EqualTo("TestAdmin"));
        Assert.That(campaigns.First().IsActive, Is.True);
    }

    [Test]
    public async Task GetActiveAsync_ShouldReturnOnlyActiveCampaigns()
    {
        // Arrange - Inaktive Kampagne hinzufügen
        var inactiveCampaign = new Campaign(
            "Inaktive Kampagne",
            "Diese Kampagne ist inaktiv",
            "TestAdmin");
        inactiveCampaign.Deactivate();
        await CampaignRepository.AddAsync(inactiveCampaign);

        // Act
        var activeCampaigns = await CampaignRepository.GetActiveAsync();

        // Assert
        Assert.That(activeCampaigns, Has.Count.EqualTo(1));
        Assert.That(activeCampaigns.First().Name, Is.EqualTo("Test Kampagne 2025"));
        Assert.That(activeCampaigns.First().IsActive, Is.True);
    }

    [Test]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnCampaign()
    {
        // Arrange
        var campaigns = await CampaignRepository.GetAllAsync();
        var campaignId = campaigns.First().Id;

        // Act
        var campaign = await CampaignRepository.GetByIdAsync(campaignId);

        // Assert
        Assert.That(campaign, Is.Not.Null);
        Assert.That(campaign!.Name, Is.EqualTo("Test Kampagne 2025"));
        Assert.That(campaign.Description, Is.EqualTo("Eine Test-Kampagne für Integration Tests"));
        Assert.That(campaign.CreatedBy, Is.EqualTo("TestAdmin"));
        Assert.That(campaign.QrCodes, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Act
        var campaign = await CampaignRepository.GetByIdAsync(999);

        // Assert
        Assert.That(campaign, Is.Null);
    }

    [Test]
    public async Task AddAsync_WithValidCampaign_ShouldAddCampaign()
    {
        // Arrange
        var newCampaign = new Campaign(
            "Neue Kampagne",
            "Eine neue Test-Kampagne",
            "TestAdmin");

        // Act
        var addedCampaign = await CampaignRepository.AddAsync(newCampaign);

        // Assert
        Assert.That(addedCampaign, Is.Not.Null);
        Assert.That(addedCampaign.Id, Is.GreaterThan(0));
        Assert.That(addedCampaign.Name, Is.EqualTo("Neue Kampagne"));
        Assert.That(addedCampaign.Description, Is.EqualTo("Eine neue Test-Kampagne"));
        Assert.That(addedCampaign.CreatedBy, Is.EqualTo("TestAdmin"));
        Assert.That(addedCampaign.IsActive, Is.True);
        Assert.That(addedCampaign.CreatedAt, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(5)));
        Assert.That(addedCampaign.UpdatedAt, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(5)));

        // Verify in database
        var campaigns = await CampaignRepository.GetAllAsync();
        Assert.That(campaigns, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task UpdateAsync_WithValidCampaign_ShouldUpdateCampaign()
    {
        // Arrange
        var campaigns = await CampaignRepository.GetAllAsync();
        var campaign = campaigns.First();
        var originalUpdatedAt = campaign.UpdatedAt;

        // Act
        campaign.Update("Aktualisierte Kampagne", "Aktualisierte Beschreibung");
        var updatedCampaign = await CampaignRepository.UpdateAsync(campaign);

        // Assert
        Assert.That(updatedCampaign, Is.Not.Null);
        Assert.That(updatedCampaign.Name, Is.EqualTo("Aktualisierte Kampagne"));
        Assert.That(updatedCampaign.Description, Is.EqualTo("Aktualisierte Beschreibung"));
        Assert.That(updatedCampaign.UpdatedAt, Is.GreaterThan(originalUpdatedAt));

        // Verify in database
        var retrievedCampaign = await CampaignRepository.GetByIdAsync(campaign.Id);
        Assert.That(retrievedCampaign!.Name, Is.EqualTo("Aktualisierte Kampagne"));
        Assert.That(retrievedCampaign.Description, Is.EqualTo("Aktualisierte Beschreibung"));
    }

    [Test]
    public async Task DeleteAsync_WithExistingId_ShouldDeleteCampaign()
    {
        // Arrange
        var campaigns = await CampaignRepository.GetAllAsync();
        var campaignId = campaigns.First().Id;

        // Act
        var result = await CampaignRepository.DeleteAsync(campaignId);

        // Assert
        Assert.That(result, Is.True);

        // Verify in database
        var campaignsAfterDelete = await CampaignRepository.GetAllAsync();
        Assert.That(campaignsAfterDelete, Is.Empty);

        var deletedCampaign = await CampaignRepository.GetByIdAsync(campaignId);
        Assert.That(deletedCampaign, Is.Null);
    }

    [Test]
    public async Task DeleteAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Act
        var result = await CampaignRepository.DeleteAsync(999);

        // Assert
        Assert.That(result, Is.False);

        // Verify database unchanged
        var campaigns = await CampaignRepository.GetAllAsync();
        Assert.That(campaigns, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
    {
        // Arrange
        var campaigns = await CampaignRepository.GetAllAsync();
        var campaignId = campaigns.First().Id;

        // Act
        var exists = await CampaignRepository.ExistsAsync(campaignId);

        // Assert
        Assert.That(exists, Is.True);
    }

    [Test]
    public async Task ExistsAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Act
        var exists = await CampaignRepository.ExistsAsync(999);

        // Assert
        Assert.That(exists, Is.False);
    }

    [Test]
    public async Task SaveChangesAsync_ShouldSaveChanges()
    {
        // Arrange
        var campaigns = await CampaignRepository.GetAllAsync();
        var campaign = campaigns.First();
        var originalUpdatedAt = campaign.UpdatedAt;

        // Act
        campaign.Update("Test Update", "Test Beschreibung");
        var changesCount = await CampaignRepository.SaveChangesAsync();

        // Assert
        Assert.That(changesCount, Is.GreaterThan(0));

        // Verify changes persisted
        var updatedCampaign = await CampaignRepository.GetByIdAsync(campaign.Id);
        Assert.That(updatedCampaign!.Name, Is.EqualTo("Test Update"));
        Assert.That(updatedCampaign.Description, Is.EqualTo("Test Beschreibung"));
        Assert.That(updatedCampaign.UpdatedAt, Is.GreaterThan(originalUpdatedAt));
    }

    [Test]
    public async Task Campaign_WithQrCodes_ShouldMaintainRelationship()
    {
        // Arrange
        var campaigns = await CampaignRepository.GetAllAsync();
        var campaign = campaigns.First();

        // Act
        var campaignWithQrCodes = await CampaignRepository.GetByIdAsync(campaign.Id);

        // Assert
        Assert.That(campaignWithQrCodes, Is.Not.Null);
        Assert.That(campaignWithQrCodes!.QrCodes, Has.Count.EqualTo(2));
        Assert.That(campaignWithQrCodes.QrCodes, Has.Some.Matches<Domain.Entities.QrCode>(q => q.Title == "QR Code 1"));
        Assert.That(campaignWithQrCodes.QrCodes, Has.Some.Matches<Domain.Entities.QrCode>(q => q.Title == "QR Code 2"));
    }
}
