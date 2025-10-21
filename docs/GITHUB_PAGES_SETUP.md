# 🚀 GitHub Pages Setup für CI Reports

## Übersicht

Dieses Setup erstellt automatisch eine GitHub Pages Website mit:
- **📊 Code Coverage Reports** (HTML + Cobertura)
- **🧪 Test Results** (TRX + Coverage XML)
- **📚 Projekt-Dokumentation** (Markdown → HTML)
- **📈 Build Status** und Metriken

## Setup-Schritte

### 1. GitHub Pages aktivieren

1. Gehe zu deinem Repository auf GitHub
2. Klicke auf **Settings** → **Pages**
3. Unter **Source** wähle **GitHub Actions**
4. Speichere die Einstellung

### 2. Workflow-Dateien sind bereits erstellt

Die folgenden Dateien wurden erstellt:
- `.github/workflows/pages.yml` - Haupt-Workflow für GitHub Pages
- `scripts/convert-markdown.sh` - Markdown zu HTML Konverter
- `docs/pages-index.md` - Index-Seite für die Website

### 3. Workflow-Trigger

Der Pages-Workflow wird automatisch ausgelöst:
- **Bei Push** auf `main` oder `master` Branch
- **Nach erfolgreichem CI-Run** (Continuous Integration)

### 4. Website-Struktur

```
https://mcnilz.github.io/easteregghunt/
├── index.html                    # Hauptseite
├── coverage/
│   ├── index.html               # Coverage Report
│   └── Summary.md               # Coverage Summary
├── architecture.html            # Architektur-Dokumentation
├── developer-guide.html         # Entwickler-Anleitung
├── troubleshooting.html         # Troubleshooting
├── sprint-planning.html         # Sprint-Planung
└── ...                         # Weitere Dokumentation
```

## Features

### 📊 Code Coverage
- **HTML Report** mit interaktiven Diagrammen
- **Cobertura XML** für CI/CD Integration
- **Markdown Summary** für GitHub PR Comments

### 📚 Dokumentation
- **Automatische Konvertierung** von Markdown zu HTML
- **Responsive Design** für Mobile und Desktop
- **Navigation** zwischen allen Seiten
- **Syntax Highlighting** für Code-Blöcke

### 🔄 Automatisierung
- **Automatische Updates** bei jedem erfolgreichen Build
- **Build-Metadaten** (Build-Nummer, Commit, Datum)
- **Fehlerbehandlung** bei fehlgeschlagenen Builds

## Konfiguration

### Workflow anpassen

Du kannst den Workflow in `.github/workflows/pages.yml` anpassen:

```yaml
# Trigger ändern
on:
  push:
    branches: [ main, develop ]  # Weitere Branches hinzufügen

# Coverage-Filter anpassen
assemblyfilters: '-EasterEggHunt.Api.Tests*;-EasterEggHunt.Application.Tests*'
classfilters: '-EasterEggHunt.Infrastructure.Migrations*'
```

### Styling anpassen

Das CSS kann in `scripts/convert-markdown.sh` angepasst werden:

```bash
# Farben ändern
echo "        h1 { border-bottom: 2px solid #3498db; }" >> "$output_file"
echo "        a { color: #3498db; }" >> "$output_file"
```

## Troubleshooting

### Workflow schlägt fehl

1. **Prüfe GitHub Pages Einstellungen:**
   - Settings → Pages → Source = "GitHub Actions"

2. **Prüfe Workflow-Berechtigungen:**
   - Settings → Actions → General → Workflow permissions
   - "Read and write permissions" aktivieren

3. **Prüfe Branch-Schutz:**
   - Settings → Branches → Branch protection rules
   - "Restrict pushes that create files" deaktivieren

### Website wird nicht aktualisiert

1. **Prüfe Workflow-Status:**
   - Actions → "Deploy CI Reports to GitHub Pages"
   - Schau nach fehlgeschlagenen Runs

2. **Prüfe CI-Workflow:**
   - Actions → "Continuous Integration"
   - Pages-Workflow läuft nur nach erfolgreichem CI

### Coverage Reports fehlen

1. **Prüfe Test-Output:**
   ```bash
   dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
   ```

2. **Prüfe ReportGenerator:**
   - Coverage-Dateien müssen in `coverage/**/coverage.cobertura.xml` existieren

## Erweiterte Features

### Custom Domain

1. Erstelle `CNAME` Datei im Repository:
   ```
   your-domain.com
   ```

2. Konfiguriere DNS:
   ```
   CNAME your-domain.com mcnilz.github.io
   ```

### Analytics

Füge Google Analytics hinzu in `scripts/convert-markdown.sh`:

```bash
echo "    <script async src='https://www.googletagmanager.com/gtag/js?id=GA_TRACKING_ID'></script>" >> "$output_file"
echo "    <script>window.dataLayer=window.dataLayer||[];function gtag(){dataLayer.push(arguments);}gtag('js',new Date());gtag('config','GA_TRACKING_ID');</script>" >> "$output_file"
```

### SEO

Füge Meta-Tags hinzu:

```bash
echo "    <meta name='description' content='Easter Egg Hunt - CI Reports and Documentation'>" >> "$output_file"
echo "    <meta name='keywords' content='easter egg hunt, ci, coverage, documentation'>" >> "$output_file"
```

## Nützliche Links

- [GitHub Pages Dokumentation](https://docs.github.com/en/pages)
- [ReportGenerator](https://github.com/danielpalme/ReportGenerator)
- [GitHub Actions](https://docs.github.com/en/actions)

---

**Hinweis:** Diese Website wird automatisch bei jedem erfolgreichen CI-Run aktualisiert. Die URL ist: `https://mcnilz.github.io/easteregghunt/`
