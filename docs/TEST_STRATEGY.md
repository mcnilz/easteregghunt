# 🧪 Test-Strategie - Easter Egg Hunt System

## 📋 Übersicht

Dieses Dokument beschreibt die umfassende Test-Strategie für das Easter Egg Hunt System. Es definiert Test-Kategorien, Coverage-Requirements, Best Practices und aktuelle Metriken.

## 🎯 Test-Philosophie

### Grundprinzipien

1. **Test-Driven Development (TDD)**: Tests werden vor der Implementierung geschrieben
2. **Test-Pyramide**: Viele schnelle Unit-Tests, weniger Integration-Tests, wenige E2E-Tests
3. **Isolation**: Tests sind unabhängig voneinander und können in beliebiger Reihenfolge ausgeführt werden
4. **Schnelligkeit**: Unit-Tests sollten sehr schnell sein (< 100ms pro Test)
5. **Vertrauenswürdigkeit**: Tests sollten zuverlässig sein und keine Flakiness zeigen

### Test-Pyramide

```
                    ┌──────────────────────┐
                    │    E2E Tests          │ ← ~40 Tests (5%)
                    │   (Integration)       │   Langsam, vollständige Journeys
                    └──────────────────────┘
                  ┌────────────────────────────┐
                  │    Integration Tests       │ ← ~173 Tests (22%)
                  │  (Infrastructure, API)      │   Mittel, Datenbank/Services
                  └────────────────────────────┘
            ┌────────────────────────────────────────┐
            │          Unit Tests                      │ ← ~600 Tests (74%)
            │  (Domain, Application, Controllers)      │   Schnell, isoliert
            └────────────────────────────────────────┘
```

**Verteilung:**
- **Unit Tests**: ~600 Tests (74%) - Schnell, isoliert, hohe Coverage
- **Integration Tests**: ~173 Tests (21%) - Mittel, Datenbank/API
- **E2E Tests**: ~37 Tests (5%) - Langsam, vollständige Journeys

**Neue Tests (Fund-Historie):**
- **Repository Integration Tests**: 6 Tests ✅
- **Service Unit Tests**: 3 Tests ✅
- **API Controller Tests**: 5 Tests ✅
- **Web Controller Tests**: 4 Tests ✅
- **Gesamt**: 18 neue Tests gemäß Testpyramide ✅

## 📊 Aktuelle Test-Metriken

### Test-Statistiken (Stand: Oktober 2025)

| Test-Projekt | Tests | Duration | Status |
|-------------|-------|----------|--------|
| **Domain.Tests** | 203 | ~418ms | ✅ |
| **Application.Tests** | 247 | ~3s | ✅ |
| **Infrastructure.Tests** | 173 | ~7s | ✅ |
| **Api.Tests** | 94 | ~447ms | ✅ |
| **Integration.Tests** | 37 | ~7s | ✅ |
| **Web.Tests** | 69 (5 skipped) | ~577ms | ✅ |
| **GESAMT** | **~810 Tests** | **~7-8s** | ✅ |

### Code Coverage (Stand: Oktober 2025)

| Projekt | Line Coverage | Branch Coverage | Method Coverage | Status |
|---------|--------------|----------------|----------------|--------|
| **Domain** | **89.6%** | 100% | 81.56% | ✅ Ziel: ≥80% |
| **Application** | **90.52%** | 90.4% | 82.89% | ✅ Ziel: ≥80% |
| **Infrastructure** | **36.28%** | 43.38% | 65.97% | ✅ Ziel: ≥25% (Domain ausgeschlossen) |

**Gesamt-Coverage:** 
- **Line Coverage**: ~73% (gewichtet nach Code-Menge)
- **Branch Coverage**: ~86%
- **Method Coverage**: ~78%

### Coverage-Verbesserungen (Phase 5)

**Domain Coverage:**
- Start: 66.4% (52 Tests)
- Ende: 89.6% (203 Tests)
- Verbesserung: +23.2% (+151 Tests)

