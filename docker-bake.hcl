# Docker Buildx Bake Configuration für Easter Egg Hunt System

# Globale Variablen
variable "REGISTRY" {
  default = "easteregghunt"
}

variable "TAG" {
  default = "latest"
}

variable "BUILD_DATE" {
  default = ""
}

variable "GIT_COMMIT" {
  default = ""
}

# Basis-Gruppe für alle Services
group "default" {
  targets = ["api", "web"]
}

# Development Gruppe
group "dev" {
  targets = ["api", "web"]
}

# Production Gruppe  
group "prod" {
  targets = ["api-prod", "web-prod"]
}

# API Service
target "api" {
  context = "."
  dockerfile = "src/EasterEggHunt.Api/Dockerfile"
  tags = [
    "${REGISTRY}/api:${TAG}",
    "${REGISTRY}/api:latest"
  ]
  labels = {
    "org.opencontainers.image.title" = "Easter Egg Hunt API"
    "org.opencontainers.image.description" = "REST API Backend für Easter Egg Hunt System"
    "org.opencontainers.image.source" = "https://github.com/your-org/easter-egg-hunt"
    "org.opencontainers.image.created" = "${BUILD_DATE}"
    "org.opencontainers.image.revision" = "${GIT_COMMIT}"
    "org.opencontainers.image.version" = "${TAG}"
  }
  platforms = ["linux/amd64"]
}

# Web Service
target "web" {
  context = "."
  dockerfile = "src/EasterEggHunt.Web/Dockerfile"
  tags = [
    "${REGISTRY}/web:${TAG}",
    "${REGISTRY}/web:latest"
  ]
  labels = {
    "org.opencontainers.image.title" = "Easter Egg Hunt Web"
    "org.opencontainers.image.description" = "MVC Frontend für Easter Egg Hunt System"
    "org.opencontainers.image.source" = "https://github.com/your-org/easter-egg-hunt"
    "org.opencontainers.image.created" = "${BUILD_DATE}"
    "org.opencontainers.image.revision" = "${GIT_COMMIT}"
    "org.opencontainers.image.version" = "${TAG}"
  }
  platforms = ["linux/amd64"]
}

# Production API (Multi-Platform)
target "api-prod" {
  inherits = ["api"]
  platforms = ["linux/amd64", "linux/arm64"]
  tags = [
    "${REGISTRY}/api:${TAG}",
    "${REGISTRY}/api:latest",
    "${REGISTRY}/api:prod"
  ]
}

# Production Web (Multi-Platform)
target "web-prod" {
  inherits = ["web"]
  platforms = ["linux/amd64", "linux/arm64"]
  tags = [
    "${REGISTRY}/web:${TAG}",
    "${REGISTRY}/web:latest",
    "${REGISTRY}/web:prod"
  ]
}
