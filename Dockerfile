# ── Stage 1: Build ──────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first (for layer caching)
COPY AssetsMangment.slnx .
COPY AssetsMangment/AssetsMangment.csproj AssetsMangment/
COPY AssetsManagement.Tests/AssetsManagement.Tests.csproj AssetsManagement.Tests/

# Fix: remove Windows-specific NuGet fallback folders before restore
RUN dotnet nuget locals all --clear

# Restore dependencies (fresh, no Windows cache references)
RUN dotnet restore AssetsMangment/AssetsMangment.csproj \
    /p:RestoreUseStaticGraphEvaluation=false

# Copy everything else and build
COPY . .
RUN dotnet publish AssetsMangment/AssetsMangment.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Stage 2: Runtime ─────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "AssetsMangment.dll"]
