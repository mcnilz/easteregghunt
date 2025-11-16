using System.Globalization;
using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Web.Controllers;
using EasterEggHunt.Web.Models;
using EasterEggHunt.Web.Services;
using EasterEggHunterApi.Abstractions.Models.Campaign;
using EasterEggHunterApi.Abstractions.Models.QrCode;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace EasterEggHunt.Web.Tests.Controllers;

/// <summary>
/// Tests für AdminController mit neuen Services
/// </summary>
[TestFixture]
public sealed class AdminControllerTests : IDisposable
{
    private Mock<ICampaignManagementService> _mockCampaignService = null!;
    private Mock<IQrCodeManagementService> _mockQrCodeService = null!;
    private Mock<IStatisticsDisplayService> _mockStatisticsService = null!;
    private Mock<IPrintLayoutService> _mockPrintService = null!;
    private Mock<IEasterEggHuntApiClient> _mockApiClient = null!;
    private Mock<ILogger<AdminController>> _mockLogger = null!;
    private AdminController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockCampaignService = new Mock<ICampaignManagementService>();
        _mockQrCodeService = new Mock<IQrCodeManagementService>();
        _mockStatisticsService = new Mock<IStatisticsDisplayService>();
        _mockPrintService = new Mock<IPrintLayoutService>();
        _mockApiClient = new Mock<IEasterEggHuntApiClient>();
        _mockLogger = new Mock<ILogger<AdminController>>();

        _controller = new AdminController(
            _mockCampaignService.Object,
            _mockQrCodeService.Object,
            _mockStatisticsService.Object,
            _mockPrintService.Object,
            _mockApiClient.Object,
            _mockLogger.Object);

