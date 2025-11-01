using EasterEggHunt.Domain.Models;

namespace EasterEggHunt.Domain.Tests.Models;

/// <summary>
/// Tests für die Statistics Models (DTOs)
/// </summary>
[TestFixture]
public class StatisticsModelsTests
{
    #region SystemOverviewStatistics Tests

    [Test]
    public void SystemOverviewStatistics_Properties_CanBeSet()
    {
        // Arrange
        var statistics = new SystemOverviewStatistics();

        // Act
        statistics.TotalCampaigns = 10;
        statistics.ActiveCampaigns = 5;
        statistics.TotalQrCodes = 50;
        statistics.TotalUsers = 100;
        statistics.TotalFinds = 200;
        statistics.CompletedFinds = 150;
        statistics.GeneratedAt = DateTime.UtcNow;

        // Assert
        Assert.That(statistics.TotalCampaigns, Is.EqualTo(10));
        Assert.That(statistics.ActiveCampaigns, Is.EqualTo(5));
        Assert.That(statistics.TotalQrCodes, Is.EqualTo(50));
        Assert.That(statistics.TotalUsers, Is.EqualTo(100));
        Assert.That(statistics.TotalFinds, Is.EqualTo(200));
        Assert.That(statistics.CompletedFinds, Is.EqualTo(150));
        Assert.That(statistics.GeneratedAt, Is.Not.EqualTo(DateTime.MinValue));
    }

    [Test]
    public void SystemOverviewStatistics_DefaultValues_AreCorrect()
    {
        // Act
        var statistics = new SystemOverviewStatistics();

        // Assert
        Assert.That(statistics.TotalCampaigns, Is.EqualTo(0));
        Assert.That(statistics.ActiveCampaigns, Is.EqualTo(0));
        Assert.That(statistics.TotalQrCodes, Is.EqualTo(0));
        Assert.That(statistics.TotalUsers, Is.EqualTo(0));
        Assert.That(statistics.TotalFinds, Is.EqualTo(0));
        Assert.That(statistics.CompletedFinds, Is.EqualTo(0));
        Assert.That(statistics.GeneratedAt, Is.EqualTo(DateTime.MinValue));
    }

    #endregion

    #region CampaignStatistics Tests

    [Test]
    public void CampaignStatistics_Properties_CanBeSet()
    {
        // Arrange
        var statistics = new CampaignStatistics();

        // Act
        statistics.CampaignId = 1;
        statistics.CampaignName = "Test Campaign";
        statistics.TotalQrCodes = 20;
        statistics.TotalFinds = 100;
        statistics.UniqueFinders = 50;
        statistics.CompletionRate = 0.75;
        statistics.GeneratedAt = DateTime.UtcNow;

        // Assert
        Assert.That(statistics.CampaignId, Is.EqualTo(1));
        Assert.That(statistics.CampaignName, Is.EqualTo("Test Campaign"));
        Assert.That(statistics.TotalQrCodes, Is.EqualTo(20));
        Assert.That(statistics.TotalFinds, Is.EqualTo(100));
        Assert.That(statistics.UniqueFinders, Is.EqualTo(50));
        Assert.That(statistics.CompletionRate, Is.EqualTo(0.75));
        Assert.That(statistics.GeneratedAt, Is.Not.EqualTo(DateTime.MinValue));
    }

    [Test]
    public void CampaignStatistics_DefaultValues_AreCorrect()
    {
        // Act
        var statistics = new CampaignStatistics();

        // Assert
        Assert.That(statistics.CampaignId, Is.EqualTo(0));
        Assert.That(statistics.CampaignName, Is.EqualTo(string.Empty));
        Assert.That(statistics.TotalQrCodes, Is.EqualTo(0));
        Assert.That(statistics.TotalFinds, Is.EqualTo(0));
        Assert.That(statistics.UniqueFinders, Is.EqualTo(0));
        Assert.That(statistics.CompletionRate, Is.EqualTo(0.0));
        Assert.That(statistics.GeneratedAt, Is.EqualTo(DateTime.MinValue));
    }

