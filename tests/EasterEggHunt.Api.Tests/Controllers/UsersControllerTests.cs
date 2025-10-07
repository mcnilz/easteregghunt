using EasterEggHunt.Api.Controllers;
using EasterEggHunt.Application.Services;
using EasterEggHunt.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace EasterEggHunt.Api.Tests.Controllers;

/// <summary>
/// Unit Tests für UsersController
/// </summary>
[TestFixture]
public class UsersControllerTests
{
    private Mock<IUserService> _mockUserService = null!;
    private Mock<ILogger<UsersController>> _mockLogger = null!;
    private UsersController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockUserService = new Mock<IUserService>();
        _mockLogger = new Mock<ILogger<UsersController>>();
        _controller = new UsersController(_mockUserService.Object, _mockLogger.Object);
    }

    [Test]
    public async Task GetActiveUsers_ReturnsOkResult_WithUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User("User 1"),
            new User("User 2")
        };

        _mockUserService.Setup(x => x.GetActiveUsersAsync())
            .ReturnsAsync(users);

        // Act
        var result = await _controller.GetActiveUsers();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult!.Value, Is.EqualTo(users));
    }

    [Test]
    public async Task GetActiveUsers_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        _mockUserService.Setup(x => x.GetActiveUsersAsync())
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        // Act
        var result = await _controller.GetActiveUsers();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public async Task GetUserById_ReturnsOkResult_WhenUserExists()
    {
        // Arrange
        var user = new User("Test User");
        _mockUserService.Setup(x => x.GetUserByIdAsync(1))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.GetUserById(1);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult!.Value, Is.EqualTo(user));
    }

    [Test]
    public async Task GetUserById_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        _mockUserService.Setup(x => x.GetUserByIdAsync(1))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.GetUserById(1);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult!.Value, Is.EqualTo("Benutzer mit ID 1 nicht gefunden"));
    }

    [Test]
    public async Task CreateUser_ReturnsCreatedAtAction_WhenValidRequest()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Name = "Test User"
        };

        var user = new User("Test User");
        _mockUserService.Setup(x => x.CreateUserAsync("Test User"))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.CreateUser(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
        var createdAtResult = result.Result as CreatedAtActionResult;
        Assert.That(createdAtResult!.ActionName, Is.EqualTo(nameof(UsersController.GetUserById)));
        Assert.That(createdAtResult.Value, Is.EqualTo(user));
    }

    [Test]
    public async Task CreateUser_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Name = "" // Invalid - empty name
        };

        _controller.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await _controller.CreateUser(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task CreateUser_ReturnsBadRequest_WhenArgumentExceptionThrown()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Name = "Test User"
        };

        _mockUserService.Setup(x => x.CreateUserAsync("Test User"))
            .ThrowsAsync(new ArgumentException("Invalid argument"));

        // Act
        var result = await _controller.CreateUser(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult!.Value, Is.EqualTo("Invalid argument"));
    }

    [Test]
    public async Task UpdateLastSeen_ReturnsNoContent_WhenUserExists()
    {
        // Arrange
        _mockUserService.Setup(x => x.UpdateUserLastSeenAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateLastSeen(1);

        // Assert
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task UpdateLastSeen_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        _mockUserService.Setup(x => x.UpdateUserLastSeenAsync(1))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.UpdateLastSeen(1);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult!.Value, Is.EqualTo("Benutzer mit ID 1 nicht gefunden"));
    }

    [Test]
    public async Task DeactivateUser_ReturnsNoContent_WhenUserExists()
    {
        // Arrange
        _mockUserService.Setup(x => x.DeactivateUserAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeactivateUser(1);

        // Assert
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task DeactivateUser_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        _mockUserService.Setup(x => x.DeactivateUserAsync(1))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeactivateUser(1);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult!.Value, Is.EqualTo("Benutzer mit ID 1 nicht gefunden"));
    }
}
