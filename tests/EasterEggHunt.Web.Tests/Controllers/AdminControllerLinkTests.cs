using EasterEggHunt.Web.Controllers;
using EasterEggHunt.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace EasterEggHunt.Web.Tests.Controllers;

/// <summary>
/// Tests für AdminController Links und Navigation
/// </summary>
[TestFixture]
public class AdminControllerLinkTests : IDisposable
{
    private Mock<ICampaignManagementService> _mockCampaignService = null!;
    private Mock<IQrCodeManagementService> _mockQrCodeService = null!;
    private Mock<IStatisticsDisplayService> _mockStatisticsService = null!;
    private Mock<IPrintLayoutService> _mockPrintService = null!;
    private Mock<ILogger<AdminController>> _mockLogger = null!;
    private AdminController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockCampaignService = new Mock<ICampaignManagementService>();
        _mockQrCodeService = new Mock<IQrCodeManagementService>();
        _mockStatisticsService = new Mock<IStatisticsDisplayService>();
        _mockPrintService = new Mock<IPrintLayoutService>();
        _mockLogger = new Mock<ILogger<AdminController>>();

        _controller = new AdminController(
            _mockCampaignService.Object,
            _mockQrCodeService.Object,
            _mockStatisticsService.Object,
            _mockPrintService.Object,
            _mockLogger.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
    }

    [Test]
    public async Task CampaignDetails_WithValidId_ReturnsView()
    {
        // Arrange
        var campaignId = 1;
        var campaign = new EasterEggHunt.Domain.Entities.Campaign
        {
            Id = campaignId,
            Name = "Test Campaign",
            Description = "Test Description",
            IsActive = true
        };

        var statistics = new Models.CampaignQrCodeStatisticsViewModel
        {
            CampaignId = campaignId,
            CampaignName = "Test Campaign",
            TotalFinds = 5
        };

        _mockCampaignService.Setup(x => x.GetCampaignByIdAsync(campaignId))
            .ReturnsAsync(campaign);
        _mockQrCodeService.Setup(x => x.GetQrCodesByCampaignAsync(campaignId))
            .ReturnsAsync(new List<EasterEggHunt.Domain.Entities.QrCode>());
        _mockStatisticsService.Setup(x => x.GetCampaignStatisticsAsync(campaignId))
            .ReturnsAsync(statistics);

        // Act
        var result = await _controller.CampaignDetails(campaignId);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult?.ViewName, Is.Null); // Sollte Standard-View verwenden
        Assert.That(viewResult?.Model, Is.InstanceOf<Models.CampaignDetailsViewModel>());

