# Easter Egg Hunt System - Einheitliches Entwicklerskript
# Konsolidiert alle Funktionen in einem einzigen, wartbaren Skript

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("dev", "build", "test", "migrate", "clean", "help")]
    [string]$Command = "dev",
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("Development", "Staging", "Production", "Test")]
    [string]$Environment = "Development",
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("Api", "Web", "Both")]
    [string]$Project = "Both",
    
    [Parameter(Mandatory=$false)]
    [switch]$HotReload = $true,
    
    [Parameter(Mandatory=$false)]
    [switch]$ShowDebug = $false
)

# Globale Variablen
$ScriptDir = $PSScriptRoot
$ProjectRoot = Split-Path $ScriptDir -Parent
$OriginalLocation = Get-Location
$ApiProject = "src/EasterEggHunt.Api"
$WebProject = "src/EasterEggHunt.Web"
$InfrastructureProject = "src/EasterEggHunt.Infrastructure"

# Farben für bessere Lesbarkeit
$Colors = @{
    Success = "Green"
    Warning = "Yellow"
    Error = "Red"
    Info = "Cyan"
    Header = "Magenta"
    Detail = "White"
}

function Write-Header {
    param([string]$Message)
    Write-Host ""
    Write-Host "🚀 $Message" -ForegroundColor $Colors.Header
    Write-Host ("=" * ($Message.Length + 3)) -ForegroundColor $Colors.Header
    Write-Host ""
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor $Colors.Success
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠️ $Message" -ForegroundColor $Colors.Warning
}

function Write-Error {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor $Colors.Error
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ️ $Message" -ForegroundColor $Colors.Info
}

function Test-DotNet {
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        Write-Error ".NET SDK ist nicht installiert oder nicht im PATH verfügbar"
        Write-Info "Bitte installieren Sie das .NET 8.0 SDK von https://dotnet.microsoft.com/download"
        exit 1
    }
    
    $version = dotnet --version
    Write-Info ".NET SDK Version: $version"
}

function Set-Environment {
    param([string]$Env)
    $env:ASPNETCORE_ENVIRONMENT = $Env
    Write-Info "Environment gesetzt: ASPNETCORE_ENVIRONMENT=$Env"
}

function Invoke-DatabaseMigration {
    Write-Info "Führe Datenbank-Migrationen aus..."
    try {
        Set-Location $ProjectRoot
        dotnet ef database update --project $InfrastructureProject --startup-project $WebProject
        Write-Success "Datenbank-Migrationen erfolgreich ausgeführt"
    }
    catch {
        Write-Warning "Fehler bei Datenbank-Migrationen: $($_.Exception.Message)"
        Write-Info "Versuche trotzdem fortzufahren..."
    }
}

function Start-Development {
    param([string]$Env, [string]$Proj, [bool]$Hot)
    
    Write-Header "Easter Egg Hunt Development Mode"
    Write-Info "Umgebung: $Env"
    Write-Info "Projekt: $Proj"
    Write-Info "Hot-Reload: $(if($Hot) { 'Aktiviert' } else { 'Deaktiviert' })"
    
    Set-Environment $Env
    
    # Migrationen ausführen (außer für Test)
    if ($Env -ne "Test") {
        Invoke-DatabaseMigration
    }
    
    # Projekt-spezifische Logik
    switch ($Proj) {
        "Api" {
            Start-ApiProject $Hot
        }
        "Web" {
            Start-WebProject $Hot
        }
        "Both" {
            Start-BothProjects $Hot
        }
    }
}

function Start-ApiProject {
    param([bool]$Hot)
    
    Write-Info "Starte API-Projekt..."
    $urls = "--urls `"https://localhost:7001;http://localhost:5001`""
    
    Set-Location $ProjectRoot
    if ($Hot) {
        dotnet watch run --project $ApiProject $urls
    } else {
        dotnet run --project $ApiProject $urls
    }
}

function Start-WebProject {
    param([bool]$Hot)
    
    Write-Info "Starte Web-Projekt..."
    $urls = "--urls `"https://localhost:7002;http://localhost:5002`""
    
    Set-Location $ProjectRoot
    if ($Hot) {
        dotnet watch run --project $WebProject $urls
    } else {
        dotnet run --project $WebProject $urls
    }
}

function Start-BothProjects {
    param([bool]$Hot)
    
    Write-Info "Starte beide Projekte..."
    
    # API im Hintergrund starten
    Write-Info "Starte API-Projekt im Hintergrund..."
    $apiJob = Start-Job -ScriptBlock {
        Set-Location $using:ProjectRoot
        $env:ASPNETCORE_ENVIRONMENT = $using:Environment
        if ($using:Hot) {
            dotnet watch run --project $using:ApiProject --urls "https://localhost:7001;http://localhost:5001"
        } else {
            dotnet run --project $using:ApiProject --urls "https://localhost:7001;http://localhost:5001"
        }
    }
    
    # Kurz warten
    Start-Sleep -Seconds 3
    
    # URLs anzeigen
    Write-Host ""
    Write-Host "📊 Verfügbare URLs:" -ForegroundColor $Colors.Success
    Write-Host "   - Web-Anwendung: https://localhost:7002" -ForegroundColor $Colors.Detail
    Write-Host "   - API: https://localhost:7001" -ForegroundColor $Colors.Detail
    if ($Environment -eq "Development") {
        Write-Host "   - Swagger UI: https://localhost:7001/swagger" -ForegroundColor $Colors.Detail
    }
    Write-Host ""
    
    if ($Hot) {
        Write-Host "🔥 Hot-Reload ist aktiviert - Änderungen werden automatisch übernommen!" -ForegroundColor $Colors.Header
        Write-Host ""
    }
    
    Write-Host "Drücken Sie Ctrl+C zum Beenden..." -ForegroundColor $Colors.Warning
    Write-Host ""
    
    try {
        # Web-Projekt im Vordergrund starten
        if ($Hot) {
            dotnet watch run --project $WebProject --urls "https://localhost:7002;http://localhost:5002"
        } else {
            dotnet run --project $WebProject --urls "https://localhost:7002;http://localhost:5002"
        }
    }
    finally {
        # API-Job stoppen
        Write-Host ""
        Write-Info "Stoppe API-Projekt..."
        Stop-Job $apiJob -ErrorAction SilentlyContinue
        Remove-Job $apiJob -ErrorAction SilentlyContinue
    }
}

