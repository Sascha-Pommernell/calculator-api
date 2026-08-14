# Build-Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Calculator.Api/Calculator.Api/Calculator.Api.csproj Calculator.Api/
RUN dotnet restore Calculator.Api/Calculator.Api.csproj

COPY Calculator.Api/Calculator.Api/ Calculator.Api/
RUN dotnet publish Calculator.Api/Calculator.Api.csproj -c Release -o /app /p:UseAppHost=false

# Runtime-Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# curl wird nur für den HEALTHCHECK benötigt
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# Nicht als root laufen (User "app" ist im aspnet-Basisimage enthalten)
USER app

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD curl -fsS http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Calculator.Api.dll"]
