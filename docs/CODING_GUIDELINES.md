# Coding Guidelines - Easter Egg Hunt System

## 🎯 Allgemeine Prinzipien

Diese Guidelines gelten für das gesamte Easter Egg Hunt System (.NET Core Projekt) und sind von allen Entwicklern einzuhalten.

### Clean Code Prinzipien

Wir folgen strikt den **Clean Code** Prinzipien von Robert C. Martin:

#### 🏗️ Komponenten-Aufteilung
- **Single Responsibility Principle (SRP)**: Jede Klasse hat nur einen Grund zur Änderung
- **Separation of Concerns**: Verschiedene Aspekte in separate Komponenten trennen
- **Dependency Inversion**: Abhängigkeiten zu Abstraktionen, nicht zu Konkretionen
- **Interface Segregation**: Kleine, spezifische Interfaces statt große, monolithische

```csharp
// ✅ Korrekt - Klare Verantwortlichkeiten
public interface ICampaignRepository
{
    Task<Campaign> GetByIdAsync(int id);
    Task<Campaign> SaveAsync(Campaign campaign);
}

public interface ICampaignValidator
{
    ValidationResult Validate(Campaign campaign);
}

public class CampaignService
{
    private readonly ICampaignRepository _repository;
    private readonly ICampaignValidator _validator;
    
    // Klare, fokussierte Verantwortlichkeit
    public async Task<Campaign> CreateCampaignAsync(CreateCampaignRequest request)
    {
        // Validierung delegieren
        var validationResult = _validator.Validate(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);
            
        // Persistierung delegieren
        var campaign = new Campaign(request.Name, request.Description);
        return await _repository.SaveAsync(campaign);
    }
}
```

#### ♻️ Kontinuierliches Refactoring
- **Refactoring ist PFLICHT, nicht optional**
- **Red-Green-Refactor Zyklus** bei jedem Feature
- **Code Smells** sofort beseitigen
- **Maximale Methodenlänge**: 20 Zeilen
- **Maximale Klassenlänge**: 200 Zeilen
- **Maximale Parameter**: 3 pro Methode

```csharp
// ❌ Falsch - Zu lange Methode, zu viele Verantwortlichkeiten
public async Task<QrCodeDto> CreateQrCodeWithValidationAndLoggingAndNotification(
    string title, string note, int campaignId, string userEmail, bool sendEmail, 
    string logLevel, DateTime createdAt, string ipAddress)
{
    // 50+ Zeilen Code...
}

// ✅ Korrekt - Aufgeteilt in kleinere, fokussierte Methoden
public async Task<QrCodeDto> CreateQrCodeAsync(CreateQrCodeRequest request)
{
    await ValidateRequestAsync(request);
    var qrCode = await CreateQrCodeEntityAsync(request);
    await LogQrCodeCreationAsync(qrCode, request.UserContext);
    await SendNotificationIfRequestedAsync(qrCode, request.NotificationSettings);
    return _mapper.Map<QrCodeDto>(qrCode);
}
```

#### 🧪 Test-First Development (MANDATORY)
- **KEIN Code ohne Test** - Ausnahmslos!
- **Tests ZUERST schreiben** (TDD Red-Green-Refactor)
- **100% Test Coverage** ist das Ziel, 90% das Minimum
- **Tests sind First-Class Citizens** - genauso wichtig wie Produktionscode

```csharp
// ✅ Korrekt - Test zuerst schreiben
[Test]
public async Task CreateCampaign_WithValidData_ShouldReturnCampaignWithGeneratedId()
{
    // Arrange - Test definiert das gewünschte Verhalten
    var request = new CreateCampaignRequest("Ostern 2025", "Büro Hamburg");
    
    // Act - Noch nicht implementierte Methode aufrufen
    var result = await _campaignService.CreateCampaignAsync(request);
    
    // Assert - Erwartetes Verhalten definieren
    Assert.That(result, Is.Not.Null);
    Assert.That(result.Id, Is.GreaterThan(0));
    Assert.That(result.Name, Is.EqualTo("Ostern 2025"));
    Assert.That(result.CreatedAt, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(5)));
}

// Dann erst die Implementierung schreiben!
```

