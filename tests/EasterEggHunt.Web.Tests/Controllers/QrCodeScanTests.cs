using System.Security.Claims;
using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Web.Controllers;
using EasterEggHunt.Web.Models;
using EasterEggHunt.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace EasterEggHunt.Web.Tests.Controllers;

/// <summary>
/// Tests für QR-Code-Scan-Funktionalität im EmployeeController
/// </summary>
[TestFixture]
public class QrCodeScanTests : IDisposable
{
    private Mock<ILogger<EmployeeController>> _mockLogger = null!;
    private Mock<IEasterEggHuntApiClient> _mockApiClient = null!;
    private EmployeeController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<EmployeeController>>();
        _mockApiClient = new Mock<IEasterEggHuntApiClient>();

        _controller = new EmployeeController(_mockLogger.Object, _mockApiClient.Object);

        // Setup authenticated user
        var claims = new List<Claim>
        {
            new Claim("UserId", "123")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal,
                Connection = { RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1") },
                Request = { Headers = { ["User-Agent"] = "Test User Agent" } }
            }
        };
    }

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
    }

    [Test]
    public async Task ScanQrCode_WithValidCode_ReturnsScanResultView()
    {
        // Arrange
        var qrCode = "d90cffe8f07b";
        var qrCodeEntity = new QrCode
        {
            Id = 1,
            Code = qrCode,
            Title = "Test QR Code",
            CampaignId = 1,
            IsActive = true
        };
        var campaign = new Campaign
        {
            Id = 1,
            Name = "Test Campaign",
            IsActive = true
        };

        _mockApiClient.Setup(x => x.GetQrCodeByCodeAsync(qrCode))
            .ReturnsAsync(qrCodeEntity);
        _mockApiClient.Setup(x => x.GetActiveCampaignsAsync())
            .ReturnsAsync(new List<Campaign> { campaign });
        _mockApiClient.Setup(x => x.GetExistingFindAsync(qrCodeEntity.Id, 123))
            .ReturnsAsync((Find?)null);
        _mockApiClient.Setup(x => x.RegisterFindAsync(qrCodeEntity.Id, 123, "127.0.0.1", "Test User Agent"))
            .ReturnsAsync(new Find { Id = 1, QrCodeId = qrCodeEntity.Id, UserId = 123 });

        // Act
        var result = await _controller.ScanQrCode((string?)qrCode);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = (ViewResult)result;
        Assert.That(viewResult.ViewName, Is.EqualTo("ScanResult"));
        Assert.That(viewResult.Model, Is.InstanceOf<ScanResultViewModel>());

        var viewModel = (ScanResultViewModel)viewResult.Model!;
        Assert.That(viewModel.QrCode.Id, Is.EqualTo(qrCodeEntity.Id));
        Assert.That(viewModel.CurrentFind, Is.Not.Null);
        Assert.That(viewModel.PreviousFind, Is.Null); // Erster Fund
    }

    [Test]
    public async Task ScanQrCode_WithInvalidCode_ReturnsInvalidQrCodeView()
    {
        // Arrange
        var invalidCode = "invalid-code";

        _mockApiClient.Setup(x => x.GetQrCodeByCodeAsync(invalidCode))
            .ReturnsAsync((QrCode?)null);

        // Act
        var result = await _controller.ScanQrCode((string?)invalidCode);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = (ViewResult)result;
        Assert.That(viewResult.ViewName, Is.EqualTo("InvalidQrCode"));
    }

    [Test]
    public async Task ScanQrCode_WithEmptyCode_ReturnsInvalidQrCodeView()
    {
        // Act
        var result = await _controller.ScanQrCode((string?)"");

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = (ViewResult)result;
        Assert.That(viewResult.ViewName, Is.EqualTo("InvalidQrCode"));
    }

    [Test]
    public async Task ScanQrCode_WithNullCode_ReturnsInvalidQrCodeView()
    {
        // Act
        var result = await _controller.ScanQrCode((string?)null);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = (ViewResult)result;
        Assert.That(viewResult.ViewName, Is.EqualTo("InvalidQrCode"));
    }

    [Test]
    public async Task ScanQrCode_WithInactiveCampaign_ReturnsNoCampaignView()
    {
        // Arrange
        var qrCode = "d90cffe8f07b";
        var qrCodeEntity = new QrCode
        {
            Id = 1,
            Code = qrCode,
            Title = "Test QR Code",
            CampaignId = 1,
            IsActive = true
        };

        _mockApiClient.Setup(x => x.GetQrCodeByCodeAsync(qrCode))
            .ReturnsAsync(qrCodeEntity);
        _mockApiClient.Setup(x => x.GetActiveCampaignsAsync())
            .ReturnsAsync(new List<Campaign>()); // Keine aktiven Kampagnen

        // Act
        var result = await _controller.ScanQrCode((string?)qrCode);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = (ViewResult)result;
        Assert.That(viewResult.ViewName, Is.EqualTo("NoCampaign"));
    }

    [Test]
    public async Task ScanQrCode_WithAlreadyFoundQrCode_ReturnsScanResultViewWithIsNewFindFalse()
    {
        // Arrange
        var qrCode = "d90cffe8f07b";
        var qrCodeEntity = new QrCode
        {
            Id = 1,
            Code = qrCode,
            Title = "Test QR Code",
            CampaignId = 1,
            IsActive = true
        };
        var campaign = new Campaign
        {
            Id = 1,
            Name = "Test Campaign",
            IsActive = true
        };
        var existingFind = new Find
        {
            Id = 1,
            QrCodeId = qrCodeEntity.Id,
            UserId = 123
        };

        _mockApiClient.Setup(x => x.GetQrCodeByCodeAsync(qrCode))
            .ReturnsAsync(qrCodeEntity);
        _mockApiClient.Setup(x => x.GetActiveCampaignsAsync())
            .ReturnsAsync(new List<Campaign> { campaign });
        _mockApiClient.Setup(x => x.GetExistingFindAsync(qrCodeEntity.Id, 123))
            .ReturnsAsync(existingFind);

        // Act
        var result = await _controller.ScanQrCode((string?)qrCode);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = (ViewResult)result;
        Assert.That(viewResult.ViewName, Is.EqualTo("ScanResult"));
        Assert.That(viewResult.Model, Is.InstanceOf<ScanResultViewModel>());

        var viewModel = (ScanResultViewModel)viewResult.Model!;
        Assert.That(viewModel.QrCode.Id, Is.EqualTo(qrCodeEntity.Id));
        Assert.That(viewModel.CurrentFind, Is.Not.Null);
        Assert.That(viewModel.PreviousFind, Is.Not.Null); // Bereits gefunden
    }

    [Test]
    public async Task ScanQrCode_WithUnauthenticatedUser_RedirectsToRegister()
    {
        // Arrange
        var qrCode = "d90cffe8f07b";

        // Setup unauthenticated user
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal() // Keine Claims = nicht authentifiziert
            }
        };

        // Act
        var result = await _controller.ScanQrCode((string?)qrCode);

        // Assert
        Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
        var redirectResult = (RedirectToActionResult)result;
        Assert.That(redirectResult.ActionName, Is.EqualTo("Register"));
        Assert.That(redirectResult.RouteValues!["qrCodeUrl"], Is.EqualTo($"/qr/{Uri.EscapeDataString(qrCode)}"));
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
