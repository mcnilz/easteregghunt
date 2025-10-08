using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Integration.Tests;
using NUnit.Framework;

namespace EasterEggHunt.Integration.Tests.Repositories;

/// <summary>
/// Integration Tests für QrCode Repository mit echter SQLite-Datenbank
/// </summary>
[TestFixture]
public class QrCodeRepositoryIntegrationTests : IntegrationTestBase
{
    [SetUp]
    public void Setup()
    {
        // Test-Daten werden bereits in IntegrationTestBase geladen
    }

    [Test]
    public void GetQrCodesByCampaign_WithValidCampaignId_ReturnsQrCodes()
    {
        // Arrange
        var campaignId = 1;

        // Act
        var qrCodes = Context.QrCodes.Where(q => q.CampaignId == campaignId).ToList();

        // Assert
        Assert.That(qrCodes, Is.Not.Empty);
        Assert.That(qrCodes.Count, Is.GreaterThanOrEqualTo(2)); // Mindestens 2 QR-Codes in Test-Daten
        Assert.That(qrCodes.All(q => q.CampaignId == campaignId), Is.True);
    }

    [Test]
    public void GetQrCode_WithValidId_ReturnsQrCode()
    {
        // Arrange
        var qrCodeId = 1;

        // Act
        var qrCode = Context.QrCodes.FirstOrDefault(q => q.Id == qrCodeId);

        // Assert
        Assert.That(qrCode, Is.Not.Null);
        Assert.That(qrCode!.Id, Is.EqualTo(qrCodeId));
        Assert.That(qrCode.Title, Is.EqualTo("QR Code 1"));
        Assert.That(qrCode.Description, Is.EqualTo("Beschreibung 1"));
    }

    [Test]
    public void GetQrCode_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var qrCodeId = 999;

        // Act
        var qrCode = Context.QrCodes.FirstOrDefault(q => q.Id == qrCodeId);

        // Assert
        Assert.That(qrCode, Is.Null);
    }

    [Test]
    public async Task CreateQrCode_WithValidData_CreatesQrCode()
    {
        // Arrange
        var uniqueId = Random.Shared.Next(1000, 9999);
        var newQrCode = new QrCode(1, "Test QR Code", "Test Beschreibung", "Test Notiz")
        {
            Id = uniqueId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        Context.QrCodes.Add(newQrCode);
        await Context.SaveChangesAsync();

        // Assert
        var createdQrCode = Context.QrCodes.FirstOrDefault(q => q.Id == uniqueId);
        Assert.That(createdQrCode, Is.Not.Null);
        Assert.That(createdQrCode!.Title, Is.EqualTo("Test QR Code"));
        Assert.That(createdQrCode.Description, Is.EqualTo("Test Beschreibung"));
        Assert.That(createdQrCode.InternalNotes, Is.EqualTo("Test Notiz"));
    }

    [Test]
    public async Task UpdateQrCode_WithValidData_UpdatesQrCode()
    {
        // Arrange
        var uniqueId = Random.Shared.Next(1000, 9999);
        var qrCode = new QrCode(1, "Test QR Code", "Test Beschreibung", "Test Notiz")
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
    public async Task DeleteQrCode_WithValidId_DeletesQrCode()
    {
        // Arrange
        var uniqueId = Random.Shared.Next(1000, 9999);
        var qrCode = new QrCode(1, "Test QR Code", "Test Beschreibung", "Test Notiz")
        {
            Id = uniqueId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        Context.QrCodes.Add(qrCode);
        await Context.SaveChangesAsync();

        // Act
        Context.QrCodes.Remove(qrCode);
        await Context.SaveChangesAsync();

        // Assert
        var deletedQrCode = Context.QrCodes.FirstOrDefault(q => q.Id == uniqueId);
        Assert.That(deletedQrCode, Is.Null);
    }

    [Test]
    public async Task ActivateQrCode_WithValidId_ActivatesQrCode()
    {
        // Arrange
        var uniqueId = Random.Shared.Next(1000, 9999);
        var qrCode = new QrCode(1, "Test QR Code", "Test Beschreibung", "Test Notiz")
        {
            Id = uniqueId,
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        Context.QrCodes.Add(qrCode);
        await Context.SaveChangesAsync();

        // Act
        qrCode.Activate();
        await Context.SaveChangesAsync();

        // Assert
        var activatedQrCode = Context.QrCodes.FirstOrDefault(q => q.Id == uniqueId);
        Assert.That(activatedQrCode, Is.Not.Null);
        Assert.That(activatedQrCode!.IsActive, Is.True);
    }

    [Test]
    public async Task DeactivateQrCode_WithValidId_DeactivatesQrCode()
    {
        // Arrange
        var uniqueId = Random.Shared.Next(1000, 9999);
        var qrCode = new QrCode(1, "Test QR Code", "Test Beschreibung", "Test Notiz")
        {
            Id = uniqueId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        Context.QrCodes.Add(qrCode);
        await Context.SaveChangesAsync();

        // Act
        qrCode.Deactivate();
        await Context.SaveChangesAsync();

        // Assert
        var deactivatedQrCode = Context.QrCodes.FirstOrDefault(q => q.Id == uniqueId);
        Assert.That(deactivatedQrCode, Is.Not.Null);
        Assert.That(deactivatedQrCode!.IsActive, Is.False);
    }

    [Test]
    public async Task SetSortOrder_WithValidData_UpdatesSortOrder()
    {
        // Arrange
        var uniqueId = Random.Shared.Next(1000, 9999);
        var qrCode = new QrCode(1, "Test QR Code", "Test Beschreibung", "Test Notiz")
        {
            Id = uniqueId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        Context.QrCodes.Add(qrCode);
        await Context.SaveChangesAsync();

        // Act
        qrCode.SetSortOrder(42);
        await Context.SaveChangesAsync();

        // Assert
        var updatedQrCode = Context.QrCodes.FirstOrDefault(q => q.Id == uniqueId);
        Assert.That(updatedQrCode, Is.Not.Null);
        Assert.That(updatedQrCode!.SortOrder, Is.EqualTo(42));
    }

    [Test]
    public void QrCode_WithValidData_HasCorrectProperties()
    {
        // Arrange & Act
        var qrCode = Context.QrCodes.FirstOrDefault(q => q.Id == 1);

        // Assert
        Assert.That(qrCode, Is.Not.Null);
        Assert.That(qrCode!.CampaignId, Is.EqualTo(1));
        Assert.That(qrCode.Title, Is.EqualTo("QR Code 1"));
        Assert.That(qrCode.Description, Is.EqualTo("Beschreibung 1"));
        Assert.That(qrCode.InternalNotes, Is.EqualTo("Notiz 1"));
        Assert.That(qrCode.IsActive, Is.True);
        Assert.That(qrCode.CreatedAt, Is.Not.EqualTo(DateTime.MinValue));
        Assert.That(qrCode.UpdatedAt, Is.Not.EqualTo(DateTime.MinValue));
    }
}
