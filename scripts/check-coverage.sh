#!/bin/bash

# Coverage Threshold Checker
# Reads thresholds from coverage-thresholds.json and validates coverage reports

set -e

COVERAGE_REPORT_PATH=""
THRESHOLDS_PATH="coverage-thresholds.json"

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --report)
            COVERAGE_REPORT_PATH="$2"
            shift 2
            ;;
        --thresholds)
            THRESHOLDS_PATH="$2"
            shift 2
            ;;
        -h|--help)
            echo "Usage: $0 --report <coverage-report-path> [--thresholds <thresholds-file>]"
            echo "  --report: Path to the coverage report XML file"
            echo "  --thresholds: Path to the thresholds JSON file (default: coverage-thresholds.json)"
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

if [[ -z "$COVERAGE_REPORT_PATH" ]]; then
    echo "Error: --report parameter is required"
    exit 1
fi

# Check if required tools are available
if ! command -v jq &> /dev/null; then
    echo "Error: jq is required but not installed. Please install jq."
    exit 1
fi

if ! command -v bc &> /dev/null; then
    echo "Error: bc is required but not installed. Please install bc."
    exit 1
fi

# Read thresholds from JSON file
read_thresholds() {
    local thresholds_file="$1"
    
    if [[ ! -f "$thresholds_file" ]]; then
        echo "Error: Thresholds file not found: $thresholds_file"
        exit 1
    fi
    
    # Extract thresholds using jq
    DOMAIN_LINE_THRESHOLD=$(jq -r '.CoverageThresholds.Domain.Line' "$thresholds_file")
    DOMAIN_BRANCH_THRESHOLD=$(jq -r '.CoverageThresholds.Domain.Branch' "$thresholds_file")
    INFRA_LINE_THRESHOLD=$(jq -r '.CoverageThresholds.Infrastructure.Line' "$thresholds_file")
    INFRA_BRANCH_THRESHOLD=$(jq -r '.CoverageThresholds.Infrastructure.Branch' "$thresholds_file")
    APP_LINE_THRESHOLD=$(jq -r '.CoverageThresholds.Application.Line' "$thresholds_file")
    APP_BRANCH_THRESHOLD=$(jq -r '.CoverageThresholds.Application.Branch' "$thresholds_file")
}

# Extract coverage from XML report
get_coverage_from_report() {
    local report_path="$1"
    
    if [[ ! -f "$report_path" ]]; then
        echo "Error: Coverage report not found: $report_path"
        exit 1
    fi
    
    # Extract coverage data using grep and sed
    DOMAIN_LINE=$(grep -A 5 -B 5 'EasterEggHunt.Domain' "$report_path" | grep 'line-rate=' | head -1 | sed 's/.*line-rate="//' | sed 's/".*//' | awk '{print $1 * 100}' | bc -l | cut -d. -f1)
    DOMAIN_BRANCH=$(grep -A 5 -B 5 'EasterEggHunt.Domain' "$report_path" | grep 'branch-rate=' | head -1 | sed 's/.*branch-rate="//' | sed 's/".*//' | awk '{print $1 * 100}' | bc -l | cut -d. -f1)
    
    INFRA_LINE=$(grep -A 5 -B 5 'EasterEggHunt.Infrastructure' "$report_path" | grep 'line-rate=' | head -1 | sed 's/.*line-rate="//' | sed 's/".*//' | awk '{print $1 * 100}' | bc -l | cut -d. -f1)
    INFRA_BRANCH=$(grep -A 5 -B 5 'EasterEggHunt.Infrastructure' "$report_path" | grep 'branch-rate=' | head -1 | sed 's/.*branch-rate="//' | sed 's/".*//' | awk '{print $1 * 100}' | bc -l | cut -d. -f1)
    
    APP_LINE=$(grep -A 5 -B 5 'EasterEggHunt.Application' "$report_path" | grep 'line-rate=' | head -1 | sed 's/.*line-rate="//' | sed 's/".*//' | awk '{print $1 * 100}' | bc -l | cut -d. -f1)
    APP_BRANCH=$(grep -A 5 -B 5 'EasterEggHunt.Application' "$report_path" | grep 'branch-rate=' | head -1 | sed 's/.*branch-rate="//' | sed 's/".*//' | awk '{print $1 * 100}' | bc -l | cut -d. -f1)
    
    # Handle cases where coverage might be 0 or empty
    DOMAIN_LINE=${DOMAIN_LINE:-0}
    DOMAIN_BRANCH=${DOMAIN_BRANCH:-0}
    INFRA_LINE=${INFRA_LINE:-0}
    INFRA_BRANCH=${INFRA_BRANCH:-0}
    APP_LINE=${APP_LINE:-0}
    APP_BRANCH=${APP_BRANCH:-0}
}

