using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EasterEggHunt.Api;
using EasterEggHunt.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace EasterEggHunt.Integration.Tests.Controllers;

/// <summary>
/// Integration Tests für QrCodesController mit WebApplicationFactory
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.None)]
public class QrCodesControllerIntegrationTests : IDisposable
{
    private WebApplicationFactory<IApiMarker>? _factory;
    private HttpClient _client = null!;
    private string? _databasePath;
    private JsonSerializerOptions _jsonOptions = null!;

    [SetUp]
    public void Setup()
    {
        // Eindeutige Test-Datenbank für jeden Test
        _databasePath = Path.Combine(Path.GetTempPath(), $"easteregghunt_controller_test_{Guid.NewGuid()}.db");

        // Lösche die Datenbank, falls sie bereits existiert
        if (File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }

#pragma warning disable CA2000
        _factory = new WebApplicationFactory<IApiMarker>()
#pragma warning restore CA2000
            .WithWebHostBuilder(builder =>
            {
                // Setze den ConnectionString über die Konfiguration
                builder.UseSetting("ConnectionStrings:DefaultConnection", $"Data Source={_databasePath}");

                // Aktiviere Migration, aber deaktiviere SeedData für Tests
                builder.UseSetting("EasterEggHunt:Database:SeedData", "false");
                builder.UseSetting("EasterEggHunt:Database:AutoMigrate", "true");
            });

        _client = _factory.CreateClient();

        // Konfiguriere JSON-Serialisierung für Tests (muss mit API übereinstimmen)
        _jsonOptions = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        // Test-Daten laden
        SeedTestData();
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
        _factory?.Dispose();

        // Test-Datenbank löschen
        if (_databasePath != null && File.Exists(_databasePath))
        {
            try
            {
                File.Delete(_databasePath);
            }
            catch (IOException)
            {
                // Ignoriere Löschfehler
            }
        }
    }

    private void SeedTestData()
    {
        using var scope = _factory!.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EasterEggHuntDbContext>();

        // Test-Kampagne erstellen
        var campaign = new EasterEggHunt.Domain.Entities.Campaign("Test Kampagne", "Test Beschreibung", "Test Admin")
        {
            Id = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Campaigns.Add(campaign);

        // Test-QR-Codes erstellen
        var qrCode1 = new EasterEggHunt.Domain.Entities.QrCode(1, "QR Code 1", "Beschreibung 1", "Notiz 1") { Id = 1, IsActive = true, SortOrder = 1 };
        var qrCode2 = new EasterEggHunt.Domain.Entities.QrCode(1, "QR Code 2", "Beschreibung 2", "Notiz 2") { Id = 2, IsActive = false, SortOrder = 2 };
        context.QrCodes.Add(qrCode1);
        context.QrCodes.Add(qrCode2);

        // Test-Benutzer erstellen
        var user = new EasterEggHunt.Domain.Entities.User("Test Benutzer") { Id = 1, IsActive = true };
        context.Users.Add(user);

        context.SaveChanges();
    }

    [Test]
    public async Task GetQrCodesByCampaign_WithValidCampaignId_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync(new Uri("/api/qrcodes/campaign/1", UriKind.Relative));

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await response.Content.ReadAsStringAsync();
        var qrCodes = JsonSerializer.Deserialize<List<EasterEggHunt.Domain.Entities.QrCode>>(content, _jsonOptions);

        Assert.That(qrCodes, Is.Not.Null);
        Assert.That(qrCodes!.Count, Is.EqualTo(2));
        Assert.That(qrCodes.All(q => q.CampaignId == 1), Is.True);
    }

    [Test]
    public async Task GetQrCodesByCampaign_WithInvalidCampaignId_ReturnsOkWithEmptyList()
    {
        // Act
        var response = await _client.GetAsync(new Uri("/api/qrcodes/campaign/999", UriKind.Relative));

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await response.Content.ReadAsStringAsync();
        var qrCodes = JsonSerializer.Deserialize<List<EasterEggHunt.Domain.Entities.QrCode>>(content, _jsonOptions);

        Assert.That(qrCodes, Is.Not.Null);
        Assert.That(qrCodes!, Is.Empty);
    }

    [Test]
    public async Task GetQrCode_WithValidId_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync(new Uri("/api/qrcodes/1", UriKind.Relative));

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await response.Content.ReadAsStringAsync();
        var qrCode = JsonSerializer.Deserialize<EasterEggHunt.Domain.Entities.QrCode>(content, _jsonOptions);

