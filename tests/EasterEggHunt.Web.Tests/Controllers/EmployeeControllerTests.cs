using System.Security.Claims;
using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Web.Controllers;
using EasterEggHunt.Web.Models;
using EasterEggHunt.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace EasterEggHunt.Web.Tests.Controllers;

[TestFixture]
public class EmployeeControllerTests : IDisposable
{
    private Mock<IEasterEggHuntApiClient> _mockApiClient = null!;
    private Mock<ILogger<EmployeeController>> _mockLogger = null!;
    private EmployeeController _controller = null!;
    private Mock<HttpContext> _mockHttpContext = null!;

    [SetUp]
    public void Setup()
    {
        _mockApiClient = new Mock<IEasterEggHuntApiClient>();
        _mockLogger = new Mock<ILogger<EmployeeController>>();
        _controller = new EmployeeController(_mockLogger.Object, _mockApiClient.Object);

        // Setup HttpContext
        _mockHttpContext = new Mock<HttpContext>();
        var mockConnection = new Mock<ConnectionInfo>();
        var mockRequest = new Mock<HttpRequest>();
        var mockHeaders = new Mock<IHeaderDictionary>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockAuthenticationService = new Mock<IAuthenticationService>();

        mockConnection.Setup(x => x.RemoteIpAddress).Returns(System.Net.IPAddress.Parse("127.0.0.1"));
        mockHeaders.Setup(x => x["User-Agent"]).Returns("TestAgent");
        mockRequest.Setup(x => x.Headers).Returns(mockHeaders.Object);

        // Setup Authentication Service
        mockAuthenticationService.Setup(x => x.SignInAsync(
            It.IsAny<HttpContext>(),
            It.IsAny<string>(),
            It.IsAny<ClaimsPrincipal>(),
            It.IsAny<AuthenticationProperties>()))
            .Returns(Task.CompletedTask);

        mockServiceProvider.Setup(x => x.GetService(typeof(IAuthenticationService)))
            .Returns(mockAuthenticationService.Object);

        // Setup MVC Services - Simplified approach
        var mockUrlHelperFactory = new Mock<IUrlHelperFactory>();
        var mockUrlHelper = new Mock<IUrlHelper>();
        var mockRouteData = new Mock<RouteData>();

        // Mock the IsLocalUrl method
        mockUrlHelper.Setup(x => x.IsLocalUrl(It.IsAny<string>())).Returns(true);

        mockUrlHelperFactory.Setup(x => x.GetUrlHelper(It.IsAny<ActionContext>()))
            .Returns(mockUrlHelper.Object);

        mockServiceProvider.Setup(x => x.GetService(typeof(IUrlHelperFactory)))
            .Returns(mockUrlHelperFactory.Object);

        // Setup TempData
        var mockTempDataProvider = new Mock<ITempDataProvider>();
        var tempData = new TempDataDictionary(_mockHttpContext.Object, mockTempDataProvider.Object);

        mockServiceProvider.Setup(x => x.GetService(typeof(ITempDataProvider)))
            .Returns(mockTempDataProvider.Object);

        _mockHttpContext.Setup(x => x.Connection).Returns(mockConnection.Object);
        _mockHttpContext.Setup(x => x.Request).Returns(mockRequest.Object);
        _mockHttpContext.Setup(x => x.RequestServices).Returns(mockServiceProvider.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = _mockHttpContext.Object,
            ActionDescriptor = new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor(),
            RouteData = mockRouteData.Object
        };

        // Set Url property manually
        _controller.Url = mockUrlHelper.Object;

        // Set TempData manually
        _controller.TempData = tempData;
    }

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
    }

    [Test]
    public async Task ScanQrCode_ReturnsRedirectToRegister_WhenUserNotAuthenticated()
    {
        // Arrange
        var code = "test-code";
        var claimsPrincipal = new ClaimsPrincipal();
        _mockHttpContext.Setup(x => x.User).Returns(claimsPrincipal);

        // Act
        var result = await _controller.ScanQrCode(code);

        // Assert
        Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
        var redirectResult = result as RedirectToActionResult;
        Assert.That(redirectResult!.ActionName, Is.EqualTo("Register"));
        Assert.That(redirectResult.RouteValues!["qrCodeUrl"], Is.EqualTo($"/qr/{Uri.EscapeDataString(code)}"));
    }

    [Test]
    public async Task ScanQrCode_ReturnsRedirectToRegister_WhenUserNotEmployeeScheme()
    {
        // Arrange
        var code = "test-code";
        var identity = new ClaimsIdentity("OtherScheme");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _mockHttpContext.Setup(x => x.User).Returns(claimsPrincipal);

        // Act
        var result = await _controller.ScanQrCode(code);

        // Assert
        Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
        var redirectResult = result as RedirectToActionResult;
        Assert.That(redirectResult!.ActionName, Is.EqualTo("Register"));
    }

    [Test]
    public async Task ScanQrCode_ReturnsInvalidQrCodeView_WhenCodeIsEmpty()
    {
        // Arrange
        var identity = new ClaimsIdentity("EmployeeScheme");
        identity.AddClaim(new Claim("UserId", "1"));
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _mockHttpContext.Setup(x => x.User).Returns(claimsPrincipal);

        // Act
        var result = await _controller.ScanQrCode("");

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.ViewName, Is.EqualTo("InvalidQrCode"));
    }

    [Test]
    public async Task ScanQrCode_ReturnsInvalidQrCodeView_WhenQrCodeNotFound()
    {
        // Arrange
        var code = "test-code";
        var identity = new ClaimsIdentity("EmployeeScheme");
        identity.AddClaim(new Claim("UserId", "1"));
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _mockHttpContext.Setup(x => x.User).Returns(claimsPrincipal);

        _mockApiClient.Setup(x => x.GetQrCodeByCodeAsync(code))
            .ReturnsAsync((QrCode?)null);

        // Act
        var result = await _controller.ScanQrCode(code);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.ViewName, Is.EqualTo("InvalidQrCode"));
    }

    [Test]
    public async Task ScanQrCode_ReturnsNoCampaignView_WhenNoActiveCampaign()
    {
        // Arrange
        var code = "test-code";
        var identity = new ClaimsIdentity("EmployeeScheme");
        identity.AddClaim(new Claim("UserId", "1"));
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _mockHttpContext.Setup(x => x.User).Returns(claimsPrincipal);

        var qrCode = new QrCode(1, "Test QR Code", "Test Description", "Test Note");
        _mockApiClient.Setup(x => x.GetQrCodeByCodeAsync(code))
            .ReturnsAsync(qrCode);

        _mockApiClient.Setup(x => x.GetActiveCampaignsAsync())
            .ReturnsAsync(new List<Campaign>());

        // Act
        var result = await _controller.ScanQrCode(code);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.ViewName, Is.EqualTo("NoCampaign"));
    }

    [Test]
    public async Task ScanQrCode_ReturnsInvalidQrCodeView_WhenQrCodeNotInActiveCampaign()
    {
        // Arrange
        var code = "test-code";
        var identity = new ClaimsIdentity("EmployeeScheme");
        identity.AddClaim(new Claim("UserId", "1"));
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _mockHttpContext.Setup(x => x.User).Returns(claimsPrincipal);

        var qrCode = new QrCode(1, "Test QR Code", "Test Description", "Test Note")
        {
            CampaignId = 2 // QR-Code gehört zu Kampagne 2
        };
        _mockApiClient.Setup(x => x.GetQrCodeByCodeAsync(code))
            .ReturnsAsync(qrCode);

        // Aktive Kampagnen sind nur Kampagne 1 und 3, nicht 2
        var activeCampaigns = new List<Campaign>
        {
            new Campaign("Test Campaign 1", "Test Description", "Test Creator") { Id = 1 },
            new Campaign("Test Campaign 3", "Test Description", "Test Creator") { Id = 3 }
        };
        _mockApiClient.Setup(x => x.GetActiveCampaignsAsync())
            .ReturnsAsync(activeCampaigns);

        // Act
        var result = await _controller.ScanQrCode(code);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.ViewName, Is.EqualTo("InvalidQrCode"));
    }

    [Test]
    public async Task ScanQrCode_ReturnsRedirectToRegister_WhenUserIdNotInClaims()
    {
        // Arrange
        var code = "test-code";
        var identity = new ClaimsIdentity("EmployeeScheme");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _mockHttpContext.Setup(x => x.User).Returns(claimsPrincipal);

        var qrCode = new QrCode(1, "Test QR Code", "Test Description", "Test Note");
        _mockApiClient.Setup(x => x.GetQrCodeByCodeAsync(code))
            .ReturnsAsync(qrCode);

        var activeCampaigns = new List<Campaign>
        {
            new Campaign("Test Campaign", "Test Description", "Test Creator") { Id = 1 }
        };
        _mockApiClient.Setup(x => x.GetActiveCampaignsAsync())
            .ReturnsAsync(activeCampaigns);

        // Act
        var result = await _controller.ScanQrCode(code);

        // Assert
        Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
        var redirectResult = result as RedirectToActionResult;
        Assert.That(redirectResult!.ActionName, Is.EqualTo("Register"));
    }

    [Test]
    public async Task ScanQrCode_ReturnsScanResultView_WhenSuccessful()
    {
        // Arrange
        var code = "test-code";
        var userId = 1;
        var identity = new ClaimsIdentity("EmployeeScheme");
        identity.AddClaim(new Claim("UserId", userId.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _mockHttpContext.Setup(x => x.User).Returns(claimsPrincipal);

        var qrCode = new QrCode(1, "Test QR Code", "Test Description", "Test Note")
        {
            Id = 1,
            CampaignId = 1,
            Code = "test-code"
        };
        _mockApiClient.Setup(x => x.GetQrCodeByCodeAsync(It.IsAny<string>()))
            .ReturnsAsync(qrCode);

        var activeCampaigns = new List<Campaign>
        {
            new Campaign("Test Campaign 1", "Test Description", "Test Creator") { Id = 1 },
            new Campaign("Test Campaign 2", "Test Description", "Test Creator") { Id = 2 }
        };
        _mockApiClient.Setup(x => x.GetActiveCampaignsAsync())
            .ReturnsAsync(activeCampaigns);

        var currentFind = new Find(qrCode.Id, userId, "127.0.0.1", "TestAgent");
        _mockApiClient.Setup(x => x.RegisterFindAsync(qrCode.Id, userId, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(currentFind);

        _mockApiClient.Setup(x => x.GetExistingFindAsync(qrCode.Id, userId))
            .ReturnsAsync((Find?)null);

        _mockApiClient.Setup(x => x.GetFindCountByUserIdAsync(userId))
            .ReturnsAsync(1);

        var campaignQrCodes = new List<QrCode> { qrCode };
        _mockApiClient.Setup(x => x.GetQrCodesByCampaignIdAsync(qrCode.CampaignId))
            .ReturnsAsync(campaignQrCodes);

        // Act
        var result = await _controller.ScanQrCode(code);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.ViewName, Is.EqualTo("ScanResult"));
        Assert.That(viewResult.Model, Is.InstanceOf<ScanResultViewModel>());
    }

    [Test]
    public async Task ScanQrCode_ReturnsScanResultView_WhenQrCodeAlreadyFound()
    {
        // Arrange
        var code = "test-code";
        var userId = 1;
        var identity = new ClaimsIdentity("EmployeeScheme");
        identity.AddClaim(new Claim("UserId", userId.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _mockHttpContext.Setup(x => x.User).Returns(claimsPrincipal);

        var qrCode = new QrCode(1, "Test QR Code", "Test Description", "Test Note")
        {
            Id = 1,
            CampaignId = 1,
            Code = "test-code"
        };
        _mockApiClient.Setup(x => x.GetQrCodeByCodeAsync(It.IsAny<string>()))
            .ReturnsAsync(qrCode);

        var activeCampaigns = new List<Campaign>
        {
            new Campaign("Test Campaign 1", "Test Description", "Test Creator") { Id = 1 },
            new Campaign("Test Campaign 2", "Test Description", "Test Creator") { Id = 2 }
        };
        _mockApiClient.Setup(x => x.GetActiveCampaignsAsync())
            .ReturnsAsync(activeCampaigns);

        // Bereits gefundener Fund
        var existingFind = new Find(qrCode.Id, userId, "127.0.0.1", "TestAgent")
        {
            Id = 1,
            FoundAt = DateTime.UtcNow.AddDays(-1)
        };
        _mockApiClient.Setup(x => x.GetExistingFindAsync(qrCode.Id, userId))
            .ReturnsAsync(existingFind);

        // RegisterFindAsync sollte NICHT aufgerufen werden
        _mockApiClient.Setup(x => x.RegisterFindAsync(qrCode.Id, userId, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(existingFind);

        _mockApiClient.Setup(x => x.GetFindCountByUserIdAsync(userId))
            .ReturnsAsync(1);

        var campaignQrCodes = new List<QrCode> { qrCode };
        _mockApiClient.Setup(x => x.GetQrCodesByCampaignIdAsync(qrCode.CampaignId))
            .ReturnsAsync(campaignQrCodes);

        // Act
        var result = await _controller.ScanQrCode(code);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.ViewName, Is.EqualTo("ScanResult"));
        Assert.That(viewResult.Model, Is.InstanceOf<ScanResultViewModel>());

        var viewModel = viewResult.Model as ScanResultViewModel;
        Assert.That(viewModel!.IsFirstFind, Is.False);
        Assert.That(viewModel.PreviousFind, Is.EqualTo(existingFind));
        Assert.That(viewModel.CurrentFind, Is.EqualTo(existingFind));

        // Verify that RegisterFindAsync was NOT called
        _mockApiClient.Verify(x => x.RegisterFindAsync(qrCode.Id, userId, It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task ScanQrCode_ReturnsInvalidQrCodeView_WhenHttpRequestException()
    {
        // Arrange
        var code = "test-code";
        var identity = new ClaimsIdentity("EmployeeScheme");
        identity.AddClaim(new Claim("UserId", "1"));
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _mockHttpContext.Setup(x => x.User).Returns(claimsPrincipal);

        _mockApiClient.Setup(x => x.GetQrCodeByCodeAsync(code))
            .ThrowsAsync(new HttpRequestException("API Error"));

        // Act
        var result = await _controller.ScanQrCode(code);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.ViewName, Is.EqualTo("InvalidQrCode"));
    }

    [Test]
    public async Task ScanQrCode_ReturnsInvalidQrCodeView_WhenInvalidOperationException()
    {
        // Arrange
        var code = "test-code";
        var identity = new ClaimsIdentity("EmployeeScheme");
        identity.AddClaim(new Claim("UserId", "1"));
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _mockHttpContext.Setup(x => x.User).Returns(claimsPrincipal);

        _mockApiClient.Setup(x => x.GetQrCodeByCodeAsync(code))
            .ThrowsAsync(new InvalidOperationException("Invalid Operation"));

        // Act
        var result = await _controller.ScanQrCode(code);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.ViewName, Is.EqualTo("InvalidQrCode"));
    }

    [Test]
    public async Task ScanQrCode_ReturnsInvalidQrCodeView_WhenUnexpectedException()
    {
        // Arrange
        var code = "test-code";
        var identity = new ClaimsIdentity("EmployeeScheme");
        identity.AddClaim(new Claim("UserId", "1"));
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _mockHttpContext.Setup(x => x.User).Returns(claimsPrincipal);

        _mockApiClient.Setup(x => x.GetQrCodeByCodeAsync(code))
            .ThrowsAsync(new InvalidOperationException("Unexpected Error"));

        // Act
        var result = await _controller.ScanQrCode(code);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.ViewName, Is.EqualTo("InvalidQrCode"));
    }

    [Test]
    public async Task Progress_ReturnsView_WithCalculatedProgress()
    {
        var userId = 7;
        var identity = new ClaimsIdentity("EmployeeScheme");
        identity.AddClaim(new Claim("UserId", userId.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _mockHttpContext.Setup(x => x.User).Returns(claimsPrincipal);

        var campaign = new Campaign("Frühling", "Desc", "Admin") { Id = 1 };
        _mockApiClient.Setup(x => x.GetActiveCampaignsAsync())
            .ReturnsAsync(new List<Campaign> { campaign });

        var qr1 = new QrCode(1, "Eingang", "", "") { Id = 10 };
        var qr2 = new QrCode(1, "IT", "", "") { Id = 11 };
        var qr3 = new QrCode(1, "HR", "", "") { Id = 12 };
        _mockApiClient.Setup(x => x.GetQrCodesByCampaignIdAsync(campaign.Id))
            .ReturnsAsync(new List<QrCode> { qr1, qr2, qr3 });

        var finds = new List<Find>
        {
            new Find(qr1.Id, userId, "127.0.0.1", "UA"),
            new Find(qr2.Id, userId, "127.0.0.1", "UA"),
            new Find(qr2.Id, userId, "127.0.0.1", "UA")
        };
        finds[0].FoundAt = DateTime.UtcNow.AddHours(-3);
        finds[1].FoundAt = DateTime.UtcNow.AddHours(-2);
        finds[2].FoundAt = DateTime.UtcNow.AddHours(-1);

        _mockApiClient.Setup(x => x.GetFindsByUserIdAsync(userId))
            .ReturnsAsync(finds);

        _mockApiClient.Setup(x => x.GetUserStatisticsAsync(userId))
            .ReturnsAsync(new EasterEggHunt.Web.Models.UserStatistics { UserId = userId, UserName = "Alice" });

        var result = await _controller.Progress();

        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.ViewName, Is.EqualTo("Progress"));
        Assert.That(viewResult.Model, Is.InstanceOf<ProgressViewModel>());
        var vm = (ProgressViewModel)viewResult.Model!;
        Assert.That(vm.TotalQrCodes, Is.EqualTo(3));
        Assert.That(vm.UniqueFound, Is.EqualTo(2));
        Assert.That(vm.Remaining, Is.EqualTo(1));
        Assert.That(vm.ProgressPercent, Is.EqualTo((int)((double)2 / 3 * 100)));
        Assert.That(vm.RecentFinds.Count, Is.GreaterThan(0));
    }

    // Simplified tests for redirect logic - removed complex mocking

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
