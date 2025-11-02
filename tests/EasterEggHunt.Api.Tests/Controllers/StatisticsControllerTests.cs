using EasterEggHunt.Api.Controllers;
using EasterEggHunt.Application.Services;
using EasterEggHunt.Domain.Models;
using EasterEggHunterApi.Abstractions.Models.Statistics;
using Microsoft.AspNetCore.Http;
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

    /// <summary>
    /// Testet den erfolgreichen Happy-Path für den API-Endpoint.
    /// Wichtig, da dieser Endpoint von der Web-Anwendung aufgerufen wird.
    /// Stellt sicher, dass die korrekten HTTP-Status-Codes (200 OK) zurückgegeben werden
    /// und die Datenstruktur korrekt serialisiert werden kann (JSON).
    /// </summary>
    [Test]
    public async Task GetTimeBasedStatistics_ReturnsOkResult_WithStatistics()
    {
        // Arrange
        var expectedStatistics = new TimeBasedStatistics
        {
            DailyStatistics = new List<TimeSeriesStatistics>
            {
                new TimeSeriesStatistics { Date = DateTime.UtcNow.Date, Count = 5, UniqueFinders = 3, UniqueQrCodes = 2 }
            },
            WeeklyStatistics = new List<TimeSeriesStatistics>
            {
                new TimeSeriesStatistics { Date = DateTime.UtcNow.Date, Count = 10, UniqueFinders = 5, UniqueQrCodes = 4 }
            },
            MonthlyStatistics = new List<TimeSeriesStatistics>
            {
                new TimeSeriesStatistics { Date = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1), Count = 20, UniqueFinders = 8, UniqueQrCodes = 6 }
            },
            GeneratedAt = DateTime.UtcNow
        };

        _mockStatisticsService.Setup(x => x.GetTimeBasedStatisticsAsync(null, null))
            .ReturnsAsync(expectedStatistics);

        // Act
        var result = await _controller.GetTimeBasedStatistics(null, null);

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<TimeBasedStatistics>>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.Value, Is.EqualTo(expectedStatistics));
    }

    /// <summary>
    /// Testet, dass Query-Parameter korrekt an den Service weitergegeben werden.
    /// Wichtig, da die Web-Anwendung Datumsfilter verwendet. Verhindert, dass Filter ignoriert werden
    /// und stellt sicher, dass alle drei Zeitreihen (täglich, wöchentlich, monatlich) gefiltert werden.
    /// </summary>
    [Test]
    public async Task GetTimeBasedStatistics_WithDateFilters_ShouldPassFiltersToService()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;
        var expectedStatistics = new TimeBasedStatistics
        {
            DailyStatistics = new List<TimeSeriesStatistics>(),
            WeeklyStatistics = new List<TimeSeriesStatistics>(),
            MonthlyStatistics = new List<TimeSeriesStatistics>(),
            GeneratedAt = DateTime.UtcNow
        };

        _mockStatisticsService.Setup(x => x.GetTimeBasedStatisticsAsync(startDate, endDate))
            .ReturnsAsync(expectedStatistics);

        // Act
        var result = await _controller.GetTimeBasedStatistics(startDate, endDate);

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<TimeBasedStatistics>>());
        _mockStatisticsService.Verify(x => x.GetTimeBasedStatisticsAsync(startDate, endDate), Times.Once);
    }

    /// <summary>
    /// Testet die Validierung von ungültigen Datumsbereichen.
    /// Wichtig, um sicherzustellen, dass ungültige Eingaben (z.B. StartDate > EndDate)
    /// nicht zu fehlerhaften Datenbank-Queries führen. Verhindert potenzielle Performance-Probleme
    /// und stellt benutzerfreundliche Fehlermeldungen sicher (400 Bad Request statt 500).
    /// </summary>
    [Test]
    public async Task GetTimeBasedStatistics_WithInvalidDateRange_ShouldReturnBadRequest()
    {
        // Arrange
        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow.AddDays(-1); // StartDate nach EndDate

        // Act
        var result = await _controller.GetTimeBasedStatistics(startDate, endDate);

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<TimeBasedStatistics>>());
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult!.Value, Is.EqualTo("Startdatum darf nicht nach Enddatum liegen"));
    }

    /// <summary>
    /// Testet die Fehlerbehandlung bei Service-Exceptions.
    /// Wichtig, da dieser Endpoint mehrere Repository-Calls ausführt (täglich, wöchentlich, monatlich).
    /// Stellt sicher, dass Fehler korrekt abgefangen werden und als 500 Internal Server Error
    /// zurückgegeben werden, nicht als ungehandelte Exceptions. Die verbesserte Exception-Behandlung
    /// ermöglicht detailliertes Logging für besseres Debugging in Production.
    /// </summary>
    [Test]
    public async Task GetTimeBasedStatistics_WhenServiceThrowsException_ShouldReturnInternalServerError()
    {
        // Arrange
        _mockStatisticsService.Setup(x => x.GetTimeBasedStatisticsAsync(null, null))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _controller.GetTimeBasedStatistics(null, null);

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<TimeBasedStatistics>>());
        var statusResult = result.Result as ObjectResult;
        Assert.That(statusResult, Is.Not.Null);
        Assert.That(statusResult!.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    #region GetFindHistory Tests

    /// <summary>
    /// Testet den erfolgreichen Happy-Path für den Fund-Historie-Endpoint.
    /// Wichtig, da dieser Endpoint von der Web-Anwendung aufgerufen wird.
    /// Stellt sicher, dass die korrekten HTTP-Status-Codes (200 OK) zurückgegeben werden
    /// und die Datenstruktur korrekt serialisiert werden kann (JSON).
    /// </summary>
    [Test]
    public async Task GetFindHistory_ReturnsOkResult_WithFindHistory()
    {
        // Arrange
        var testFinds = new List<Domain.Entities.Find>
        {
            new Domain.Entities.Find(1, 1, "127.0.0.1", "User Agent 1"),
            new Domain.Entities.Find(2, 1, "192.168.1.1", "User Agent 2")
        };

        _mockStatisticsService.Setup(x => x.GetFindHistoryAsync(
            It.IsAny<DateTime?>(),
            It.IsAny<DateTime?>(),
            It.IsAny<int?>(),
            It.IsAny<int?>(),
            It.IsAny<int?>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<string>()))
            .ReturnsAsync((testFinds, 2));

        // Act
        var result = await _controller.GetFindHistory();

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<FindHistoryResponse>>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var response = okResult!.Value as FindHistoryResponse;
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Finds, Has.Count.EqualTo(2));
        Assert.That(response.TotalCount, Is.EqualTo(2));
    }

    /// <summary>
    /// Testet, dass alle Filter-Parameter korrekt an den Service weitergegeben werden.
    /// Wichtig, da die Web-Anwendung verschiedene Filter verwendet.
    /// Verhindert, dass Filter ignoriert werden und stellt sicher, dass alle Parameter korrekt verarbeitet werden.
    /// </summary>
    [Test]
    public async Task GetFindHistory_WithAllFilters_ShouldPassFiltersToService()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;
        var userId = 1;
        var qrCodeId = 2;
        var campaignId = 3;
        var skip = 10;
        var take = 25;
        var sortBy = "UserId";
        var sortDirection = "asc";

        _mockStatisticsService.Setup(x => x.GetFindHistoryAsync(
            It.IsAny<DateTime?>(),
            It.IsAny<DateTime?>(),
            It.IsAny<int?>(),
            It.IsAny<int?>(),
            It.IsAny<int?>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<string>()))
            .ReturnsAsync((new List<Domain.Entities.Find>(), 0));

        // Act
        await _controller.GetFindHistory(startDate, endDate, userId, qrCodeId, campaignId, skip, take, sortBy, sortDirection);

        // Assert
        _mockStatisticsService.Verify(x => x.GetFindHistoryAsync(
            startDate, endDate, userId, qrCodeId, campaignId, skip, take, sortBy, sortDirection), Times.Once);
    }

    /// <summary>
    /// Testet die Validierung von ungültigen Datumsbereichen.
    /// Wichtig, um sicherzustellen, dass ungültige Eingaben (z.B. StartDate > EndDate)
    /// nicht zu fehlerhaften Datenbank-Queries führen. Verhindert potenzielle Performance-Probleme
    /// und stellt benutzerfreundliche Fehlermeldungen sicher (400 Bad Request statt 500).
    /// </summary>
    [Test]
    public async Task GetFindHistory_WithInvalidDateRange_ShouldReturnBadRequest()
    {
        // Arrange
        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow.AddDays(-1); // StartDate nach EndDate

        // Act
        var result = await _controller.GetFindHistory(startDate, endDate);

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<FindHistoryResponse>>());
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult!.Value, Is.EqualTo("Startdatum darf nicht nach Enddatum liegen"));
    }

    /// <summary>
    /// Testet die Validierung von ungültigen Pagination-Parametern.
    /// Wichtig, um sicherzustellen, dass ungültige Eingaben (z.B. negativer Skip oder Take > 100)
    /// nicht zu fehlerhaften Datenbank-Queries führen. Verhindert potenzielle Performance-Probleme.
    /// </summary>
    [Test]
    public async Task GetFindHistory_WithInvalidPagination_ShouldReturnBadRequest()
    {
        // Arrange
        // Test mit negativem Skip
        var result1 = await _controller.GetFindHistory(skip: -1);
        var badRequest1 = result1.Result as BadRequestObjectResult;
        Assert.That(badRequest1, Is.Not.Null);
        Assert.That(badRequest1!.Value, Is.EqualTo("Skip darf nicht negativ sein"));

        // Test mit Take > 100
        var result2 = await _controller.GetFindHistory(take: 101);
        var badRequest2 = result2.Result as BadRequestObjectResult;
        Assert.That(badRequest2, Is.Not.Null);
        Assert.That(badRequest2!.Value, Is.EqualTo("Take muss zwischen 1 und 100 liegen"));

        // Test mit Take < 1
        var result3 = await _controller.GetFindHistory(take: 0);
        var badRequest3 = result3.Result as BadRequestObjectResult;
        Assert.That(badRequest3, Is.Not.Null);
        Assert.That(badRequest3!.Value, Is.EqualTo("Take muss zwischen 1 und 100 liegen"));
    }

    /// <summary>
    /// Testet die Fehlerbehandlung bei Service-Exceptions.
    /// Wichtig, da dieser Endpoint mehrere Repository-Calls ausführt.
    /// Stellt sicher, dass Fehler korrekt abgefangen werden und als 500 Internal Server Error
    /// zurückgegeben werden, nicht als ungehandelte Exceptions.
    /// </summary>
    [Test]
    public async Task GetFindHistory_WhenServiceThrowsException_ShouldReturnInternalServerError()
    {
        // Arrange
        _mockStatisticsService.Setup(x => x.GetFindHistoryAsync(
            It.IsAny<DateTime?>(),
            It.IsAny<DateTime?>(),
            It.IsAny<int?>(),
            It.IsAny<int?>(),
            It.IsAny<int?>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _controller.GetFindHistory();

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<FindHistoryResponse>>());
        var statusResult = result.Result as ObjectResult;
        Assert.That(statusResult, Is.Not.Null);
        Assert.That(statusResult!.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    #endregion
}
