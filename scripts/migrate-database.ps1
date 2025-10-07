# PowerShell-Skript für Datenbank-Migrationen
# Führt alle ausstehenden Migrationen aus

Write-Host "🗄️ Easter Egg Hunt - Datenbank-Migrationen" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green

# Prüfe ob dotnet verfügbar ist
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Error "❌ .NET SDK ist nicht installiert oder nicht im PATH verfügbar"
    exit 1
}

# Wechsle zum Projektverzeichnis
Set-Location $PSScriptRoot\..

Write-Host "📁 Arbeitsverzeichnis: $(Get-Location)" -ForegroundColor Cyan

# Führe Migrationen aus
Write-Host "🔄 Führe Datenbank-Migrationen aus..." -ForegroundColor Yellow

try {
    $result = dotnet ef database update --project src/EasterEggHunt.Infrastructure --startup-project src/EasterEggHunt.Web --verbose
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Datenbank-Migrationen erfolgreich ausgeführt!" -ForegroundColor Green
        Write-Host ""
        Write-Host "📊 Datenbank-Status:" -ForegroundColor Cyan
        Write-Host "   - Alle Migrationen wurden angewendet" -ForegroundColor White
        Write-Host "   - Datenbank ist bereit für die Anwendung" -ForegroundColor White
    }
    else {
        Write-Error "❌ Fehler bei der Ausführung der Migrationen"
        exit 1
    }
}
catch {
    Write-Error "❌ Unerwarteter Fehler: $($_.Exception.Message)"
    exit 1
}

Write-Host ""
Write-Host "🚀 Sie können jetzt die Anwendung starten mit:" -ForegroundColor Green
Write-Host "   .\scripts\dev-with-hotreload.ps1" -ForegroundColor White
Write-Host ""
Write-Host "Oder manuell:" -ForegroundColor Green
Write-Host "   dotnet run --project src/EasterEggHunt.Web" -ForegroundColor White
Write-Host "   dotnet run --project src/EasterEggHunt.Api" -ForegroundColor White
