# 🥚 Easter Egg Hunt System

[![CI](https://github.com/mcnilz/easteregghunt/workflows/Continuous%20Integration/badge.svg)](https://github.com/mcnilz/easteregghunt/actions)
[![Coverage](https://img.shields.io/badge/coverage-85%25-brightgreen.svg)](https://github.com/mcnilz/easteregghunt/actions)
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
git clone https://github.com/mcnilz/easteregghunt.git
cd easteregghunt

# Dependencies installieren
dotnet restore
```

### 🛠️ Entwicklung mit Skripten

Das Projekt bietet praktische Skripte für die Entwicklung:

#### **PowerShell (Windows)**
```powershell
# Entwicklung mit Hot-Reload starten
.\scripts\easter-egg-hunt.ps1 dev

# Nur API starten
.\scripts\easter-egg-hunt.ps1 dev -Project Api

# Nur Web starten  
.\scripts\easter-egg-hunt.ps1 dev -Project Web

# Ohne Hot-Reload
.\scripts\easter-egg-hunt.ps1 dev -HotReload:$false

# Tests ausführen
.\scripts\easter-egg-hunt.ps1 test

# Build
.\scripts\easter-egg-hunt.ps1 build

# Datenbank migrieren
.\scripts\easter-egg-hunt.ps1 migrate

# Clean
.\scripts\easter-egg-hunt.ps1 clean

# Hilfe anzeigen
.\scripts\easter-egg-hunt.ps1 help
```

#### **Bash (Linux/macOS)**
```bash
# Entwicklung mit Hot-Reload starten
./scripts/easter-egg-hunt.sh dev

# Nur API starten
./scripts/easter-egg-hunt.sh dev -p Api

# Nur Web starten
./scripts/easter-egg-hunt.sh dev -p Web

# Ohne Hot-Reload
./scripts/easter-egg-hunt.sh dev -h false

# Tests ausführen
./scripts/easter-egg-hunt.sh test

# Build
./scripts/easter-egg-hunt.sh build

# Datenbank migrieren
./scripts/easter-egg-hunt.sh migrate

# Clean
./scripts/easter-egg-hunt.sh clean

# Hilfe anzeigen
./scripts/easter-egg-hunt.sh help
```

### 🐳 Docker Deployment

```bash
# Mit Docker Compose (verwendet automatisch compose.yaml)
docker compose up

# Oder mit den Build-Skripten
.\scripts\easter-egg-hunt.ps1 build
```

### 📊 Verfügbare URLs

Nach dem Start sind folgende URLs verfügbar:
- **Web-Anwendung**: https://localhost:7002
- **API**: https://localhost:7001  
- **Swagger UI**: https://localhost:7001/swagger

### ⚡ Häufige Entwicklungskommandos

```bash
# Projekt starten (beide Services mit Hot-Reload)
.\scripts\easter-egg-hunt.ps1 dev

# Nur Tests ausführen
.\scripts\easter-egg-hunt.ps1 test

# Code formatieren
dotnet format

# Datenbank zurücksetzen
.\scripts\easter-egg-hunt.ps1 migrate

# Projekt bereinigen
.\scripts\easter-egg-hunt.ps1 clean
```

## 📖 Dokumentation

- **[Entwicklerhandbuch](docs/DEVELOPER_GUIDE.md)** - Umfassendes Handbuch für Entwickler
- **[Architektur](docs/ARCHITECTURE.md)** - Technische Architektur und Design-Entscheidungen
- **[Datenbank-Schema](docs/DATABASE_SCHEMA.md)** - ER-Diagramm und Datenbankdokumentation
- **[Sprint-Planung](docs/SPRINT_PLANNING.md)** - Detaillierte Sprint-Planung und User Stories
- **[Gherkin Features](features/)** - Detaillierte Feature-Spezifikationen
- **[Coding Guidelines](docs/CODING_GUIDELINES.md)** - Entwicklungsstandards
- **[Docker Build](docs/BUILD.md)** - Docker Buildx Bake Dokumentation
- **[Environment-Konfiguration](docs/ENVIRONMENT_CONFIGURATION.md)** - Umgebungs-spezifische Einstellungen
- **[Hot-Reload](docs/HOTRELOAD.md)** - Hot-Reload Konfiguration
- **[Contributing](docs/CONTRIBUTING.md)** - Beitrag zum Projekt
- **[Security](docs/SECURITY.md)** - Sicherheitsrichtlinien
- **[Changelog](docs/CHANGELOG.md)** - Versionshistorie

## 🤝 Beitragen

Wir freuen uns über Beiträge! Bitte lesen Sie unsere [Contributing Guidelines](docs/CONTRIBUTING.md) für Details zum Entwicklungsprozess.

## 📄 Lizenz

Dieses Projekt ist unter der [MIT Lizenz](LICENSE) lizenziert.

## 🔒 Sicherheit

Sicherheitslücken bitte über [GitHub Issues](../../issues) mit dem Label "security" melden. Siehe [SECURITY.md](docs/SECURITY.md) für Details.

## 📞 Support

- **Issues**: [GitHub Issues](../../issues)
- **Diskussionen**: [GitHub Discussions](../../discussions)
