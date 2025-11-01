# Repository-Tests Analyse - Phase 1.1

## Zusammenfassung

**Integration.Tests/Repositories**: 74 Tests insgesamt
**Infrastructure.Tests/Integration**: 100 Tests insgesamt

## Identifizierte Duplikate

### 1. CampaignRepositoryIntegrationTests
- **Integration.Tests/Repositories**: 12 Tests (nur CRUD über Context)
- **Infrastructure.Tests/Integration**: 12 Tests (CRUD über Repository)
- **Entscheidung**: Integration.Tests/Repositories löschen (redundante CRUD-Tests)

### 2. FindRepositoryIntegrationTests
- **Integration.Tests/Repositories**: 17 Tests (nur CRUD über Context)
- **Infrastructure.Tests/Integration**: 17 Tests (CRUD über Repository)
- **Entscheidung**: Integration.Tests/Repositories löschen (redundante CRUD-Tests)

### 3. QrCodeRepositoryIntegrationTests
- **Integration.Tests/Repositories**: 10 Tests (nur CRUD über Context)
- **Infrastructure.Tests/Integration**: 13 Tests (CRUD über Repository)
- **Entscheidung**: Integration.Tests/Repositories löschen (redundante CRUD-Tests)

### 4. SessionRepositoryIntegrationTests
- **Integration.Tests/Repositories**: 19 Tests (nur CRUD über Context)
- **Infrastructure.Tests/Integration**: 17 Tests (CRUD über Repository)
- **Entscheidung**: Integration.Tests/Repositories löschen (redundante CRUD-Tests)

### 5. AdminRepositoryIntegrationTests
- **Integration.Tests/Repositories**: 4 Tests (nur CRUD über Context, plus Datenbank-Prüfung)
- **Infrastructure.Tests/Integration**: 19 Tests in AdminUserRepositoryIntegrationTests (CRUD über Repository)
- **Entscheidung**: Integration.Tests/Repositories löschen (redundante CRUD-Tests)

### 6. UserRepositoryIntegrationTests
- **Integration.Tests/Repositories**: 12 Tests (workflow-spezifisch, z.B. `EmployeeRegistration_CompleteFlow_Success`)
- **Infrastructure.Tests/Integration**: 16 Tests (CRUD über Repository)
- **Entscheidung**: **BEIBEHALTEN** (workflow-spezifische Tests sind wertvoll)

## Zu löschende Dateien

1. `tests/EasterEggHunt.Integration.Tests/Repositories/CampaignRepositoryIntegrationTests.cs` (12 Tests)
2. `tests/EasterEggHunt.Integration.Tests/Repositories/FindRepositoryIntegrationTests.cs` (17 Tests)
3. `tests/EasterEggHunt.Integration.Tests/Repositories/QrCodeRepositoryIntegrationTests.cs` (10 Tests)
4. `tests/EasterEggHunt.Integration.Tests/Repositories/SessionRepositoryIntegrationTests.cs` (19 Tests)
5. `tests/EasterEggHunt.Integration.Tests/Repositories/AdminRepositoryIntegrationTests.cs` (4 Tests)

**Gesamt zu löschende Tests**: 62 Tests

## Zu behaltende Dateien

1. `tests/EasterEggHunt.Integration.Tests/Repositories/UserRepositoryIntegrationTests.cs` (12 Tests - workflow-spezifisch)

## Begründung

- **Infrastructure.Tests/Integration** testet Repository CRUD-Operationen korrekt über Repository-Interface (Unit-ähnliche Integration Tests)
- **Integration.Tests/Repositories** testet CRUD direkt über DbContext, was redundante Tests sind
- **Integration.Tests/Repositories/UserRepositoryIntegrationTests** enthält wertvolle workflow-spezifische Tests (z.B. Employee Registration Flow), die beibehalten werden sollten
- Alle anderen Tests in Integration.Tests/Repositories sind reine CRUD-Tests und werden bereits in Infrastructure.Tests/Integration abgedeckt

## Erwartete Reduktion

- **Vorher**: 174 Tests in Integration.Tests/Repositories
- **Nachher**: 12 Tests (nur UserRepositoryIntegrationTests)
- **Reduktion**: 162 Tests entfernt

**Hinweis**: Die tatsächliche Reduktion kann geringer sein, da andere Test-Dateien in Integration.Tests existieren (z.B. Workflows, Controllers).

