# Testkonzept – Calculator API

## 1. Zielsetzung

Dieses Dokument beschreibt das Konzept für die automatisierte Testung der **Calculator API** (ASP.NET Core, .NET 10). Ziel ist die Absicherung der fachlichen Korrektheit, der Eingabevalidierung und des API-Vertrags durch automatisierte End-to-End-API-Tests mit **NUnit** und **Microsoft Playwright für .NET**, ergänzt um **Unit-Tests (xUnit)** für die Rechenlogik, ausgeführt in einer CI-Pipeline über **GitHub Actions**.

## 2. Testgegenstand

Die Calculator API stellt vier REST-Endpunkte bereit:

| Endpunkt | Methode | Operation |
|---|---|---|
| `/api/calculator/add` | POST | Addition |
| `/api/calculator/subtract` | POST | Subtraktion |
| `/api/calculator/multiply` | POST | Multiplikation |
| `/api/calculator/divide` | POST | Division |

**Request-Body:** `{ "numbers": [zahl1, zahl2, ...] }` – mindestens zwei, höchstens 1000 Zahlen.

**Response (200):** `{ "operation": string, "numbers": number[], "result": number }`

**Fehlerfälle (400):** ProblemDetails bei Division durch null, weniger als zwei Zahlen, fehlendem/ungültigem Body.

Zusätzlich stellt die API einen Health-Endpoint `GET /health` bereit, der von Tests und CI als Erreichbarkeits-Probe genutzt wird.

## 3. Teststrategie

### 3.1 Testebenen und Werkzeuge

- **API-Tests (Black-Box)** gegen die laufende Anwendung – kein Browser nötig, Playwright wird mit dem `APIRequestContext` verwendet.
  - **Technologie:** C#/.NET 10, **NUnit 4** + **Microsoft.Playwright**, Reporting über **Allure** und **TRX**.
  - **Projekt:** eigenständiges Repository `calculator-api-tests-c` (Projekt `CalculatorApiTests`).
  - **Ausführung:** parallelisiert auf Fixture-Ebene (NUnit `Parallelizable`); jede Fixture nutzt eine eigene Playwright-Instanz.
- **Unit-Tests (White-Box)** für die Rechenlogik des `CalculatorService` – **xUnit**-Projekt `Calculator.Api.Tests` im API-Repository (schnellere, feinere Ebene der Testpyramide; läuft in CI vor den API-Tests).
- **Teststart der API:** Die API wird nicht aus dem Test-Repository gestartet, sondern extern betrieben (lokal via `dotnet run` oder Docker; in CI als Docker-Container mit **Produktionskonfiguration**). Die Basis-URL ist über die Umgebungsvariable `API_BASE_URL` konfigurierbar (Standard: `http://localhost:5116`). Vor dem Testlauf wird die Erreichbarkeit über `GET /health` geprüft; lokal werden die Tests bei nicht erreichbarer API als *Inconclusive* markiert, in CI (`CI=true`) schlagen sie hart fehl, damit die Pipeline nicht „grün ohne Tests“ wird.

### 3.2 Testarten

| Testart | Abdeckung |
|---|---|
| Unit-Tests (xUnit) | Rechenlogik des `CalculatorService` (inkl. Ausnahmefälle) |
| Funktionale Tests (Happy Path) | Korrekte Ergebnisse aller vier Operationen |
| Negativtests | Division durch null, ungültige/unvollständige Eingaben |
| Vertragstests | Response-Struktur, Statuscodes, Content-Type |
| Robustheitstests | Ungültiges JSON, falsche HTTP-Methode, unbekannte Routen |

### 3.3 Testentwurfsverfahren

Die Testfälle in Kapitel 4 wurden mit folgenden Black-Box-Testentwurfsverfahren (gemäß ISTQB) abgeleitet:

| Verfahren | Anwendung |
|---|---|
| Äquivalenzklassenbildung | Gültige Eingaben (≥ 2 Zahlen, positive/negative/dezimale Werte) vs. ungültige Eingaben (< 2 Zahlen, > 1000 Zahlen, null, falsche Typen, ungültiges JSON) |
| Grenzwertanalyse | Leeres Array / 1 Zahl / 2 Zahlen; Obergrenze 1000/1001 Zahlen; Divisor 0; Zahlbereichsgrenzen (`1e308`, `1e-308`) |
| Zustandsunabhängige Vertragsprüfung | Response-Struktur, Statuscodes, HTTP-Methoden, Routen |
| Fehlererwartungsmethode (Error Guessing) | Gleitkomma-Präzision (0.1 + 0.2), Überlauf/`Infinity`-Serialisierung, Zusatzfelder im Body |

### 3.4 Risikobasierte Priorisierung