        // Initialize TempData for controller
        var mockTempDataProvider = new Mock<ITempDataProvider>();
        var tempData = new TempDataDictionary(new DefaultHttpContext(), mockTempDataProvider.Object);
        _controller.TempData = tempData;
    }

    [Test]
    public async Task Index_ReturnsViewWithDashboardData()
    {
        // Arrange
        var dashboardViewModel = new AdminDashboardViewModel(new List<Campaign>())
        {
            TotalUsers = 5,
            ActiveCampaigns = 2,
            TotalQrCodes = 10,
            ActiveQrCodes = 8,
            TotalFinds = 15,
            RecentActivities = new List<RecentActivityViewModel>()
        };

        _mockStatisticsService.Setup(x => x.BuildDashboardStatisticsAsync())
            .ReturnsAsync(dashboardViewModel);

        // Act
        var result = await _controller.Index();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.Model, Is.EqualTo(dashboardViewModel));
    }

    [Test]
    public async Task Index_ReturnsErrorView_WhenExceptionOccurs()
    {
        // Arrange
        _mockStatisticsService.Setup(x => x.BuildDashboardStatisticsAsync())
            .ThrowsAsync(new HttpRequestException("API Error"));

        // Act
        var result = await _controller.Index();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.ViewName, Is.EqualTo("Error"));
    }

    [Test]
    public async Task Campaigns_ReturnsViewWithCampaigns()
    {
        // Arrange
        var campaigns = new List<Campaign>
        {
            new Campaign("Test Campaign 1", "Description 1", "Admin") { Id = 1, IsActive = true },
            new Campaign("Test Campaign 2", "Description 2", "Admin") { Id = 2, IsActive = false }
        };

        _mockCampaignService.Setup(x => x.GetActiveCampaignsAsync())
            .ReturnsAsync(campaigns);

        // Act
        var result = await _controller.Campaigns();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.Model, Is.EqualTo(campaigns));
    }

    [Test]
    public void CreateCampaign_ReturnsViewWithEmptyRequest()
    {
        // Act
        var result = _controller.CreateCampaign();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.Model, Is.InstanceOf<CreateCampaignRequest>());
    }

    [Test]
    public async Task CreateCampaign_ReturnsRedirect_WhenModelIsValid()
    {
        // Arrange
        var request = new CreateCampaignRequest
        {
            Name = "Test Campaign",
            Description = "Test Description",
            CreatedBy = "Admin"
        };

        var campaign = new Campaign("Test Campaign", "Test Description", "Admin") { Id = 1 };

        _mockCampaignService.Setup(x => x.CreateCampaignAsync(request))
            .ReturnsAsync(campaign);

        // Ensure ModelState is valid - Controller sets CreatedBy if empty, so we need to ensure ModelState is valid
        _controller.ModelState.Clear();
        // Simulate that the model binder has processed the request successfully
        _controller.ModelState.SetModelValue("Name", new ValueProviderResult("Test Campaign", CultureInfo.InvariantCulture));
        _controller.ModelState.SetModelValue("Description", new ValueProviderResult("Test Description", CultureInfo.InvariantCulture));
        _controller.ModelState.SetModelValue("CreatedBy", new ValueProviderResult("Admin", CultureInfo.InvariantCulture));

        // Mark all entries as valid
        foreach (var key in _controller.ModelState.Keys.ToList())
        {
            var entry = _controller.ModelState[key];
            if (entry != null)
            {
                entry.ValidationState = ModelValidationState.Valid;
            }
        }

        // Act
        var result = await _controller.CreateCampaign(request);

        // Assert
        Assert.That(result, Is.InstanceOf<RedirectToActionResult>(), $"Expected RedirectToActionResult but got {result?.GetType().Name}");
        var redirectResult = result as RedirectToActionResult;
        Assert.That(redirectResult!.ActionName, Is.EqualTo(nameof(AdminController.Campaigns)));
    }

    [Test]
    public async Task CreateCampaign_ReturnsView_WhenModelIsInvalid()
    {
        // Arrange
        var request = new CreateCampaignRequest
        {
            Name = "", // Invalid
            Description = "Test Description",
            CreatedBy = "Admin"
        };

        _controller.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await _controller.CreateCampaign(request);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.Model, Is.EqualTo(request));
    }

    [Test]
    public async Task EditCampaign_ReturnsView_WhenCampaignExists()
    {
        // Arrange
        var campaignId = 1;
        var campaign = new Campaign("Test Campaign", "Test Description", "Admin")
        {
            Id = campaignId,
            IsActive = true
        };

        _mockCampaignService.Setup(x => x.GetCampaignByIdAsync(campaignId))
            .ReturnsAsync(campaign);

        // Act
        var result = await _controller.EditCampaign(campaignId);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.Model, Is.InstanceOf<UpdateCampaignRequest>());
    }

    [Test]
    public async Task EditCampaign_ReturnsNotFound_WhenCampaignDoesNotExist()
    {
        // Arrange
        var campaignId = 999;

        _mockCampaignService.Setup(x => x.GetCampaignByIdAsync(campaignId))
            .ReturnsAsync((Campaign?)null);

        // Act
        var result = await _controller.EditCampaign(campaignId);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task EditCampaign_ReturnsRedirect_WhenModelIsValid()
    {
        // Arrange
        var campaignId = 1;
        var campaign = new Campaign("Test Campaign", "Test Description", "Admin")
        {
            Id = campaignId
        };

        var request = new UpdateCampaignRequest
        {
            Name = "Updated Campaign",
            Description = "Updated Description",
        };

        _mockCampaignService.Setup(x => x.GetCampaignByIdAsync(campaignId))
            .ReturnsAsync(campaign);
        _mockCampaignService.Setup(x => x.UpdateCampaignAsync(campaignId, request))
            .Returns(Task.CompletedTask);

        // Ensure ModelState is valid
        _controller.ModelState.Clear();
        _controller.ModelState.SetModelValue("Name", new ValueProviderResult("Updated Campaign", CultureInfo.InvariantCulture));
        _controller.ModelState.SetModelValue("Description", new ValueProviderResult("Updated Description", CultureInfo.InvariantCulture));

        // Mark all entries as valid
        foreach (var key in _controller.ModelState.Keys.ToList())
        {
            var entry = _controller.ModelState[key];
            if (entry != null)
            {
                entry.ValidationState = ModelValidationState.Valid;
            }
        }

        // Act
        var result = await _controller.EditCampaign(campaignId, request);

        // Assert
        Assert.That(result, Is.InstanceOf<RedirectToActionResult>(), $"Expected RedirectToActionResult but got {result?.GetType().Name}");
        var redirectResult = result as RedirectToActionResult;
        Assert.That(redirectResult!.ActionName, Is.EqualTo(nameof(AdminController.Campaigns)));
    }

    [Test]
    public async Task DeleteCampaign_ReturnsView_WhenCampaignExists()
    {
        // Arrange
        var campaignId = 1;
        var campaign = new Campaign("Test Campaign", "Test Description", "Admin")
        {
            Id = campaignId
        };

        _mockCampaignService.Setup(x => x.GetCampaignByIdAsync(campaignId))
            .ReturnsAsync(campaign);

        // Act
        var result = await _controller.DeleteCampaign(campaignId);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.Model, Is.EqualTo(campaign));
    }

    [Test]
    public async Task DeleteCampaignConfirmed_ReturnsRedirect_WhenSuccessful()
    {
        // Arrange
        var campaignId = 1;

        _mockCampaignService.Setup(x => x.DeleteCampaignAsync(campaignId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteCampaignConfirmed(campaignId);

        // Assert
        Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
        var redirectResult = result as RedirectToActionResult;
        Assert.That(redirectResult!.ActionName, Is.EqualTo(nameof(AdminController.Campaigns)));
    }

    [Test]
    public async Task QrCodes_ReturnsViewWithQrCodes()
    {
        // Arrange
        var campaignId = 1;
        var campaign = new Campaign("Test Campaign", "Test Description", "Admin")
        {
            Id = campaignId
        };
        var qrCodes = new List<QrCode>
        {
            new QrCode(campaignId, "QR 1", "Description 1", "Notes 1") { Id = 1 },
            new QrCode(campaignId, "QR 2", "Description 2", "Notes 2") { Id = 2 }
        };

        _mockCampaignService.Setup(x => x.GetCampaignByIdAsync(campaignId))
            .ReturnsAsync(campaign);
        _mockQrCodeService.Setup(x => x.GetQrCodesByCampaignAsync(campaignId))
            .ReturnsAsync(qrCodes);

        // Act
        var result = await _controller.QrCodes(campaignId);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.Model, Is.InstanceOf<CampaignQrCodesViewModel>());
    }

    [Test]
    public async Task CreateQrCode_ReturnsView_WhenCampaignExists()
    {
        // Arrange
        var campaignId = 1;
        var campaign = new Campaign("Test Campaign", "Test Description", "Admin")
        {
            Id = campaignId
        };

        _mockCampaignService.Setup(x => x.GetCampaignByIdAsync(campaignId))
            .ReturnsAsync(campaign);

        // Act
        var result = await _controller.CreateQrCode(campaignId);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.Model, Is.InstanceOf<CreateQrCodeRequest>());
    }

    [Test]
    public async Task CreateQrCode_ReturnsRedirect_WhenModelIsValid()
    {
        // Arrange
        var request = new CreateQrCodeRequest
        {
            CampaignId = 1,
            Title = "Test QR Code",
            Description = "Test Description",
            InternalNotes = "Test Notes"
        };

        var qrCode = new QrCode(1, "Test QR Code", "Test Description", "Test Notes") { Id = 1 };

        _mockQrCodeService.Setup(x => x.CreateQrCodeAsync(request))
            .ReturnsAsync(qrCode);

        // Ensure ModelState is valid
        _controller.ModelState.Clear();
        _controller.ModelState.SetModelValue("Title", new ValueProviderResult("Test QR Code", CultureInfo.InvariantCulture));
        _controller.ModelState.SetModelValue("Description", new ValueProviderResult("Test Description", CultureInfo.InvariantCulture));
        _controller.ModelState.SetModelValue("InternalNotes", new ValueProviderResult("Test Notes", CultureInfo.InvariantCulture));
        _controller.ModelState.SetModelValue("CampaignId", new ValueProviderResult("1", CultureInfo.InvariantCulture));

        // Mark all entries as valid
        foreach (var key in _controller.ModelState.Keys.ToList())
        {
            var entry = _controller.ModelState[key];
            if (entry != null)
            {
                entry.ValidationState = ModelValidationState.Valid;
            }
        }

        // Act
        var result = await _controller.CreateQrCode(request);

        // Assert
        Assert.That(result, Is.InstanceOf<RedirectToActionResult>(), $"Expected RedirectToActionResult but got {result?.GetType().Name}");
        var redirectResult = result as RedirectToActionResult;
        Assert.That(redirectResult!.ActionName, Is.EqualTo(nameof(AdminController.QrCodes)));
    }

    [Test]
    public async Task PrintQrCodes_ReturnsView_WhenCampaignExists()
    {
        // Arrange
        var campaignId = 1;
        var campaign = new Campaign("Test Campaign", "Test Description", "Admin") { Id = campaignId };
        var printData = new PrintLayoutViewModel
        {
            Campaign = campaign,
            QrCodes = new List<PrintQrCodeItem>(),
            PrintDate = DateTime.UtcNow
        };

        _mockPrintService.Setup(x => x.PreparePrintDataAsync(campaignId))
            .ReturnsAsync(printData);

        // Act
        var result = await _controller.PrintQrCodes(campaignId);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.Model, Is.EqualTo(printData));
    }

    [Test]
    public async Task CampaignStatistics_ReturnsViewWithStatistics()
    {
        // Arrange
        var campaignId = 1;
        var statistics = new CampaignQrCodeStatisticsViewModel
        {
            CampaignId = campaignId,
            CampaignName = "Test Campaign",
            TotalQrCodes = 5,
            TotalFinds = 10,
            FoundQrCodes = 3
        };

        _mockStatisticsService.Setup(x => x.GetCampaignStatisticsAsync(campaignId))
            .ReturnsAsync(statistics);

        // Act
        var result = await _controller.CampaignStatistics(campaignId);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.Model, Is.EqualTo(statistics));
    }

    [Test]
    public async Task QrCodeStatistics_ReturnsViewWithStatistics()
    {
        // Arrange
        var qrCodeId = 1;
        var statistics = new QrCodeStatisticsViewModel
        {
            QrCodeId = qrCodeId,
            Title = "Test QR Code",
            FindCount = 5,
            Finders = new List<FinderInfoViewModel>()
        };

        _mockStatisticsService.Setup(x => x.GetQrCodeStatisticsAsync(qrCodeId))
            .ReturnsAsync(statistics);

        // Act
        var result = await _controller.QrCodeStatistics(qrCodeId);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.Model, Is.EqualTo(statistics));
    }

    [Test]
    public async Task ReorderQrCodes_ReturnsJsonSuccess_WhenValid()
    {
        // Arrange
        var campaignId = 1;
        var qrCodeIds = new[] { 1, 2, 3 };

        // Act
        var result = await _controller.ReorderQrCodes(campaignId, qrCodeIds);

        // Assert
        Assert.That(result, Is.InstanceOf<JsonResult>());
        var jsonResult = result as JsonResult;
        var value = jsonResult!.Value;
        Assert.That(value, Is.Not.Null);
    }

    [Test]
    public async Task Leaderboard_ReturnsViewWithTopPerformers()
    {
        // Arrange
        var topPerformers = new TopPerformersStatisticsViewModel
        {
            TopByTotalFinds = new List<UserStatistics>
            {
                new UserStatistics { UserId = 1, UserName = "User1", TotalFinds = 10, UniqueQrCodesFound = 8 },
                new UserStatistics { UserId = 2, UserName = "User2", TotalFinds = 8, UniqueQrCodesFound = 7 }
            },
            TopByUniqueQrCodes = new List<UserStatistics>
            {
                new UserStatistics { UserId = 1, UserName = "User1", TotalFinds = 10, UniqueQrCodesFound = 8 },
                new UserStatistics { UserId = 2, UserName = "User2", TotalFinds = 8, UniqueQrCodesFound = 7 }
            },
            MostRecentActivity = new List<UserStatistics>
            {
                new UserStatistics { UserId = 1, UserName = "User1", TotalFinds = 10, UniqueQrCodesFound = 8, LastFindDate = DateTime.UtcNow }
            },
            GeneratedAt = DateTime.UtcNow
        };

        _mockStatisticsService.Setup(x => x.GetTopPerformersAsync())
            .ReturnsAsync(topPerformers);

        // Act
        var result = await _controller.Leaderboard();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.Model, Is.EqualTo(topPerformers));
    }

    [Test]
    public async Task Leaderboard_ReturnsErrorView_WhenExceptionOccurs()
    {
        // Arrange
        _mockStatisticsService.Setup(x => x.GetTopPerformersAsync())
            .ThrowsAsync(new HttpRequestException("API Error"));

        // Act
        var result = await _controller.Leaderboard();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.ViewName, Is.EqualTo("Error"));
    }

    [Test]
    public async Task Leaderboard_ReturnsErrorView_WhenUnexpectedExceptionOccurs()
    {
        // Arrange
        _mockStatisticsService.Setup(x => x.GetTopPerformersAsync())
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        // Act
        var result = await _controller.Leaderboard();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.ViewName, Is.EqualTo("Error"));
    }

    [Test]
    public async Task Statistics_ReturnsViewWithStatistics()
    {
        // Arrange
        var statistics = new StatisticsViewModel
        {
            TotalCampaigns = 3,
            ActiveCampaigns = 2,
            TotalUsers = 10,
            ActiveUsers = 8,
            TotalQrCodes = 15,
            TotalFinds = 25,
            TopFoundQrCodes = new List<QrCodeStatisticsViewModel>
            {
                new QrCodeStatisticsViewModel
                {
                    QrCodeId = 1,
                    Title = "Top QR Code",
                    FindCount = 10,
                    CampaignName = "Test Campaign"
                },
                new QrCodeStatisticsViewModel
                {
                    QrCodeId = 2,
                    Title = "Second QR Code",
                    FindCount = 8,
                    CampaignName = "Test Campaign"
                }
            },
            UnfoundQrCodesByCampaign = new Dictionary<string, IReadOnlyList<QrCodeStatisticsViewModel>>
            {
                ["Test Campaign"] = new List<QrCodeStatisticsViewModel>
                {
                    new QrCodeStatisticsViewModel
                    {
                        QrCodeId = 3,
                        Title = "Unfound QR Code",
                        FindCount = 0,
                        CampaignName = "Test Campaign"
                    }
                }
            }
        };

        _mockStatisticsService.Setup(x => x.GetStatisticsAsync())
            .ReturnsAsync(statistics);

        // Act
        var result = await _controller.Statistics();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.Model, Is.EqualTo(statistics));
    }

    [Test]
    public async Task Statistics_ReturnsErrorView_WhenHttpRequestExceptionOccurs()
    {
        // Arrange
        _mockStatisticsService.Setup(x => x.GetStatisticsAsync())
            .ThrowsAsync(new HttpRequestException("API Error"));

        // Act
        var result = await _controller.Statistics();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.ViewName, Is.EqualTo("Error"));
    }

    [Test]
    public async Task Statistics_ReturnsErrorView_WhenUnexpectedExceptionOccurs()
    {
        // Arrange
        _mockStatisticsService.Setup(x => x.GetStatisticsAsync())
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        // Act
        var result = await _controller.Statistics();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.ViewName, Is.EqualTo("Error"));
    }

    [Test]
    public async Task Statistics_WithEmptyData_ReturnsViewWithEmptyStatistics()
    {
        // Arrange
        var statistics = new StatisticsViewModel
        {
            TotalCampaigns = 0,
            ActiveCampaigns = 0,
            TotalUsers = 0,
            ActiveUsers = 0,
            TotalQrCodes = 0,
            TotalFinds = 0,
            TopFoundQrCodes = new List<QrCodeStatisticsViewModel>(),
            UnfoundQrCodesByCampaign = new Dictionary<string, IReadOnlyList<QrCodeStatisticsViewModel>>()
        };

        _mockStatisticsService.Setup(x => x.GetStatisticsAsync())
            .ReturnsAsync(statistics);

        // Act
        var result = await _controller.Statistics();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.Model, Is.EqualTo(statistics));
        var model = viewResult.Model as StatisticsViewModel;
        Assert.That(model!.TotalCampaigns, Is.EqualTo(0));
        Assert.That(model.TotalFinds, Is.EqualTo(0));
        Assert.That(model.TopFoundQrCodes, Is.Empty);
    }

    /// <summary>
    /// Testet, dass Statistics Performance-Metriken im ViewModel enthält.
    /// Wichtig, da Performance-Metriken in der View angezeigt werden.
    /// Stellt sicher, dass Performance-Daten korrekt übergeben werden.
    /// </summary>
    [Test]
    public async Task Statistics_WithPerformanceMetrics_ReturnsViewWithPerformanceData()
    {
        // Arrange
        var statistics = new StatisticsViewModel
        {
            TotalCampaigns = 2,
            ActiveCampaigns = 1,
            TotalUsers = 5,
            ActiveUsers = 4,
            TotalQrCodes = 10,
            TotalFinds = 15,
            TopFoundQrCodes = new List<QrCodeStatisticsViewModel>(),
            UnfoundQrCodesByCampaign = new Dictionary<string, IReadOnlyList<QrCodeStatisticsViewModel>>(),
            PerformanceMetrics = new PerformanceMetricsViewModel
            {
                AverageResponseTimeMs = 125.5,
                ApiCallsLastMinute = 5,
                SlowestResponseTimeMs = 250,
                FastestResponseTimeMs = 50,
                SlowRequestsCount = 0,
                LastUpdated = DateTime.UtcNow
            }
        };

        _mockStatisticsService.Setup(x => x.GetStatisticsAsync())
            .ReturnsAsync(statistics);

        // Act
        var result = await _controller.Statistics();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        var model = viewResult!.Model as StatisticsViewModel;
        Assert.That(model!.PerformanceMetrics, Is.Not.Null);
        Assert.That(model.PerformanceMetrics!.AverageResponseTimeMs, Is.EqualTo(125.5));
        Assert.That(model.PerformanceMetrics.ApiCallsLastMinute, Is.EqualTo(5));
        Assert.That(model.PerformanceMetrics.SlowestResponseTimeMs, Is.EqualTo(250));
        Assert.That(model.PerformanceMetrics.FastestResponseTimeMs, Is.EqualTo(50));
    }

    /// <summary>
    /// Testet, dass Statistics auch ohne Performance-Metriken funktioniert.
    /// Wichtig, um Rückwärtskompatibilität sicherzustellen und sicherzustellen,
    /// dass die View auch ohne Performance-Daten funktioniert.
    /// </summary>
    [Test]
    public async Task Statistics_WithoutPerformanceMetrics_ReturnsViewWithNullMetrics()
    {
        // Arrange
        var statistics = new StatisticsViewModel
        {
            TotalCampaigns = 2,
            ActiveCampaigns = 1,
            TotalUsers = 5,
            ActiveUsers = 4,
            TotalQrCodes = 10,
            TotalFinds = 15,
            TopFoundQrCodes = new List<QrCodeStatisticsViewModel>(),
            UnfoundQrCodesByCampaign = new Dictionary<string, IReadOnlyList<QrCodeStatisticsViewModel>>(),
            PerformanceMetrics = null
        };

        _mockStatisticsService.Setup(x => x.GetStatisticsAsync())
            .ReturnsAsync(statistics);

        // Act
        var result = await _controller.Statistics();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        var model = viewResult!.Model as StatisticsViewModel;
        Assert.That(model!.PerformanceMetrics, Is.Null);
    }

    #region FindHistory Tests

    /// <summary>
    /// Testet FindHistory mit gültigen Filtern.
    /// Wichtig, da diese Action die Fund-Historie-Seite rendert.
    /// Stellt sicher, dass die View mit korrektem ViewModel zurückgegeben wird.
    /// </summary>
    [Test]
    public async Task FindHistory_ReturnsViewWithFindHistoryViewModel()
    {
        // Arrange
        var findHistoryViewModel = new FindHistoryViewModel
        {
            Finds = new List<Find>
            {
                new Find(1, 1, "127.0.0.1", "User Agent 1"),
                new Find(2, 1, "192.168.1.1", "User Agent 2")
            },
            TotalCount = 2,
            Skip = 0,
            Take = 50,
            AllUsers = new List<User>(),
            AllQrCodes = new List<QrCode>(),
            AllCampaigns = new List<Campaign>()
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
            .ReturnsAsync(findHistoryViewModel);

        // Act
        var result = await _controller.FindHistory();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.Model, Is.EqualTo(findHistoryViewModel));
    }

    /// <summary>
    /// Testet FindHistory mit Filter-Parametern.
    /// Wichtig, um sicherzustellen, dass alle Filter-Parameter korrekt weitergegeben werden.
    /// Verhindert, dass Filter ignoriert werden.
    /// </summary>
    [Test]
    public async Task FindHistory_WithFilters_ShouldPassFiltersToService()
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
            .ReturnsAsync(new FindHistoryViewModel
            {
                Finds = new List<Find>(),
                TotalCount = 0,
                Skip = skip,
                Take = take,
                AllUsers = new List<User>(),
                AllQrCodes = new List<QrCode>(),
                AllCampaigns = new List<Campaign>()
            });

        // Act
        await _controller.FindHistory(startDate, endDate, userId, qrCodeId, campaignId, skip, take, sortBy, sortDirection);

        // Assert
        _mockStatisticsService.Verify(x => x.GetFindHistoryAsync(
            startDate, endDate, userId, qrCodeId, campaignId, skip, take, sortBy, sortDirection), Times.Once);
    }

    /// <summary>
    /// Testet FindHistory mit HttpRequestException.
    /// Wichtig, um sicherzustellen, dass API-Fehler korrekt behandelt werden.
    /// Verhindert ungehandelte Exceptions in Production.
    /// </summary>
    [Test]
    public async Task FindHistory_ReturnsErrorView_WhenHttpRequestExceptionOccurs()
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
            .ThrowsAsync(new HttpRequestException("API Error"));

        // Act
        var result = await _controller.FindHistory();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.ViewName, Is.EqualTo("Error"));
    }

    /// <summary>
    /// Testet FindHistory mit unerwarteter Exception.
    /// Wichtig, um sicherzustellen, dass alle Exceptions korrekt behandelt werden.
    /// Verhindert ungehandelte Exceptions in Production.
    /// </summary>
    [Test]
    public async Task FindHistory_ReturnsErrorView_WhenUnexpectedExceptionOccurs()
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
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        // Act
        var result = await _controller.FindHistory();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.ViewName, Is.EqualTo("Error"));
    }

    #endregion

    public void Dispose()
    {
        _controller?.Dispose();
    }
}
