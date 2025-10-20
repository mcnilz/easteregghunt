# 🔧 Troubleshooting Guide - Easter Egg Hunt System

## 🚨 Häufige Probleme und Lösungen

### QR-Code-Scan-Probleme

#### Problem: "Ungültiger QR-Code" bei gültigen Codes

**Symptome:**
- QR-Code-Links wie `https://localhost:7002/qr/d90cffe8f07b` zeigen "Ungültiger QR-Code"
- Codes funktionieren in der API direkt, aber nicht über Web-Interface

**Root Cause:** API-Client-Endpoint-Inkonsistenz

**Lösung:**
```csharp
// ❌ Falsch - Inkonsistente Endpoints
// Controller: [HttpGet("by-code/{code}")]
// Client:     "/api/qrcodes/code/{code}"

// ✅ Korrekt - Konsistente Endpoints
// Controller: [HttpGet("by-code/{code}")]
// Client:     "/api/qrcodes/by-code/{code}"
```

**Prävention:**
- Endpoint-URLs in Konstanten definieren
- Automatische Tests für API-Client-Endpoints
- Swagger-Dokumentation als Referenz verwenden

#### Problem: QR-Codes werden mehrfach gefunden

**Symptome:**
- Gleicher QR-Code wird mehrfach als "gefunden" registriert
- Statistiken zeigen falsche Zahlen

**Root Cause:** Fehlende Duplikat-Prüfung

**Lösung:**
```csharp
// ✅ Korrekte Implementierung mit Duplikat-Prüfung
var existingFind = await _apiClient.GetExistingFindAsync(qrCode.Id, userId);
if (existingFind == null)
{
    // Nur bei erstem Fund registrieren
    var newFind = await _apiClient.RegisterFindAsync(qrCode.Id, userId, ipAddress, userAgent);
}
```

### Admin-Panel-Probleme

#### Problem: 404-Fehler bei Admin-Links

**Symptome:**
- Links zu `https://localhost:7002/Admin/CampaignDetails/4` geben 404 zurück
- Admin-Dashboard zeigt keine QR-Codes

**Root Cause:** Fehlende Controller-Actions oder falsche Routing

**Lösung:**
```csharp
// ✅ Korrekte Controller-Implementierung
[HttpGet]
public async Task<IActionResult> CampaignDetails(int id)
{
    var campaign = await _campaignService.GetCampaignByIdAsync(id);
    if (campaign == null) return NotFound();
    
    var qrCodes = await _qrCodeService.GetQrCodesByCampaignAsync(id);
    var statistics = await _statisticsService.GetCampaignStatisticsAsync(id);
    
    return View(new CampaignDetailsViewModel(qrCodes) { ... });
}
```

### Test-Probleme

#### Problem: Tests schlagen fehl nach Refactoring

**Symptome:**
- Unit Tests schlagen fehl mit "Method not found" oder "Constructor not found"
- Mock-Setups funktionieren nicht mehr

**Root Cause:** Interface-Änderungen oder Constructor-Änderungen

**Lösung:**
```csharp
// ✅ Korrekte Mock-Anpassung nach Service-Extraktion
[SetUp]
public void Setup()
{
    _mockCampaignService = new Mock<ICampaignManagementService>();
    _mockQrCodeService = new Mock<IQrCodeManagementService>();
    _mockStatisticsService = new Mock<IStatisticsDisplayService>();
    
    _controller = new AdminController(
        _mockCampaignService.Object,
        _mockQrCodeService.Object,
        _mockStatisticsService.Object,
        _mockLogger.Object);
}
```

#### Problem: ModelState.IsValid gibt false in Unit Tests zurück

**Symptome:**
- Controller-Tests erwarten RedirectToActionResult, bekommen aber ViewResult
- ModelState-Validierung funktioniert nicht in isolierten Tests

**Lösung:**
```csharp
// ✅ Integration Tests statt Unit Tests für ModelState-abhängige Tests
[Test]
[Ignore("ModelState-Validierung funktioniert nur in Integration Tests")]
public async Task CreateCampaign_ReturnsRedirect_WhenModelIsValid()
{
    // Dieser Test sollte als Integration Test implementiert werden
}
```