function Invoke-Build {
    Write-Header "Build Easter Egg Hunt System"
    
    Set-Location $ProjectRoot
    
    Write-Info "Führe Build aus..."
    dotnet build --configuration Release
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Build erfolgreich abgeschlossen"
    } else {
        Write-Error "Build fehlgeschlagen"
        exit 1
    }
}

function Invoke-Test {
    Write-Header "Test Easter Egg Hunt System"
    
    Set-Location $ProjectRoot
    
    Write-Info "Führe Tests aus..."
    dotnet test --configuration Release --collect:"XPlat Code Coverage" --results-directory ./coverage
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Alle Tests erfolgreich"
    } else {
        Write-Error "Tests fehlgeschlagen"
        exit 1
    }
}

function Invoke-Clean {
    Write-Header "Clean Easter Egg Hunt System"
    
    Set-Location $ProjectRoot
    
    Write-Info "Lösche Build-Artefakte..."
    dotnet clean
    
    Write-Info "Lösche Test-Coverage..."
    if (Test-Path "./coverage") {
        Remove-Item -Recurse -Force "./coverage"
    }
    
    Write-Info "Lösche bin/obj Verzeichnisse..."
    Get-ChildItem -Path . -Recurse -Directory -Name "bin" | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
    Get-ChildItem -Path . -Recurse -Directory -Name "obj" | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
    
    Write-Success "Clean erfolgreich abgeschlossen"
}

function Show-Help {
    Write-Header "Easter Egg Hunt System - Entwicklerhilfe"
    
    Write-Host "VERWENDUNG:" -ForegroundColor $Colors.Header
    Write-Host "  .\easter-egg-hunt.ps1 [COMMAND] [OPTIONS]" -ForegroundColor $Colors.Detail
    Write-Host ""
    
    Write-Host "BEFEHLE:" -ForegroundColor $Colors.Header
    Write-Host "  dev        Startet die Entwicklungsumgebung (Standard)" -ForegroundColor $Colors.Detail
    Write-Host "  build      Führt einen Release-Build aus" -ForegroundColor $Colors.Detail
    Write-Host "  test       Führt alle Tests aus" -ForegroundColor $Colors.Detail
    Write-Host "  migrate    Führt nur Datenbank-Migrationen aus" -ForegroundColor $Colors.Detail
    Write-Host "  clean      Löscht alle Build-Artefakte" -ForegroundColor $Colors.Detail
    Write-Host "  help       Zeigt diese Hilfe an" -ForegroundColor $Colors.Detail
    Write-Host ""
    
    Write-Host "OPTIONEN:" -ForegroundColor $Colors.Header
    Write-Host "  -Environment    Umgebung: Development, Staging, Production, Test" -ForegroundColor $Colors.Detail
    Write-Host "  -Project        Projekt: Api, Web, Both" -ForegroundColor $Colors.Detail
    Write-Host "  -HotReload      Hot-Reload aktivieren (Standard: true)" -ForegroundColor $Colors.Detail
    Write-Host "  -ShowDebug      Debug-Ausgabe" -ForegroundColor $Colors.Detail
    Write-Host ""
    
    Write-Host "BEISPIELE:" -ForegroundColor $Colors.Header
    Write-Host "  .\easter-egg-hunt.ps1 dev" -ForegroundColor $Colors.Detail
    Write-Host "  .\easter-egg-hunt.ps1 dev -Environment Production -Project Api" -ForegroundColor $Colors.Detail
    Write-Host "  .\easter-egg-hunt.ps1 build" -ForegroundColor $Colors.Detail
    Write-Host "  .\easter-egg-hunt.ps1 test" -ForegroundColor $Colors.Detail
    Write-Host "  .\easter-egg-hunt.ps1 migrate" -ForegroundColor $Colors.Detail
    Write-Host "  .\easter-egg-hunt.ps1 clean" -ForegroundColor $Colors.Detail
    Write-Host ""
}

# Hauptlogik
try {
    # Prüfe .NET SDK
    Test-DotNet
    
    # Wechsle zum Projektverzeichnis
    Set-Location $ProjectRoot
    
    # Führe Befehl aus
    switch ($Command) {
        "dev" {
            Start-Development $Environment $Project $HotReload
        }
        "build" {
            Invoke-Build
        }
        "test" {
            Invoke-Test
        }
        "migrate" {
            Set-Environment $Environment
            Invoke-DatabaseMigration
        }
        "clean" {
            Invoke-Clean
        }
        "help" {
            Show-Help
        }
        default {
            Write-Error "Unbekannter Befehl: $Command"
            Show-Help
            exit 1
        }
    }
}
catch {
    Write-Error "Fehler: $($_.Exception.Message)"
    if ($ShowDebug) {
        Write-Host $_.ScriptStackTrace -ForegroundColor $Colors.Error
    }
    exit 1
}
finally {
    # Zurück zum ursprünglichen Verzeichnis
    Set-Location $OriginalLocation
}
