using EasterEggHunt.Application.Services;
using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace EasterEggHunt.Application.Tests.Services;

/// <summary>
/// Tests für den CampaignService
/// </summary>
[TestFixture]
public class CampaignServiceTests
{
    private Mock<ICampaignRepository> _mockRepository = null!;
    private Mock<ILogger<CampaignService>> _mockLogger = null!;
    private CampaignService _campaignService = null!;

    [SetUp]
    public void Setup()
    {
        _mockRepository = new Mock<ICampaignRepository>();
        _mockLogger = new Mock<ILogger<CampaignService>>();
        _campaignService = new CampaignService(_mockRepository.Object, _mockLogger.Object);
    }

    [Test]
    public void CampaignService_Constructor_WithNullRepository_ShouldThrowArgumentNullException()
    {
        // Arrange
        var logger = new Mock<ILogger<CampaignService>>().Object;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new CampaignService(null!, logger));
        Assert.That(ex.ParamName, Is.EqualTo("campaignRepository"));
    }

    [Test]
    public void CampaignService_Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange
        var repository = new Mock<ICampaignRepository>().Object;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new CampaignService(repository, null!));
        Assert.That(ex.ParamName, Is.EqualTo("logger"));
    }

    [Test]
    public void CampaignService_Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var repository = new Mock<ICampaignRepository>().Object;
        var logger = new Mock<ILogger<CampaignService>>().Object;

        // Act
        var service = new CampaignService(repository, logger);

        // Assert
        Assert.That(service, Is.Not.Null);
    }

    [Test]
    public async Task GetActiveCampaignsAsync_ReturnsActiveCampaigns()
    {
        // Arrange
        var campaigns = new List<Campaign>
        {
            new Campaign("Test Campaign 1", "Description 1", "admin"),
            new Campaign("Test Campaign 2", "Description 2", "admin")
        };
        _mockRepository.Setup(r => r.GetActiveAsync()).ReturnsAsync(campaigns);

        // Act
        var result = await _campaignService.GetActiveCampaignsAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(2));
        _mockRepository.Verify(r => r.GetActiveAsync(), Times.Once);
    }

    [Test]
    public async Task GetCampaignByIdAsync_WithValidId_ReturnsCampaign()
    {
        // Arrange
        var campaignId = 1;
        var campaign = new Campaign("Test Campaign", "Description", "admin");
        _mockRepository.Setup(r => r.GetByIdAsync(campaignId)).ReturnsAsync(campaign);

        // Act
        var result = await _campaignService.GetCampaignByIdAsync(campaignId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("Test Campaign"));
        _mockRepository.Verify(r => r.GetByIdAsync(campaignId), Times.Once);
    }

    [Test]
    public async Task GetCampaignByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var campaignId = 999;
        _mockRepository.Setup(r => r.GetByIdAsync(campaignId)).ReturnsAsync((Campaign?)null);

        // Act
        var result = await _campaignService.GetCampaignByIdAsync(campaignId);

        // Assert
        Assert.That(result, Is.Null);
        _mockRepository.Verify(r => r.GetByIdAsync(campaignId), Times.Once);
    }

    [Test]
    public async Task CreateCampaignAsync_WithValidData_ReturnsCreatedCampaign()
    {
        // Arrange
        var name = "New Campaign";
        var description = "New Description";
        var createdBy = "admin";

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Campaign>()))
            .ReturnsAsync((Campaign campaign) => campaign);
        _mockRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _campaignService.CreateCampaignAsync(name, description, createdBy);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo(name));
        Assert.That(result.Description, Is.EqualTo(description));
        Assert.That(result.CreatedBy, Is.EqualTo(createdBy));
        Assert.That(result.IsActive, Is.True);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Campaign>()), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void CreateCampaignAsync_WithEmptyName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() =>
            _campaignService.CreateCampaignAsync("", "Description", "admin"));
    }

    [Test]
    public void CreateCampaignAsync_WithEmptyDescription_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() =>
            _campaignService.CreateCampaignAsync("Name", "", "admin"));
    }

    [Test]
    public void CreateCampaignAsync_WithEmptyCreatedBy_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() =>
            _campaignService.CreateCampaignAsync("Name", "Description", ""));
    }

    [Test]
    public async Task UpdateCampaignAsync_WithValidData_ReturnsTrue()
    {
        // Arrange
        var campaignId = 1;
        var name = "Updated Name";
        var description = "Updated Description";
        var campaign = new Campaign("Original Name", "Original Description", "admin");

        _mockRepository.Setup(r => r.GetByIdAsync(campaignId)).ReturnsAsync(campaign);
        _mockRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _campaignService.UpdateCampaignAsync(campaignId, name, description);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(campaign.Name, Is.EqualTo(name));
        Assert.That(campaign.Description, Is.EqualTo(description));
        _mockRepository.Verify(r => r.GetByIdAsync(campaignId), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task UpdateCampaignAsync_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var campaignId = 999;
        var name = "Updated Name";
        var description = "Updated Description";

        _mockRepository.Setup(r => r.GetByIdAsync(campaignId)).ReturnsAsync((Campaign?)null);

        // Act
        var result = await _campaignService.UpdateCampaignAsync(campaignId, name, description);

        // Assert
        Assert.That(result, Is.False);
        _mockRepository.Verify(r => r.GetByIdAsync(campaignId), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Test]
    public void UpdateCampaignAsync_WithEmptyName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() =>
            _campaignService.UpdateCampaignAsync(1, "", "Description"));
    }

    [Test]
    public void UpdateCampaignAsync_WithEmptyDescription_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() =>
            _campaignService.UpdateCampaignAsync(1, "Name", ""));
    }

    [Test]
    public async Task DeactivateCampaignAsync_WithValidId_ReturnsTrue()
    {
        // Arrange
        var campaignId = 1;
        var campaign = new Campaign("Test Campaign", "Description", "admin");

        _mockRepository.Setup(r => r.GetByIdAsync(campaignId)).ReturnsAsync(campaign);
        _mockRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _campaignService.DeactivateCampaignAsync(campaignId);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(campaign.IsActive, Is.False);
        _mockRepository.Verify(r => r.GetByIdAsync(campaignId), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task DeactivateCampaignAsync_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var campaignId = 999;

        _mockRepository.Setup(r => r.GetByIdAsync(campaignId)).ReturnsAsync((Campaign?)null);

        // Act
        var result = await _campaignService.DeactivateCampaignAsync(campaignId);

        // Assert
        Assert.That(result, Is.False);
        _mockRepository.Verify(r => r.GetByIdAsync(campaignId), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Test]
    public async Task ActivateCampaignAsync_WithValidId_ReturnsTrue()
    {
        // Arrange
        var campaignId = 1;
        var campaign = new Campaign("Test Campaign", "Description", "admin");
        campaign.Deactivate(); // Start with deactivated campaign

        _mockRepository.Setup(r => r.GetByIdAsync(campaignId)).ReturnsAsync(campaign);
        _mockRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _campaignService.ActivateCampaignAsync(campaignId);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(campaign.IsActive, Is.True);
        _mockRepository.Verify(r => r.GetByIdAsync(campaignId), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task ActivateCampaignAsync_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var campaignId = 999;

        _mockRepository.Setup(r => r.GetByIdAsync(campaignId)).ReturnsAsync((Campaign?)null);

        // Act
        var result = await _campaignService.ActivateCampaignAsync(campaignId);

        // Assert
        Assert.That(result, Is.False);
        _mockRepository.Verify(r => r.GetByIdAsync(campaignId), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
}
