# 🥚 Easter Egg Hunt System

[![CI](https://github.com/your-org/easter-egg-hunt/workflows/Continuous%20Integration/badge.svg)](https://github.com/your-org/easter-egg-hunt/actions)
[![Coverage](https://img.shields.io/badge/coverage-100%25-brightgreen.svg)](https://github.com/your-org/easter-egg-hunt/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)

Ein interaktives Easter Egg Hunt System für Unternehmen, das es ermöglicht, QR-Code-basierte Osterspiele im Büro zu organisieren.

## Übersicht

Das Easter Egg Hunt System ermöglicht es Unternehmen, interne Osterspiele zu organisieren, bei denen Mitarbeiter QR-Codes im Büro suchen und scannen können.

## Features

### Admin-Bereich
- **Admin Authentication** (`features/admin_authentication.feature`): Login/Logout für Administratoren
- **Campaign Management** (`features/campaign_management.feature`): Erstellen und Verwalten von Kampagnen
- **QR-Code Management** (`features/qr_code_management.feature`): Erstellen und Verwalten von QR-Codes
- **Print Layout** (`features/print_layout.feature`): Druckfreundliche Ansicht für QR-Codes
- **Admin Statistics** (`features/admin_statistics.feature`): Statistiken und Übersichten über Funde

### Mitarbeiter-Bereich
- **Employee Registration** (`features/employee_registration.feature`): Registrierung beim ersten QR-Code Scan
- **QR-Code Scanning** (`features/qr_code_scanning.feature`): Scannen von QR-Codes und Fundverfolgung
- **Employee Progress** (`features/employee_progress.feature`): Fortschrittsanzeige für Mitarbeiter

### System
- **Session Management** (`features/session_management.feature`): Cookie-basierte Session-Verwaltung

## Hauptfunktionen

### Für Administratoren:
1. Sichere Anmeldung im Admin-Bereich
2. Erstellen und Verwalten von Easter Egg Hunt Kampagnen
3. Hinzufügen von QR-Codes mit Titeln und internen Notizen
4. Druckfreundliche Übersicht aller QR-Codes
5. Detaillierte Statistiken über Teilnehmer und Funde

### Für Mitarbeiter:
1. Einfache Teilnahme ohne Login (nur Name beim ersten Scan)
2. QR-Code Scannen mit dem Smartphone
3. Übersicht über gefundene und noch zu findende Verstecke
4. Fortschrittsanzeige mit motivierenden Elementen

### Technische Features:
1. Cookie-basierte Session-Verwaltung
2. Mehrfache Funde pro QR-Code möglich
3. Zeitstempel für alle Funde
4. Mobile-optimierte Benutzeroberfläche

## 🚀 Quick Start

### Voraussetzungen

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) oder [VS Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/)

### Installation

```bash
# Repository klonen
git clone https://github.com/your-org/easter-egg-hunt.git
cd easter-egg-hunt

# Dependencies installieren
dotnet restore

# Datenbank erstellen (falls erforderlich)
dotnet ef database update

# Anwendung starten
dotnet run --project src/EasterEggHunt.Web

# Oder mit Docker Compose (verwendet automatisch compose.yaml)
docker compose up
```

### Tests ausführen

```bash
# Alle Tests ausführen
dotnet test

# Mit Code Coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 📖 Dokumentation

- **[Architektur](ARCHITECTURE.md)** - Technische Architektur und Design-Entscheidungen
- **[Datenbank-Schema](DATABASE_SCHEMA.md)** - ER-Diagramm und Datenbankdokumentation
- **[Sprint-Planung](SPRINT_PLANNING.md)** - Detaillierte Sprint-Planung und User Stories
- **[Gherkin Features](features/)** - Detaillierte Feature-Spezifikationen
- **[Coding Guidelines](CODING_GUIDELINES.md)** - Entwicklungsstandards
- **[Docker Build](README-BUILD.md)** - Docker Buildx Bake Dokumentation
- **[Contributing](CONTRIBUTING.md)** - Beitrag zum Projekt
- **[Security](SECURITY.md)** - Sicherheitsrichtlinien
- **[Changelog](CHANGELOG.md)** - Versionshistorie

## 🤝 Beitragen

Wir freuen uns über Beiträge! Bitte lesen Sie unsere [Contributing Guidelines](CONTRIBUTING.md) für Details zum Entwicklungsprozess.

## 📄 Lizenz

Dieses Projekt ist unter der [MIT Lizenz](LICENSE) lizenziert.

## 🔒 Sicherheit

Sicherheitslücken bitte an security@your-domain.com melden. Siehe [SECURITY.md](SECURITY.md) für Details.

## 📞 Support

- **Issues**: [GitHub Issues](../../issues)
- **Diskussionen**: [GitHub Discussions](../../discussions)
- **E-Mail**: support@your-domain.com