## 📝 Sprache und Kommentare

### Variablen und Code
- **Alle Variablen, Methoden, Klassen und Properties**: Englisch
- **Namespaces**: Englisch
- **Dateinamen**: Englisch
- **API Endpoints**: Englisch

### Kommentare und Dokumentation
- **Alle Kommentare**: Deutsch
- **XML-Dokumentation**: Deutsch
- **README und Dokumentation**: Deutsch
- **Commit Messages**: Deutsch
- **Exception Messages**: Deutsch (für interne Logs), Englisch (für API Responses)

```csharp
// ✅ Korrekt
public class CampaignService
{
    private readonly ICampaignRepository _campaignRepository;
    
    /// <summary>
    /// Erstellt eine neue Kampagne mit den angegebenen Parametern
    /// </summary>
    /// <param name="campaignName">Name der Kampagne</param>
    /// <returns>Die erstellte Kampagne</returns>
    public async Task<Campaign> CreateCampaignAsync(string campaignName)
    {
        // Validierung der Eingabeparameter
        if (string.IsNullOrWhiteSpace(campaignName))
        {
            throw new ArgumentException("Kampagnenname darf nicht leer sein", nameof(campaignName));
        }
        
        // Neue Kampagne erstellen und speichern
        var campaign = new Campaign { Name = campaignName };
        return await _campaignRepository.SaveAsync(campaign);
    }
}
```

## 🧪 Test-Driven Development (TDD) - MANDATORY

### Grundsätze (NICHT VERHANDELBAR)
- **JEDE Zeile Code MUSS getestet werden**
- **Tests werden ZUERST geschrieben** (Red-Green-Refactor)
- **Kein Merge ohne Tests** - Pull Requests werden abgelehnt
- **Refactoring nur mit grünen Tests**

### Test Coverage Ziel
- **Minimum**: 90% Code Coverage (Build schlägt fehl bei weniger)
- **Ziel**: 100% Code Coverage soweit technisch möglich
- **Ausnahmen**: Nur nach expliziter Begründung und Team-Absprache

### Clean Code in Tests
Tests müssen genauso sauber sein wie Produktionscode:

```csharp
// ❌ Falsch - Unklarer, langer Test
[Test]
public async Task Test1()
{
    var service = new CampaignService(new Mock<ICampaignRepository>().Object, 
        new Mock<ICampaignValidator>().Object, new Mock<ILogger<CampaignService>>().Object);
    var result = await service.CreateCampaignAsync(new CreateCampaignRequest("Test", "Test"));
    Assert.IsNotNull(result);
    Assert.AreEqual("Test", result.Name);
}

// ✅ Korrekt - Klarer, strukturierter Test
[Test]
public async Task CreateCampaign_WithValidRequest_ShouldReturnCampaignWithCorrectProperties()
{
    // Arrange - Testdaten und Mocks klar strukturiert
    var expectedCampaign = CampaignTestDataBuilder
        .New()
        .WithName("Ostern 2025")
        .WithDescription("Büro Hamburg")
        .Build();
        
    _mockRepository
        .Setup(r => r.SaveAsync(It.IsAny<Campaign>()))
        .ReturnsAsync(expectedCampaign);
        
    _mockValidator
        .Setup(v => v.Validate(It.IsAny<Campaign>()))
        .Returns(ValidationResult.Success);
        
    var request = new CreateCampaignRequest("Ostern 2025", "Büro Hamburg");
    
    // Act - Eine klare Aktion
    var result = await _campaignService.CreateCampaignAsync(request);
    
    // Assert - Spezifische, aussagekräftige Assertions
    Assert.That(result, Is.Not.Null);
    Assert.That(result.Name, Is.EqualTo("Ostern 2025"));
    Assert.That(result.Description, Is.EqualTo("Büro Hamburg"));
    Assert.That(result.CreatedAt, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(5)));
    
    // Verify - Interaktionen überprüfen
    _mockRepository.Verify(r => r.SaveAsync(It.IsAny<Campaign>()), Times.Once);
    _mockValidator.Verify(v => v.Validate(It.IsAny<Campaign>()), Times.Once);
}
```

