# Coding Guidelines - Easter Egg Hunt System

## 🎯 Allgemeine Prinzipien

Diese Guidelines gelten für das gesamte Easter Egg Hunt System (.NET Core Projekt) und sind von allen Entwicklern einzuhalten.

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

## 🧪 Test-Driven Development (TDD)

### Test Coverage Ziel
- **Minimum**: 90% Code Coverage
- **Ziel**: 100% Code Coverage soweit technisch möglich
- **Ausnahmen**: Nur nach expliziter Begründung und Team-Absprache

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
<PackageReference Include="FluentAssertions" Version="6.12.0" />
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

### Vor jedem Commit
- [ ] Alle Tests laufen durch (`dotnet test`)
- [ ] Code Coverage mindestens 90% (`dotnet test --collect:"XPlat Code Coverage"`)
- [ ] Keine Compiler Warnings
- [ ] Alle neuen Methoden haben deutsche XML-Dokumentation
- [ ] Variablen und Methoden auf Englisch
- [ ] Kommentare auf Deutsch

### Vor jedem Pull Request
- [ ] Feature Tests geschrieben (basierend auf Gherkin Features)
- [ ] Integration Tests für neue Endpoints
- [ ] Performance Tests bei kritischen Pfaden
- [ ] Security Tests bei Authentication/Authorization
- [ ] Dokumentation aktualisiert

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

Diese Guidelines sind verbindlich und werden bei jedem Code Review überprüft. Bei Fragen oder Unklarheiten bitte im Team besprechen.
