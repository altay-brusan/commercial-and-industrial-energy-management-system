# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and build config files
COPY Derms-Backend.sln .
COPY Directory.Build.props .
COPY Directory.Packages.props .

# Copy project files for restore
COPY src/Domain/Domain.csproj src/Domain/
COPY src/Application/Application.csproj src/Application/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/Proto/Proto.csproj src/Proto/
COPY src/Web/Web.csproj src/Web/

# Restore dependencies (only Web project to skip test projects)
RUN dotnet restore src/Web/Web.csproj

# Copy all source code
COPY src/ src/

# Build
RUN dotnet build src/Web/Web.csproj -c Release --no-restore

# Publish stage
FROM build AS publish
RUN dotnet publish src/Web/Web.csproj -c Release -o /app/publish --no-build

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Non-root user
RUN adduser --disabled-password --gecos "" --uid 1000 appuser

# Copy published output
COPY --from=publish /app/publish .

# Install curl for health checks
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

# Switch to non-root user
USER appuser

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Backend.Web.dll"]
