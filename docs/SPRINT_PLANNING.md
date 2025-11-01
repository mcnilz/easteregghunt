# 🏃‍♂️ Sprint Planning - Easter Egg Hunt System

## 📋 Projekt-Übersicht

**Technologie-Stack:**
- **Backend**: .NET Core 8.0 Web API
- **Frontend**: .NET Core 8.0 MVC/Razor Pages
- **Datenbank**: SQLite mit Entity Framework Core
- **Containerisierung**: Docker Compose
- **Testing**: NUnit, Moq
- **CI/CD**: GitHub Actions

**Entwicklungsansatz:**
- Test-Driven Development (TDD)
- Clean Code Prinzipien
- Domain-Driven Design (DDD)
- 80% Test Coverage Minimum (realistisch angepasst)

## 🎯 Aktueller Projekt-Status

**Aktueller Sprint:** Sprint 2 - QR-Code Management ✅ VOLLSTÄNDIG ABGESCHLOSSEN  
**Fortschritt:** 100% abgeschlossen (29/29 Story Points) ✅  
**Tests:** 652+ Tests erfolgreich mit exzellenter Code Coverage  
**Letzte Commits:** QR-Code Statistiken implementiert, Find-Counts, Finder-Listen, Top-Performer  
**✅ VOLLSTÄNDIG:** QR-Code Management mit Statistiken und Drucklayout komplett!

**Technische Highlights:**
- ✅ Vollständige Clean Architecture mit 4 Layers
- ✅ 6 Domain Entities mit umfassenden Tests (78 Tests)
- ✅ Repository Pattern mit Integration Tests (SQLite)
- ✅ Service Layer mit Dependency Injection (6 Services)
- ✅ API Controller mit vollständigen CRUD-Operationen (4 Controller)
- ✅ MVC-Struktur für Admin und Employee Interfaces
- ✅ Admin-Authentifizierung mit BCrypt
- ✅ QR-Code CRUD-Operationen vollständig implementiert (8 Endpoints)
- ✅ Controller Integration Tests mit WebApplicationFactory
- ✅ Request/Response DTOs für bessere API-Struktur
- ✅ Hot-Reload Konfiguration für Entwicklungsumgebung
- ✅ Environment-spezifische Konfiguration (Dev/Staging/Prod/Test)
- ✅ Automatische Datenbank-Migrationen in Entwicklungsskripten
- ✅ Umfassende Dokumentation (Architektur, Hot-Reload, Environment)
- ✅ 100% Coding Guidelines Compliance
- ✅ JSON-Serialisierung mit camelCase-Konvertierung
- ✅ Vollständige HTTP-Status-Code-Abdeckung (200, 201, 204, 400, 404, 500)
- ✅ **Code Coverage: Domain 89.6%, Application 90.52%, Infrastructure 39.16%**
- ✅ **Test-Pyramide: 73% Unit, 22% Integration, 5% E2E** ✅
- ✅ **Test-Geschwindigkeit: ~27s für 792 Tests** ✅
- ✅ **ReportGenerator für Coverage-Visualisierung**
- ✅ **Vereinfachte Coverage-Checks mit dotnet test Thresholds**
- ✅ **Client-side QR-Code-Generierung mit QRCode.js (MIT-Lizenz)**
- ✅ **QR-Code Drucklayout mit A4-Optimierung und Größenanpassung**
- ✅ **QR-Code Statistiken mit Find-Counts und Finder-Listen**
- ✅ **Top-Performer QR-Codes und ungerundene QR-Codes Übersicht**
- ✅ **Vollständige Web-Authentifizierung mit Cookie-basiertem Login**
- ✅ **Admin-Login-System mit Session-Management und Sicherheitsfeatures**

---

## 🎯 Sprint 0: Projekt-Foundation (2 Wochen)

### Sprint-Ziel
Technische Grundlagen schaffen und Architektur etablieren

### 📝 User Stories

