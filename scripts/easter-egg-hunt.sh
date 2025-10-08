#!/bin/bash
# Easter Egg Hunt System - Einheitliches Entwicklerskript (Bash)
# Konsolidiert alle Funktionen in einem einzigen, wartbaren Skript

# Globale Variablen
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
ORIGINAL_LOCATION="$(pwd)"
API_PROJECT="src/EasterEggHunt.Api"
WEB_PROJECT="src/EasterEggHunt.Web"
INFRASTRUCTURE_PROJECT="src/EasterEggHunt.Infrastructure"

# Standardwerte
COMMAND="dev"
ENVIRONMENT="Development"
PROJECT="Both"
HOT_RELOAD=true
VERBOSE=false

# Farben für bessere Lesbarkeit
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
MAGENTA='\033[0;35m'
CYAN='\033[0;36m'
WHITE='\033[1;37m'
NC='\033[0m' # No Color

# Hilfsfunktionen
print_header() {
    echo ""
    echo -e "${MAGENTA}🚀 $1${NC}"
    echo -e "${MAGENTA}$(printf '=%.0s' $(seq 1 $((${#1} + 3))))${NC}"
    echo ""
}

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠️ $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

print_info() {
    echo -e "${CYAN}ℹ️ $1${NC}"
}

test_dotnet() {
    if ! command -v dotnet &> /dev/null; then
        print_error ".NET SDK ist nicht installiert oder nicht im PATH verfügbar"
        print_info "Bitte installieren Sie das .NET 8.0 SDK von https://dotnet.microsoft.com/download"
        exit 1
    fi
    
    local version=$(dotnet --version)
    print_info ".NET SDK Version: $version"
}

set_environment() {
    local env=$1
    export ASPNETCORE_ENVIRONMENT=$env
    print_info "Environment gesetzt: ASPNETCORE_ENVIRONMENT=$env"
}

invoke_database_migration() {
    print_info "Führe Datenbank-Migrationen aus..."
    cd "$PROJECT_ROOT"
    
    if dotnet ef database update --project "$INFRASTRUCTURE_PROJECT" --startup-project "$WEB_PROJECT"; then
        print_success "Datenbank-Migrationen erfolgreich ausgeführt"
    else
        print_warning "Fehler bei Datenbank-Migrationen"
        print_info "Versuche trotzdem fortzufahren..."
    fi
}

start_development() {
    local env=$1
    local proj=$2
    local hot=$3
    
    print_header "Easter Egg Hunt Development Mode"
    print_info "Umgebung: $env"
    print_info "Projekt: $proj"
    print_info "Hot-Reload: $([ "$hot" = true ] && echo "Aktiviert" || echo "Deaktiviert")"
    
    set_environment "$env"
    
    # Migrationen ausführen (außer für Test)
    if [ "$env" != "Test" ]; then
        invoke_database_migration
    fi
    
    # Projekt-spezifische Logik
    case $proj in
        "Api")
            start_api_project "$hot"
            ;;
        "Web")
            start_web_project "$hot"
            ;;
        "Both")
            start_both_projects "$hot"
            ;;
    esac
}

start_api_project() {
    local hot=$1
    
    print_info "Starte API-Projekt..."
    cd "$PROJECT_ROOT"
    
    if [ "$hot" = true ]; then
        dotnet watch run --project "$API_PROJECT" --urls "https://localhost:7001;http://localhost:5001"
    else
        dotnet run --project "$API_PROJECT" --urls "https://localhost:7001;http://localhost:5001"
    fi
}

start_web_project() {
    local hot=$1
    
    print_info "Starte Web-Projekt..."
    cd "$PROJECT_ROOT"
    
    if [ "$hot" = true ]; then
        dotnet watch run --project "$WEB_PROJECT" --urls "https://localhost:7002;http://localhost:5002"
    else
        dotnet run --project "$WEB_PROJECT" --urls "https://localhost:7002;http://localhost:5002"
    fi
}

start_both_projects() {
    local hot=$1
    
    print_info "Starte beide Projekte..."
    
    # API im Hintergrund starten
    print_info "Starte API-Projekt im Hintergrund..."
    cd "$PROJECT_ROOT"
    
    if [ "$hot" = true ]; then
        dotnet watch run --project "$API_PROJECT" --urls "https://localhost:7001;http://localhost:5001" &
    else
        dotnet run --project "$API_PROJECT" --urls "https://localhost:7001;http://localhost:5001" &
    fi
    local api_pid=$!
    
    # Kurz warten
    sleep 3
    
    # URLs anzeigen
    echo ""
    echo -e "${GREEN}📊 Verfügbare URLs:${NC}"
    echo -e "${WHITE}   - Web-Anwendung: https://localhost:7002${NC}"
    echo -e "${WHITE}   - API: https://localhost:7001${NC}"
    if [ "$ENVIRONMENT" = "Development" ]; then
        echo -e "${WHITE}   - Swagger UI: https://localhost:7001/swagger${NC}"
    fi
    echo ""
    
    if [ "$hot" = true ]; then
        echo -e "${MAGENTA}🔥 Hot-Reload ist aktiviert - Änderungen werden automatisch übernommen!${NC}"
        echo ""
    fi
    
    echo -e "${YELLOW}Drücken Sie Ctrl+C zum Beenden...${NC}"
    echo ""
    
    # Cleanup-Funktion
    cleanup() {
        echo ""
        print_info "Stoppe API-Projekt..."
        kill $api_pid 2>/dev/null || true
        wait $api_pid 2>/dev/null || true
        exit 0
    }
    
    # Signal-Handler für sauberes Beenden
    trap cleanup SIGINT SIGTERM
    
    # Web-Projekt im Vordergrund starten
    if [ "$hot" = true ]; then
        dotnet watch run --project "$WEB_PROJECT" --urls "https://localhost:7002;http://localhost:5002"
    else
        dotnet run --project "$WEB_PROJECT" --urls "https://localhost:7002;http://localhost:5002"
    fi
}