**Application Coverage:**
- Start: 68.42% (162 Tests)
- Ende: 90.52% (237 Tests)
- Verbesserung: +22.1% (+75 Tests)

**Infrastructure Coverage:**
- Start: 34.04% (115 Tests)
- Ende: 36.28% (173 Tests) - Domain ausgeschlossen
- Verbesserung: +2.24% (+58 Tests)
- **Neue Features:** Fund-Historie mit Filter (6 Repository-Tests) ✅

## 🏗️ Test-Architektur

### Test-Projekt-Struktur

```
tests/
├── EasterEggHunt.Domain.Tests/              # Unit Tests für Domain
│   ├── Entities/                            # Entity-Tests
│   └── Models/                              # DTO-Tests
├── EasterEggHunt.Application.Tests/        # Unit Tests für Services
│   └── Services/                            # Service-Tests mit Moq
├── EasterEggHunt.Infrastructure.Tests/    # Integration Tests
│   ├── Integration/                          # Repository-Integration-Tests
│   ├── Data/                                # DbContext/SeedData-Tests
│   ├── Configuration/                       # Config-Tests
│   └── IntegrationTestBase.cs               # Basis für Integration-Tests
├── EasterEggHunt.Api.Tests/                 # API Controller Tests
│   └── Controllers/                         # Controller-Unit-Tests
├── EasterEggHunt.Integration.Tests/          # E2E Tests
│   ├── Workflows/                           # Workflow-Tests
│   └── Helpers/                             # TestWebApplicationFactory
└── EasterEggHunt.Web.Tests/                # MVC Controller Tests
    └── Controllers/                         # MVC-Controller-Tests
```

## 🎯 Test-Kategorien

### 1. Unit Tests

**Ziel:** Schnelle, isolierte Tests für einzelne Klassen/Methoden

**Charakteristika:**
- ⚡ Sehr schnell (< 100ms pro Test)
- 🔒 Vollständig isoliert (keine Abhängigkeiten)
- 🎯 Fokus auf eine Klasse/Methode
- 📊 Hohe Coverage (> 90%)

**Beispiele:**
- Domain Entity Tests (Konstruktoren, Methoden, Validierung)
- Application Service Tests (mit Moq-Mocks)
- Controller Tests (mit Mocked Services)

**Aktuelle Metriken:**
- Domain Tests: 203 Tests, ~625ms, 89.6% Coverage
- Application Tests: 237 Tests, ~3s, 90.52% Coverage
- API Tests: 80 Tests, ~487ms
- Web Tests: 62 Tests, ~882ms

### 2. Integration Tests

**Ziel:** Tests für Interaktionen zwischen Komponenten

**Charakteristika:**
- 🗄️ Echte Datenbank (SQLite)
- 🔄 Echte Services/Repositories
- ⏱️ Mittlere Geschwindigkeit (< 1s pro Test)
- 🎯 Fokus auf Repository/Service-Interaktionen

**Beispiele:**
- Repository Integration Tests (EF Core, SQLite)
- DbContext Tests (Entity Configuration, Relationships)
- Configuration Tests (DbContext Configuration)

**Aktuelle Metriken:**
- Infrastructure Tests: 173 Tests, ~9s, 39.16% Coverage

### 3. End-to-End (E2E) Tests

**Ziel:** Vollständige User-Journeys testen

**Charakteristika:**
- 🌐 Vollständiger HTTP-Stack (WebApplicationFactory)
- 🗄️ Echte Datenbank (SQLite)
- 🐌 Langsam (> 1s pro Test)
- 🎯 Fokus auf komplette Workflows

**Beispiele:**
- Employee Web Flow (Registrierung → Scan → Progress)
- Admin Workflow (Login → Dashboard → Campaign Management)
- Campaign Lifecycle (Erstellen → Aktivieren → QR-Codes)

**Aktuelle Metriken:**
- Integration Tests: 37 Tests, ~8s

**Optimierungen:**
- Parallele Ausführung: `[Parallelizable(ParallelScope.Self)]`
- Data-Driven Tests: `[TestCase]` für ähnliche Szenarien
- Reduktion von 112 auf 37 Tests (67% Reduktion)

