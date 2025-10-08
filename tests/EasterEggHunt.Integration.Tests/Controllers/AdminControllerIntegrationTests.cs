using EasterEggHunt.Integration.Tests;
using NUnit.Framework;

namespace EasterEggHunt.Integration.Tests.Controllers;

/// <summary>
/// Integration Tests für Datenbank-Operationen mit echter SQLite-Datenbank
/// </summary>
[TestFixture]
public class AdminControllerIntegrationTests : IntegrationTestBase
{
    [SetUp]
    public void Setup()
    {
        // Test-Daten werden bereits in IntegrationTestBase geladen
    }

    [Test]
    public async Task QrCode_WithDescription_CanBeCreatedAndRetrieved()
    {
        // Arrange
        var uniqueId = Random.Shared.Next(1000, 9999);
        var qrCode = new EasterEggHunt.Domain.Entities.QrCode(1, "Test QR Code", "Test Beschreibung", "Test Notiz")
        {
            Id = uniqueId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        Context.QrCodes.Add(qrCode);
        await Context.SaveChangesAsync();

        // Assert
        var retrievedQrCode = Context.QrCodes.FirstOrDefault(q => q.Id == uniqueId);
        Assert.That(retrievedQrCode, Is.Not.Null);
        Assert.That(retrievedQrCode!.Title, Is.EqualTo("Test QR Code"));
        Assert.That(retrievedQrCode.Description, Is.EqualTo("Test Beschreibung"));
        Assert.That(retrievedQrCode.InternalNotes, Is.EqualTo("Test Notiz"));
    }

    [Test]
    public async Task QrCode_Description_CanBeUpdated()
    {
        // Arrange - Erstelle einen neuen QR-Code für diesen Test
        var uniqueId = Random.Shared.Next(1000, 9999);
        var qrCode = new EasterEggHunt.Domain.Entities.QrCode(1, "Test QR Code", "Test Beschreibung", "Test Notiz")
        {
            Id = uniqueId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        Context.QrCodes.Add(qrCode);
        await Context.SaveChangesAsync();

        // Act
        qrCode.Update("Aktualisierter Titel", "Aktualisierte Beschreibung", "Aktualisierte Notiz");
        await Context.SaveChangesAsync();

        // Assert
        var updatedQrCode = Context.QrCodes.FirstOrDefault(q => q.Id == uniqueId);
        Assert.That(updatedQrCode, Is.Not.Null);
        Assert.That(updatedQrCode!.Title, Is.EqualTo("Aktualisierter Titel"));
        Assert.That(updatedQrCode.Description, Is.EqualTo("Aktualisierte Beschreibung"));
        Assert.That(updatedQrCode.InternalNotes, Is.EqualTo("Aktualisierte Notiz"));
    }

    [Test]
    public async Task QrCode_CanBeDeleted()
    {
        // Arrange
        var qrCode = Context.QrCodes.FirstOrDefault(q => q.Id == 1);
        Assert.That(qrCode, Is.Not.Null);

        // Act
        Context.QrCodes.Remove(qrCode!);
        await Context.SaveChangesAsync();

        // Assert
        var deletedQrCode = Context.QrCodes.FirstOrDefault(q => q.Id == 1);
        Assert.That(deletedQrCode, Is.Null);
    }

    [Test]
    public void Database_ContainsCorrectTestData()
    {
        // Assert
        var campaigns = Context.Campaigns.ToList();
        var qrCodes = Context.QrCodes.ToList();
        var users = Context.Users.ToList();
        var finds = Context.Finds.ToList();

        Assert.That(campaigns.Count, Is.EqualTo(1), $"Erwartet 1 Kampagne, aber {campaigns.Count} gefunden");
        Assert.That(qrCodes.Count, Is.EqualTo(2), $"Erwartet 2 QR-Codes, aber {qrCodes.Count} gefunden");
        Assert.That(users.Count, Is.EqualTo(1), $"Erwartet 1 Benutzer, aber {users.Count} gefunden");
        Assert.That(finds.Count, Is.EqualTo(2), $"Erwartet 2 Funde, aber {finds.Count} gefunden");

        if (campaigns.Any())
            Assert.That(campaigns[0].Name, Is.EqualTo("Test Kampagne"));
        if (qrCodes.Any())
        {
            Assert.That(qrCodes[0].Title, Is.EqualTo("QR Code 1"));
            Assert.That(qrCodes[0].Description, Is.EqualTo("Beschreibung 1"));
        }
        if (qrCodes.Count > 1)
        {
            Assert.That(qrCodes[1].Title, Is.EqualTo("QR Code 2"));
            Assert.That(qrCodes[1].Description, Is.EqualTo("Beschreibung 2"));
        }
        if (users.Any())
            Assert.That(users[0].Name, Is.EqualTo("Test Benutzer"));
    }
}
