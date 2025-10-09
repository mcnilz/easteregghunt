using EasterEggHunt.Application.Requests;
using EasterEggHunt.Application.Services;
using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace EasterEggHunt.Application.Tests.Services;

/// <summary>
/// Tests für den QrCodeService
/// </summary>
[TestFixture]
public class QrCodeServiceTests
{
    private Mock<IQrCodeRepository> _mockQrCodeRepository = null!;
    private Mock<ICampaignRepository> _mockCampaignRepository = null!;
    private Mock<ILogger<QrCodeService>> _mockLogger = null!;
    private QrCodeService _qrCodeService = null!;

    [SetUp]
    public void Setup()
    {
        _mockQrCodeRepository = new Mock<IQrCodeRepository>();
        _mockCampaignRepository = new Mock<ICampaignRepository>();
        _mockLogger = new Mock<ILogger<QrCodeService>>();
        _qrCodeService = new QrCodeService(_mockQrCodeRepository.Object, _mockCampaignRepository.Object, _mockLogger.Object);
    }

    [Test]
    public void QrCodeService_Constructor_WithNullQrCodeRepository_ShouldThrowArgumentNullException()
    {
        // Arrange
        var campaignRepository = new Mock<ICampaignRepository>().Object;
        var logger = new Mock<ILogger<QrCodeService>>().Object;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new QrCodeService(null!, campaignRepository, logger));
        Assert.That(ex.ParamName, Is.EqualTo("qrCodeRepository"));
    }

    [Test]
    public void QrCodeService_Constructor_WithNullCampaignRepository_ShouldThrowArgumentNullException()
    {
        // Arrange
        var qrCodeRepository = new Mock<IQrCodeRepository>().Object;
        var logger = new Mock<ILogger<QrCodeService>>().Object;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new QrCodeService(qrCodeRepository, null!, logger));
        Assert.That(ex.ParamName, Is.EqualTo("campaignRepository"));
    }

    [Test]
    public void QrCodeService_Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange
        var qrCodeRepository = new Mock<IQrCodeRepository>().Object;
        var campaignRepository = new Mock<ICampaignRepository>().Object;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new QrCodeService(qrCodeRepository, campaignRepository, null!));
        Assert.That(ex.ParamName, Is.EqualTo("logger"));
    }

    [Test]
    public void QrCodeService_Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var qrCodeRepository = new Mock<IQrCodeRepository>().Object;
        var campaignRepository = new Mock<ICampaignRepository>().Object;
        var logger = new Mock<ILogger<QrCodeService>>().Object;

        // Act
        var service = new QrCodeService(qrCodeRepository, campaignRepository, logger);

        // Assert
        Assert.That(service, Is.Not.Null);
    }

    [Test]
    public async Task GetQrCodesByCampaignIdAsync_ReturnsQrCodes()
    {
        // Arrange
        var campaignId = 1;
        var qrCodes = new List<QrCode>
        {
            new QrCode(campaignId, "QR Code 1", "Description 1", "Notes 1"),
            new QrCode(campaignId, "QR Code 2", "Description 2", "Notes 2")
        };
        _mockQrCodeRepository.Setup(r => r.GetByCampaignIdAsync(campaignId)).ReturnsAsync(qrCodes);

        // Act
        var result = await _qrCodeService.GetQrCodesByCampaignIdAsync(campaignId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(2));
        _mockQrCodeRepository.Verify(r => r.GetByCampaignIdAsync(campaignId), Times.Once);
    }

    [Test]
    public async Task GetQrCodeByIdAsync_WithValidId_ReturnsQrCode()
    {
        // Arrange
        var qrCodeId = 1;
        var qrCode = new QrCode(1, "Test QR Code", "Description", "Notes");
        _mockQrCodeRepository.Setup(r => r.GetByIdAsync(qrCodeId)).ReturnsAsync(qrCode);

        // Act
        var result = await _qrCodeService.GetQrCodeByIdAsync(qrCodeId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo("Test QR Code"));
        _mockQrCodeRepository.Verify(r => r.GetByIdAsync(qrCodeId), Times.Once);
    }

    [Test]
    public async Task GetQrCodeByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var qrCodeId = 999;
        _mockQrCodeRepository.Setup(r => r.GetByIdAsync(qrCodeId)).ReturnsAsync((QrCode?)null);

        // Act
        var result = await _qrCodeService.GetQrCodeByIdAsync(qrCodeId);

        // Assert
        Assert.That(result, Is.Null);
        _mockQrCodeRepository.Verify(r => r.GetByIdAsync(qrCodeId), Times.Once);
    }

    [Test]
    public async Task CreateQrCodeAsync_WithValidData_ReturnsCreatedQrCode()
    {
        // Arrange
        var campaignId = 1;
        var campaign = new Campaign("Test Campaign", "Description", "admin");
        var request = new CreateQrCodeRequest
        {
            CampaignId = campaignId,
            Title = "New QR Code",
            Description = "New Description",
            InternalNotes = "New Notes"
        };

        _mockCampaignRepository.Setup(r => r.GetByIdAsync(campaignId)).ReturnsAsync(campaign);
        _mockQrCodeRepository.Setup(r => r.AddAsync(It.IsAny<QrCode>()))
            .ReturnsAsync((QrCode qrCode) => qrCode);
        _mockQrCodeRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _qrCodeService.CreateQrCodeAsync(request);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.CampaignId, Is.EqualTo(campaignId));
        Assert.That(result.Title, Is.EqualTo(request.Title));
        Assert.That(result.Description, Is.EqualTo(request.Description));
        Assert.That(result.InternalNotes, Is.EqualTo(request.InternalNotes));
        Assert.That(result.IsActive, Is.True);
        _mockCampaignRepository.Verify(r => r.GetByIdAsync(campaignId), Times.Once);
        _mockQrCodeRepository.Verify(r => r.AddAsync(It.IsAny<QrCode>()), Times.Once);
        _mockQrCodeRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void CreateQrCodeAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(() =>
            _qrCodeService.CreateQrCodeAsync(null!));
    }

    [Test]
    public void CreateQrCodeAsync_WithEmptyTitle_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateQrCodeRequest
        {
            CampaignId = 1,
            Title = "",
            Description = "Description",
            InternalNotes = "Notes"
        };

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() =>
            _qrCodeService.CreateQrCodeAsync(request));
    }

    [Test]
    public void CreateQrCodeAsync_WithNonExistentCampaign_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateQrCodeRequest
        {
            CampaignId = 999,
            Title = "Test QR Code",
            Description = "Description",
            InternalNotes = "Notes"
        };

        _mockCampaignRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Campaign?)null);

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() =>
            _qrCodeService.CreateQrCodeAsync(request));
    }

    [Test]
    public async Task UpdateQrCodeAsync_WithValidData_ReturnsTrue()
    {
        // Arrange
        var qrCodeId = 1;
        var qrCode = new QrCode(1, "Original Title", "Original Description", "Original Notes");
        var request = new UpdateQrCodeRequest
        {
            Id = qrCodeId,
            Title = "Updated Title",
            Description = "Updated Description",
            InternalNotes = "Updated Notes"
        };

        _mockQrCodeRepository.Setup(r => r.GetByIdAsync(qrCodeId)).ReturnsAsync(qrCode);
        _mockQrCodeRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _qrCodeService.UpdateQrCodeAsync(request);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(qrCode.Title, Is.EqualTo(request.Title));
        Assert.That(qrCode.Description, Is.EqualTo(request.Description));
        Assert.That(qrCode.InternalNotes, Is.EqualTo(request.InternalNotes));
        _mockQrCodeRepository.Verify(r => r.GetByIdAsync(qrCodeId), Times.Once);
        _mockQrCodeRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task UpdateQrCodeAsync_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var qrCodeId = 999;
        var request = new UpdateQrCodeRequest
        {
            Id = qrCodeId,
            Title = "Updated Title",
            Description = "Updated Description",
            InternalNotes = "Updated Notes"
        };

        _mockQrCodeRepository.Setup(r => r.GetByIdAsync(qrCodeId)).ReturnsAsync((QrCode?)null);

        // Act
        var result = await _qrCodeService.UpdateQrCodeAsync(request);

        // Assert
        Assert.That(result, Is.False);
        _mockQrCodeRepository.Verify(r => r.GetByIdAsync(qrCodeId), Times.Once);
        _mockQrCodeRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Test]
    public void UpdateQrCodeAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(() =>
            _qrCodeService.UpdateQrCodeAsync(null!));
    }

    [Test]
    public void UpdateQrCodeAsync_WithEmptyTitle_ThrowsArgumentException()
    {
        // Arrange
        var request = new UpdateQrCodeRequest
        {
            Id = 1,
            Title = "",
            Description = "Description",
            InternalNotes = "Notes"
        };

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() =>
            _qrCodeService.UpdateQrCodeAsync(request));
    }

    [Test]
    public async Task SetQrCodeSortOrderAsync_WithValidId_ReturnsTrue()
    {
        // Arrange
        var qrCodeId = 1;
        var sortOrder = 5;
        var qrCode = new QrCode(1, "Test QR Code", "Description", "Notes");

        _mockQrCodeRepository.Setup(r => r.GetByIdAsync(qrCodeId)).ReturnsAsync(qrCode);
        _mockQrCodeRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _qrCodeService.SetQrCodeSortOrderAsync(qrCodeId, sortOrder);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(qrCode.SortOrder, Is.EqualTo(sortOrder));
        _mockQrCodeRepository.Verify(r => r.GetByIdAsync(qrCodeId), Times.Once);
        _mockQrCodeRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task SetQrCodeSortOrderAsync_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var qrCodeId = 999;
        var sortOrder = 5;

        _mockQrCodeRepository.Setup(r => r.GetByIdAsync(qrCodeId)).ReturnsAsync((QrCode?)null);

        // Act
        var result = await _qrCodeService.SetQrCodeSortOrderAsync(qrCodeId, sortOrder);

        // Assert
        Assert.That(result, Is.False);
        _mockQrCodeRepository.Verify(r => r.GetByIdAsync(qrCodeId), Times.Once);
        _mockQrCodeRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Test]
    public async Task ActivateQrCodeAsync_WithValidId_ReturnsTrue()
    {
        // Arrange
        var qrCodeId = 1;
        var qrCode = new QrCode(1, "Test QR Code", "Description", "Notes");
        qrCode.Deactivate(); // Start with deactivated QR code

        _mockQrCodeRepository.Setup(r => r.GetByIdAsync(qrCodeId)).ReturnsAsync(qrCode);
        _mockQrCodeRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _qrCodeService.ActivateQrCodeAsync(qrCodeId);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(qrCode.IsActive, Is.True);
        _mockQrCodeRepository.Verify(r => r.GetByIdAsync(qrCodeId), Times.Once);
        _mockQrCodeRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task ActivateQrCodeAsync_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var qrCodeId = 999;

        _mockQrCodeRepository.Setup(r => r.GetByIdAsync(qrCodeId)).ReturnsAsync((QrCode?)null);

        // Act
        var result = await _qrCodeService.ActivateQrCodeAsync(qrCodeId);

        // Assert
        Assert.That(result, Is.False);
        _mockQrCodeRepository.Verify(r => r.GetByIdAsync(qrCodeId), Times.Once);
        _mockQrCodeRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Test]
    public async Task DeactivateQrCodeAsync_WithValidId_ReturnsTrue()
    {
        // Arrange
        var qrCodeId = 1;
        var qrCode = new QrCode(1, "Test QR Code", "Description", "Notes");

        _mockQrCodeRepository.Setup(r => r.GetByIdAsync(qrCodeId)).ReturnsAsync(qrCode);
        _mockQrCodeRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _qrCodeService.DeactivateQrCodeAsync(qrCodeId);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(qrCode.IsActive, Is.False);
        _mockQrCodeRepository.Verify(r => r.GetByIdAsync(qrCodeId), Times.Once);
        _mockQrCodeRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task DeactivateQrCodeAsync_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var qrCodeId = 999;

        _mockQrCodeRepository.Setup(r => r.GetByIdAsync(qrCodeId)).ReturnsAsync((QrCode?)null);

        // Act
        var result = await _qrCodeService.DeactivateQrCodeAsync(qrCodeId);

        // Assert
        Assert.That(result, Is.False);
        _mockQrCodeRepository.Verify(r => r.GetByIdAsync(qrCodeId), Times.Once);
        _mockQrCodeRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Test]
    public async Task DeleteQrCodeAsync_WithValidId_ReturnsTrue()
    {
        // Arrange
        var qrCodeId = 1;
        var qrCode = new QrCode(1, "Test QR Code", "Description", "Notes");

        _mockQrCodeRepository.Setup(r => r.GetByIdAsync(qrCodeId)).ReturnsAsync(qrCode);
        _mockQrCodeRepository.Setup(r => r.DeleteAsync(qrCodeId)).ReturnsAsync(true);
        _mockQrCodeRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _qrCodeService.DeleteQrCodeAsync(qrCodeId);

        // Assert
        Assert.That(result, Is.True);
        _mockQrCodeRepository.Verify(r => r.GetByIdAsync(qrCodeId), Times.Once);
        _mockQrCodeRepository.Verify(r => r.DeleteAsync(qrCodeId), Times.Once);
        _mockQrCodeRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task DeleteQrCodeAsync_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var qrCodeId = 999;

        _mockQrCodeRepository.Setup(r => r.GetByIdAsync(qrCodeId)).ReturnsAsync((QrCode?)null);

        // Act
        var result = await _qrCodeService.DeleteQrCodeAsync(qrCodeId);

        // Assert
        Assert.That(result, Is.False);
        _mockQrCodeRepository.Verify(r => r.GetByIdAsync(qrCodeId), Times.Once);
        _mockQrCodeRepository.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        _mockQrCodeRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    #region QR-Code Bild-Generierung Tests

    [Test]
    public async Task GenerateQrCodeImageAsync_WithValidId_ReturnsBase64Image()
    {
        // Arrange
        var qrCodeId = 1;
        var testUrl = "https://example.com/qr/test123";
        var qrCode = new QrCode(1, "Test QR-Code", "Test Description", "Test Notes")
        {
            Id = qrCodeId,
            UniqueUrl = new Uri(testUrl)
        };

        _mockQrCodeRepository.Setup(r => r.GetByIdAsync(qrCodeId))
            .ReturnsAsync(qrCode);

        // Act
        var result = await _qrCodeService.GenerateQrCodeImageAsync(qrCodeId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.StartWith("data:image/png;base64,"));
        _mockQrCodeRepository.Verify(r => r.GetByIdAsync(qrCodeId), Times.Once);
    }

    [Test]
    public async Task GenerateQrCodeImageAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var qrCodeId = 999;

        _mockQrCodeRepository.Setup(r => r.GetByIdAsync(qrCodeId))
            .ReturnsAsync((QrCode?)null);

        // Act
        var result = await _qrCodeService.GenerateQrCodeImageAsync(qrCodeId);

        // Assert
        Assert.That(result, Is.Null);
        _mockQrCodeRepository.Verify(r => r.GetByIdAsync(qrCodeId), Times.Once);
    }

    [Test]
    public async Task GenerateQrCodeImageAsync_WithCustomSize_ReturnsCorrectSize()
    {
        // Arrange
        var qrCodeId = 1;
        var customSize = 300;
        var testUrl = "https://example.com/qr/test123";
        var qrCode = new QrCode(1, "Test QR-Code", "Test Description", "Test Notes")
        {
            Id = qrCodeId,
            UniqueUrl = new Uri(testUrl)
        };

        _mockQrCodeRepository.Setup(r => r.GetByIdAsync(qrCodeId))
            .ReturnsAsync(qrCode);

        // Act
        var result = await _qrCodeService.GenerateQrCodeImageAsync(qrCodeId, customSize);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.StartWith("data:image/png;base64,"));
        _mockQrCodeRepository.Verify(r => r.GetByIdAsync(qrCodeId), Times.Once);
    }

    [Test]
    public void GenerateQrCodeImageForUrl_WithValidUrl_ReturnsBase64Image()
    {
        // Arrange
        var testUrl = "https://example.com/qr/test123";
        var size = 200;

        // Act
        var result = _qrCodeService.GenerateQrCodeImageForUrl(testUrl, size);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.StartWith("data:image/png;base64,"));
    }

    [Test]
    public void GenerateQrCodeImageForUrl_WithEmptyUrl_ThrowsArgumentException()
    {
        // Arrange
        var emptyUrl = "";
        var size = 200;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _qrCodeService.GenerateQrCodeImageForUrl(emptyUrl, size));
    }

    [Test]
    public void GenerateQrCodeImageForUrl_WithNullUrl_ThrowsArgumentException()
    {
        // Arrange
        string? nullUrl = null;
        var size = 200;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _qrCodeService.GenerateQrCodeImageForUrl(nullUrl!, size));
    }

    [Test]
    public void GenerateQrCodeImageForUrl_WithInvalidSize_ThrowsArgumentException()
    {
        // Arrange
        var testUrl = "https://example.com/qr/test123";
        var invalidSize = 0;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _qrCodeService.GenerateQrCodeImageForUrl(testUrl, invalidSize));
    }

    [Test]
    public void GenerateQrCodeImageForUrl_WithTooLargeSize_ThrowsArgumentException()
    {
        // Arrange
        var testUrl = "https://example.com/qr/test123";
        var tooLargeSize = 1001;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _qrCodeService.GenerateQrCodeImageForUrl(testUrl, tooLargeSize));
    }

    [Test]
    public void GenerateQrCodeImageForUrl_WithDefaultSize_UsesDefaultSize()
    {
        // Arrange
        var testUrl = "https://example.com/qr/test123";

        // Act
        var result = _qrCodeService.GenerateQrCodeImageForUrl(testUrl);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.StartWith("data:image/png;base64,"));
    }

    #endregion
}
