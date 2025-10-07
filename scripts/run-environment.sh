#!/bin/bash

# Bash-Skript für verschiedene Umgebungen
# Startet die Anwendung in verschiedenen Umgebungen (Development, Staging, Production, Test)

set -e

# Parameter prüfen
if [ $# -lt 1 ]; then
    echo "❌ Verwendung: $0 <Environment> [Project]"
    echo "   Environment: Development, Staging, Production, Test"
    echo "   Project: Api, Web, Both (Standard: Both)"
    exit 1
fi

ENVIRONMENT=$1
PROJECT=${2:-Both}

# Environment validieren
case $ENVIRONMENT in
    Development|Staging|Production|Test)
        ;;
    *)
        echo "❌ Ungültige Umgebung: $ENVIRONMENT"
        echo "   Gültige Umgebungen: Development, Staging, Production, Test"
        exit 1
        ;;
esac

# Project validieren
case $PROJECT in
    Api|Web|Both)
        ;;
    *)
        echo "❌ Ungültiges Projekt: $PROJECT"
        echo "   Gültige Projekte: Api, Web, Both"
        exit 1
        ;;
esac

echo "🚀 Easter Egg Hunt - Environment Launcher"
echo "========================================="
echo ""

# Wechsle zum Projektverzeichnis
cd "$(dirname "$0")/.."

echo "📁 Arbeitsverzeichnis: $(pwd)"
echo "🌍 Umgebung: $ENVIRONMENT"
echo "📦 Projekt: $PROJECT"
echo ""

# Setze Environment-Variable
export ASPNETCORE_ENVIRONMENT=$ENVIRONMENT

echo "🔧 Environment-Variable gesetzt: ASPNETCORE_ENVIRONMENT=$ENVIRONMENT"

# Führe Migrationen aus (außer für Test)
if [ "$ENVIRONMENT" != "Test" ]; then
    echo "🗄️ Führe Datenbank-Migrationen aus..."
    if dotnet ef database update --project src/EasterEggHunt.Infrastructure --startup-project src/EasterEggHunt.Web; then
        echo "✅ Datenbank-Migrationen erfolgreich ausgeführt"
    else
        echo "⚠️ Fehler bei Datenbank-Migrationen, versuche trotzdem fortzufahren..."
    fi
    echo ""
fi

# Starte Projekte basierend auf Parameter
case $PROJECT in
    "Api")
        echo "📡 Starte API-Projekt..."
        dotnet run --project src/EasterEggHunt.Api --urls "https://localhost:7001;http://localhost:5001"
        ;;
    "Web")
        echo "🌐 Starte Web-Projekt..."
        dotnet run --project src/EasterEggHunt.Web --urls "https://localhost:7002;http://localhost:5002"
        ;;
    "Both")
        echo "🚀 Starte beide Projekte..."
        echo ""
        
        # Starte API im Hintergrund
        echo "📡 Starte API-Projekt im Hintergrund..."
        dotnet run --project src/EasterEggHunt.Api --urls "https://localhost:7001;http://localhost:5001" &
        API_PID=$!
        
        # Warte kurz
        sleep 3
        
        # Starte Web im Vordergrund
        echo "🌐 Starte Web-Projekt..."
        echo ""
        echo "📊 Verfügbare URLs:"
        echo "   - Web-Anwendung: https://localhost:7002"
        echo "   - API: https://localhost:7001"
        if [ "$ENVIRONMENT" = "Development" ]; then
            echo "   - Swagger UI: https://localhost:7001/swagger"
        fi
        echo ""
        echo "Drücken Sie Ctrl+C zum Beenden..."
        echo ""
        
        # Cleanup-Funktion
        cleanup() {
            echo ""
            echo "🛑 Stoppe alle Projekte..."
            kill $API_PID 2>/dev/null || true
            wait $API_PID 2>/dev/null || true
            echo "✅ Alle Projekte wurden gestoppt"
        }
        
        # Trap für Ctrl+C
        trap cleanup INT TERM
        
        # Starte Web-Projekt
        dotnet run --project src/EasterEggHunt.Web --urls "https://localhost:7002;http://localhost:5002"
        
        # Cleanup
        cleanup
        ;;
esac

echo ""
echo "✅ Anwendung wurde beendet"