    #endregion

    #region UserStatistics Tests

    [Test]
    public void UserStatistics_Properties_CanBeSet()
    {
        // Arrange
        var statistics = new UserStatistics();

        // Act
        statistics.UserId = 123;
        statistics.UserName = "Test User";
        statistics.TotalFinds = 10;
        statistics.UniqueQrCodesFound = 8;
        statistics.FirstFindDate = DateTime.UtcNow.AddDays(-30);
        statistics.LastFindDate = DateTime.UtcNow;
        statistics.GeneratedAt = DateTime.UtcNow;

        // Assert
        Assert.That(statistics.UserId, Is.EqualTo(123));
        Assert.That(statistics.UserName, Is.EqualTo("Test User"));
        Assert.That(statistics.TotalFinds, Is.EqualTo(10));
        Assert.That(statistics.UniqueQrCodesFound, Is.EqualTo(8));
        Assert.That(statistics.FirstFindDate, Is.Not.Null);
        Assert.That(statistics.LastFindDate, Is.Not.Null);
        Assert.That(statistics.GeneratedAt, Is.Not.EqualTo(DateTime.MinValue));
    }

    [Test]
    public void UserStatistics_DefaultValues_AreCorrect()
    {
        // Act
        var statistics = new UserStatistics();

        // Assert
        Assert.That(statistics.UserId, Is.EqualTo(0));
        Assert.That(statistics.UserName, Is.EqualTo(string.Empty));
        Assert.That(statistics.TotalFinds, Is.EqualTo(0));
        Assert.That(statistics.UniqueQrCodesFound, Is.EqualTo(0));
        Assert.That(statistics.FirstFindDate, Is.Null);
        Assert.That(statistics.LastFindDate, Is.Null);
        Assert.That(statistics.GeneratedAt, Is.EqualTo(DateTime.MinValue));
    }

    #endregion

    #region QrCodeStatisticsDto Tests

    [Test]
    public void QrCodeStatisticsDto_Properties_CanBeSet()
    {
        // Arrange
        var statistics = new QrCodeStatisticsDto();

        // Act
        statistics.QrCodeId = 456;
        statistics.QrCodeTitle = "Test QR Code";
        statistics.TotalFinds = 25;
        statistics.UniqueFinders = 20;
        statistics.FirstFindDate = DateTime.UtcNow.AddDays(-10);
        statistics.LastFindDate = DateTime.UtcNow;
        statistics.RecentFinds = new List<QrCodeStatisticsRecentFind>();
        statistics.GeneratedAt = DateTime.UtcNow;

        // Assert
        Assert.That(statistics.QrCodeId, Is.EqualTo(456));
        Assert.That(statistics.QrCodeTitle, Is.EqualTo("Test QR Code"));
        Assert.That(statistics.TotalFinds, Is.EqualTo(25));
        Assert.That(statistics.UniqueFinders, Is.EqualTo(20));
        Assert.That(statistics.FirstFindDate, Is.Not.Null);
        Assert.That(statistics.LastFindDate, Is.Not.Null);
        Assert.That(statistics.RecentFinds, Is.Not.Null);
        Assert.That(statistics.GeneratedAt, Is.Not.EqualTo(DateTime.MinValue));
    }

    [Test]
    public void QrCodeStatisticsDto_DefaultValues_AreCorrect()
    {
        // Act
        var statistics = new QrCodeStatisticsDto();

        // Assert
        Assert.That(statistics.QrCodeId, Is.EqualTo(0));
        Assert.That(statistics.QrCodeTitle, Is.EqualTo(string.Empty));
        Assert.That(statistics.TotalFinds, Is.EqualTo(0));
        Assert.That(statistics.UniqueFinders, Is.EqualTo(0));
        Assert.That(statistics.FirstFindDate, Is.Null);
        Assert.That(statistics.LastFindDate, Is.Null);
        Assert.That(statistics.RecentFinds, Is.Not.Null);
        Assert.That(statistics.GeneratedAt, Is.EqualTo(DateTime.MinValue));
    }

