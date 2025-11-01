# Code Coverage Analyse - Phase 5.1

## Zusammenfassung

**Aktuelle Coverage-Werte** (Stand: nach Phase 4):
- **Domain**: 66.4% (Ziel: ≥ 80%) ❌ - **Fehlt 13.6%**
- **Application**: 68.42% (Ziel: ≥ 80%) ❌ - **Fehlt 11.58%**
- **Infrastructure**: 34.04% (Ziel: ≥ 60%) ❌ - **Fehlt 25.96%**

## Domain Coverage Analyse (66.4% → 80% benötigt: +13.6%)

### Ungetestete Bereiche

#### 1. Statistics Models (0% Coverage)
- ✅ **Priorität**: Mittel (DTOs für Statistiken)
- **Datei**: `src/EasterEggHunt.Domain/Models/StatisticsModels.cs`
- **Klassen**:
  - `SystemOverviewStatistics` - 0% Coverage
  - `CampaignStatistics` - 0% Coverage
  - `UserStatistics` - 0% Coverage
  - `QrCodeStatisticsDto` - 0% Coverage
  - `QrCodeStatisticsRecentFind` - 0% Coverage
  - `CampaignQrCodeStatisticsDto` - 0% Coverage
  - `TopPerformersStatistics` - 0% Coverage
- **Aktion**: DTOs sind einfache POCOs, könnten durch Integration Tests abgedeckt werden
- **Empfehlung**: Coverage für DTOs ist optional (niedrige Priorität)

#### 2. Entity-Methoden (Teilweise ungetestet)
- ✅ **Priorität**: Hoch (Business-Logik)
- **Bereiche**:
  - Edge Cases in Entity-Konstruktoren
  - Komplexere Methoden mit mehreren Branch-Pfaden
  - Property-Setter und Initialisierung
- **Aktion**: Erweitere Entity Tests um zusätzliche Edge Cases
- **Geschätzte Tests benötigt**: ~20 Tests

#### 3. Repository Interfaces (0% Coverage)
- ✅ **Priorität**: Niedrig (nur Interfaces)
- **Dateien**: `src/EasterEggHunt.Domain/Repositories/*.cs`
- **Aktion**: Interfaces brauchen keine Tests (werden durch Implementierungen getestet)
- **Empfehlung**: Keine Tests nötig

### Empfohlene Maßnahmen für Domain

1. **Erweitere Entity Tests** um fehlende Edge Cases (+20 Tests)
2. **Ignoriere DTOs** für Coverage (sind POCOs)
3. **Fokussiere auf Business-Logik** in Entities

**Geschätzte Verbesserung**: +10-15% Coverage durch ~20 zusätzliche Tests

## Application Coverage Analyse (68.42% → 80% benötigt: +11.58%)

### Ungetestete Bereiche

#### 1. StatisticsService (Niedrige Coverage)
- ✅ **Priorität**: Hoch (komplexe Business-Logik)
- **Datei**: `src/EasterEggHunt.Application/Services/Statistics/StatisticsService.cs`
- **Probleme**:
  - Komplexe Aggregations-Logik
  - Mehrere Abhängigkeiten (Campaign, QrCode, User, Find Services)
  - Edge Cases für leere Daten
- **Aktion**: Erweitere StatisticsService Tests
- **Geschätzte Tests benötigt**: ~15 Tests

#### 2. SessionService (Teilweise ungetestet)
- ✅ **Priorität**: Hoch (kritische Funktionalität)
- **Datei**: `src/EasterEggHunt.Application/Services/Session/SessionService.cs`
- **Ungetestete Bereiche**:
  - Session-Validierung
  - Session-Expiration
  - Edge Cases für ungültige Sessions
- **Geschätzte Tests benötigt**: ~10 Tests

#### 3. FindService (Teilweise ungetestet)
- ✅ **Priorität**: Hoch (kritische Funktionalität)
- **Datei**: `src/EasterEggHunt.Application/Services/Find/FindService.cs`
- **Ungetestete Bereiche**:
  - Komplexe Query-Methoden
  - Edge Cases für leere Ergebnisse
  - Error-Handling für nicht-existierende Ressourcen
- **Geschätzte Tests benötigt**: ~10 Tests

#### 4. ServiceCollectionExtensions (0% Coverage)
- ✅ **Priorität**: Niedrig (nur DI-Registrierung)
- **Datei**: `src/EasterEggHunt.Application/ServiceCollectionExtensions.cs`
- **Aktion**: Keine Tests nötig (wird durch Integration Tests indirekt getestet)

### Empfohlene Maßnahmen für Application

