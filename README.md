# Calculator API

Eine REST-API für Grundrechenarten (Addition, Subtraktion, Multiplikation, Division) mit zwei oder mehr Zahlen, umgesetzt mit **ASP.NET Core** (.NET 10).

## Voraussetzungen

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Optional: Docker (für den Container-Betrieb)

## Projektstruktur

```
calculator-api/
├── Calculator.Api/
│   ├── Calculator.Api.slnx          # Solution
│   └── Calculator.Api/              # ASP.NET-Core-Projekt
│       ├── Controllers/             # CalculatorController (4 Endpunkte)
│       ├── Models/                  # CalculationRequest / CalculationResponse
│       └── Services/                # ICalculatorService / CalculatorService
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

Alle Endpunkte erwarten einen `POST`-Request mit `Content-Type: application/json`.

| Endpunkt | Operation |
|---|---|
| `POST /api/calculator/add` | Addiert alle Zahlen |
| `POST /api/calculator/subtract` | Subtrahiert alle weiteren Zahlen von der ersten |
| `POST /api/calculator/multiply` | Multipliziert alle Zahlen |
| `POST /api/calculator/divide` | Dividiert die erste Zahl nacheinander durch alle weiteren |

### Request

```json
{
  "numbers": [10, 5, 2]
}
```

Es müssen **mindestens zwei Zahlen** angegeben werden.

### Response (200 OK)

```json
{
  "operation": "Division",
  "numbers": [10, 5, 2],
  "result": 1
}
```

### Fehlerfälle (400 Bad Request)

Fehler werden als **ProblemDetails** (RFC 9457) zurückgegeben, z. B. bei:

- weniger als zwei Zahlen oder fehlendem `numbers`-Feld (Validierungsfehler)
- Division durch null
- Überlauf (Ergebnis außerhalb des Wertebereichs)

Beispiel:

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

## Docker

```powershell
docker build -t calculator-api .
docker run --rm -p 5116:8080 calculator-api
```

Die API ist anschließend unter `http://localhost:5116` erreichbar (Container-intern Port 8080).

## Tests

Die API-Tests befinden sich im separaten Projekt **calculator-api-tests-c** (NUnit + Playwright + Allure). Das zugehörige Testkonzept ist in [docs/Testkonzept.md](docs/Testkonzept.md) dokumentiert.
