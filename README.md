# Calculator API

Eine REST-API für Grundrechenarten (Addition, Subtraktion, Multiplikation, Division) mit zwei oder mehr Zahlen, umgesetzt mit **ASP.NET Core** (.NET 10).

## Voraussetzungen

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Optional: Docker (für den Container-Betrieb)

## Projektstruktur

```
calculator-api/
├── .github/workflows/api-tests.yml  # CI: Unit-Tests, Docker-Build, API-Tests, Allure-Report
├── Calculator.Api/
│   ├── Calculator.Api.slnx          # Solution
│   ├── Calculator.Api/              # ASP.NET-Core-Projekt
│   │   ├── Controllers/             # CalculatorController (4 Endpunkte)
│   │   ├── Infrastructure/          # CalculationExceptionHandler (zentrale Fehlerbehandlung)
│   │   ├── Models/                  # CalculationRequest / CalculationResponse / OperationNames
│   │   └── Services/                # ICalculatorService / CalculatorService
│   └── Calculator.Api.Tests/        # Unit-Tests (xUnit) für den CalculatorService
├── docs/
│   └── Testkonzept.md               # Testkonzept für die API-Tests
└── Dockerfile
```

## API starten

```powershell
dotnet run --project Calculator.Api/Calculator.Api
```

Die API läuft standardmäßig unter `http://localhost:5116` (Profil `http`, siehe `Properties/launchSettings.json`).

Im Development-Modus stehen zusätzlich bereit:

- OpenAPI-Dokument: `http://localhost:5116/openapi/v1.json`
- Swagger UI: `http://localhost:5116/swagger`

## Endpunkte

Alle Rechen-Endpunkte erwarten einen `POST`-Request mit `Content-Type: application/json`.

| Endpunkt | Operation |
|---|---|
| `POST /api/calculator/add` | Addiert alle Zahlen |
| `POST /api/calculator/subtract` | Subtrahiert alle weiteren Zahlen von der ersten |
| `POST /api/calculator/multiply` | Multipliziert alle Zahlen |
| `POST /api/calculator/divide` | Dividiert die erste Zahl nacheinander durch alle weiteren |
| `GET /health` | Health-Endpoint (für Probes/Monitoring, liefert `200 Healthy`) |

Zum Schutz vor Überlastung ist ein Rate Limit von 200 Requests pro Sekunde und Client-IP aktiv (darüber: `429 Too Many Requests`).

### Request

```json
{
  "numbers": [10, 5, 2]
}
```

Es müssen **mindestens zwei** und dürfen **höchstens 1000 Zahlen** angegeben werden.

### Response (200 OK)

```json
{
  "operation": "Division",
  "numbers": [10, 5, 2],
  "result": 1
}
```

### Fehlerfälle (400 Bad Request)

Fehler werden als **ProblemDetails** (RFC 9457) zurückgegeben – auch unerwartete Fehler (500) und Statuscodes ohne Body (404/405/415) über die zentrale Fehlerbehandlung. Es gibt zwei Ausprägungen:

**Eingabevalidierung** (z. B. weniger als zwei Zahlen, fehlendes `numbers`-Feld) – `ValidationProblemDetails` mit `errors`-Objekt:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Numbers": ["Es müssen mindestens zwei Zahlen angegeben werden."]
  }
}
```

**Fachliche Fehler** (Division durch null, Überlauf) – `ProblemDetails` mit `title`/`detail`:

```json
{
  "title": "Ungültige Berechnung",
  "status": 400,
  "detail": "Division durch null ist nicht erlaubt."
}
```

### Beispiel-Request (PowerShell)

```powershell
Invoke-RestMethod -Uri "http://localhost:5116/api/calculator/add" `
  -Method Post -ContentType "application/json" `
  -Body '{"numbers": [1, 2, 3]}'
```

## Unit-Tests

Die Rechenlogik des `CalculatorService` wird durch Unit-Tests (xUnit) abgesichert:

```powershell
dotnet test Calculator.Api/Calculator.Api.slnx
```

## Docker

Das Image läuft aus Sicherheitsgründen als non-root-User (`app`) und bringt einen `HEALTHCHECK` auf `/health` mit.

```powershell
docker build -t calculator-api .
docker run --rm -p 5116:8080 calculator-api
```

Die API ist anschließend unter `http://localhost:5116` erreichbar (Container-intern Port 8080).

## Tests

Die API-Tests befinden sich im separaten Projekt **calculator-api-tests-c** (NUnit + Playwright + Allure). Das zugehörige Testkonzept ist in [docs/Testkonzept.md](docs/Testkonzept.md) dokumentiert.

### Continuous Integration (GitHub Actions)

Die Tests werden automatisch über den GitHub-Actions-Workflow [`api-tests.yml`](.github/workflows/api-tests.yml) ausgeführt – bei jedem Push auf `main` sowie manuell per *workflow_dispatch*. Der Workflow:

1. baut API und Tests jeweils als Docker-Image,
2. startet die API in einem Docker-Netzwerk,
3. führt die Tests im Container gegen die laufende API aus,
4. generiert einen **Allure-Report** (inkl. Historie der letzten Läufe) und veröffentlicht ihn auf **GitHub Pages**.

### Testergebnisse (GitHub Pages)

Der aktuelle Allure-Testbericht ist hier abrufbar:

➡️ **<https://sascha-pommernell.github.io/calculator-api/>**

Der Link zum Bericht des jeweiligen Laufs wird zusätzlich in der Workflow-Zusammenfassung (*Summary*) des Actions-Laufs verlinkt; die Rohdaten stehen dort außerdem als Artefakt `testergebnisse` zum Download bereit.
