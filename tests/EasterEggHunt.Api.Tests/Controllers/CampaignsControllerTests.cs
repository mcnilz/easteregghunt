using EasterEggHunt.Api.Controllers;
using EasterEggHunt.Application.Services;
using EasterEggHunt.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace EasterEggHunt.Api.Tests.Controllers;

/// <summary>
/// Unit Tests für CampaignsController
/// </summary>
[TestFixture]
public class CampaignsControllerTests
{
    private Mock<ICampaignService> _mockCampaignService = null!;
    private Mock<ILogger<CampaignsController>> _mockLogger = null!;
    private CampaignsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockCampaignService = new Mock<ICampaignService>();
        _mockLogger = new Mock<ILogger<CampaignsController>>();
        _controller = new CampaignsController(_mockCampaignService.Object, _mockLogger.Object);
    }

    [Test]
    public async Task GetActiveCampaigns_ReturnsOkResult_WithCampaigns()
    {
        // Arrange
        var campaigns = new List<Campaign>
        {
            new Campaign("Test Campaign 1", "Description 1", "Admin"),
            new Campaign("Test Campaign 2", "Description 2", "Admin")
        };

        _mockCampaignService.Setup(x => x.GetActiveCampaignsAsync())
            .ReturnsAsync(campaigns);

        // Act
        var result = await _controller.GetActiveCampaigns();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult!.Value, Is.EqualTo(campaigns));
    }

    [Test]
    public async Task GetActiveCampaigns_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        _mockCampaignService.Setup(x => x.GetActiveCampaignsAsync())
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        // Act
        var result = await _controller.GetActiveCampaigns();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public async Task GetCampaign_ReturnsOkResult_WhenCampaignExists()
    {
        // Arrange
        var campaign = new Campaign("Test Campaign", "Description", "Admin");
        _mockCampaignService.Setup(x => x.GetCampaignByIdAsync(1))
            .ReturnsAsync(campaign);

        // Act
        var result = await _controller.GetCampaign(1);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult!.Value, Is.EqualTo(campaign));
    }

    [Test]
    public async Task GetCampaign_ReturnsNotFound_WhenCampaignDoesNotExist()
    {
        // Arrange
        _mockCampaignService.Setup(x => x.GetCampaignByIdAsync(1))
            .ReturnsAsync((Campaign?)null);

        // Act
        var result = await _controller.GetCampaign(1);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult!.Value, Is.EqualTo("Kampagne mit ID 1 nicht gefunden"));
    }

    [Test]
    public async Task CreateCampaign_ReturnsCreatedAtAction_WhenValidRequest()
    {
        // Arrange
        var request = new CreateCampaignRequest
        {
            Name = "Test Campaign",
            Description = "Test Description",
            CreatedBy = "Admin"
        };

        var campaign = new Campaign("Test Campaign", "Test Description", "Admin");
        _mockCampaignService.Setup(x => x.CreateCampaignAsync("Test Campaign", "Test Description", "Admin"))
            .ReturnsAsync(campaign);

        // Act
        var result = await _controller.CreateCampaign(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
        var createdAtResult = result.Result as CreatedAtActionResult;
        Assert.That(createdAtResult!.ActionName, Is.EqualTo(nameof(CampaignsController.GetCampaign)));
        Assert.That(createdAtResult.Value, Is.EqualTo(campaign));
    }

    [Test]
    public async Task CreateCampaign_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        var request = new CreateCampaignRequest
        {
            Name = "", // Invalid - empty name
            Description = "Test Description",
            CreatedBy = "Admin"
        };

        _controller.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await _controller.CreateCampaign(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task CreateCampaign_ReturnsBadRequest_WhenArgumentExceptionThrown()
    {
        // Arrange
        var request = new CreateCampaignRequest
        {
            Name = "Test Campaign",
            Description = "Test Description",
            CreatedBy = "Admin"
        };

        _mockCampaignService.Setup(x => x.CreateCampaignAsync("Test Campaign", "Test Description", "Admin"))
            .ThrowsAsync(new ArgumentException("Invalid argument"));

        // Act
        var result = await _controller.CreateCampaign(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult!.Value, Is.EqualTo("Invalid argument"));
    }

    [Test]
    public async Task UpdateCampaign_ReturnsNoContent_WhenCampaignExists()
    {
        // Arrange
        var request = new UpdateCampaignRequest
        {
            Name = "Updated Campaign",
            Description = "Updated Description"
        };

        _mockCampaignService.Setup(x => x.UpdateCampaignAsync(1, "Updated Campaign", "Updated Description"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateCampaign(1, request);

        // Assert
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task UpdateCampaign_ReturnsNotFound_WhenCampaignDoesNotExist()
    {
        // Arrange
        var request = new UpdateCampaignRequest
        {
            Name = "Updated Campaign",
            Description = "Updated Description"
        };

        _mockCampaignService.Setup(x => x.UpdateCampaignAsync(1, "Updated Campaign", "Updated Description"))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.UpdateCampaign(1, request);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult!.Value, Is.EqualTo("Kampagne mit ID 1 nicht gefunden"));
    }

    [Test]
    public async Task ActivateCampaign_ReturnsNoContent_WhenCampaignExists()
    {
        // Arrange
        _mockCampaignService.Setup(x => x.ActivateCampaignAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ActivateCampaign(1);

        // Assert
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task ActivateCampaign_ReturnsNotFound_WhenCampaignDoesNotExist()
    {
        // Arrange
        _mockCampaignService.Setup(x => x.ActivateCampaignAsync(1))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.ActivateCampaign(1);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult!.Value, Is.EqualTo("Kampagne mit ID 1 nicht gefunden"));
    }

    [Test]
    public async Task DeactivateCampaign_ReturnsNoContent_WhenCampaignExists()
    {
        // Arrange
        _mockCampaignService.Setup(x => x.DeactivateCampaignAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeactivateCampaign(1);

        // Assert
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task DeactivateCampaign_ReturnsNotFound_WhenCampaignDoesNotExist()
    {
        // Arrange
        _mockCampaignService.Setup(x => x.DeactivateCampaignAsync(1))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeactivateCampaign(1);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult!.Value, Is.EqualTo("Kampagne mit ID 1 nicht gefunden"));
    }
}
