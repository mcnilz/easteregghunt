using EasterEggHunt.Application.Services;
using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Web.Controllers;
using EasterEggHunt.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace EasterEggHunt.Web.Tests.Controllers;

/// <summary>
/// Tests für AdminController
/// </summary>
[TestFixture]
public class AdminControllerTests : IDisposable
{
    private Mock<ICampaignService> _mockCampaignService = null!;
    private Mock<IQrCodeService> _mockQrCodeService = null!;
    private Mock<IUserService> _mockUserService = null!;
    private Mock<IFindService> _mockFindService = null!;
    private Mock<ILogger<AdminController>> _mockLogger = null!;
    private AdminController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockCampaignService = new Mock<ICampaignService>();
        _mockQrCodeService = new Mock<IQrCodeService>();
        _mockUserService = new Mock<IUserService>();
        _mockFindService = new Mock<IFindService>();
        _mockLogger = new Mock<ILogger<AdminController>>();

        _controller = new AdminController(
            _mockCampaignService.Object,
            _mockQrCodeService.Object,
            _mockUserService.Object,
            _mockFindService.Object,
            _mockLogger.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
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

        _mockCampaignService.Setup(x => x.GetActiveCampaignsAsync())
            .ReturnsAsync(campaigns);
        _mockUserService.Setup(x => x.GetActiveUsersAsync())
            .ReturnsAsync(users);
        _mockQrCodeService.Setup(x => x.GetQrCodesByCampaignIdAsync(1))
            .ReturnsAsync(qrCodes);
        _mockQrCodeService.Setup(x => x.GetQrCodesByCampaignIdAsync(2))
            .ReturnsAsync(new List<QrCode>());
        _mockFindService.Setup(x => x.GetFindsByQrCodeIdAsync(1))
            .ReturnsAsync(finds.Where(f => f.QrCodeId == 1));
        _mockFindService.Setup(x => x.GetFindsByQrCodeIdAsync(2))
            .ReturnsAsync(finds.Where(f => f.QrCodeId == 2));

        // Act
        var result = await _controller.Index();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.Model, Is.InstanceOf<AdminDashboardViewModel>());

        var viewModel = viewResult.Model as AdminDashboardViewModel;
        Assert.That(viewModel!.TotalUsers, Is.EqualTo(2));
        Assert.That(viewModel.ActiveCampaigns, Is.EqualTo(1));
        Assert.That(viewModel.TotalQrCodes, Is.EqualTo(2));
        Assert.That(viewModel.ActiveQrCodes, Is.EqualTo(1));
        Assert.That(viewModel.TotalFinds, Is.EqualTo(2));
        Assert.That(viewModel.RecentActivities, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task Index_WithNoData_ReturnsViewWithEmptyData()
    {
        // Arrange
        _mockCampaignService.Setup(x => x.GetActiveCampaignsAsync())
            .ReturnsAsync(new List<Campaign>());
        _mockUserService.Setup(x => x.GetActiveUsersAsync())
            .ReturnsAsync(new List<User>());

        // Act
        var result = await _controller.Index();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.Model, Is.InstanceOf<AdminDashboardViewModel>());

        var viewModel = viewResult.Model as AdminDashboardViewModel;
        Assert.That(viewModel!.TotalUsers, Is.EqualTo(0));
        Assert.That(viewModel.ActiveCampaigns, Is.EqualTo(0));
        Assert.That(viewModel.TotalQrCodes, Is.EqualTo(0));
        Assert.That(viewModel.ActiveQrCodes, Is.EqualTo(0));
        Assert.That(viewModel.TotalFinds, Is.EqualTo(0));
        Assert.That(viewModel.RecentActivities, Is.Empty);
    }

    [Test]
    public async Task Index_WithException_ReturnsErrorView()
    {
        // Arrange
        _mockCampaignService.Setup(x => x.GetActiveCampaignsAsync())
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        // Act
        var result = await _controller.Index();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.ViewName, Is.EqualTo("Error"));
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _controller?.Dispose();
        }
    }
}
