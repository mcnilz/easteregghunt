# 🐳 Docker Build Guide - Easter Egg Hunt System

## 🚀 Docker Buildx Bake Setup

Dieses Projekt verwendet **Docker Buildx Bake** für professionelle Multi-Service Builds mit Caching und Multi-Platform Support.

## 📋 Voraussetzungen

```bash
# Docker Buildx installieren (meist bereits enthalten)
docker buildx version

# Git für Commit-Hashes (optional)
git --version
```

## 🛠️ Build-Kommandos

### Schnell-Start (Development)

```bash
# Linux/macOS
./scripts/build.sh

# Windows PowerShell
.\scripts\build.ps1

# Oder direkt mit Bake
docker buildx bake
```

### Erweiterte Builds

```bash
# Development Build mit spezifischem Tag
./scripts/build.sh dev v1.0.0

# Production Build (Multi-Platform)
./scripts/build.sh prod v1.0.0

# Nur API Service bauen
docker buildx bake api

# Nur Web Service bauen
docker buildx bake web

# Mit Custom Registry
REGISTRY=my-registry.com/easteregghunt docker buildx bake
```

## 🎯 Build-Targets

### Development (`dev`)
- **Platform**: `linux/amd64`
- **Cache**: Docker Layer Cache (automatisch)
- **Tags**: `latest`, `{custom-tag}`

### Production (`prod`)
- **Platforms**: `linux/amd64`, `linux/arm64`
- **Cache**: Optimiert für CI/CD
- **Tags**: `latest`, `prod`, `{custom-tag}`

## 📊 Build-Konfiguration

### docker-bake.hcl Features

```hcl
# Globale Variablen
variable "REGISTRY" { default = "easteregghunt" }
variable "TAG" { default = "latest" }

# Service-Gruppen
group "default" { targets = ["api", "web"] }
group "dev" { targets = ["api", "web"] }
group "prod" { targets = ["api-prod", "web-prod"] }

# Labels für Container Metadata
labels = {
  "org.opencontainers.image.title" = "Easter Egg Hunt API"
  "org.opencontainers.image.source" = "https://github.com/..."
  "org.opencontainers.image.created" = "${BUILD_DATE}"
}
```

### Build-Cache

Docker verwendet automatisch den integrierten Layer-Cache für schnellere Rebuilds. Keine manuelle Cache-Konfiguration erforderlich.

## 🔄 Development Workflow

### 1. Code ändern
```bash
# Normale Entwicklung in IDE
```

### 2. Build und Start (All-in-One)
```bash
# Baut Images und startet Services
./scripts/start.sh

# Oder mit spezifischem Tag
./scripts/start.sh dev v1.0.0
```

### 3. Nur Images bauen (optional)
```bash
# Schneller Development Build
./scripts/build.sh dev

# Oder spezifischer Service
docker buildx bake api
```

### 4. Services manuell starten
```bash
# Mit Docker Compose (verwendet automatisch compose.yaml)
docker compose up

# Oder einzeln
docker run -p 5001:8080 easteregghunt/api:latest
```

### 5. Logs anzeigen
```bash
# Alle Services
docker compose logs -f

# Nur API
docker compose logs -f easteregghunt-api
```

## 🚀 CI/CD Integration

### GitHub Actions Beispiel

```yaml
- name: Build Images
  run: |
    export BUILD_DATE=$(date -u +"%Y-%m-%dT%H:%M:%SZ")
    export GIT_COMMIT=${{ github.sha }}
    docker buildx bake --file docker-bake.hcl prod

- name: Push to Registry
  run: |
    docker buildx bake --file docker-bake.hcl prod --push
```

### Azure DevOps Beispiel

```yaml
- script: |
    export BUILD_DATE=$(date -u +"%Y-%m-%dT%H:%M:%SZ")
    export GIT_COMMIT=$(Build.SourceVersion)
    docker buildx bake --file docker-bake.hcl prod
  displayName: 'Build Images'
```

## 🐛 Troubleshooting

### Builder Probleme
```bash
# Builder neu erstellen
docker buildx rm easteregghunt-builder
./scripts/build.sh

# Verfügbare Builder anzeigen
docker buildx ls
```

### Cache Probleme
```bash
# Build ohne Cache
docker buildx bake --no-cache

# Docker System Cache aufräumen
docker system prune -f
```

### Platform Probleme
```bash
# Nur für aktuelle Platform bauen
docker buildx bake --platform linux/amd64

# QEMU für ARM64 installieren
docker run --privileged --rm tonistiigi/binfmt --install all
```

## 📈 Performance Tipps

### Build-Optimierung
- ✅ **Layer Caching**: Dockerfile-Reihenfolge optimieren
- ✅ **Multi-Stage**: Unnötige Dependencies in Build-Stage
- ✅ **BuildKit**: Parallele Builds nutzen
- ✅ **Cache Mount**: Für NuGet/npm Packages

### Dockerfile Best Practices
```dockerfile
# Dependencies zuerst (besseres Caching)
COPY ["*.csproj", "./"]
RUN dotnet restore

# Source Code zuletzt
COPY . .
RUN dotnet build
```

## 🔧 Anpassungen

### Registry ändern
```bash
# In docker-bake.hcl
variable "REGISTRY" {
  default = "your-registry.com/easteregghunt"
}

# Oder als Umgebungsvariable
export REGISTRY=your-registry.com/easteregghunt
```

### Zusätzliche Platforms
```hcl
# In docker-bake.hcl
platforms = ["linux/amd64", "linux/arm64", "linux/arm/v7"]
```

### Custom Labels
```hcl
labels = {
  "com.company.team" = "backend"
  "com.company.environment" = "production"
}
```

---

**Docker Buildx Bake macht Multi-Service Builds einfach, schnell und reproduzierbar!** 🎯
