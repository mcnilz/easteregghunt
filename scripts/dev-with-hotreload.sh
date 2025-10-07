#!/bin/bash

# Bash-Skript für Entwicklung mit Hot-Reload
# Startet beide Projekte (API und Web) mit Hot-Reload-Unterstützung

echo "🚀 Starte Easter Egg Hunt System mit Hot-Reload..."

# Prüfe ob dotnet verfügbar ist
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK ist nicht installiert oder nicht im PATH verfügbar"
    exit 1
fi

# Wechsle zum Projektverzeichnis
cd "$(dirname "$0")/.."

# Funktion zum Stoppen aller Hintergrundprozesse
cleanup() {
    echo "🛑 Stoppe alle Projekte..."
    kill $API_PID $WEB_PID 2>/dev/null
    wait $API_PID $WEB_PID 2>/dev/null
    echo "✅ Alle Projekte wurden gestoppt"
    exit 0
}

# Setze Signal-Handler für sauberes Beenden
trap cleanup SIGINT SIGTERM

# Starte API-Projekt im Hintergrund
echo "📡 Starte API-Projekt mit Hot-Reload..."
dotnet watch run --project src/EasterEggHunt.Api --urls "https://localhost:7001;http://localhost:5001" &
API_PID=$!

# Warte kurz, damit die API startet
sleep 3

# Starte Web-Projekt im Hintergrund
echo "🌐 Starte Web-Projekt mit Hot-Reload..."
dotnet watch run --project src/EasterEggHunt.Web --urls "https://localhost:7002;http://localhost:5002" &
WEB_PID=$!

# Warte kurz, damit das Web-Projekt startet
sleep 3

echo "✅ Beide Projekte wurden gestartet!"
echo "📡 API: https://localhost:7001 (Swagger: https://localhost:7001/swagger)"
echo "🌐 Web: https://localhost:7002"
echo ""
echo "🔥 Hot-Reload ist aktiviert - Änderungen werden automatisch übernommen!"
echo ""
echo "Drücke Ctrl+C zum Beenden..."

# Warte auf Beendigung
wait
