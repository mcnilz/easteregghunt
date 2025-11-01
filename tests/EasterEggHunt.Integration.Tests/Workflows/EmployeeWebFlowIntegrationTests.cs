using System.Text;
using System.Text.Json;
using EasterEggHunt.Integration.Tests.Helpers;
using EasterEggHunterApi.Abstractions.Models.User;
using NUnit.Framework;

namespace EasterEggHunt.Integration.Tests.Workflows;

/// <summary>
/// Integration Tests für den vollständigen Employee Web-Controller Flow
/// Testet die gefixten Endpunkte:
/// - /api/users/check-name/{name} (CheckUserNameExistsAsync)
/// - /api/users (POST für Registrierung)
/// - /api/finds/check (GetExistingFindAsync)
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.Self)] // Parallele Ausführung für unabhängige Tests
public class EmployeeWebFlowIntegrationTests : IDisposable
{
    private TestWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;
    private JsonSerializerOptions _jsonOptions = null!;

    [SetUp]
    public async Task Setup()
    {
        _factory = new TestWebApplicationFactory();
        await _factory.SeedTestDataAsync();
        _client = _factory.CreateClient();

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
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

    #region Complete Flow Tests

    [Test]
    public async Task CompleteEmployeeWebFlow_RegistrationAndScan_Success()
    {
        // Arrange
        var userName = "CompleteFlowTestUser";

        // Step 1: Prüfen ob Name existiert (sollte false sein)
        var checkRequest1 = new { Name = userName };
        var checkContent1 = new StringContent(
            JsonSerializer.Serialize(checkRequest1),
            Encoding.UTF8,
            "application/json");
        var checkResponse1 = await _client.PostAsync("/api/users/check-name", checkContent1);
        Assert.That(checkResponse1.IsSuccessStatusCode, Is.True, "Check sollte erfolgreich sein");
        var checkResponseContent1 = await checkResponse1.Content.ReadAsStringAsync();
        var checkResult1 = JsonSerializer.Deserialize<CheckUserNameResponse>(checkResponseContent1, _jsonOptions);
        Assert.That(checkResult1!.Exists, Is.False, "Name sollte nicht existieren");

        // Step 2: User registrieren
        var registrationData = new { Name = userName };
        var registrationContent = new StringContent(
            JsonSerializer.Serialize(registrationData),
            Encoding.UTF8,
            "application/json");

        var registrationResponse = await _client.PostAsync("/api/users", registrationContent);
        Assert.That(registrationResponse.IsSuccessStatusCode, Is.True, "Registrierung sollte erfolgreich sein");

        var userContent = await registrationResponse.Content.ReadAsStringAsync();
        var user = JsonSerializer.Deserialize<JsonElement>(userContent, _jsonOptions);
        var userId = user.GetProperty("id").GetInt32();

        // Step 3: Prüfen ob Name jetzt existiert (sollte true sein)
        var checkRequest2 = new { Name = userName };
        var checkContent2 = new StringContent(
            JsonSerializer.Serialize(checkRequest2),
            Encoding.UTF8,
            "application/json");
        var checkResponse2 = await _client.PostAsync("/api/users/check-name", checkContent2);
        Assert.That(checkResponse2.IsSuccessStatusCode, Is.True, "Check sollte erfolgreich sein");
        var checkResponseContent2 = await checkResponse2.Content.ReadAsStringAsync();
        var checkResult2 = JsonSerializer.Deserialize<CheckUserNameResponse>(checkResponseContent2, _jsonOptions);
        Assert.That(checkResult2!.Exists, Is.True, "Name sollte jetzt existieren");

        // Step 4: QR-Code abrufen
        var qrCodeResponse = await _client.GetAsync("/api/qrcodes/by-code/testcode1");
        Assert.That(qrCodeResponse.IsSuccessStatusCode, Is.True, "QR-Code sollte abrufbar sein");

        var qrCodeContent = await qrCodeResponse.Content.ReadAsStringAsync();
        var qrCode = JsonSerializer.Deserialize<JsonElement>(qrCodeContent, _jsonOptions);
        var qrCodeId = qrCode.GetProperty("id").GetInt32();

        // Step 5: Prüfen ob Fund existiert (sollte null sein)
        var findCheckResponse1 = await _client.GetAsync($"/api/finds/check?qrCodeId={qrCodeId}&userId={userId}");
        Assert.That(findCheckResponse1.IsSuccessStatusCode, Is.True, "Find-Check sollte erfolgreich sein");
        var findCheckContent1 = await findCheckResponse1.Content.ReadAsStringAsync();
        Assert.That(findCheckContent1.Trim(), Is.EqualTo("null").Or.Empty,
            "Fund sollte noch nicht existieren");

        // Step 6: Fund registrieren
        var findData = new
        {
            QrCodeId = qrCodeId,
            UserId = userId,
            IpAddress = "127.0.0.1",
            UserAgent = "Test Agent"
        };

        var findContent = new StringContent(
            JsonSerializer.Serialize(findData),
            Encoding.UTF8,
            "application/json");

        var findResponse = await _client.PostAsync("/api/finds", findContent);
        Assert.That(findResponse.IsSuccessStatusCode, Is.True, "Fund sollte registriert werden");

        // Step 7: Prüfen ob Fund jetzt existiert (sollte nicht null sein)
        var findCheckResponse2 = await _client.GetAsync($"/api/finds/check?qrCodeId={qrCodeId}&userId={userId}");
        Assert.That(findCheckResponse2.IsSuccessStatusCode, Is.True, "Find-Check sollte erfolgreich sein");
        var findCheckContent2 = await findCheckResponse2.Content.ReadAsStringAsync();
        Assert.That(findCheckContent2, Is.Not.Empty, "Fund sollte jetzt existieren");
        Assert.That(findCheckContent2.Trim(), Is.Not.EqualTo("null"), "Fund sollte nicht null sein");

        var existingFind = JsonSerializer.Deserialize<JsonElement>(findCheckContent2, _jsonOptions);
        Assert.That(existingFind.GetProperty("id").GetInt32(), Is.GreaterThan(0), "Fund sollte eine ID haben");
    }

    #endregion

    #region Progress Page Tests

    [Test]
    public async Task ProgressPage_CompleteFlow_Success()
    {
        // Arrange - User und Funde erstellen
        var userName = "ProgressPageTestUser";
        var registrationData = new { Name = userName };
        var registrationContent = new StringContent(
            JsonSerializer.Serialize(registrationData),
            Encoding.UTF8,
            "application/json");

        var registrationResponse = await _client.PostAsync("/api/users", registrationContent);
        Assert.That(registrationResponse.IsSuccessStatusCode, Is.True, "Registrierung sollte erfolgreich sein");

        var userContent = await registrationResponse.Content.ReadAsStringAsync();
        var user = JsonSerializer.Deserialize<JsonElement>(userContent, _jsonOptions);
        var userId = user.GetProperty("id").GetInt32();

        // Kampagne abrufen
        var campaignsResponse = await _client.GetAsync("/api/campaigns/active");
        Assert.That(campaignsResponse.IsSuccessStatusCode, Is.True, "Kampagnen sollten abrufbar sein");
        var campaignsContent = await campaignsResponse.Content.ReadAsStringAsync();
        var campaigns = JsonSerializer.Deserialize<JsonElement[]>(campaignsContent, _jsonOptions);
        Assert.That(campaigns, Is.Not.Null.And.Not.Empty, "Es sollte mindestens eine aktive Kampagne geben");
        var campaignId = campaigns![0].GetProperty("id").GetInt32();

        // QR-Code abrufen und Fund registrieren
        var qrCodeResponse = await _client.GetAsync("/api/qrcodes/by-code/testcode1");
        Assert.That(qrCodeResponse.IsSuccessStatusCode, Is.True, "QR-Code sollte abrufbar sein");
        var qrCodeContent = await qrCodeResponse.Content.ReadAsStringAsync();
        var qrCode = JsonSerializer.Deserialize<JsonElement>(qrCodeContent, _jsonOptions);
        var qrCodeId = qrCode.GetProperty("id").GetInt32();

        var findData = new
        {
            QrCodeId = qrCodeId,
            UserId = userId,
            IpAddress = "127.0.0.1",
            UserAgent = "Test Agent"
        };

        var findContent = new StringContent(
            JsonSerializer.Serialize(findData),
            Encoding.UTF8,
            "application/json");

        var findResponse = await _client.PostAsync("/api/finds", findContent);
        Assert.That(findResponse.IsSuccessStatusCode, Is.True, "Fund sollte registriert werden");

        // Act - Alle Endpunkte für Progress-Seite testen
        // 1. Funde für User und Kampagne abrufen
        var findsResponse = await _client.GetAsync($"/api/finds/user/{userId}/by-campaign?campaignId={campaignId}&take=10");
        Assert.That(findsResponse.IsSuccessStatusCode, Is.True,
            "GetFindsByUserAndCampaign sollte erfolgreich sein");

        // 2. User-Statistiken abrufen
        var statisticsResponse = await _client.GetAsync($"/api/statistics/user/{userId}");
        Assert.That(statisticsResponse.IsSuccessStatusCode, Is.True,
            "GetUserStatistics sollte erfolgreich sein");

        // Assert
        var findsContent = await findsResponse.Content.ReadAsStringAsync();
        var finds = JsonSerializer.Deserialize<JsonElement[]>(findsContent, _jsonOptions);
        Assert.That(finds, Is.Not.Null, "Funde sollten nicht null sein");
        Assert.That(finds!.Length, Is.GreaterThan(0), "Es sollte mindestens ein Fund zurückgegeben werden");

        var statisticsContent = await statisticsResponse.Content.ReadAsStringAsync();
        var statistics = JsonSerializer.Deserialize<JsonElement>(statisticsContent, _jsonOptions);
        Assert.That(statistics.ValueKind, Is.EqualTo(JsonValueKind.Object),
            "Statistiken sollten ein Objekt sein");
        Assert.That(statistics.GetProperty("userId").GetInt32(), Is.EqualTo(userId),
            "User ID sollte korrekt sein");
    }

    #endregion
}

