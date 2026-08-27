#!/usr/bin/env bash
set -e

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
mkdir -p "$SCRIPT_DIR/dockerdata/redis"
mkdir -p "$SCRIPT_DIR/dockerdata/mongodb"
mkdir -p "$SCRIPT_DIR/dockerdata/api"

echo "Starting Docker containers for BTG Prototyping Environment..."
docker compose up --build -d

echo "Docker containers startup command sent."
echo "Presentation API: http://localhost:8080"
echo "Swagger UI: http://localhost:8080/swagger"
echo "Health Check: http://localhost:8080/api/health"