### Test Struktur
```
src/
├── EasterEggHunt.Api/
├── EasterEggHunt.Core/
├── EasterEggHunt.Infrastructure/
└── EasterEggHunt.Web/

tests/
├── EasterEggHunt.Api.Tests/
├── EasterEggHunt.Core.Tests/
├── EasterEggHunt.Infrastructure.Tests/
└── EasterEggHunt.Web.Tests/
```

### Test Kategorien
1. **Unit Tests**: Einzelne Methoden und Klassen
2. **Integration Tests**: Datenbankzugriffe, externe Services
3. **End-to-End Tests**: Komplette User Journeys
4. **API Tests**: Controller und Endpoints

### Test Naming Convention
```csharp
// Pattern: MethodName_Scenario_ExpectedResult
[Test]
public void CreateCampaign_WithValidName_ShouldReturnCampaign()
{
    // Arrange - Vorbereitung der Testdaten
    
    // Act - Ausführung der zu testenden Methode
    
    // Assert - Überprüfung des Ergebnisses
}

[Test]
public void CreateCampaign_WithEmptyName_ShouldThrowArgumentException()
{
    // Test Implementation
}
```

## 🏗️ .NET Core Spezifische Guidelines

### Projekt Struktur
```
EasterEggHunt/
├── src/
│   ├── EasterEggHunt.Api/          # Web API
│   ├── EasterEggHunt.Core/         # Domain Models, Interfaces
│   ├── EasterEggHunt.Infrastructure/ # Data Access, External Services
│   └── EasterEggHunt.Web/          # MVC Frontend
├── tests/
├── docs/
└── scripts/
```

### Dependency Injection
```csharp
// ✅ Korrekt - Interface-basierte Abhängigkeiten
public class QrCodeService
{
    private readonly IQrCodeRepository _qrCodeRepository;
    private readonly ILogger<QrCodeService> _logger;
    
    public QrCodeService(
        IQrCodeRepository qrCodeRepository, 
        ILogger<QrCodeService> logger)
    {
        _qrCodeRepository = qrCodeRepository ?? throw new ArgumentNullException(nameof(qrCodeRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}
```

### Async/Await Pattern
```csharp
// ✅ Korrekt - Async Methoden enden mit "Async"
public async Task<List<Campaign>> GetActiveCampaignsAsync()
{
    // Immer ConfigureAwait(false) in Libraries verwenden
    return await _repository.GetActiveCampaignsAsync().ConfigureAwait(false);
}
```

### Error Handling
```csharp
// ✅ Korrekt - Spezifische Exceptions mit deutschen Nachrichten
public class CampaignNotFoundException : Exception
{
    public CampaignNotFoundException(int campaignId) 
        : base($"Kampagne mit ID {campaignId} wurde nicht gefunden")
    {
        CampaignId = campaignId;
    }
    
    public int CampaignId { get; }
}
```

## 🚨 Code Smells und Refactoring Triggers

### Wann MUSS refactoriert werden?

#### Sofortige Refactoring-Pflicht:
- **Methode > 20 Zeilen** → Aufteilen
- **Klasse > 200 Zeilen** → Single Responsibility prüfen
- **Mehr als 3 Parameter** → Parameter Object einführen
- **Duplizierter Code** → DRY Prinzip anwenden
- **Tiefe Verschachtelung (> 3 Ebenen)** → Guard Clauses verwenden
- **Magische Zahlen** → Konstanten definieren
- **Lange Parameterlisten** → Builder Pattern oder DTOs