# Test coverage thresholds
test_coverage_thresholds() {
    local all_passed=true
    
    echo "📊 Coverage Validation Results:"
    echo "================================"
    
    # Check Domain coverage
    echo ""
    echo "🏗️  Domain Layer:"
    if [[ $DOMAIN_LINE -ge $DOMAIN_LINE_THRESHOLD ]]; then
        echo "   Line: ${DOMAIN_LINE}% (threshold: ${DOMAIN_LINE_THRESHOLD}%) ✅"
    else
        echo "   Line: ${DOMAIN_LINE}% (threshold: ${DOMAIN_LINE_THRESHOLD}%) ❌"
        all_passed=false
    fi
    
    if [[ $DOMAIN_BRANCH -ge $DOMAIN_BRANCH_THRESHOLD ]]; then
        echo "   Branch: ${DOMAIN_BRANCH}% (threshold: ${DOMAIN_BRANCH_THRESHOLD}%) ✅"
    else
        echo "   Branch: ${DOMAIN_BRANCH}% (threshold: ${DOMAIN_BRANCH_THRESHOLD}%) ❌"
        all_passed=false
    fi
    
    # Check Infrastructure coverage
    echo ""
    echo "🔧 Infrastructure Layer:"
    if [[ $INFRA_LINE -ge $INFRA_LINE_THRESHOLD ]]; then
        echo "   Line: ${INFRA_LINE}% (threshold: ${INFRA_LINE_THRESHOLD}%) ✅"
    else
        echo "   Line: ${INFRA_LINE}% (threshold: ${INFRA_LINE_THRESHOLD}%) ❌"
        all_passed=false
    fi
    
    if [[ $INFRA_BRANCH -ge $INFRA_BRANCH_THRESHOLD ]]; then
        echo "   Branch: ${INFRA_BRANCH}% (threshold: ${INFRA_BRANCH_THRESHOLD}%) ✅"
    else
        echo "   Branch: ${INFRA_BRANCH}% (threshold: ${INFRA_BRANCH_THRESHOLD}%) ❌"
        all_passed=false
    fi
    
    # Check Application coverage
    echo ""
    echo "⚙️  Application Layer:"
    if [[ $APP_LINE -ge $APP_LINE_THRESHOLD ]]; then
        echo "   Line: ${APP_LINE}% (threshold: ${APP_LINE_THRESHOLD}%) ✅"
    else
        echo "   Line: ${APP_LINE}% (threshold: ${APP_LINE_THRESHOLD}%) ❌"
        all_passed=false
    fi
    
    if [[ $APP_BRANCH -ge $APP_BRANCH_THRESHOLD ]]; then
        echo "   Branch: ${APP_BRANCH}% (threshold: ${APP_BRANCH_THRESHOLD}%) ✅"
    else
        echo "   Branch: ${APP_BRANCH}% (threshold: ${APP_BRANCH_THRESHOLD}%) ❌"
        all_passed=false
    fi
    
    echo ""
    echo "================================"
    
    if [[ "$all_passed" == "true" ]]; then
        echo "✅ All coverage thresholds met!"
        return 0
    else
        echo "❌ Some coverage thresholds not met!"
        return 1
    fi
}

# Main execution
read_thresholds "$THRESHOLDS_PATH"
get_coverage_from_report "$COVERAGE_REPORT_PATH"
test_coverage_thresholds
