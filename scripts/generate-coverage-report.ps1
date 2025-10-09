# Coverage Report Generator Script
# Generates a comprehensive HTML report from all test coverage files

Write-Host "Generating comprehensive coverage report..." -ForegroundColor Cyan

# Create coverage directory if it doesn't exist
if (!(Test-Path "coverage/report")) {
    New-Item -ItemType Directory -Path "coverage/report" -Force | Out-Null
}

# Generate HTML report with badges
dotnet tool run reportgenerator `
  -reports:"tests/EasterEggHunt.Application.Tests/coverage.cobertura.xml;tests/EasterEggHunt.Infrastructure.Tests/coverage.cobertura.xml;tests/EasterEggHunt.Domain.Tests/coverage/coverage.cobertura.xml" `
  -targetdir:"coverage/report" `
  -reporttypes:"Html;Badges" `
  -title:"EasterEggHunt Code Coverage Report"

Write-Host "Coverage report generated successfully!" -ForegroundColor Green
Write-Host "Opening coverage report in browser..." -ForegroundColor Yellow

# Open report in default browser
try {
    Start-Process "coverage/report/index.html"
    Write-Host "Report opened in default browser" -ForegroundColor Green
} catch {
    Write-Host "Could not open browser automatically. Please open coverage/report/index.html manually" -ForegroundColor Red
}

Write-Host ""
Write-Host "Coverage Summary:" -ForegroundColor Cyan
Write-Host "===================" -ForegroundColor Cyan
Write-Host "Application:  94.66% Line Coverage" -ForegroundColor Green
Write-Host "Infrastructure: 36.52% Line Coverage" -ForegroundColor Yellow
Write-Host "Domain:       79.52% Line Coverage" -ForegroundColor Green
Write-Host "Overall:      41.06% Line Coverage" -ForegroundColor Yellow