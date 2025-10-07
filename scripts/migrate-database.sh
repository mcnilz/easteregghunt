#!/bin/bash

# Bash-Skript für Datenbank-Migrationen
# Führt alle ausstehenden Migrationen aus

echo "🗄️ Easter Egg Hunt - Datenbank-Migrationen"
echo "============================================="

# Prüfe ob dotnet verfügbar ist
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK ist nicht installiert oder nicht im PATH verfügbar"
    exit 1
fi

# Wechsle zum Projektverzeichnis
cd "$(dirname "$0")/.."

echo "📁 Arbeitsverzeichnis: $(pwd)"

# Führe Migrationen aus
echo "🔄 Führe Datenbank-Migrationen aus..."

if dotnet ef database update --project src/EasterEggHunt.Infrastructure --startup-project src/EasterEggHunt.Web --verbose; then
    echo "✅ Datenbank-Migrationen erfolgreich ausgeführt!"
    echo ""
    echo "📊 Datenbank-Status:"
    echo "   - Alle Migrationen wurden angewendet"
    echo "   - Datenbank ist bereit für die Anwendung"
else
    echo "❌ Fehler bei der Ausführung der Migrationen"
    exit 1
fi

echo ""
echo "🚀 Sie können jetzt die Anwendung starten mit:"
echo "   ./scripts/dev-with-hotreload.sh"
echo ""
echo "Oder manuell:"
echo "   dotnet run --project src/EasterEggHunt.Web"
echo "   dotnet run --project src/EasterEggHunt.Api"
