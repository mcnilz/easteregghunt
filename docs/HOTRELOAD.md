# Hot-Reload Konfiguration

Dieses Dokument beschreibt die Hot-Reload-Konfiguration für das Easter Egg Hunt System.

## Übersicht

Hot-Reload ermöglicht es, Änderungen am Code während der Entwicklung automatisch zu übernehmen, ohne die Anwendung neu starten zu müssen. Dies beschleunigt den Entwicklungsprozess erheblich.

## Konfiguration

### Projekt-Einstellungen

Beide Projekte (API und Web) sind für Hot-Reload konfiguriert:

- **EasterEggHunt.Api**: `HotReloadEnabled=true`
- **EasterEggHunt.Web**: `HotReloadEnabled=true` + `UseRazorSourceGenerator=true`

### Programm-Konfiguration

In beiden `Program.cs` Dateien ist Hot-Reload aktiviert:

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseHotReload();
    // ... andere Development-spezifische Middleware
}
```

### Konfigurationsdateien

- `hotreload.json`: Globale Hot-Reload-Konfiguration
- `appsettings.Development.json`: Entwicklungsspezifische Einstellungen

## Verwendung

### Automatischer Start

Verwenden Sie die bereitgestellten Skripte:

**Windows (PowerShell):**
```powershell
.\scripts\dev-with-hotreload.ps1
```

**Linux/macOS (Bash):**
```bash
./scripts/dev-with-hotreload.sh
```

### Datenbank-Migrationen

Die Skripte führen automatisch die Datenbank-Migrationen aus. Sie können diese auch manuell ausführen:

**Windows (PowerShell):**
```powershell
.\scripts\migrate-database.ps1
```

**Linux/macOS (Bash):**
```bash
./scripts/migrate-database.sh
```

**Manuell:**
```bash
dotnet ef database update --project src/EasterEggHunt.Infrastructure --startup-project src/EasterEggHunt.Web
```

### Manueller Start

**API-Projekt:**
```bash
dotnet watch run --project src/EasterEggHunt.Api
```

**Web-Projekt:**
```bash
dotnet watch run --project src/EasterEggHunt.Web
```

## Unterstützte Dateitypen

Hot-Reload funktioniert mit folgenden Dateitypen:

- ✅ C#-Dateien (`.cs`)
- ✅ Razor-Views (`.cshtml`)
- ✅ CSS-Dateien (`.css`)
- ✅ JavaScript-Dateien (`.js`)
- ✅ Konfigurationsdateien (`.json`)

## Ausgeschlossene Dateien

Folgende Dateien werden von Hot-Reload ausgeschlossen:

- `**/bin/**` - Kompilierte Binärdateien
- `**/obj/**` - Objektdateien
- `**/node_modules/**` - Node.js-Abhängigkeiten
- `**/.git/**` - Git-Verzeichnis
- `**/coverage/**` - Test-Coverage-Dateien
- `**/Migrations/**` - Entity Framework-Migrationen
- `**/*.db` - Datenbankdateien
- `**/*.db-shm` - SQLite-Shared-Memory-Dateien
- `**/*.db-wal` - SQLite-Write-Ahead-Log-Dateien

## URLs

Nach dem Start sind die Anwendungen verfügbar unter:

- **API**: https://localhost:7001
  - Swagger UI: https://localhost:7001/swagger
- **Web**: https://localhost:7002

## Troubleshooting

### Hot-Reload funktioniert nicht

1. Stellen Sie sicher, dass Sie sich im Development-Modus befinden
2. Überprüfen Sie, ob `HotReloadEnabled=true` in den Projektdateien gesetzt ist
3. Starten Sie die Anwendung mit `dotnet watch run`

### Performance-Probleme

1. Überprüfen Sie die `hotreload.json` Konfiguration
2. Stellen Sie sicher, dass große Verzeichnisse ausgeschlossen sind
3. Verwenden Sie `UseRazorSourceGenerator=true` für Razor-Views

### Fehler beim Start

1. Überprüfen Sie, ob alle Abhängigkeiten installiert sind
2. Führen Sie `dotnet restore` aus
3. Überprüfen Sie die Logs auf spezifische Fehlermeldungen

## Erweiterte Konfiguration

### Anpassung der Hot-Reload-Einstellungen

Bearbeiten Sie `hotreload.json`:

```json
{
  "hotReloadProfile": "aspnetcore",
  "exclude": [
    "**/bin/**",
    "**/obj/**"
  ],
  "include": [
    "**/*.cs",
    "**/*.cshtml"
  ]
}
```

### Entwicklungsumgebungs-spezifische Einstellungen

Bearbeiten Sie `appsettings.Development.json`:

```json
{
  "HotReload": {
    "Enabled": true,
    "Profile": "aspnetcore",
    "Exclude": [
      "**/bin/**",
      "**/obj/**"
    ]
  }
}
```

## Best Practices

1. **Verwenden Sie Hot-Reload nur in der Entwicklung** - In Produktionsumgebungen sollte Hot-Reload deaktiviert sein
2. **Überwachen Sie die Performance** - Hot-Reload kann die Anwendungsleistung beeinträchtigen
3. **Testen Sie regelmäßig** - Führen Sie Tests aus, um sicherzustellen, dass Änderungen korrekt übernommen werden
4. **Verwenden Sie Version Control** - Hot-Reload ersetzt nicht die Notwendigkeit, Änderungen zu committen

## Weitere Informationen

- [ASP.NET Core Hot Reload](https://docs.microsoft.com/en-us/aspnet/core/test/hot-reload)
- [.NET Hot Reload](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-watch)
