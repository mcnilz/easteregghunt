# Environment-Konfiguration

Dieses Dokument beschreibt die verschiedenen Umgebungskonfigurationen für das Easter Egg Hunt System.

## Übersicht

Das System unterstützt vier verschiedene Umgebungen:

- **Development**: Lokale Entwicklung
- **Staging**: Testumgebung vor Produktion
- **Production**: Live-Produktionsumgebung
- **Test**: Automatisierte Tests

## Konfigurationsdateien

### API-Projekt (`src/EasterEggHunt.Api/`)

- `appsettings.json` - Basis-Konfiguration
- `appsettings.Development.json` - Entwicklungsumgebung
- `appsettings.Staging.json` - Staging-Umgebung
- `appsettings.Production.json` - Produktionsumgebung
- `appsettings.Test.json` - Test-Umgebung

### Web-Projekt (`src/EasterEggHunt.Web/`)

- `appsettings.json` - Basis-Konfiguration
- `appsettings.Development.json` - Entwicklungsumgebung
- `appsettings.Staging.json` - Staging-Umgebung
- `appsettings.Production.json` - Produktionsumgebung
- `appsettings.Test.json` - Test-Umgebung

## Konfigurationsstruktur

Alle Konfigurationsdateien folgen der gleichen Struktur unter dem `EasterEggHunt`-Abschnitt:

```json
{
  "EasterEggHunt": {
    "Database": {
      "Provider": "SQLite",
      "ConnectionString": "Data Source=easteregghunt.db"
    },
    "Security": {
      "RequireHttps": true,
      "AllowedOrigins": ["https://yourdomain.com"]
    },
    "Features": {
      "EnableSwagger": true,
      "EnableDetailedErrors": true,
      "EnableSensitiveDataLogging": false
    },
    "Performance": {
      "EnableResponseCompression": true,
      "EnableResponseCaching": true,
      "CacheExpirationMinutes": 5
    },
    "UI": {
      "Title": "Easter Egg Hunt System",
      "CompanyName": "Your Company",
      "SupportEmail": "support@yourdomain.com"
    }
  }
}
```

## Umgebungsdetails

### Development

**Zweck**: Lokale Entwicklung und Debugging

**Eigenschaften**:
- Swagger UI aktiviert
- Detaillierte Fehlermeldungen
- Sensible Daten-Logging aktiviert
- Hot-Reload aktiviert
- Keine HTTPS-Anforderung
- Alle CORS-Origins erlaubt

**Datenbank**: `easteregghunt_development.db`

### Staging

**Zweck**: Testumgebung vor Produktion

**Eigenschaften**:
- Swagger UI aktiviert (für API-Tests)
- Detaillierte Fehlermeldungen
- Keine sensiblen Daten-Logs
- HTTPS erforderlich
- Begrenzte CORS-Origins
- Response-Kompression aktiviert

**Datenbank**: `easteregghunt_staging.db`

### Production

**Zweck**: Live-Produktionsumgebung

**Eigenschaften**:
- Swagger UI deaktiviert
- Keine detaillierten Fehlermeldungen
- Keine sensiblen Daten-Logs
- HTTPS erforderlich
- Strenge CORS-Konfiguration
- Alle Performance-Features aktiviert

**Datenbank**: `easteregghunt_production.db`

### Test

**Zweck**: Automatisierte Tests

**Eigenschaften**:
- Swagger UI deaktiviert
- Keine detaillierten Fehlermeldungen
- Keine sensiblen Daten-Logs
- Keine HTTPS-Anforderung
- Alle CORS-Origins erlaubt
- Keine Performance-Features

**Datenbank**: `easteregghunt_test.db`

## Verwendung

### Automatische Skripte

**Windows (PowerShell)**:
```powershell
# Development (Standard)
.\scripts\run-environment.ps1 Development

# Staging
.\scripts\run-environment.ps1 Staging

# Production
.\scripts\run-environment.ps1 Production

# Test
.\scripts\run-environment.ps1 Test

# Nur API starten
.\scripts\run-environment.ps1 Development Api

# Nur Web starten
.\scripts\run-environment.ps1 Development Web
```

**Linux/macOS (Bash)**:
```bash
# Development (Standard)
./scripts/run-environment.sh Development

# Staging
./scripts/run-environment.sh Staging

# Production
./scripts/run-environment.sh Production

# Test
./scripts/run-environment.sh Test

# Nur API starten
./scripts/run-environment.sh Development Api

# Nur Web starten
./scripts/run-environment.sh Development Web
```

### Manuelle Ausführung

**API-Projekt**:
```bash
# Development
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/EasterEggHunt.Api

# Staging
ASPNETCORE_ENVIRONMENT=Staging dotnet run --project src/EasterEggHunt.Api

# Production
ASPNETCORE_ENVIRONMENT=Production dotnet run --project src/EasterEggHunt.Api

# Test
ASPNETCORE_ENVIRONMENT=Test dotnet run --project src/EasterEggHunt.Api
```

**Web-Projekt**:
```bash
# Development
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/EasterEggHunt.Web

# Staging
ASPNETCORE_ENVIRONMENT=Staging dotnet run --project src/EasterEggHunt.Web

# Production
ASPNETCORE_ENVIRONMENT=Production dotnet run --project src/EasterEggHunt.Web

# Test
ASPNETCORE_ENVIRONMENT=Test dotnet run --project src/EasterEggHunt.Web
```

## Datenbank-Migrationen

Die Skripte führen automatisch Datenbank-Migrationen aus (außer für Test-Umgebung):

```bash
# Manuell für alle Umgebungen
dotnet ef database update --project src/EasterEggHunt.Infrastructure --startup-project src/EasterEggHunt.Web

# Für spezifische Umgebung
ASPNETCORE_ENVIRONMENT=Production dotnet ef database update --project src/EasterEggHunt.Infrastructure --startup-project src/EasterEggHunt.Web
```

## Konfiguration anpassen

### Neue Umgebung hinzufügen

1. Erstelle neue `appsettings.{Environment}.json` Dateien
2. Passe die Konfigurationswerte an
3. Aktualisiere die Skripte falls nötig

### Konfigurationswerte ändern

1. Bearbeite die entsprechende `appsettings.{Environment}.json` Datei
2. Starte die Anwendung neu

### Neue Konfigurationsoptionen

1. Erweitere `EasterEggHuntOptions` in `src/EasterEggHunt.Domain/Configuration/`
2. Aktualisiere die Extension-Methoden in `src/EasterEggHunt.Infrastructure/Configuration/`
3. Füge die neuen Optionen zu allen `appsettings.{Environment}.json` Dateien hinzu

## Sicherheitshinweise

- **Production**: Verwende starke Passwörter und sichere Verbindungszeichenfolgen
- **Staging**: Ähnlich wie Production, aber mit Testdaten
- **Development**: Kann unsichere Einstellungen haben, aber nie in Production verwenden
- **Test**: Isoliert von anderen Umgebungen

## Troubleshooting

### Konfiguration wird nicht geladen

- Prüfe, ob die `ASPNETCORE_ENVIRONMENT` Variable korrekt gesetzt ist
- Stelle sicher, dass die `appsettings.{Environment}.json` Datei existiert
- Prüfe die JSON-Syntax auf Fehler

### Datenbank-Probleme

- Führe Migrationen manuell aus
- Prüfe die Verbindungszeichenfolge
- Stelle sicher, dass die Datenbankdatei schreibbar ist

### CORS-Probleme

- Prüfe die `AllowedOrigins` Konfiguration
- Stelle sicher, dass die URLs korrekt sind
- Für Development können `AllowAnyOrigin()` verwendet werden
