using EasterEggHunt.Domain.Entities;
using EasterEggHunt.Infrastructure.Data;
using EasterEggHunt.Integration.Tests.Helpers;
using EasterEggHunterApi.Abstractions.Models.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace EasterEggHunt.Integration.Tests.Workflows;

/// <summary>
/// Integration Tests für GDPR-Compliance
/// Testet die API-Endpoints für Datenlöschung und Anonymisierung
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.Self)]
public sealed class GdprComplianceIntegrationTests : IDisposable
{
    private TestWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;
    private EasterEggHuntDbContext _context = null!;

    [SetUp]
    public async Task Setup()
    {
        _factory = new TestWebApplicationFactory();
        await _factory.SeedTestDataAsync();
        _client = _factory.CreateClient();

        // DbContext für direkte Datenbank-Zugriffe
        var scope = _factory.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<EasterEggHuntDbContext>();
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
        _context?.Dispose();
        _factory?.Dispose();
    }

    public void Dispose()
    {
        TearDown();
        GC.SuppressFinalize(this);
    }

    [Test]
    public async Task DeleteUserData_ShouldDeleteAllSessions()
    {
        // Arrange
        var user = new User("Test User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Erstelle mehrere Sessions
        var session1 = new Session(user.Id, 30);
        var session2 = new Session(user.Id, 30);
        var session3 = new Session(user.Id, 30);

        await _context.Sessions.AddRangeAsync(session1, session2, session3);
        await _context.SaveChangesAsync();

        var request = new GdprDeleteRequest
        {
            UserId = user.Id,
            DeleteFinds = false
        };

        using var content = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(request),
            System.Text.Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/users/gdpr/delete", content);

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True, "GDPR-Löschung sollte erfolgreich sein");

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = System.Text.Json.JsonSerializer.Deserialize<GdprDeleteResponse>(
            responseContent,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.DeletedSessions, Is.EqualTo(3), "Alle 3 Sessions sollten gelöscht sein");
        Assert.That(result.UserDeleted, Is.True, "Benutzer sollte gelöscht sein");

        // Prüfe, dass Sessions tatsächlich gelöscht wurden
        var remainingSessions = await _context.Sessions
            .Where(s => s.UserId == user.Id)
            .ToListAsync();
        Assert.That(remainingSessions, Is.Empty, "Keine Sessions sollten noch vorhanden sein");
    }

    [Test]
    public async Task DeleteUserData_WithDeleteFindsTrue_ShouldDeleteFindsAndSessions()
    {
        // Arrange
        var campaign = new Campaign("Test Campaign", "Description", "Admin");
        var user = new User("Test User");
        await _context.Campaigns.AddAsync(campaign);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var qrCode = new QrCode(campaign.Id, "Test QR", "Description", "Notes");
        await _context.QrCodes.AddAsync(qrCode);
        await _context.SaveChangesAsync();

        var find1 = new Find(qrCode.Id, user.Id, "127.0.0.1", "User Agent 1");
        var find2 = new Find(qrCode.Id, user.Id, "127.0.0.1", "User Agent 2");
        var session = new Session(user.Id, 30);

        await _context.Finds.AddRangeAsync(find1, find2);
        await _context.Sessions.AddAsync(session);
        await _context.SaveChangesAsync();

        var request = new GdprDeleteRequest
        {
            UserId = user.Id,
            DeleteFinds = true
        };

        using var content = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(request),
            System.Text.Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/users/gdpr/delete", content);

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True, "GDPR-Löschung sollte erfolgreich sein");

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = System.Text.Json.JsonSerializer.Deserialize<GdprDeleteResponse>(
            responseContent,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.DeletedSessions, Is.EqualTo(1));
        Assert.That(result.DeletedFinds, Is.EqualTo(2), "Beide Funde sollten gelöscht sein");
        Assert.That(result.UserDeleted, Is.True);

        // Prüfe, dass alles gelöscht wurde
        var remainingFinds = await _context.Finds
            .Where(f => f.UserId == user.Id)
            .ToListAsync();
        Assert.That(remainingFinds, Is.Empty, "Keine Funde sollten noch vorhanden sein");
    }

    [Test]
    public async Task DeleteUserData_WithNonExistingUser_ShouldReturnZero()
    {
        // Arrange
        var request = new GdprDeleteRequest
        {
            UserId = 99999,
            DeleteFinds = false
        };

        using var content = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(request),
            System.Text.Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/users/gdpr/delete", content);

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True);

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = System.Text.Json.JsonSerializer.Deserialize<GdprDeleteResponse>(
            responseContent,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.TotalDeleted, Is.EqualTo(0), "Keine Daten sollten gelöscht werden wenn User nicht existiert");
    }

    [Test]
    public async Task AnonymizeUserData_ShouldAnonymizeUserName()
    {
        // Arrange
        var user = new User("Test User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var session = new Session(user.Id, 30);
        await _context.Sessions.AddAsync(session);
        await _context.SaveChangesAsync();

        var originalName = user.Name;

        // Act
        var response = await _client.PostAsync($"/api/users/{user.Id}/gdpr/anonymize", null);

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True, "Anonymisierung sollte erfolgreich sein");

        // Prüfe, dass Benutzername anonymisiert wurde
        await _context.Entry(user).ReloadAsync();
        Assert.That(user.Name, Does.Contain("Anonymized_User_"));
        Assert.That(user.Name, Is.Not.EqualTo(originalName), "Benutzername sollte anonymisiert sein");

        // Prüfe, dass Sessions gelöscht wurden
        var remainingSessions = await _context.Sessions
            .Where(s => s.UserId == user.Id)
            .ToListAsync();
        Assert.That(remainingSessions, Is.Empty, "Sessions sollten gelöscht sein");
    }

    [Test]
    public async Task AnonymizeUserData_WithNonExistingUser_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.PostAsync("/api/users/99999/gdpr/anonymize", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound));
    }
}

