using EasterEggHunt.Api.Controllers;
using EasterEggHunt.Application.Requests;
using EasterEggHunt.Application.Services;
using EasterEggHunt.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace EasterEggHunt.Api.Tests.Controllers;

/// <summary>
/// Unit Tests für QrCodesController
/// </summary>
[TestFixture]
public class QrCodesControllerTests
{
    private Mock<IQrCodeService> _mockQrCodeService = null!;
    private Mock<ILogger<QrCodesController>> _mockLogger = null!;
    private QrCodesController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockQrCodeService = new Mock<IQrCodeService>();
        _mockLogger = new Mock<ILogger<QrCodesController>>();
        _controller = new QrCodesController(_mockQrCodeService.Object, _mockLogger.Object);
    }

    [Test]
    public async Task GetQrCodesByCampaign_ReturnsOkResult_WithQrCodes()
    {
        // Arrange
        var campaignId = 1;
        var qrCodes = new List<QrCode>
        {
            new QrCode(campaignId, "QR Code 1", "Description 1", "Note 1"),
            new QrCode(campaignId, "QR Code 2", "Description 2", "Note 2")
        };

        _mockQrCodeService.Setup(x => x.GetQrCodesByCampaignIdAsync(campaignId))
            .ReturnsAsync(qrCodes);

        // Act
        var result = await _controller.GetQrCodesByCampaign(campaignId);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult!.Value, Is.EqualTo(qrCodes));
    }

    [Test]
    public async Task GetQrCodesByCampaign_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        var campaignId = 1;
        _mockQrCodeService.Setup(x => x.GetQrCodesByCampaignIdAsync(campaignId))
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        // Act
        var result = await _controller.GetQrCodesByCampaign(campaignId);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public async Task GetQrCode_ReturnsOkResult_WhenQrCodeExists()
    {
        // Arrange
        var qrCode = new QrCode(1, "Test QR Code", "Test Description", "Test Note");
        _mockQrCodeService.Setup(x => x.GetQrCodeByIdAsync(1))
            .ReturnsAsync(qrCode);

        // Act
        var result = await _controller.GetQrCode(1);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult!.Value, Is.EqualTo(qrCode));
    }

    [Test]
    public async Task GetQrCode_ReturnsNotFound_WhenQrCodeDoesNotExist()
    {
        // Arrange
        _mockQrCodeService.Setup(x => x.GetQrCodeByIdAsync(1))
            .ReturnsAsync((QrCode?)null);

        // Act
        var result = await _controller.GetQrCode(1);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult!.Value, Is.EqualTo("QR-Code mit ID 1 nicht gefunden"));
    }

    [Test]
    public async Task CreateQrCode_ReturnsCreatedAtAction_WhenValidRequest()
    {
        // Arrange
        var request = new CreateQrCodeApiRequest
        {
            CampaignId = 1,
            Title = "Test QR Code",
            InternalNote = "Test Note"
        };

        var qrCode = new QrCode(1, "Test QR Code", "Test Description", "Test Note");
        _mockQrCodeService.Setup(x => x.CreateQrCodeAsync(It.IsAny<CreateQrCodeRequest>()))
            .ReturnsAsync(qrCode);

        // Act
        var result = await _controller.CreateQrCode(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
        var createdAtResult = result.Result as CreatedAtActionResult;
        Assert.That(createdAtResult!.ActionName, Is.EqualTo(nameof(QrCodesController.GetQrCode)));
        Assert.That(createdAtResult.Value, Is.EqualTo(qrCode));
    }

    [Test]
    public async Task CreateQrCode_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        var request = new CreateQrCodeApiRequest
        {
            CampaignId = 0, // Invalid - must be > 0
            Title = "", // Invalid - empty title
            InternalNote = "Test Note"
        };

        _controller.ModelState.AddModelError("CampaignId", "CampaignId must be greater than 0");
        _controller.ModelState.AddModelError("Title", "Title is required");

        // Act
        var result = await _controller.CreateQrCode(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task CreateQrCode_ReturnsBadRequest_WhenArgumentExceptionThrown()
    {
        // Arrange
        var request = new CreateQrCodeApiRequest
        {
            CampaignId = 1,
            Title = "Test QR Code",
            InternalNote = "Test Note"
        };

        _mockQrCodeService.Setup(x => x.CreateQrCodeAsync(It.IsAny<CreateQrCodeRequest>()))
            .ThrowsAsync(new ArgumentException("Invalid argument"));

        // Act
        var result = await _controller.CreateQrCode(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult!.Value, Is.EqualTo("Invalid argument"));
    }

    [Test]
    public async Task UpdateQrCode_ReturnsNoContent_WhenQrCodeExists()
    {
        // Arrange
        var request = new UpdateQrCodeApiRequest
        {
            Title = "Updated QR Code",
            InternalNote = "Updated Note"
        };

        _mockQrCodeService.Setup(x => x.UpdateQrCodeAsync(It.IsAny<UpdateQrCodeRequest>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateQrCode(1, request);

        // Assert
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task UpdateQrCode_ReturnsNotFound_WhenQrCodeDoesNotExist()
    {
        // Arrange
        var request = new UpdateQrCodeApiRequest
        {
            Title = "Updated QR Code",
            InternalNote = "Updated Note"
        };

        _mockQrCodeService.Setup(x => x.UpdateQrCodeAsync(It.IsAny<UpdateQrCodeRequest>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.UpdateQrCode(1, request);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult!.Value, Is.EqualTo("QR-Code mit ID 1 nicht gefunden"));
    }

    [Test]
    public async Task SetSortOrder_ReturnsNoContent_WhenQrCodeExists()
    {
        // Arrange
        var request = new SetSortOrderRequest
        {
            SortOrder = 5
        };

        _mockQrCodeService.Setup(x => x.SetQrCodeSortOrderAsync(1, 5))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.SetSortOrder(1, request);

        // Assert
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task SetSortOrder_ReturnsNotFound_WhenQrCodeDoesNotExist()
    {
        // Arrange
        var request = new SetSortOrderRequest
        {
            SortOrder = 5
        };

        _mockQrCodeService.Setup(x => x.SetQrCodeSortOrderAsync(1, 5))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.SetSortOrder(1, request);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult!.Value, Is.EqualTo("QR-Code mit ID 1 nicht gefunden"));
    }

    [Test]
    public async Task ActivateQrCode_ReturnsNoContent_WhenQrCodeExists()
    {
        // Arrange
        _mockQrCodeService.Setup(x => x.ActivateQrCodeAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ActivateQrCode(1);

        // Assert
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task ActivateQrCode_ReturnsNotFound_WhenQrCodeDoesNotExist()
    {
        // Arrange
        _mockQrCodeService.Setup(x => x.ActivateQrCodeAsync(1))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.ActivateQrCode(1);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult!.Value, Is.EqualTo("QR-Code mit ID 1 nicht gefunden"));
    }

    [Test]
    public async Task DeactivateQrCode_ReturnsNoContent_WhenQrCodeExists()
    {
        // Arrange
        _mockQrCodeService.Setup(x => x.DeactivateQrCodeAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeactivateQrCode(1);

        // Assert
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task DeactivateQrCode_ReturnsNotFound_WhenQrCodeDoesNotExist()
    {
        // Arrange
        _mockQrCodeService.Setup(x => x.DeactivateQrCodeAsync(1))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeactivateQrCode(1);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult!.Value, Is.EqualTo("QR-Code mit ID 1 nicht gefunden"));
    }

    [Test]
    public async Task GetQrCodeByCode_ReturnsOkResult_WhenQrCodeExists()
    {
        // Arrange
        var code = "test-code-123";
        var qrCode = new QrCode(1, "Test QR Code", "Test Description", "Test Note")
        {
            Code = code
        };

        _mockQrCodeService.Setup(x => x.GetQrCodeByCodeAsync(code))
            .ReturnsAsync(qrCode);

        // Act
        var result = await _controller.GetQrCodeByCode(code);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult!.Value, Is.EqualTo(qrCode));
    }

    [Test]
    public async Task GetQrCodeByCode_ReturnsNotFound_WhenQrCodeDoesNotExist()
    {
        // Arrange
        var code = "non-existent-code";
        _mockQrCodeService.Setup(x => x.GetQrCodeByCodeAsync(code))
            .ReturnsAsync((QrCode?)null);

        // Act
        var result = await _controller.GetQrCodeByCode(code);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult!.Value, Is.EqualTo($"QR-Code mit UniqueUrl '{code}' nicht gefunden"));
    }

    [Test]
    public async Task GetQrCodeByCode_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        var code = "test-code-123";
        _mockQrCodeService.Setup(x => x.GetQrCodeByCodeAsync(code))
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        // Act
        var result = await _controller.GetQrCodeByCode(code);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
    }
}
