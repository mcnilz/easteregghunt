using EasterEggHunt.Application.Services;
using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace EasterEggHunt.Application.Tests.Services;

/// <summary>
/// Tests für StatisticsService
/// </summary>
[TestFixture]
public class StatisticsServiceTests
{
    private Mock<ICampaignService> _mockCampaignService = null!;
    private Mock<IQrCodeService> _mockQrCodeService = null!;
    private Mock<IUserService> _mockUserService = null!;
    private Mock<IFindService> _mockFindService = null!;
    private Mock<ILogger<StatisticsService>> _mockLogger = null!;
    private StatisticsService _statisticsService = null!;

    [SetUp]
    public void Setup()
    {
        _mockCampaignService = new Mock<ICampaignService>();
        _mockQrCodeService = new Mock<IQrCodeService>();
        _mockUserService = new Mock<IUserService>();
        _mockFindService = new Mock<IFindService>();
        _mockLogger = new Mock<ILogger<StatisticsService>>();

        _statisticsService = new StatisticsService(
            _mockCampaignService.Object,
            _mockQrCodeService.Object,
            _mockUserService.Object,
            _mockFindService.Object,
            _mockLogger.Object);
    }

    #region Constructor Tests

    [Test]
    public void StatisticsService_Constructor_WithNullCampaignService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new StatisticsService(
            null!,
            _mockQrCodeService.Object,
            _mockUserService.Object,
            _mockFindService.Object,
            _mockLogger.Object));
    }

    [Test]
    public void StatisticsService_Constructor_WithNullQrCodeService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new StatisticsService(
            _mockCampaignService.Object,
            null!,
            _mockUserService.Object,
            _mockFindService.Object,
            _mockLogger.Object));
    }

    [Test]
    public void StatisticsService_Constructor_WithNullUserService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new StatisticsService(
            _mockCampaignService.Object,
            _mockQrCodeService.Object,
            null!,
            _mockFindService.Object,
            _mockLogger.Object));
    }

    [Test]
    public void StatisticsService_Constructor_WithNullFindService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new StatisticsService(
            _mockCampaignService.Object,
            _mockQrCodeService.Object,
            _mockUserService.Object,
            null!,
            _mockLogger.Object));
    }

    [Test]
    public void StatisticsService_Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new StatisticsService(
            _mockCampaignService.Object,
            _mockQrCodeService.Object,
            _mockUserService.Object,
            _mockFindService.Object,
            null!));
    }

    #endregion

    #region GetSystemOverviewAsync Tests

    [Test]
    public async Task GetSystemOverviewAsync_WithEmptyData_ShouldReturnZeroStatistics()
    {
        // Arrange
        _mockCampaignService.Setup(s => s.GetActiveCampaignsAsync())
            .ReturnsAsync(Enumerable.Empty<Campaign>());
        _mockUserService.Setup(s => s.GetActiveUsersAsync())
            .ReturnsAsync(Enumerable.Empty<User>());

        // Act
        var result = await _statisticsService.GetSystemOverviewAsync();

        // Assert
        Assert.That(result.TotalCampaigns, Is.EqualTo(0));
        Assert.That(result.ActiveCampaigns, Is.EqualTo(0));
        Assert.That(result.TotalQrCodes, Is.EqualTo(0));
        Assert.That(result.TotalUsers, Is.EqualTo(0));
        Assert.That(result.TotalFinds, Is.EqualTo(0));
        Assert.That(result.CompletedFinds, Is.EqualTo(0));
    }

    [Test]
    public async Task GetSystemOverviewAsync_WithSingleCampaign_ShouldCalculateCorrectly()
    {
        // Arrange
        var campaign = new Campaign("Test Campaign", "Description", "Admin");
        var qrCode1 = new QrCode(campaign.Id, "QR1", "Desc1", "Notes1");
        var qrCode2 = new QrCode(campaign.Id, "QR2", "Desc2", "Notes2");
        var user = new User("Test User");
        var find1 = new Find(qrCode1.Id, user.Id, "192.168.1.1", "UserAgent1");
        var find2 = new Find(qrCode2.Id, user.Id, "192.168.1.2", "UserAgent2");

        _mockCampaignService.Setup(s => s.GetActiveCampaignsAsync())
            .ReturnsAsync(new[] { campaign });
        _mockUserService.Setup(s => s.GetActiveUsersAsync())
            .ReturnsAsync(new[] { user });
        _mockQrCodeService.Setup(s => s.GetQrCodesByCampaignIdAsync(campaign.Id))
            .ReturnsAsync(new[] { qrCode1, qrCode2 });
        _mockFindService.Setup(s => s.GetFindsByQrCodeIdAsync(qrCode1.Id))
            .ReturnsAsync(new[] { find1 });
        _mockFindService.Setup(s => s.GetFindsByQrCodeIdAsync(qrCode2.Id))
            .ReturnsAsync(new[] { find2 });

        // Act
        var result = await _statisticsService.GetSystemOverviewAsync();

        // Assert
        Assert.That(result.TotalCampaigns, Is.EqualTo(1));
        Assert.That(result.ActiveCampaigns, Is.EqualTo(1));
        Assert.That(result.TotalQrCodes, Is.EqualTo(2));
        Assert.That(result.TotalUsers, Is.EqualTo(1));
        Assert.That(result.TotalFinds, Is.EqualTo(2));
        Assert.That(result.CompletedFinds, Is.EqualTo(2));
    }

    [Test]
    public async Task GetSystemOverviewAsync_WithMultipleCampaigns_ShouldAggregateCorrectly()
    {
        // Arrange
        var campaign1 = new Campaign("Campaign 1", "Desc1", "Admin");
        var campaign2 = new Campaign("Campaign 2", "Desc2", "Admin");
        var qrCode1 = new QrCode(campaign1.Id, "QR1", "Desc1", "Notes1");
        var qrCode2 = new QrCode(campaign2.Id, "QR2", "Desc2", "Notes2");
        var user = new User("Test User");
        var find1 = new Find(qrCode1.Id, user.Id, "192.168.1.1", "UserAgent1");
        var find2 = new Find(qrCode2.Id, user.Id, "192.168.1.2", "UserAgent2");

        _mockCampaignService.Setup(s => s.GetActiveCampaignsAsync())
            .ReturnsAsync(new[] { campaign1, campaign2 });
        _mockUserService.Setup(s => s.GetActiveUsersAsync())
            .ReturnsAsync(new[] { user });
        _mockQrCodeService.Setup(s => s.GetQrCodesByCampaignIdAsync(campaign1.Id))
            .ReturnsAsync(new[] { qrCode1 });
        _mockQrCodeService.Setup(s => s.GetQrCodesByCampaignIdAsync(campaign2.Id))
            .ReturnsAsync(new[] { qrCode2 });
        _mockFindService.Setup(s => s.GetFindsByQrCodeIdAsync(qrCode1.Id))
            .ReturnsAsync(new[] { find1 });
        _mockFindService.Setup(s => s.GetFindsByQrCodeIdAsync(qrCode2.Id))
            .ReturnsAsync(new[] { find2 });

        // Act
        var result = await _statisticsService.GetSystemOverviewAsync();

        // Assert
        Assert.That(result.TotalCampaigns, Is.EqualTo(2));
        Assert.That(result.ActiveCampaigns, Is.EqualTo(2));
        Assert.That(result.TotalQrCodes, Is.EqualTo(2));
        Assert.That(result.TotalFinds, Is.EqualTo(2));
    }

    [Test]
    public async Task GetSystemOverviewAsync_WithInactiveCampaign_ShouldCountOnlyActive()
    {
        // Arrange
        var activeCampaign = new Campaign("Active", "Desc", "Admin");
        var inactiveCampaign = new Campaign("Inactive", "Desc", "Admin");
        inactiveCampaign.Deactivate();

        _mockCampaignService.Setup(s => s.GetActiveCampaignsAsync())
            .ReturnsAsync(new[] { activeCampaign, inactiveCampaign });
        _mockUserService.Setup(s => s.GetActiveUsersAsync())
            .ReturnsAsync(Enumerable.Empty<User>());
        _mockQrCodeService.Setup(s => s.GetQrCodesByCampaignIdAsync(It.IsAny<int>()))
            .ReturnsAsync(Enumerable.Empty<QrCode>());

        // Act
        var result = await _statisticsService.GetSystemOverviewAsync();

        // Assert
        Assert.That(result.TotalCampaigns, Is.EqualTo(2));
        Assert.That(result.ActiveCampaigns, Is.EqualTo(1)); // Only active campaign
    }

    #endregion

    #region GetCampaignStatisticsAsync Tests

    [Test]
    public async Task GetCampaignStatisticsAsync_WithExistingCampaign_ShouldReturnStatistics()
    {
        // Arrange
        var campaign = new Campaign("Test Campaign", "Description", "Admin") { Id = 1 };
        var qrCode = new QrCode(campaign.Id, "QR1", "Desc1", "Notes1") { Id = 1 };
        var user1 = new User("User1") { Id = 1 };
        var user2 = new User("User2") { Id = 2 };
        var find1 = new Find(qrCode.Id, user1.Id, "192.168.1.1", "UserAgent1");
        var find2 = new Find(qrCode.Id, user2.Id, "192.168.1.2", "UserAgent2");

        _mockCampaignService.Setup(s => s.GetCampaignByIdAsync(campaign.Id))
            .ReturnsAsync(campaign);
        _mockQrCodeService.Setup(s => s.GetQrCodesByCampaignIdAsync(campaign.Id))
            .ReturnsAsync(new[] { qrCode });
        _mockFindService.Setup(s => s.GetFindsByQrCodeIdAsync(qrCode.Id))
            .ReturnsAsync(new[] { find1, find2 });

        // Act
        var result = await _statisticsService.GetCampaignStatisticsAsync(campaign.Id);

        // Assert
        Assert.That(result.CampaignId, Is.EqualTo(campaign.Id));
        Assert.That(result.CampaignName, Is.EqualTo("Test Campaign"));
        Assert.That(result.TotalQrCodes, Is.EqualTo(1));
        Assert.That(result.TotalFinds, Is.EqualTo(2));
        Assert.That(result.UniqueFinders, Is.EqualTo(2));
        Assert.That(result.CompletionRate, Is.EqualTo(2.0));
    }

    [Test]
    public void GetCampaignStatisticsAsync_WithNonExistentCampaign_ShouldThrowArgumentException()
    {
        // Arrange
        var campaignId = 999;
        _mockCampaignService.Setup(s => s.GetCampaignByIdAsync(campaignId))
            .ReturnsAsync((Campaign?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _statisticsService.GetCampaignStatisticsAsync(campaignId));
        Assert.That(ex!.Message, Does.Contain("nicht gefunden"));
    }

    [Test]
    public async Task GetCampaignStatisticsAsync_WithNoQrCodes_ShouldReturnZeroCompletionRate()
    {
        // Arrange
        var campaign = new Campaign("Test Campaign", "Description", "Admin");
        _mockCampaignService.Setup(s => s.GetCampaignByIdAsync(campaign.Id))
            .ReturnsAsync(campaign);
        _mockQrCodeService.Setup(s => s.GetQrCodesByCampaignIdAsync(campaign.Id))
            .ReturnsAsync(Enumerable.Empty<QrCode>());

        // Act
        var result = await _statisticsService.GetCampaignStatisticsAsync(campaign.Id);

        // Assert
        Assert.That(result.TotalQrCodes, Is.EqualTo(0));
        Assert.That(result.CompletionRate, Is.EqualTo(0));
    }

    [Test]
    public async Task GetCampaignStatisticsAsync_WithDuplicateFinders_ShouldCountUniqueFinders()
    {
        // Arrange
        var campaign = new Campaign("Test Campaign", "Description", "Admin");
        var qrCode = new QrCode(campaign.Id, "QR1", "Desc1", "Notes1");
        var user = new User("User1");
        var find1 = new Find(qrCode.Id, user.Id, "192.168.1.1", "UserAgent1");
        var find2 = new Find(qrCode.Id, user.Id, "192.168.1.2", "UserAgent2"); // Same user

        _mockCampaignService.Setup(s => s.GetCampaignByIdAsync(campaign.Id))
            .ReturnsAsync(campaign);
        _mockQrCodeService.Setup(s => s.GetQrCodesByCampaignIdAsync(campaign.Id))
            .ReturnsAsync(new[] { qrCode });
        _mockFindService.Setup(s => s.GetFindsByQrCodeIdAsync(qrCode.Id))
            .ReturnsAsync(new[] { find1, find2 });

        // Act
        var result = await _statisticsService.GetCampaignStatisticsAsync(campaign.Id);

        // Assert
        Assert.That(result.TotalFinds, Is.EqualTo(2));
        Assert.That(result.UniqueFinders, Is.EqualTo(1)); // Same user counted once
    }

    #endregion

    #region GetUserStatisticsAsync Tests

    [Test]
    public async Task GetUserStatisticsAsync_WithExistingUser_ShouldReturnStatistics()
    {
        // Arrange
        var user = new User("Test User") { Id = 1 };
        var qrCode1 = new QrCode(1, "QR1", "Desc1", "Notes1") { Id = 1 };
        var qrCode2 = new QrCode(1, "QR2", "Desc2", "Notes2") { Id = 2 };
        var find1 = new Find(qrCode1.Id, user.Id, "192.168.1.1", "UserAgent1");
        var find2 = new Find(qrCode2.Id, user.Id, "192.168.1.2", "UserAgent2");

        _mockUserService.Setup(s => s.GetUserByIdAsync(user.Id))
            .ReturnsAsync(user);
        _mockFindService.Setup(s => s.GetFindsByUserIdAsync(user.Id))
            .ReturnsAsync(new[] { find1, find2 });

        // Act
        var result = await _statisticsService.GetUserStatisticsAsync(user.Id);

        // Assert
        Assert.That(result.UserId, Is.EqualTo(user.Id));
        Assert.That(result.UserName, Is.EqualTo("Test User"));
        Assert.That(result.TotalFinds, Is.EqualTo(2));
        Assert.That(result.UniqueQrCodesFound, Is.EqualTo(2));
        Assert.That(result.FirstFindDate, Is.Not.Null);
        Assert.That(result.LastFindDate, Is.Not.Null);
    }

    [Test]
    public void GetUserStatisticsAsync_WithNonExistentUser_ShouldThrowArgumentException()
    {
        // Arrange
        var userId = 999;
        _mockUserService.Setup(s => s.GetUserByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _statisticsService.GetUserStatisticsAsync(userId));
        Assert.That(ex!.Message, Does.Contain("nicht gefunden"));
    }

    [Test]
    public async Task GetUserStatisticsAsync_WithNoFinds_ShouldReturnZeroStatistics()
    {
        // Arrange
        var user = new User("Test User");
        _mockUserService.Setup(s => s.GetUserByIdAsync(user.Id))
            .ReturnsAsync(user);
        _mockFindService.Setup(s => s.GetFindsByUserIdAsync(user.Id))
            .ReturnsAsync(Enumerable.Empty<Find>());

        // Act
        var result = await _statisticsService.GetUserStatisticsAsync(user.Id);

        // Assert
        Assert.That(result.TotalFinds, Is.EqualTo(0));
        Assert.That(result.UniqueQrCodesFound, Is.EqualTo(0));
        Assert.That(result.FirstFindDate, Is.Null);
        Assert.That(result.LastFindDate, Is.Null);
    }

    [Test]
    public async Task GetUserStatisticsAsync_WithDuplicateQrCodes_ShouldCountUniqueQrCodes()
    {
        // Arrange
        var user = new User("Test User");
        var qrCode = new QrCode(1, "QR1", "Desc1", "Notes1");
        var find1 = new Find(qrCode.Id, user.Id, "192.168.1.1", "UserAgent1");
        var find2 = new Find(qrCode.Id, user.Id, "192.168.1.2", "UserAgent2"); // Same QR code

        _mockUserService.Setup(s => s.GetUserByIdAsync(user.Id))
            .ReturnsAsync(user);
        _mockFindService.Setup(s => s.GetFindsByUserIdAsync(user.Id))
            .ReturnsAsync(new[] { find1, find2 });

        // Act
        var result = await _statisticsService.GetUserStatisticsAsync(user.Id);

        // Assert
        Assert.That(result.TotalFinds, Is.EqualTo(2));
        Assert.That(result.UniqueQrCodesFound, Is.EqualTo(1)); // Same QR code counted once
    }

    #endregion

    #region GetQrCodeStatisticsAsync Tests

    [Test]
    public async Task GetQrCodeStatisticsAsync_WithExistingQrCode_ShouldReturnStatistics()
    {
        // Arrange
        var qrCode = new QrCode(1, "Test QR", "Description", "Notes") { Id = 1 };
        var user1 = new User("User1") { Id = 1 };
        var user2 = new User("User2") { Id = 2 };
        var find1 = new Find(qrCode.Id, user1.Id, "192.168.1.1", "UserAgent1");
        var find2 = new Find(qrCode.Id, user2.Id, "192.168.1.2", "UserAgent2");

        _mockQrCodeService.Setup(s => s.GetQrCodeByIdAsync(qrCode.Id))
            .ReturnsAsync(qrCode);
        _mockFindService.Setup(s => s.GetFindsByQrCodeIdAsync(qrCode.Id))
            .ReturnsAsync(new[] { find1, find2 });

        // Act
        var result = await _statisticsService.GetQrCodeStatisticsAsync(qrCode.Id);

        // Assert
        Assert.That(result.QrCodeId, Is.EqualTo(qrCode.Id));
        Assert.That(result.QrCodeTitle, Is.EqualTo("Test QR"));
        Assert.That(result.TotalFinds, Is.EqualTo(2));
        Assert.That(result.UniqueFinders, Is.EqualTo(2));
        Assert.That(result.FirstFindDate, Is.Not.Null);
        Assert.That(result.LastFindDate, Is.Not.Null);
    }

    [Test]
    public void GetQrCodeStatisticsAsync_WithNonExistentQrCode_ShouldThrowArgumentException()
    {
        // Arrange
        var qrCodeId = 999;
        _mockQrCodeService.Setup(s => s.GetQrCodeByIdAsync(qrCodeId))
            .ReturnsAsync((QrCode?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _statisticsService.GetQrCodeStatisticsAsync(qrCodeId));
        Assert.That(ex!.Message, Does.Contain("nicht gefunden"));
    }

    [Test]
    public async Task GetQrCodeStatisticsAsync_WithNoFinds_ShouldReturnZeroStatistics()
    {
        // Arrange
        var qrCode = new QrCode(1, "Test QR", "Description", "Notes");
        _mockQrCodeService.Setup(s => s.GetQrCodeByIdAsync(qrCode.Id))
            .ReturnsAsync(qrCode);
        _mockFindService.Setup(s => s.GetFindsByQrCodeIdAsync(qrCode.Id))
            .ReturnsAsync(Enumerable.Empty<Find>());

        // Act
        var result = await _statisticsService.GetQrCodeStatisticsAsync(qrCode.Id);

        // Assert
        Assert.That(result.TotalFinds, Is.EqualTo(0));
        Assert.That(result.UniqueFinders, Is.EqualTo(0));
        Assert.That(result.FirstFindDate, Is.Null);
        Assert.That(result.LastFindDate, Is.Null);
        Assert.That(result.RecentFinds, Is.Empty);
    }

    [Test]
    public async Task GetQrCodeStatisticsAsync_WithMoreThan10Finds_ShouldLimitRecentFinds()
    {
        // Arrange
        var qrCode = new QrCode(1, "Test QR", "Description", "Notes");
        var user = new User("Test User");
        var finds = Enumerable.Range(1, 15)
            .Select(i => new Find(qrCode.Id, user.Id, $"192.168.1.{i}", "UserAgent"))
            .ToList();

        _mockQrCodeService.Setup(s => s.GetQrCodeByIdAsync(qrCode.Id))
            .ReturnsAsync(qrCode);
        _mockFindService.Setup(s => s.GetFindsByQrCodeIdAsync(qrCode.Id))
            .ReturnsAsync(finds);

        // Act
        var result = await _statisticsService.GetQrCodeStatisticsAsync(qrCode.Id);

        // Assert
        Assert.That(result.TotalFinds, Is.EqualTo(15));
        Assert.That(result.RecentFinds.Count, Is.EqualTo(10)); // Limited to 10
    }

    #endregion

    #region GetCampaignQrCodeStatisticsAsync Tests

    [Test]
    public async Task GetCampaignQrCodeStatisticsAsync_WithExistingCampaign_ShouldReturnStatistics()
    {
        // Arrange
        var campaign = new Campaign("Test Campaign", "Description", "Admin");
        var qrCode1 = new QrCode(campaign.Id, "QR1", "Desc1", "Notes1");
        var qrCode2 = new QrCode(campaign.Id, "QR2", "Desc2", "Notes2");
        var user = new User("Test User");
        var find1 = new Find(qrCode1.Id, user.Id, "192.168.1.1", "UserAgent1");
        var find2 = new Find(qrCode2.Id, user.Id, "192.168.1.2", "UserAgent2");

        _mockCampaignService.Setup(s => s.GetCampaignByIdAsync(campaign.Id))
            .ReturnsAsync(campaign);
        _mockQrCodeService.Setup(s => s.GetQrCodesByCampaignIdAsync(campaign.Id))
            .ReturnsAsync(new[] { qrCode1, qrCode2 });
        _mockFindService.Setup(s => s.GetFindsByQrCodeIdAsync(qrCode1.Id))
            .ReturnsAsync(new[] { find1 });
        _mockFindService.Setup(s => s.GetFindsByQrCodeIdAsync(qrCode2.Id))
            .ReturnsAsync(new[] { find2 });

        // Act
        var result = await _statisticsService.GetCampaignQrCodeStatisticsAsync(campaign.Id);

        // Assert
        Assert.That(result.CampaignId, Is.EqualTo(campaign.Id));
        Assert.That(result.CampaignName, Is.EqualTo("Test Campaign"));
        Assert.That(result.QrCodeStatistics.Count, Is.EqualTo(2));
    }

    [Test]
    public void GetCampaignQrCodeStatisticsAsync_WithNonExistentCampaign_ShouldThrowArgumentException()
    {
        // Arrange
        var campaignId = 999;
        _mockCampaignService.Setup(s => s.GetCampaignByIdAsync(campaignId))
            .ReturnsAsync((Campaign?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _statisticsService.GetCampaignQrCodeStatisticsAsync(campaignId));
        Assert.That(ex!.Message, Does.Contain("nicht gefunden"));
    }

    [Test]
    public async Task GetCampaignQrCodeStatisticsAsync_WithNoQrCodes_ShouldReturnEmptyStatistics()
    {
        // Arrange
        var campaign = new Campaign("Test Campaign", "Description", "Admin");
        _mockCampaignService.Setup(s => s.GetCampaignByIdAsync(campaign.Id))
            .ReturnsAsync(campaign);
        _mockQrCodeService.Setup(s => s.GetQrCodesByCampaignIdAsync(campaign.Id))
            .ReturnsAsync(Enumerable.Empty<QrCode>());

        // Act
        var result = await _statisticsService.GetCampaignQrCodeStatisticsAsync(campaign.Id);

        // Assert
        Assert.That(result.QrCodeStatistics, Is.Empty);
    }

    [Test]
    public async Task GetCampaignQrCodeStatisticsAsync_ShouldOrderByTotalFinds()
    {
        // Arrange
        var campaign = new Campaign("Test Campaign", "Description", "Admin") { Id = 1 };
        var qrCode1 = new QrCode(campaign.Id, "QR1", "Desc1", "Notes1") { Id = 1 };
        var qrCode2 = new QrCode(campaign.Id, "QR2", "Desc2", "Notes2") { Id = 2 };
        var user = new User("Test User") { Id = 1 };
        var find1 = new Find(qrCode1.Id, user.Id, "192.168.1.1", "UserAgent1");
        var find2a = new Find(qrCode2.Id, user.Id, "192.168.1.2", "UserAgent2");
        var find2b = new Find(qrCode2.Id, user.Id, "192.168.1.3", "UserAgent3");

        _mockCampaignService.Setup(s => s.GetCampaignByIdAsync(campaign.Id))
            .ReturnsAsync(campaign);
        _mockQrCodeService.Setup(s => s.GetQrCodesByCampaignIdAsync(campaign.Id))
            .ReturnsAsync(new[] { qrCode1, qrCode2 });
        _mockFindService.Setup(s => s.GetFindsByQrCodeIdAsync(qrCode1.Id))
            .ReturnsAsync(new[] { find1 });
        _mockFindService.Setup(s => s.GetFindsByQrCodeIdAsync(qrCode2.Id))
            .ReturnsAsync(new[] { find2a, find2b });

        // Act
        var result = await _statisticsService.GetCampaignQrCodeStatisticsAsync(campaign.Id);

        // Assert
        Assert.That(result.QrCodeStatistics.First().TotalFinds, Is.EqualTo(2)); // Most finds first
        Assert.That(result.QrCodeStatistics.Last().TotalFinds, Is.EqualTo(1));
    }

    #endregion

    #region GetTopPerformersAsync Tests

    [Test]
    public async Task GetTopPerformersAsync_WithUsers_ShouldReturnTopPerformers()
    {
        // Arrange
        var user1 = new User("User1") { Id = 1 };
        var user2 = new User("User2") { Id = 2 };
        var qrCode = new QrCode(1, "QR1", "Desc1", "Notes1") { Id = 1 };
        var find1 = new Find(qrCode.Id, user1.Id, "192.168.1.1", "UserAgent1");
        var find2 = new Find(qrCode.Id, user2.Id, "192.168.1.2", "UserAgent2");
        var find3 = new Find(qrCode.Id, user1.Id, "192.168.1.3", "UserAgent3");

        _mockUserService.Setup(s => s.GetActiveUsersAsync())
            .ReturnsAsync(new[] { user1, user2 });
        _mockFindService.Setup(s => s.GetFindsByUserIdAsync(user1.Id))
            .ReturnsAsync(new[] { find1, find3 });
        _mockFindService.Setup(s => s.GetFindsByUserIdAsync(user2.Id))
            .ReturnsAsync(new[] { find2 });

        // Act
        var result = await _statisticsService.GetTopPerformersAsync();

        // Assert
        Assert.That(result.TopByTotalFinds, Is.Not.Null);
        Assert.That(result.TopByTotalFinds.Count, Is.EqualTo(2)); // Both users, limited by total
        Assert.That(result.TopByTotalFinds.First().TotalFinds, Is.EqualTo(2)); // User1 has more finds
        Assert.That(result.TopByUniqueQrCodes, Is.Not.Null);
        Assert.That(result.MostRecentActivity, Is.Not.Null);
    }

    [Test]
    public async Task GetTopPerformersAsync_WithEmptyUsers_ShouldReturnEmptyStatistics()
    {
        // Arrange
        _mockUserService.Setup(s => s.GetActiveUsersAsync())
            .ReturnsAsync(Enumerable.Empty<User>());

        // Act
        var result = await _statisticsService.GetTopPerformersAsync();

        // Assert
        Assert.That(result.TopByTotalFinds, Is.Empty);
        Assert.That(result.TopByUniqueQrCodes, Is.Empty);
        Assert.That(result.MostRecentActivity, Is.Empty);
    }

    [Test]
    public async Task GetTopPerformersAsync_ShouldLimitTopPerformersTo10()
    {
        // Arrange
        var users = Enumerable.Range(1, 15)
            .Select(i => new User($"User{i}") { Id = i })
            .ToList();
        var qrCode = new QrCode(1, "QR1", "Desc1", "Notes1") { Id = 1 };

        _mockUserService.Setup(s => s.GetActiveUsersAsync())
            .ReturnsAsync(users);
        foreach (var user in users)
        {
            var find = new Find(qrCode.Id, user.Id, "192.168.1.1", "UserAgent");
            _mockFindService.Setup(s => s.GetFindsByUserIdAsync(user.Id))
                .ReturnsAsync(new[] { find });
        }

        // Act
        var result = await _statisticsService.GetTopPerformersAsync();

        // Assert
        Assert.That(result.TopByTotalFinds.Count, Is.EqualTo(10)); // Limited to 10
        Assert.That(result.TopByUniqueQrCodes.Count, Is.EqualTo(10)); // Limited to 10
    }

    [Test]
    public async Task GetTopPerformersAsync_WithUsersWithoutFinds_ShouldFilterMostRecentActivity()
    {
        // Arrange
        var user1 = new User("User1") { Id = 1 };
        var user2 = new User("User2") { Id = 2 };
        var find = new Find(1, user1.Id, "192.168.1.1", "UserAgent");

        _mockUserService.Setup(s => s.GetActiveUsersAsync())
            .ReturnsAsync(new[] { user1, user2 });
        _mockFindService.Setup(s => s.GetFindsByUserIdAsync(user1.Id))
            .ReturnsAsync(new[] { find });
        _mockFindService.Setup(s => s.GetFindsByUserIdAsync(user2.Id))
            .ReturnsAsync(Enumerable.Empty<Find>());

        // Act
        var result = await _statisticsService.GetTopPerformersAsync();

        // Assert
        Assert.That(result.MostRecentActivity.Count, Is.EqualTo(1)); // Only user1 has finds
        Assert.That(result.MostRecentActivity.First().UserId, Is.EqualTo(user1.Id));
    }

    #endregion
}