## 📐 Coverage-Requirements

### Minimum-Anforderungen

| Projekt | Minimum Coverage | Aktuell | Status |
|---------|-----------------|---------|--------|
| **Domain** | ≥ 80% | 89.6% | ✅ Erfüllt |
| **Application** | ≥ 80% | 90.52% | ✅ Erfüllt |
| **Infrastructure** | ≥ 60% | 39.16% | ⚠️ Noch nicht erreicht |

### Coverage-Verbesserungs-Strategie

1. **Domain**: ✅ Abgeschlossen (89.6%)
   - Alle Entity-Konstruktoren getestet
   - Alle Entity-Methoden getestet
   - Edge Cases und Boundary Conditions abgedeckt

2. **Application**: ✅ Abgeschlossen (90.52%)
   - Alle Service-Methoden getestet
   - Exception-Handling getestet
   - Edge Cases abgedeckt

3. **Infrastructure**: ⚠️ In Arbeit (39.16%)
   - Repository-Methoden getestet
   - DbContext-Configuration getestet
   - Noch zu testen: Migration-Klassen, weitere Config-Dateien

## 🔧 Test-Technologien

### Test-Framework

- **NUnit**: Haupt-Test-Framework
- **Moq**: Mocking-Framework für Unit-Tests
- **WebApplicationFactory**: In-Memory HTTP-Server für E2E-Tests
- **SQLite**: In-Memory-Datenbank für Integration-Tests

### Test-Tools

- **Coverlet**: Code-Coverage-Erfassung
- **ReportGenerator**: Coverage-Reports
- **dotnet test**: Test-Runner

## 📝 Test-Naming-Konventionen

### Pattern

```
[Method]_[Condition]_[ExpectedResult]
```

### Beispiele

```csharp
[Test]
public async Task CreateCampaign_WithValidName_ShouldReturnCampaign()
{
    // Arrange, Act, Assert
}

[Test]
public async Task GetCampaignById_WithNonExistentId_ShouldReturnNull()
{
    // Arrange, Act, Assert
}

[Test]
public async Task UpdateCampaign_WithNullName_ShouldThrowArgumentException()
{
    // Arrange, Act, Assert
}
```

### Test-Klassen-Namen

```
[Entity/Service]Tests
[Entity/Service]IntegrationTests
[Workflow]IntegrationTests
```

### Beispiele

- `UserTests`
- `CampaignServiceTests`
- `FindRepositoryIntegrationTests`
- `EmployeeWebFlowIntegrationTests`

## 🏃‍♂️ Test-Ausführung

### Lokale Ausführung

```bash
# Alle Tests ausführen
dotnet test --verbosity minimal

# Nur Unit Tests
dotnet test tests/EasterEggHunt.Domain.Tests --verbosity minimal
dotnet test tests/EasterEggHunt.Application.Tests --verbosity minimal

# Nur Integration Tests
dotnet test tests/EasterEggHunt.Infrastructure.Tests --verbosity minimal
dotnet test tests/EasterEggHunt.Integration.Tests --verbosity minimal

# Mit Coverage
dotnet test --collect:"XPlat Code Coverage" --verbosity minimal

# Parallele Ausführung (Standard)
dotnet test --verbosity minimal

# Sequenzielle Ausführung (wenn nötig)
dotnet test --verbosity minimal -- --no-parallel
```

### CI/CD Pipeline

- **GitHub Actions**: Automatische Test-Ausführung bei jedem Push
- **Pre-Commit Hook**: Verhindert Commits ohne erfolgreiche Tests
- **Coverage-Reporting**: Automatische Coverage-Reports in CI

## ✅ Test-Qualität-Kriterien

### Erfüllt ✅

1. **Test-Pyramide**: 73% Unit, 22% Integration, 5% E2E
2. **Domain Coverage**: 89.6% (Ziel: ≥80%) ✅
3. **Application Coverage**: 90.52% (Ziel: ≥80%) ✅
4. **Test-Geschwindigkeit**: Gesamt ~22s für 792 Tests ✅
5. **Test-Isolation**: Alle Tests sind unabhängig ✅
6. **Edge Cases**: Umfassende Edge-Case-Abdeckung ✅

