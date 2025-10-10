# 🤖 KI-Agent Rules - Easter Egg Hunt Projekt

## 📋 Projekt-Kontext

**Easter Egg Hunt** ist ein internes Firmen-Spiel mit Clean Architecture (.NET Core 8.0, SQLite, Docker). Mitarbeiter scannen QR-Codes im Büro.

## 🚨 Kritische Regeln

### ⚠️ **NIEMALS ohne Tests committen!**
```bash
# IMMER vor jedem Commit ausführen:
dotnet test --verbosity minimal
```
**Warum:** Strikte Code Coverage Requirements (80% Domain, 60% Infrastructure). Commits ohne Tests führen zu Pipeline-Fehlern.

### ⚠️ **Git Hooks sind aktiv!**
Pre-Commit und Pre-Push Hooks führen `dotnet format` aus:
- **Pre-Commit:** Verhindert Commits mit Formatierungsfehlern
- **Pre-Push:** Zusätzliche Sicherheit vor Push

**Bei Formatierungsfehlern:**
```bash
dotnet format
git add .
git commit -m "deine nachricht"
```

### ⚠️ **Windows PowerShell Empfehlung**
Verwende **PowerShell** (`pwsh`) statt cmd.exe für bessere .NET-Integration und Pfad-Behandlung.

## 🛠️ Entwicklungsumgebung

### Setup
```bash
# Repository klonen
git clone https://github.com/mcnilz/easteregghunt.git
cd easteregghunt

# Docker-Umgebung starten
docker buildx bake
docker compose up -d

# Tests ausführen
dotnet test --verbosity minimal
```

### Projektstruktur
```
easteregghunt/
├── src/
│   ├── EasterEggHunt.Domain/          # Domain Entities & Interfaces
│   ├── EasterEggHunt.Application/     # Service Layer & Business Logic
│   ├── EasterEggHunt.Infrastructure/  # Data Access & External Services
│   ├── EasterEggHunt.Api/            # Web API Controllers
│   └── EasterEggHunt.Web/           # Frontend MVC
├── tests/                            # Unit & Integration Tests
├── docs/                             # Dokumentation
└── .github/workflows/               # CI/CD Pipeline
```

## 🧪 Testing-Strategie

### Test-Kategorien
1. **Domain Tests** (52 Tests) - Unit Tests für Entities
2. **Application Tests** (162 Tests) - Service Layer Tests mit Moq
3. **Infrastructure Tests** (116 Tests) - Integration Tests mit SQLite
4. **API Tests** (59 Tests) - Controller Tests mit WebApplicationFactory
5. **Web Tests** (27 Tests) - MVC Controller Tests mit Mocking
6. **Integration Tests** (78 Tests) - End-to-End Tests

### Code Coverage Requirements
- **Domain:** ≥ 80% (aktuell 87.7%) ✅
- **Infrastructure:** ≥ 60% (aktuell 88%) ✅
- **Application:** ≥ 80% (aktuell 94.66%) ✅

## 🔧 Häufige Probleme & Lösungen

### Problem 1: Code Coverage zu niedrig
**Symptom:** `dotnet test` schlägt fehl mit Coverage-Fehler
**Lösung:** 
```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
```

### Problem 2: Formatierungsfehler
**Symptom:** Pre-Commit Hook verhindert Commit
**Lösung:**
```bash
dotnet format
git add .
git commit -m "deine nachricht"
```

### Problem 3: Integration Tests schlagen fehl
**Symptom:** Tests mit SQLite-Datenbank schlagen fehl
**Lösung:** 
- Prüfe `IntegrationTestBase.cs` für Seed-Daten
- Stelle sicher, dass `SaveChangesAsync()` aufgerufen wird
- Verwende dynamische IDs statt hardcoded IDs

### Problem 4: Web-Projekt kann API-Projekt nicht referenzieren
**Symptom:** Build-Fehler "The type or namespace name 'Api' does not exist"
**Lösung:** 
- Kopiere DTOs in Web-Projekt statt direkter Referenz
- Verwende separate `ApiModels.cs` Dateien im Web-Projekt
- Vermeide zirkuläre Abhängigkeiten zwischen Projekten

### Problem 5: CA2227/CA1002 Code Analysis Warnings
**Symptom:** Collection Properties sollten read-only sein
**Lösung:**
```csharp
// Statt:
public List<T> Items { get; set; } = new();

// Verwende:
public IReadOnlyList<T> Items { get; set; } = new List<T>();
```

### Problem 6: MVC Controller Tests mit Extension Methods
**Symptom:** "Extension methods may not be used in setup/verification expressions"
**Lösung:**
- Mocke `IUrlHelperFactory` und `IUrlHelper` direkt
- Verwende `HttpContext.User` Mock statt Extension Methods
- Setze `TempData` direkt auf Controller statt über Factory

## 📚 Wichtige Dateien

### Konfiguration
- `Directory.Build.props` - Zentrale MSBuild-Einstellungen
- `.editorconfig` - Code-Formatierung
- `compose.yaml` - Docker Compose Konfiguration
- `docker-bake.hcl` - Docker Buildx Bake

### Dokumentation
- `docs/SPRINT_PLANNING.md` - Aktuelle Sprint-Planung
- `docs/ARCHITECTURE.md` - Architektur-Dokumentation
- `docs/DATABASE_SCHEMA.md` - Datenbank-Schema
- `CODING_GUIDELINES.md` - Coding-Standards