        var viewModel = viewResult?.Model as Models.CampaignDetailsViewModel;
        Assert.That(viewModel?.Campaign.Id, Is.EqualTo(campaignId));
        Assert.That(viewModel?.TotalFinds, Is.EqualTo(5));
    }

    [Test]
    public async Task CampaignDetails_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidCampaignId = 99999;
        _mockCampaignService.Setup(x => x.GetCampaignByIdAsync(invalidCampaignId))
            .ReturnsAsync((EasterEggHunt.Domain.Entities.Campaign?)null);

        // Act
        var result = await _controller.CampaignDetails(invalidCampaignId);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task CampaignDetails_WithServiceException_ReturnsErrorView()
    {
        // Arrange
        var campaignId = 1;
        _mockCampaignService.Setup(x => x.GetCampaignByIdAsync(campaignId))
            .ThrowsAsync(new HttpRequestException("Service error"));

        // Act
        var result = await _controller.CampaignDetails(campaignId);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult?.ViewName, Is.EqualTo("Error"));
    }

    [Test]
    public async Task Index_ReturnsDashboardView()
    {
        // Arrange
        var dashboardStats = new Models.AdminDashboardViewModel(new List<EasterEggHunt.Domain.Entities.Campaign>());
        _mockStatisticsService.Setup(x => x.BuildDashboardStatisticsAsync())
            .ReturnsAsync(dashboardStats);

        // Act
        var result = await _controller.Index();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult?.ViewName, Is.Null); // Standard-View
        Assert.That(viewResult?.Model, Is.InstanceOf<Models.AdminDashboardViewModel>());
    }

    [Test]
    public async Task Campaigns_ReturnsCampaignsView()
    {
        // Arrange
        var campaigns = new List<EasterEggHunt.Domain.Entities.Campaign>
        {
            new() { Id = 1, Name = "Campaign 1" },
            new() { Id = 2, Name = "Campaign 2" }
        };
        _mockCampaignService.Setup(x => x.GetActiveCampaignsAsync())
            .ReturnsAsync(campaigns);

        // Act
        var result = await _controller.Campaigns();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult?.ViewName, Is.Null); // Standard-View
        Assert.That(viewResult?.Model, Is.InstanceOf<List<EasterEggHunt.Domain.Entities.Campaign>>());
    }

    [Test]
    public void CreateCampaign_ReturnsCreateView()
    {
        // Act
        var result = _controller.CreateCampaign();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult?.ViewName, Is.Null); // Standard-View
        Assert.That(viewResult?.Model, Is.InstanceOf<Models.CreateCampaignRequest>());
    }

    [Test]
    public async Task QrCodes_WithValidCampaignId_ReturnsView()
    {
        // Arrange
        var campaignId = 1;
        var campaign = new EasterEggHunt.Domain.Entities.Campaign
        {
            Id = campaignId,
            Name = "Test Campaign"
        };
        _mockCampaignService.Setup(x => x.GetCampaignByIdAsync(campaignId))
            .ReturnsAsync(campaign);

        // Act
        var result = await _controller.QrCodes(campaignId);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult?.ViewName, Is.Null); // Standard-View
    }

    [Test]
    public async Task QrCodes_WithInvalidCampaignId_ReturnsNotFound()
    {
        // Arrange
        var invalidCampaignId = 99999;
        _mockCampaignService.Setup(x => x.GetCampaignByIdAsync(invalidCampaignId))
            .ReturnsAsync((EasterEggHunt.Domain.Entities.Campaign?)null);

        // Act
        var result = await _controller.QrCodes(invalidCampaignId);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task CampaignStatistics_WithValidId_ReturnsView()
    {
        // Arrange
        var campaignId = 1;
        var statistics = new Models.CampaignQrCodeStatisticsViewModel
        {
            CampaignId = campaignId,
            CampaignName = "Test Campaign"
        };
        _mockStatisticsService.Setup(x => x.GetCampaignStatisticsAsync(campaignId))
            .ReturnsAsync(statistics);

        // Act
        var result = await _controller.CampaignStatistics(campaignId);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult?.ViewName, Is.Null); // Standard-View
        Assert.That(viewResult?.Model, Is.InstanceOf<Models.CampaignQrCodeStatisticsViewModel>());
    }

    [Test]
    public async Task QrCodeStatistics_WithValidId_ReturnsView()
    {
        // Arrange
        var qrCodeId = 1;
        var statistics = new Models.QrCodeStatisticsViewModel
        {
            QrCodeId = qrCodeId,
            Title = "Test QR Code"
        };
        _mockStatisticsService.Setup(x => x.GetQrCodeStatisticsAsync(qrCodeId))
            .ReturnsAsync(statistics);

        // Act
        var result = await _controller.QrCodeStatistics(qrCodeId);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult?.ViewName, Is.Null); // Standard-View
        Assert.That(viewResult?.Model, Is.InstanceOf<Models.QrCodeStatisticsViewModel>());
    }

    [Test]
    public async Task AllAdminActions_ReturnCorrectActionResultTypes()
    {
        // Arrange
        var campaignId = 1;
        var qrCodeId = 1;

        var campaign = new EasterEggHunt.Domain.Entities.Campaign { Id = campaignId, Name = "Test" };
        var dashboardStats = new Models.AdminDashboardViewModel(new List<EasterEggHunt.Domain.Entities.Campaign>());
        var campaignStats = new Models.CampaignQrCodeStatisticsViewModel();
        var qrCodeStats = new Models.QrCodeStatisticsViewModel();

        _mockCampaignService.Setup(x => x.GetCampaignByIdAsync(campaignId)).ReturnsAsync(campaign);
        _mockCampaignService.Setup(x => x.GetCampaignWithQrCodesAsync(campaignId)).ReturnsAsync(campaign);
        _mockStatisticsService.Setup(x => x.BuildDashboardStatisticsAsync()).ReturnsAsync(dashboardStats);
        _mockStatisticsService.Setup(x => x.GetCampaignStatisticsAsync(campaignId)).ReturnsAsync(campaignStats);
        _mockStatisticsService.Setup(x => x.GetQrCodeStatisticsAsync(qrCodeId)).ReturnsAsync(qrCodeStats);

        // Act & Assert
        var indexResult = await _controller.Index();
        Assert.That(indexResult, Is.InstanceOf<ViewResult>());

        var campaignsResult = await _controller.Campaigns();
        Assert.That(campaignsResult, Is.InstanceOf<ViewResult>());

        var createResult = _controller.CreateCampaign();
        Assert.That(createResult, Is.InstanceOf<ViewResult>());

        var detailsResult = await _controller.CampaignDetails(campaignId);
        Assert.That(detailsResult, Is.InstanceOf<ViewResult>());

        var qrCodesResult = await _controller.QrCodes(campaignId);
        Assert.That(qrCodesResult, Is.InstanceOf<ViewResult>());

        var campaignStatsResult = await _controller.CampaignStatistics(campaignId);
        Assert.That(campaignStatsResult, Is.InstanceOf<ViewResult>());

        var qrCodeStatsResult = await _controller.QrCodeStatistics(qrCodeId);
        Assert.That(qrCodeStatsResult, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public async Task AdminController_HandlesServiceExceptions_Gracefully()
    {
        // Arrange
        var campaignId = 1;
        _mockCampaignService.Setup(x => x.GetCampaignByIdAsync(campaignId))
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        // Act
        var result = await _controller.CampaignDetails(campaignId);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult?.ViewName, Is.EqualTo("Error"));
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _controller?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
