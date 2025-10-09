# Changelog

Alle wichtigen Änderungen an diesem Projekt werden in dieser Datei dokumentiert.

Das Format basiert auf [Keep a Changelog](https://keepachangelog.com/de/1.0.0/),
und dieses Projekt folgt [Semantic Versioning](https://semver.org/lang/de/).

## [Unreleased]

### Hinzugefügt
- **API-Client Architektur**: Web-Projekt kommuniziert jetzt über HTTP mit der API
- **EasterEggHunt.Common Projekt**: Shared Configuration und Utilities für zukünftige Verwendung
- **Automatisches Prozess-Management**: Entwicklungsskript beendet automatisch laufende Prozesse
- **Migration-Korrektur**: Datenbank-Migrationen laufen jetzt über API-Projekt statt Web-Projekt
- **Projekt-Setup**: Vollständige .NET Core 8.0 Solution mit Clean Architecture
- **Docker-Integration**: Docker Compose, Dockerfiles, Buildx Bake Setup
- **Datenbank-Schema**: 6 Domain Entities (Campaign, QrCode, User, Find, Session, AdminUser)
- **Repository Pattern**: Vollständige Implementierung mit 6 Repository-Interfaces und -Implementierungen
- **Integration Tests**: 95 umfassende Tests mit echter SQLite-Datenbank
- **Domain Tests**: 52 Unit Tests für alle Domain Entities

### Geändert
- **Web-Projekt**: Entfernte direkte Datenbankverbindung, verwendet jetzt API-Client
- **AdminController**: Refaktoriert für API-Client-Architektur
- **Entwicklungsskript**: Korrigierte Migration-Startup-Projekt von Web auf API
- **Projekt-Referenzen**: Entfernte Common-Projekt-Referenzen aus API und Web
- **CommonConfigurationExtensions**: Verschoben von Common-Projekt ins API-Projekt

### Behoben
- **Migration-Fehler**: "EasterEggHunt.Infrastructure.dll not found" beim Web-Projekt
- **Error.cshtml**: NullReferenceException durch null-safe Prüfung behoben
- **API-URL-Konfiguration**: Korrigierte Port-Konfiguration (7001 statt 7002)

### Entfernt
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