```csharp
// ❌ Code Smell - Zu viele Parameter, zu lang
public async Task<QrCode> CreateQrCode(string title, string note, int campaignId, 
    string createdBy, DateTime createdAt, bool isActive, string description, 
    int sortOrder, string category, bool sendNotification)
{
    if (string.IsNullOrEmpty(title)) throw new ArgumentException("Title required");
    if (string.IsNullOrEmpty(note)) throw new ArgumentException("Note required");
    if (campaignId <= 0) throw new ArgumentException("Invalid campaign");
    if (string.IsNullOrEmpty(createdBy)) throw new ArgumentException("Creator required");
    
    var campaign = await _campaignRepository.GetByIdAsync(campaignId);
    if (campaign == null) throw new NotFoundException("Campaign not found");
    if (!campaign.IsActive) throw new InvalidOperationException("Campaign inactive");
    
    var qrCode = new QrCode
    {
        Title = title,
        Note = note,
        CampaignId = campaignId,
        CreatedBy = createdBy,
        CreatedAt = createdAt,
        IsActive = isActive,
        Description = description,
        SortOrder = sortOrder,
        Category = category
    };
    
    await _qrCodeRepository.SaveAsync(qrCode);
    
    if (sendNotification)
    {
        await _notificationService.SendQrCodeCreatedNotification(qrCode);
    }
    
    return qrCode;
}

// ✅ Nach Refactoring - Klare Struktur, Single Responsibility
public async Task<QrCode> CreateQrCodeAsync(CreateQrCodeRequest request)
{
    await _validator.ValidateAndThrowAsync(request);
    await _campaignValidator.EnsureCampaignIsActiveAsync(request.CampaignId);
    
    var qrCode = await _qrCodeFactory.CreateAsync(request);
    var savedQrCode = await _qrCodeRepository.SaveAsync(qrCode);
    
    await _notificationService.NotifyIfRequestedAsync(savedQrCode, request.NotificationSettings);
    
    return savedQrCode;
}
```

### Refactoring-Techniken (MANDATORY)

#### 1. Extract Method
```csharp
// ❌ Vorher - Alles in einer Methode
public async Task ProcessQrCodeScan(string qrCodeId, string userId)
{
    // Validierung (5 Zeilen)
    // QR-Code laden (3 Zeilen)  
    // Benutzer validieren (4 Zeilen)
    // Fund erstellen (6 Zeilen)
    // Statistiken aktualisieren (4 Zeilen)
    // Benachrichtigung senden (3 Zeilen)
}

// ✅ Nachher - Aufgeteilt in fokussierte Methoden
public async Task ProcessQrCodeScanAsync(string qrCodeId, string userId)
{
    await ValidateScanRequestAsync(qrCodeId, userId);
    var qrCode = await LoadAndValidateQrCodeAsync(qrCodeId);
    var user = await LoadAndValidateUserAsync(userId);
    var find = await CreateFindRecordAsync(qrCode, user);
    await UpdateStatisticsAsync(find);
    await SendNotificationAsync(find);
}
```

#### 2. Extract Class (bei > 200 Zeilen)
```csharp
// ❌ Zu große Klasse mit mehreren Verantwortlichkeiten
public class CampaignService
{
    // Campaign CRUD (50 Zeilen)
    // QR-Code Management (80 Zeilen)
    // Statistics (60 Zeilen)
    // Notifications (40 Zeilen)
}

// ✅ Aufgeteilt in fokussierte Services
public class CampaignService { /* Nur Campaign CRUD */ }
public class QrCodeService { /* Nur QR-Code Management */ }
public class CampaignStatisticsService { /* Nur Statistics */ }
public class CampaignNotificationService { /* Nur Notifications */ }
```

## 📋 Code Style Guidelines

### Naming Conventions
```csharp
// ✅ Korrekt
public class CampaignController : ControllerBase
{
    private readonly ICampaignService _campaignService;
    private const int MaxCampaignsPerUser = 10;
    
    public async Task<ActionResult<Campaign>> GetCampaignAsync(int campaignId)
    {
        var campaign = await _campaignService.GetByIdAsync(campaignId);
        return Ok(campaign);
    }
}

// ❌ Falsch
public class campaignController
{
    private ICampaignService campaignservice;
    private const int max_campaigns = 10;
    
    public ActionResult<Campaign> getcampaign(int id)
    {
        // ...
    }
}
```

### Properties und Fields
```csharp
// ✅ Korrekt
public class Campaign
{
    // Private fields mit Underscore
    private readonly List<QrCode> _qrCodes = new();
    
    // Public Properties PascalCase
    public string Name { get; set; }
    public DateTime CreatedAt { get; init; }
    
    // Public readonly Properties
    public IReadOnlyList<QrCode> QrCodes => _qrCodes.AsReadOnly();
}
```

## 🔧 Tools und Konfiguration

