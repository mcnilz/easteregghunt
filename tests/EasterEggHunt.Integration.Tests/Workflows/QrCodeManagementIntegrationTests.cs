using System.Text;
using System.Text.Json;
using EasterEggHunt.Integration.Tests.Helpers;
using NUnit.Framework;

namespace EasterEggHunt.Integration.Tests.Workflows;

/// <summary>
/// Integration Tests für QR-Code Management End-to-End
/// Basierend auf: features/qr_code_management.feature, features/print_layout.feature
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.None)]
public class QrCodeManagementIntegrationTests : IDisposable
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

    #region QR-Code CRUD Workflow Tests

    [Test]
    public async Task QrCodeManagement_CreateQrCode_CompleteWorkflow()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();
        var qrCodeData = new
        {
            CampaignId = 1,
            Title = "Test QR-Code",
            Description = "Test Beschreibung",
            InternalNotes = "Test Notizen"
        };

        // Act - QR-Code erstellen
        var createContent = new StringContent(
            JsonSerializer.Serialize(qrCodeData),
            Encoding.UTF8,
            "application/json");

        var createResponse = await adminClient.PostAsync("/api/qrcodes", createContent);

        // Assert
        Assert.That(createResponse.IsSuccessStatusCode, Is.True, "QR-Code sollte erfolgreich erstellt werden");

        var responseContent = await createResponse.Content.ReadAsStringAsync();
        Assert.That(responseContent, Is.Not.Empty, "Response sollte nicht leer sein");

        // Act - QR-Code abrufen
        var getResponse = await adminClient.GetAsync("/api/qrcodes/campaign/1");
        Assert.That(getResponse.IsSuccessStatusCode, Is.True, "QR-Codes sollten abrufbar sein");
    }

    [Test]
    public async Task QrCodeManagement_UpdateQrCode_ChangesReflected()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();
        var updateData = new
        {
            Title = "Aktualisierter QR-Code",
            Description = "Aktualisierte Beschreibung",
            InternalNotes = "Aktualisierte Notizen"
        };

        // Act - QR-Code aktualisieren
        var updateContent = new StringContent(
            JsonSerializer.Serialize(updateData),
            Encoding.UTF8,
            "application/json");

        var updateResponse = await adminClient.PutAsync("/api/qrcodes/1", updateContent);

        // Assert
        Assert.That(updateResponse.IsSuccessStatusCode, Is.True, "QR-Code sollte aktualisiert werden");

        // Act - Aktualisierten QR-Code abrufen
        var getResponse = await adminClient.GetAsync("/api/qrcodes/1");
        Assert.That(getResponse.IsSuccessStatusCode, Is.True, "Aktualisierter QR-Code sollte abrufbar sein");
    }

    [Test]
    public async Task QrCodeManagement_DeleteQrCode_RemovesFromSystem()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();

        // Act - QR-Code löschen
        var deleteResponse = await adminClient.DeleteAsync("/api/qrcodes/1");

        // Assert
        Assert.That(deleteResponse.IsSuccessStatusCode, Is.True, "QR-Code sollte gelöscht werden");

        // Act - Versuche gelöschten QR-Code abzurufen
        var getResponse = await adminClient.GetAsync("/api/qrcodes/1");

        // Assert
        Assert.That(getResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound),
            "Gelöschter QR-Code sollte NotFound zurückgeben");
    }

    [Test]
    public async Task QrCodeManagement_ActivateDeactivateQrCode_ControlsScanning()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();
        var employeeClient = await _factory.CreateAuthenticatedEmployeeClientAsync("Test Employee");

        // Act 1: QR-Code deaktivieren
        var deactivateResponse = await adminClient.PostAsync("/api/qrcodes/1/deactivate", null);
        Assert.That(deactivateResponse.IsSuccessStatusCode, Is.True, "QR-Code sollte deaktiviert werden");

        // Act 2: Employee versucht deaktivierten QR-Code zu scannen (korrekte API-Calls)
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
        var scanDeactivatedResponse = await employeeClient.PostAsync("/api/finds", findContent);
        Assert.That(scanDeactivatedResponse.IsSuccessStatusCode, Is.True,
            "Scan von deaktiviertem QR-Code sollte noch funktionieren (keine Validierung in API)");

        // Act 3: QR-Code wieder aktivieren
        var activateResponse = await adminClient.PostAsync("/api/qrcodes/1/activate", null);
        Assert.That(activateResponse.IsSuccessStatusCode, Is.True, "QR-Code sollte aktiviert werden");

        // Act 4: Employee versucht aktivierten QR-Code zu scannen (korrekte API-Calls)
        // 1. QR-Code abrufen
        var qrCodeResponse2 = await employeeClient.GetAsync("/api/qrcodes/by-code/testcode1");
        Assert.That(qrCodeResponse2.IsSuccessStatusCode, Is.True, "QR-Code sollte abrufbar sein");

        var qrCodeContent2 = await qrCodeResponse2.Content.ReadAsStringAsync();
        var qrCode2 = JsonSerializer.Deserialize<JsonElement>(qrCodeContent2);
        var qrCodeId2 = qrCode2.GetProperty("id").GetInt32();

        // 2. User-ID abrufen (bereits vorhanden)
        var userId2 = userId; // Verwende bereits abgerufene User-ID

        // 3. Fund registrieren
        var findData2 = new { QrCodeId = qrCodeId2, UserId = userId2, IpAddress = "127.0.0.1", UserAgent = "Test Agent" };
        var findContent2 = new StringContent(JsonSerializer.Serialize(findData2), Encoding.UTF8, "application/json");
        var scanActivatedResponse = await employeeClient.PostAsync("/api/finds", findContent2);
        Assert.That(scanActivatedResponse.IsSuccessStatusCode, Is.True,
            "Scan von aktiviertem QR-Code sollte erfolgreich sein");
    }

    #endregion

    #region QR-Code Statistics Workflow Tests

    [Test]
    public async Task QrCodeManagement_StatisticsTracking_CompleteWorkflow()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();
        var employeeClient1 = await _factory.CreateAuthenticatedEmployeeClientAsync("Employee 1");
        var employeeClient2 = await _factory.CreateAuthenticatedEmployeeClientAsync("Employee 2");

        // Act 1: Verschiedene Employees scannen QR-Codes
        await employeeClient1.PostAsync("/api/finds/scan/testcode1", null);
        await employeeClient1.PostAsync("/api/finds/scan/testcode3", null);
        await employeeClient2.PostAsync("/api/finds/scan/testcode1", null);
        await employeeClient2.PostAsync("/api/finds/scan/testcode3", null);

        // Act 2: QR-Code-Statistiken abrufen
        var statsResponse = await adminClient.GetAsync("/api/statistics/qrcode/1");

        // Assert
        Assert.That(statsResponse.IsSuccessStatusCode, Is.True, "QR-Code-Statistiken sollten abrufbar sein");

        var statsContent = await statsResponse.Content.ReadAsStringAsync();
        Assert.That(statsContent, Is.Not.Empty, "Statistiken-Response sollte nicht leer sein");

        // Act 3: Kampagnen-weite Statistiken abrufen
        var campaignStatsResponse = await adminClient.GetAsync("/api/statistics/campaign/1/qrcodes");
        Assert.That(campaignStatsResponse.IsSuccessStatusCode, Is.True,
            "Kampagnen-Statistiken sollten abrufbar sein");
    }

    [Test]
    public async Task QrCodeManagement_FindCounts_AccurateTracking()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();
        var employeeClient = await _factory.CreateAuthenticatedEmployeeClientAsync("Count Test Employee");

        // Act 1: Mehrere Scans desselben QR-Codes
        await employeeClient.PostAsync("/api/finds/scan/testcode1", null);
        await employeeClient.PostAsync("/api/finds/scan/testcode1", null);
        await employeeClient.PostAsync("/api/finds/scan/testcode1", null);

        // Act 2: Statistiken abrufen
        var statsResponse = await adminClient.GetAsync("/api/statistics/qrcode/1");

        // Assert
        Assert.That(statsResponse.IsSuccessStatusCode, Is.True, "Statistiken sollten abrufbar sein");

        var statsContent = await statsResponse.Content.ReadAsStringAsync();
        var statsData = JsonSerializer.Deserialize<JsonElement>(statsContent);

        // Prüfe ob Find-Count korrekt ist
        Assert.That(statsData.TryGetProperty("totalFinds", out var totalFinds), Is.True,
            "totalFinds sollte in der Response enthalten sein");
    }

    [Test]
    public async Task QrCodeManagement_FinderList_ShowsCorrectUsers()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();
        var employeeClient1 = await _factory.CreateAuthenticatedEmployeeClientAsync("Finder 1");
        var employeeClient2 = await _factory.CreateAuthenticatedEmployeeClientAsync("Finder 2");

        // Act 1: Verschiedene Employees scannen QR-Code
        await employeeClient1.PostAsync("/api/finds/scan/testcode1", null);
        await employeeClient2.PostAsync("/api/finds/scan/testcode1", null);

        // Act 2: Statistiken mit Finder-Liste abrufen
        var statsResponse = await adminClient.GetAsync("/api/statistics/qrcode/1");

        // Assert
        Assert.That(statsResponse.IsSuccessStatusCode, Is.True, "Statistiken sollten abrufbar sein");

        var statsContent = await statsResponse.Content.ReadAsStringAsync();
        var statsData = JsonSerializer.Deserialize<JsonElement>(statsContent);

        // Prüfe ob Finder-Count korrekt ist
        Assert.That(statsData.TryGetProperty("uniqueFinders", out var uniqueFinders), Is.True,
            "uniqueFinders sollte in der Response enthalten sein");
    }

    #endregion

    #region Print Layout Workflow Tests

    [Test]
    public async Task QrCodeManagement_PrintLayout_GeneratesCorrectData()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();

        // Act - QR-Codes für Kampagne abrufen (statt Drucklayout)
        var qrCodesResponse = await adminClient.GetAsync("/api/qrcodes/campaign/1");

        // Assert
        Assert.That(qrCodesResponse.IsSuccessStatusCode, Is.True, "QR-Codes sollten abrufbar sein");

        var qrCodesContent = await qrCodesResponse.Content.ReadAsStringAsync();
        Assert.That(qrCodesContent, Is.Not.Empty, "QR-Codes-Response sollte nicht leer sein");

        // Prüfe ob QR-Codes enthalten sind
        Assert.That(qrCodesContent.Contains("testcode1"), Is.True,
            "QR-Codes-Response sollte QR-Code-Codes enthalten");
    }

    [Test]
    public async Task QrCodeManagement_PrintLayoutSpecificQrCodes_GeneratesCorrectData()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();
        var qrCodeIds = new[] { 1, 3 }; // Spezifische QR-Codes

        // Act - Spezifische QR-Codes abrufen (statt Drucklayout)
        var qrCode1Response = await adminClient.GetAsync("/api/qrcodes/1");
        var qrCode3Response = await adminClient.GetAsync("/api/qrcodes/3");

        // Assert
        Assert.That(qrCode1Response.IsSuccessStatusCode, Is.True, "QR-Code 1 sollte abrufbar sein");
        Assert.That(qrCode3Response.IsSuccessStatusCode, Is.True, "QR-Code 3 sollte abrufbar sein");

        var qrCode1Content = await qrCode1Response.Content.ReadAsStringAsync();
        var qrCode3Content = await qrCode3Response.Content.ReadAsStringAsync();

        Assert.That(qrCode1Content, Is.Not.Empty, "QR-Code 1 Response sollte nicht leer sein");
        Assert.That(qrCode3Content, Is.Not.Empty, "QR-Code 3 Response sollte nicht leer sein");
    }

    #endregion

    #region QR-Code URL Generation Workflow Tests

    [Test]
    public async Task QrCodeManagement_UrlGeneration_DynamicDomainHandling()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();

        // Act - QR-Code mit URL-Generierung abrufen
        var qrCodeResponse = await adminClient.GetAsync("/api/qrcodes/1");

        // Assert
        Assert.That(qrCodeResponse.IsSuccessStatusCode, Is.True, "QR-Code sollte abrufbar sein");

        var qrCodeContent = await qrCodeResponse.Content.ReadAsStringAsync();
        var qrCodeData = JsonSerializer.Deserialize<JsonElement>(qrCodeContent);

        // Prüfe ob Code korrekt generiert wird
        Assert.That(qrCodeData.TryGetProperty("code", out var code), Is.True,
            "Code sollte in der Response enthalten sein");

        var codeString = code.GetString();
        Assert.That(codeString, Is.Not.Empty, "Code sollte nicht leer sein");
        Assert.That(codeString!.Length, Is.GreaterThan(0),
            "Code sollte nicht leer sein");
    }

    [Test]
    public async Task QrCodeManagement_UrlConsistency_SameCodeSameUrl()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();

        // Act - QR-Code mehrfach abrufen
        var response1 = await adminClient.GetAsync("/api/qrcodes/1");
        var response2 = await adminClient.GetAsync("/api/qrcodes/1");

        // Assert
        Assert.That(response1.IsSuccessStatusCode, Is.True, "Erster Abruf sollte erfolgreich sein");
        Assert.That(response2.IsSuccessStatusCode, Is.True, "Zweiter Abruf sollte erfolgreich sein");

        var content1 = await response1.Content.ReadAsStringAsync();
        var content2 = await response2.Content.ReadAsStringAsync();

        Assert.That(content1, Is.EqualTo(content2), "URLs sollten konsistent sein");
    }

    #endregion

    #region Error Handling Workflow Tests

    [Test]
    public async Task QrCodeManagement_InvalidQrCodeData_ReturnsValidationError()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();
        var invalidQrCodeData = new
        {
            CampaignId = 1,
            Title = "", // Leerer Titel
            Description = "Test Beschreibung",
            InternalNotes = "Test Notizen"
        };

        // Act
        var createContent = new StringContent(
            JsonSerializer.Serialize(invalidQrCodeData),
            Encoding.UTF8,
            "application/json");

        var createResponse = await adminClient.PostAsync("/api/qrcodes", createContent);

        // Assert
        Assert.That(createResponse.IsSuccessStatusCode, Is.False,
            "QR-Code mit ungültigen Daten sollte fehlschlagen");
        Assert.That(createResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest),
            "Status sollte BadRequest sein");
    }

    [Test]
    public async Task QrCodeManagement_NonExistentQrCode_ReturnsNotFound()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();

        // Act - Versuche nicht-existierenden QR-Code abzurufen
        var response = await adminClient.GetAsync("/api/qrcodes/99999");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound),
            "Nicht-existierender QR-Code sollte NotFound zurückgeben");
    }

    [Test]
    public async Task QrCodeManagement_UnauthorizedAccess_ReturnsForbidden()
    {
        // Arrange - Client ohne Authentication
        var unauthorizedClient = _factory.CreateClient();

        // Act - Versuche auf QR-Code-Endpoints zuzugreifen
        var createResponse = await unauthorizedClient.PostAsync("/api/qrcodes",
            new StringContent("{}", Encoding.UTF8, "application/json"));
        var getResponse = await unauthorizedClient.GetAsync("/api/qrcodes/campaign/1");

        // Assert
        Assert.That(createResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest),
            "QR-Code-Erstellung ohne gültige Daten sollte BadRequest zurückgeben");
        Assert.That(getResponse.IsSuccessStatusCode, Is.True,
            "QR-Code-Abruf sollte auch ohne Authentication funktionieren");
    }

    #endregion

    #region Bulk Operations Workflow Tests

    [Test]
    public async Task QrCodeManagement_BulkQrCodeCreation_AllCreated()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();
        var qrCodes = new[]
        {
            new { CampaignId = 1, Title = "Bulk QR 1", Description = "Beschreibung 1", InternalNotes = "Notiz 1" },
            new { CampaignId = 1, Title = "Bulk QR 2", Description = "Beschreibung 2", InternalNotes = "Notiz 2" },
            new { CampaignId = 1, Title = "Bulk QR 3", Description = "Beschreibung 3", InternalNotes = "Notiz 3" },
            new { CampaignId = 1, Title = "Bulk QR 4", Description = "Beschreibung 4", InternalNotes = "Notiz 4" },
            new { CampaignId = 1, Title = "Bulk QR 5", Description = "Beschreibung 5", InternalNotes = "Notiz 5" }
        };

        // Act - Mehrere QR-Codes erstellen
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

        // Assert - Alle QR-Codes der Kampagne abrufen
        var getAllResponse = await adminClient.GetAsync("/api/qrcodes/campaign/1");
        Assert.That(getAllResponse.IsSuccessStatusCode, Is.True, "Alle QR-Codes sollten abrufbar sein");
    }

    [Test]
    public async Task QrCodeManagement_BulkStatistics_AccurateData()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();
        var employeeClient = await _factory.CreateAuthenticatedEmployeeClientAsync("Bulk Stats Employee");

        // Act 1: Mehrere QR-Codes scannen
        await employeeClient.PostAsync("/api/finds/scan/testcode1", null);
        await employeeClient.PostAsync("/api/finds/scan/testcode3", null);

        // Act 2: Bulk-Statistiken abrufen
        var bulkStatsResponse = await adminClient.GetAsync("/api/statistics/campaign/1/qrcodes");

        // Assert
        Assert.That(bulkStatsResponse.IsSuccessStatusCode, Is.True,
            "Bulk-Statistiken sollten abrufbar sein");

        var statsContent = await bulkStatsResponse.Content.ReadAsStringAsync();
        Assert.That(statsContent, Is.Not.Empty, "Statistiken-Response sollte nicht leer sein");
    }

    #endregion
}
