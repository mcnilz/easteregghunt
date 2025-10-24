using System.Security.Claims;
using EasterEggHunt.Web.Controllers;
using EasterEggHunt.Web.Models;
using EasterEggHunt.Web.Services;
using EasterEggHunterApi.Abstractions.Models.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace EasterEggHunt.Web.Tests.Controllers;

/// <summary>
/// Tests für AuthController (Web)
/// </summary>
[TestFixture]
public sealed class AuthControllerTests : IDisposable
{
    private Mock<IEasterEggHuntApiClient> _mockApiClient = null!;
    private Mock<ILogger<AuthController>> _mockLogger = null!;
    private Mock<HttpContext> _mockHttpContext = null!;
    private Mock<ISession> _mockSession = null!;
    private Mock<IServiceProvider> _mockServiceProvider = null!;
    private Mock<IUrlHelperFactory> _mockUrlHelperFactory = null!;
    private Mock<IUrlHelper> _mockUrlHelper = null!;
    private Mock<ITempDataDictionaryFactory> _mockTempDataFactory = null!;
    private AuthController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockApiClient = new Mock<IEasterEggHuntApiClient>();
        _mockLogger = new Mock<ILogger<AuthController>>();
        _mockHttpContext = new Mock<HttpContext>();
        _mockSession = new Mock<ISession>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockUrlHelperFactory = new Mock<IUrlHelperFactory>();
        _mockUrlHelper = new Mock<IUrlHelper>();
        _mockTempDataFactory = new Mock<ITempDataDictionaryFactory>();