#### Story 0.1: Projekt-Setup
**Als** Entwickler  
**Möchte ich** eine vollständige Entwicklungsumgebung haben  
**Damit** ich sofort mit der Entwicklung beginnen kann  

**Akzeptanzkriterien:**
- [x] .NET Core 8.0 Solution mit Clean Architecture
- [x] Docker Compose für lokale Entwicklung
- [x] Alle NuGet Packages konfiguriert
- [x] EditorConfig und Code Analysis aktiv
- [x] GitHub Actions CI/CD Pipeline funktioniert
- [x] README mit Setup-Anweisungen

**Aufwand:** 8 Story Points

#### Story 0.2: Datenbank-Schema Design
**Als** Entwickler  
**Möchte ich** ein durchdachtes Datenbankschema haben  
**Damit** alle Features effizient implementiert werden können  

**Akzeptanzkriterien:**
- [x] Entity-Relationship-Diagramm erstellt
- [x] EF Core Entities definiert
- [x] Seed-Daten für Entwicklung (SeedDataService)
- [x] Migration-Strategie festgelegt
- [x] Datenbank-Constraints definiert
- [x] Performance-Überlegungen dokumentiert

**Aufwand:** 5 Story Points

#### Story 0.3: Architektur-Design
**Als** Entwickler  
**Möchte ich** eine klare Architektur haben  
**Damit** der Code wartbar und testbar ist  

**Akzeptanzkriterien:**
- [x] Clean Architecture Layers definiert
- [x] Dependency Injection konfiguriert
- [x] Repository Pattern implementiert
- [x] Service Layer Struktur (5 Services implementiert)
- [ ] API Controller Struktur
- [ ] Frontend MVC Struktur
- [x] Architektur-Dokumentation

**Aufwand:** 8 Story Points

#### Story 0.4: Docker-Entwicklungsumgebung
**Als** Entwickler  
**Möchte ich** eine containerisierte Entwicklungsumgebung  
**Damit** alle Entwickler die gleiche Umgebung haben  

**Akzeptanzkriterien:**
- [x] Dockerfile für API
- [x] Dockerfile für Web Frontend
- [x] Docker Compose für lokale Entwicklung
- [x] SQLite Volume-Mapping
- [ ] Hot-Reload für Entwicklung
- [ ] Environment-spezifische Konfiguration

**Aufwand:** 5 Story Points

**Sprint 0 Gesamt:** 26 Story Points

### 📊 Sprint 0 Status (Stand: 09.10.2025)
- **Abgeschlossen:** 22 Story Points (85%)
- **Verbleibend:** 4 Story Points (15%)
- **Tests:** 116 Tests erfolgreich, exzellente Code Coverage
- **Repository Pattern:** Vollständig implementiert mit Integration Tests
- **Service Layer:** 6 Services mit Dependency Injection implementiert
- **Coverage-Verbesserung:** Infrastructure von 0% auf 33.44% gesteigert

**Abgeschlossene Aufgaben:**
- ✅ API Controller Grundstruktur (4 Controller mit CRUD-Operationen)
- ✅ Frontend MVC Grundstruktur (Admin & Employee Interfaces)
- ✅ Hot-Reload Konfiguration (automatische Änderungsübernahme)
- ✅ Environment-spezifische Konfiguration (4 Umgebungen)

---

## 🚀 Sprint 1: Admin-Authentifizierung und QR-Code-Management (2 Wochen)

### Sprint-Ziel
Admin kann sich anmelden und QR-Codes für Kampagnen verwalten

### 📝 User Stories

#### Story 1.1: Admin-Authentifizierung ✅ ABGESCHLOSSEN
**Als** Administrator  
**Möchte ich** mich sicher anmelden können  
**Damit** ich das System verwalten kann  