### Required NuGet Packages für Tests
```xml
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
<PackageReference Include="NUnit" Version="3.14.0" />
<PackageReference Include="NUnit3TestAdapter" Version="4.5.0" />
<PackageReference Include="Moq" Version="4.20.69" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
<PackageReference Include="coverlet.collector" Version="6.0.0" />
```

### EditorConfig (.editorconfig)
```ini
root = true

[*.cs]
indent_style = space
indent_size = 4
end_of_line = crlf
charset = utf-8
trim_trailing_whitespace = true
insert_final_newline = true

# .NET Code Style
dotnet_sort_system_directives_first = true
dotnet_separate_import_directive_groups = false

# C# Code Style
csharp_new_line_before_open_brace = all
csharp_new_line_before_else = true
csharp_new_line_before_catch = true
csharp_new_line_before_finally = true
```

### Test Coverage Konfiguration
```xml
<!-- Directory.Build.props -->
<Project>
  <PropertyGroup>
    <CollectCoverage>true</CollectCoverage>
    <CoverletOutputFormat>opencover</CoverletOutputFormat>
    <CoverletOutput>./coverage/</CoverletOutput>
    <Threshold>90</Threshold>
    <ThresholdType>line</ThresholdType>
    <ThresholdStat>minimum</ThresholdStat>
  </PropertyGroup>
</Project>
```

## 📊 Code Review Checklist

### Clean Code Checklist (MANDATORY)

#### Vor jedem Commit
- [ ] **Tests ZUERST geschrieben** (TDD Red-Green-Refactor befolgt)
- [ ] Alle Tests laufen durch (`dotnet test`)
- [ ] Code Coverage mindestens 90% (`dotnet test --collect:"XPlat Code Coverage"`)
- [ ] **Keine Methode > 20 Zeilen**
- [ ] **Keine Klasse > 200 Zeilen**
- [ ] **Maximal 3 Parameter pro Methode**
- [ ] **Keine Code-Duplikation**
- [ ] **Keine magischen Zahlen** (Konstanten verwenden)
- [ ] **Single Responsibility Principle** eingehalten
- [ ] Keine Compiler Warnings
- [ ] Alle neuen Methoden haben deutsche XML-Dokumentation
- [ ] Variablen und Methoden auf Englisch
- [ ] Kommentare auf Deutsch

#### Refactoring Checklist
- [ ] **Code Smells identifiziert und beseitigt**
- [ ] **Lange Methoden aufgeteilt** (Extract Method)
- [ ] **Große Klassen aufgeteilt** (Extract Class)
- [ ] **Parameter Objects** für lange Parameterlisten
- [ ] **Guard Clauses** statt tiefer Verschachtelung
- [ ] **Dependency Injection** korrekt verwendet
- [ ] **Interface Segregation** beachtet

#### Test Quality Checklist
- [ ] **Tests sind selbsterklärend** (kein Kommentar nötig)
- [ ] **Arrange-Act-Assert** Struktur befolgt
- [ ] **Test Data Builder** für komplexe Objekte
- [ ] **NUnit Assertions** für bessere Lesbarkeit
- [ ] **Mocks korrekt konfiguriert** und verifiziert
- [ ] **Edge Cases getestet**
- [ ] **Negative Tests** für Fehlerbehandlung

### Vor jedem Pull Request
- [ ] **Alle Clean Code Regeln befolgt**
- [ ] Feature Tests geschrieben (basierend auf Gherkin Features)
- [ ] Integration Tests für neue Endpoints
- [ ] Performance Tests bei kritischen Pfaden
- [ ] Security Tests bei Authentication/Authorization
- [ ] **Refactoring durchgeführt** wo nötig
- [ ] Dokumentation aktualisiert