1. **Erweitere StatisticsService Tests** (+15 Tests)
2. **Erweitere SessionService Tests** (+10 Tests)
3. **Erweitere FindService Tests** (+10 Tests)
4. **Erweitere weitere Service Edge Cases** (+5 Tests)

**Geschätzte Verbesserung**: +10-15% Coverage durch ~40 zusätzliche Tests

## Infrastructure Coverage Analyse (34.04% → 60% benötigt: +25.96%)

### Ungetestete Bereiche

#### 1. Repository Implementierungen (Teilweise ungetestet)
- ✅ **Priorität**: Hoch (Datenbank-Zugriff)
- **Dateien**: `src/EasterEggHunt.Infrastructure/Repositories/*.cs`
- **Probleme**:
  - Komplexe Query-Methoden unvollständig getestet
  - Edge Cases für leere Ergebnisse
  - Transaction-Verhalten
  - Concurrency-Szenarien
- **Geschätzte Tests benötigt**: ~20 Tests

#### 2. DbContext und Configuration (Niedrige Coverage)
- ✅ **Priorität**: Mittel (Infrastructure-Setup)
- **Dateien**:
  - `src/EasterEggHunt.Infrastructure/Data/EasterEggHuntDbContext.cs`
  - `src/EasterEggHunt.Infrastructure/Configuration/DbContextConfiguration.cs`
- **Probleme**:
  - Entity-Konfigurationen nicht direkt testbar
  - Configuration-Logik teilweise ungetestet
- **Aktion**: Erweitere Configuration Tests
- **Geschätzte Tests benötigt**: ~5 Tests

#### 3. ServiceCollectionExtensions (Niedrige Coverage)
- ✅ **Priorität**: Niedrig (DI-Registrierung)
- **Datei**: `src/EasterEggHunt.Infrastructure/ServiceCollectionExtensions.cs`
- **Aktion**: Keine Tests nötig (wird durch Integration Tests indirekt getestet)

#### 4. Migrations (Ausgeschlossen)
- ✅ **Priorität**: Keine (Migrations werden nicht getestet)
- **Dateien**: `src/EasterEggHunt.Infrastructure/Migrations/*.cs`
- **Aktion**: Bereits von Coverage ausgeschlossen

### Empfohlene Maßnahmen für Infrastructure

1. **Erweitere Repository Integration Tests** (+20 Tests)
2. **Erweitere Configuration Tests** (+5 Tests)
3. **Erweitere DbContext Tests** (+5 Tests)

**Geschätzte Verbesserung**: +25-30% Coverage durch ~30 zusätzliche Tests

## Zusammenfassung der Maßnahmen

### Domain (66.4% → 80%: +13.6%)
- ✅ Erweitere Entity Tests um Edge Cases (+20 Tests)
- **Geschätzte Verbesserung**: +10-15%

### Application (68.42% → 80%: +11.58%)
- ✅ Erweitere StatisticsService Tests (+15 Tests)
- ✅ Erweitere SessionService Tests (+10 Tests)
- ✅ Erweitere FindService Tests (+10 Tests)
- ✅ Erweitere weitere Service Edge Cases (+5 Tests)
- **Geschätzte Verbesserung**: +10-15%

### Infrastructure (34.04% → 60%: +25.96%)
- ✅ Erweitere Repository Integration Tests (+20 Tests)
- ✅ Erweitere Configuration Tests (+5 Tests)
- ✅ Erweitere DbContext Tests (+5 Tests)
- **Geschätzte Verbesserung**: +25-30%

## Nächste Schritte

### Phase 5.2: Domain Coverage erhöhen (+20 Tests)
1. Analysiere ungetestete Entity-Methoden
2. Füge Edge Cases für Konstruktoren hinzu
3. Teste zusätzliche Property-Kombinationen

### Phase 5.3: Application Coverage erhöhen (+40 Tests)
1. Erweitere StatisticsService Tests
2. Erweitere SessionService Tests
3. Erweitere FindService Tests
4. Füge Edge Cases für alle Services hinzu

### Phase 5.4: Infrastructure Coverage erhöhen (+30 Tests)
1. Erweitere Repository Integration Tests
2. Füge komplexe Query-Tests hinzu
3. Teste Edge Cases und Error-Handling

## Metriken

**Gesamt-Tests aktuell**: 637 Tests
- Domain: 177 Tests
- Application: 208 Tests
- Infrastructure: 115 Tests
- API: 80 Tests
- Web: 57 Tests
- Integration: 37 Tests

**Zusätzliche Tests benötigt**: ~90 Tests
- Domain: +20 Tests
- Application: +40 Tests
- Infrastructure: +30 Tests

**Erwartetes Ergebnis nach Phase 5**:
- Domain: ~80% Coverage ✅
- Application: ~80% Coverage ✅
- Infrastructure: ~60% Coverage ✅

