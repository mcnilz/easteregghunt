#!/bin/bash

# Easter Egg Hunt - Build Script mit Docker Buildx Bake
# Verwendung: ./scripts/build.sh [dev|prod] [tag]

set -e

# Farben für Output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Funktionen
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Parameter verarbeiten
ENVIRONMENT=${1:-dev}
TAG=${2:-latest}
BUILD_DATE=$(date -u +"%Y-%m-%dT%H:%M:%SZ")
GIT_COMMIT=$(git rev-parse --short HEAD 2>/dev/null || echo "unknown")

log_info "Building Easter Egg Hunt System"
log_info "Environment: $ENVIRONMENT"
log_info "Tag: $TAG"
log_info "Build Date: $BUILD_DATE"
log_info "Git Commit: $GIT_COMMIT"

# Cache-Verzeichnisse werden nicht mehr benötigt (Docker Driver Limitation)

# Buildx Builder erstellen/verwenden
BUILDER_NAME="easteregghunt-builder"
if ! docker buildx inspect $BUILDER_NAME >/dev/null 2>&1; then
    log_info "Creating buildx builder: $BUILDER_NAME"
    docker buildx create --name $BUILDER_NAME --use --bootstrap
else
    log_info "Using existing buildx builder: $BUILDER_NAME"
    docker buildx use $BUILDER_NAME
fi

# Build-Variablen setzen
export REGISTRY="easteregghunt"
export TAG="$TAG"
export BUILD_DATE="$BUILD_DATE"
export GIT_COMMIT="$GIT_COMMIT"

# Build ausführen
case $ENVIRONMENT in
    "dev")
        log_info "Building development images..."
        docker buildx bake --file docker-bake.hcl dev
        ;;
    "prod")
        log_info "Building production images (multi-platform)..."
        docker buildx bake --file docker-bake.hcl prod
        ;;
    *)
        log_error "Unknown environment: $ENVIRONMENT"
        log_info "Usage: $0 [dev|prod] [tag]"
        exit 1
        ;;
esac

log_success "Build completed successfully!"

# Images anzeigen
log_info "Built images:"
docker images --filter "reference=easteregghunt/*:$TAG" --format "table {{.Repository}}\t{{.Tag}}\t{{.Size}}\t{{.CreatedAt}}"

# Nächste Schritte
echo ""
log_info "Next steps:"
echo "  • Start services: docker compose up"
echo "  • View logs: docker compose logs -f"
echo "  • Stop services: docker compose down"