**Akzeptanzkriterien:**
- [x] Login-Seite mit Formular
- [x] Passwort-Hashing (bcrypt)
- [x] Session-Management
- [x] Logout-Funktionalität
- [x] "Remember Me" Option
- [x] Fehlerbehandlung bei falschen Daten
- [x] 100% Test Coverage (19 Tests)

**Gherkin:** `features/admin_authentication.feature`  
**Aufwand:** 8 Story Points ✅

#### Story 1.2: QR-Code CRUD-Operationen ✅ ABGESCHLOSSEN
**Als** Administrator  
**Möchte ich** QR-Codes für eine Kampagne erstellen, bearbeiten und löschen  
**Damit** ich die Oster-Eier für die Suche vorbereiten kann  

**Akzeptanzkriterien:**
- [x] QR-Code erstellen mit Titel, Beschreibung und internen Notizen
- [x] QR-Code bearbeiten (alle Felder)
- [x] QR-Code löschen mit Bestätigung
- [x] QR-Code-Liste pro Kampagne anzeigen
- [x] Validierung aller Eingaben
- [x] Request/Response DTOs implementiert
- [x] 100% Test Coverage (alle Tests angepasst)

**Gherkin:** `features/qr_code_management.feature`  
**Aufwand:** 8 Story Points ✅

#### Story 1.3: Admin-Dashboard erweitern ✅ ABGESCHLOSSEN
**Als** Administrator  
**Möchte ich** eine Übersicht über alle Kampagnen und QR-Codes haben  
**Damit** ich den Status schnell erfassen kann  

**Akzeptanzkriterien:**
- [x] Dashboard mit Kampagnen-Übersicht
- [x] Anzahl QR-Codes pro Kampagne
- [x] Anzahl Teilnehmer pro Kampagne
- [x] QR-Code-Statistiken anzeigen
- [x] Schnellzugriff auf QR-Code-Management
- [x] Responsive Design
- [x] 100% Test Coverage (3 Web-Tests)

**Gherkin:** `features/admin_statistics.feature`  
**Aufwand:** 4 Story Points ✅

#### Story 1.4: Web-Authentifizierung implementieren ✅ ABGESCHLOSSEN
**Als** Administrator  
**Möchte ich** mich über das Web-Interface anmelden können  
**Damit** ich sicher auf Admin-Funktionen zugreifen kann  

**Akzeptanzkriterien:**
- [x] Login-Controller mit GET/POST Actions
- [x] Login-View mit Formular (Username/Password)
- [x] Authentication-Middleware konfigurieren
- [x] Session-Management im Web-Projekt
- [x] [Authorize] Attribute für Admin-Controller
- [x] Logout-Funktionalität
- [x] Redirect nach Login zu ursprünglich angeforderter Seite
- [x] Fehlerbehandlung bei falschen Login-Daten
- [x] "Remember Me" Checkbox
- [x] 100% Test Coverage (18 Tests)

**Default-Admin-Login:**
- **Benutzername:** `admin`
- **Passwort:** `admin123`
- **E-Mail:** `admin@easteregghunt.local`

**Gherkin:** `features/admin_authentication.feature`  
**Aufwand:** 6 Story Points ✅

**Sprint 1 Gesamt:** 26 Story Points (26/26 abgeschlossen = 100%) ✅

---

## 🎨 Sprint 2: QR-Code Management (2 Wochen)

### Sprint-Ziel
Admin kann QR-Codes erstellen und verwalten

### 📝 User Stories

#### Story 2.1: QR-Code CRUD ✅ ABGESCHLOSSEN
**Als** Administrator  
**Möchte ich** QR-Codes für eine Kampagne erstellen können  
**Damit** Mitarbeiter sie finden können  

**Akzeptanzkriterien:**
- [x] QR-Code mit Titel und interner Notiz erstellen
- [x] QR-Code bearbeiten
- [x] QR-Code löschen
- [x] QR-Code aktivieren/deaktivieren
- [x] Eindeutige URLs generieren
- [x] QR-Code Bild generieren (Client-side mit QRCode.js)
- [x] 100% Test Coverage

