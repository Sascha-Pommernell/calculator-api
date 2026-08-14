# Testkonzept – Calculator API

## 1. Zielsetzung

Dieses Dokument beschreibt das Konzept für die automatisierte Testung der **Calculator API** (ASP.NET Core, .NET 10). Ziel ist die Absicherung der fachlichen Korrektheit, der Eingabevalidierung und des API-Vertrags durch automatisierte End-to-End-API-Tests mit **Playwright**, ausgeführt in einer CI-Pipeline über **GitHub Actions**.

## 2. Testgegenstand

Die Calculator API stellt vier REST-Endpunkte bereit:

| Endpunkt | Methode | Operation |
|---|---|---|
| `/api/calculator/add` | POST | Addition |
| `/api/calculator/subtract` | POST | Subtraktion |
| `/api/calculator/multiply` | POST | Multiplikation |
| `/api/calculator/divide` | POST | Division |

**Request-Body:** `{ "numbers": [zahl1, zahl2, ...] }` – mindestens zwei Zahlen erforderlich.

**Response (200):** `{ "operation": string, "numbers": number[], "result": number }`

**Fehlerfälle (400):** ProblemDetails bei Division durch null, weniger als zwei Zahlen, fehlendem/ungültigem Body.

## 3. Teststrategie

### 3.1 Testebene und Werkzeug

- **Testebene:** API-Tests (Black-Box) gegen die laufende Anwendung – kein Browser nötig, Playwright wird mit dem `APIRequestContext` (`request`-Fixture) verwendet.
- **Technologie:** Playwright Test (TypeScript), eigenständiges npm-Projekt unter `tests/api`.
- **Teststart der API:** Die API befindet sich nicht in diesem Repository und wird extern betrieben bzw. gestartet. Die Tests laufen gegen eine bereits laufende API-Instanz; die Basis-URL ist über die Umgebungsvariable `API_BASE_URL` konfigurierbar (Standard: `http://localhost:5116`).

### 3.2 Testarten

| Testart | Abdeckung |
|---|---|
| Funktionale Tests (Happy Path) | Korrekte Ergebnisse aller vier Operationen |
| Negativtests | Division durch null, ungültige/unvollständige Eingaben |
| Vertragstests | Response-Struktur, Statuscodes, Content-Type |
| Robustheitstests | Ungültiges JSON, falsche HTTP-Methode, unbekannte Routen |

### 3.3 Testentwurfsverfahren

Die Testfälle in Kapitel 4 wurden mit folgenden Black-Box-Testentwurfsverfahren (gemäß ISTQB) abgeleitet:

| Verfahren | Anwendung |
|---|---|
| Äquivalenzklassenbildung | Gültige Eingaben (≥ 2 Zahlen, positive/negative/dezimale Werte) vs. ungültige Eingaben (< 2 Zahlen, null, falsche Typen, ungültiges JSON) |
| Grenzwertanalyse | Leeres Array / 1 Zahl / 2 Zahlen; Divisor 0; Zahlbereichsgrenzen (`1e308`, `1e-308`) |
| Zustandsunabhängige Vertragsprüfung | Response-Struktur, Statuscodes, HTTP-Methoden, Routen |
| Fehlererwartungsmethode (Error Guessing) | Gleitkomma-Präzision (0.1 + 0.2), Überlauf/`Infinity`-Serialisierung, Zusatzfelder im Body |

### 3.4 Risikobasierte Priorisierung

Jeder Testfallgruppe ist eine Priorität zugeordnet, die die Ausführungs- und Behebungsreihenfolge bestimmt:

| Priorität | Testfallgruppen | Begründung |
|---|---|---|
| Hoch | 4.1 Happy Path, 4.3 Division durch null, 4.4 Eingabevalidierung | Kernfunktionalität und Fehlerbehandlung; Fehler hier betreffen alle Nutzer direkt |
| Mittel | 4.2 Gleitkomma-Randfälle, 4.5 API-Vertrag | Randbedingungen und Vertragsstabilität; geringere Eintrittswahrscheinlichkeit |

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

Zusätzlich wird bei den Validierungsfällen (TC-VAL-01 bis TC-VAL-07) die Struktur der Fehlerantwort geprüft: ProblemDetails gemäß RFC 9457 mit `title` und (bei Modelvalidierung) `errors`-Objekt.

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
| .NET SDK | nicht erforderlich (API extern) | nicht erforderlich (API extern) |
| Node.js | ≥ 20 | 22 (`actions/setup-node`) |
| API-Start | extern (laufende API-Instanz erforderlich) | extern (URL via Repository-Variable `API_BASE_URL`) |
| Basis-URL | `http://localhost:5116` (Standard) | konfigurierbar via `API_BASE_URL` |

## 6. CI/CD-Integration (GitHub Actions)

- **Trigger:** Push und Pull Request auf `main`, zusätzlich manuell (`workflow_dispatch`).
- **Workflow-Datei:** `.github/workflows/api-tests.yml`
- **Ablauf:**
  1. Checkout des Repositories
  2. Setup Node.js 22 (mit npm-Cache)
  3. `npm ci` im Testprojekt (`tests/api`)
  4. `npx playwright test` gegen die extern erreichbare API (`API_BASE_URL` als Repository-Variable)
  5. Upload des HTML-Testreports als Pages-Artifact (Läufe auf `main`) bzw. als Workflow-Artifact (Pull Requests)
  6. Veröffentlichung des Reports auf **GitHub Pages** (nur bei Läufen auf `main`)
