using System.Text;
using System.Text.Json;
using EasterEggHunt.Integration.Tests.Helpers;
using NUnit.Framework;

namespace EasterEggHunt.Integration.Tests.Workflows;

/// <summary>
/// Integration Tests für komplette Mitarbeiter-Journeys
/// Basierend auf: features/employee_registration.feature, features/qr_code_scanning.feature, features/employee_progress.feature
/// </summary>
[TestFixture]
[Parallelizable(ParallelScope.Self)] // Parallele Ausführung für unabhängige Tests
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

    #endregion

    #region QR-Code Scanning Workflow Tests

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

    #endregion

    #region Progress Tracking Workflow Tests
    // Progress Tracking wird bereits in EmployeeWebFlowIntegrationTests.ProgressPage_CompleteFlow_Success abgedeckt
    #endregion

    #region Session Management Workflow Tests
    // Session Management wird bereits in CrossCuttingIntegrationTests abgedeckt
    #endregion

    #region Error Handling Workflow Tests
    // Error Handling wird bereits in CrossCuttingIntegrationTests und vollständigen Flows abgedeckt
    #endregion
}