### Code Review Kriterien (für Reviewer)
- [ ] **Clean Code Prinzipien eingehalten**
- [ ] **SOLID Prinzipien befolgt**
- [ ] **DRY Prinzip** (Don't Repeat Yourself)
- [ ] **YAGNI Prinzip** (You Aren't Gonna Need It)
- [ ] **Tests sind aussagekräftig** und vollständig
- [ ] **Naming ist selbsterklärend**
- [ ] **Keine Premature Optimization**
- [ ] **Error Handling** korrekt implementiert

## 🚀 Continuous Integration

### GitHub Actions / Azure DevOps Pipeline
```yaml
# Beispiel für Test Pipeline
- name: Run Tests
  run: dotnet test --configuration Release --collect:"XPlat Code Coverage" --results-directory ./coverage

- name: Check Coverage
  run: |
    dotnet tool install -g dotnet-reportgenerator-globaltool
    reportgenerator -reports:"coverage/**/coverage.cobertura.xml" -targetdir:"coverage/report" -reporttypes:"Html;Cobertura"
    
- name: Fail if coverage below 90%
  run: |
    # Script to check if coverage is below 90% and fail build
```

## 🎯 Spezielle Regeln für Easter Egg Hunt System

### Domain Models
```csharp
// ✅ Korrekt - Deutsche Kommentare, englische Properties
public class Campaign
{
    /// <summary>
    /// Eindeutige ID der Kampagne
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Name der Kampagne (wird öffentlich angezeigt)
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Zeitpunkt der Erstellung
    /// </summary>
    public DateTime CreatedAt { get; init; }
    
    /// <summary>
    /// Liste aller QR-Codes in dieser Kampagne
    /// </summary>
    public virtual ICollection<QrCode> QrCodes { get; set; } = new List<QrCode>();
}
```

### API Controllers
```csharp
[ApiController]
[Route("api/[controller]")]
public class CampaignsController : ControllerBase
{
    /// <summary>
    /// Erstellt eine neue Kampagne
    /// </summary>
    /// <param name="request">Kampagnen-Erstellungsanfrage</param>
    /// <returns>Die erstellte Kampagne</returns>
    [HttpPost]
    public async Task<ActionResult<CampaignDto>> CreateCampaignAsync([FromBody] CreateCampaignRequest request)
    {
        // Implementation mit deutschen Kommentaren
    }
}
```

## ⚡ Clean Code Enforcement

### Automatische Überprüfung
- **Build schlägt fehl** bei Code Coverage < 90%
- **Pull Requests werden automatisch abgelehnt** bei:
  - Fehlenden Tests
  - Code Smells (lange Methoden/Klassen)
  - Compiler Warnings
  - Sicherheitslücken

### Team-Verantwortung
- **Jeder Entwickler** ist für Clean Code verantwortlich
- **Code Reviews** sind MANDATORY - kein Merge ohne Review
- **Pair Programming** bei komplexen Features empfohlen
- **Refactoring Sessions** regelmäßig im Team

### Konsequenzen bei Nicht-Einhaltung
1. **Erste Warnung**: Coaching und Pair Programming
2. **Zweite Warnung**: Zusätzliche Code Review Runden
3. **Dritte Warnung**: Eskalation an Team Lead

## 📚 Empfohlene Literatur

### Pflichtlektüre für alle Entwickler:
- **"Clean Code"** von Robert C. Martin
- **"Refactoring"** von Martin Fowler
- **"Test Driven Development"** von Kent Beck
- **"Clean Architecture"** von Robert C. Martin

### Online-Ressourcen:
- [Clean Code Cheat Sheet](https://www.planetgeek.ch/wp-content/uploads/2014/11/Clean-Code-V2.4.pdf)
- [SOLID Principles](https://www.digitalocean.com/community/conceptual_articles/s-o-l-i-d-the-first-five-principles-of-object-oriented-design)
- [Refactoring Guru](https://refactoring.guru/)

## 🎓 Praktische Lektionen aus der Entwicklung

### API-Client-Endpoint-Konsistenz

**Kritisch:** API-Client und Controller müssen identische Endpoints verwenden

```csharp
// ❌ Häufiger Fehler - Inkonsistente Endpoints
// Controller: [HttpGet("by-code/{code}")]
// Client:     "/api/qrcodes/code/{code}"  // Falsch!

// ✅ Korrekt - Konsistente Endpoints
// Controller: [HttpGet("by-code/{code}")]
// Client:     "/api/qrcodes/by-code/{code}"  // Richtig!
```

**Prävention:**
- Endpoint-URLs in Konstanten definieren
- Automatische Tests für alle API-Client-Aufrufe
- Swagger-Dokumentation als Referenz verwenden

### Test-Coverage für kritische User-Flows

**Kritisch:** Alle User-Interaktionen müssen Unit Tests haben

```csharp
// ✅ Vollständige Test-Coverage für Controller-Actions
[TestFixture]
public class QrCodeScanTests
{
    // Happy Path
    [Test] public async Task ScanQrCode_WithValidCode_ReturnsScanResultView()
    
    // Edge Cases
    [Test] public async Task ScanQrCode_WithInvalidCode_ReturnsInvalidQrCodeView()
    [Test] public async Task ScanQrCode_WithEmptyCode_ReturnsInvalidQrCodeView()
    [Test] public async Task ScanQrCode_WithNullCode_ReturnsInvalidQrCodeView()
    
    // Business Logic
    [Test] public async Task ScanQrCode_WithInactiveCampaign_ReturnsNoCampaignView()
    [Test] public async Task ScanQrCode_WithAlreadyFoundQrCode_ReturnsScanResultView()
    
    // Authentication
    [Test] public async Task ScanQrCode_WithUnauthenticatedUser_RedirectsToRegister()
}
```

**Prävention:**
- Unit Tests für alle Controller-Actions
- Edge Cases explizit testen
- Mock-basierte Tests für isolierte Tests
- Integration Tests für End-to-End-Szenarien

### Service-Layer-Architektur

**Kritisch:** Controller sollten nicht zu groß werden

```csharp
// ❌ Anti-Pattern - Monolithischer Controller
public class AdminController : Controller
{
    // 500+ Zeilen Code
    // Direkte API-Aufrufe
    // Komplexe Business-Logik
    // Schwer testbar
}

// ✅ Clean Architecture - Service-Layer
public class AdminController : Controller
{
    private readonly ICampaignManagementService _campaignService;
    private readonly IQrCodeManagementService _qrCodeService;
    private readonly IStatisticsDisplayService _statisticsService;
    
    // Controller delegiert an Services
    public async Task<IActionResult> CampaignDetails(int id)
    {
        var campaign = await _campaignService.GetCampaignByIdAsync(id);
        var qrCodes = await _qrCodeService.GetQrCodesByCampaignAsync(id);
        var statistics = await _statisticsService.GetCampaignStatisticsAsync(id);
        
        return View(new CampaignDetailsViewModel(qrCodes) { ... });
    }
}
```

**Prävention:**
- Controller nur für HTTP-Concerns
- Business-Logik in Service-Layer
- Dependency Injection für lose Kopplung
- Interface-basierte Services für Testbarkeit

### Strukturierte Fehlerbehandlung

**Kritisch:** Aussagekräftige Logs und spezifische Exception-Behandlung

```csharp
// ✅ Korrekte Fehlerbehandlung mit strukturierten Logs
public async Task<IActionResult> ScanQrCode(string? code)
{
    try
    {
        _logger.LogInformation("Registrierter Mitarbeiter scannt QR-Code: {Code}", code);
        
        var qrCode = await _apiClient.GetQrCodeByCodeAsync(code);
        if (qrCode == null)
        {
            _logger.LogWarning("QR-Code mit Code '{Code}' nicht gefunden", code);
            return View("InvalidQrCode");
        }
        
        // ... weitere Logik
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "Fehler beim Abrufen des QR-Codes: {Code}", code);
        return View("Error");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unerwarteter Fehler beim Scannen des QR-Codes: {Code}", code);
        return View("Error");
    }
}
```

**Prävention:**
- Strukturierte Logs mit Parametern (`{Code}` statt String-Interpolation)
- Spezifische Exception-Typen abfangen
- Kontextuelle Informationen in Logs
- Log-Levels korrekt verwenden (Information, Warning, Error)

---

**Diese Guidelines sind VERBINDLICH und werden bei jedem Code Review strikt überprüft. Clean Code ist nicht optional - es ist die Grundlage für wartbaren, testbaren und erweiterbaren Code.**

Bei Fragen oder Unklarheiten bitte im Team besprechen. **Unwissenheit schützt nicht vor Code Review Ablehnung!** 😉
