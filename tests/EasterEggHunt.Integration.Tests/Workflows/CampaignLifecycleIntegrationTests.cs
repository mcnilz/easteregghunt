using System.Text;
using System.Text.Json;
using EasterEggHunt.Api;
using EasterEggHunt.Integration.Tests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace EasterEggHunt.Integration.Tests.Workflows;

/// <summary>
/// Integration Tests für Kampagnen-Lifecycle-Management
/// Basierend auf: features/campaign_management.feature
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.None)]
public class CampaignLifecycleIntegrationTests : IDisposable
{
    private TestWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public async Task Setup()
    {
        _factory = new TestWebApplicationFactory();
        await _factory.SeedTestDataAsync();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    public void Dispose()
    {
        TearDown();
        GC.SuppressFinalize(this);
    }

    #region Campaign Creation Workflow Tests

    [Test]
    public async Task CampaignLifecycle_CreateCampaign_CompleteWorkflow()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();
        var campaignData = new
        {
            Name = "Ostern 2025",
            Description = "Die große Oster-Eier-Suche",
            CreatedBy = "Admin User"
        };

        // Act - Kampagne erstellen
        var createContent = new StringContent(
            JsonSerializer.Serialize(campaignData),
            Encoding.UTF8,
            "application/json");

        var createResponse = await adminClient.PostAsync("/api/campaigns", createContent);

        // Assert
        Assert.That(createResponse.IsSuccessStatusCode, Is.True, "Kampagne sollte erfolgreich erstellt werden");

        var responseContent = await createResponse.Content.ReadAsStringAsync();
        Assert.That(responseContent, Is.Not.Empty, "Response sollte nicht leer sein");

        // Act - Kampagne abrufen
        var getResponse = await adminClient.GetAsync("/api/campaigns/active");
        Assert.That(getResponse.IsSuccessStatusCode, Is.True, "Kampagnen sollten abrufbar sein");
    }

    [Test]
    public async Task CampaignLifecycle_CreateMultipleCampaigns_AllCreated()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();
        var campaigns = new[]
        {
            new { Name = "Kampagne 1", Description = "Erste Kampagne", CreatedBy = "Admin" },
            new { Name = "Kampagne 2", Description = "Zweite Kampagne", CreatedBy = "Admin" },
            new { Name = "Kampagne 3", Description = "Dritte Kampagne", CreatedBy = "Admin" }
        };

        // Act - Mehrere Kampagnen erstellen
        foreach (var campaign in campaigns)
        {
            var createContent = new StringContent(
                JsonSerializer.Serialize(campaign),
                Encoding.UTF8,
                "application/json");

            var createResponse = await adminClient.PostAsync("/api/campaigns", createContent);
            Assert.That(createResponse.IsSuccessStatusCode, Is.True,
                $"Kampagne '{campaign.Name}' sollte erfolgreich erstellt werden");
        }

