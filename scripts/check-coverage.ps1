# Coverage Threshold Checker
# Reads thresholds from coverage-thresholds.json and validates coverage reports

param(
    [Parameter(Mandatory=$true)]
    [string]$CoverageReportPath,
    
    [Parameter(Mandatory=$false)]
    [string]$ThresholdsPath = "coverage-thresholds.json"
)

function Read-Thresholds {
    param([string]$Path)
    
    if (-not (Test-Path $Path)) {
        Write-Error "Thresholds file not found: $Path"
        exit 1
    }
    
    try {
        $content = Get-Content $Path -Raw | ConvertFrom-Json
        return $content.CoverageThresholds
    }
    catch {
        Write-Error "Failed to parse thresholds file: $_"
        exit 1
    }
}

function Get-CoverageFromReport {
    param([string]$ReportPath)
    
    if (-not (Test-Path $ReportPath)) {
        Write-Error "Coverage report not found: $ReportPath"
        exit 1
    }
    
    try {
        [xml]$xml = Get-Content $ReportPath
        $coverage = @{}
        
        foreach ($package in $xml.coverage.packages.package) {
            $name = $package.name
            $lineRate = [double]$package.'line-rate'
            $branchRate = [double]$package.'branch-rate'
            
            $coverage[$name] = @{
                Line = [math]::Round($lineRate * 100, 2)
                Branch = [math]::Round($branchRate * 100, 2)
            }
        }
        
        return $coverage
    }
    catch {
        Write-Error "Failed to parse coverage report: $_"
        exit 1
    }
}

function Test-CoverageThresholds {
    param(
        [hashtable]$Coverage,
        [object]$Thresholds
    )
    
    $allPassed = $true
    
    Write-Host "📊 Coverage Validation Results:" -ForegroundColor Cyan
    Write-Host "================================" -ForegroundColor Cyan
    
    # Check Domain coverage
    if ($Coverage.ContainsKey("EasterEggHunt.Domain")) {
        $domain = $Coverage["EasterEggHunt.Domain"]
        $domainThreshold = $Thresholds.Domain
        
        Write-Host "`n🏗️  Domain Layer:" -ForegroundColor Yellow
        Write-Host "   Line: $($domain.Line)% (threshold: $($domainThreshold.Line)%)" -ForegroundColor $(if ($domain.Line -ge $domainThreshold.Line) { "Green" } else { "Red" })
        Write-Host "   Branch: $($domain.Branch)% (threshold: $($domainThreshold.Branch)%)" -ForegroundColor $(if ($domain.Branch -ge $domainThreshold.Branch) { "Green" } else { "Red" })
        
        if ($domain.Line -lt $domainThreshold.Line -or $domain.Branch -lt $domainThreshold.Branch) {
            $allPassed = $false
        }
    }
    
    # Check Infrastructure coverage
    if ($Coverage.ContainsKey("EasterEggHunt.Infrastructure")) {
        $infra = $Coverage["EasterEggHunt.Infrastructure"]
        $infraThreshold = $Thresholds.Infrastructure
        
        Write-Host "`n🔧 Infrastructure Layer:" -ForegroundColor Yellow
        Write-Host "   Line: $($infra.Line)% (threshold: $($infraThreshold.Line)%)" -ForegroundColor $(if ($infra.Line -ge $infraThreshold.Line) { "Green" } else { "Red" })
        Write-Host "   Branch: $($infra.Branch)% (threshold: $($infraThreshold.Branch)%)" -ForegroundColor $(if ($infra.Branch -ge $infraThreshold.Branch) { "Green" } else { "Red" })
        
        if ($infra.Line -lt $infraThreshold.Line -or $infra.Branch -lt $infraThreshold.Branch) {
            $allPassed = $false
        }
    }
    
    # Check Application coverage
    if ($Coverage.ContainsKey("EasterEggHunt.Application")) {
        $app = $Coverage["EasterEggHunt.Application"]
        $appThreshold = $Thresholds.Application
        
        Write-Host "`n⚙️  Application Layer:" -ForegroundColor Yellow
        Write-Host "   Line: $($app.Line)% (threshold: $($appThreshold.Line)%)" -ForegroundColor $(if ($app.Line -ge $appThreshold.Line) { "Green" } else { "Red" })
        Write-Host "   Branch: $($app.Branch)% (threshold: $($appThreshold.Branch)%)" -ForegroundColor $(if ($app.Branch -ge $appThreshold.Branch) { "Green" } else { "Red" })
        
        if ($app.Line -lt $appThreshold.Line -or $app.Branch -lt $appThreshold.Branch) {
            $allPassed = $false
        }
    }
    
    Write-Host "`n================================" -ForegroundColor Cyan
    
    if ($allPassed) {
        Write-Host "✅ All coverage thresholds met!" -ForegroundColor Green
        return 0
    } else {
        Write-Host "❌ Some coverage thresholds not met!" -ForegroundColor Red
        return 1
    }
}

# Main execution
try {
    $thresholds = Read-Thresholds -Path $ThresholdsPath
    $coverage = Get-CoverageFromReport -ReportPath $CoverageReportPath
    $exitCode = Test-CoverageThresholds -Coverage $coverage -Thresholds $thresholds
    exit $exitCode
}
catch {
    Write-Error "Script execution failed: $_"
    exit 1
}
