using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Web.Models;
using EasterEggHunt.Web.Services;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace EasterEggHunt.Web.Tests.Services;

/// <summary>
/// Unit Tests für StatisticsDisplayService
/// </summary>
[TestFixture]
public class StatisticsDisplayServiceTests
{
    private Mock<IEasterEggHuntApiClient> _mockApiClient = null!;
    private Mock<ILogger<StatisticsDisplayService>> _mockLogger = null!;
    private StatisticsDisplayService _service = null!;

    [SetUp]
    public void Setup()
    {
        _mockApiClient = new Mock<IEasterEggHuntApiClient>();
        _mockLogger = new Mock<ILogger<StatisticsDisplayService>>();
        _service = new StatisticsDisplayService(_mockApiClient.Object, _mockLogger.Object);
    }

    #region GetStatisticsAsync Tests

    /// <summary>
    /// Testet GetStatisticsAsync mit Performance-Metriken.
    /// Wichtig, da diese Methode Performance-Tracking implementiert.
    /// Stellt sicher, dass Response-Zeiten korrekt gemessen werden.
    /// </summary>
    [Test]
    public async Task GetStatisticsAsync_WithPerformanceTracking_ShouldIncludePerformanceMetrics()
    {
        // Arrange
        var campaigns = new List<Campaign>
        {
            new Campaign("Test Campaign", "Description", "Admin") { Id = 1 }
        };
        var users = new List<User>
        {
            new User("Test User") { Id = 1 }
        };
        var qrCodes = new List<QrCode>
        {
            new QrCode(1, "QR Code", "Description", "Notes") { Id = 1 }
        };
        var qrCodeStats = new QrCodeStatisticsViewModel
        {
            QrCodeId = 1,
            Title = "QR Code",
            FindCount = 0
        };

        _mockApiClient.Setup(x => x.GetActiveCampaignsAsync())
            .ReturnsAsync(campaigns);
        _mockApiClient.Setup(x => x.GetActiveUsersAsync())
            .ReturnsAsync(users);
        _mockApiClient.Setup(x => x.GetQrCodesByCampaignIdAsync(1))
            .ReturnsAsync(qrCodes);
        _mockApiClient.Setup(x => x.GetQrCodeStatisticsAsync(1))
            .ReturnsAsync(qrCodeStats);

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.PerformanceMetrics, Is.Not.Null);
        Assert.That(result.PerformanceMetrics!.ApiCallsLastMinute, Is.GreaterThan(0));
        Assert.That(result.PerformanceMetrics.AverageResponseTimeMs, Is.GreaterThanOrEqualTo(0));
        Assert.That(result.PerformanceMetrics.SlowestResponseTimeMs, Is.GreaterThanOrEqualTo(0));
        Assert.That(result.PerformanceMetrics.FastestResponseTimeMs, Is.GreaterThanOrEqualTo(0));
        Assert.That(result.PerformanceMetrics.LastUpdated, Is.LessThanOrEqualTo(DateTime.UtcNow));
    }

    /// <summary>
    /// Testet GetStatisticsAsync mit mehreren API-Calls.
    /// Wichtig, um sicherzustellen, dass alle API-Calls in den Performance-Metriken erfasst werden.
    /// Verhindert, dass Performance-Daten unvollständig sind.
    /// </summary>
    [Test]
    public async Task GetStatisticsAsync_WithMultipleApiCalls_ShouldTrackAllCalls()
    {
        // Arrange
        var campaigns = new List<Campaign>
        {
            new Campaign("Campaign 1", "Description", "Admin") { Id = 1 },
            new Campaign("Campaign 2", "Description", "Admin") { Id = 2 }
        };
        var users = new List<User>
        {
            new User("User 1") { Id = 1 },
            new User("User 2") { Id = 2 }
        };
        var qrCodes1 = new List<QrCode>
        {
            new QrCode(1, "QR 1", "Description", "Notes") { Id = 1 },
            new QrCode(1, "QR 2", "Description", "Notes") { Id = 2 }
        };
        var qrCodes2 = new List<QrCode>
        {
            new QrCode(2, "QR 3", "Description", "Notes") { Id = 3 }
        };
        var qrCodeStats1 = new QrCodeStatisticsViewModel { QrCodeId = 1, Title = "QR 1", FindCount = 1 };
        var qrCodeStats2 = new QrCodeStatisticsViewModel { QrCodeId = 2, Title = "QR 2", FindCount = 0 };
        var qrCodeStats3 = new QrCodeStatisticsViewModel { QrCodeId = 3, Title = "QR 3", FindCount = 2 };

        _mockApiClient.Setup(x => x.GetActiveCampaignsAsync())
            .ReturnsAsync(campaigns);
        _mockApiClient.Setup(x => x.GetActiveUsersAsync())
            .ReturnsAsync(users);
        _mockApiClient.Setup(x => x.GetQrCodesByCampaignIdAsync(1))
            .ReturnsAsync(qrCodes1);
        _mockApiClient.Setup(x => x.GetQrCodesByCampaignIdAsync(2))
            .ReturnsAsync(qrCodes2);
        _mockApiClient.Setup(x => x.GetQrCodeStatisticsAsync(1))
            .ReturnsAsync(qrCodeStats1);
        _mockApiClient.Setup(x => x.GetQrCodeStatisticsAsync(2))
            .ReturnsAsync(qrCodeStats2);
        _mockApiClient.Setup(x => x.GetQrCodeStatisticsAsync(3))
            .ReturnsAsync(qrCodeStats3);

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.PerformanceMetrics, Is.Not.Null);
        // Sollte mindestens 1 Campaigns + 1 Users + 2 QrCodes + 3 Statistics = 7 API-Calls sein
        // (2 Campaigns = 2 GetQrCodesByCampaignIdAsync calls + 3 GetQrCodeStatisticsAsync calls = 5, + 1 GetActiveCampaignsAsync + 1 GetActiveUsersAsync = 7)
        Assert.That(result.PerformanceMetrics!.ApiCallsLastMinute, Is.GreaterThanOrEqualTo(7));
        Assert.That(result.PerformanceMetrics.AverageResponseTimeMs, Is.GreaterThanOrEqualTo(0));
    }

    /// <summary>
    /// Testet GetStatisticsAsync mit langsamen API-Calls.
    /// Wichtig, um sicherzustellen, dass langsame Requests korrekt erkannt werden.
    /// Verhindert, dass Performance-Probleme übersehen werden.
    /// </summary>
    [Test]
    public async Task GetStatisticsAsync_WithSlowApiCalls_ShouldDetectSlowRequests()
    {
        // Arrange
        var campaigns = new List<Campaign>
        {
            new Campaign("Test Campaign", "Description", "Admin") { Id = 1 }
        };
        var users = new List<User>();
        var qrCodes = new List<QrCode>
        {
            new QrCode(1, "QR Code", "Description", "Notes") { Id = 1 }
        };
        var qrCodeStats = new QrCodeStatisticsViewModel
        {
            QrCodeId = 1,
            Title = "QR Code",
            FindCount = 0
        };

        _mockApiClient.Setup(x => x.GetActiveCampaignsAsync())
            .Returns(async () =>
            {
                await Task.Delay(1100); // > 1s
                return (IEnumerable<Campaign>)campaigns;
            });
        _mockApiClient.Setup(x => x.GetActiveUsersAsync())
            .ReturnsAsync(users);
        _mockApiClient.Setup(x => x.GetQrCodesByCampaignIdAsync(1))
            .ReturnsAsync(qrCodes);
        _mockApiClient.Setup(x => x.GetQrCodeStatisticsAsync(1))
            .ReturnsAsync(qrCodeStats);

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.PerformanceMetrics, Is.Not.Null);
        Assert.That(result.PerformanceMetrics!.SlowRequestsCount, Is.GreaterThan(0));
        Assert.That(result.PerformanceMetrics.SlowestResponseTimeMs, Is.GreaterThanOrEqualTo(1000));
    }

    #endregion
}

