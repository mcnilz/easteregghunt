using EasterEggHunt.Api.Controllers;
using EasterEggHunt.Application.Services;
using EasterEggHunt.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace EasterEggHunt.Api.Tests.Controllers;

/// <summary>
/// Unit Tests für StatisticsController
/// </summary>
[TestFixture]
public class StatisticsControllerTests
{
    private Mock<IStatisticsService> _mockStatisticsService = null!;
    private Mock<ILogger<StatisticsController>> _mockLogger = null!;
    private StatisticsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockStatisticsService = new Mock<IStatisticsService>();
        _mockLogger = new Mock<ILogger<StatisticsController>>();
        _controller = new StatisticsController(
            _mockStatisticsService.Object,
            _mockLogger.Object);
    }

    [Test]
    public async Task GetSystemOverview_ReturnsOkResult_WithStatistics()
    {
        // Arrange
        var expectedStatistics = new SystemOverviewStatistics
        {
            TotalCampaigns = 2,
            ActiveCampaigns = 1,
            TotalQrCodes = 5,
            TotalUsers = 10,
            TotalFinds = 15,
            CompletedFinds = 15,
            GeneratedAt = DateTime.UtcNow
        };

        _mockStatisticsService.Setup(x => x.GetSystemOverviewAsync())
            .ReturnsAsync(expectedStatistics);

        // Act
        var result = await _controller.GetSystemOverview();

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<SystemOverviewStatistics>>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(expectedStatistics));
    }

    [Test]
    public async Task GetCampaignStatistics_ReturnsOkResult_WhenCampaignExists()
    {
        // Arrange
        var campaignId = 1;
        var expectedStatistics = new CampaignStatistics
        {
            CampaignId = campaignId,
            CampaignName = "Test Campaign",
            TotalQrCodes = 5,
            TotalFinds = 10,
            UniqueFinders = 3,
            CompletionRate = 2.0,
            GeneratedAt = DateTime.UtcNow
        };

        _mockStatisticsService.Setup(x => x.GetCampaignStatisticsAsync(campaignId))
            .ReturnsAsync(expectedStatistics);

        // Act
        var result = await _controller.GetCampaignStatistics(campaignId);

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<CampaignStatistics>>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(expectedStatistics));
    }

    [Test]
    public async Task GetCampaignStatistics_ReturnsNotFound_WhenCampaignDoesNotExist()
    {
        // Arrange
        var campaignId = 999;
        _mockStatisticsService.Setup(x => x.GetCampaignStatisticsAsync(campaignId))
            .ThrowsAsync(new ArgumentException($"Kampagne mit ID {campaignId} nicht gefunden"));

        // Act
        var result = await _controller.GetCampaignStatistics(campaignId);

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<CampaignStatistics>>());
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
    }

    [Test]
    public async Task GetUserStatistics_ReturnsOkResult_WhenUserExists()
    {
        // Arrange
        var userId = 1;
        var expectedStatistics = new UserStatistics
        {
            UserId = userId,
            UserName = "Test User",
            TotalFinds = 5,
            UniqueQrCodesFound = 3,
            FirstFindDate = DateTime.UtcNow.AddDays(-7),
            LastFindDate = DateTime.UtcNow,
            GeneratedAt = DateTime.UtcNow
        };

        _mockStatisticsService.Setup(x => x.GetUserStatisticsAsync(userId))
            .ReturnsAsync(expectedStatistics);

        // Act
        var result = await _controller.GetUserStatistics(userId);

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<UserStatistics>>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(expectedStatistics));
    }

    [Test]
    public async Task GetUserStatistics_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = 999;
        _mockStatisticsService.Setup(x => x.GetUserStatisticsAsync(userId))
            .ThrowsAsync(new ArgumentException($"Benutzer mit ID {userId} nicht gefunden"));

        // Act
        var result = await _controller.GetUserStatistics(userId);

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<UserStatistics>>());
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
    }

    [Test]
    public async Task GetQrCodeStatistics_ReturnsOkResult_WhenQrCodeExists()
    {
        // Arrange
        var qrCodeId = 1;
        var expectedStatistics = new QrCodeStatisticsDto
        {
            QrCodeId = qrCodeId,
            QrCodeTitle = "Test QR Code",
            TotalFinds = 3,
            UniqueFinders = 2,
            FirstFindDate = DateTime.UtcNow.AddDays(-5),
            LastFindDate = DateTime.UtcNow,
            RecentFinds = new List<QrCodeStatisticsRecentFind>(),
            GeneratedAt = DateTime.UtcNow
        };

        _mockStatisticsService.Setup(x => x.GetQrCodeStatisticsAsync(qrCodeId))
            .ReturnsAsync(expectedStatistics);

        // Act
        var result = await _controller.GetQrCodeStatistics(qrCodeId);

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<QrCodeStatisticsDto>>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(expectedStatistics));
    }

    [Test]
    public async Task GetQrCodeStatistics_ReturnsNotFound_WhenQrCodeDoesNotExist()
    {
        // Arrange
        var qrCodeId = 999;
        _mockStatisticsService.Setup(x => x.GetQrCodeStatisticsAsync(qrCodeId))
            .ThrowsAsync(new ArgumentException($"QR-Code mit ID {qrCodeId} nicht gefunden"));

        // Act
        var result = await _controller.GetQrCodeStatistics(qrCodeId);

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<QrCodeStatisticsDto>>());
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
    }

    [Test]
    public async Task GetCampaignQrCodeStatistics_ReturnsOkResult_WhenCampaignExists()
    {
        // Arrange
        var campaignId = 1;
        var expectedStatistics = new CampaignQrCodeStatisticsDto
        {
            CampaignId = campaignId,
            CampaignName = "Test Campaign",
            QrCodeStatistics = new List<QrCodeStatisticsDto>(),
            GeneratedAt = DateTime.UtcNow
        };

        _mockStatisticsService.Setup(x => x.GetCampaignQrCodeStatisticsAsync(campaignId))
            .ReturnsAsync(expectedStatistics);

        // Act
        var result = await _controller.GetCampaignQrCodeStatistics(campaignId);

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<CampaignQrCodeStatisticsDto>>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(expectedStatistics));
    }

    [Test]
    public async Task GetCampaignQrCodeStatistics_ReturnsNotFound_WhenCampaignDoesNotExist()
    {
        // Arrange
        var campaignId = 999;
        _mockStatisticsService.Setup(x => x.GetCampaignQrCodeStatisticsAsync(campaignId))
            .ThrowsAsync(new ArgumentException($"Kampagne mit ID {campaignId} nicht gefunden"));

        // Act
        var result = await _controller.GetCampaignQrCodeStatistics(campaignId);

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<CampaignQrCodeStatisticsDto>>());
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
    }

    [Test]
    public async Task GetTopPerformers_ReturnsOkResult_WithStatistics()
    {
        // Arrange
        var expectedStatistics = new TopPerformersStatistics
        {
            TopByTotalFinds = new List<UserStatistics>(),
            TopByUniqueQrCodes = new List<UserStatistics>(),
            MostRecentActivity = new List<UserStatistics>(),
            GeneratedAt = DateTime.UtcNow
        };

        _mockStatisticsService.Setup(x => x.GetTopPerformersAsync())
            .ReturnsAsync(expectedStatistics);

        // Act
        var result = await _controller.GetTopPerformers();

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<TopPerformersStatistics>>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(expectedStatistics));
    }
}
