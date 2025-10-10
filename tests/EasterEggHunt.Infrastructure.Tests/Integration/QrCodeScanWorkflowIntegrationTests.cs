using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Infrastructure.Data;
using EasterEggHunt.Infrastructure.Repositories;
using EasterEggHunt.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace EasterEggHunt.Infrastructure.Tests.Integration;

[TestFixture]
public class QrCodeScanWorkflowIntegrationTests : IntegrationTestBase
{
    private QrCodeRepository _qrCodeRepository = null!;
    private FindRepository _findRepository = null!;
    private QrCodeService _qrCodeService = null!;
    private FindService _findService = null!;

    [SetUp]
    public async Task Setup()
    {
        await SeedTestDataAsync();
        
        _qrCodeRepository = new QrCodeRepository(_context);
        _findRepository = new FindRepository(_context);
        
        var mockQrCodeLogger = new Mock<ILogger<QrCodeService>>();
        var mockFindLogger = new Mock<ILogger<FindService>>();
        
        _qrCodeService = new QrCodeService(_qrCodeRepository, mockQrCodeLogger.Object);
        _findService = new FindService(_findRepository, mockFindLogger.Object);
    }

    [Test]
    public async Task QrCodeScanWorkflow_FirstTimeScan_RegistersFindSuccessfully()
    {
        // Arrange
        var uniqueUrl = "https://example.com/qr/test-code";
        var userId = 1;
        var ipAddress = "127.0.0.1";
        var userAgent = "TestAgent";

        // Act - QR-Code abrufen
        var qrCode = await _qrCodeService.GetQrCodeByUniqueUrlAsync(uniqueUrl);
        
        // Assert - QR-Code gefunden
        Assert.That(qrCode, Is.Not.Null);
        Assert.That(qrCode!.Title, Is.EqualTo("Test QR-Code"));

        // Act - Prüfen ob bereits gefunden
        var existingFind = await _findService.GetExistingFindAsync(qrCode.Id, userId);
        
        // Assert - Noch nicht gefunden
        Assert.That(existingFind, Is.Null);

        // Act - Fund registrieren
        var newFind = await _findService.RegisterFindAsync(qrCode.Id, userId, ipAddress, userAgent);
        
        // Assert - Fund erfolgreich registriert
        Assert.That(newFind, Is.Not.Null);
        Assert.That(newFind.QrCodeId, Is.EqualTo(qrCode.Id));
        Assert.That(newFind.UserId, Is.EqualTo(userId));
        Assert.That(newFind.IpAddress, Is.EqualTo(ipAddress));
        Assert.That(newFind.UserAgent, Is.EqualTo(userAgent));
        Assert.That(newFind.FoundAt, Is.Not.EqualTo(DateTime.MinValue));

        // Act - Prüfen ob jetzt gefunden
        var hasFound = await _findService.HasUserFoundQrCodeAsync(qrCode.Id, userId);
        
        // Assert - Jetzt gefunden
        Assert.That(hasFound, Is.True);
    }

    [Test]
    public async Task QrCodeScanWorkflow_MultipleScans_AllowsMultipleFinds()
    {
        // Arrange
        var uniqueUrl = "https://example.com/qr/test-code";
        var userId = 1;
        var ipAddress1 = "127.0.0.1";
        var ipAddress2 = "127.0.0.2";
        var userAgent1 = "TestAgent1";
        var userAgent2 = "TestAgent2";

        // Act - Ersten Fund registrieren
        var qrCode = await _qrCodeService.GetQrCodeByUniqueUrlAsync(uniqueUrl);
        var firstFind = await _findService.RegisterFindAsync(qrCode!.Id, userId, ipAddress1, userAgent1);
        
        // Assert - Erster Fund erfolgreich
        Assert.That(firstFind, Is.Not.Null);
        Assert.That(firstFind.FoundAt, Is.Not.EqualTo(DateTime.MinValue));

        // Act - Zweiten Fund registrieren
        var secondFind = await _findService.RegisterFindAsync(qrCode.Id, userId, ipAddress2, userAgent2);
        
        // Assert - Zweiter Fund erfolgreich
        Assert.That(secondFind, Is.Not.Null);
        Assert.That(secondFind.Id, Is.Not.EqualTo(firstFind.Id));
        Assert.That(secondFind.IpAddress, Is.EqualTo(ipAddress2));
        Assert.That(secondFind.UserAgent, Is.EqualTo(userAgent2));

        // Act - Ersten Fund abrufen
        var existingFind = await _findService.GetExistingFindAsync(qrCode.Id, userId);
        
        // Assert - Erster Fund wird zurückgegeben
        Assert.That(existingFind, Is.Not.Null);
        Assert.That(existingFind!.Id, Is.EqualTo(firstFind.Id));
        Assert.That(existingFind.FoundAt, Is.EqualTo(firstFind.FoundAt));

        // Act - Prüfen ob gefunden
        var hasFound = await _findService.HasUserFoundQrCodeAsync(qrCode.Id, userId);
        
        // Assert - Gefunden
        Assert.That(hasFound, Is.True);
    }

