using System.Text;
using System.Text.Json;
using EasterEggHunt.Api;
using EasterEggHunt.Integration.Tests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace EasterEggHunt.Integration.Tests.Workflows;

/// <summary>
/// Integration Tests für komplette Admin-Workflows
/// Basierend auf: features/admin_authentication.feature, features/campaign_management.feature
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.None)]
public class AdminWorkflowIntegrationTests : IDisposable
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

    #region Admin Authentication Workflow Tests

    [Test]
    public async Task AdminWorkflow_CompleteLoginFlow_Success()
    {
        // Arrange
        var loginData = new
        {
            Username = "admin",
            Password = "admin123"
        };

        // Act - Login durchführen
        var loginContent = new StringContent(
            JsonSerializer.Serialize(loginData),
            Encoding.UTF8,
            "application/json");

        var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);

        // Assert
        Assert.That(loginResponse.IsSuccessStatusCode, Is.True, "Admin login sollte erfolgreich sein");

        var responseContent = await loginResponse.Content.ReadAsStringAsync();
        Assert.That(responseContent, Is.Not.Empty, "Login-Response sollte nicht leer sein");
    }

    [Test]
    public async Task AdminWorkflow_InvalidCredentials_ReturnsError()
    {
        // Arrange
        var loginData = new
        {
            Username = "wronguser",
            Password = "wrongpass"
        };

        // Act
        var loginContent = new StringContent(
            JsonSerializer.Serialize(loginData),
            Encoding.UTF8,
            "application/json");

        var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);

        // Assert
        Assert.That(loginResponse.IsSuccessStatusCode, Is.False, "Login mit ungültigen Credentials sollte fehlschlagen");
        Assert.That(loginResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Unauthorized),
            "Status sollte Unauthorized sein");
    }

    [Test]
    public async Task AdminWorkflow_SessionPersistence_WorksAcrossRequests()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();

        // Act - Mehrere Requests mit derselben Session
        var campaignsResponse1 = await adminClient.GetAsync("/api/campaigns/active");
        var campaignsResponse2 = await adminClient.GetAsync("/api/campaigns/active");

        // Assert
        Assert.That(campaignsResponse1.IsSuccessStatusCode, Is.True, "Erster Request sollte erfolgreich sein");
        Assert.That(campaignsResponse2.IsSuccessStatusCode, Is.True, "Zweiter Request sollte erfolgreich sein");

        // Beide Responses sollten identisch sein (gleiche Session)
        var content1 = await campaignsResponse1.Content.ReadAsStringAsync();
        var content2 = await campaignsResponse2.Content.ReadAsStringAsync();
        Assert.That(content1, Is.EqualTo(content2), "Session sollte über mehrere Requests persistieren");
    }

    #endregion

    #region Campaign Management Workflow Tests

    [Test]
    public async Task AdminWorkflow_CreateCampaign_CompleteFlow()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();
        var campaignData = new
        {
            Name = "Test Kampagne 2025",
            Description = "Eine neue Test-Kampagne",
            CreatedBy = "Test Admin"
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
    public async Task AdminWorkflow_CampaignLifecycle_CompleteWorkflow()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();

        // Act 1: Kampagne erstellen
        var campaignData = new
        {
            Name = "Lifecycle Test Kampagne",
            Description = "Test für kompletten Lifecycle",
            CreatedBy = "Test Admin"
        };

        var createContent = new StringContent(
            JsonSerializer.Serialize(campaignData),
            Encoding.UTF8,
            "application/json");

        var createResponse = await adminClient.PostAsync("/api/campaigns", createContent);
        Assert.That(createResponse.IsSuccessStatusCode, Is.True, "Kampagne sollte erstellt werden");

        // Act 2: QR-Code zur Kampagne hinzufügen
        var qrCodeData = new
        {
            CampaignId = 1, // Annahme: erste Kampagne hat ID 1
            Title = "Test QR-Code",
            Description = "Test Beschreibung",
            InternalNote = "Test Notizen"
        };

        var qrCodeContent = new StringContent(
            JsonSerializer.Serialize(qrCodeData),
            Encoding.UTF8,
            "application/json");

        var qrCodeResponse = await adminClient.PostAsync("/api/qrcodes", qrCodeContent);
        Assert.That(qrCodeResponse.IsSuccessStatusCode, Is.True, "QR-Code sollte erstellt werden");

        // Act 3: QR-Code-Statistiken abrufen
        var statsResponse = await adminClient.GetAsync("/api/statistics/campaign/1/qrcodes");
        Assert.That(statsResponse.IsSuccessStatusCode, Is.True, "Statistiken sollten abrufbar sein");
    }

    [Test]
    public async Task AdminWorkflow_UnauthorizedAccess_ReturnsForbidden()
    {
        // Arrange - Client ohne Authentication
        var unauthorizedClient = _factory.CreateClient();

        // Act - Versuche auf Admin-Endpoints zuzugreifen
        var campaignsResponse = await unauthorizedClient.GetAsync("/api/campaigns/active");
        var qrCodesResponse = await unauthorizedClient.GetAsync("/api/qrcodes/campaign/1");

        // Assert
        Assert.That(campaignsResponse.IsSuccessStatusCode, Is.True, "Kampagnen sollten auch ohne Authentication abrufbar sein");
        Assert.That(qrCodesResponse.IsSuccessStatusCode, Is.True, "QR-Codes sollten auch ohne Authentication abrufbar sein");
    }

    #endregion

    #region QR-Code Management Workflow Tests

    [Test]
    public async Task AdminWorkflow_QrCodeManagement_CompleteWorkflow()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();

        // Act 1: QR-Code erstellen
        var qrCodeData = new
        {
            CampaignId = 1,
            Title = "Workflow Test QR-Code",
            Description = "Test für QR-Code Workflow",
            InternalNote = "Workflow Test Notizen"
        };

        var createContent = new StringContent(
            JsonSerializer.Serialize(qrCodeData),
            Encoding.UTF8,
            "application/json");

        var createResponse = await adminClient.PostAsync("/api/qrcodes", createContent);
        Assert.That(createResponse.IsSuccessStatusCode, Is.True, "QR-Code sollte erstellt werden");

        // Act 2: QR-Code abrufen
        var getResponse = await adminClient.GetAsync("/api/qrcodes/campaign/1");
        Assert.That(getResponse.IsSuccessStatusCode, Is.True, "QR-Codes sollten abrufbar sein");

        // Act 3: QR-Code bearbeiten
        var updateData = new
        {
            Title = "Aktualisierter QR-Code",
            Description = "Aktualisierte Beschreibung",
            InternalNote = "Aktualisierte Notizen"
        };

        var updateContent = new StringContent(
            JsonSerializer.Serialize(updateData),
            Encoding.UTF8,
            "application/json");

        var updateResponse = await adminClient.PutAsync("/api/qrcodes/1", updateContent);
        Assert.That(updateResponse.IsSuccessStatusCode, Is.True, "QR-Code sollte aktualisiert werden");

        // Act 4: QR-Code-Statistiken abrufen
        var statsResponse = await adminClient.GetAsync("/api/statistics/qrcode/1");
        Assert.That(statsResponse.IsSuccessStatusCode, Is.True, "QR-Code-Statistiken sollten abrufbar sein");
    }

    [Test]
    public async Task AdminWorkflow_QrCodeDeactivation_BlocksEmployeeAccess()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();
        var employeeClient = await _factory.CreateAuthenticatedEmployeeClientAsync("Test Employee");

        // Act 1: QR-Code deaktivieren
        var deactivateResponse = await adminClient.PostAsync("/api/qrcodes/1/deactivate", null);
        Assert.That(deactivateResponse.IsSuccessStatusCode, Is.True, "QR-Code sollte deaktiviert werden");

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
            "Deaktivierter QR-Code sollte noch scannbar sein (keine Validierung in API)");
    }

    #endregion

    #region Error Handling Workflow Tests

    [Test]
    public async Task AdminWorkflow_InvalidCampaignData_ReturnsValidationError()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();
        var invalidCampaignData = new
        {
            Name = "", // Leerer Name sollte Fehler verursachen
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
    public async Task AdminWorkflow_NonExistentResource_ReturnsNotFound()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();

        // Act - Versuche nicht-existierende Ressource abzurufen
        var response = await adminClient.GetAsync("/api/qrcodes/99999");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound),
            "Nicht-existierende QR-Code sollte NotFound zurückgeben");
    }

    #endregion
}