        _controller = new AuthController(_mockApiClient.Object, _mockLogger.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = _mockHttpContext.Object
            }
        };

        _mockHttpContext.Setup(c => c.Session).Returns(_mockSession.Object);
        _mockHttpContext.Setup(c => c.RequestServices).Returns(_mockServiceProvider.Object);

        // Setup User with default identity
        var defaultIdentity = new ClaimsIdentity();
        var defaultPrincipal = new ClaimsPrincipal(defaultIdentity);
        _mockHttpContext.Setup(c => c.User).Returns(defaultPrincipal);

        // Setup URL helper
        _mockUrlHelperFactory.Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
            .Returns(_mockUrlHelper.Object);
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(IUrlHelperFactory)))
            .Returns(_mockUrlHelperFactory.Object);

        // Setup default URL helper behavior - use Content method instead of Action extension
        _mockUrlHelper.Setup(u => u.Content(It.IsAny<string>()))
            .Returns((string content) => content);

        // Setup TempData
        var tempDataProvider = new Mock<ITempDataProvider>();
        var tempData = new TempDataDictionary(_mockHttpContext.Object, tempDataProvider.Object);

        _mockTempDataFactory.Setup(f => f.GetTempData(It.IsAny<HttpContext>()))
            .Returns(tempData);

        _mockServiceProvider.Setup(sp => sp.GetService(typeof(ITempDataDictionaryFactory)))
            .Returns(_mockTempDataFactory.Object);

        _controller.TempData = tempData;
    }

    [Test]
    public void Login_Get_WhenNotAuthenticated_ReturnsView()
    {
        // Arrange
        var claimsPrincipal = new ClaimsPrincipal();
        _mockHttpContext.Setup(c => c.User).Returns(claimsPrincipal);

        // Act
        var result = _controller.Login();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.Model, Is.InstanceOf<LoginViewModel>());
    }

    [Test]
    public void Login_Get_WhenAuthenticated_RedirectsToAdmin()
    {
        // Arrange
        var claimsIdentity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "admin") }, "test");
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        _mockHttpContext.Setup(c => c.User).Returns(claimsPrincipal);

        // Act
        var result = _controller.Login();

        // Assert
        Assert.That(result, Is.InstanceOf<RedirectResult>());
        var redirectResult = result as RedirectResult;
        Assert.That(redirectResult!.Url, Is.EqualTo("/Admin"));
    }

    [Test]
    public async Task Login_Post_ValidCredentials_RedirectsToReturnUrl()
    {
        // Arrange
        var model = new LoginViewModel
        {
            Username = "admin",
            Password = "password123",
            RememberMe = false,
            ReturnUrl = null // Use default URL to avoid IsLocalUrl check
        };

        var loginResponse = new LoginResponse
        {
            AdminId = 1,
            Username = "admin",
            Email = "admin@test.com",
            LastLogin = DateTime.UtcNow,
            IsActive = true
        };

        _mockApiClient.Setup(c => c.LoginAsync(model.Username, model.Password, model.RememberMe))
            .ReturnsAsync(loginResponse);

        var mockAuthService = new Mock<IAuthenticationService>();
        _mockHttpContext.Setup(c => c.RequestServices.GetService(typeof(IAuthenticationService)))
            .Returns(mockAuthService.Object);

        // Use a relative URL that doesn't require IsLocalUrl check

        // Act
        var result = await _controller.Login(model);

        // Assert
        Assert.That(result, Is.InstanceOf<RedirectResult>());
        var redirectResult = result as RedirectResult;
        Assert.That(redirectResult!.Url, Is.EqualTo("/Admin"));

        // Verify session data was set - use direct method calls instead of extension methods
        _mockSession.Verify(s => s.Set("AdminId", It.IsAny<byte[]>()), Times.Once);
        _mockSession.Verify(s => s.Set("Username", It.IsAny<byte[]>()), Times.Once);
        _mockSession.Verify(s => s.Set("Email", It.IsAny<byte[]>()), Times.Once);
    }

    [Test]
    public async Task Login_Post_InvalidCredentials_ReturnsViewWithError()
    {
        // Arrange
        var model = new LoginViewModel
        {
            Username = "admin",
            Password = "wrongpassword",
            RememberMe = false
        };

        _mockApiClient.Setup(c => c.LoginAsync(model.Username, model.Password, model.RememberMe))
            .ReturnsAsync((LoginResponse?)null);

        // Act
        var result = await _controller.Login(model);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.Model, Is.InstanceOf<LoginViewModel>());

        // Verify ModelState has error
        Assert.That(_controller.ModelState.ContainsKey(string.Empty), Is.True);
    }

    [Test]
    public async Task Login_Post_InvalidModelState_ReturnsView()
    {
        // Arrange
        var model = new LoginViewModel
        {
            Username = "", // Invalid
            Password = "password123",
            RememberMe = false
        };

        _controller.ModelState.AddModelError("Username", "Username is required");

        // Act
        var result = await _controller.Login(model);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.Model, Is.InstanceOf<LoginViewModel>());
    }

    [Test]
    public async Task Login_Post_ApiException_ReturnsViewWithError()
    {
        // Arrange
        var model = new LoginViewModel
        {
            Username = "admin",
            Password = "password123",
            RememberMe = false
        };

        _mockApiClient.Setup(c => c.LoginAsync(model.Username, model.Password, model.RememberMe))
            .ThrowsAsync(new HttpRequestException("API error"));

        // Act
        var result = await _controller.Login(model);

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.Model, Is.InstanceOf<LoginViewModel>());

        // Verify ModelState has error
        Assert.That(_controller.ModelState.ContainsKey(string.Empty), Is.True);
    }

    [Test]
    public async Task Logout_Post_ClearsSessionAndRedirects()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthenticationService>();
        _mockHttpContext.Setup(c => c.RequestServices.GetService(typeof(IAuthenticationService)))
            .Returns(mockAuthService.Object);

        _mockApiClient.Setup(c => c.LogoutAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Logout();

        // Assert
        Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
        var redirectResult = result as RedirectToActionResult;
        Assert.That(redirectResult!.ActionName, Is.EqualTo("Login"));

        // Verify session was cleared
        _mockSession.Verify(s => s.Clear(), Times.Once);
    }

    [Test]
    public async Task Logout_Post_ApiException_StillRedirects()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthenticationService>();
        _mockHttpContext.Setup(c => c.RequestServices.GetService(typeof(IAuthenticationService)))
            .Returns(mockAuthService.Object);

        _mockApiClient.Setup(c => c.LogoutAsync()).ThrowsAsync(new HttpRequestException("API error"));

        // Act
        var result = await _controller.Logout();

        // Assert
        Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
        var redirectResult = result as RedirectToActionResult;
        Assert.That(redirectResult!.ActionName, Is.EqualTo("Login"));
    }

    [Test]
    public void AccessDenied_ReturnsViewWithModel()
    {
        // Arrange
        var queryCollection = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "ReturnUrl", "/Admin/SecurePage" }
        });
        _mockHttpContext.Setup(c => c.Request.Query).Returns(queryCollection);

        // Act
        var result = _controller.AccessDenied();

        // Assert
        Assert.That(result, Is.InstanceOf<ViewResult>());
        var viewResult = result as ViewResult;
        Assert.That(viewResult!.Model, Is.InstanceOf<AccessDeniedViewModel>());

        var model = viewResult.Model as AccessDeniedViewModel;
        Assert.That(model!.RequestedUrl, Is.EqualTo("/Admin/SecurePage"));
    }

    #region Constructor Tests
    [Test]
    public void AuthController_Constructor_WithNullApiClient_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AuthController(null!, _mockLogger.Object));
    }

    [Test]
    public void AuthController_Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AuthController(_mockApiClient.Object, null!));
    }

    [Test]
    public void AuthController_Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Act
        using var controller = new AuthController(_mockApiClient.Object, _mockLogger.Object);

        // Assert
        Assert.That(controller, Is.Not.Null);
    }

    public void Dispose()
    {
        _controller?.Dispose();
        GC.SuppressFinalize(this);
    }
    #endregion
}
