using System.Text;
using System.Text.Json;
using EasterEggHunt.Integration.Tests.Helpers;
using NUnit.Framework;

namespace EasterEggHunt.Integration.Tests.Workflows;

/// <summary>
/// Integration Tests für komplette Admin-Workflows
/// Basierend auf: features/admin_authentication.feature, features/campaign_management.feature
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.Self)] // Parallele Ausführung für unabhängige Tests
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

    #endregion

    #region Campaign Management Workflow Tests

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
            InternalNotes = "Test Notizen"
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

    #endregion

    #region QR-Code Management Workflow Tests
    // QR-Code Management wird bereits in CampaignLifecycle_CompleteWorkflow abgedeckt
    #endregion

    #region Error Handling Workflow Tests
    // Error Handling wird bereits in vollständigen Flows und Unit/Application Tests abgedeckt
    #endregion
}
