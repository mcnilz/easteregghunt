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
[Parallelizable(ParallelScope.None)]
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

    #region CheckUserNameExistsAsync Tests

    [Test]
    public async Task CheckUserNameExistsAsync_NewName_ReturnsFalse()
    {
        // Arrange
        var userName = "NewTestUser123";
        var requestData = new { Name = userName };
        var requestContent = new StringContent(
            JsonSerializer.Serialize(requestData),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/users/check-name", requestContent);

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True, "Check sollte erfolgreich sein");
        var content = await response.Content.ReadAsStringAsync();
        var checkResponse = JsonSerializer.Deserialize<CheckUserNameResponse>(content, _jsonOptions);

        Assert.That(checkResponse, Is.Not.Null, "Response sollte nicht null sein");
        Assert.That(checkResponse!.Exists, Is.False, "Neuer Name sollte nicht existieren");
        Assert.That(checkResponse.Name, Is.EqualTo(userName), "Name sollte im Response sein");
    }

    [Test]
    public async Task CheckUserNameExistsAsync_ExistingName_ReturnsTrue()
    {
        // Arrange - Erst einen User erstellen
        var userName = "ExistingTestUser";
        var registrationData = new { Name = userName };
        var registrationContent = new StringContent(
            JsonSerializer.Serialize(registrationData),
            Encoding.UTF8,
            "application/json");

        var registrationResponse = await _client.PostAsync("/api/users", registrationContent);
        Assert.That(registrationResponse.IsSuccessStatusCode, Is.True, "Registrierung sollte erfolgreich sein");

        // Act - Prüfen ob Name existiert
        var requestData = new { Name = userName };
        var requestContent = new StringContent(
            JsonSerializer.Serialize(requestData),
            Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync("/api/users/check-name", requestContent);

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True, "Check sollte erfolgreich sein");
        var content = await response.Content.ReadAsStringAsync();
        var checkResponse = JsonSerializer.Deserialize<CheckUserNameResponse>(content, _jsonOptions);

        Assert.That(checkResponse, Is.Not.Null, "Response sollte nicht null sein");
        Assert.That(checkResponse!.Exists, Is.True, "Existierender Name sollte gefunden werden");
        Assert.That(checkResponse.Name, Is.EqualTo(userName), "Name sollte im Response sein");
    }

    [Test]
    public async Task CheckUserNameExistsAsync_EmptyName_ReturnsBadRequest()
    {
        // Arrange - Leerer Name
        var requestData = new { Name = "" };
        var requestContent = new StringContent(
            JsonSerializer.Serialize(requestData),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/users/check-name", requestContent);

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.False, "Check mit leerem Namen sollte fehlschlagen");
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest),
            "Leerer Name sollte BadRequest zurückgeben");
    }

    #endregion

    #region RegisterEmployeeAsync Tests

    [Test]
    public async Task RegisterEmployeeAsync_NewUser_CreatesUser()
    {
        // Arrange
        var userName = "NewRegistrationUser";

        // Act
        var registrationData = new { Name = userName };
        var registrationContent = new StringContent(
            JsonSerializer.Serialize(registrationData),
            Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync("/api/users", registrationContent);

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True, "Registrierung sollte erfolgreich sein");
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created),
            "Status sollte Created sein");

        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Is.Not.Empty, "Response sollte nicht leer sein");

        var user = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);
        Assert.That(user.GetProperty("id").GetInt32(), Is.GreaterThan(0), "User sollte eine ID haben");
        Assert.That(user.GetProperty("name").GetString(), Is.EqualTo(userName), "Name sollte korrekt sein");
    }

    [Test]
    public async Task RegisterEmployeeAsync_DuplicateName_ReturnsError()
    {
        // Arrange - Erst einen User erstellen
        var userName = "DuplicateRegistrationUser";
        var registrationData = new { Name = userName };
        var registrationContent = new StringContent(
            JsonSerializer.Serialize(registrationData),
            Encoding.UTF8,
            "application/json");

        var firstRegistration = await _client.PostAsync("/api/users", registrationContent);
        Assert.That(firstRegistration.IsSuccessStatusCode, Is.True, "Erste Registrierung sollte erfolgreich sein");

        // Act - Versuche denselben Namen nochmal zu registrieren
        var secondRegistrationContent = new StringContent(
            JsonSerializer.Serialize(registrationData),
            Encoding.UTF8,
            "application/json");

        var secondResponse = await _client.PostAsync("/api/users", secondRegistrationContent);

        // Assert
        Assert.That(secondResponse.IsSuccessStatusCode, Is.False,
            "Registrierung mit bereits existierendem Namen sollte fehlschlagen");
        Assert.That(secondResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.InternalServerError),
            "Status sollte InternalServerError sein");
    }

    #endregion

    #region GetExistingFindAsync Tests

    [Test]
    public async Task GetExistingFindAsync_NoExistingFind_ReturnsNull()
    {
        // Arrange - Erst einen User und QR-Code erstellen
        var userName = "FindTestUser";
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

        // QR-Code abrufen
        var qrCodeResponse = await _client.GetAsync("/api/qrcodes/by-code/testcode1");
        Assert.That(qrCodeResponse.IsSuccessStatusCode, Is.True, "QR-Code sollte abrufbar sein");

        var qrCodeContent = await qrCodeResponse.Content.ReadAsStringAsync();
        var qrCode = JsonSerializer.Deserialize<JsonElement>(qrCodeContent, _jsonOptions);
        var qrCodeId = qrCode.GetProperty("id").GetInt32();

        // Act - Prüfen ob Fund existiert (sollte null sein)
        var checkResponse = await _client.GetAsync($"/api/finds/check?qrCodeId={qrCodeId}&userId={userId}");

        // Assert
        Assert.That(checkResponse.IsSuccessStatusCode, Is.True, "Check sollte erfolgreich sein");
        var checkContent = await checkResponse.Content.ReadAsStringAsync();

        // Response sollte null sein (als JSON "null" oder leerer String)
        Assert.That(checkContent.Trim(), Is.EqualTo("null").Or.Empty,
            "Response sollte null sein wenn kein Fund existiert");
    }

    [Test]
    public async Task GetExistingFindAsync_ExistingFind_ReturnsFind()
    {
        // Arrange - Erst einen User erstellen
        var userName = "ExistingFindTestUser";
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

        // QR-Code abrufen
        var qrCodeResponse = await _client.GetAsync("/api/qrcodes/by-code/testcode1");
        Assert.That(qrCodeResponse.IsSuccessStatusCode, Is.True, "QR-Code sollte abrufbar sein");

        var qrCodeContent = await qrCodeResponse.Content.ReadAsStringAsync();
        var qrCode = JsonSerializer.Deserialize<JsonElement>(qrCodeContent, _jsonOptions);
        var qrCodeId = qrCode.GetProperty("id").GetInt32();

        // Fund registrieren
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

        // Act - Prüfen ob Fund existiert
        var checkResponse = await _client.GetAsync($"/api/finds/check?qrCodeId={qrCodeId}&userId={userId}");

        // Assert
        Assert.That(checkResponse.IsSuccessStatusCode, Is.True, "Check sollte erfolgreich sein");
        var checkContent = await checkResponse.Content.ReadAsStringAsync();

        // Response sollte nicht leer oder null sein
        Assert.That(checkContent, Is.Not.Empty, "Response sollte nicht leer sein");
        Assert.That(checkContent.Trim(), Is.Not.EqualTo("null"), "Response sollte nicht null sein");

        var existingFind = JsonSerializer.Deserialize<JsonElement>(checkContent, _jsonOptions);
        Assert.That(existingFind.GetProperty("id").GetInt32(), Is.GreaterThan(0), "Fund sollte eine ID haben");
        Assert.That(existingFind.GetProperty("qrCodeId").GetInt32(), Is.EqualTo(qrCodeId), "QR-Code ID sollte korrekt sein");
        Assert.That(existingFind.GetProperty("userId").GetInt32(), Is.EqualTo(userId), "User ID sollte korrekt sein");
    }

    #endregion

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

    #region Progress Page Endpoint Tests

    [Test]
    public async Task GetFindsByUserAndCampaign_ValidUserAndCampaign_ReturnsFinds()
    {
        // Arrange - User und Kampagne erstellen
        var userName = "ProgressTestUser";
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

        // Kampagne abrufen (aus Seed-Daten)
        var campaignsResponse = await _client.GetAsync("/api/campaigns/active");
        Assert.That(campaignsResponse.IsSuccessStatusCode, Is.True, "Kampagnen sollten abrufbar sein");
        var campaignsContent = await campaignsResponse.Content.ReadAsStringAsync();
        var campaigns = JsonSerializer.Deserialize<JsonElement[]>(campaignsContent, _jsonOptions);
        Assert.That(campaigns, Is.Not.Null.And.Not.Empty, "Es sollte mindestens eine aktive Kampagne geben");
        var campaignId = campaigns![0].GetProperty("id").GetInt32();

        // QR-Code abrufen
        var qrCodeResponse = await _client.GetAsync("/api/qrcodes/by-code/testcode1");
        Assert.That(qrCodeResponse.IsSuccessStatusCode, Is.True, "QR-Code sollte abrufbar sein");
        var qrCodeContent = await qrCodeResponse.Content.ReadAsStringAsync();
        var qrCode = JsonSerializer.Deserialize<JsonElement>(qrCodeContent, _jsonOptions);
        var qrCodeId = qrCode.GetProperty("id").GetInt32();

        // Fund registrieren
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

        // Act - Funde für User und Kampagne abrufen (mit korrektem Endpunkt)
        var progressResponse = await _client.GetAsync($"/api/finds/user/{userId}/by-campaign?campaignId={campaignId}&take=10");

        // Assert
        Assert.That(progressResponse.IsSuccessStatusCode, Is.True,
            "GetFindsByUserAndCampaign sollte erfolgreich sein");
        Assert.That(progressResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK),
            "Status sollte OK sein");

        var progressContent = await progressResponse.Content.ReadAsStringAsync();
        Assert.That(progressContent, Is.Not.Empty, "Response sollte nicht leer sein");

        var finds = JsonSerializer.Deserialize<JsonElement[]>(progressContent, _jsonOptions);
        Assert.That(finds, Is.Not.Null, "Funde sollten nicht null sein");
        Assert.That(finds!.Length, Is.GreaterThan(0), "Es sollte mindestens ein Fund zurückgegeben werden");

        // Prüfe dass der Fund die erwarteten Eigenschaften hat
        var find = finds[0];
        Assert.That(find.GetProperty("userId").GetInt32(), Is.EqualTo(userId),
            "User ID sollte korrekt sein");
        Assert.That(find.GetProperty("qrCodeId").GetInt32(), Is.EqualTo(qrCodeId),
            "QR-Code ID sollte korrekt sein");
    }

    [Test]
    public async Task GetUserStatistics_ValidUser_ReturnsStatistics()
    {
        // Arrange - User erstellen
        var userName = "StatisticsTestUser";
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

        // Act - User-Statistiken abrufen (mit korrektem Endpunkt)
        var statisticsResponse = await _client.GetAsync($"/api/statistics/user/{userId}");

        // Assert
        Assert.That(statisticsResponse.IsSuccessStatusCode, Is.True,
            "GetUserStatistics sollte erfolgreich sein");
        Assert.That(statisticsResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK),
            "Status sollte OK sein");

        var statisticsContent = await statisticsResponse.Content.ReadAsStringAsync();
        Assert.That(statisticsContent, Is.Not.Empty, "Response sollte nicht leer sein");

        var statistics = JsonSerializer.Deserialize<JsonElement>(statisticsContent, _jsonOptions);
        Assert.That(statistics.ValueKind, Is.EqualTo(JsonValueKind.Object),
            "Statistiken sollten ein Objekt sein");
        Assert.That(statistics.GetProperty("userId").GetInt32(), Is.EqualTo(userId),
            "User ID sollte korrekt sein");
        Assert.That(statistics.GetProperty("userName").GetString(), Is.EqualTo(userName),
            "User Name sollte korrekt sein");
    }

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

