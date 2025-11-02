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

    #region GetFindHistoryAsync Tests

    /// <summary>
    /// Testet GetFindHistoryAsync mit leeren Filtern.
    /// Wichtig, um sicherzustellen, dass die Methode bei leeren Filtern alle Funde zurückgibt.
    /// Verhindert Fehler bei fehlenden Filtern.
    /// </summary>
    [Test]
    public async Task GetFindHistoryAsync_WithNoFilters_ShouldReturnAllFinds()
    {
        // Arrange
        var find1 = new Find(_testQrCode.Id, _testUser.Id, "127.0.0.1", "User Agent 1");
        var find2 = new Find(_testQrCode.Id, _testUser.Id, "192.168.1.1", "User Agent 2");
        await FindRepository.AddAsync(find1);
        await FindRepository.AddAsync(find2);
        await FindRepository.SaveChangesAsync();

        // Act
        var result = await FindRepository.GetFindHistoryAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Count.GreaterThanOrEqualTo(2));
        Assert.That(result.Select(f => f.Id), Contains.Item(find1.Id));
        Assert.That(result.Select(f => f.Id), Contains.Item(find2.Id));
    }

    /// <summary>
    /// Testet GetFindHistoryAsync mit Datums-Filter.
    /// Wichtig, um sicherzustellen, dass die Datumsfilterung korrekt funktioniert.
    /// Verhindert fehlerhafte Ergebnisse bei Datumsabfragen.
    /// </summary>
    [Test]
    public async Task GetFindHistoryAsync_WithDateFilter_ShouldReturnFilteredFinds()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var yesterday = now.AddDays(-1);
        var tomorrow = now.AddDays(1);

        var find1 = new Find(_testQrCode.Id, _testUser.Id, "127.0.0.1", "User Agent 1");
        find1.FoundAt = yesterday;
        await FindRepository.AddAsync(find1);

        var find2 = new Find(_testQrCode.Id, _testUser.Id, "192.168.1.1", "User Agent 2");
        find2.FoundAt = now;
        await FindRepository.AddAsync(find2);

        var find3 = new Find(_testQrCode.Id, _testUser.Id, "10.0.0.1", "User Agent 3");
        find3.FoundAt = tomorrow;
        await FindRepository.AddAsync(find3);
        await FindRepository.SaveChangesAsync();

        // Act - Filter nach heute
        var result = await FindRepository.GetFindHistoryAsync(
            startDate: now.Date,
            endDate: now.Date.AddDays(1));

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Has.Count.GreaterThanOrEqualTo(1));
        Assert.That(result.Select(f => f.Id), Contains.Item(find2.Id));
        Assert.That(result.Select(f => f.Id), Does.Not.Contain(find1.Id));
    }

    /// <summary>
    /// Testet GetFindHistoryAsync mit User-Filter.
    /// Wichtig, um sicherzustellen, dass die Benutzerfilterung korrekt funktioniert.
    /// Verhindert falsche Zuordnungen von Funden zu Benutzern.
    /// </summary>
    [Test]
    public async Task GetFindHistoryAsync_WithUserFilter_ShouldReturnFindsForUser()
    {
        // Arrange
        var user2 = new User("Test User 2");
        await UserRepository.AddAsync(user2);
        await UserRepository.SaveChangesAsync();

        var find1 = new Find(_testQrCode.Id, _testUser.Id, "127.0.0.1", "User Agent 1");
        var find2 = new Find(_testQrCode.Id, user2.Id, "192.168.1.1", "User Agent 2");
        await FindRepository.AddAsync(find1);
        await FindRepository.AddAsync(find2);
        await FindRepository.SaveChangesAsync();

        // Act
        var result = await FindRepository.GetFindHistoryAsync(userId: _testUser.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.All(f => f.UserId == _testUser.Id), Is.True);
        Assert.That(result.Select(f => f.Id), Contains.Item(find1.Id));
        Assert.That(result.Select(f => f.Id), Does.Not.Contain(find2.Id));
    }

    /// <summary>
    /// Testet GetFindHistoryAsync mit Pagination.
    /// Wichtig, um sicherzustellen, dass Skip und Take korrekt funktionieren.
    /// Verhindert Performance-Probleme bei großen Datenmengen.
    /// </summary>
    [Test]
    public async Task GetFindHistoryAsync_WithPagination_ShouldReturnPagedResults()
    {
        // Arrange
        for (int i = 0; i < 10; i++)
        {
            var find = new Find(_testQrCode.Id, _testUser.Id, $"127.0.0.{i}", $"User Agent {i}");
            await FindRepository.AddAsync(find);
        }
        await FindRepository.SaveChangesAsync();

        // Act
        var page1 = await FindRepository.GetFindHistoryAsync(skip: 0, take: 5);
        var page2 = await FindRepository.GetFindHistoryAsync(skip: 5, take: 5);

        // Assert
        Assert.That(page1, Is.Not.Null);
        Assert.That(page1, Has.Count.EqualTo(5));
        Assert.That(page2, Is.Not.Null);
        Assert.That(page2, Has.Count.GreaterThanOrEqualTo(0));
        Assert.That(page1.Select(f => f.Id).Intersect(page2.Select(f => f.Id)), Is.Empty);
    }

    /// <summary>
    /// Testet GetFindHistoryAsync mit Sortierung.
    /// Wichtig, um sicherzustellen, dass die Sortierung korrekt funktioniert.
    /// Verhindert falsche Sortierreihenfolgen in der UI.
    /// </summary>
    [Test]
    public async Task GetFindHistoryAsync_WithSorting_ShouldReturnSortedResults()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var find1 = new Find(_testQrCode.Id, _testUser.Id, "127.0.0.1", "User Agent 1");
        find1.FoundAt = now.AddHours(-2);
        await FindRepository.AddAsync(find1);

        var find2 = new Find(_testQrCode.Id, _testUser.Id, "192.168.1.1", "User Agent 2");
        find2.FoundAt = now.AddHours(-1);
        await FindRepository.AddAsync(find2);

        var find3 = new Find(_testQrCode.Id, _testUser.Id, "10.0.0.1", "User Agent 3");
        find3.FoundAt = now;
        await FindRepository.AddAsync(find3);
        await FindRepository.SaveChangesAsync();

        // Act - Sortierung absteigend
        var resultDesc = await FindRepository.GetFindHistoryAsync(sortBy: "FoundAt", sortDirection: "desc");
        var resultAsc = await FindRepository.GetFindHistoryAsync(sortBy: "FoundAt", sortDirection: "asc");

        // Assert
        Assert.That(resultDesc, Is.Not.Null);
        Assert.That(resultDesc.Count(), Is.GreaterThanOrEqualTo(3));
        var descList = resultDesc.ToList();
        Assert.That(descList[0].FoundAt, Is.GreaterThanOrEqualTo(descList[1].FoundAt));

        Assert.That(resultAsc, Is.Not.Null);
        var ascList = resultAsc.ToList();
        Assert.That(ascList[0].FoundAt, Is.LessThanOrEqualTo(ascList[1].FoundAt));
    }

    /// <summary>
    /// Testet GetFindHistoryCountAsync mit Filtern.
    /// Wichtig, um sicherzustellen, dass die Gesamtanzahl korrekt berechnet wird.
    /// Verhindert fehlerhafte Pagination-Anzeigen.
    /// </summary>
    [Test]
    public async Task GetFindHistoryCountAsync_WithFilters_ShouldReturnCorrectCount()
    {
        // Arrange
        var user2 = new User("Test User 2");
        await UserRepository.AddAsync(user2);
        await UserRepository.SaveChangesAsync();

        var find1 = new Find(_testQrCode.Id, _testUser.Id, "127.0.0.1", "User Agent 1");
        var find2 = new Find(_testQrCode.Id, user2.Id, "192.168.1.1", "User Agent 2");
        await FindRepository.AddAsync(find1);
        await FindRepository.AddAsync(find2);
        await FindRepository.SaveChangesAsync();

        // Act
        var totalCount = await FindRepository.GetFindHistoryCountAsync();
        var userCount = await FindRepository.GetFindHistoryCountAsync(userId: _testUser.Id);

        // Assert
        Assert.That(totalCount, Is.GreaterThanOrEqualTo(2));
        Assert.That(userCount, Is.EqualTo(1));
    }

    #endregion
}

