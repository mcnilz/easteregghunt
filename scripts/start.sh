#!/bin/bash

# Easter Egg Hunt - Start Script
# Baut Images mit Buildx Bake und startet Services mit Docker Compose

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

log_info "Starting Easter Egg Hunt System"
log_info "Environment: $ENVIRONMENT"
log_info "Tag: $TAG"

# Immer neu bauen für Development
log_info "Building fresh images with Buildx Bake..."
./scripts/build.sh $ENVIRONMENT $TAG
if [ $? -ne 0 ]; then
    log_error "Build failed!"
    exit 1
fi
log_success "Images built successfully"

# Services starten
log_info "Starting services with Docker Compose..."
if [ "$TAG" != "latest" ]; then
    # Custom Tag verwenden
    export TAG=$TAG
    docker compose up -d
else
    # Standard latest Tag
    docker compose up -d
fi

# Status prüfen
sleep 3
log_info "Checking service status..."

if docker compose ps --format table | grep -q "Up"; then
    log_success "Services started successfully!"
    echo ""
    log_info "Available services:"
    echo "  • API: http://localhost:5001"
    echo "  • Web: http://localhost:5000"
    echo "  • SQLite Browser (optional): docker compose --profile tools up sqlite-browser"
    echo ""
    log_info "Useful commands:"
    echo "  • View logs: docker compose logs -f"
    echo "  • Stop services: docker compose down"
    echo "  • Restart: docker compose restart"
else
    log_error "Some services failed to start!"
    docker compose ps
    exit 1
fi

