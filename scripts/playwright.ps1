#!/usr/bin/env pwsh
# Playwright Installation Script für Easter Egg Hunt Projekt
# Installiert Playwright-Browser für Frontend-Tests

param(
    [switch]$Force,
    [string]$ProjectPath = "tests/EasterEggHunt.Web.Tests"
)

Write-Host "🎭 Playwright Installation für Easter Egg Hunt" -ForegroundColor Cyan
Write-Host ""

# Prüfe ob Node.js installiert ist
try {
    $nodeVersion = node --version
    Write-Host "✅ Node.js gefunden: $nodeVersion" -ForegroundColor Green
}
catch {
    Write-Host "❌ Node.js ist nicht installiert!" -ForegroundColor Red
    Write-Host "   Bitte installiere Node.js von https://nodejs.org/" -ForegroundColor Yellow
    exit 1
}

# Prüfe ob npx verfügbar ist
try {
    $npxVersion = npx --version
    Write-Host "✅ npx gefunden: $npxVersion" -ForegroundColor Green
}
catch {
    Write-Host "❌ npx ist nicht verfügbar!" -ForegroundColor Red
    exit 1
}

# Wechsle zum Projekt-Verzeichnis
$originalPath = Get-Location
$projectPath = Join-Path $originalPath $ProjectPath

if (-not (Test-Path $projectPath)) {
    Write-Host "❌ Projekt-Pfad nicht gefunden: $projectPath" -ForegroundColor Red
    exit 1
}

Write-Host "📁 Wechsle zu: $projectPath" -ForegroundColor Yellow
Set-Location $projectPath

try {
    Write-Host ""
    Write-Host "🔧 Installiere Playwright-Browser..." -ForegroundColor Cyan
    Write-Host "   (Dies kann einige Minuten dauern)" -ForegroundColor Gray
    
    if ($Force) {
        Write-Host "   Force-Modus aktiviert - Browser werden neu installiert" -ForegroundColor Yellow
        npx playwright install --force
    }
    else {
        npx playwright install
    }
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✅ Playwright-Browser erfolgreich installiert!" -ForegroundColor Green
        Write-Host ""
        Write-Host "📝 Nächste Schritte:" -ForegroundColor Cyan
        Write-Host "   1. Führe die Tests aus: dotnet test --filter 'FullyQualifiedName~LoadingIndicators'" -ForegroundColor White
        Write-Host "   2. Oder alle Web-Tests: dotnet test tests/EasterEggHunt.Web.Tests/" -ForegroundColor White
    }
    else {
        Write-Host ""
        Write-Host "❌ Playwright-Installation fehlgeschlagen!" -ForegroundColor Red
        Write-Host "   Exit-Code: $LASTEXITCODE" -ForegroundColor Yellow
        exit 1
    }
}
catch {
    Write-Host ""
    Write-Host "❌ Fehler bei der Playwright-Installation:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Yellow
    exit 1
}
finally {
    # Zurück zum ursprünglichen Verzeichnis
    Set-Location $originalPath
}

Write-Host ""
Write-Host "✨ Fertig!" -ForegroundColor Green









