# Easter Egg Hunt - Start Script (PowerShell)
# Baut Images mit Buildx Bake und startet Services mit Docker Compose

param(
    [string]$Environment = "dev",
    [string]$Tag = "latest"
)

# Farben für Output
$Colors = @{
    Red = "Red"
    Green = "Green"
    Yellow = "Yellow"
    Blue = "Blue"
    White = "White"
}

function Write-LogInfo {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor $Colors.Blue
}

function Write-LogSuccess {
    param([string]$Message)
    Write-Host "[SUCCESS] $Message" -ForegroundColor $Colors.Green
}

function Write-LogWarning {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor $Colors.Yellow
}

function Write-LogError {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor $Colors.Red
}

Write-LogInfo "Starting Easter Egg Hunt System"
Write-LogInfo "Environment: $Environment"
Write-LogInfo "Tag: $Tag"

# Prüfen ob Images existieren
$apiImageExists = $false
$webImageExists = $false

# Prüfe API Image
$apiResult = docker image inspect "easteregghunt/api:$Tag" 2>&1
if ($LASTEXITCODE -eq 0 -and $apiResult -notmatch "Error response from daemon") {
    $apiImageExists = $true
}

# Prüfe Web Image
$webResult = docker image inspect "easteregghunt/web:$Tag" 2>&1
if ($LASTEXITCODE -eq 0 -and $webResult -notmatch "Error response from daemon") {
    $webImageExists = $true
}

if ($apiImageExists -and $webImageExists) {
    Write-LogInfo "Using existing images"
} else {
    Write-LogWarning "Images not found. Building with Buildx Bake..."
    & ".\scripts\build.ps1" $Environment $Tag
    if ($LASTEXITCODE -ne 0) {
        Write-LogError "Build failed!"
        exit 1
    }
    Write-LogSuccess "Images built successfully"
}

# Services starten
Write-LogInfo "Starting services with Docker Compose..."
try {
    if ($Tag -ne "latest") {
        # Custom Tag verwenden
        $env:TAG = $Tag
        docker compose up -d --pull never
    } else {
        # Standard latest Tag
        docker compose up -d --pull never
    }
    
    if ($LASTEXITCODE -ne 0) {
        throw "Docker Compose failed"
    }
    
    # Status prüfen
    Start-Sleep -Seconds 3
    Write-LogInfo "Checking service status..."
    
    $status = docker compose ps --format table
    if ($status -match "Up") {
        Write-LogSuccess "Services started successfully!"
        Write-Host ""
        Write-LogInfo "Available services:"
        Write-Host "  • API: http://localhost:5001"
        Write-Host "  • Web: http://localhost:5000"
        Write-Host "  • SQLite Browser (optional): docker compose --profile tools up sqlite-browser"
        Write-Host ""
        Write-LogInfo "Useful commands:"
        Write-Host "  • View logs: docker compose logs -f"
        Write-Host "  • Stop services: docker compose down"
        Write-Host "  • Restart: docker compose restart"
    } else {
        Write-LogError "Some services failed to start!"
        docker compose ps
        exit 1
    }
    
} catch {
    Write-LogError "Failed to start services: $($_.Exception.Message)"
    docker compose ps
    exit 1
}