### Tests
- `tests/*/IntegrationTestBase.cs` - Basis für Integration Tests
- `tests/EasterEggHunt.Domain.Tests/` - Domain Entity Tests
- `tests/EasterEggHunt.Infrastructure.Tests/Integration/` - Repository Tests

## 🚀 Workflow für KI-Agenten

### 1. Vor jeder Änderung
```bash
# Aktuellen Status prüfen
git status
dotnet test --verbosity minimal
```

### 2. Während der Entwicklung
```bash
# Häufig testen
dotnet test --verbosity minimal

# Formatierung prüfen
dotnet format --verify-no-changes

# Build prüfen bei größeren Änderungen
dotnet build
```

### 3. Vor dem Commit
```bash
# Alle Tests ausführen
dotnet test --verbosity minimal

# Formatierung korrigieren falls nötig
dotnet format
git add .
git commit -m "feat: beschreibung der änderung"
```

### 3.1. Nach erfolgreicher Story-Umsetzung
**⚠️ WICHTIG:** Nach jeder erfolgreich implementierten Story IMMER:
1. **Akzeptanzkriterien in `docs/SPRINT_PLANNING.md` abhaken** `[ ]` → `[x]`
2. **Story-Status auf "✅ ABGESCHLOSSEN" setzen**
3. **Fortschritt in `AGENTS.md` aktualisieren**
4. **Nächste Meilensteine anpassen**

**Beispiel:**
```markdown
#### Story 3.1: Mitarbeiter-Registrierung ✅ ABGESCHLOSSEN
**Akzeptanzkriterien:**
- [x] Registrierungsformular beim ersten Scan
- [x] Name-Eingabe mit Validierung
- [x] Cookie-basierte Session
- [x] Mobile-optimierte UI
- [x] Fehlerbehandlung
- [x] 100% Test Coverage
```

### 4. Nach dem Push
- GitHub Actions Pipeline überwachen
- Bei Fehlern: Lokal reproduzieren und beheben

### 5. Best Practices für KI-Agenten
- **Immer Tests schreiben** vor der Implementierung (TDD)
- **Kleine, atomare Commits** mit klaren Nachrichten
- **Code Analysis Warnings** sofort beheben
- **Read-only Collections** verwenden (IReadOnlyList statt List)
- **Separation of Concerns** zwischen API und Web Projekten beachten
- **Mocking-Strategien** für MVC Controller Tests verstehen
- **⚠️ IMMER Akzeptanzkriterien abhaken** nach erfolgreicher Umsetzung in `SPRINT_PLANNING.md` und `AGENTS.md`

## 🔍 Debugging-Tipps

### Logs prüfen
```bash
# Docker Container Logs
docker compose logs -f

# Test-Ausgabe detailliert
dotnet test --verbosity detailed
```

### Coverage-Details
```bash
# Coverage-Report generieren
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
# Report in coverage/coverage.opencover.xml
```

### Datenbank-Status
```bash
# SQLite-Datenbank prüfen (falls lokal)
sqlite3 easteregghunt.db ".tables"
```

## ⚡ Quick Commands

```bash
# Projekt starten
docker compose up -d

# Tests ausführen
dotnet test --verbosity minimal

# Formatierung korrigieren
dotnet format

# Build prüfen
dotnet build --configuration Release

# Coverage generieren
dotnet test --collect:"XPlat Code Coverage"

# Git Status
git status
git log --oneline -5
```

## 🎯 Erfolgs-Kriterien

Ein KI-Agent hat erfolgreich onbordet, wenn:
- ✅ Alle Tests lokal durchlaufen (`dotnet test`) - **494 Tests**
- ✅ Code-Formatierung korrekt (`dotnet format --verify-no-changes`)
- ✅ Commits erfolgreich (`git commit` ohne Hook-Fehler)
- ✅ GitHub Actions Pipeline grün
- ✅ Verständnis der Clean Architecture
- ✅ Kenntnis der Test-Strategie
- ✅ **Code Coverage Requirements erfüllt** (Domain 87.7%, Infrastructure 88%, Application 94.66%)
- ✅ **MVC Controller Tests** mit korrektem Mocking verstehen
- ✅ **API/Web Projekt Separation** beachten (keine direkten Referenzen)

## 🏆 Bewährte Praktiken aus der Praxis

### Architektur-Erkenntnisse
- **Clean Architecture** funktioniert hervorragend für Testbarkeit
- **Repository Pattern** mit Integration Tests ist sehr robust
- **Service Layer** mit Dependency Injection macht Code wartbar
- **DTOs** zwischen API und Web Projekten vermeiden Abhängigkeiten

### Test-Strategien
- **Integration Tests** mit SQLite sind schnell und zuverlässig
- **WebApplicationFactory** für API Tests funktioniert perfekt
- **Mocking** für MVC Controller erfordert spezielle Techniken
- **Coverage Requirements** sind realistisch und erreichbar

### Entwicklungsworkflow
- **TDD** führt zu besserer Code-Qualität
- **Kleine Commits** machen Debugging einfacher
- **Formatierung** sollte automatisch erfolgen
- **Code Analysis** Warnings sofort beheben

---

**Letzte Aktualisierung:** Oktober 2025  
**Projekt-Status:** Sprint 2 - 100% abgeschlossen, Sprint 3 - Story 3.1 abgeschlossen  
**Nächste Meilensteine:** Sprint 3 - Story 3.2: QR-Code Scannen