using EasterEggHunt.Application.Services;
using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace EasterEggHunt.Application.Tests.Services;

[TestFixture]
public class FindServiceTests
{
    private Mock<IFindRepository> _mockFindRepository = null!;
    private Mock<IQrCodeRepository> _mockQrCodeRepository = null!;
    private Mock<IUserRepository> _mockUserRepository = null!;
    private Mock<ILogger<FindService>> _mockLogger = null!;
    private FindService _findService = null!;

    [SetUp]
    public void Setup()
    {
        _mockFindRepository = new Mock<IFindRepository>();
        _mockQrCodeRepository = new Mock<IQrCodeRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockLogger = new Mock<ILogger<FindService>>();
        _findService = new FindService(
            _mockFindRepository.Object,
            _mockQrCodeRepository.Object,
            _mockUserRepository.Object,
            _mockLogger.Object);
    }

    #region Constructor Tests

    [Test]
    public void FindService_Constructor_WithNullFindRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FindService(
            null!,
            _mockQrCodeRepository.Object,
            _mockUserRepository.Object,
            _mockLogger.Object));
    }

    [Test]
    public void FindService_Constructor_WithNullQrCodeRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FindService(
            _mockFindRepository.Object,
            null!,
            _mockUserRepository.Object,
            _mockLogger.Object));
    }

    [Test]
    public void FindService_Constructor_WithNullUserRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FindService(
            _mockFindRepository.Object,
            _mockQrCodeRepository.Object,
            null!,
            _mockLogger.Object));
    }

    [Test]
    public void FindService_Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FindService(
            _mockFindRepository.Object,
            _mockQrCodeRepository.Object,
            _mockUserRepository.Object,
            null!));
    }

    [Test]
    public void FindService_Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Act
        var service = new FindService(
            _mockFindRepository.Object,
            _mockQrCodeRepository.Object,
            _mockUserRepository.Object,
            _mockLogger.Object);

        // Assert
        Assert.That(service, Is.Not.Null);
    }

    #endregion

    #region RegisterFindAsync Tests

    [Test]
    public async Task RegisterFindAsync_WithValidData_ReturnsCreatedFind()
    {
        // Arrange
        var qrCodeId = 1;
        var userId = 2;
        var ipAddress = "192.168.1.1";
        var userAgent = "Mozilla/5.0";

        var qrCode = new QrCode(1, "Test QR", "Test Description", "Test Notes");
        var user = new User("testuser");

        _mockQrCodeRepository.Setup(r => r.GetByIdAsync(qrCodeId))
            .ReturnsAsync(qrCode);
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mockFindRepository.Setup(r => r.AddAsync(It.IsAny<Find>()))
            .ReturnsAsync((Find find) => find);
        _mockFindRepository.Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _findService.RegisterFindAsync(qrCodeId, userId, ipAddress, userAgent);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.QrCodeId, Is.EqualTo(qrCodeId));
        Assert.That(result.UserId, Is.EqualTo(userId));
        Assert.That(result.IpAddress, Is.EqualTo(ipAddress));
        Assert.That(result.UserAgent, Is.EqualTo(userAgent));

        _mockQrCodeRepository.Verify(r => r.GetByIdAsync(qrCodeId), Times.Once);
        _mockUserRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
        _mockFindRepository.Verify(r => r.AddAsync(It.IsAny<Find>()), Times.Once);
        _mockFindRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void RegisterFindAsync_WithEmptyIpAddress_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _findService.RegisterFindAsync(1, 1, "", "useragent"));
    }

    [Test]
    public void RegisterFindAsync_WithWhitespaceIpAddress_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _findService.RegisterFindAsync(1, 1, "   ", "useragent"));
    }

    [Test]
    public void RegisterFindAsync_WithNullIpAddress_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _findService.RegisterFindAsync(1, 1, null!, "useragent"));
    }

    [Test]
    public void RegisterFindAsync_WithEmptyUserAgent_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _findService.RegisterFindAsync(1, 1, "192.168.1.1", ""));
    }

    [Test]
    public void RegisterFindAsync_WithWhitespaceUserAgent_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _findService.RegisterFindAsync(1, 1, "192.168.1.1", "   "));
    }

    [Test]
    public void RegisterFindAsync_WithNullUserAgent_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _findService.RegisterFindAsync(1, 1, "192.168.1.1", null!));
    }

    [Test]
    public void RegisterFindAsync_WithNonExistentQrCode_ThrowsArgumentException()
    {
        // Arrange
        var qrCodeId = 999;
        _mockQrCodeRepository.Setup(r => r.GetByIdAsync(qrCodeId))
            .ReturnsAsync((QrCode?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(() =>
            _findService.RegisterFindAsync(qrCodeId, 1, "192.168.1.1", "useragent"));

        Assert.That(ex!.Message, Does.Contain($"QR-Code mit ID {qrCodeId} nicht gefunden"));
    }

    [Test]
    public void RegisterFindAsync_WithNonExistentUser_ThrowsArgumentException()
    {
        // Arrange
        var userId = 999;
        var qrCode = new QrCode(1, "Test QR", "Test Description", "Test Notes");

        _mockQrCodeRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(qrCode);
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(() =>
            _findService.RegisterFindAsync(1, userId, "192.168.1.1", "useragent"));

        Assert.That(ex!.Message, Does.Contain($"Benutzer mit ID {userId} nicht gefunden"));
    }

    #endregion

    #region GetFindsByQrCodeIdAsync Tests

    [Test]
    public async Task GetFindsByQrCodeIdAsync_ReturnsFinds()
    {
        // Arrange
        var qrCodeId = 1;
        var finds = new List<Find>
        {
            new Find(qrCodeId, 1, "192.168.1.1", "Mozilla/5.0"),
            new Find(qrCodeId, 2, "192.168.1.2", "Chrome/91.0")
        };

        _mockFindRepository.Setup(r => r.GetByQrCodeIdAsync(qrCodeId))
            .ReturnsAsync(finds);

        // Act
        var result = await _findService.GetFindsByQrCodeIdAsync(qrCodeId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(2));
        _mockFindRepository.Verify(r => r.GetByQrCodeIdAsync(qrCodeId), Times.Once);
    }

    [Test]
    public async Task GetFindsByQrCodeIdAsync_WithNoFinds_ReturnsEmptyCollection()
    {
        // Arrange
        var qrCodeId = 1;
        _mockFindRepository.Setup(r => r.GetByQrCodeIdAsync(qrCodeId))
            .ReturnsAsync(new List<Find>());

        // Act
        var result = await _findService.GetFindsByQrCodeIdAsync(qrCodeId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(0));
        _mockFindRepository.Verify(r => r.GetByQrCodeIdAsync(qrCodeId), Times.Once);
    }

    #endregion

    #region GetFindsByUserIdAsync Tests

    [Test]
    public async Task GetFindsByUserIdAsync_ReturnsFinds()
    {
        // Arrange
        var userId = 1;
        var finds = new List<Find>
        {
            new Find(1, userId, "192.168.1.1", "Mozilla/5.0"),
            new Find(2, userId, "192.168.1.2", "Chrome/91.0")
        };

        _mockFindRepository.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(finds);

        // Act
        var result = await _findService.GetFindsByUserIdAsync(userId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(2));
        _mockFindRepository.Verify(r => r.GetByUserIdAsync(userId), Times.Once);
    }

    [Test]
    public async Task GetFindsByUserIdAsync_WithNoFinds_ReturnsEmptyCollection()
    {
        // Arrange
        var userId = 1;
        _mockFindRepository.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(new List<Find>());

        // Act
        var result = await _findService.GetFindsByUserIdAsync(userId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(0));
        _mockFindRepository.Verify(r => r.GetByUserIdAsync(userId), Times.Once);
    }

    #endregion

    #region GetFindCountByQrCodeIdAsync Tests

    [Test]
    public async Task GetFindCountByQrCodeIdAsync_ReturnsCorrectCount()
    {
        // Arrange
        var qrCodeId = 1;
        var finds = new List<Find>
        {
            new Find(qrCodeId, 1, "192.168.1.1", "Mozilla/5.0"),
            new Find(qrCodeId, 2, "192.168.1.2", "Chrome/91.0"),
            new Find(qrCodeId, 3, "192.168.1.3", "Safari/14.0")
        };

        _mockFindRepository.Setup(r => r.GetByQrCodeIdAsync(qrCodeId))
            .ReturnsAsync(finds);

        // Act
        var result = await _findService.GetFindCountByQrCodeIdAsync(qrCodeId);

        // Assert
        Assert.That(result, Is.EqualTo(3));
        _mockFindRepository.Verify(r => r.GetByQrCodeIdAsync(qrCodeId), Times.Once);
    }

    [Test]
    public async Task GetFindCountByQrCodeIdAsync_WithNoFinds_ReturnsZero()
    {
        // Arrange
        var qrCodeId = 1;
        _mockFindRepository.Setup(r => r.GetByQrCodeIdAsync(qrCodeId))
            .ReturnsAsync(new List<Find>());

        // Act
        var result = await _findService.GetFindCountByQrCodeIdAsync(qrCodeId);

        // Assert
        Assert.That(result, Is.EqualTo(0));
        _mockFindRepository.Verify(r => r.GetByQrCodeIdAsync(qrCodeId), Times.Once);
    }

    #endregion

    #region GetFindCountByUserIdAsync Tests

    [Test]
    public async Task GetFindCountByUserIdAsync_ReturnsCorrectCount()
    {
        // Arrange
        var userId = 1;
        var finds = new List<Find>
        {
            new Find(1, userId, "192.168.1.1", "Mozilla/5.0"),
            new Find(2, userId, "192.168.1.2", "Chrome/91.0")
        };

        _mockFindRepository.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(finds);

        // Act
        var result = await _findService.GetFindCountByUserIdAsync(userId);

        // Assert
        Assert.That(result, Is.EqualTo(2));
        _mockFindRepository.Verify(r => r.GetByUserIdAsync(userId), Times.Once);
    }

    [Test]
    public async Task GetFindCountByUserIdAsync_WithNoFinds_ReturnsZero()
    {
        // Arrange
        var userId = 1;
        _mockFindRepository.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(new List<Find>());

        // Act
        var result = await _findService.GetFindCountByUserIdAsync(userId);

        // Assert
        Assert.That(result, Is.EqualTo(0));
        _mockFindRepository.Verify(r => r.GetByUserIdAsync(userId), Times.Once);
    }

    #endregion
}
