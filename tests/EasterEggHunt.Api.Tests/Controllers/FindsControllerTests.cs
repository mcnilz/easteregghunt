using EasterEggHunt.Api.Controllers;
using EasterEggHunt.Application.Services;
using EasterEggHunt.Domain.Entities;
using EasterEggHunterApi.Abstractions.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace EasterEggHunt.Api.Tests.Controllers;

[TestFixture]
public class FindsControllerTests
{
    private Mock<IFindService> _mockFindService = null!;
    private FindsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockFindService = new Mock<IFindService>();
        var mockLogger = new Mock<ILogger<FindsController>>();
        _controller = new FindsController(_mockFindService.Object, mockLogger.Object);
    }

    [Test]
    public async Task GetFindsByQrCodeId_ReturnsOkResult_WithFinds()
    {
        // Arrange
        var qrCodeId = 1;
        var finds = new List<Find>
        {
            new Find(qrCodeId, 1, "127.0.0.1", "TestAgent"),
            new Find(qrCodeId, 2, "127.0.0.2", "TestAgent2")
        };

        _mockFindService.Setup(x => x.GetFindsByQrCodeIdAsync(qrCodeId))
            .ReturnsAsync(finds);

        // Act
        var result = await _controller.GetFindsByQrCodeId(qrCodeId);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult!.Value, Is.EqualTo(finds));
    }

    [Test]
    public async Task GetFindsByQrCodeId_ReturnsOkResult_WithEmptyList()
    {
        // Arrange
        var qrCodeId = 1;
        var finds = new List<Find>();

        _mockFindService.Setup(x => x.GetFindsByQrCodeIdAsync(qrCodeId))
            .ReturnsAsync(finds);

        // Act
        var result = await _controller.GetFindsByQrCodeId(qrCodeId);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult!.Value, Is.EqualTo(finds));
    }

    [Test]
    public async Task GetFindsByQrCodeId_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        var qrCodeId = 1;
        _mockFindService.Setup(x => x.GetFindsByQrCodeIdAsync(qrCodeId))
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        // Act
        var result = await _controller.GetFindsByQrCodeId(qrCodeId);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public async Task GetFindCountByUserId_ReturnsOkResult_WithCount()
    {
        // Arrange
        var userId = 1;
        var count = 5;

        _mockFindService.Setup(x => x.GetFindCountByUserIdAsync(userId))
            .ReturnsAsync(count);

        // Act
        var result = await _controller.GetFindCountByUserId(userId);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult!.Value, Is.EqualTo(count));
    }

    [Test]
    public async Task RegisterFind_ReturnsCreatedAtAction_WhenValidRequest()
    {
        // Arrange
        var request = new RegisterFindRequest
        {
            QrCodeId = 1,
            UserId = 1,
            IpAddress = "127.0.0.1",
            UserAgent = "TestAgent"
        };
        var find = new Find(request.QrCodeId, request.UserId, request.IpAddress, request.UserAgent);

        _mockFindService.Setup(x => x.RegisterFindAsync(
                request.QrCodeId, request.UserId, request.IpAddress, request.UserAgent))
            .ReturnsAsync(find);

        // Act
        var result = await _controller.RegisterFind(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
        var createdAtResult = result.Result as CreatedAtActionResult;
        Assert.That(createdAtResult!.ActionName, Is.EqualTo(nameof(FindsController.GetFindsByUserId)));
        Assert.That(createdAtResult.Value, Is.EqualTo(find));
    }

    [Test]
    public async Task RegisterFind_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        var request = new RegisterFindRequest
        {
            QrCodeId = 0, // Invalid
            UserId = 0,   // Invalid
            IpAddress = "", // Invalid
            UserAgent = ""  // Invalid
        };

        _controller.ModelState.AddModelError("QrCodeId", "QrCodeId is required");
        _controller.ModelState.AddModelError("UserId", "UserId is required");

        // Act
        var result = await _controller.RegisterFind(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task RegisterFind_ReturnsBadRequest_WhenArgumentExceptionThrown()
    {
        // Arrange
        var request = new RegisterFindRequest
        {
            QrCodeId = 1,
            UserId = 1,
            IpAddress = "127.0.0.1",
            UserAgent = "TestAgent"
        };

        _mockFindService.Setup(x => x.RegisterFindAsync(
                request.QrCodeId, request.UserId, request.IpAddress, request.UserAgent))
            .ThrowsAsync(new ArgumentException("Invalid argument"));

        // Act
        var result = await _controller.RegisterFind(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult!.Value, Is.EqualTo("Invalid argument"));
    }

    [Test]
    public async Task RegisterFind_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        var request = new RegisterFindRequest
        {
            QrCodeId = 1,
            UserId = 1,
            IpAddress = "127.0.0.1",
            UserAgent = "TestAgent"
        };

        _mockFindService.Setup(x => x.RegisterFindAsync(
                request.QrCodeId, request.UserId, request.IpAddress, request.UserAgent))
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        // Act
        var result = await _controller.RegisterFind(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public async Task CheckExistingFind_ReturnsOkResult_WhenFindExists()
    {
        // Arrange
        var qrCodeId = 1;
        var userId = 1;
        var find = new Find(qrCodeId, userId, "127.0.0.1", "TestAgent");

        _mockFindService.Setup(x => x.GetExistingFindAsync(qrCodeId, userId))
            .ReturnsAsync(find);

        // Act
        var result = await _controller.CheckExistingFind(qrCodeId, userId);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult!.Value, Is.EqualTo(find));
    }

    [Test]
    public async Task CheckExistingFind_ReturnsOkResult_WhenFindDoesNotExist()
    {
        // Arrange
        var qrCodeId = 1;
        var userId = 1;

        _mockFindService.Setup(x => x.GetExistingFindAsync(qrCodeId, userId))
            .ReturnsAsync((Find?)null);

        // Act
        var result = await _controller.CheckExistingFind(qrCodeId, userId);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult!.Value, Is.Null);
    }

    [Test]
    public async Task CheckExistingFind_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        var qrCodeId = 1;
        var userId = 1;

        _mockFindService.Setup(x => x.GetExistingFindAsync(qrCodeId, userId))
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        // Act
        var result = await _controller.CheckExistingFind(qrCodeId, userId);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
    }
}
