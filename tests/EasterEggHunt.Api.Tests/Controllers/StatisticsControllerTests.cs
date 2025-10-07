using EasterEggHunt.Api.Controllers;
using EasterEggHunt.Application.Services;
using EasterEggHunt.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace EasterEggHunt.Api.Tests.Controllers;

/// <summary>
/// Unit Tests für StatisticsController
/// </summary>
[TestFixture]
public class StatisticsControllerTests
{
    private Mock<ICampaignService> _mockCampaignService = null!;
    private Mock<IQrCodeService> _mockQrCodeService = null!;
    private Mock<IUserService> _mockUserService = null!;
    private Mock<IFindService> _mockFindService = null!;
    private Mock<ILogger<StatisticsController>> _mockLogger = null!;
    private StatisticsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockCampaignService = new Mock<ICampaignService>();
        _mockQrCodeService = new Mock<IQrCodeService>();
        _mockUserService = new Mock<IUserService>();
        _mockFindService = new Mock<IFindService>();
        _mockLogger = new Mock<ILogger<StatisticsController>>();
        _controller = new StatisticsController(
            _mockCampaignService.Object,
            _mockQrCodeService.Object,
            _mockUserService.Object,
            _mockFindService.Object,
            _mockLogger.Object);
    }

    [Test]
    public async Task GetSystemOverview_ReturnsOkResult_WithStatistics()
    {
        // Arrange
        var campaigns = new List<Campaign>
        {
            new Campaign("Campaign 1", "Description 1", "Admin"),
            new Campaign("Campaign 2", "Description 2", "Admin")
        };
        var users = new List<User>
        {
            new User("User 1"),
            new User("User 2")
        };

        _mockCampaignService.Setup(x => x.GetActiveCampaignsAsync()).ReturnsAsync(campaigns);
        _mockUserService.Setup(x => x.GetActiveUsersAsync()).ReturnsAsync(users);
        _mockQrCodeService.Setup(x => x.GetQrCodesByCampaignIdAsync(It.IsAny<int>())).ReturnsAsync(new List<QrCode>());
        _mockFindService.Setup(x => x.GetFindCountByQrCodeIdAsync(It.IsAny<int>())).ReturnsAsync(0);

        // Act
        var result = await _controller.GetSystemOverview();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult!.Value, Is.InstanceOf<SystemOverviewStatistics>());
        var statistics = okResult.Value as SystemOverviewStatistics;
        Assert.That(statistics!.TotalCampaigns, Is.EqualTo(2));
        Assert.That(statistics.TotalUsers, Is.EqualTo(2));
    }

    [Test]
    public async Task GetSystemOverview_ReturnsInternalServerError_OnException()
    {
        // Arrange
        _mockCampaignService.Setup(x => x.GetActiveCampaignsAsync()).ThrowsAsync(new InvalidOperationException("Test exception"));

        // Act
        var result = await _controller.GetSystemOverview();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
        Assert.That(objectResult.Value, Is.EqualTo("Interner Serverfehler"));
    }

    [Test]
    public async Task GetCampaignStatistics_ReturnsOkResult_WhenCampaignExists()
    {
        // Arrange
        var campaign = new Campaign("Test Campaign", "Description", "Admin");
        var qrCodes = new List<QrCode>
        {
            new QrCode(1, "QR 1", "Note 1") { Id = 1 },
            new QrCode(1, "QR 2", "Note 2") { Id = 2 }
        };
        var finds = new List<Find>
        {
            new Find(1, 1, "127.0.0.1", "Test Agent"),
            new Find(2, 2, "127.0.0.1", "Test Agent")
        };

        _mockCampaignService.Setup(x => x.GetCampaignByIdAsync(1)).ReturnsAsync(campaign);
        _mockQrCodeService.Setup(x => x.GetQrCodesByCampaignIdAsync(1)).ReturnsAsync(qrCodes);
        _mockFindService.Setup(x => x.GetFindsByQrCodeIdAsync(1)).ReturnsAsync(finds.Take(1));
        _mockFindService.Setup(x => x.GetFindsByQrCodeIdAsync(2)).ReturnsAsync(finds.Skip(1));

        // Act
        var result = await _controller.GetCampaignStatistics(1);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult!.Value, Is.InstanceOf<CampaignStatistics>());
        var statistics = okResult.Value as CampaignStatistics;
        Assert.That(statistics!.CampaignId, Is.EqualTo(1));
        Assert.That(statistics.CampaignName, Is.EqualTo("Test Campaign"));
        Assert.That(statistics.TotalQrCodes, Is.EqualTo(2));
        Assert.That(statistics.TotalFinds, Is.EqualTo(2));
        Assert.That(statistics.UniqueFinders, Is.EqualTo(2));
    }

    [Test]
    public async Task GetCampaignStatistics_ReturnsNotFound_WhenCampaignDoesNotExist()
    {
        // Arrange
        _mockCampaignService.Setup(x => x.GetCampaignByIdAsync(1)).ReturnsAsync((Campaign?)null);

        // Act
        var result = await _controller.GetCampaignStatistics(1);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult!.Value, Is.EqualTo("Kampagne mit ID 1 nicht gefunden"));
    }

    [Test]
    public async Task GetCampaignStatistics_ReturnsInternalServerError_OnException()
    {
        // Arrange
        _mockCampaignService.Setup(x => x.GetCampaignByIdAsync(1)).ThrowsAsync(new InvalidOperationException("Test exception"));

        // Act
        var result = await _controller.GetCampaignStatistics(1);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
        Assert.That(objectResult.Value, Is.EqualTo("Interner Serverfehler"));
    }

    [Test]
    public async Task GetUserStatistics_ReturnsOkResult_WhenUserExists()
    {
        // Arrange
        var user = new User("Test User");
        var finds = new List<Find>
        {
            new Find(1, 1, "127.0.0.1", "Test Agent"),
            new Find(2, 2, "127.0.0.1", "Test Agent")
        };

        _mockUserService.Setup(x => x.GetUserByIdAsync(1)).ReturnsAsync(user);
        _mockFindService.Setup(x => x.GetFindsByUserIdAsync(1)).ReturnsAsync(finds);

        // Act
        var result = await _controller.GetUserStatistics(1);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult!.Value, Is.InstanceOf<UserStatistics>());
        var statistics = okResult.Value as UserStatistics;
        Assert.That(statistics!.UserId, Is.EqualTo(1));
        Assert.That(statistics.UserName, Is.EqualTo("Test User"));
        Assert.That(statistics.TotalFinds, Is.EqualTo(2));
        Assert.That(statistics.UniqueQrCodesFound, Is.EqualTo(2));
    }

    [Test]
    public async Task GetUserStatistics_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        _mockUserService.Setup(x => x.GetUserByIdAsync(1)).ReturnsAsync((User?)null);

        // Act
        var result = await _controller.GetUserStatistics(1);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult!.Value, Is.EqualTo("Benutzer mit ID 1 nicht gefunden"));
    }

    [Test]
    public async Task GetUserStatistics_ReturnsInternalServerError_OnException()
    {
        // Arrange
        _mockUserService.Setup(x => x.GetUserByIdAsync(1)).ThrowsAsync(new InvalidOperationException("Test exception"));

        // Act
        var result = await _controller.GetUserStatistics(1);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
        Assert.That(objectResult.Value, Is.EqualTo("Interner Serverfehler"));
    }

    [Test]
    public async Task GetTopPerformers_ReturnsOkResult_WithTopPerformers()
    {
        // Arrange
        var users = new List<User>
        {
            new User("User 1") { Id = 1 },
            new User("User 2") { Id = 2 }
        };

        _mockUserService.Setup(x => x.GetActiveUsersAsync()).ReturnsAsync(users);
        _mockFindService.Setup(x => x.GetFindCountByUserIdAsync(1)).ReturnsAsync(5);
        _mockFindService.Setup(x => x.GetFindCountByUserIdAsync(2)).ReturnsAsync(3);

        // Act
        var result = await _controller.GetTopPerformers();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult!.Value, Is.InstanceOf<TopPerformersStatistics>());
        var statistics = okResult.Value as TopPerformersStatistics;
        Assert.That(statistics!.TopPerformers.Count, Is.EqualTo(2));
        Assert.That(statistics.TopPerformers[0].FindCount, Is.EqualTo(5));
        Assert.That(statistics.TopPerformers[1].FindCount, Is.EqualTo(3));
    }

    [Test]
    public async Task GetTopPerformers_ReturnsInternalServerError_OnException()
    {
        // Arrange
        _mockUserService.Setup(x => x.GetActiveUsersAsync()).ThrowsAsync(new InvalidOperationException("Test exception"));

        // Act
        var result = await _controller.GetTopPerformers();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
        Assert.That(objectResult.Value, Is.EqualTo("Interner Serverfehler"));
    }
}