    [Test]
    public async Task QrCodeScanWorkflow_InvalidUniqueUrl_ReturnsNull()
    {
        // Arrange
        var invalidUniqueUrl = "https://example.com/qr/invalid-code";

        // Act
        var qrCode = await _qrCodeService.GetQrCodeByUniqueUrlAsync(invalidUniqueUrl);

        // Assert
        Assert.That(qrCode, Is.Null);
    }

    [Test]
    public async Task QrCodeScanWorkflow_EmptyUniqueUrl_ReturnsNull()
    {
        // Arrange
        var emptyUniqueUrl = "";

        // Act
        var qrCode = await _qrCodeService.GetQrCodeByUniqueUrlAsync(emptyUniqueUrl);

        // Assert
        Assert.That(qrCode, Is.Null);
    }

    [Test]
    public async Task QrCodeScanWorkflow_InvalidUniqueUrlFormat_ReturnsNull()
    {
        // Arrange
        var invalidFormatUrl = "not-a-valid-url";

        // Act
        var qrCode = await _qrCodeService.GetQrCodeByUniqueUrlAsync(invalidFormatUrl);

        // Assert
        Assert.That(qrCode, Is.Null);
    }

    [Test]
    public async Task QrCodeScanWorkflow_DifferentUsers_SeparateFinds()
    {
        // Arrange
        var uniqueUrl = "https://example.com/qr/test-code";
        var userId1 = 1;
        var userId2 = 2;
        var ipAddress = "127.0.0.1";
        var userAgent = "TestAgent";

        // Act - User 1 scannt
        var qrCode = await _qrCodeService.GetQrCodeByUniqueUrlAsync(uniqueUrl);
        var find1 = await _findService.RegisterFindAsync(qrCode!.Id, userId1, ipAddress, userAgent);
        
        // Act - User 2 scannt
        var find2 = await _findService.RegisterFindAsync(qrCode.Id, userId2, ipAddress, userAgent);

        // Assert - Beide Funde erfolgreich
        Assert.That(find1, Is.Not.Null);
        Assert.That(find2, Is.Not.Null);
        Assert.That(find1.Id, Is.Not.EqualTo(find2.Id));
        Assert.That(find1.UserId, Is.EqualTo(userId1));
        Assert.That(find2.UserId, Is.EqualTo(userId2));

        // Act - Prüfen ob beide gefunden haben
        var hasFound1 = await _findService.HasUserFoundQrCodeAsync(qrCode.Id, userId1);
        var hasFound2 = await _findService.HasUserFoundQrCodeAsync(qrCode.Id, userId2);
        
        // Assert - Beide haben gefunden
        Assert.That(hasFound1, Is.True);
        Assert.That(hasFound2, Is.True);
    }

    private async Task SeedTestDataAsync()
    {
        // Kampagne erstellen
        var campaign = new Campaign("Test Campaign", "Test Description", DateTime.Now, DateTime.Now.AddDays(1));
        _context.Campaigns.Add(campaign);
        await _context.SaveChangesAsync();

        // QR-Code erstellen
        var qrCode = new QrCode(campaign.Id, "Test QR-Code", "Test Description", "Test Notes")
        {
            UniqueUrl = new Uri("https://example.com/qr/test-code")
        };
        _context.QrCodes.Add(qrCode);

        // User erstellen
        var user = new User("Test User", "test@example.com");
        _context.Users.Add(user);

        await _context.SaveChangesAsync();
    }
}