invoke_build() {
    print_header "Build Easter Egg Hunt System"
    
    cd "$PROJECT_ROOT"
    
    print_info "Führe Build aus..."
    if dotnet build --configuration Release; then
        print_success "Build erfolgreich abgeschlossen"
    else
        print_error "Build fehlgeschlagen"
        exit 1
    fi
}

invoke_test() {
    print_header "Test Easter Egg Hunt System"
    
    cd "$PROJECT_ROOT"
    
    print_info "Führe Tests aus..."
    if dotnet test --configuration Release --collect:"XPlat Code Coverage" --results-directory ./coverage; then
        print_success "Alle Tests erfolgreich"
    else
        print_error "Tests fehlgeschlagen"
        exit 1
    fi
}

invoke_clean() {
    print_header "Clean Easter Egg Hunt System"
    
    cd "$PROJECT_ROOT"
    
    print_info "Lösche Build-Artefakte..."
    dotnet clean
    
    print_info "Lösche Test-Coverage..."
    if [ -d "./coverage" ]; then
        rm -rf "./coverage"
    fi
    
    print_info "Lösche bin/obj Verzeichnisse..."
    find . -type d -name "bin" -exec rm -rf {} + 2>/dev/null || true
    find . -type d -name "obj" -exec rm -rf {} + 2>/dev/null || true
    
    print_success "Clean erfolgreich abgeschlossen"
}

show_help() {
    print_header "Easter Egg Hunt System - Entwicklerhilfe"
    
    echo -e "${MAGENTA}VERWENDUNG:${NC}"
    echo -e "${WHITE}  ./easter-egg-hunt.sh [COMMAND] [OPTIONS]${NC}"
    echo ""
    
    echo -e "${MAGENTA}BEFEHLE:${NC}"
    echo -e "${WHITE}  dev        Startet die Entwicklungsumgebung (Standard)${NC}"
    echo -e "${WHITE}  build      Führt einen Release-Build aus${NC}"
    echo -e "${WHITE}  test       Führt alle Tests aus${NC}"
    echo -e "${WHITE}  migrate    Führt nur Datenbank-Migrationen aus${NC}"
    echo -e "${WHITE}  clean      Löscht alle Build-Artefakte${NC}"
    echo -e "${WHITE}  help       Zeigt diese Hilfe an${NC}"
    echo ""
    
    echo -e "${MAGENTA}OPTIONEN:${NC}"
    echo -e "${WHITE}  -e, --environment    Umgebung: Development, Staging, Production, Test${NC}"
    echo -e "${WHITE}  -p, --project        Projekt: Api, Web, Both${NC}"
    echo -e "${WHITE}  --no-hot-reload     Hot-Reload deaktivieren${NC}"
    echo -e "${WHITE}  -v, --verbose       Ausführliche Ausgabe${NC}"
    echo ""
    
    echo -e "${MAGENTA}BEISPIELE:${NC}"
    echo -e "${WHITE}  ./easter-egg-hunt.sh dev${NC}"
    echo -e "${WHITE}  ./easter-egg-hunt.sh dev -e Production -p Api${NC}"
    echo -e "${WHITE}  ./easter-egg-hunt.sh build${NC}"
    echo -e "${WHITE}  ./easter-egg-hunt.sh test${NC}"
    echo -e "${WHITE}  ./easter-egg-hunt.sh migrate${NC}"
    echo -e "${WHITE}  ./easter-egg-hunt.sh clean${NC}"
    echo ""
}

# Parameter-Parsing
while [[ $# -gt 0 ]]; do
    case $1 in
        dev|build|test|migrate|clean|help)
            COMMAND="$1"
            shift
            ;;
        -e|--environment)
            ENVIRONMENT="$2"
            shift 2
            ;;
        -p|--project)
            PROJECT="$2"
            shift 2
            ;;
        --no-hot-reload)
            HOT_RELOAD=false
            shift
            ;;
        -v|--verbose)
            VERBOSE=true
            shift
            ;;
        -h|--help)
            show_help
            exit 0
            ;;
        *)
            print_error "Unbekannte Option: $1"
            show_help
            exit 1
            ;;
    esac
done

# Hauptlogik
main() {
    # Prüfe .NET SDK
    test_dotnet
    
    # Wechsle zum Projektverzeichnis
    cd "$PROJECT_ROOT"
    
    # Führe Befehl aus
    case $COMMAND in
        "dev")
            start_development "$ENVIRONMENT" "$PROJECT" "$HOT_RELOAD"
            ;;
        "build")
            invoke_build
            ;;
        "test")
            invoke_test
            ;;
        "migrate")
            set_environment "$ENVIRONMENT"
            invoke_database_migration
            ;;
        "clean")
            invoke_clean
            ;;
        "help")
            show_help
            ;;
        *)
            print_error "Unbekannter Befehl: $COMMAND"
            show_help
            exit 1
            ;;
    esac
}

# Cleanup-Funktion
cleanup() {
    cd "$ORIGINAL_LOCATION"
}

# Signal-Handler für sauberes Beenden
trap cleanup EXIT

# Script ausführen
main "$@"