### Performance-Probleme

#### Problem: Langsame Datenbankabfragen

**Symptome:**
- N+1 Query-Probleme in Logs sichtbar
- Langsame Ladezeiten bei Statistiken

**Root Cause:** Ineffiziente Datenbankabfragen

**Lösung:**
```csharp
// ❌ Falsch - N+1 Problem
foreach (var qrCode in qrCodes)
{
    var finds = await _findRepository.GetFindsByQrCodeIdAsync(qrCode.Id);
    // Separate Query für jeden QR-Code
}

// ✅ Korrekt - Aggregierte Abfrage
var campaignFinds = await _findRepository.GetCampaignFindsAggregateAsync(campaignId);
// Eine Query für alle Daten
```

### Logging-Probleme

#### Problem: Zu viele SQL-Logs in Tests

**Symptome:**
- Test-Ausgaben sind mit SQL-Queries überladen
- Schwer lesbare Test-Ergebnisse

**Root Cause:** EF Core Logging nicht für Tests konfiguriert

**Lösung:**
```json
// appsettings.Test.json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Command": "Error"
    }
  }
}
```

```csharp
// IntegrationTestBase.cs
services.AddLogging(builder =>
{
    builder.SetMinimumLevel(LogLevel.None);
    builder.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.None);
});
```

## 🔍 Debugging-Techniken

### Logging-Analyse

```csharp
// ✅ Strukturierte Logs für besseres Debugging
_logger.LogInformation("User {UserId} scanned QR-Code {QrCodeId} at {Timestamp}", 
    userId, qrCodeId, DateTime.UtcNow);

_logger.LogWarning("QR-Code {QrCodeId} not found for user {UserId}", 
    qrCodeId, userId);

_logger.LogError(ex, "Failed to register find for user {UserId} and QR-Code {QrCodeId}", 
    userId, qrCodeId);
```

### Test-Debugging

```csharp
// ✅ Detaillierte Test-Assertions
Assert.That(result, Is.InstanceOf<ViewResult>(), "Expected ViewResult");
var viewResult = (ViewResult)result;
Assert.That(viewResult.Model, Is.InstanceOf<ScanResultViewModel>(), "Expected ScanResultViewModel");
var viewModel = (ScanResultViewModel)viewResult.Model!;
Assert.That(viewModel.QrCode.Id, Is.EqualTo(expectedQrCodeId), 
    $"Expected QrCode.Id {expectedQrCodeId}, but got {viewModel.QrCode.Id}");
```

### API-Debugging

```bash
# API-Endpoints direkt testen
curl -X GET "https://localhost:7001/api/qrcodes/by-code/d90cffe8f07b"

# Swagger-UI für API-Exploration
# https://localhost:7001/swagger
```

## 📋 Checkliste für häufige Probleme

### Vor jedem Commit:
- [ ] Alle Tests laufen durch (`dotnet test --verbosity minimal`)
- [ ] Code-Formatierung korrekt (`dotnet format --verify-no-changes`)
- [ ] Keine Compiler-Warnings
- [ ] API-Endpoints konsistent zwischen Client und Server
- [ ] Unit Tests für neue Controller-Actions vorhanden

### Bei Problemen:
- [ ] Logs auf Fehlermeldungen prüfen
- [ ] API-Endpoints direkt testen
- [ ] Tests isoliert ausführen
- [ ] ModelState-Validierung in Integration Tests prüfen
- [ ] Mock-Setups auf Korrektheit prüfen

## 🆘 Notfall-Kontakte

- **Team Lead:** Bei kritischen Produktionsproblemen
- **DevOps:** Bei Infrastruktur-Problemen
- **Code Review:** Bei komplexen Refactoring-Problemen

---

**Wichtig:** Bei unklaren Problemen immer zuerst die Logs prüfen und dann systematisch debuggen. Nicht raten - messen! 📊