Jeder Testfallgruppe ist eine Priorität zugeordnet, die die Ausführungs- und Behebungsreihenfolge bestimmt:

| Priorität | Testfallgruppen | Begründung |
|---|---|---|
| Hoch | 4.1 Happy Path, 4.3 Division durch null, 4.4 Eingabevalidierung | Kernfunktionalität und Fehlerbehandlung; Fehler hier betreffen alle Nutzer direkt |
| Mittel | 4.2 Gleitkomma-Randfälle, 4.5 API-Vertrag | Randbedingungen und Vertragsstabilität; geringere Eintrittswahrscheinlichkeit |

Die Prioritäten sind im Testcode als NUnit-Kategorien (`[Category("Prio-Hoch")]` / `[Category("Prio-Mittel")]`) sowie als Allure-Severity (`critical` / `normal`) hinterlegt. Ein priorisierter Lauf ist damit gezielt möglich, z. B.:

```powershell
dotnet test --filter "TestCategory=Prio-Hoch"
```

### 3.5 Nicht im Scope

- Last-/Performancetests
- Security-Tests (AuthN/AuthZ ist nicht implementiert)
- UI-Tests (keine UI vorhanden)

## 4. Testfälle

### 4.1 Happy Path – Grundrechenarten (erwartet: 200 OK)

| ID | Endpunkt | Eingabe | Erwartetes Ergebnis |
|---|---|---|---|
| TC-ADD-01 | add | [1, 2] | 3 |
| TC-ADD-02 | add | [1, 2, 3, 4] | 10 (mehr als zwei Operanden) |
| TC-ADD-03 | add | [-5, 2.5] | -2.5 (negative & dezimale Zahlen) |
| TC-SUB-01 | subtract | [10, 4] | 6 |
| TC-SUB-02 | subtract | [10, 4, 3] | 3 (links-assoziativ) |
| TC-SUB-03 | subtract | [-1, -1] | 0 |
| TC-MUL-01 | multiply | [3, 4] | 12 |
| TC-MUL-02 | multiply | [2, 3, 4] | 24 |
| TC-MUL-03 | multiply | [5, 0] | 0 |
| TC-MUL-04 | multiply | [-2, 2.5] | -5 |
| TC-DIV-01 | divide | [10, 4] | 2.5 |
| TC-DIV-02 | divide | [100, 5, 2] | 10 (verkettete Division) |
| TC-DIV-03 | divide | [-9, 3] | -3 |
| TC-DIV-04 | divide | [0, 5] | 0 (Dividend null ist erlaubt) |

### 4.2 Gleitkomma-Randfälle

| ID | Endpunkt | Eingabe | Erwartung |
|---|---|---|---|
| TC-FLT-01 | add | [0.1, 0.2] | 200, Ergebnis ≈ 0.3 (Toleranzvergleich) |
| TC-FLT-02 | add | [1e308, 1e308] | 400 Bad Request – arithmetischer Überlauf (`Infinity`) wird als ungültige Berechnung abgelehnt |
| TC-FLT-03 | divide | [1e308, 1e-308] | 400 Bad Request – Überlauf bei Division, analog TC-FLT-02 |
| TC-FLT-04 | divide | [1, 3] | 200, Ergebnis ≈ 0.3333… (periodisches Ergebnis, Toleranzvergleich) |
| TC-FLT-05 | multiply | [1e308, 1e308] | 400 Bad Request – Überlauf bei Multiplikation |
| TC-FLT-06 | subtract | [-1e308, 1e308] | 400 Bad Request – Überlauf ins negative `Infinity` bei Subtraktion |

> **Hinweis:** Die API prüft das Ergebnis auf `double.IsFinite`; arithmetische Überläufe (`Infinity`/`NaN`) werden mit 400 Bad Request (ProblemDetails, Titel „Ungültige Berechnung“) abgelehnt.

### 4.3 Division durch null (erwartet: 400 Bad Request)

| ID | Eingabe | Erwartung |
|---|---|---|
| TC-DIV0-01 | [10, 0] | 400, ProblemDetails mit Titel „Ungültige Berechnung“, Detail enthält „Division durch null“ |
| TC-DIV0-02 | [10, 2, 0] | 400 (null an beliebiger späterer Position) |

### 4.4 Eingabevalidierung (erwartet: 400 Bad Request, alle 4 Endpunkte)

