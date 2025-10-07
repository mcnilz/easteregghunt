# PowerShell-Skript für verschiedene Umgebungen
# Startet die Anwendung in verschiedenen Umgebungen (Development, Staging, Production, Test)

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("Development", "Staging", "Production", "Test")]
    [string]$Environment,
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("Api", "Web", "Both")]
    [string]$Project = "Both"
)

Write-Host "🚀 Easter Egg Hunt - Environment Launcher" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green
Write-Host ""

# Wechsle zum Projektverzeichnis
Set-Location $PSScriptRoot\..

Write-Host "📁 Arbeitsverzeichnis: $(Get-Location)" -ForegroundColor Cyan
Write-Host "🌍 Umgebung: $Environment" -ForegroundColor Cyan
Write-Host "📦 Projekt: $Project" -ForegroundColor Cyan
Write-Host ""

# Setze Environment-Variable
$env:ASPNETCORE_ENVIRONMENT = $Environment

Write-Host "🔧 Environment-Variable gesetzt: ASPNETCORE_ENVIRONMENT=$Environment" -ForegroundColor Yellow

# Führe Migrationen aus (außer für Test)
if ($Environment -ne "Test") {
    Write-Host "🗄️ Führe Datenbank-Migrationen aus..." -ForegroundColor Yellow
    try {
        dotnet ef database update --project src/EasterEggHunt.Infrastructure --startup-project src/EasterEggHunt.Web
        Write-Host "✅ Datenbank-Migrationen erfolgreich ausgeführt" -ForegroundColor Green
    }
    catch {
        Write-Warning "⚠️ Fehler bei Datenbank-Migrationen: $($_.Exception.Message)"
        Write-Host "Versuche trotzdem fortzufahren..." -ForegroundColor Yellow
    }
    Write-Host ""
}

# Starte Projekte basierend auf Parameter
switch ($Project) {
    "Api" {
        Write-Host "📡 Starte API-Projekt..." -ForegroundColor Yellow
        dotnet run --project src/EasterEggHunt.Api --urls "https://localhost:7001;http://localhost:5001"
    }
    "Web" {
        Write-Host "🌐 Starte Web-Projekt..." -ForegroundColor Yellow
        dotnet run --project src/EasterEggHunt.Web --urls "https://localhost:7002;http://localhost:5002"
    }
    "Both" {
        Write-Host "🚀 Starte beide Projekte..." -ForegroundColor Yellow
        Write-Host ""
        
        # Starte API im Hintergrund
        Write-Host "📡 Starte API-Projekt im Hintergrund..." -ForegroundColor Yellow
        $apiJob = Start-Job -ScriptBlock {
            Set-Location $using:PWD
            $env:ASPNETCORE_ENVIRONMENT = $using:Environment
            dotnet run --project src/EasterEggHunt.Api --urls "https://localhost:7001;http://localhost:5001"
        }
        
        # Warte kurz
        Start-Sleep -Seconds 3
        
        # Starte Web im Vordergrund
        Write-Host "🌐 Starte Web-Projekt..." -ForegroundColor Yellow
        Write-Host ""
        Write-Host "📊 Verfügbare URLs:" -ForegroundColor Green
        Write-Host "   - Web-Anwendung: https://localhost:7002" -ForegroundColor White
        Write-Host "   - API: https://localhost:7001" -ForegroundColor White
        if ($Environment -eq "Development") {
            Write-Host "   - Swagger UI: https://localhost:7001/swagger" -ForegroundColor White
        }
        Write-Host ""
        Write-Host "Drücken Sie Ctrl+C zum Beenden..." -ForegroundColor Yellow
        Write-Host ""
        
        try {
            dotnet run --project src/EasterEggHunt.Web --urls "https://localhost:7002;http://localhost:5002"
        }
        finally {
            # Stoppe API-Job
            Write-Host ""
            Write-Host "🛑 Stoppe API-Projekt..." -ForegroundColor Yellow
            Stop-Job $apiJob -ErrorAction SilentlyContinue
            Remove-Job $apiJob -ErrorAction SilentlyContinue
        }
    }
}

Write-Host ""
Write-Host "✅ Anwendung wurde beendet" -ForegroundColor Green
