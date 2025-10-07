# PowerShell-Skript für Entwicklung mit Hot-Reload
# Startet beide Projekte (API und Web) mit Hot-Reload-Unterstützung

Write-Host "🚀 Starte Easter Egg Hunt System mit Hot-Reload..." -ForegroundColor Green

# Prüfe ob dotnet verfügbar ist
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Error "❌ .NET SDK ist nicht installiert oder nicht im PATH verfügbar"
    exit 1
}

# Wechsle zum Projektverzeichnis
Set-Location $PSScriptRoot\..

# Führe Datenbank-Migrationen aus
Write-Host "🗄️ Führe Datenbank-Migrationen aus..." -ForegroundColor Yellow
try {
    dotnet ef database update --project src/EasterEggHunt.Infrastructure --startup-project src/EasterEggHunt.Web
    Write-Host "✅ Datenbank-Migrationen erfolgreich ausgeführt" -ForegroundColor Green
}
catch {
    Write-Warning "⚠️ Fehler bei Datenbank-Migrationen: $($_.Exception.Message)"
    Write-Host "Versuche trotzdem fortzufahren..." -ForegroundColor Yellow
}

# Starte API-Projekt im Hintergrund
Write-Host "📡 Starte API-Projekt mit Hot-Reload..." -ForegroundColor Yellow
$apiJob = Start-Job -ScriptBlock {
    Set-Location $using:PWD
    dotnet watch run --project src/EasterEggHunt.Api --urls "https://localhost:7001;http://localhost:5001"
}

# Warte kurz, damit die API startet
Start-Sleep -Seconds 3

# Starte Web-Projekt im Hintergrund
Write-Host "🌐 Starte Web-Projekt mit Hot-Reload..." -ForegroundColor Yellow
$webJob = Start-Job -ScriptBlock {
    Set-Location $using:PWD
    dotnet watch run --project src/EasterEggHunt.Web --urls "https://localhost:7002;http://localhost:5002"
}

# Warte kurz, damit das Web-Projekt startet
Start-Sleep -Seconds 3

Write-Host "✅ Beide Projekte wurden gestartet!" -ForegroundColor Green
Write-Host "📡 API: https://localhost:7001 (Swagger: https://localhost:7001/swagger)" -ForegroundColor Cyan
Write-Host "🌐 Web: https://localhost:7002" -ForegroundColor Cyan
Write-Host ""
Write-Host "🔥 Hot-Reload ist aktiviert - Änderungen werden automatisch übernommen!" -ForegroundColor Magenta
Write-Host ""
Write-Host "Drücke Ctrl+C zum Beenden..." -ForegroundColor Yellow

try {
    # Warte auf Benutzer-Eingabe
    while ($true) {
        Start-Sleep -Seconds 1
        
        # Prüfe ob Jobs noch laufen
        if ($apiJob.State -ne "Running" -or $webJob.State -ne "Running") {
            Write-Warning "⚠️  Eines der Projekte ist beendet worden"
            break
        }
    }
}
finally {
    # Stoppe alle Jobs
    Write-Host "🛑 Stoppe alle Projekte..." -ForegroundColor Yellow
    Stop-Job $apiJob, $webJob -ErrorAction SilentlyContinue
    Remove-Job $apiJob, $webJob -ErrorAction SilentlyContinue
    Write-Host "✅ Alle Projekte wurden gestoppt" -ForegroundColor Green
}
