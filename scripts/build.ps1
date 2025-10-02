# Easter Egg Hunt - Build Script mit Docker Buildx Bake (PowerShell)
# Verwendung: .\scripts\build.ps1 [dev|prod] [tag]

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

# Build-Informationen sammeln
$BuildDate = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
try {
    $GitCommit = (git rev-parse --short HEAD 2>$null)
    if (-not $GitCommit) { $GitCommit = "unknown" }
} catch {
    $GitCommit = "unknown"
}

Write-LogInfo "Building Easter Egg Hunt System"
Write-LogInfo "Environment: $Environment"
Write-LogInfo "Tag: $Tag"
Write-LogInfo "Build Date: $BuildDate"
Write-LogInfo "Git Commit: $GitCommit"

# Cache-Verzeichnisse werden nicht mehr benötigt (Docker Driver Limitation)

# Buildx Builder erstellen/verwenden
$BuilderName = "easteregghunt-builder"
try {
    docker buildx inspect $BuilderName | Out-Null
    Write-LogInfo "Using existing buildx builder: $BuilderName"
    docker buildx use $BuilderName
} catch {
    Write-LogInfo "Creating buildx builder: $BuilderName"
    docker buildx create --name $BuilderName --use --bootstrap
}

# Umgebungsvariablen setzen
$env:REGISTRY = "easteregghunt"
$env:TAG = $Tag
$env:BUILD_DATE = $BuildDate
$env:GIT_COMMIT = $GitCommit

# Build ausführen
try {
    switch ($Environment.ToLower()) {
        "dev" {
            Write-LogInfo "Building development images..."
            docker buildx bake --file docker-bake.hcl dev
        }
        "prod" {
            Write-LogInfo "Building production images (multi-platform)..."
            docker buildx bake --file docker-bake.hcl prod
        }
        default {
            Write-LogError "Unknown environment: $Environment"
            Write-LogInfo "Usage: .\scripts\build.ps1 [dev|prod] [tag]"
            exit 1
        }
    }
    
    Write-LogSuccess "Build completed successfully!"
    
    # Images anzeigen
    Write-LogInfo "Built images:"
    docker images --filter "reference=easteregghunt/*:$Tag" --format "table {{.Repository}}\t{{.Tag}}\t{{.Size}}\t{{.CreatedAt}}"
    
    # Nächste Schritte
    Write-Host ""
    Write-LogInfo "Next steps:"
    Write-Host "  • Start services: docker compose up"
    Write-Host "  • View logs: docker compose logs -f"
    Write-Host "  • Stop services: docker compose down"
    
} catch {
    Write-LogError "Build failed: $($_.Exception.Message)"
    exit 1
}
