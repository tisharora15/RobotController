# ── Stage 1: Build ────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project file and restore dependencies (cached layer)
COPY robot-controller-api.csproj ./
COPY tests/robot_controller_api.Tests.csproj ./tests/
RUN dotnet restore robot-controller-api.csproj

# Copy everything and build
COPY . ./
RUN dotnet publish robot-controller-api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Stage 2: Test ──────────────────────────────────────────────────────────
FROM build AS test
WORKDIR /app
RUN dotnet restore tests/robot_controller_api.Tests.csproj
RUN dotnet test tests/robot_controller_api.Tests.csproj \
    --no-restore \
    --logger "trx;LogFileName=test-results.xml" \
    --results-directory /app/test-results

# ── Stage 3: Runtime ───────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Security: run as non-root user
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
USER appuser

COPY --from=build /app/publish .

# Expose HTTP port
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "robot-controller-api.dll"]