        Assert.That(qrCode, Is.Not.Null);
        Assert.That(qrCode!.Id, Is.EqualTo(1));
        Assert.That(qrCode.Title, Is.EqualTo("QR Code 1"));
    }

    [Test]
    public async Task GetQrCode_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync(new Uri("/api/qrcodes/999", UriKind.Relative));

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task CreateQrCode_WithValidData_ReturnsCreated()
    {
        // Arrange
        var request = new
        {
            CampaignId = 1,
            Title = "Test QR Code",
            Description = "Test Beschreibung",
            InternalNote = "Test Notiz"
        };
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync(new Uri("/api/qrcodes", UriKind.Relative), content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var responseContent = await response.Content.ReadAsStringAsync();
        var qrCode = JsonSerializer.Deserialize<EasterEggHunt.Domain.Entities.QrCode>(responseContent, _jsonOptions);

        Assert.That(qrCode, Is.Not.Null);
        Assert.That(qrCode!.Title, Is.EqualTo("Test QR Code"));
        Assert.That(qrCode.Description, Is.EqualTo("Test Beschreibung"));
        Assert.That(qrCode.InternalNotes, Is.EqualTo("Test Notiz"));
    }

    [Test]
    public async Task CreateQrCode_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var request = new
        {
            CampaignId = 0, // Ungültige CampaignId
            Title = "", // Leerer Titel
            Description = "Test Beschreibung",
            InternalNote = "Test Notiz"
        };
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync(new Uri("/api/qrcodes", UriKind.Relative), content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task UpdateQrCode_WithValidData_ReturnsNoContent()
    {
        // Arrange
        var request = new
        {
            Title = "Aktualisierter Titel",
            Description = "Aktualisierte Beschreibung",
            InternalNote = "Aktualisierte Notiz"
        };
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync(new Uri("/api/qrcodes/1", UriKind.Relative), content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // Verifiziere, dass der QR-Code aktualisiert wurde
        using var scope = _factory!.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EasterEggHuntDbContext>();
        var updatedQrCode = context.QrCodes.FirstOrDefault(q => q.Id == 1);
        Assert.That(updatedQrCode, Is.Not.Null);
        Assert.That(updatedQrCode!.Title, Is.EqualTo("Aktualisierter Titel"));
    }

    [Test]
    public async Task UpdateQrCode_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var request = new
        {
            Title = "Aktualisierter Titel",
            Description = "Aktualisierte Beschreibung",
            InternalNote = "Aktualisierte Notiz"
        };
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync(new Uri("/api/qrcodes/999", UriKind.Relative), content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task DeleteQrCode_WithValidId_ReturnsNoContent()
    {
        // Act
        var response = await _client.DeleteAsync(new Uri("/api/qrcodes/1", UriKind.Relative));

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // Verifiziere, dass der QR-Code gelöscht wurde
        using var scope = _factory!.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EasterEggHuntDbContext>();
        var deletedQrCode = context.QrCodes.FirstOrDefault(q => q.Id == 1);
        Assert.That(deletedQrCode, Is.Null);
    }

    [Test]
    public async Task DeleteQrCode_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync(new Uri("/api/qrcodes/999", UriKind.Relative));

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task ActivateQrCode_WithValidId_ReturnsNoContent()
    {
        // Act
        var response = await _client.PostAsync(new Uri("/api/qrcodes/2/activate", UriKind.Relative), null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // Verifiziere, dass der QR-Code aktiviert wurde
        using var scope = _factory!.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EasterEggHuntDbContext>();
        var activatedQrCode = context.QrCodes.FirstOrDefault(q => q.Id == 2);
        Assert.That(activatedQrCode, Is.Not.Null);
        Assert.That(activatedQrCode!.IsActive, Is.True);
    }

    [Test]
    public async Task ActivateQrCode_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.PostAsync(new Uri("/api/qrcodes/999/activate", UriKind.Relative), null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task DeactivateQrCode_WithValidId_ReturnsNoContent()
    {
        // Act
        var response = await _client.PostAsync(new Uri("/api/qrcodes/1/deactivate", UriKind.Relative), null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // Verifiziere, dass der QR-Code deaktiviert wurde
        using var scope = _factory!.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EasterEggHuntDbContext>();
        var deactivatedQrCode = context.QrCodes.FirstOrDefault(q => q.Id == 1);
        Assert.That(deactivatedQrCode, Is.Not.Null);
        Assert.That(deactivatedQrCode!.IsActive, Is.False);
    }

    [Test]
    public async Task DeactivateQrCode_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.PostAsync(new Uri("/api/qrcodes/999/deactivate", UriKind.Relative), null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task SetSortOrder_WithValidData_ReturnsNoContent()
    {
        // Arrange
        var request = new { SortOrder = 42 };
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync(new Uri("/api/qrcodes/1/sort-order", UriKind.Relative), content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // Verifiziere, dass die Sortierreihenfolge gesetzt wurde
        // Verwende einen neuen Scope, um die Änderungen zu sehen
        using var scope = _factory!.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EasterEggHuntDbContext>();
        var updatedQrCode = context.QrCodes.FirstOrDefault(q => q.Id == 1);
        Assert.That(updatedQrCode, Is.Not.Null);
        Assert.That(updatedQrCode!.SortOrder, Is.EqualTo(42));
    }

    [Test]
    public async Task SetSortOrder_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var request = new { SortOrder = 42 };
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync(new Uri("/api/qrcodes/999/sort-order", UriKind.Relative), content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            TearDown();
        }
    }
}
