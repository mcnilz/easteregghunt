using EasterEggHunt.Api.Controllers;
using EasterEggHunt.Application.Services;
using EasterEggHunt.Domain.Entities;
using EasterEggHunterApi.Abstractions.Models.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace EasterEggHunt.Api.Tests.Controllers;

/// <summary>
/// Tests für AuthController
/// </summary>
[TestFixture]
public class AuthControllerTests
{
    private Mock<IAuthService> _mockAuthService = null!;
    private Mock<ILogger<AuthController>> _mockLogger = null!;
    private AuthController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockLogger = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(_mockAuthService.Object, _mockLogger.Object);
    }

    [Test]
    public async Task Login_ValidCredentials_ReturnsOkWithLoginResponse()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "admin",
            Password = "password123",
            RememberMe = false
        };

        var adminUser = new AdminUser("admin", "admin@test.com", "hashedpassword");
        adminUser.UpdateLastLogin();

        _mockAuthService.Setup(s => s.AuthenticateAdminAsync(request.Username, request.Password))
            .ReturnsAsync(adminUser);

        // Act
        var result = await _controller.Login(request);

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<LoginResponse>>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo(200));

        var response = okResult.Value as LoginResponse;
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.AdminId, Is.EqualTo(adminUser.Id));
        Assert.That(response.Username, Is.EqualTo(adminUser.Username));
        Assert.That(response.Email, Is.EqualTo(adminUser.Email));
        Assert.That(response.IsActive, Is.EqualTo(adminUser.IsActive));
    }

    [Test]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "admin",
            Password = "wrongpassword",
            RememberMe = false
        };

        _mockAuthService.Setup(s => s.AuthenticateAdminAsync(request.Username, request.Password))
            .ReturnsAsync((AdminUser?)null);

        // Act
        var result = await _controller.Login(request);

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<LoginResponse>>());
        var unauthorizedResult = result.Result as UnauthorizedObjectResult;
        Assert.That(unauthorizedResult, Is.Not.Null);
        Assert.That(unauthorizedResult!.StatusCode, Is.EqualTo(401));
    }

    [Test]
    public async Task Login_InactiveUser_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "admin",
            Password = "password123",
            RememberMe = false
        };

        var adminUser = new AdminUser("admin", "admin@test.com", "hashedpassword");
        adminUser.Deactivate();

        _mockAuthService.Setup(s => s.AuthenticateAdminAsync(request.Username, request.Password))
            .ReturnsAsync(adminUser);

        // Act
        var result = await _controller.Login(request);

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<LoginResponse>>());
        var unauthorizedResult = result.Result as UnauthorizedObjectResult;
        Assert.That(unauthorizedResult, Is.Not.Null);
        Assert.That(unauthorizedResult!.StatusCode, Is.EqualTo(401));
    }

    [Test]
    public async Task Login_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "", // Invalid - empty username
            Password = "password123",
            RememberMe = false
        };

        _controller.ModelState.AddModelError("Username", "Username is required");

        // Act
        var result = await _controller.Login(request);

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<LoginResponse>>());
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task Login_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "admin",
            Password = "password123",
            RememberMe = false
        };

        _mockAuthService.Setup(s => s.AuthenticateAdminAsync(request.Username, request.Password))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _controller.Login(request);

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<LoginResponse>>());
        var statusCodeResult = result.Result as ObjectResult;
        Assert.That(statusCodeResult, Is.Not.Null);
        Assert.That(statusCodeResult!.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public void Validate_ReturnsOk()
    {
        // Act
        var result = _controller.Validate();

        // Assert
        Assert.That(result, Is.InstanceOf<IActionResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public void Logout_ReturnsOk()
    {
        // Act
        var result = _controller.Logout();

        // Assert
        Assert.That(result, Is.InstanceOf<IActionResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo(200));
    }

    #region Constructor Tests
    [Test]
    public void AuthController_Constructor_WithNullAuthService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AuthController(null!, _mockLogger.Object));
    }

    [Test]
    public void AuthController_Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AuthController(_mockAuthService.Object, null!));
    }

    [Test]
    public void AuthController_Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Act
        var controller = new AuthController(_mockAuthService.Object, _mockLogger.Object);

        // Assert
        Assert.That(controller, Is.Not.Null);
    }
    #endregion
}
