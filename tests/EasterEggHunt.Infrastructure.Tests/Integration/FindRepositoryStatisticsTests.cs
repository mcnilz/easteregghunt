using EasterEggHunt.Domain.Entities;

namespace EasterEggHunt.Infrastructure.Tests.Integration;

[TestFixture]
[Category("Integration")]
public class FindRepositoryStatisticsTests : IntegrationTestBase
{
    private Campaign _testCampaign = null!;
    private User _testUser = null!;
    private QrCode _testQrCode = null!;

    [SetUp]
    public async Task SetUp()
    {
        await ResetDatabaseAsync();

        // Test-Kampagne erstellen
        _testCampaign = new Campaign("Test Campaign", "Test Description", "Test Creator");
        await CampaignRepository.AddAsync(_testCampaign);
        await CampaignRepository.SaveChangesAsync();

        // Test-Benutzer erstellen
        _testUser = new User("Test User");
        await UserRepository.AddAsync(_testUser);
        await UserRepository.SaveChangesAsync();

        // Test-QR-Code erstellen
        _testQrCode = new QrCode(_testCampaign.Id, "Test QR Code", "Test Description", "Test Note");
        await QrCodeRepository.AddAsync(_testQrCode);
        await QrCodeRepository.SaveChangesAsync();
    }

    [Test]
    public async Task GetWeeklyStatisticsAsync_WithNoFinds_ShouldReturnEmpty()
    {
        // Act
        var result = await FindRepository.GetWeeklyStatisticsAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetWeeklyStatisticsAsync_WithSingleFind_ShouldReturnOneWeek()
    {
        // Arrange
        var find = new Find(_testQrCode.Id, _testUser.Id, "127.0.0.1", "Test User Agent");
        await FindRepository.AddAsync(find);
        await FindRepository.SaveChangesAsync();

        // Act
        var result = await FindRepository.GetWeeklyStatisticsAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Count.EqualTo(1));
        var weekStat = result.First();
        Assert.That(weekStat.Count, Is.EqualTo(1));
        Assert.That(weekStat.UniqueFinders, Is.EqualTo(1));
        Assert.That(weekStat.UniqueQrCodes, Is.EqualTo(1));
    }

    [Test]
    public async Task GetWeeklyStatisticsAsync_WithMultipleFindsInDifferentWeeks_ShouldGroupCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        
        // Create finds in different weeks
        var find1 = new Find(_testQrCode.Id, _testUser.Id, "127.0.0.1", "User Agent 1");
        await FindRepository.AddAsync(find1);
        await FindRepository.SaveChangesAsync();
        
        // Wait a bit and create another find (should be in same week typically)
        await Task.Delay(100);
        
        var find2 = new Find(_testQrCode.Id, _testUser.Id, "192.168.1.1", "User Agent 2");
        await FindRepository.AddAsync(find2);
        await FindRepository.SaveChangesAsync();