**Gherkin:** `features/qr_code_management.feature`  
**Aufwand:** 13 Story Points ✅

#### Story 2.2: QR-Code Drucklayout ✅ ABGESCHLOSSEN
**Als** Administrator  
**Möchte ich** QR-Codes druckfreundlich anzeigen können  
**Damit** ich sie ausdrucken und verstecken kann  

**Akzeptanzkriterien:**
- [x] Druckansicht für alle QR-Codes
- [x] Auswahl spezifischer QR-Codes
- [x] A4-optimiertes Layout
- [x] Anpassbare QR-Code-Größe
- [x] Titel ein-/ausblendbar
- [x] Browser-Druckdialog Integration
- [x] 100% Test Coverage

**Gherkin:** `features/print_layout.feature`  
**Aufwand:** 8 Story Points ✅

#### Story 2.3: QR-Code Statistiken ✅ ABGESCHLOSSEN
**Als** Administrator  
**Möchte ich** sehen, wie oft QR-Codes gefunden wurden  
**Damit** ich die Beliebtheit der Verstecke bewerten kann  

**Akzeptanzkriterien:**
- [x] Anzahl Funde pro QR-Code
- [x] Liste der Finder mit Zeitstempel
- [x] Noch nicht gefundene QR-Codes hervorheben
- [x] Sortierung nach Beliebtheit
- [x] Top-Performer QR-Codes Übersicht
- [x] Ungerundene QR-Codes gruppiert nach Kampagne
- [x] Detailansicht für einzelne QR-Code Statistiken
- [x] 100% Test Coverage

**Gherkin:** `features/qr_code_management.feature`  
**Aufwand:** 8 Story Points ✅

**Sprint 2 Gesamt:** 29 Story Points (29/29 abgeschlossen = 100%) ✅

---

## 📱 Sprint 3: Mitarbeiter-Frontend (2 Wochen)

### Sprint-Ziel
Mitarbeiter können QR-Codes scannen und ihren Fortschritt verfolgen

### 📝 User Stories

#### Story 3.1: Mitarbeiter-Registrierung ✅ ABGESCHLOSSEN
**Als** Mitarbeiter  
**Möchte ich** mich beim ersten QR-Code-Scan registrieren  
**Damit** ich am Easter Egg Hunt teilnehmen kann  

**Akzeptanzkriterien:**
- [x] Registrierungsformular beim ersten Scan
- [x] Name-Eingabe mit Validierung (eindeutige Namen)
- [x] Cookie-basierte Session (EmployeeScheme)
- [x] Mobile-optimierte UI (Bootstrap, responsive)
- [x] Fehlerbehandlung (API-Fehler, Validierung)
- [x] 100% Test Coverage (alle Tests laufen erfolgreich)
- [x] Employee Authentication mit Claims
- [x] Session-Management mit persistenten Daten
- [x] QR-Code URL Preservation nach Registrierung

**Gherkin:** `features/employee_registration.feature`  
**Aufwand:** 8 Story Points ✅

#### Story 3.2: QR-Code Scannen ✅ ABGESCHLOSSEN
**Als** Mitarbeiter  
**Möchte ich** QR-Codes scannen können  
**Damit** ich Verstecke finden kann  

**Akzeptanzkriterien:**
- [x] QR-Code URL-Verarbeitung (`/qr/{uniqueUrl}`)
- [x] Fund-Bestätigung anzeigen (Titel, Beschreibung, Zeitstempel, Fortschritt)
- [x] Mehrfache Funde erlauben (mit Hinweis "bereits gefunden")
- [x] Zeitstempel speichern
- [x] Ungültige QR-Codes behandeln
- [x] Mobile-optimiert
- [x] 100% Test Coverage

**Gherkin:** `features/qr_code_scanning.feature`  
**Aufwand:** 13 Story Points ✅

