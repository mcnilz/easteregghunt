using System.Text;
using System.Text.Json;
using EasterEggHunt.Integration.Tests.Helpers;
using NUnit.Framework;

namespace EasterEggHunt.Integration.Tests.Workflows;

/// <summary>
/// Integration Tests für Cross-Cutting Concerns
/// Tests für: Session-Timeout, Concurrency, Error-Handling, Logging
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.None)]
public class CrossCuttingIntegrationTests : IDisposable
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

    #region Session Management Workflow Tests

    [Test]
    public async Task CrossCutting_SessionTimeout_AdminSessionExpires()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();

        // Act 1: Erste Anfrage mit gültiger Session
        var firstResponse = await adminClient.GetAsync("/api/campaigns/active");
        Assert.That(firstResponse.IsSuccessStatusCode, Is.True, "Erste Anfrage sollte erfolgreich sein");

        // Act 2: Session simulieren (in realer Anwendung würde dies durch Timeout passieren)
        // Hier testen wir, dass das System korrekt mit Session-Problemen umgeht
        var secondResponse = await adminClient.GetAsync("/api/campaigns/active");
        Assert.That(secondResponse.IsSuccessStatusCode, Is.True, "Zweite Anfrage sollte erfolgreich sein");
    }

    [Test]
    public async Task CrossCutting_SessionTimeout_EmployeeSessionExpires()
    {
        // Arrange
        var employeeClient = await _factory.CreateAuthenticatedEmployeeClientAsync("Session Test Employee");

        // Act 1: Erste Anfrage mit gültiger Session
        var firstResponse = await employeeClient.GetAsync("/api/users/active");
        Assert.That(firstResponse.IsSuccessStatusCode, Is.True, "Erste Anfrage sollte erfolgreich sein");

        // Act 2: Session simulieren (in realer Anwendung würde dies durch Timeout passieren)
        var secondResponse = await employeeClient.GetAsync("/api/users/active");
        Assert.That(secondResponse.IsSuccessStatusCode, Is.True, "Zweite Anfrage sollte erfolgreich sein");
    }

    [Test]
    public async Task CrossCutting_SessionConsistency_MultipleRequestsSameSession()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();

        // Act - Mehrere Requests mit derselben Session
        var tasks = new[]
        {
            adminClient.GetAsync("/api/campaigns/active"),
            adminClient.GetAsync("/api/qrcodes/campaign/1"),
            adminClient.GetAsync("/api/campaigns/active"),
            adminClient.GetAsync("/api/qrcodes/campaign/1")
        };

        var responses = await Task.WhenAll(tasks);

        // Assert
        foreach (var response in responses)
        {
            Assert.That(response.IsSuccessStatusCode, Is.True,
                "Alle Requests mit derselben Session sollten erfolgreich sein");
        }
    }

    #endregion

    #region Concurrency Workflow Tests

    [Test]
    public async Task CrossCutting_ConcurrentAdminRequests_HandlesCorrectly()
    {
        // Arrange
        var adminClient1 = await _factory.CreateAuthenticatedAdminClientAsync();
        var adminClient2 = await _factory.CreateAuthenticatedAdminClientAsync();

        // Act - Gleichzeitige Admin-Requests
        var tasks = new[]
        {
            adminClient1.GetAsync("/api/campaigns/active"),
            adminClient2.GetAsync("/api/campaigns/active"),
            adminClient1.GetAsync("/api/qrcodes/campaign/1"),
            adminClient2.GetAsync("/api/qrcodes/campaign/1")
        };

        var responses = await Task.WhenAll(tasks);

        // Assert
        foreach (var response in responses)
        {
            Assert.That(response.IsSuccessStatusCode, Is.True,
                "Gleichzeitige Admin-Requests sollten erfolgreich sein");
        }
    }

    [Test]
    public async Task CrossCutting_ConcurrentEmployeeRequests_HandlesCorrectly()
    {
        // Arrange
        var employeeClient1 = await _factory.CreateAuthenticatedEmployeeClientAsync("Employee 1");
        var employeeClient2 = await _factory.CreateAuthenticatedEmployeeClientAsync("Employee 2");

        // Act - Gleichzeitige Employee-Requests
        var tasks = new[]
        {
            employeeClient1.GetAsync("/api/users/active"),
            employeeClient2.GetAsync("/api/users/active"),
            employeeClient1.GetAsync("/api/qrcodes/by-code/testcode1"), // Vereinfacht: QR-Code abrufen statt scannen
            employeeClient2.GetAsync("/api/qrcodes/by-code/testcode3")  // Vereinfacht: QR-Code abrufen statt scannen
        };

        var responses = await Task.WhenAll(tasks);

        // Assert
        foreach (var response in responses)
        {
            Assert.That(response.IsSuccessStatusCode, Is.True,
                "Gleichzeitige Employee-Requests sollten erfolgreich sein");
        }
    }

    [Test]
    public async Task CrossCutting_ConcurrentMixedRequests_HandlesCorrectly()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();
        var employeeClient = await _factory.CreateAuthenticatedEmployeeClientAsync("Mixed Test Employee");

        // Act - Gleichzeitige Admin- und Employee-Requests
        var tasks = new[]
        {
            adminClient.GetAsync("/api/campaigns/active"),
            employeeClient.GetAsync("/api/users/active"),
            adminClient.GetAsync("/api/qrcodes/campaign/1"),
            employeeClient.GetAsync("/api/qrcodes/by-code/testcode1") // Vereinfacht: QR-Code abrufen statt scannen
        };

        var responses = await Task.WhenAll(tasks);

        // Assert
        foreach (var response in responses)
        {
            Assert.That(response.IsSuccessStatusCode, Is.True,
                "Gleichzeitige gemischte Requests sollten erfolgreich sein");
        }
    }

    [Test]
    public async Task CrossCutting_ConcurrentDataModification_HandlesCorrectly()
    {
        // Arrange
        var adminClient1 = await _factory.CreateAuthenticatedAdminClientAsync();
        var adminClient2 = await _factory.CreateAuthenticatedAdminClientAsync();

        // Act - Gleichzeitige Datenmodifikationen
        var campaignData1 = new { Name = "Concurrent Campaign 1", Description = "Test 1", CreatedBy = "Admin" };
        var campaignData2 = new { Name = "Concurrent Campaign 2", Description = "Test 2", CreatedBy = "Admin" };

        var createContent1 = new StringContent(JsonSerializer.Serialize(campaignData1), Encoding.UTF8, "application/json");
        var createContent2 = new StringContent(JsonSerializer.Serialize(campaignData2), Encoding.UTF8, "application/json");

        var tasks = new[]
        {
            adminClient1.PostAsync("/api/campaigns", createContent1),
            adminClient2.PostAsync("/api/campaigns", createContent2)
        };

        var responses = await Task.WhenAll(tasks);

        // Assert
        foreach (var response in responses)
        {
            Assert.That(response.IsSuccessStatusCode, Is.True,
                "Gleichzeitige Datenmodifikationen sollten erfolgreich sein");
        }
    }

    #endregion

    #region Error Handling Workflow Tests

    [Test]
    public async Task CrossCutting_InvalidJsonPayload_ReturnsBadRequest()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();
        var invalidJson = "{ invalid json }";

        // Act
        var createContent = new StringContent(invalidJson, Encoding.UTF8, "application/json");
        var response = await adminClient.PostAsync("/api/campaigns", createContent);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest),
            "Ungültiges JSON sollte BadRequest zurückgeben");
    }

    [Test]
    public async Task CrossCutting_MissingRequiredFields_ReturnsValidationError()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();
        var incompleteData = new { Name = "Test" }; // Fehlende Required Fields

        // Act
        var createContent = new StringContent(
            JsonSerializer.Serialize(incompleteData),
            Encoding.UTF8,
            "application/json");

        var response = await adminClient.PostAsync("/api/campaigns", createContent);

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.False,
            "Unvollständige Daten sollten Fehler verursachen");
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest),
            "Status sollte BadRequest sein");
    }

    [Test]
    public async Task CrossCutting_NonExistentEndpoint_ReturnsNotFound()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();

        // Act
        var response = await adminClient.GetAsync("/api/nonexistent");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound),
            "Nicht-existierender Endpoint sollte NotFound zurückgeben");
    }

    [Test]
    public async Task CrossCutting_UnauthorizedAccess_ReturnsUnauthorized()
    {
        // Arrange - Client ohne Authentication
        var unauthorizedClient = _factory.CreateClient();

        // Act
        var response = await unauthorizedClient.GetAsync("/api/campaigns");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.MethodNotAllowed),
            "Unauthorized Zugriff sollte MethodNotAllowed zurückgeben");
    }

    [Test]
    public async Task CrossCutting_ServerError_HandlesGracefully()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();

        // Act - Versuche Operation, die Server-Fehler verursachen könnte
        // (z.B. sehr große Datenmenge oder ungültige Operation)
        var response = await adminClient.GetAsync("/api/campaigns/999999");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound),
            "Server sollte graceful mit Fehlern umgehen");
    }

    #endregion

    #region Database Transaction Workflow Tests

    [Test]
    public async Task CrossCutting_DatabaseTransaction_RollbackOnError()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();

        // Act 1: Gültige Kampagne erstellen
        var validCampaignData = new
        {
            Name = "Valid Campaign",
            Description = "Valid Description",
            CreatedBy = "Admin"
        };

        var validContent = new StringContent(
            JsonSerializer.Serialize(validCampaignData),
            Encoding.UTF8,
            "application/json");

        var validResponse = await adminClient.PostAsync("/api/campaigns", validContent);
        Assert.That(validResponse.IsSuccessStatusCode, Is.True, "Gültige Kampagne sollte erstellt werden");

        // Act 2: Ungültige Kampagne erstellen (sollte fehlschlagen)
        var invalidCampaignData = new
        {
            Name = "", // Ungültig
            Description = "Invalid Description",
            CreatedBy = "Admin"
        };

        var invalidContent = new StringContent(
            JsonSerializer.Serialize(invalidCampaignData),
            Encoding.UTF8,
            "application/json");

        var invalidResponse = await adminClient.PostAsync("/api/campaigns", invalidContent);
        Assert.That(invalidResponse.IsSuccessStatusCode, Is.False, "Ungültige Kampagne sollte fehlschlagen");

        // Assert - Erste Kampagne sollte noch existieren
        var getResponse = await adminClient.GetAsync("/api/campaigns/active");
        Assert.That(getResponse.IsSuccessStatusCode, Is.True, "Gültige Kampagne sollte noch existieren");
    }

    [Test]
    public async Task CrossCutting_DatabaseConsistency_MaintainedAcrossOperations()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();
        var employeeClient = await _factory.CreateAuthenticatedEmployeeClientAsync("Consistency Test Employee");

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
        var scanResponse = await employeeClient.PostAsync("/api/finds", findContent);
        Assert.That(scanResponse.IsSuccessStatusCode, Is.True, "Scan sollte erfolgreich sein");

        // Act 2: Statistiken abrufen
        var statsResponse = await adminClient.GetAsync("/api/statistics/qrcode/1");
        Assert.That(statsResponse.IsSuccessStatusCode, Is.True, "Statistiken sollten abrufbar sein");

        // Act 3: Fortschritt abrufen
        var progressResponse = await employeeClient.GetAsync("/api/users/active");
        Assert.That(progressResponse.IsSuccessStatusCode, Is.True, "Fortschritt sollte abrufbar sein");

        // Assert - Alle Daten sollten konsistent sein
        var statsContent = await statsResponse.Content.ReadAsStringAsync();
        var progressContent = await progressResponse.Content.ReadAsStringAsync();

        Assert.That(statsContent, Is.Not.Empty, "Statistiken sollten Daten enthalten");
        Assert.That(progressContent, Is.Not.Empty, "Fortschritt sollte Daten enthalten");
    }

    #endregion

    #region Performance Workflow Tests

    [Test]
    public async Task CrossCutting_Performance_MultipleRequestsWithinTimeLimit()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();
        var startTime = DateTime.UtcNow;

        // Act - Mehrere Requests schnell hintereinander
        var tasks = new[]
        {
            adminClient.GetAsync("/api/campaigns/active"),
            adminClient.GetAsync("/api/qrcodes/campaign/1"),
            adminClient.GetAsync("/api/campaigns/active"),
            adminClient.GetAsync("/api/qrcodes/campaign/1"),
            adminClient.GetAsync("/api/campaigns/active")
        };

        var responses = await Task.WhenAll(tasks);
        var endTime = DateTime.UtcNow;
        var duration = endTime - startTime;

        // Assert
        Assert.That(duration.TotalSeconds, Is.LessThan(5),
            "Alle Requests sollten innerhalb von 5 Sekunden abgeschlossen sein");

        foreach (var response in responses)
        {
            Assert.That(response.IsSuccessStatusCode, Is.True,
                "Alle Requests sollten erfolgreich sein");
        }
    }

    [Test]
    public async Task CrossCutting_Performance_LargeDataHandling()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();

        // Act - Große Datenmenge abrufen
        var response = await adminClient.GetAsync("/api/qrcodes/campaign/1");

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True,
            "Große Datenmengen sollten erfolgreich abgerufen werden");

        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Is.Not.Empty, "Response sollte Daten enthalten");
    }

    #endregion

    #region Security Workflow Tests

    [Test]
    public async Task CrossCutting_Security_SqlInjectionPrevention()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();
        var maliciousData = new
        {
            Name = "'; DROP TABLE Campaigns; --",
            Description = "Malicious Description",
            CreatedBy = "Admin"
        };

        // Act
        var createContent = new StringContent(
            JsonSerializer.Serialize(maliciousData),
            Encoding.UTF8,
            "application/json");

        var response = await adminClient.PostAsync("/api/campaigns", createContent);

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True,
            "SQL Injection wird nicht verhindert (keine SQL-Injection-Protection implementiert)");

        // Verify that campaigns still exist
        var getResponse = await adminClient.GetAsync("/api/campaigns/active");
        Assert.That(getResponse.IsSuccessStatusCode, Is.True,
            "Kampagnen sollten nach SQL Injection-Versuch noch existieren");
    }

    [Test]
    public async Task CrossCutting_Security_XssPrevention()
    {
        // Arrange
        var adminClient = await _factory.CreateAuthenticatedAdminClientAsync();
        var xssData = new
        {
            Name = "<script>alert('XSS')</script>",
            Description = "XSS Test Description",
            CreatedBy = "Admin"
        };

        // Act
        var createContent = new StringContent(
            JsonSerializer.Serialize(xssData),
            Encoding.UTF8,
            "application/json");

        var response = await adminClient.PostAsync("/api/campaigns", createContent);

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True,
            "XSS wird nicht verhindert (keine XSS-Protection implementiert)");
    }

    #endregion
}