| ID | Eingabe | Erwartung |
|---|---|---|
| TC-VAL-01 | `{ "numbers": [42] }` | 400 – nur eine Zahl |
| TC-VAL-02 | `{ "numbers": [] }` | 400 – leeres Array |
| TC-VAL-03 | `{ }` | 400 – Feld `numbers` fehlt |
| TC-VAL-04 | leerer Body | 400 |
| TC-VAL-05 | `{ "numbers": [1, "abc"] }` | 400 – ungültiger Typ |
| TC-VAL-06 | syntaktisch ungültiges JSON | 400 |
| TC-VAL-07 | `{ "numbers": null }` | 400 – explizit null (eigener Pfad gegenüber fehlendem Feld) |
| TC-VAL-08 | gültiger Body mit `Content-Type: text/plain` | 415 Unsupported Media Type |
| TC-VAL-09 | `{ "numbers": [1, 1, …] }` mit 1001 Zahlen | 400 – Obergrenze (max. 1000) überschritten |

Zusätzlich wird bei den Validierungsfällen (TC-VAL-01 bis TC-VAL-07 sowie TC-VAL-09) die Struktur der Fehlerantwort geprüft: ProblemDetails gemäß RFC 9457 mit `title`, `status` und nicht-leerem `errors`-Objekt (Modelvalidierung).

### 4.5 API-Vertrag / Robustheit

| ID | Prüfung | Erwartung |
|---|---|---|
| TC-CON-01 | Response-Felder | Genau `operation`, `numbers`, `result` |
| TC-CON-02 | Header | `Content-Type: application/json` |
| TC-CON-03 | GET auf POST-Endpunkt | 405 Method Not Allowed |
| TC-CON-04 | Unbekannte Operation (z. B. `/modulo`) | 404 Not Found |
| TC-CON-05 | Unbekannte Zusatzfelder im Body (z. B. `{ "numbers": [1, 2], "extra": true }`) | 200 – Zusatzfelder werden toleriert (Robustheitsprinzip) |

## 5. Testumgebung

| Aspekt | Lokal | CI (GitHub Actions) |
|---|---|---|
| Betriebssystem | Windows (Entwicklung) | ubuntu-latest |
| .NET SDK | .NET 10 SDK | 10.0.x (`actions/setup-dotnet`) bzw. Docker-Images |
| API-Start | extern (`dotnet run` oder Docker-Container) | Docker-Container (**Produktionskonfiguration**, Erreichbarkeit via `GET /health`) |
| Testlauf | `dotnet test` gegen laufende API | Docker-Container im gemeinsamen Netzwerk mit der API |
| Basis-URL | `http://localhost:5116` (Standard) | `http://calculator-api:8080` via `API_BASE_URL` |

## 6. CI/CD-Integration (GitHub Actions)

Beide Repositories besitzen eine eigene Pipeline:

**API-Repository (`calculator-api`)** – Workflow `.github/workflows/api-tests.yml`:

- **Trigger:** Push und Pull Request auf `main`, zusätzlich manuell (`workflow_dispatch`, dabei ist der Git-Ref des Test-Repos wählbar).
- **Ablauf:**
  1. Checkout des API-Repos, .NET 10 SDK einrichten
  2. **Unit-Tests** (`dotnet test Calculator.Api/Calculator.Api.slnx`) als schnelles Quality Gate
  3. Checkout des Test-Repos (`calculator-api-tests-c`)
  4. Docker-Images für API und Tests bauen
  5. API-Container mit **Produktionskonfiguration** starten, Warten auf `GET /health`
  6. Test-Container im gemeinsamen Docker-Netzwerk ausführen (`API_BASE_URL`, `CI=true`)
  7. TRX- und Allure-Ergebnisse als Workflow-Artefakt hochladen (14 Tage Aufbewahrung) und TRX als Check veröffentlichen (`dorny/test-reporter`)
  8. Nur bei Push auf `main`: Allure-Report (inkl. Historie der letzten 20 Läufe) generieren und auf **GitHub Pages** veröffentlichen
- **Fehlerverhalten:** Fehlgeschlagene Unit- oder API-Tests brechen die Pipeline ab (PR-Gate über den `pull_request`-Trigger). Es gibt keinen automatischen Retry; Flakiness wird durch die Erreichbarkeits-Probe (`/health`) und deterministische, zustandslose Tests vermieden. Ist die API im Testlauf nicht erreichbar, schlagen die Tests wegen `CI=true` hart fehl (kein „grün ohne Tests“). Der Report wird auch bei fehlgeschlagenen Tests veröffentlicht.
- **Berechtigungen:** `contents: write` (gh-pages-Publish) und `checks: write` (Test-Check).
- **Voraussetzung:** GitHub Pages mit Quelle „gh-pages-Branch“.

**Test-Repository (`calculator-api-tests-c`)** – Workflow `.github/workflows/tests-ci.yml`:

- **Trigger:** Push und Pull Request auf `main`, zusätzlich manuell (dabei ist der Git-Ref des API-Repos wählbar).
- **Ablauf:** identisch zu Schritt 3–7 oben (API-Repo wird ausgecheckt, beide Container gebaut, Tests ausgeführt, Ergebnisse als Artefakt/Check veröffentlicht) – ohne Pages-Deployment. Damit werden auch Änderungen am Testcode selbst durch ein PR-Gate absichert.