- **Fehlerverhalten:** Fehlgeschlagene Tests brechen die Pipeline ab (PR-Gate). In CI werden Tests bei Fehlschlag bis zu 2× wiederholt (Flakiness-Abfederung). Der Report wird auch bei fehlgeschlagenen Tests veröffentlicht, damit Fehleranalysen direkt im Browser möglich sind.
- **Berechtigungen:** Der Workflow benötigt `pages: write` und `id-token: write`; als Deployment-Mechanismus werden `actions/upload-pages-artifact` und `actions/deploy-pages` verwendet.
- **Voraussetzung:** In den Repository-Einstellungen muss GitHub Pages mit der Quelle „GitHub Actions“ aktiviert sein.

## 7. Berichterstattung

- **Lokal:** Playwright-List-Reporter in der Konsole; HTML-Report via `npx playwright show-report`.
- **CI:** Testergebnisse direkt im Actions-Log sichtbar; der HTML-Report wird über **GitHub Pages** veröffentlicht.
- **GitHub Pages:**
  - Der Playwright-HTML-Report des jeweils letzten Laufs auf `main` ist dauerhaft erreichbar unter:
    `https://sascha-pommernell.github.io/calculator-api/`
  - Jeder neue Lauf auf `main` überschreibt den vorherigen Report (es wird immer der aktuelle Stand angezeigt).
  - Für Pull Requests wird der Report nicht auf Pages deployt, sondern nur als Workflow-Artifact bereitgestellt (14 Tage Aufbewahrung).

## 8. Testorganisation

### 8.1 Rollen und Verantwortlichkeiten

| Rolle | Verantwortlich | Aufgaben |
|---|---|---|
| Testmanager / Testanalyst / Testautomatisierer / Entwickler | Sascha Pommernell (Einzelentwickler) | Testkonzept, Testfallentwurf, Automatisierung, Pflege, Auswertung, Fehlerbehebung |

### 8.2 Eingangskriterien (Entry Criteria)

- Eine laufende, erreichbare Instanz der Calculator API steht unter der konfigurierten `API_BASE_URL` bereit.
- Die Testumgebung ist verfügbar (lokal: Node.js ≥ 20; CI: Runner mit Setup-Actions).
- Das Testprojekt ist installierbar (`npm ci` erfolgreich).

### 8.3 Endekriterien (Exit Criteria / Abnahme)

- Alle Testfälle aus Abschnitt 4 sind automatisiert umgesetzt.
- Alle Tests laufen lokal und in GitHub Actions erfolgreich („grün“); bekannte, als Defect erfasste Abweichungen (siehe Kap. 10) sind dokumentiert.
- Ein PR kann nur gemerged werden, wenn die Test-Pipeline erfolgreich ist.

### 8.4 Abbruch- und Wiederaufnahmekriterien (Suspension Criteria)

- **Abbruch:** Die Testdurchführung wird ausgesetzt, wenn die API unter der konfigurierten `API_BASE_URL` nicht erreichbar ist oder mehr als 50 % der Tests aufgrund eines Umgebungsproblems fehlschlagen.
- **Wiederaufnahme:** Nach Behebung des blockierenden Problems und erfolgreichem Smoke-Check (ein Happy-Path-Test pro Endpunkt) wird die vollständige Testsuite erneut ausgeführt.

## 9. Rückverfolgbarkeit (Traceability)

- Jeder automatisierte Test trägt die Testfall-ID aus Kapitel 4 im Testnamen (z. B. `TC-ADD-01: add(1, 2) = 3`).
- Dadurch ist die Zuordnung Testkonzept ↔ Testcode ↔ Testreport (Playwright-HTML-Report) lückenlos möglich.
- Bei Änderungen am Testkonzept werden betroffene Testfall-IDs im Commit/PR referenziert.

## 10. Fehler- und Abweichungsmanagement (Defect Management)

- Gefundene Abweichungen werden als **GitHub Issues** im Repository erfasst.
- Jedes Issue enthält: betroffene Testfall-ID(s), Ist- und Soll-Verhalten, Reproduktionsschritte (Request-Beispiel) und Link zum fehlgeschlagenen Workflow-Lauf.
- Bekannte offene Defects: derzeit keine. (Das Überlaufverhalten TC-FLT-02/03 wurde durch eine `double.IsFinite`-Prüfung im `CalculatorService` behoben; Überläufe liefern jetzt 400 Bad Request.)

## 11. Wartung und Erweiterung

- Neue Endpunkte oder Operationen erfordern entsprechende neue Testfälle (Happy Path + Negativtests) vor dem Merge.
- Bei Änderungen am Response-Format sind die Vertragstests (4.5) anzupassen.
- Optional zukünftig: Unit-Tests für `CalculatorService` (xUnit) als schnellere, feinere Testebene unterhalb der API-Tests.
