# Create persistent data directories under dockerdata
$scriptPath = $PSScriptRoot
New-Item -ItemType Directory -Force -Path "$scriptPath/dockerdata/redis" | Out-Null
New-Item -ItemType Directory -Force -Path "$scriptPath/dockerdata/mongodb" | Out-Null
New-Item -ItemType Directory -Force -Path "$scriptPath/dockerdata/api" | Out-Null

Write-Host "Starting Docker containers for BTG Prototyping Environment..." -ForegroundColor Green
docker compose up --build -d

Write-Host "Docker containers startup command sent." -ForegroundColor Green
Write-Host "Presentation API will be available at: http://localhost:8080" -ForegroundColor Cyan
Write-Host "Swagger UI will be available at: http://localhost:8080/swagger" -ForegroundColor Cyan
Write-Host "Health Check available at: http://localhost:8080/api/health" -ForegroundColor Cyan