## 7. Berichterstattung

- **Lokal:** NUnit-Ausgabe in der Konsole; optional TRX (`dotnet test --logger trx`); Allure-Report via `allure serve` über die `allure-results` im Build-Ausgabeverzeichnis.
- **CI:** Testergebnisse im Actions-Log, als TRX-Check am Commit/PR (`dorny/test-reporter`) und als Workflow-Artefakt (TRX + Allure-Rohdaten, 14 Tage); der **Allure-Report** wird über **GitHub Pages** veröffentlicht.
- **GitHub Pages:**
  - Reports der Läufe auf `main` sind versioniert (pro Run-Nummer) erreichbar unter:
    `https://sascha-pommernell.github.io/calculator-api/<run-nummer>/` (Startseite leitet auf den aktuellen Lauf weiter; Historie der letzten 20 Läufe bleibt erhalten).
  - Für Pull Requests wird kein Pages-Deployment durchgeführt; Ergebnisse stehen als Workflow-Artefakt (14 Tage) und TRX-Check bereit.

## 8. Testorganisation

### 8.1 Rollen und Verantwortlichkeiten

| Rolle | Verantwortlich | Aufgaben |
|---|---|---|
| Testmanager / Testanalyst / Testautomatisierer / Entwickler | Sascha Pommernell (Einzelentwickler) | Testkonzept, Testfallentwurf, Automatisierung, Pflege, Auswertung, Fehlerbehebung |

### 8.2 Eingangskriterien (Entry Criteria)

- Eine laufende, erreichbare Instanz der Calculator API steht unter der konfigurierten `API_BASE_URL` bereit (Erreichbarkeit via `GET /health`).
- Die Testumgebung ist verfügbar (lokal: .NET 10 SDK; CI: Runner mit Docker).
- Das Testprojekt ist baubar (`dotnet build` erfolgreich).

### 8.3 Endekriterien (Exit Criteria / Abnahme)

- Alle Testfälle aus Abschnitt 4 sind automatisiert umgesetzt.
- Alle Tests laufen lokal und in GitHub Actions erfolgreich („grün“); bekannte, als Defect erfasste Abweichungen (siehe Kap. 10) sind dokumentiert.
- Ein PR kann nur gemerged werden, wenn die Test-Pipeline erfolgreich ist.

### 8.4 Abbruch- und Wiederaufnahmekriterien (Suspension Criteria)

- **Abbruch:** Die Testdurchführung wird ausgesetzt, wenn die API unter der konfigurierten `API_BASE_URL` nicht erreichbar ist (Prüfung via `GET /health`; lokal: *Inconclusive*, CI: harter Fehlschlag) oder mehr als 50 % der Tests aufgrund eines Umgebungsproblems fehlschlagen.
- **Wiederaufnahme:** Nach Behebung des blockierenden Problems und erfolgreichem Smoke-Check (ein Happy-Path-Test pro Endpunkt) wird die vollständige Testsuite erneut ausgeführt.

## 9. Rückverfolgbarkeit (Traceability)

- Jeder automatisierte Test trägt die Testfall-ID aus Kapitel 4 im Testnamen bzw. in der Testbeschreibung (z. B. `TC-ADD-01 add 1+2=3` via `SetArgDisplayNames` oder `TC-FLT-01: …` via `Description`).
- Dadurch ist die Zuordnung Testkonzept ↔ Testcode ↔ Testreport (Allure/TRX) lückenlos möglich.
- Bei Änderungen am Testkonzept werden betroffene Testfall-IDs im Commit/PR referenziert.

## 10. Fehler- und Abweichungsmanagement (Defect Management)

- Gefundene Abweichungen werden als **GitHub Issues** im Repository erfasst.
- Jedes Issue enthält: betroffene Testfall-ID(s), Ist- und Soll-Verhalten, Reproduktionsschritte (Request-Beispiel) und Link zum fehlgeschlagenen Workflow-Lauf.
- Bekannte offene Defects: derzeit keine. (Das Überlaufverhalten TC-FLT-02/03 wurde durch eine `double.IsFinite`-Prüfung im `CalculatorService` behoben; Überläufe liefern jetzt 400 Bad Request.)

## 11. Wartung und Erweiterung

- Neue Endpunkte oder Operationen erfordern entsprechende neue Testfälle (Happy Path + Negativtests) vor dem Merge.
- Bei Änderungen am Response-Format sind die Vertragstests (4.5) anzupassen.
- Unit-Tests für den `CalculatorService` sind als xUnit-Projekt `Calculator.Api.Tests` im API-Repository umgesetzt und laufen in der CI-Pipeline vor den API-Tests.
