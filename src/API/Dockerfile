FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and project files first for efficient caching
COPY btg_challenge.slnx ./
COPY src/entities/entities.csproj src/entities/
COPY src/repositories/repositories.csproj src/repositories/
COPY src/services/services.csproj src/services/
COPY src/crosscutting/crosscutting.csproj src/crosscutting/
COPY src/API/API.csproj src/API/

# Restore dependencies
RUN dotnet restore src/API/API.csproj

# Copy full source code
COPY src/ src/

# Build and publish release binaries
WORKDIR /app/src/API
RUN dotnet publish API.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "API.dll"]
