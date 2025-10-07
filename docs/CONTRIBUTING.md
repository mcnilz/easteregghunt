# Beitrag zum Easter Egg Hunt System

Vielen Dank für Ihr Interesse, zum Easter Egg Hunt System beizutragen! 🎉

## 📋 Übersicht

Dieses Dokument beschreibt, wie Sie zu diesem Projekt beitragen können. Bitte lesen Sie es vollständig durch, bevor Sie Änderungen vornehmen.

## 🚀 Erste Schritte

### Voraussetzungen

- .NET 8.0 SDK oder höher
- Visual Studio 2022 oder Visual Studio Code
- Git

### Repository klonen

```bash
git clone https://github.com/your-org/easter-egg-hunt.git
cd easter-egg-hunt
```

### Entwicklungsumgebung einrichten

```bash
# Dependencies installieren
dotnet restore

# Tests ausführen
dotnet test

# Code Coverage prüfen
dotnet test --collect:"XPlat Code Coverage"
```

## 📝 Coding Guidelines

Bitte beachten Sie unsere [Coding Guidelines](CODING_GUIDELINES.md) für:

- **Sprache**: Variablen auf Englisch, Kommentare auf Deutsch
- **Test Coverage**: Minimum 90%, Ziel 100%
- **Code Style**: Definiert in `.editorconfig`
- **Static Analysis**: Konfiguriert in `code-analysis.ruleset`

## 🔄 Entwicklungsprozess

### 1. Issue erstellen oder auswählen

- Schauen Sie in die [Issues](../../issues) für offene Aufgaben
- Erstellen Sie ein neues Issue für neue Features oder Bugs
- Kommentieren Sie das Issue, wenn Sie daran arbeiten möchten

### 2. Branch erstellen

```bash
# Feature Branch
git checkout -b feature/beschreibung-des-features

# Bugfix Branch
git checkout -b bugfix/beschreibung-des-bugs

# Hotfix Branch
git checkout -b hotfix/beschreibung-des-hotfixes
```

### 3. Entwicklung

#### Test-Driven Development (TDD)

1. **Test schreiben** - Schreiben Sie zuerst den Test
2. **Test fehlschlagen lassen** - Stellen Sie sicher, dass der Test rot ist
3. **Code implementieren** - Implementieren Sie die minimale Lösung
4. **Test erfolgreich machen** - Stellen Sie sicher, dass der Test grün ist
5. **Refactoring** - Verbessern Sie den Code ohne Tests zu brechen

#### Gherkin Features

- Neue Features sollten entsprechende Gherkin-Szenarien haben
- Aktualisieren Sie bestehende Features bei Änderungen
- Verwenden Sie deutsche Sprache für Gherkin-Szenarien

### 4. Code Quality sicherstellen

```bash
# Alle Tests ausführen
dotnet test

# Code Coverage prüfen (mindestens 90%)
dotnet test --collect:"XPlat Code Coverage"

# Build ohne Warnings
dotnet build --configuration Release
```

### 5. Commit Guidelines

#### Commit Message Format

```
<typ>: <kurze beschreibung>

<optional: längere beschreibung>

<optional: footer>
```

#### Commit Typen

- `feat`: Neues Feature
- `fix`: Bugfix
- `docs`: Dokumentation
- `style`: Code-Formatierung
- `refactor`: Code-Refactoring
- `test`: Tests hinzufügen/ändern
- `chore`: Build-Prozess, Tools, etc.

#### Beispiele

```bash
feat: Add QR code generation for campaigns

- Implement QR code service with configurable size
- Add unit tests with 100% coverage
- Update campaign controller to generate codes

Closes #123
```

### 6. Pull Request erstellen

#### PR Checklist

- [ ] Alle Tests laufen durch
- [ ] Code Coverage mindestens 90%
- [ ] Keine Compiler Warnings
- [ ] Gherkin Features aktualisiert (falls nötig)
- [ ] CHANGELOG.md aktualisiert
- [ ] Dokumentation aktualisiert (falls nötig)

