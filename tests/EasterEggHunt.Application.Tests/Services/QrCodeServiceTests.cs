using EasterEggHunt.Application.Requests;
using EasterEggHunt.Application.Services;
using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace EasterEggHunt.Application.Tests.Services;

/// <summary>
/// Unit Tests für QrCodeService
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

    #region Constructor Tests

    [Test]
    public void Constructor_WithNullQrCodeRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new QrCodeService(null!, _mockCampaignRepository.Object, _mockLogger.Object));
    }

    [Test]
    public void Constructor_WithNullCampaignRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new QrCodeService(_mockQrCodeRepository.Object, null!, _mockLogger.Object));
    }

    [Test]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new QrCodeService(_mockQrCodeRepository.Object, _mockCampaignRepository.Object, null!));
    }

    [Test]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Act
        var service = new QrCodeService(_mockQrCodeRepository.Object, _mockCampaignRepository.Object, _mockLogger.Object);

        // Assert
        Assert.That(service, Is.Not.Null);
    }

    #endregion

    #region GetQrCodesByCampaignIdAsync Tests

    [Test]
    public async Task GetQrCodesByCampaignIdAsync_WithValidCampaignId_ReturnsQrCodes()
    {
        // Arrange
        var campaignId = 1;
        var expectedQrCodes = new List<QrCode>
        {
            new QrCode(campaignId, "QR Code 1", "Description 1", "Note 1") { Id = 1 },
            new QrCode(campaignId, "QR Code 2", "Description 2", "Note 2") { Id = 2 }
        };

        _mockQrCodeRepository.Setup(r => r.GetByCampaignIdAsync(campaignId))
            .ReturnsAsync(expectedQrCodes);

        // Act
        var result = await _qrCodeService.GetQrCodesByCampaignIdAsync(campaignId);

        // Assert
        Assert.That(result, Is.EqualTo(expectedQrCodes));
        _mockQrCodeRepository.Verify(r => r.GetByCampaignIdAsync(campaignId), Times.Once);
    }

    #endregion

    #region GetQrCodeByIdAsync Tests

    [Test]
    public async Task GetQrCodeByIdAsync_WithValidId_ReturnsQrCode()
    {
        // Arrange
        var qrCodeId = 1;
        var expectedQrCode = new QrCode(1, "Test QR-Code", "Test Description", "Test Notes")
        {
            Id = qrCodeId
        };

        _mockQrCodeRepository.Setup(r => r.GetByIdAsync(qrCodeId))
            .ReturnsAsync(expectedQrCode);

        // Act
        var result = await _qrCodeService.GetQrCodeByIdAsync(qrCodeId);

        // Assert
        Assert.That(result, Is.EqualTo(expectedQrCode));
        _mockQrCodeRepository.Verify(r => r.GetByIdAsync(qrCodeId), Times.Once);
    }

    [Test]
    public async Task GetQrCodeByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var qrCodeId = 999;
        _mockQrCodeRepository.Setup(r => r.GetByIdAsync(qrCodeId))
            .ReturnsAsync((QrCode?)null);

        // Act
        var result = await _qrCodeService.GetQrCodeByIdAsync(qrCodeId);

        // Assert
        Assert.That(result, Is.Null);
        _mockQrCodeRepository.Verify(r => r.GetByIdAsync(qrCodeId), Times.Once);
    }

    #endregion

    #region CreateQrCodeAsync Tests

    [Test]
    public async Task CreateQrCodeAsync_WithValidRequest_CreatesQrCode()
    {
        // Arrange
        var campaignId = 1;
        var campaign = new Campaign("Test Campaign", "Test Description", "Admin")
        {
            Id = campaignId,
            IsActive = true
        };

        var request = new CreateQrCodeRequest
        {
            CampaignId = campaignId,
            Title = "Test QR-Code",
            Description = "Test Description",
            InternalNotes = "Test Notes"
        };

        _mockCampaignRepository.Setup(r => r.GetByIdAsync(campaignId))
            .ReturnsAsync(campaign);
        _mockQrCodeRepository.Setup(r => r.AddAsync(It.IsAny<QrCode>()))
            .ReturnsAsync(It.IsAny<QrCode>());
        _mockQrCodeRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _qrCodeService.CreateQrCodeAsync(request);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo(request.Title));
        Assert.That(result.Description, Is.EqualTo(request.Description));
        Assert.That(result.InternalNotes, Is.EqualTo(request.InternalNotes));
        Assert.That(result.CampaignId, Is.EqualTo(request.CampaignId));

        _mockCampaignRepository.Verify(r => r.GetByIdAsync(campaignId), Times.Once);
        _mockQrCodeRepository.Verify(r => r.AddAsync(It.IsAny<QrCode>()), Times.Once);
        _mockQrCodeRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void CreateQrCodeAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(() => _qrCodeService.CreateQrCodeAsync(null!));
    }

    [Test]
    public void CreateQrCodeAsync_WithEmptyTitle_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateQrCodeRequest
        {
            CampaignId = 1,
            Title = "",
            Description = "Test Description",
            InternalNotes = "Test Notes"
        };

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _qrCodeService.CreateQrCodeAsync(request));
        Assert.That(ex?.Message, Does.Contain("QR-Code Titel darf nicht leer sein"));
    }

    [Test]
    public void CreateQrCodeAsync_WithWhitespaceTitle_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateQrCodeRequest
        {
            CampaignId = 1,
            Title = "   ",
            Description = "Test Description",
            InternalNotes = "Test Notes"
        };

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _qrCodeService.CreateQrCodeAsync(request));
        Assert.That(ex?.Message, Does.Contain("QR-Code Titel darf nicht leer sein"));
    }

    [Test]
    public void CreateQrCodeAsync_WithNonExistentCampaign_ThrowsArgumentException()
    {
        // Arrange
        var campaignId = 999;
        var request = new CreateQrCodeRequest
        {
            CampaignId = campaignId,
            Title = "Test QR-Code",
            Description = "Test Description",
            InternalNotes = "Test Notes"
        };

        _mockCampaignRepository.Setup(r => r.GetByIdAsync(campaignId))
            .ReturnsAsync((Campaign?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _qrCodeService.CreateQrCodeAsync(request));
        Assert.That(ex?.Message, Does.Contain($"Kampagne mit ID {campaignId} nicht gefunden"));
    }

    #endregion

    #region UpdateQrCodeAsync Tests

    [Test]
    public async Task UpdateQrCodeAsync_WithValidRequest_UpdatesQrCode()
    {
        // Arrange
        var qrCodeId = 1;
        var existingQrCode = new QrCode(1, "Old Title", "Old Description", "Old Notes")
        {
            Id = qrCodeId
        };

        var request = new UpdateQrCodeRequest
        {
            Id = qrCodeId,
            Title = "New Title",
            Description = "New Description",
            InternalNotes = "New Notes"
        };

        _mockQrCodeRepository.Setup(r => r.GetByIdAsync(qrCodeId))
            .ReturnsAsync(existingQrCode);
        _mockQrCodeRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _qrCodeService.UpdateQrCodeAsync(request);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(existingQrCode.Title, Is.EqualTo(request.Title));
        Assert.That(existingQrCode.Description, Is.EqualTo(request.Description));
        Assert.That(existingQrCode.InternalNotes, Is.EqualTo(request.InternalNotes));

        _mockQrCodeRepository.Verify(r => r.GetByIdAsync(qrCodeId), Times.Once);
        _mockQrCodeRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void UpdateQrCodeAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(() => _qrCodeService.UpdateQrCodeAsync(null!));
    }

    [Test]
    public void UpdateQrCodeAsync_WithEmptyTitle_ThrowsArgumentException()
    {
        // Arrange
        var request = new UpdateQrCodeRequest
        {
            Id = 1,
            Title = "",
            Description = "Test Description",
            InternalNotes = "Test Notes"
        };

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _qrCodeService.UpdateQrCodeAsync(request));
        Assert.That(ex?.Message, Does.Contain("QR-Code Titel darf nicht leer sein"));
    }

    [Test]
    public async Task UpdateQrCodeAsync_WithNonExistentQrCode_ReturnsFalse()
    {
        // Arrange
        var qrCodeId = 999;
        var request = new UpdateQrCodeRequest
        {
            Id = qrCodeId,
            Title = "Test Title",
            Description = "Test Description",
            InternalNotes = "Test Notes"
        };

        _mockQrCodeRepository.Setup(r => r.GetByIdAsync(qrCodeId))
            .ReturnsAsync((QrCode?)null);

        // Act
        var result = await _qrCodeService.UpdateQrCodeAsync(request);

        // Assert
        Assert.That(result, Is.False);
        _mockQrCodeRepository.Verify(r => r.GetByIdAsync(qrCodeId), Times.Once);
        _mockQrCodeRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    #endregion

    #region SetQrCodeSortOrderAsync Tests

    [Test]
    public async Task SetQrCodeSortOrderAsync_WithValidId_SetsSortOrder()
    {
        // Arrange
        var qrCodeId = 1;
        var sortOrder = 5;
        var qrCode = new QrCode(1, "Test QR-Code", "Test Description", "Test Notes")
        {
            Id = qrCodeId,
            SortOrder = 1
        };

        _mockQrCodeRepository.Setup(r => r.GetByIdAsync(qrCodeId))
            .ReturnsAsync(qrCode);
        _mockQrCodeRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _qrCodeService.SetQrCodeSortOrderAsync(qrCodeId, sortOrder);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(qrCode.SortOrder, Is.EqualTo(sortOrder));
        _mockQrCodeRepository.Verify(r => r.GetByIdAsync(qrCodeId), Times.Once);
        _mockQrCodeRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task SetQrCodeSortOrderAsync_WithNonExistentQrCode_ReturnsFalse()
    {
        // Arrange
        var qrCodeId = 999;
        var sortOrder = 5;

        _mockQrCodeRepository.Setup(r => r.GetByIdAsync(qrCodeId))
            .ReturnsAsync((QrCode?)null);

        // Act
        var result = await _qrCodeService.SetQrCodeSortOrderAsync(qrCodeId, sortOrder);

        // Assert
        Assert.That(result, Is.False);
        _mockQrCodeRepository.Verify(r => r.GetByIdAsync(qrCodeId), Times.Once);
        _mockQrCodeRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    #endregion

    #region ActivateQrCodeAsync Tests

    [Test]
    public async Task ActivateQrCodeAsync_WithValidId_ActivatesQrCode()
    {
        // Arrange
        var qrCodeId = 1;
        var qrCode = new QrCode(1, "Test QR-Code", "Test Description", "Test Notes")
        {
            Id = qrCodeId,
            IsActive = false
        };

        _mockQrCodeRepository.Setup(r => r.GetByIdAsync(qrCodeId))
            .ReturnsAsync(qrCode);
        _mockQrCodeRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _qrCodeService.ActivateQrCodeAsync(qrCodeId);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(qrCode.IsActive, Is.True);
        _mockQrCodeRepository.Verify(r => r.GetByIdAsync(qrCodeId), Times.Once);
        _mockQrCodeRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task ActivateQrCodeAsync_WithNonExistentQrCode_ReturnsFalse()
    {
        // Arrange
        var qrCodeId = 999;

        _mockQrCodeRepository.Setup(r => r.GetByIdAsync(qrCodeId))
            .ReturnsAsync((QrCode?)null);

        // Act
        var result = await _qrCodeService.ActivateQrCodeAsync(qrCodeId);

        // Assert
        Assert.That(result, Is.False);
        _mockQrCodeRepository.Verify(r => r.GetByIdAsync(qrCodeId), Times.Once);
        _mockQrCodeRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    #endregion

    #region DeactivateQrCodeAsync Tests

    [Test]
    public async Task DeactivateQrCodeAsync_WithValidId_DeactivatesQrCode()
    {
        // Arrange
        var qrCodeId = 1;
        var qrCode = new QrCode(1, "Test QR-Code", "Test Description", "Test Notes")
        {
            Id = qrCodeId,
            IsActive = true
        };

        _mockQrCodeRepository.Setup(r => r.GetByIdAsync(qrCodeId))
            .ReturnsAsync(qrCode);
        _mockQrCodeRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _qrCodeService.DeactivateQrCodeAsync(qrCodeId);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(qrCode.IsActive, Is.False);
        _mockQrCodeRepository.Verify(r => r.GetByIdAsync(qrCodeId), Times.Once);
        _mockQrCodeRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task DeactivateQrCodeAsync_WithNonExistentQrCode_ReturnsFalse()
    {
        // Arrange
        var qrCodeId = 999;

        _mockQrCodeRepository.Setup(r => r.GetByIdAsync(qrCodeId))
            .ReturnsAsync((QrCode?)null);

        // Act
        var result = await _qrCodeService.DeactivateQrCodeAsync(qrCodeId);

        // Assert
        Assert.That(result, Is.False);
        _mockQrCodeRepository.Verify(r => r.GetByIdAsync(qrCodeId), Times.Once);
        _mockQrCodeRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    #endregion

    #region DeleteQrCodeAsync Tests

    [Test]
    public async Task DeleteQrCodeAsync_WithValidId_DeletesQrCode()
    {
        // Arrange
        var qrCodeId = 1;
        var qrCode = new QrCode(1, "Test QR-Code", "Test Description", "Test Notes")
        {
            Id = qrCodeId
        };

        _mockQrCodeRepository.Setup(r => r.GetByIdAsync(qrCodeId))
            .ReturnsAsync(qrCode);
        _mockQrCodeRepository.Setup(r => r.DeleteAsync(qrCodeId))
            .ReturnsAsync(true);
        _mockQrCodeRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _qrCodeService.DeleteQrCodeAsync(qrCodeId);

        // Assert
        Assert.That(result, Is.True);
        _mockQrCodeRepository.Verify(r => r.GetByIdAsync(qrCodeId), Times.Once);
        _mockQrCodeRepository.Verify(r => r.DeleteAsync(qrCodeId), Times.Once);
        _mockQrCodeRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task DeleteQrCodeAsync_WithNonExistentQrCode_ReturnsFalse()
    {
        // Arrange
        var qrCodeId = 999;

        _mockQrCodeRepository.Setup(r => r.GetByIdAsync(qrCodeId))
            .ReturnsAsync((QrCode?)null);

        // Act
        var result = await _qrCodeService.DeleteQrCodeAsync(qrCodeId);

        // Assert
        Assert.That(result, Is.False);
        _mockQrCodeRepository.Verify(r => r.GetByIdAsync(qrCodeId), Times.Once);
        _mockQrCodeRepository.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        _mockQrCodeRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    #endregion

    #region GetQrCodeByCodeAsync Tests

    [Test]
    public async Task GetQrCodeByCodeAsync_WithValidCode_ReturnsQrCode()
    {
        // Arrange
        var code = "test-code-123";
        var expectedQrCode = new QrCode(1, "Test QR-Code", "Test Description", "Test Notes")
        {
            Id = 1,
            Code = code
        };

        _mockQrCodeRepository.Setup(r => r.GetByCodeAsync(It.IsAny<string>()))
            .ReturnsAsync(expectedQrCode);

        // Act
        var result = await _qrCodeService.GetQrCodeByCodeAsync(code);

        // Assert
        Assert.That(result, Is.EqualTo(expectedQrCode));
        _mockQrCodeRepository.Verify(r => r.GetByCodeAsync(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task GetQrCodeByCodeAsync_WithInvalidCode_ReturnsNull()
    {
        // Arrange
        var code = "invalid-code";
        _mockQrCodeRepository.Setup(r => r.GetByCodeAsync(It.IsAny<string>()))
            .ReturnsAsync((QrCode?)null);

        // Act
        var result = await _qrCodeService.GetQrCodeByCodeAsync(code);

        // Assert
        Assert.That(result, Is.Null);
        _mockQrCodeRepository.Verify(r => r.GetByCodeAsync(code), Times.Once);
    }

    [Test]
    public async Task GetQrCodeByCodeAsync_WithEmptyCode_ReturnsNull()
    {
        // Arrange
        var code = "";

        // Act
        var result = await _qrCodeService.GetQrCodeByCodeAsync(code);

        // Assert
        Assert.That(result, Is.Null);
        _mockQrCodeRepository.Verify(r => r.GetByCodeAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task GetQrCodeByCodeAsync_WithWhitespaceCode_ReturnsNull()
    {
        // Arrange
        var code = "   ";

        // Act
        var result = await _qrCodeService.GetQrCodeByCodeAsync(code);

        // Assert
        Assert.That(result, Is.Null);
        _mockQrCodeRepository.Verify(r => r.GetByCodeAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task GetQrCodeByCodeAsync_WithNonExistentCode_ReturnsNull()
    {
        // Arrange
        var code = "non-existent-code";
        _mockQrCodeRepository.Setup(r => r.GetByCodeAsync(It.IsAny<string>()))
            .ReturnsAsync((QrCode?)null);

        // Act
        var result = await _qrCodeService.GetQrCodeByCodeAsync(code);

        // Assert
        Assert.That(result, Is.Null);
        _mockQrCodeRepository.Verify(r => r.GetByCodeAsync(It.IsAny<string>()), Times.Once);
    }

    #endregion
}
