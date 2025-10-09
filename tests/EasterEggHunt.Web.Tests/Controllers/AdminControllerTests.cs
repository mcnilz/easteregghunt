using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Web.Controllers;
using EasterEggHunt.Web.Models;
using EasterEggHunt.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace EasterEggHunt.Web.Tests.Controllers;

/// <summary>
/// Tests für AdminController mit API-Client
/// </summary>
[TestFixture]
public sealed class AdminControllerTests : IDisposable
{
    private Mock<IEasterEggHuntApiClient> _mockApiClient = null!;
    private Mock<ILogger<AdminController>> _mockLogger = null!;
    private AdminController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockApiClient = new Mock<IEasterEggHuntApiClient>();
        _mockLogger = new Mock<ILogger<AdminController>>();

        _controller = new AdminController(
            _mockApiClient.Object,
            _mockLogger.Object);
    }

    [Test]
    public async Task Index_ReturnsViewWithDashboardData()
    {
        // Arrange
        var campaigns = new List<Campaign>
        {
            new Campaign("Test Campaign 1", "Description 1", "Admin") { Id = 1, IsActive = true },
            new Campaign("Test Campaign 2", "Description 2", "Admin") { Id = 2, IsActive = false }
        };

        var users = new List<User>
        {
            new User("User 1") { Id = 1 },
            new User("User 2") { Id = 2 }
        };

        var qrCodes = new List<QrCode>
        {
            new QrCode(1, "QR Code 1", "Description 1", "Note 1") { Id = 1, IsActive = true },
            new QrCode(1, "QR Code 2", "Description 2", "Note 2") { Id = 2, IsActive = false }
        };

        var finds = new List<Find>
        {
            new Find(1, 1, "127.0.0.1", "Test Agent") { Id = 1, FoundAt = DateTime.UtcNow },
            new Find(2, 2, "127.0.0.1", "Test Agent") { Id = 2, FoundAt = DateTime.UtcNow.AddMinutes(-5) }
        };

        _mockApiClient.Setup(x => x.GetActiveCampaignsAsync())
            .ReturnsAsync(campaigns);
        _mockApiClient.Setup(x => x.GetActiveUsersAsync())
            .ReturnsAsync(users);
        _mockApiClient.Setup(x => x.GetQrCodesByCampaignIdAsync(It.IsAny<int>()))
            .ReturnsAsync(qrCodes);
        _mockApiClient.Setup(x => x.GetFindsByQrCodeIdAsync(It.IsAny<int>()))
            .ReturnsAsync(finds);

        // Act
        var result = await _controller.Index();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = (ViewResult)result;
        Assert.That(viewResult.Model, Is.Not.Null);
        Assert.That(viewResult.Model, Is.InstanceOf<AdminDashboardViewModel>());

        var viewModel = (AdminDashboardViewModel)viewResult.Model!;
        Assert.That(viewModel.Campaigns.Count(), Is.EqualTo(2));
        Assert.That(viewModel.TotalUsers, Is.EqualTo(2));
        Assert.That(viewModel.TotalQrCodes, Is.EqualTo(4)); // 2 campaigns * 2 qr codes each
        Assert.That(viewModel.TotalFinds, Is.EqualTo(8)); // 2 campaigns * 2 qr codes * 2 finds each
        Assert.That(viewModel.ActiveCampaigns, Is.EqualTo(1));
        Assert.That(viewModel.ActiveQrCodes, Is.EqualTo(2));
    }

    [Test]
    public async Task CampaignDetails_WithValidId_ReturnsViewWithCampaignData()
    {
        // Arrange
        var campaignId = 1;
        var campaign = new Campaign("Test Campaign", "Test Description", "Admin")
        {
            Id = campaignId,
            IsActive = true
        };

        var qrCodes = new List<QrCode>
        {
            new QrCode(campaignId, "QR Code 1", "Description 1", "Note 1") { Id = 1, IsActive = true },
            new QrCode(campaignId, "QR Code 2", "Description 2", "Note 2") { Id = 2, IsActive = false }
        };

        var finds = new List<Find>
        {
            new Find(1, 1, "127.0.0.1", "Test Agent") { Id = 1, FoundAt = DateTime.UtcNow },
            new Find(2, 2, "127.0.0.1", "Test Agent") { Id = 2, FoundAt = DateTime.UtcNow.AddMinutes(-5) }
        };

        _mockApiClient.Setup(x => x.GetCampaignByIdAsync(campaignId))
            .ReturnsAsync(campaign);
        _mockApiClient.Setup(x => x.GetQrCodesByCampaignIdAsync(campaignId))
            .ReturnsAsync(qrCodes);
        _mockApiClient.Setup(x => x.GetFindsByQrCodeIdAsync(It.IsAny<int>()))
            .ReturnsAsync(finds);

        // Act
        var result = await _controller.CampaignDetails(campaignId);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = (ViewResult)result;
        Assert.That(viewResult.Model, Is.Not.Null);
        Assert.That(viewResult.Model, Is.InstanceOf<CampaignDetailsViewModel>());

        var viewModel = (CampaignDetailsViewModel)viewResult.Model!;
        Assert.That(viewModel.Campaign, Is.EqualTo(campaign));
        Assert.That(viewModel.QrCodes.Count(), Is.EqualTo(2));
        Assert.That(viewModel.TotalFinds, Is.EqualTo(4)); // 2 qr codes * 2 finds each
        Assert.That(viewModel.UniqueFinders, Is.EqualTo(2)); // 2 unique users
    }

    [Test]
    public async Task CampaignDetails_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var campaignId = 999;
        _mockApiClient.Setup(x => x.GetCampaignByIdAsync(campaignId))
            .ReturnsAsync((Campaign?)null);

        // Act
        var result = await _controller.CampaignDetails(campaignId);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task CreateCampaign_WithValidModel_ReturnsRedirectToCampaignDetails()
    {
        // Arrange
        var viewModel = new CreateCampaignViewModel
        {
            Name = "New Campaign",
            Description = "New Description"
        };

        var createdCampaign = new Campaign(viewModel.Name, viewModel.Description, "Admin")
        {
            Id = 1,
            IsActive = true
        };

        _mockApiClient.Setup(x => x.CreateCampaignAsync(
            viewModel.Name,
            viewModel.Description,
            "Admin"))
            .ReturnsAsync(createdCampaign);

        // Act
        var result = await _controller.CreateCampaign(viewModel);

        // Assert
        Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
        var redirectResult = (RedirectToActionResult)result;
        Assert.That(redirectResult.ActionName, Is.EqualTo(nameof(AdminController.CampaignDetails)));
        Assert.That(redirectResult.RouteValues?["id"], Is.EqualTo(createdCampaign.Id));
    }

    [Test]
    public async Task CreateCampaign_WithInvalidModel_ReturnsViewWithModel()
    {
        // Arrange
        var viewModel = new CreateCampaignViewModel
        {
            Name = "", // Invalid - empty name
            Description = "Valid Description"
        };

        _controller.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await _controller.CreateCampaign(viewModel);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = (ViewResult)result;
        Assert.That(viewResult.Model, Is.EqualTo(viewModel));
    }

    [Test]
    public async Task Users_ReturnsViewWithUserData()
    {
        // Arrange
        var users = new List<User>
        {
            new User("User 1") { Id = 1 },
            new User("User 2") { Id = 2 }
        };

        _mockApiClient.Setup(x => x.GetActiveUsersAsync())
            .ReturnsAsync(users);
        _mockApiClient.Setup(x => x.GetFindCountByUserIdAsync(It.IsAny<int>()))
            .ReturnsAsync(5);

        // Act
        var result = await _controller.Users();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = (ViewResult)result;
        Assert.That(viewResult.Model, Is.Not.Null);
        Assert.That(viewResult.Model, Is.InstanceOf<List<UserViewModel>>());

        var viewModel = (List<UserViewModel>)viewResult.Model!;
        Assert.That(viewModel.Count, Is.EqualTo(2));
        Assert.That(viewModel[0].User, Is.EqualTo(users[0]));
        Assert.That(viewModel[0].FindCount, Is.EqualTo(5));
    }

    [Test]
    public async Task Statistics_ReturnsViewWithStatisticsData()
    {
        // Arrange
        var campaigns = new List<Campaign>
        {
            new Campaign("Campaign 1", "Description 1", "Admin") { Id = 1, IsActive = true },
            new Campaign("Campaign 2", "Description 2", "Admin") { Id = 2, IsActive = false }
        };

        var users = new List<User>
        {
            new User("User 1") { Id = 1, IsActive = true },
            new User("User 2") { Id = 2, IsActive = false }
        };

        var qrCodes = new List<QrCode>
        {
            new QrCode(1, "QR Code 1", "Description 1", "Note 1") { Id = 1, IsActive = true },
            new QrCode(1, "QR Code 2", "Description 2", "Note 2") { Id = 2, IsActive = false }
        };

        var finds = new List<Find>
        {
            new Find(1, 1, "127.0.0.1", "Test Agent") { Id = 1, FoundAt = DateTime.UtcNow },
            new Find(2, 2, "127.0.0.1", "Test Agent") { Id = 2, FoundAt = DateTime.UtcNow.AddMinutes(-5) }
        };

        _mockApiClient.Setup(x => x.GetActiveCampaignsAsync())
            .ReturnsAsync(campaigns);
        _mockApiClient.Setup(x => x.GetActiveUsersAsync())
            .ReturnsAsync(users);
        _mockApiClient.Setup(x => x.GetQrCodesByCampaignIdAsync(It.IsAny<int>()))
            .ReturnsAsync(qrCodes);
        _mockApiClient.Setup(x => x.GetFindsByQrCodeIdAsync(It.IsAny<int>()))
            .ReturnsAsync(finds);

        // Act
        var result = await _controller.Statistics();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = (ViewResult)result;
        Assert.That(viewResult.Model, Is.Not.Null);
        Assert.That(viewResult.Model, Is.InstanceOf<StatisticsViewModel>());

        var viewModel = (StatisticsViewModel)viewResult.Model!;
        Assert.That(viewModel.TotalCampaigns, Is.EqualTo(2));
        Assert.That(viewModel.ActiveCampaigns, Is.EqualTo(1));
        Assert.That(viewModel.TotalUsers, Is.EqualTo(2));
        Assert.That(viewModel.ActiveUsers, Is.EqualTo(1));
        Assert.That(viewModel.TotalQrCodes, Is.EqualTo(4)); // 2 campaigns * 2 qr codes each
        Assert.That(viewModel.TotalFinds, Is.EqualTo(8)); // 2 campaigns * 2 qr codes * 2 finds each
    }

    #region QR-Code Druckfunktionalität Tests

    [Test]
    public async Task PrintQrCodes_WithValidCampaignId_ReturnsViewWithPrintData()
    {
        // Arrange
        var campaignId = 1;
        var campaign = new Campaign("Test Campaign", "Test Description", "Admin")
        {
            Id = campaignId,
            IsActive = true
        };

        var qrCodes = new List<QrCode>
        {
            new QrCode(campaignId, "QR Code 1", "Description 1", "Note 1") { Id = 1, IsActive = true, SortOrder = 1 },
            new QrCode(campaignId, "QR Code 2", "Description 2", "Note 2") { Id = 2, IsActive = true, SortOrder = 2 },
            new QrCode(campaignId, "QR Code 3", "Description 3", "Note 3") { Id = 3, IsActive = false, SortOrder = 3 }
        };

        _mockApiClient.Setup(x => x.GetCampaignByIdAsync(campaignId))
            .ReturnsAsync(campaign);
        _mockApiClient.Setup(x => x.GetQrCodesByCampaignIdAsync(campaignId))
            .ReturnsAsync(qrCodes);

        // Act
        var result = await _controller.PrintQrCodes(campaignId);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = (ViewResult)result;
        Assert.That(viewResult.Model, Is.Not.Null);
        Assert.That(viewResult.Model, Is.InstanceOf<PrintQrCodesViewModel>());

        var viewModel = (PrintQrCodesViewModel)viewResult.Model!;
        Assert.That(viewModel.Campaign, Is.EqualTo(campaign));
        Assert.That(viewModel.QrCodes.Count(), Is.EqualTo(3));
        Assert.That(viewModel.Size, Is.EqualTo(200)); // Default size
        Assert.That(viewModel.ShowTitles, Is.True); // Default show titles
    }

    [Test]
    public async Task PrintQrCodes_WithSpecificQrCodeIds_ReturnsFilteredQrCodes()
    {
        // Arrange
        var campaignId = 1;
        var campaign = new Campaign("Test Campaign", "Test Description", "Admin")
        {
            Id = campaignId,
            IsActive = true
        };

        var qrCodes = new List<QrCode>
        {
            new QrCode(campaignId, "QR Code 1", "Description 1", "Note 1") { Id = 1, IsActive = true, SortOrder = 1 },
            new QrCode(campaignId, "QR Code 2", "Description 2", "Note 2") { Id = 2, IsActive = true, SortOrder = 2 },
            new QrCode(campaignId, "QR Code 3", "Description 3", "Note 3") { Id = 3, IsActive = true, SortOrder = 3 }
        };

        _mockApiClient.Setup(x => x.GetCampaignByIdAsync(campaignId))
            .ReturnsAsync(campaign);
        _mockApiClient.Setup(x => x.GetQrCodesByCampaignIdAsync(campaignId))
            .ReturnsAsync(qrCodes);

        // Act
        var result = await _controller.PrintQrCodes(campaignId, "1,3");

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = (ViewResult)result;
        var viewModel = (PrintQrCodesViewModel)viewResult.Model!;

        Assert.That(viewModel.QrCodes.Count(), Is.EqualTo(2));
        Assert.That(viewModel.QrCodes.Any(q => q.Id == 1), Is.True);
        Assert.That(viewModel.QrCodes.Any(q => q.Id == 3), Is.True);
        Assert.That(viewModel.QrCodes.Any(q => q.Id == 2), Is.False);
    }

    [Test]
    public async Task PrintQrCodes_WithCustomSizeAndShowTitles_ReturnsCorrectSettings()
    {
        // Arrange
        var campaignId = 1;
        var campaign = new Campaign("Test Campaign", "Test Description", "Admin")
        {
            Id = campaignId,
            IsActive = true
        };

        var qrCodes = new List<QrCode>
        {
            new QrCode(campaignId, "QR Code 1", "Description 1", "Note 1") { Id = 1, IsActive = true }
        };

        _mockApiClient.Setup(x => x.GetCampaignByIdAsync(campaignId))
            .ReturnsAsync(campaign);
        _mockApiClient.Setup(x => x.GetQrCodesByCampaignIdAsync(campaignId))
            .ReturnsAsync(qrCodes);

        // Act
        var result = await _controller.PrintQrCodes(campaignId, null, 300, false);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = (ViewResult)result;
        var viewModel = (PrintQrCodesViewModel)viewResult.Model!;

        Assert.That(viewModel.Size, Is.EqualTo(300));
        Assert.That(viewModel.ShowTitles, Is.False);
    }

    [Test]
    public async Task PrintQrCodes_WithInvalidCampaignId_ReturnsNotFound()
    {
        // Arrange
        var campaignId = 999;
        _mockApiClient.Setup(x => x.GetCampaignByIdAsync(campaignId))
            .ReturnsAsync((Campaign?)null);

        // Act
        var result = await _controller.PrintQrCodes(campaignId);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task PrintQrCodes_WithInvalidQrCodeIds_ReturnsAllQrCodes()
    {
        // Arrange
        var campaignId = 1;
        var campaign = new Campaign("Test Campaign", "Test Description", "Admin")
        {
            Id = campaignId,
            IsActive = true
        };

        var qrCodes = new List<QrCode>
        {
            new QrCode(campaignId, "QR Code 1", "Description 1", "Note 1") { Id = 1, IsActive = true },
            new QrCode(campaignId, "QR Code 2", "Description 2", "Note 2") { Id = 2, IsActive = true }
        };

        _mockApiClient.Setup(x => x.GetCampaignByIdAsync(campaignId))
            .ReturnsAsync(campaign);
        _mockApiClient.Setup(x => x.GetQrCodesByCampaignIdAsync(campaignId))
            .ReturnsAsync(qrCodes);

        // Act - Invalid QR-Code IDs (non-existent)
        var result = await _controller.PrintQrCodes(campaignId, "999,1000");

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = (ViewResult)result;
        var viewModel = (PrintQrCodesViewModel)viewResult.Model!;

        // Should return all QR-Codes when invalid IDs are provided (no matches found)
        Assert.That(viewModel.QrCodes.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task PrintQrCodes_WithSizeLimits_ClampsSizeCorrectly()
    {
        // Arrange
        var campaignId = 1;
        var campaign = new Campaign("Test Campaign", "Test Description", "Admin")
        {
            Id = campaignId,
            IsActive = true
        };

        var qrCodes = new List<QrCode>
        {
            new QrCode(campaignId, "QR Code 1", "Description 1", "Note 1") { Id = 1, IsActive = true }
        };

        _mockApiClient.Setup(x => x.GetCampaignByIdAsync(campaignId))
            .ReturnsAsync(campaign);
        _mockApiClient.Setup(x => x.GetQrCodesByCampaignIdAsync(campaignId))
            .ReturnsAsync(qrCodes);

        // Act - Size too small (should be clamped to 50)
        var resultSmall = await _controller.PrintQrCodes(campaignId, null, 25);
        var viewModelSmall = (PrintQrCodesViewModel)((ViewResult)resultSmall).Model!;

        // Act - Size too large (should be clamped to 500)
        var resultLarge = await _controller.PrintQrCodes(campaignId, null, 600);
        var viewModelLarge = (PrintQrCodesViewModel)((ViewResult)resultLarge).Model!;

        // Assert
        Assert.That(viewModelSmall.Size, Is.EqualTo(50));
        Assert.That(viewModelLarge.Size, Is.EqualTo(500));
    }

    [Test]
    public async Task PrintQrCodes_WithEmptyQrCodeIds_ReturnsAllQrCodes()
    {
        // Arrange
        var campaignId = 1;
        var campaign = new Campaign("Test Campaign", "Test Description", "Admin")
        {
            Id = campaignId,
            IsActive = true
        };

        var qrCodes = new List<QrCode>
        {
            new QrCode(campaignId, "QR Code 1", "Description 1", "Note 1") { Id = 1, IsActive = true },
            new QrCode(campaignId, "QR Code 2", "Description 2", "Note 2") { Id = 2, IsActive = true }
        };

        _mockApiClient.Setup(x => x.GetCampaignByIdAsync(campaignId))
            .ReturnsAsync(campaign);
        _mockApiClient.Setup(x => x.GetQrCodesByCampaignIdAsync(campaignId))
            .ReturnsAsync(qrCodes);

        // Act - Empty QR-Code IDs
        var result = await _controller.PrintQrCodes(campaignId, "");

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = (ViewResult)result;
        var viewModel = (PrintQrCodesViewModel)viewResult.Model!;

        // Should return all QR-Codes when empty string is provided
        Assert.That(viewModel.QrCodes.Count(), Is.EqualTo(2));
    }

    #endregion

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
    }

    public void Dispose()
    {
        _controller?.Dispose();
        GC.SuppressFinalize(this);
    }
}