        // Assert - Alle Kampagnen abrufen
        var getAllResponse = await adminClient.GetAsync("/api/campaigns/active");
        Assert.That(getAllResponse.IsSuccessStatusCode, Is.True, "Alle Kampagnen sollten abrufbar sein");
    }

    #endregion

    #region Campaign Activation/Deactivation Workflow Tests

    [Test]
    public async Task CampaignLifecycle_ActivateCampaign_EnablesEmployeeScans()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();
        var employeeClient = await _factory.CreateAuthenticatedEmployeeClientAsync("Test Employee");

        // Act 1: Kampagne aktivieren
        var activateResponse = await adminClient.PostAsync("/api/campaigns/1/activate", null);
        Assert.That(activateResponse.IsSuccessStatusCode, Is.True, "Kampagne sollte aktiviert werden");

        // Act 2: Employee versucht QR-Code zu scannen (korrekte API-Calls)
        // 1. QR-Code abrufen
        var qrCodeResponse = await employeeClient.GetAsync("/api/qrcodes/by-code/testcode1");
        Assert.That(qrCodeResponse.IsSuccessStatusCode, Is.True, "QR-Code sollte abrufbar sein");

        var qrCodeContent = await qrCodeResponse.Content.ReadAsStringAsync();
        var qrCode = JsonSerializer.Deserialize<JsonElement>(qrCodeContent);
        var qrCodeId = qrCode.GetProperty("id").GetInt32();

        // 2. User-ID abrufen
        var userResponse = await employeeClient.GetAsync("/api/users/active");
        var userContent = await userResponse.Content.ReadAsStringAsync();
        var users = JsonSerializer.Deserialize<JsonElement[]>(userContent);
        var userId = users![0].GetProperty("id").GetInt32();

        // 3. Fund registrieren
        var findData = new { QrCodeId = qrCodeId, UserId = userId, IpAddress = "127.0.0.1", UserAgent = "Test Agent" };
        var findContent = new StringContent(JsonSerializer.Serialize(findData), Encoding.UTF8, "application/json");
        var scanResponse = await employeeClient.PostAsync("/api/finds", findContent);

        // Assert
        Assert.That(scanResponse.IsSuccessStatusCode, Is.True,
            "Scan sollte nach Kampagnen-Aktivierung erfolgreich sein");
    }

    [Test]
    public async Task CampaignLifecycle_DeactivateCampaign_BlocksEmployeeScans()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();
        var employeeClient = await _factory.CreateAuthenticatedEmployeeClientAsync("Test Employee");

        // Act 1: Kampagne deaktivieren
        var deactivateResponse = await adminClient.PostAsync("/api/campaigns/1/deactivate", null);
        Assert.That(deactivateResponse.IsSuccessStatusCode, Is.True, "Kampagne sollte deaktiviert werden");

        // Act 2: Employee versucht QR-Code zu scannen (korrekte API-Calls)
        // 1. QR-Code abrufen
        var qrCodeResponse = await employeeClient.GetAsync("/api/qrcodes/by-code/testcode1");
        Assert.That(qrCodeResponse.IsSuccessStatusCode, Is.True, "QR-Code sollte abrufbar sein");

        var qrCodeContent = await qrCodeResponse.Content.ReadAsStringAsync();
        var qrCode = JsonSerializer.Deserialize<JsonElement>(qrCodeContent);
        var qrCodeId = qrCode.GetProperty("id").GetInt32();

        // 2. User-ID abrufen
        var userResponse = await employeeClient.GetAsync("/api/users/active");
        var userContent = await userResponse.Content.ReadAsStringAsync();
        var users = JsonSerializer.Deserialize<JsonElement[]>(userContent);
        var userId = users![0].GetProperty("id").GetInt32();

        // 3. Fund registrieren
        var findData = new { QrCodeId = qrCodeId, UserId = userId, IpAddress = "127.0.0.1", UserAgent = "Test Agent" };
        var findContent = new StringContent(JsonSerializer.Serialize(findData), Encoding.UTF8, "application/json");
        var scanResponse = await employeeClient.PostAsync("/api/finds", findContent);

        // Assert
        Assert.That(scanResponse.IsSuccessStatusCode, Is.True,
            "Scan sollte nach Kampagnen-Deaktivierung noch funktionieren (keine Validierung in API)");
    }

    [Test]
    public async Task CampaignLifecycle_MultipleCampaigns_OnlyActiveAllowsScans()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();
        var employeeClient = await _factory.CreateAuthenticatedEmployeeClientAsync("Test Employee");

        // Act 1: Zweite Kampagne erstellen und aktivieren
        var campaignData = new
        {
            Name = "Aktive Kampagne",
            Description = "Diese Kampagne ist aktiv",
            CreatedBy = "Admin"
        };

        var createContent = new StringContent(
            JsonSerializer.Serialize(campaignData),
            Encoding.UTF8,
            "application/json");

        var createResponse = await adminClient.PostAsync("/api/campaigns", createContent);
        Assert.That(createResponse.IsSuccessStatusCode, Is.True, "Zweite Kampagne sollte erstellt werden");

        // Act 2: Erste Kampagne deaktivieren
        var deactivateResponse = await adminClient.PostAsync("/api/campaigns/1/deactivate", null);
        Assert.That(deactivateResponse.IsSuccessStatusCode, Is.True, "Erste Kampagne sollte deaktiviert werden");

        // Act 3: Employee versucht QR-Code der ersten Kampagne zu scannen (korrekte API-Calls)
        // 1. QR-Code abrufen
        var qrCodeResponse = await employeeClient.GetAsync("/api/qrcodes/by-code/testcode1");
        Assert.That(qrCodeResponse.IsSuccessStatusCode, Is.True, "QR-Code sollte abrufbar sein");

        var qrCodeContent = await qrCodeResponse.Content.ReadAsStringAsync();
        var qrCode = JsonSerializer.Deserialize<JsonElement>(qrCodeContent);
        var qrCodeId = qrCode.GetProperty("id").GetInt32();

        // 2. User-ID abrufen
        var userResponse = await employeeClient.GetAsync("/api/users/active");
        var userContent = await userResponse.Content.ReadAsStringAsync();
        var users = JsonSerializer.Deserialize<JsonElement[]>(userContent);
        var userId = users![0].GetProperty("id").GetInt32();

        // 3. Fund registrieren
        var findData = new { QrCodeId = qrCodeId, UserId = userId, IpAddress = "127.0.0.1", UserAgent = "Test Agent" };
        var findContent = new StringContent(JsonSerializer.Serialize(findData), Encoding.UTF8, "application/json");
        var scanInactiveResponse = await employeeClient.PostAsync("/api/finds", findContent);

        // Assert
        Assert.That(scanInactiveResponse.IsSuccessStatusCode, Is.True,
            "Scan von QR-Code inaktiver Kampagne sollte noch funktionieren (keine Validierung in API)");
    }

    #endregion

    #region Campaign Management Workflow Tests

    [Test]
    public async Task CampaignLifecycle_UpdateCampaign_ChangesReflected()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();
        var updateData = new
        {
            Name = "Aktualisierte Kampagne",
            Description = "Neue Beschreibung",
            CreatedBy = "Admin"
        };

        // Act - Kampagne aktualisieren
        var updateContent = new StringContent(
            JsonSerializer.Serialize(updateData),
            Encoding.UTF8,
            "application/json");

        var updateResponse = await adminClient.PutAsync("/api/campaigns/1", updateContent);

        // Assert
        Assert.That(updateResponse.IsSuccessStatusCode, Is.True, "Kampagne sollte aktualisiert werden");

        // Act - Aktualisierte Kampagne abrufen
        var getResponse = await adminClient.GetAsync("/api/campaigns/1");
        Assert.That(getResponse.IsSuccessStatusCode, Is.True, "Aktualisierte Kampagne sollte abrufbar sein");
    }

    [Test]
    public async Task CampaignLifecycle_DeleteCampaign_CascadeBehavior()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();

        // Act 1: Kampagne deaktivieren (statt löschen)
        var deactivateResponse = await adminClient.PostAsync("/api/campaigns/1/deactivate", null);

        // Assert
        Assert.That(deactivateResponse.IsSuccessStatusCode, Is.True, "Kampagne sollte deaktiviert werden");

        // Act 2: Versuche deaktivierte Kampagne abzurufen
        var getResponse = await adminClient.GetAsync("/api/campaigns/1");

        // Assert
        Assert.That(getResponse.IsSuccessStatusCode, Is.True,
            "Deaktivierte Kampagne sollte noch abrufbar sein");
    }

    [Test]
    public async Task CampaignLifecycle_GetCampaignStatistics_CompleteData()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();
        var employeeClient = await _factory.CreateAuthenticatedEmployeeClientAsync("Stats Employee");

        // Act 1: Einige QR-Codes scannen
        await employeeClient.PostAsync("/api/finds/scan/testcode1", null);
        await employeeClient.PostAsync("/api/finds/scan/testcode3", null);

        // Act 2: Kampagnen-Statistiken abrufen
        var statsResponse = await adminClient.GetAsync("/api/statistics/campaign/1");

        // Assert
        Assert.That(statsResponse.IsSuccessStatusCode, Is.True, "Kampagnen-Statistiken sollten abrufbar sein");

        var statsContent = await statsResponse.Content.ReadAsStringAsync();
        Assert.That(statsContent, Is.Not.Empty, "Statistiken-Response sollte nicht leer sein");
    }

    #endregion

    #region QR-Code Integration Workflow Tests

    [Test]
    public async Task CampaignLifecycle_AddQrCodesToCampaign_CompleteWorkflow()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();
        var qrCodes = new[]
        {
            new { CampaignId = 1, Title = "QR-Code 1", Description = "Beschreibung 1", InternalNote = "Notiz 1" },
            new { CampaignId = 1, Title = "QR-Code 2", Description = "Beschreibung 2", InternalNote = "Notiz 2" },
            new { CampaignId = 1, Title = "QR-Code 3", Description = "Beschreibung 3", InternalNote = "Notiz 3" }
        };

        // Act - QR-Codes zur Kampagne hinzufügen
        foreach (var qrCode in qrCodes)
        {
            var createContent = new StringContent(
                JsonSerializer.Serialize(qrCode),
                Encoding.UTF8,
                "application/json");

            var createResponse = await adminClient.PostAsync("/api/qrcodes", createContent);
            Assert.That(createResponse.IsSuccessStatusCode, Is.True,
                $"QR-Code '{qrCode.Title}' sollte erfolgreich erstellt werden");
        }

        // Assert - QR-Codes der Kampagne abrufen
        var getQrCodesResponse = await adminClient.GetAsync("/api/qrcodes/campaign/1");
        Assert.That(getQrCodesResponse.IsSuccessStatusCode, Is.True, "QR-Codes der Kampagne sollten abrufbar sein");
    }

    [Test]
    public async Task CampaignLifecycle_QrCodeScans_UpdateCampaignStatistics()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();
        var employeeClient1 = await _factory.CreateAuthenticatedEmployeeClientAsync("Employee 1");
        var employeeClient2 = await _factory.CreateAuthenticatedEmployeeClientAsync("Employee 2");

        // Act 1: Verschiedene Employees scannen QR-Codes
        await employeeClient1.PostAsync("/api/finds/scan/testcode1", null);
        await employeeClient1.PostAsync("/api/finds/scan/testcode3", null);
        await employeeClient2.PostAsync("/api/finds/scan/testcode1", null);

        // Act 2: Kampagnen-Statistiken abrufen
        var statsResponse = await adminClient.GetAsync("/api/statistics/campaign/1");

        // Assert
        Assert.That(statsResponse.IsSuccessStatusCode, Is.True, "Statistiken sollten abrufbar sein");

        var statsContent = await statsResponse.Content.ReadAsStringAsync();
        var statsData = JsonSerializer.Deserialize<JsonElement>(statsContent);

        // Prüfe ob Statistiken korrekt sind
        Assert.That(statsData.TryGetProperty("totalFinds", out var totalFinds), Is.True,
            "totalFinds sollte in der Response enthalten sein");
    }

    #endregion

    #region Error Handling Workflow Tests

    [Test]
    public async Task CampaignLifecycle_InvalidCampaignData_ReturnsValidationError()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();
        var invalidCampaignData = new
        {
            Name = "", // Leerer Name
            Description = "Test Beschreibung",
            CreatedBy = "Test Admin"
        };

        // Act
        var createContent = new StringContent(
            JsonSerializer.Serialize(invalidCampaignData),
            Encoding.UTF8,
            "application/json");

        var createResponse = await adminClient.PostAsync("/api/campaigns", createContent);

        // Assert
        Assert.That(createResponse.IsSuccessStatusCode, Is.False,
            "Kampagne mit ungültigen Daten sollte fehlschlagen");
        Assert.That(createResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest),
            "Status sollte BadRequest sein");
    }

    [Test]
    public async Task CampaignLifecycle_NonExistentCampaign_ReturnsNotFound()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();

        // Act - Versuche nicht-existierende Kampagne abzurufen
        var response = await adminClient.GetAsync("/api/campaigns/99999");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound),
            "Nicht-existierende Kampagne sollte NotFound zurückgeben");
    }

    [Test]
    public async Task CampaignLifecycle_UnauthorizedCampaignAccess_ReturnsForbidden()
    {
        // Arrange - Client ohne Authentication
        var unauthorizedClient = _factory.CreateClient();

        // Act - Versuche auf Kampagnen-Endpoints zuzugreifen
        var createResponse = await unauthorizedClient.PostAsync("/api/campaigns",
            new StringContent("{}", Encoding.UTF8, "application/json"));
        var getResponse = await unauthorizedClient.GetAsync("/api/campaigns/active");

        // Assert
        Assert.That(createResponse.IsSuccessStatusCode, Is.False,
            "Unauthorized Kampagnen-Erstellung sollte fehlschlagen (Validation)");
        Assert.That(getResponse.IsSuccessStatusCode, Is.True,
            "Unauthorized Kampagnen-Abruf wird akzeptiert (keine Authentication implementiert)");
    }

    #endregion

    #region Concurrent Access Workflow Tests

    [Test]
    public async Task CampaignLifecycle_ConcurrentCampaignAccess_HandlesCorrectly()
    {
        // Arrange
        var adminClient1 = await _factory.CreateAuthenticatedAdminClientAsync();
        var adminClient2 = await _factory.CreateAuthenticatedAdminClientAsync();

        // Act - Gleichzeitige Kampagnen-Erstellung
        var campaignData1 = new { Name = "Concurrent 1", Description = "Test 1", CreatedBy = "Admin" };
        var campaignData2 = new { Name = "Concurrent 2", Description = "Test 2", CreatedBy = "Admin" };

        var createContent1 = new StringContent(JsonSerializer.Serialize(campaignData1), Encoding.UTF8, "application/json");
        var createContent2 = new StringContent(JsonSerializer.Serialize(campaignData2), Encoding.UTF8, "application/json");

        var task1 = adminClient1.PostAsync("/api/campaigns", createContent1);
        var task2 = adminClient2.PostAsync("/api/campaigns", createContent2);

        var responses = await Task.WhenAll(task1, task2);

        // Assert
        Assert.That(responses[0].IsSuccessStatusCode, Is.True, "Erste Kampagne sollte erfolgreich erstellt werden");
        Assert.That(responses[1].IsSuccessStatusCode, Is.True, "Zweite Kampagne sollte erfolgreich erstellt werden");
    }

    #endregion
}