#### Story 3.2.1: QR-Code URL-System Refactoring ✅ ABGESCHLOSSEN
**Als** Entwickler  
**Möchte ich** dass QR-Code URLs nur den Code ohne Domain speichern  
**Damit** Server-URLs ohne Migrationen gewechselt werden können  

**Akzeptanzkriterien:**
- [x] QR-Code `UniqueUrl` speichert nur den Code (z.B. `d90cffe8f07b`)
- [x] UniqueUrl zu Code umbenennen
- [x] Domain wird dynamisch zur Laufzeit hinzugefügt
- [x] Backend QR-Code Modal zeigt korrekte URLs mit aktueller Domain
- [x] Druckvorschau zeigt korrekte URLs mit aktueller Domain
- [x] QR-Code-Scanning funktioniert weiterhin korrekt
- [x] **Prüfung:** Datenbank-Migration erforderlich oder nicht (Entity Framework Migration)
- [x] **Prüfung:** Bestehende Daten kompatibel mit neuer Struktur
- [x] Alle bestehenden Tests bestehen weiterhin
- [x] Neue Tests für URL-Generierung

**Technische Details:**
- **Datenbank:** `QrCode.UniqueUrl` ändert von `Uri` zu `string` (nur Code)
- **Migration:** Prüfung ob EF Core Migration erforderlich ist
- **Daten-Kompatibilität:** Bestehende `Uri`-Werte müssen zu `string`-Codes konvertiert werden
- **Backend:** URL-Generierung in JavaScript für Modal und Druckvorschau
- **API:** QR-Code-Scanning verwendet weiterhin `UniqueUrl` für Lookup
- **Tests:** Unit Tests für URL-Generierung, Integration Tests für QR-Code-Scanning

**Gherkin:** `features/qr_code_url_refactoring.feature`  
**Aufwand:** 5 Story Points

#### Story 3.3: Fortschritts-Anzeige ✅ ABGESCHLOSSEN
**Als** Mitarbeiter  
**Möchte ich** meinen Fortschritt sehen können  
**Damit** ich weiß, wie viele Verstecke ich noch finden muss  

**Akzeptanzkriterien:**
- [x] Übersicht gefundener Verstecke
- [x] Fortschrittsbalken
- [x] Anzahl verbleibender Verstecke
- [x] Chronologische Liste der Funde
- [x] Glückwunsch bei Vollendung
- [x] Mobile-optimiert
- [x] 100% Test Coverage
- [x] Dynamische Kampagnen-Auswahl basierend auf letztem Fund
- [x] Optimierte Datenbankabfragen (N+1 Problem behoben)
- [x] EF Core Query-Logs in Tests unterdrückt

**Technische Details:**
- **API:** Neue Endpoints für User-Statistiken und Campaign-spezifische Finds
- **Web:** Progress-Seite mit dynamischer Kampagnen-Auswahl
- **Performance:** Aggregierte Queries statt N+1 Probleme
- **Tests:** Ruhige Test-Logs durch Logging-Filter
- **Dokumentation:** DEVELOPER_GUIDE um Test-Logging-Sektion erweitert

**Gherkin:** `features/employee_progress.feature`  
**Aufwand:** 8 Story Points

**Sprint 3 Gesamt:** 34 Story Points ✅ **VOLLSTÄNDIG ABGESCHLOSSEN**

### 🚀 Zusätzliche Optimierungen (abgeschlossen)

#### Datenbank-Performance-Optimierung ✅ ABGESCHLOSSEN
**Ziel:** N+1 Query-Probleme eliminieren und Test-Logs bereinigen