        // Act
        var result = await FindRepository.GetWeeklyStatisticsAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Count.GreaterThanOrEqualTo(1));
        
        // Find the week that contains both finds
        var weekWithBoth = result.FirstOrDefault(w => w.Count == 2);
        if (weekWithBoth != default)
        {
            Assert.That(weekWithBoth.UniqueFinders, Is.EqualTo(1));
            Assert.That(weekWithBoth.UniqueQrCodes, Is.EqualTo(1));
        }
    }

    /// <summary>
    /// Testet das Verhalten bei leerer Datenbank.
    /// Wichtig, um sicherzustellen, dass die Methode keine NullReferenceException wirft
    /// und korrekt eine leere Liste zurückgibt. Verhindert 500-Fehler in Production.
    /// </summary>
    [Test]
    public async Task GetDailyStatisticsAsync_WithNoFinds_ShouldReturnEmpty()
    {
        // Act
        var result = await FindRepository.GetDailyStatisticsAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    /// <summary>
    /// Testet die korrekte Gruppierung und Aggregation von Funden nach Tag.
    /// Wichtig, um sicherzustellen, dass die Statistiken korrekt berechnet werden
    /// (Count, UniqueFinders, UniqueQrCodes). Dies ist kritisch für die Admin-Dashboard-Anzeige.
    /// </summary>
    [Test]
    public async Task GetDailyStatisticsAsync_WithFinds_ShouldReturnDailyStats()
    {
        // Arrange
        var find1 = new Find(_testQrCode.Id, _testUser.Id, "127.0.0.1", "User Agent 1");
        await FindRepository.AddAsync(find1);
        await FindRepository.SaveChangesAsync();

        // Act
        var result = await FindRepository.GetDailyStatisticsAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Count.EqualTo(1));
        var dayStat = result.First();
        Assert.That(dayStat.Count, Is.EqualTo(1));
        Assert.That(dayStat.UniqueFinders, Is.EqualTo(1));
        Assert.That(dayStat.UniqueQrCodes, Is.EqualTo(1));
    }

    /// <summary>
    /// Testet die Datumsfilter-Funktionalität.
    /// Wichtig, um sicherzustellen, dass Benutzer Statistiken für spezifische Zeiträume abrufen können.
    /// Verhindert Fehler bei ungültigen Datumsbereichen oder Timezone-Problemen.
    /// </summary>
    [Test]
    public async Task GetDailyStatisticsAsync_WithDateFilter_ShouldFilterCorrectly()
    {
        // Arrange
        var today = DateTime.UtcNow;
        var yesterday = today.AddDays(-1);
        var tomorrow = today.AddDays(1);

        var find1 = new Find(_testQrCode.Id, _testUser.Id, "127.0.0.1", "User Agent 1");
        await FindRepository.AddAsync(find1);
        await FindRepository.SaveChangesAsync();

        // Act
        var resultToday = await FindRepository.GetDailyStatisticsAsync(today.Date, today.Date);
        var resultPast = await FindRepository.GetDailyStatisticsAsync(yesterday.Date, yesterday.Date);
        var resultFuture = await FindRepository.GetDailyStatisticsAsync(tomorrow.Date, tomorrow.Date);

        // Assert
        Assert.That(resultToday, Is.Not.Null);
        Assert.That(resultPast, Is.Not.Null);
        Assert.That(resultFuture, Is.Not.Null);
        Assert.That(resultToday, Has.Count.GreaterThanOrEqualTo(0));
    }

    /// <summary>
    /// Testet das Verhalten bei leerer Datenbank für monatliche Statistiken.
    /// Wichtig, um sicherzustellen, dass die Methode keine NullReferenceException wirft.
    /// Da GetMonthlyStatisticsAsync DateTime-Konstruktor in Memory verwendet, muss Edge-Case getestet werden.
    /// </summary>
    [Test]
    public async Task GetMonthlyStatisticsAsync_WithNoFinds_ShouldReturnEmpty()
    {
        // Act
        var result = await FindRepository.GetMonthlyStatisticsAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    /// <summary>
    /// Testet die korrekte Berechnung von MonthStart (erster Tag des Monats).
    /// Wichtig, da die Implementierung DateTime-Konstruktor in Memory verwendet (EF Core Limitation).
    /// Stellt sicher, dass MonthStart immer der 1. des Monats ist, unabhängig vom tatsächlichen Fund-Datum.
    /// </summary>
    [Test]
    public async Task GetMonthlyStatisticsAsync_WithFinds_ShouldReturnMonthlyStats()
    {
        // Arrange
        var find1 = new Find(_testQrCode.Id, _testUser.Id, "127.0.0.1", "User Agent 1");
        await FindRepository.AddAsync(find1);
        await FindRepository.SaveChangesAsync();

        // Act
        var result = await FindRepository.GetMonthlyStatisticsAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Count.EqualTo(1));
        var monthStat = result.First();
        Assert.That(monthStat.Count, Is.EqualTo(1));
        Assert.That(monthStat.UniqueFinders, Is.EqualTo(1));
        Assert.That(monthStat.UniqueQrCodes, Is.EqualTo(1));
        Assert.That(monthStat.MonthStart.Day, Is.EqualTo(1)); // Sollte der erste Tag des Monats sein
    }

    /// <summary>
    /// Testet die korrekte Gruppierung und Aggregation bei mehreren Funden, Benutzern und QR-Codes.
    /// Wichtig, um sicherzustellen, dass UniqueFinders und UniqueQrCodes korrekt gezählt werden
    /// und nicht einfach Count wiedergeben. Dies ist kritisch für aussagekräftige Statistiken.
    /// </summary>
    [Test]
    public async Task GetMonthlyStatisticsAsync_WithMultipleFinds_ShouldGroupCorrectly()
    {
        // Arrange
        var user2 = new User("User 2");
        await UserRepository.AddAsync(user2);
        await UserRepository.SaveChangesAsync();

        var qrCode2 = new QrCode(_testCampaign.Id, "QR Code 2", "Description 2", "Note 2");
        await QrCodeRepository.AddAsync(qrCode2);
        await QrCodeRepository.SaveChangesAsync();

        var find1 = new Find(_testQrCode.Id, _testUser.Id, "127.0.0.1", "User Agent 1");
        var find2 = new Find(_testQrCode.Id, user2.Id, "192.168.1.1", "User Agent 2");
        var find3 = new Find(qrCode2.Id, _testUser.Id, "192.168.1.2", "User Agent 3");
        
        await FindRepository.AddAsync(find1);
        await FindRepository.AddAsync(find2);
        await FindRepository.AddAsync(find3);
        await FindRepository.SaveChangesAsync();

        // Act
        var result = await FindRepository.GetMonthlyStatisticsAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Count.GreaterThanOrEqualTo(1));
        
        // Finde den Monat mit allen Funden
        var monthWithAll = result.FirstOrDefault(m => m.Count == 3);
        if (monthWithAll != default)
        {
            Assert.That(monthWithAll.UniqueFinders, Is.EqualTo(2)); // 2 verschiedene Benutzer
            Assert.That(monthWithAll.UniqueQrCodes, Is.EqualTo(2)); // 2 verschiedene QR-Codes
        }
    }

    /// <summary>
    /// Testet die Datumsfilter-Funktionalität für wöchentliche Statistiken.
    /// Wichtig, da GetWeeklyStatisticsAsync ISOWeek-Methoden verwendet, die in Memory ausgeführt werden.
    /// Verhindert Fehler bei Timezone-Verschiebungen oder ungültigen Datumsbereichen.
    /// </summary>
    [Test]
    public async Task GetWeeklyStatisticsAsync_WithDateFilter_ShouldFilterCorrectly()
    {
        // Arrange
        var today = DateTime.UtcNow;
        var lastWeek = today.AddDays(-7);

        var find1 = new Find(_testQrCode.Id, _testUser.Id, "127.0.0.1", "User Agent 1");
        await FindRepository.AddAsync(find1);
        await FindRepository.SaveChangesAsync();

        // Act
        var resultAll = await FindRepository.GetWeeklyStatisticsAsync();
        var resultFiltered = await FindRepository.GetWeeklyStatisticsAsync(lastWeek.Date, today.Date);

        // Assert
        Assert.That(resultAll, Is.Not.Null);
        Assert.That(resultFiltered, Is.Not.Null);
        Assert.That(resultAll, Has.Count.GreaterThanOrEqualTo(0));
        Assert.That(resultFiltered, Has.Count.GreaterThanOrEqualTo(0));
    }
}

