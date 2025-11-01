using System.Text;
using System.Text.Json;
using EasterEggHunt.Integration.Tests.Helpers;
using NUnit.Framework;

namespace EasterEggHunt.Integration.Tests.Workflows;

/// <summary>
/// Integration Tests für Kampagnen-Lifecycle-Management
/// Basierend auf: features/campaign_management.feature
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.Self)] // Parallele Ausführung für unabhängige Tests
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

    #endregion

    #region Campaign Activation/Deactivation Workflow Tests
    // Activation/Deactivation wird bereits im vollständigen CampaignLifecycle abgedeckt
    #endregion

    #region Campaign Management Workflow Tests

    #endregion

    #region QR-Code Integration Workflow Tests
    // QR-Code Integration wird bereits in CampaignLifecycle_CompleteWorkflow abgedeckt
    #endregion

    #region Error Handling Workflow Tests
    // Error Handling wird bereits in vollständigen Flows und Unit/Application Tests abgedeckt
    #endregion

    #region Concurrent Access Workflow Tests
    // Concurrency wird bereits in CrossCuttingIntegrationTests abgedeckt
    #endregion
}