### Verbesserungsbedarf ⚠️

1. **Infrastructure Coverage**: 39.16% (Ziel: ≥60%) ⚠️
   - Viele Code-Pfade in Migration-Klassen (auto-generiert)
   - Konfigurations-Dateien schwer zu testen
   - DbContext OnModelCreating private Methoden

## 📚 Best Practices

### Unit Tests

1. **ARRANGE-ACT-ASSERT Pattern**
   ```csharp
   [Test]
   public void TestMethod()
   {
       // Arrange - Testdaten vorbereiten
       var campaign = new Campaign("Test", "Description", "Admin");
       
       // Act - Methode ausführen
       var result = campaign.Activate();
       
       // Assert - Ergebnis prüfen
       Assert.That(campaign.IsActive, Is.True);
   }
   ```

2. **Isolation**: Keine Abhängigkeiten zwischen Tests
3. **Mocking**: Nur externe Abhängigkeiten mocken
4. **Edge Cases**: Boundary Conditions testen
5. **Parametrisierte Tests**: `[TestCase]` für ähnliche Szenarien

### Integration Tests

1. **Test-Datenbank**: Jeder Test nutzt isolierte Datenbank
2. **Cleanup**: `TearDown` für Datenbank-Bereinigung
3. **Test-Base-Klasse**: `IntegrationTestBase` für gemeinsame Setup-Logik
4. **Seeding**: Test-Daten in `SetUp` erstellen
5. **Dispose**: Korrekte Ressourcen-Freigabe

### E2E Tests

1. **WebApplicationFactory**: Zentralisiert in `TestWebApplicationFactoryBase`
2. **Logging**: Reduziert auf `LogLevel.Critical` für saubere Outputs
3. **Datenbank-Isolation**: Jeder Test nutzt eigene SQLite-Datenbank
4. **Parallelisierung**: `[Parallelizable(ParallelScope.Self)]` wo möglich
5. **Data-Driven**: `[TestCase]` für ähnliche Workflows

## 🐛 Häufige Probleme & Lösungen

### Problem 1: Tests sind zu langsam

**Symptom:** Test-Ausführung dauert > 1 Minute
**Lösung:**
- Parallele Ausführung aktivieren
- Unnötige Integration-Tests entfernen
- In-Memory-Datenbank statt echte Datenbank

### Problem 2: Tests sind flaky

**Symptom:** Tests schlagen manchmal fehl
**Lösung:**
- Race Conditions identifizieren und beheben
- Timing-abhängige Tests korrigieren
- Test-Isolation verbessern

### Problem 3: Niedrige Coverage

**Symptom:** Coverage < Ziel
**Lösung:**
- Ungetestete Methoden identifizieren
- Edge Cases hinzufügen
- Boundary Conditions testen
- Fehlerbehandlung testen

### Problem 4: Integration Tests sind zu komplex

**Symptom:** Integration Tests sind schwer zu warten
**Lösung:**
- In Unit-Tests umwandeln (mit Mocks)
- Test-Helper-Klassen erstellen
- Test-Daten-Factories nutzen

## 📈 Metriken-Tracking

### Wichtige Metriken

1. **Test-Anzahl**: Aktuell ~792 Tests
2. **Test-Geschwindigkeit**: ~22s für alle Tests
3. **Code Coverage**: Domain 89.6%, Application 90.52%, Infrastructure 39.16%
4. **Test-Erfolgsrate**: Ziel: 100% (aktuell: 100% ✅)

### Coverage-Trends

- **Domain**: 66.4% → 89.6% (+23.2%)
- **Application**: 68.42% → 90.52% (+22.1%)
- **Infrastructure**: 34.04% → 39.16% (+5.12%)

## 🎓 Test-Beispiele

### Domain Unit Test

