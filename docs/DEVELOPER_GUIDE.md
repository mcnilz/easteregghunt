# 🚀 Easter Egg Hunt System - Entwicklerhandbuch

## 📋 Übersicht

Dieses Handbuch konsolidiert alle wichtigen Informationen für die Entwicklung des Easter Egg Hunt Systems in einem zentralen Dokument.

## 🏗️ Projektstruktur

```
EasterEggHunt/
├── src/
│   ├── EasterEggHunt.Api/          # Web API (Backend)
│   ├── EasterEggHunt.Application/  # Business Logic Layer
│   ├── EasterEggHunt.Common/       # Shared Configuration & Utilities
│   ├── EasterEggHunt.Domain/       # Domain Models & Interfaces
│   ├── EasterEggHunt.Infrastructure/ # Data Access & External Services
│   └── EasterEggHunt.Web/          # MVC Frontend (API Client)
├── tests/                          # Test Projects
├── scripts/                        # Entwicklungsskripte
├── docs/                          # Dokumentation
└── features/                      # Gherkin Feature Files
```

## 🛠️ Entwicklungsumgebung

### Voraussetzungen

- **.NET 8.0 SDK** ([Download](https://dotnet.microsoft.com/download))
- **Visual Studio 2022** oder **VS Code** mit C# Extension
- **Git** für Versionskontrolle
- **Docker Desktop** (optional, für Containerisierung)

### Schnellstart

```bash
# 1. Repository klonen
git clone <repository-url>
cd easteregghunt

# 2. Entwicklungsumgebung starten
./scripts/easter-egg-hunt.sh dev

# 3. URLs öffnen
# - Web: https://localhost:7002
# - API: https://localhost:7001
# - Swagger: https://localhost:7001/swagger
```

## 🎯 Einheitliches Entwicklerskript

Das Projekt verwendet ein konsolidiertes Skript für alle Entwicklungsaufgaben:

### Windows (PowerShell)
```powershell
.\scripts\easter-egg-hunt.ps1 [COMMAND] [OPTIONS]
```

### Linux/macOS (Bash)
```bash
./scripts/easter-egg-hunt.sh [COMMAND] [OPTIONS]
```

### Verfügbare Befehle

| Befehl | Beschreibung | Beispiel |
|--------|-------------|----------|
| `dev` | Startet Entwicklungsumgebung | `./easter-egg-hunt.sh dev` |
| `build` | Führt Release-Build aus | `./easter-egg-hunt.sh build` |
| `test` | Führt alle Tests aus | `./easter-egg-hunt.sh test` |
| `migrate` | Führt DB-Migrationen aus | `./easter-egg-hunt.sh migrate` |
| `clean` | Löscht Build-Artefakte | `./easter-egg-hunt.sh clean` |
| `help` | Zeigt Hilfe an | `./easter-egg-hunt.sh help` |

### Optionen

| Option | Beschreibung | Werte | Standard |
|--------|-------------|-------|----------|
| `-e, --environment` | Zielumgebung | Development, Staging, Production, Test | Development |
| `-p, --project` | Zu startendes Projekt | Api, Web, Both | Both |
| `--no-hot-reload` | Hot-Reload deaktivieren | - | false |
| `-v, --verbose` | Ausführliche Ausgabe | - | false |

### Beispiele

```bash
# Standard-Entwicklungsumgebung
./easter-egg-hunt.sh dev

# Nur API in Production-Umgebung
./easter-egg-hunt.sh dev -e Production -p Api

# Tests mit Coverage
./easter-egg-hunt.sh test

# Datenbank-Migrationen
./easter-egg-hunt.sh migrate -e Staging

# Build-Artefakte löschen
./easter-egg-hunt.sh clean
```

## 🏗️ Architektur-Übersicht

Das Easter Egg Hunt System folgt **Clean Architecture** mit klarer Trennung zwischen Frontend und Backend:

### API-Client Architektur

```
┌─────────────────────────────────────────────────────────────┐
│                    WEB PROJECT (Frontend)                   │
├─────────────────────────────────────────────────────────────┤
│  MVC Controllers    │  Razor Views      │  API Client       │
│  - AdminController  │  - Admin Views    │  - HttpClient     │
│  - EmployeeController│  - Employee Views│  - JSON Serialization│
└─────────────────────────────────────────────────────────────┘
                                │ HTTP/JSON
┌─────────────────────────────────────────────────────────────┐
│                    API PROJECT (Backend)                    │
├─────────────────────────────────────────────────────────────┤
│  REST Controllers   │  Application Services │  Infrastructure │
│  - CampaignsController│  - CampaignService   │  - EF Core      │
│  - QrCodesController  │  - QrCodeService     │  - SQLite      │
│  - UsersController    │  - UserService        │  - Repositories│
│  - FindsController    │  - FindService        │                │
└─────────────────────────────────────────────────────────────┘
```

### Projekt-Verantwortlichkeiten

- **EasterEggHunt.Web**: MVC Frontend mit API-Client (keine direkte DB-Verbindung)
- **EasterEggHunt.Api**: REST API Backend mit Datenbankzugriff
- **EasterEggHunt.Application**: Business Logic und Services
- **EasterEggHunt.Domain**: Entities und Domain Models
- **EasterEggHunt.Infrastructure**: Data Access und externe Services
- **EasterEggHunt.Common**: Shared Configuration und Utilities (für zukünftige Verwendung)

### Migration-Strategie

- **Datenbank-Migrationen**: Werden über das API-Projekt ausgeführt
- **Web-Projekt**: Hat keine direkte Datenbankverbindung mehr
- **Entwicklungsskript**: Verwendet API-Projekt als Startup für Migrationen

## 🔥 Hot-Reload Konfiguration

### Automatische Aktivierung

Hot-Reload ist standardmäßig aktiviert und funktioniert automatisch mit `dotnet watch`:

```bash
# Hot-Reload aktiviert (Standard)
./easter-egg-hunt.sh dev

# Hot-Reload deaktiviert
./easter-egg-hunt.sh dev --no-hot-reload
```

### Unterstützte Änderungen

- **C# Code**: Automatische Neukompilierung und Neustart
- **Razor Views**: Sofortige Aktualisierung ohne Neustart
- **CSS/JS**: Live-Reload im Browser
- **Konfigurationsdateien**: Automatischer Neustart

### Troubleshooting

```bash
# Hot-Reload zurücksetzen
./easter-egg-hunt.sh clean
./easter-egg-hunt.sh dev

# Verbose-Modus für Debugging
./easter-egg-hunt.sh dev -v
```

## 🌍 Environment-Konfiguration

### Verfügbare Umgebungen

| Umgebung | Beschreibung | Datenbank | Logging |
|----------|-------------|-----------|---------|
| **Development** | Lokale Entwicklung | SQLite (easteregghunt.db) | Debug |
| **Staging** | Test-Umgebung | SQLite (staging.db) | Information |
| **Production** | Produktionsumgebung | SQLite (production.db) | Warning |
| **Test** | Unit/Integration Tests | In-Memory | None |

### Environment-spezifische Einstellungen

```json
// appsettings.Development.json
{
  "EasterEggHunt": {
    "Database": {
      "ConnectionString": "Data Source=easteregghunt.db"
    },
    "Security": {
      "RequireHttps": false,
      "AllowedOrigins": ["*"]
    },
    "Features": {
      "EnableUserRegistration": true,
      "EnableLeaderboard": true
    }
  }
}
```

### Umgebung wechseln

```bash
# Development (Standard)
./easter-egg-hunt.sh dev

# Staging
./easter-egg-hunt.sh dev -e Staging

# Production
./easter-egg-hunt.sh dev -e Production

# Test
./easter-egg-hunt.sh dev -e Test
```

## 🧪 Testing

### Test-Strategie

- **Unit Tests**: Einzelne Methoden und Klassen
- **Integration Tests**: Datenbankzugriffe und externe Services
- **API Tests**: Controller und Endpoints
- **End-to-End Tests**: Komplette User Journeys

### Test ausführen

```bash
# Alle Tests
./easter-egg-hunt.sh test

# Mit Coverage-Report
dotnet test --collect:"XPlat Code Coverage"

# Spezifisches Test-Projekt
dotnet test tests/EasterEggHunt.Api.Tests/
```

### Test Coverage

- **Ziel**: 90% Code Coverage
- **Aktuell**: 80% Line Coverage, 100% Branch Coverage
- **Coverage-Report**: `./coverage/report/index.html`

## 🗄️ Datenbank

### Migrationen

```bash
# Migrationen ausführen
./easter-egg-hunt.sh migrate

# Neue Migration erstellen
dotnet ef migrations add MigrationName --project src/EasterEggHunt.Infrastructure

# Migration zurücksetzen
dotnet ef database update PreviousMigration --project src/EasterEggHunt.Infrastructure
```

### Seed-Daten

Entwicklungsdaten werden automatisch erstellt:

- **Admin-Benutzer**: 
  - **Benutzername:** `admin`
  - **Passwort:** `admin123`
  - **E-Mail:** `admin@easteregghunt.local`
- **3 Test-Kampagnen**
- **15 QR-Codes**
- **5 Test-Benutzer**
- **17 Test-Funde**
- **3 aktive Sessions**

### 🔐 Admin-Login

**Login-URL:** `http://localhost:5000/Auth/Login` (automatische Weiterleitung)

**Login-Daten:**
- **Benutzername:** `admin`
- **Passwort:** `admin123`

**Features:**
- Cookie-basierte Authentifizierung
- Session-Management (8 Stunden)
- "Remember Me" Funktionalität
- Automatische Weiterleitung nach Login
- Logout-Button in der Navigation

## 🏗️ Build & Deployment

### Build

```bash
# Release-Build
./easter-egg-hunt.sh build

# Debug-Build
dotnet build

# Mit Tests
dotnet build --configuration Release
dotnet test --configuration Release
```

### Docker

```bash
# Docker Compose starten
docker-compose up -d

# Einzelne Services
docker-compose up api
docker-compose up web
```

## 📊 Monitoring & Logging

### Logging-Levels

- **Development**: Debug, Information, Warning, Error
- **Staging**: Information, Warning, Error
- **Production**: Warning, Error
- **Test**: None

### Log-Ausgabe

```bash
# Verbose-Logging aktivieren
./easter-egg-hunt.sh dev -v

# Logs in Datei
dotnet run > logs/app.log 2>&1
```

## 🔧 Entwicklungstools

### Code-Formatierung

```bash
# Automatische Formatierung
dotnet format

# Vor jedem Commit
git add .
dotnet format
git add .
git commit -m "feat: Neue Funktion"
```

### Code-Analyse

```bash
# Code-Analyse ausführen
dotnet build --verbosity normal

# Warnings als Errors behandeln
dotnet build --configuration Release --warnaserror
```

## 🐛 Troubleshooting

### Häufige Probleme

#### Port bereits belegt
```bash
# Prozesse auf Ports finden
netstat -ano | findstr :7001
netstat -ano | findstr :7002

# Prozesse beenden
taskkill /PID <PID> /F
```

#### Datenbank-Fehler
```bash
# Datenbank zurücksetzen
./easter-egg-hunt.sh clean
./easter-egg-hunt.sh migrate
```

#### Hot-Reload funktioniert nicht
```bash
# Clean und Neustart
./easter-egg-hunt.sh clean
./easter-egg-hunt.sh dev
```

### Debug-Modus

```bash
# Mit Debug-Informationen
./easter-egg-hunt.sh dev -v

# Einzelnes Projekt debuggen
dotnet run --project src/EasterEggHunt.Api --verbosity detailed
```

## 📚 Weitere Dokumentation

- [Architektur](ARCHITECTURE.md) - Clean Architecture Übersicht
- [Coding Guidelines](CODING_GUIDELINES.md) - Code-Qualitätsrichtlinien
- [Sprint Planning](SPRINT_PLANNING.md) - Projektplanung und User Stories
- [Datenbank Schema](DATABASE_SCHEMA.md) - Datenbankstruktur

## 🤝 Beitragen

### Workflow

1. **Feature Branch erstellen**
   ```bash
   git checkout -b feature/neue-funktion
   ```

2. **Entwickeln und testen**
   ```bash
   ./easter-egg-hunt.sh dev
   ./easter-egg-hunt.sh test
   ```

3. **Code formatieren**
   ```bash
   dotnet format
   ```

4. **Commit und Push**
   ```bash
   git add .
   git commit -m "feat: Neue Funktion implementiert"
   git push origin feature/neue-funktion
   ```

5. **Pull Request erstellen**

### Code-Review Checkliste

- [ ] Alle Tests bestehen
- [ ] Code Coverage ≥ 90%
- [ ] Coding Guidelines eingehalten
- [ ] Keine Compiler-Warnings
- [ ] Dokumentation aktualisiert
- [ ] Code formatiert (`dotnet format`)

---

**Dieses Handbuch wird kontinuierlich aktualisiert. Bei Fragen oder Problemen bitte ein Issue erstellen.**
