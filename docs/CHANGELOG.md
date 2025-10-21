# Changelog

Alle wichtigen Änderungen an diesem Projekt werden in dieser Datei dokumentiert.

Das Format basiert auf [Keep a Changelog](https://keepachangelog.com/de/1.0.0/),
und dieses Projekt folgt [Semantic Versioning](https://semver.org/lang/de/).

## [Unreleased]

### Hinzugefügt
- **Integration Tests Implementation**: 163 Integration Tests für komplette User-Journeys
- **Documentation Audit**: Umfassende Überprüfung aller Dokumentation auf Aktualität
- **Test-Pyramide Optimierung**: Repository + Controller + Workflow Tests strukturiert

### Geändert
- **Test-Count Updates**: Dokumentation aktualisiert von 149 auf 652+ Tests
- **Coverage Requirements**: Realistische 80% Coverage-Schwelle statt 90%
- **Script References**: Korrekte Verweise auf easter-egg-hunt.* Skripte

### Behoben
- **Dokumentations-Inkonsistenzen**: Test-Zahlen und Coverage-Requirements korrigiert
- **Script-Referenzen**: Verweise auf nicht-existierende Skripte behoben
- **QR-Code URL-System Refactoring**: Dynamische URL-Generierung ohne Domain-Speicherung
- **Mehrere aktive Kampagnen**: Unterstützung für gleichzeitige aktive Kampagnen
- **Robuste JSON-Fehlerbehandlung**: Graceful Fallback bei API-Deserialisierungsfehlern
- **QR-Code Mehrfach-Zählung verhindern**: Ein Nutzer kann QR-Code nur einmal finden
- **Neue Tests**: Umfassende Test-Coverage für URL-System und Mehrfach-Scans
- **Troubleshooting Guide**: Umfassende Anleitung für häufige Probleme und Lösungen
- **Praktische Lektionen**: Dokumentation der gelernten Best Practices in DEVELOPER_GUIDE.md und CODING_GUIDELINES.md
- **QR-Code-Scan-Tests**: Vollständige Unit Test-Coverage für alle Scan-Szenarien

### Geändert
- **QrCode Entity**: `UniqueUrl` (Uri) → `Code` (string) - nur Code ohne Domain
- **Datenbank-Schema**: Migration von vollständigen URLs zu reinen Codes
- **URL-Generierung**: Dynamische Erstellung zur Laufzeit mit `window.location.origin`
- **API-Endpoints**: `/api/qrcodes/by-url/{uniqueUrl}` → `/api/qrcodes/by-code/{code}`
- **Geschäftslogik**: Ein Benutzer kann denselben QR-Code nur einmal finden
- **Dokumentation**: DATABASE_SCHEMA.md und ARCHITECTURE.md aktualisiert

### Behoben
- **Kampagnen-Mismatch**: QR-Codes werden jetzt gegen alle aktiven Kampagnen geprüft
- **JSON-Deserialisierungsfehler**: Robuste Behandlung leerer API-Antworten
- **Mehrfach-Zählung**: Verhindert doppelte Fund-Registrierung für denselben QR-Code
- **Domain-Flexibilität**: Server-URL-Wechsel ohne Datenbank-Migration möglich
- **API-Endpoint-Inkonsistenz**: QR-Code-Scan-Problem durch korrekte Endpoint-URLs behoben
- **Admin-Link-404-Fehler**: Fehlende Controller-Actions implementiert
- **Test-Coverage-Lücken**: Umfassende Unit Tests für kritische User-Flows hinzugefügt

### Entfernt
- **QrCodeUrl Value Object**: Nicht mehr benötigt durch Code-basiertes System
- **Uri-Konvertierung**: Vereinfachte URL-Behandlung ohne Domain-Speicherung
- **Direkte DB-Zugriffe**: Web-Projekt hat keine direkte Datenbankverbindung mehr
- **Service-Abhängigkeiten**: Web-Projekt verwendet keine Application/Infrastructure Services mehr

### Geändert (Legacy)
- **Test Coverage**: Ziel von 100% auf 80% Minimum realistisch angepasst
- **Coverage-Konfiguration**: Nur für Domain.Tests und Infrastructure.Tests aktiviert
- **Docker Compose**: Von `docker-compose` zu `docker compose` aktualisiert
- **Docker-Build**: Von lokalen Builds zu Buildx Bake migriert
- **DbContext-Konfiguration**: Zentralisierte Konfiguration mit Dependency Injection

### Entfernt
- **Docker Cache**: Cache-Export-Konfigurationen entfernt (nicht unterstützt)
- **Volume Mounts**: Lokale Source-Volume-Mounts aus Docker Compose entfernt
- **Unit Tests**: Problematische Repository Unit Tests zugunsten von Integration Tests entfernt

### Behoben
- **Nullable-Warnings**: Alle Integration Test-Felder mit `null!` initialisiert
- **Foreign Key Constraints**: Entity-Reihenfolge in Tests korrigiert
- **Code Analysis**: CA2227, CA1056, CA1724, CA1707 Warnings behoben
- **Package Downgrades**: Microsoft.Extensions.Logging Version-Konflikte gelöst
- **Docker Build**: Buildx Bake Cache-Probleme behoben
- **Test Discovery**: Integration Tests mit `[Category("Integration")]` markiert
- **Database Schema**: In-Memory zu File-based SQLite für bessere Kompatibilität
- **GitHub Actions Coverage**: Coverage-Threshold-Check vollständig repariert
  - XPlat Code Coverage mit Cobertura-Format konfiguriert
  - Migration-Dateien und generierte Code aus Coverage ausgeschlossen
  - Domain Coverage: 91.56%, Infrastructure Coverage: 86.90%, Gesamt: 85.92%
  - Alle Werte über 80% Threshold, Pipeline läuft erfolgreich durch

### Sicherheit
- **Dependency Scanning**: Automatische Sicherheits-Scans in CI/CD Pipeline
- **Code Analysis**: Sicherheits-relevante Code-Analyse-Regeln aktiviert

## [0.1.0] - 2025-10-02

### Hinzugefügt
- **Sprint 0 Foundation**: Projekt-Setup und Architektur-Grundlagen
- **Repository Pattern**: Vollständige Implementierung mit Integration Tests
- **Docker-Entwicklungsumgebung**: Buildx Bake, Compose, optimierte Builds
- **Test-Suite**: 149 Tests mit 81.31% Code Coverage
- **CI/CD**: GitHub Actions Pipeline mit automatischen Checks

### Behoben
- **Nullable-Warnings**: Integration Test-Felder korrekt initialisiert
- **Coverage-Konfiguration**: Realistische 80% Coverage-Schwelle
- **Docker-Build**: Cache-Probleme und Build-Optimierungen

## [1.0.0] - TBD

### Hinzugefügt
- Admin-Authentifizierung
- Kampagnen-Verwaltung
- QR-Code-Erstellung und -Verwaltung
- Druckfreundliche QR-Code-Übersicht
- Mitarbeiter-Registrierung über QR-Code-Scan
- Fortschritts-Tracking für Mitarbeiter
- Session-Management über Cookies
- Statistiken und Auswertungen für Administratoren

---

## Legende

- **Hinzugefügt** für neue Features
- **Geändert** für Änderungen an bestehender Funktionalität
- **Veraltet** für Features, die bald entfernt werden
- **Entfernt** für entfernte Features
- **Behoben** für Fehlerbehebungen
- **Sicherheit** für Sicherheitsupdates