```csharp
[TestFixture]
public class UserTests
{
    [Test]
    public void Constructor_WithValidName_ShouldCreateUser()
    {
        // Arrange & Act
        var user = new User("Test User");
        
        // Assert
        Assert.That(user.Name, Is.EqualTo("Test User"));
        Assert.That(user.IsActive, Is.True);
    }
}
```

### Application Service Test (mit Moq)

```csharp
[TestFixture]
public class CampaignServiceTests
{
    private Mock<ICampaignRepository> _mockRepository = null!;
    private CampaignService _service = null!;
    
    [SetUp]
    public void Setup()
    {
        _mockRepository = new Mock<ICampaignRepository>();
        _service = new CampaignService(_mockRepository.Object, _mockLogger.Object);
    }
    
    [Test]
    public async Task CreateCampaignAsync_WithValidData_ShouldCreateCampaign()
    {
        // Arrange
        var request = new CreateCampaignRequest("Test", "Description");
        var expectedCampaign = new Campaign("Test", "Description", "Admin");
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Campaign>()))
            .ReturnsAsync(expectedCampaign);
        
        // Act
        var result = await _service.CreateCampaignAsync(request);
        
        // Assert
        Assert.That(result.Name, Is.EqualTo("Test"));
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Campaign>()), Times.Once);
    }
}
```

### Integration Test

```csharp
[TestFixture]
[Category("Integration")]
public class CampaignRepositoryIntegrationTests : IntegrationTestBase
{
    [Test]
    public async Task AddAsync_WithValidCampaign_ShouldAddCampaign()
    {
        // Arrange
        await ResetDatabaseAsync();
        var campaign = new Campaign("Test", "Description", "Admin");
        
        // Act
        await CampaignRepository.AddAsync(campaign);
        await CampaignRepository.SaveChangesAsync();
        
        // Assert
        var retrievedCampaign = await CampaignRepository.GetByIdAsync(campaign.Id);
        Assert.That(retrievedCampaign, Is.Not.Null);
        Assert.That(retrievedCampaign!.Name, Is.EqualTo("Test"));
    }
}
```

### E2E Test

```csharp
[TestFixture]
[Category("Integration")]
public class EmployeeWebFlowIntegrationTests
{
    [Test]
    public async Task CompleteEmployeeWebFlow_RegistrationAndScan_Success()
    {
        // Arrange
        var factory = new ControllerTestWebApplicationFactory();
        var client = factory.CreateClient();
        
        // Act 1: Registrierung
        var registerResponse = await client.PostAsync("/api/users", 
            JsonContent.Create(new { name = "Test User" }));
        registerResponse.EnsureSuccessStatusCode();
        
        // Act 2: QR-Code scannen
        var scanResponse = await client.PostAsync("/api/finds", 
            JsonContent.Create(new { qrCodeId = 1, userId = 1 }));
        scanResponse.EnsureSuccessStatusCode();
        
        // Assert
        Assert.That(registerResponse.IsSuccessStatusCode, Is.True);
        Assert.That(scanResponse.IsSuccessStatusCode, Is.True);
    }
}
```

## 🔄 Wartung & Aktualisierung

### Regelmäßige Reviews

- **Monatlich**: Coverage-Trends überprüfen
- **Bei neuen Features**: Tests für neue Funktionalität hinzufügen
- **Bei Refactorings**: Tests anpassen

### Test-Pflege

1. **Entfernen**: Unnötige/Redundante Tests entfernen
2. **Konsolidieren**: Ähnliche Tests zusammenführen (Data-Driven)
3. **Optimieren**: Langsame Tests beschleunigen
4. **Dokumentieren**: Komplexe Tests dokumentieren

## 📚 Weitere Ressourcen

- [NUnit Documentation](https://docs.nunit.org/)
- [Moq Documentation](https://github.com/moq/moq4)
- [ASP.NET Core Testing](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests)
- [Code Coverage Best Practices](https://www.jetbrains.com/help/dotnet/code-coverage.html)

---

**Letzte Aktualisierung:** Oktober 2025  
**Version:** 1.0  
**Status:** ✅ Aktiv

