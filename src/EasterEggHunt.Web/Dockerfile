# Verwende das offizielle .NET 8.0 Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Verwende das .NET 8.0 SDK Image für den Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Kopiere die Solution-Datei und alle Projekt-Dateien
COPY ["EasterEggHunt.sln", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["src/EasterEggHunt.Web/EasterEggHunt.Web.csproj", "src/EasterEggHunt.Web/"]
COPY ["src/EasterEggHunt.Application/EasterEggHunt.Application.csproj", "src/EasterEggHunt.Application/"]
COPY ["src/EasterEggHunt.Domain/EasterEggHunt.Domain.csproj", "src/EasterEggHunt.Domain/"]
COPY ["src/EasterEggHunt.Infrastructure/EasterEggHunt.Infrastructure.csproj", "src/EasterEggHunt.Infrastructure/"]

# Restore der Dependencies
RUN dotnet restore "src/EasterEggHunt.Web/EasterEggHunt.Web.csproj"

# Kopiere den gesamten Source Code
COPY . .

# Build der Anwendung
WORKDIR "/src/src/EasterEggHunt.Web"
RUN dotnet build "EasterEggHunt.Web.csproj" -c Release -o /app/build

# Publish der Anwendung
FROM build AS publish
RUN dotnet publish "EasterEggHunt.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Finales Image erstellen
FROM base AS final
WORKDIR /app

# Kopiere die publizierte Anwendung
COPY --from=publish /app/publish .

# Setze den Entry Point
ENTRYPOINT ["dotnet", "EasterEggHunt.Web.dll"]

