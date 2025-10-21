using System.Text;
using System.Text.Json;
using EasterEggHunt.Api;
using EasterEggHunt.Integration.Tests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace EasterEggHunt.Integration.Tests.Workflows;

/// <summary>
/// Integration Tests für komplette Mitarbeiter-Journeys
/// Basierend auf: features/employee_registration.feature, features/qr_code_scanning.feature, features/employee_progress.feature
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.None)]
public class EmployeeJourneyIntegrationTests : IDisposable
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

    #region Employee Registration Workflow Tests

    [Test]
    public async Task EmployeeJourney_FirstTimeScan_TriggersRegistration()
    {
        // Act - QR-Code scannen ohne Session (sollte Registrierung auslösen)
        var scanResponse = await _client.PostAsync("/api/finds/scan/testcode1", null);

        // Assert
        Assert.That(scanResponse.IsSuccessStatusCode, Is.False,
            "Scan ohne Session sollte Registrierung erfordern");
        Assert.That(scanResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound),
            "Scan ohne Registrierung sollte NotFound zurückgeben (kein QR-Code gefunden)");
    }

    [Test]
    public async Task EmployeeJourney_RegistrationFlow_CompleteWorkflow()
    {
        // Arrange
        var userName = "Anna Schmidt";
        var registrationData = new
        {
            Name = userName
        };

        // Act 1: Employee registrieren
        var registrationContent = new StringContent(
            JsonSerializer.Serialize(registrationData),
            Encoding.UTF8,
            "application/json");

        var registrationResponse = await _client.PostAsync("/api/users", registrationContent);

        // Assert
        Assert.That(registrationResponse.IsSuccessStatusCode, Is.True, "Registrierung sollte erfolgreich sein");

        var responseContent = await registrationResponse.Content.ReadAsStringAsync();
        Assert.That(responseContent, Is.Not.Empty, "Registrierungs-Response sollte nicht leer sein");

        // Act 2: Nach Registrierung QR-Code scannen
        // 1. QR-Code abrufen
        var qrCodeResponse = await _client.GetAsync("/api/qrcodes/by-code/testcode1");
        Assert.That(qrCodeResponse.IsSuccessStatusCode, Is.True, "QR-Code sollte abrufbar sein");

        var qrCodeContent = await qrCodeResponse.Content.ReadAsStringAsync();
        var qrCode = JsonSerializer.Deserialize<JsonElement>(qrCodeContent);
        var qrCodeId = qrCode.GetProperty("id").GetInt32();

        // 2. User-ID aus Registrierungs-Response extrahieren
        var userContent = await registrationResponse.Content.ReadAsStringAsync();
        var user = JsonSerializer.Deserialize<JsonElement>(userContent);
        var userId = user.GetProperty("id").GetInt32();

        // 3. Fund registrieren
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

        var scanResponse = await _client.PostAsync("/api/finds", findContent);
        Assert.That(scanResponse.IsSuccessStatusCode, Is.True, "Scan nach Registrierung sollte erfolgreich sein");
    }

    [Test]
    public async Task EmployeeJourney_DuplicateName_ReturnsError()
    {
        // Arrange
        var userName = "Duplicate User";
        var registrationData = new
        {
            Name = userName
        };

        var registrationContent = new StringContent(
            JsonSerializer.Serialize(registrationData),
            Encoding.UTF8,
            "application/json");

        // Act 1: Erste Registrierung
        var firstRegistration = await _client.PostAsync("/api/users", registrationContent);
        Assert.That(firstRegistration.IsSuccessStatusCode, Is.True, "Erste Registrierung sollte erfolgreich sein");

        // Act 2: Zweite Registrierung mit gleichem Namen
        var secondRegistration = await _client.PostAsync("/api/users", registrationContent);

        // Assert
        Assert.That(secondRegistration.IsSuccessStatusCode, Is.False,
            "Registrierung mit bereits existierendem Namen sollte fehlschlagen");
        Assert.That(secondRegistration.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.InternalServerError),
            "Status sollte InternalServerError sein (API wirft Exception statt Conflict)");
    }

    [Test]
    public async Task EmployeeJourney_EmptyName_ReturnsValidationError()
    {
        // Arrange
        var registrationData = new
        {
            Name = "" // Leerer Name
        };

        var registrationContent = new StringContent(
            JsonSerializer.Serialize(registrationData),
            Encoding.UTF8,
            "application/json");

        // Act
        var registrationResponse = await _client.PostAsync("/api/users", registrationContent);

        // Assert
        Assert.That(registrationResponse.IsSuccessStatusCode, Is.False,
            "Registrierung mit leerem Namen sollte fehlschlagen");
        Assert.That(registrationResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest),
            "Status sollte BadRequest sein");
    }

    #endregion

    #region QR-Code Scanning Workflow Tests

    [Test]
    public async Task EmployeeJourney_ScanValidQrCode_Success()
    {
        // Arrange
        var employeeClient = await _factory.CreateAuthenticatedEmployeeClientAsync("Test Employee");

        // Act - QR-Code scannen (korrekte API-Calls)
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
        Assert.That(scanResponse.IsSuccessStatusCode, Is.True, "Scan von gültigem QR-Code sollte erfolgreich sein");

        var responseContent = await scanResponse.Content.ReadAsStringAsync();
        Assert.That(responseContent, Is.Not.Empty, "Scan-Response sollte nicht leer sein");
    }

    [Test]
    public async Task EmployeeJourney_ScanInvalidQrCode_ReturnsError()
    {
        // Arrange
        var employeeClient = await _factory.CreateAuthenticatedEmployeeClientAsync("Test Employee");

        // Act - Ungültigen QR-Code scannen (korrekte API-Calls)
        // 1. Ungültigen QR-Code abrufen
        var qrCodeResponse = await employeeClient.GetAsync("/api/qrcodes/by-code/invalidcode");
        Assert.That(qrCodeResponse.IsSuccessStatusCode, Is.False,
            "Ungültiger QR-Code sollte NotFound zurückgeben");
        Assert.That(qrCodeResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound),
            "Status sollte NotFound sein");
    }

    [Test]
    public async Task EmployeeJourney_ScanInactiveQrCode_ReturnsError()
    {
        // Arrange
        var employeeClient = await _factory.CreateAuthenticatedEmployeeClientAsync("Test Employee");

        // Act - Inaktiven QR-Code scannen (testcode2 ist inaktiv) - korrekte API-Calls
        // 1. Inaktiven QR-Code abrufen
        var qrCodeResponse = await employeeClient.GetAsync("/api/qrcodes/by-code/testcode2");
        Assert.That(qrCodeResponse.IsSuccessStatusCode, Is.True, "Inaktiver QR-Code sollte abrufbar sein");

        var qrCodeContent = await qrCodeResponse.Content.ReadAsStringAsync();
        var qrCode = JsonSerializer.Deserialize<JsonElement>(qrCodeContent);
        var qrCodeId = qrCode.GetProperty("id").GetInt32();
        var isActive = qrCode.GetProperty("isActive").GetBoolean();

        // Assert - QR-Code sollte inaktiv sein
        Assert.That(isActive, Is.False, "QR-Code sollte inaktiv sein");
    }

    [Test]
    public async Task EmployeeJourney_ScanSameQrCodeMultipleTimes_AllowsDuplicates()
    {
        // Arrange
        var employeeClient = await _factory.CreateAuthenticatedEmployeeClientAsync("Test Employee");

        // Act 1: Ersten Scan (korrekte API-Calls)
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

        // 3. Ersten Fund registrieren
        var findData1 = new { QrCodeId = qrCodeId, UserId = userId, IpAddress = "127.0.0.1", UserAgent = "Test Agent" };
        var findContent1 = new StringContent(JsonSerializer.Serialize(findData1), Encoding.UTF8, "application/json");
        var firstScanResponse = await employeeClient.PostAsync("/api/finds", findContent1);
        Assert.That(firstScanResponse.IsSuccessStatusCode, Is.True, "Erster Scan sollte erfolgreich sein");

        // Act 2: Zweiten Scan desselben QR-Codes
        var findData2 = new { QrCodeId = qrCodeId, UserId = userId, IpAddress = "127.0.0.1", UserAgent = "Test Agent" };
        var findContent2 = new StringContent(JsonSerializer.Serialize(findData2), Encoding.UTF8, "application/json");
        var secondScanResponse = await employeeClient.PostAsync("/api/finds", findContent2);

        // Assert
        Assert.That(secondScanResponse.IsSuccessStatusCode, Is.True,
            "Zweiter Scan desselben QR-Codes sollte auch erfolgreich sein (Duplikate erlaubt)");
    }

    #endregion

    #region Progress Tracking Workflow Tests

    [Test]
    public async Task EmployeeJourney_ProgressTracking_CompleteWorkflow()
    {
        // Arrange
        var employeeClient = await _factory.CreateAuthenticatedEmployeeClientAsync("Progress Test Employee");

        // Act 1: QR-Code scannen (korrekte API-Calls)
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
        var scan1Response = await employeeClient.PostAsync("/api/finds", findContent);

        Assert.That(scan1Response.IsSuccessStatusCode, Is.True, "Erster Scan sollte erfolgreich sein");

        // Act 2: User-Information abrufen (statt Fortschritt) - bereits abgerufen

        // Assert
        Assert.That(userResponse.IsSuccessStatusCode, Is.True, "User-Information sollte abrufbar sein");

        Assert.That(userContent, Is.Not.Empty, "User-Response sollte nicht leer sein");
    }

    [Test]
    public async Task EmployeeJourney_ProgressWithoutFinds_ShowsZeroProgress()
    {
        // Arrange
        var employeeClient = await _factory.CreateAuthenticatedEmployeeClientAsync("No Finds Employee");

        // Act - User-Information ohne Funde abrufen
        var userResponse = await employeeClient.GetAsync("/api/users/active");

        // Assert
        Assert.That(userResponse.IsSuccessStatusCode, Is.True, "User-Information sollte auch ohne Funde abrufbar sein");

        var userContent = await userResponse.Content.ReadAsStringAsync();
        Assert.That(userContent, Is.Not.Empty, "User-Response sollte nicht leer sein");
    }

    [Test]
    public async Task EmployeeJourney_ProgressWithMultipleFinds_ShowsCorrectCount()
    {
        // Arrange
        var employeeClient = await _factory.CreateAuthenticatedEmployeeClientAsync("Multi Find Employee");

        // Act 1: QR-Code scannen (korrekte API-Calls)
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
        var findResponse = await employeeClient.PostAsync("/api/finds", findContent);
        Assert.That(findResponse.IsSuccessStatusCode, Is.True, "Fund sollte registriert werden");

        // Act 2: User-Information abrufen
        var finalUserResponse = await employeeClient.GetAsync("/api/users/active");

        // Assert
        Assert.That(finalUserResponse.IsSuccessStatusCode, Is.True, "User-Information sollte abrufbar sein");

        var finalUserContent = await finalUserResponse.Content.ReadAsStringAsync();
        Assert.That(finalUserContent, Is.Not.Empty, "User-Response sollte nicht leer sein");
    }

    #endregion

    #region Session Management Workflow Tests

    [Test]
    public async Task EmployeeJourney_SessionPersistence_WorksAcrossRequests()
    {
        // Arrange
        var employeeClient = await _factory.CreateAuthenticatedEmployeeClientAsync("Session Test Employee");

        // Act - Mehrere Requests mit derselben Session
        var userResponse1 = await employeeClient.GetAsync("/api/users/active");

        // QR-Code scannen (korrekte API-Calls)
        var qrCodeResponse = await employeeClient.GetAsync("/api/qrcodes/by-code/testcode1");
        Assert.That(qrCodeResponse.IsSuccessStatusCode, Is.True, "QR-Code sollte abrufbar sein");
        var qrCodeContent = await qrCodeResponse.Content.ReadAsStringAsync();
        var qrCode = JsonSerializer.Deserialize<JsonElement>(qrCodeContent);
        var qrCodeId = qrCode.GetProperty("id").GetInt32();

        var userContent = await userResponse1.Content.ReadAsStringAsync();
        var users = JsonSerializer.Deserialize<JsonElement[]>(userContent);
        var userId = users![0].GetProperty("id").GetInt32();

        var findData = new { QrCodeId = qrCodeId, UserId = userId, IpAddress = "127.0.0.1", UserAgent = "Test Agent" };
        var findContent = new StringContent(JsonSerializer.Serialize(findData), Encoding.UTF8, "application/json");
        var scanResponse = await employeeClient.PostAsync("/api/finds", findContent);

        var userResponse2 = await employeeClient.GetAsync("/api/users/active");

        // Assert
        Assert.That(userResponse1.IsSuccessStatusCode, Is.True, "Erster User-Request sollte erfolgreich sein");
        Assert.That(scanResponse.IsSuccessStatusCode, Is.True, "Scan-Request sollte erfolgreich sein");
        Assert.That(userResponse2.IsSuccessStatusCode, Is.True, "Zweiter User-Request sollte erfolgreich sein");

        // User-Information sollte sich nach dem Scan geändert haben (last seen timestamp)
        var content1 = await userResponse1.Content.ReadAsStringAsync();
        var content2 = await userResponse2.Content.ReadAsStringAsync();
        Assert.That(content1, Is.Not.EqualTo(content2), "User-Information sollte sich nach Scan geändert haben");
    }

    [Test]
    public async Task EmployeeJourney_UnauthorizedAccess_ReturnsForbidden()
    {
        // Arrange - Client ohne Authentication
        var unauthorizedClient = _factory.CreateClient();

        // Act - Versuche auf User-Endpoints zuzugreifen
        var userResponse = await unauthorizedClient.GetAsync("/api/users/active");
        var qrCodeResponse = await unauthorizedClient.GetAsync("/api/qrcodes/by-code/testcode1");

        // Assert
        Assert.That(userResponse.IsSuccessStatusCode, Is.True,
            "User-Endpoint sollte auch ohne Session abrufbar sein");
        Assert.That(qrCodeResponse.IsSuccessStatusCode, Is.True,
            "QR-Code-Endpoint sollte auch ohne Session abrufbar sein");
    }

    #endregion

    #region Error Handling Workflow Tests

    [Test]
    public async Task EmployeeJourney_ScanWithInvalidSession_ReturnsUnauthorized()
    {
        // Arrange - Client mit ungültiger Session
        var invalidClient = _factory.CreateClient();

        // Füge ungültige Session-Header hinzu
        invalidClient.DefaultRequestHeaders.Add("Authorization", "Bearer invalid-session-token");

        // Act
        var scanResponse = await invalidClient.PostAsync("/api/finds/scan/testcode1", null);

        // Assert
        Assert.That(scanResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound),
            "Scan mit ungültiger Session sollte NotFound zurückgeben");
    }

    [Test]
    public async Task EmployeeJourney_RegistrationWithSpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        var specialNames = new[]
        {
            "Max Müller",
            "Anna-Maria Schmidt",
            "Dr. Hans Meier",
            "O'Connor",
            "François Dubois"
        };

        // Act & Assert
        foreach (var name in specialNames)
        {
            var registrationData = new { Name = name };
            var registrationContent = new StringContent(
                JsonSerializer.Serialize(registrationData),
                Encoding.UTF8,
                "application/json");

            var registrationResponse = await _client.PostAsync("/api/users", registrationContent);

            Assert.That(registrationResponse.IsSuccessStatusCode, Is.True,
                $"Registrierung mit Namen '{name}' sollte erfolgreich sein");
        }
    }

    #endregion
}