#### PR Template

```markdown
## Beschreibung
Kurze Beschreibung der Änderungen

## Typ der Änderung
- [ ] Bugfix
- [ ] Neues Feature
- [ ] Breaking Change
- [ ] Dokumentation

## Tests
- [ ] Neue Tests hinzugefügt
- [ ] Bestehende Tests aktualisiert
- [ ] Alle Tests laufen durch

## Checklist
- [ ] Code folgt den Coding Guidelines
- [ ] Self-Review durchgeführt
- [ ] Kommentare in schwer verständlichem Code
- [ ] Dokumentation aktualisiert
```

## 🧪 Testing

### Test Struktur

```
tests/
├── EasterEggHunt.Api.Tests/          # API Controller Tests
├── EasterEggHunt.Core.Tests/         # Business Logic Tests
├── EasterEggHunt.Infrastructure.Tests/ # Data Access Tests
└── EasterEggHunt.Web.Tests/          # MVC Tests
```

### Test Kategorien

1. **Unit Tests**: Einzelne Klassen/Methoden
2. **Integration Tests**: Datenbankzugriffe, Services
3. **End-to-End Tests**: Komplette User Journeys
4. **API Tests**: HTTP Endpoints

### Test Naming

```csharp
[Test]
public void CreateCampaign_WithValidName_ShouldReturnCampaign()
{
    // Arrange - Testdaten vorbereiten
    
    // Act - Methode ausführen
    
    // Assert - Ergebnis prüfen
}
```

## 📚 Dokumentation

### Code Dokumentation

- Alle öffentlichen Methoden benötigen XML-Dokumentation
- Kommentare auf Deutsch
- Komplexe Algorithmen erklären

```csharp
/// <summary>
/// Erstellt eine neue Kampagne mit den angegebenen Parametern
/// </summary>
/// <param name="name">Name der Kampagne</param>
/// <returns>Die erstellte Kampagne</returns>
public async Task<Campaign> CreateCampaignAsync(string name)
{
    // Implementierung...
}
```

### README Updates

- Neue Features in README.md dokumentieren
- Installation/Setup-Anweisungen aktualisieren
- API-Änderungen dokumentieren

## 🐛 Bug Reports

### Bug Report Template

```markdown
**Beschreibung**
Kurze und klare Beschreibung des Bugs

**Reproduktion**
Schritte zur Reproduktion:
1. Gehe zu '...'
2. Klicke auf '...'
3. Scrolle nach unten zu '...'
4. Siehe Fehler

**Erwartetes Verhalten**
Was sollte passieren

**Screenshots**
Falls zutreffend, Screenshots hinzufügen

**Umgebung:**
- OS: [z.B. Windows 10]
- Browser: [z.B. Chrome 91]
- Version: [z.B. 1.0.0]
```

## 💡 Feature Requests

### Feature Request Template

```markdown
**Problem**
Welches Problem löst dieses Feature?

**Lösung**
Beschreibung der gewünschten Lösung

**Alternativen**
Alternative Lösungsansätze

**Zusätzlicher Kontext**
Screenshots, Mockups, etc.
```

## 🔒 Sicherheit

### Sicherheitslücken melden

- **NICHT** öffentliche Issues für Sicherheitslücken erstellen
- Erstellen Sie ein privates GitHub Issue mit dem Label "security"
- Beschreiben Sie die Lücke detailliert
- Warten Sie auf Antwort vor Veröffentlichung

## 📞 Kontakt

- **Issues**: [GitHub Issues](../../issues)
- **Diskussionen**: [GitHub Discussions](../../discussions)
- **Entwicklerteam**: [GitHub Discussions](../../discussions)

## 📄 Lizenz

Durch Beiträge zu diesem Projekt stimmen Sie zu, dass Ihre Beiträge unter der [MIT Lizenz](LICENSE) lizenziert werden.