    #endregion

    #region QrCodeStatisticsRecentFind Tests

    [Test]
    public void QrCodeStatisticsRecentFind_Properties_CanBeSet()
    {
        // Arrange
        var recentFind = new QrCodeStatisticsRecentFind();

        // Act
        recentFind.UserId = 789;
        recentFind.UserName = "Finder User";
        recentFind.FoundAt = DateTime.UtcNow;
        recentFind.IpAddress = "192.168.1.100";

        // Assert
        Assert.That(recentFind.UserId, Is.EqualTo(789));
        Assert.That(recentFind.UserName, Is.EqualTo("Finder User"));
        Assert.That(recentFind.FoundAt, Is.Not.EqualTo(DateTime.MinValue));
        Assert.That(recentFind.IpAddress, Is.EqualTo("192.168.1.100"));
    }

    [Test]
    public void QrCodeStatisticsRecentFind_DefaultValues_AreCorrect()
    {
        // Act
        var recentFind = new QrCodeStatisticsRecentFind();

        // Assert
        Assert.That(recentFind.UserId, Is.EqualTo(0));
        Assert.That(recentFind.UserName, Is.EqualTo(string.Empty));
        Assert.That(recentFind.FoundAt, Is.EqualTo(DateTime.MinValue));
        Assert.That(recentFind.IpAddress, Is.EqualTo(string.Empty));
    }

    #endregion

    #region CampaignQrCodeStatisticsDto Tests

    [Test]
    public void CampaignQrCodeStatisticsDto_Properties_CanBeSet()
    {
        // Arrange
        var statistics = new CampaignQrCodeStatisticsDto();

        // Act
        statistics.CampaignId = 999;
        statistics.CampaignName = "Campaign Name";
        statistics.QrCodeStatistics = new List<QrCodeStatisticsDto>();

        // Assert
        Assert.That(statistics.CampaignId, Is.EqualTo(999));
        Assert.That(statistics.CampaignName, Is.EqualTo("Campaign Name"));
        Assert.That(statistics.QrCodeStatistics, Is.Not.Null);
    }

    [Test]
    public void CampaignQrCodeStatisticsDto_DefaultValues_AreCorrect()
    {
        // Act
        var statistics = new CampaignQrCodeStatisticsDto();

        // Assert
        Assert.That(statistics.CampaignId, Is.EqualTo(0));
        Assert.That(statistics.CampaignName, Is.EqualTo(string.Empty));
        Assert.That(statistics.QrCodeStatistics, Is.Not.Null);
    }

    #endregion

    #region TopPerformersStatistics Tests

    [Test]
    public void TopPerformersStatistics_Properties_CanBeSet()
    {
        // Arrange
        var statistics = new TopPerformersStatistics();

        // Act
        statistics.TopByTotalFinds = new List<UserStatistics>();
        statistics.TopByUniqueQrCodes = new List<UserStatistics>();
        statistics.MostRecentActivity = new List<UserStatistics>();
        statistics.GeneratedAt = DateTime.UtcNow;

        // Assert
        Assert.That(statistics.TopByTotalFinds, Is.Not.Null);
        Assert.That(statistics.TopByUniqueQrCodes, Is.Not.Null);
        Assert.That(statistics.MostRecentActivity, Is.Not.Null);
        Assert.That(statistics.GeneratedAt, Is.Not.EqualTo(DateTime.MinValue));
    }

    [Test]
    public void TopPerformersStatistics_DefaultValues_AreCorrect()
    {
        // Act
        var statistics = new TopPerformersStatistics();

        // Assert
        Assert.That(statistics.TopByTotalFinds, Is.Not.Null);
        Assert.That(statistics.TopByUniqueQrCodes, Is.Not.Null);
        Assert.That(statistics.MostRecentActivity, Is.Not.Null);
        Assert.That(statistics.GeneratedAt, Is.EqualTo(DateTime.MinValue));
    }

    #endregion
}