**Durchgeführte Optimierungen:**
- [x] **FindService:** Aggregierte Queries (`CountAsync`, `AnyAsync`, `FirstOrDefaultAsync`)
- [x] **StatisticsController:** Direkte Repository-Calls statt Service-Layer-Loops
- [x] **FindsController:** Neuer Campaign-Filter-Endpoint mit optionalem `take` Parameter
- [x] **Web Progress:** Dynamische Kampagnen-Auswahl basierend auf letztem User-Fund
- [x] **Test-Logs:** EF Core SQL-Logs in Integration Tests unterdrückt
- [x] **Dokumentation:** DEVELOPER_GUIDE um Test-Logging-Sektion erweitert

**Technische Verbesserungen:**
- **Performance:** Reduzierte Datenbankabfragen von O(n) auf O(1) für Statistiken
- **Test-Qualität:** Saubere Test-Ausgaben ohne SQL-Spam
- **Code-Qualität:** Bessere Separation of Concerns zwischen API und Web
- **Wartbarkeit:** Klarere Repository-Pattern-Implementierung

---

## 📊 Sprint 4: Admin-Statistiken & Polish (2 Wochen)

### Sprint-Ziel
Vollständige Statistiken und System-Verfeinerung

### 📝 User Stories

#### Story 4.1: Umfassende Admin-Statistiken
**Als** Administrator  
**Möchte ich** detaillierte Statistiken sehen  
**Damit** ich den Erfolg der Kampagne bewerten kann  

**Akzeptanzkriterien:**
- [x] Teilnehmer-Rangliste ✅
- [x] Zeitbasierte Statistiken ✅
- [ ] Fund-Historie mit Filter
- [ ] Diagramme und Visualisierungen
- [ ] Performance-Metriken
- [ ] 100% Test Coverage

**Gherkin:** `features/admin_statistics.feature`  
**Aufwand:** 13 Story Points

#### Story 4.2: Session-Management Optimierung
**Als** System  
**Möchte ich** Sessions effizient verwalten  
**Damit** Benutzer eine nahtlose Erfahrung haben  

**Akzeptanzkriterien:**
- [ ] Session-Bereinigung
- [ ] Cookie-Sicherheit
- [ ] Session-Timeout
- [ ] Geräte-übergreifende Behandlung
- [ ] GDPR-Compliance
- [ ] 100% Test Coverage

**Gherkin:** `features/session_management.feature`  
**Aufwand:** 8 Story Points

#### Story 4.3: UI/UX Polish & Performance
**Als** Benutzer  
**Möchte ich** eine schnelle und intuitive Benutzeroberfläche  
**Damit** die Nutzung Spaß macht  

**Akzeptanzkriterien:**
- [ ] Responsive Design für alle Bildschirmgrößen
- [ ] Loading-Indikatoren
- [ ] Error-Handling mit benutzerfreundlichen Nachrichten
- [ ] Performance-Optimierung
- [ ] Accessibility (WCAG 2.1)
- [ ] Browser-Kompatibilität
- [ ] 100% Test Coverage

**Aufwand:** 8 Story Points

**Sprint 4 Gesamt:** 29 Story Points

---

## 🚢 Sprint 5: Deployment & Dokumentation (1 Woche)

### Sprint-Ziel
Produktionsreife und vollständige Dokumentation

### 📝 User Stories

#### Story 5.1: Produktions-Deployment
**Als** DevOps  
**Möchte ich** das System in Produktion deployen können  
**Damit** es von Mitarbeitern genutzt werden kann  

**Akzeptanzkriterien:**
- [ ] Produktions-Docker-Images
- [ ] Environment-Konfiguration
- [ ] SSL/HTTPS-Setup
- [ ] Backup-Strategie
- [ ] Monitoring und Logging
- [ ] Health Checks
- [ ] Deployment-Dokumentation

**Aufwand:** 8 Story Points

#### Story 5.2: Benutzer-Dokumentation
**Als** Benutzer  
**Möchte ich** eine verständliche Anleitung haben  
**Damit** ich das System effektiv nutzen kann  

