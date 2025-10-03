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
        campaigns.Should().HaveCount(1);
        campaigns.First().Name.Should().Be("Test Kampagne 2025");
        campaigns.First().Description.Should().Be("Eine Test-Kampagne für Integration Tests");
        campaigns.First().CreatedBy.Should().Be("TestAdmin");
        campaigns.First().IsActive.Should().BeTrue();
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
        activeCampaigns.Should().HaveCount(1);
        activeCampaigns.First().Name.Should().Be("Test Kampagne 2025");
        activeCampaigns.First().IsActive.Should().BeTrue();
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
        campaign.Should().NotBeNull();
        campaign!.Name.Should().Be("Test Kampagne 2025");
        campaign.Description.Should().Be("Eine Test-Kampagne für Integration Tests");
        campaign.CreatedBy.Should().Be("TestAdmin");
        campaign.QrCodes.Should().HaveCount(2);
    }

    [Test]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Act
        var campaign = await CampaignRepository.GetByIdAsync(999);

        // Assert
        campaign.Should().BeNull();
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
        updatedCampaign.Should().NotBeNull();
        updatedCampaign.Name.Should().Be("Aktualisierte Kampagne");
        updatedCampaign.Description.Should().Be("Aktualisierte Beschreibung");
        updatedCampaign.UpdatedAt.Should().BeAfter(originalUpdatedAt);

        // Verify in database
        var retrievedCampaign = await CampaignRepository.GetByIdAsync(campaign.Id);
        retrievedCampaign!.Name.Should().Be("Aktualisierte Kampagne");
        retrievedCampaign.Description.Should().Be("Aktualisierte Beschreibung");
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
        result.Should().BeTrue();

        // Verify in database
        var campaignsAfterDelete = await CampaignRepository.GetAllAsync();
        campaignsAfterDelete.Should().BeEmpty();

        var deletedCampaign = await CampaignRepository.GetByIdAsync(campaignId);
        deletedCampaign.Should().BeNull();
    }

    [Test]
    public async Task DeleteAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Act
        var result = await CampaignRepository.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();

        // Verify database unchanged
        var campaigns = await CampaignRepository.GetAllAsync();
        campaigns.Should().HaveCount(1);
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
        exists.Should().BeTrue();
    }

    [Test]
    public async Task ExistsAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Act
        var exists = await CampaignRepository.ExistsAsync(999);

        // Assert
        exists.Should().BeFalse();
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
        changesCount.Should().BeGreaterThan(0);

        // Verify changes persisted
        var updatedCampaign = await CampaignRepository.GetByIdAsync(campaign.Id);
        updatedCampaign!.Name.Should().Be("Test Update");
        updatedCampaign.Description.Should().Be("Test Beschreibung");
        updatedCampaign.UpdatedAt.Should().BeAfter(originalUpdatedAt);
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
        campaignWithQrCodes.Should().NotBeNull();
        campaignWithQrCodes!.QrCodes.Should().HaveCount(2);
        campaignWithQrCodes.QrCodes.Should().Contain(q => q.Title == "QR Code 1");
        campaignWithQrCodes.QrCodes.Should().Contain(q => q.Title == "QR Code 2");
    }
}