**Akzeptanzkriterien:**
- [ ] Admin-Handbuch
- [ ] Mitarbeiter-Anleitung
- [ ] FAQ-Sektion
- [ ] Video-Tutorials (optional)
- [ ] Troubleshooting-Guide
- [ ] Screenshots und Beispiele

**Aufwand:** 5 Story Points

#### Story 5.3: Sicherheits-Audit
**Als** Sicherheitsverantwortlicher  
**Möchte ich** sicherstellen, dass das System sicher ist  
**Damit** keine Sicherheitslücken existieren  

**Akzeptanzkriterien:**
- [ ] Penetration Testing
- [ ] Dependency-Vulnerability-Scan
- [ ] OWASP-Compliance-Check
- [ ] Datenschutz-Bewertung
- [ ] Sicherheits-Dokumentation
- [ ] Security Headers

**Aufwand:** 5 Story Points

**Sprint 5 Gesamt:** 18 Story Points

---

## 📈 Sprint-Übersicht

| Sprint | Dauer | Story Points | Fokus |
|--------|-------|--------------|-------|
| Sprint 0 | 2 Wochen | 26 | Foundation & Architektur |
| Sprint 1 | 2 Wochen | 26 | Admin-Grundfunktionen ✅ |
| Sprint 2 | 2 Wochen | 29 | QR-Code Management ✅ |
| Sprint 3 | 2 Wochen | 34 | Mitarbeiter-Frontend ✅ |
| Sprint 4 | 2 Wochen | 29 | Statistiken & Polish |
| Sprint 5 | 1 Woche | 18 | Deployment & Docs |
| **Gesamt** | **11 Wochen** | **165 SP** | **Vollständiges System** |

## 🎯 Definition of Done

Für jede Story müssen folgende Kriterien erfüllt sein:

### Code Quality
- [ ] Clean Code Prinzipien befolgt
- [ ] Alle Tests geschrieben (TDD)
- [ ] 100% Test Coverage erreicht
- [ ] Code Review abgeschlossen
- [ ] Keine Compiler Warnings

### Funktionalität
- [ ] Alle Akzeptanzkriterien erfüllt
- [ ] Gherkin-Szenarien implementiert
- [ ] Manuelle Tests durchgeführt
- [ ] Cross-Browser getestet (Chrome, Firefox, Safari)
- [ ] Mobile-Responsivität geprüft

### Dokumentation
- [ ] Code-Dokumentation aktualisiert
- [ ] API-Dokumentation erweitert
- [ ] README bei Bedarf aktualisiert
- [ ] Deployment-Notizen erstellt

### Deployment
- [ ] CI/CD Pipeline erfolgreich
- [ ] Docker-Images funktionieren
- [ ] Staging-Deployment erfolgreich
- [ ] Performance-Tests bestanden

## 🔄 Sprint-Rituale

### Sprint Planning (2h)
- Story-Schätzung mit Planning Poker
- Sprint-Ziel definieren
- Capacity-Planung
- Task-Breakdown

### Daily Standups (15min)
- Was wurde gestern gemacht?
- Was wird heute gemacht?
- Gibt es Hindernisse?

### Sprint Review (1h)
- Demo der implementierten Features
- Stakeholder-Feedback
- Backlog-Anpassungen

### Sprint Retrospective (1h)
- Was lief gut?
- Was kann verbessert werden?
- Action Items für nächsten Sprint

## 🚨 Risiken & Mitigation

### Technische Risiken
- **Docker-Komplexität**: Frühe Prototypen, Pair Programming
- **EF Core Performance**: Profiling, Query-Optimierung
- **Mobile-Kompatibilität**: Kontinuierliches Testing

### Projekt-Risiken
- **Scope Creep**: Strikte Priorisierung, Change Control
- **Test Coverage**: Automatische Coverage-Gates
- **Code Quality**: Mandatory Code Reviews

---

**Dieses Sprint Planning folgt agilen Prinzipien und Clean Code Standards. Jeder Sprint liefert funktionsfähige Software mit hoher Qualität.**
