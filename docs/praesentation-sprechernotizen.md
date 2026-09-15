# Sprechernotizen – Präsentation „Calculator API“

Foliensatz: [`praesentation.html`](praesentation.html) · 25 Folien

**Bedienung:** `←` / `→` blättern · `O` Übersicht · `F` Vollbild · `D` Dunkelmodus ·
`Strg+P` → „Als PDF speichern“ (Querformat, Ränder „keine“, Hintergrundgrafiken aktivieren)

**Zeitbudget bei 20 Minuten Vortrag:** Teil 1 (Folien 1–12) ca. 9 Min · Teil 2
(Folien 13–21) ca. 8 Min · Abschluss (22–25) ca. 3 Min. Bei 10 Minuten: Folien
5, 8, 15, 19, 24 überspringen – der rote Faden bleibt erhalten.

---

## 1 · Titel

Kurz halten. Ein Satz zur Rahmung:

> „Ich zeige heute eine REST-API für Grundrechenarten. Das Spannende daran ist
> nicht das Rechnen – sondern alles drumherum: wie ich sicherstelle, dass sie
> korrekt ist, und wie das automatisiert bei jedem Push überprüft wird.“

## 2 · Agenda

Nur überfliegen. Sagen, dass die Präsentation zwei Teile hat: **die API** und
**wie sie abgesichert wird**.

## 3 · Motivation

**Wichtigste Folie für die Erwartungssteuerung.** Hier vorwegnehmen, was das
Publikum sonst denkt („ein Taschenrechner, ernsthaft?“):

> „Die Fachlichkeit ist absichtlich trivial. Wenn ich hier ein komplexes
> Domänenmodell hätte, würde die halbe Zeit für Erklärungen draufgehen. So kann
> ich stattdessen zeigen, wie ein Projekt aufgebaut ist, das man auch in zwei
> Jahren noch anfassen kann.“

## 4 · Zwei Repositories

Kernbotschaft: **Trennung erzwingt Black-Box-Disziplin.** Wären die Tests im
gleichen Projekt, könnte man versehentlich interne Klassen aufrufen und würde
dann nicht mehr die API testen, sondern den Code.

Wenn gefragt wird, ob das nicht unnötig kompliziert ist: Ja, für dieses Projekt
ist es etwas Overhead – aber genau so trennen echte Projekte Test- und
Produktivcode, wenn ein separates QA-Team existiert.

## 5 · Tech-Stack

Nicht vorlesen. Nur zwei Punkte hervorheben:

- **Playwright ohne Browser** – überrascht viele. Playwright kann auch reines
  HTTP (`APIRequestContext`). Man bekommt Parallelisierung und Tracing gratis.
- **Allure** – der Grund, warum am Ende ein Bericht existiert, den auch
  Nichtentwickler lesen können.

## 6 · Schichten

Am Baum entlanggehen. Die Pointe steht in der Karte unten rechts: weil die
Fachlogik nicht am HTTP-Stack hängt, laufen 33 Tests in 46 Millisekunden. Das
ist der praktische Nutzen der Schichtentrennung – nicht die Theorie.

## 7 · Endpunkte

Die interessante Stelle ist die **Entwurfsentscheidung** unten links: vier
Endpunkte statt `/calculate?op=add`. Begründung: der Vertrag wird
selbstdokumentierend, jeder Endpunkt hat eigene OpenAPI-Beschreibung, und es
gibt keinen String, den man falsch schreiben kann.

Zweite Pointe: Es sind **Listen**, keine zwei Operanden – `[1,2,3,4]` geht.

## 8 · Request / Response

Nur kurz. Erklären, warum die Antwort die Eingabe zurückspiegelt: bei
asynchronen oder parallelen Aufrufen kann der Client Anfrage und Ergebnis
zuordnen, ohne selbst mitzuzählen.

## 9 · Controller

Hier langsamer werden – erste Code-Folie.

Zwei Dinge betonen:
1. **Eine Hilfsmethode für alle vier Operationen**, die Operation kommt als
   Delegate herein. Kein Copy-Paste ×4.
2. **Kein `try/catch`.** Bewusst. Das führt zur nächsten Folie über zentrale
   Fehlerbehandlung.

## 10 · Service

Die inhaltlich stärkste Code-Folie. Zwei Fallen im Umgang mit `double`:

> „In C# wirft die Division `10.0 / 0` **keine** Exception – sie liefert
> `Infinity`. Wer sich auf `DivideByZeroException` verlässt, hat einen Bug. Also
> prüfe ich den Divisor selbst, **vor** der Rechnung.“

> „Und selbst wenn kein Divisor null ist, kann das Ergebnis aus dem darstellbaren
> Bereich laufen. `EnsureFinite` fängt das ab – sonst würde die API `Infinity`
> ausliefern, was in JSON überhaupt kein gültiger Wert ist.“

Hier ankündigen: „Diese Prüfung ist nicht aus Weitsicht entstanden, sondern weil
ein Test fehlgeschlagen ist – dazu komme ich auf Folie 22.“

## 11 · ProblemDetails

Kernbotschaft: **einheitliches Fehlerformat**, standardisiert nach RFC 9457.

Der Unterschied zwischen den beiden Beispielen ist wichtig:
- links: **Validierung** greift, bevor der Code läuft → `errors`-Objekt pro Feld
- rechts: **fachlicher Fehler** im Service → `title` + `detail`

Abschließender Satz: „Der Client braucht keine Textanalyse. Und weil die
Struktur Teil des Vertrags ist, wird sie getestet – nicht nur dokumentiert.“

## 12 · Härtung

Sechs Karten, jede in einem Satz. Nicht alle gleich lang behandeln – Schwerpunkt
auf:
- **Rate Limiting**: die API würde ohne Limit von einer Schleife lahmgelegt.
- **non-root im Container**: eine Zeile `USER app`, aber deutlich kleinere
  Angriffsfläche, wenn der Prozess kompromittiert wird.
- **Swagger nur in Development**: in Produktion soll die API ihre eigene
  Landkarte nicht ausliefern.

## 13 · Abschnittstrenner „Qualitätssicherung“

Atempause. Ein Satz:

> „Damit ist die API beschrieben. Jetzt der eigentliche Kern: wie ich beweise,
> dass sie funktioniert.“

## 14 · Zwei Testebenen

Die Testpyramide erklären: viele schnelle Tests unten, wenige teure oben.

Wichtigster Punkt – **„kein grün ohne Tests“**:

> „Ein früher Zustand meiner Pipeline war grün, obwohl gar keine Tests gelaufen
> waren: die API war nicht erreichbar, und die Tests haben sich selbst
> übersprungen. Das ist schlimmer als ein roter Build, weil man ihm glaubt. Jetzt
> erzwingt `CI=true` einen harten Fehlschlag.“

Zweiter Punkt: In der CI wird gegen den **Docker-Container in
Produktionskonfiguration** getestet, nicht gegen `dotnet run`. Getestet wird das
Artefakt, das ausgeliefert würde.

## 15 · Testentwurfsverfahren

Hier zeigt sich Methodik statt Bauchgefühl. Ein Beispiel pro Verfahren, nicht
mehr:

- **Äquivalenzklassen**: alle Eingaben mit ≥ 2 Zahlen verhalten sich gleich –
  also braucht man nicht 50 davon.
- **Grenzwertanalyse**: die Fehler sitzen an den Rändern. Deshalb 1 / 2 Zahlen
  und 1000 / 1001 Zahlen.
- **Error Guessing**: `0.1 + 0.2` ist in IEEE-754 nicht exakt `0.3`. Klassische
  Stolperfalle, also explizit ein Testfall.

Priorisierung: nicht jeder Test ist gleich wichtig. Die Prios stehen als
NUnit-Kategorie **im Code**, sind also gezielt ausführbar – nicht nur ein Vermerk
im Dokument.

## 16 · Testumfang (Zahlen)

Kurz auf die Verteilung zeigen. Die Aussage steht in der Karte unten:

> „Weniger als die Hälfte der Testfälle prüft den Erfolgsfall. Der größere Teil
> prüft Grenzen, Fehler und Vertragsbruch – und genau dort entstehen in der
> Praxis die Probleme.“

Bei Nachfrage: Die 9 Validierungsfälle laufen gegen **alle vier Endpunkte**, die
Zahl ausgeführter Tests ist also deutlich höher als 36.

## 17 · Unit-Tests

Code zeigen, drei Punkte:
1. **Deutsche, sprechende Testnamen** – der Bericht liest sich wie eine
   Spezifikation.
2. **`[Theory]` + `[InlineData]`** – ein neuer Testfall ist eine neue Zeile.
3. **`[MemberData]`** prüft eine Regel gegen alle vier Operationen gleichzeitig.
   Verhindert, dass Validierung bei einer Operation vergessen wird.

Und die Toleranz bei `double`: `Assert.Equal(0.3, …, 1e-10)`. Nie exakt
vergleichen.

## 18 · API-Tests

Playwright als HTTP-Client erklären. Dann der Punkt, der methodisch am meisten
wert ist – **Traceability**:

> „Jeder Test trägt die Testfall-ID aus dem Konzept im Namen. Ich kann also von
> einem roten Test im Bericht rückwärts zum Testkonzept gehen – und umgekehrt für
> jeden spezifizierten Fall nachweisen, dass er automatisiert ist.“

`TC-CON-05` als Beispiel erwähnen: unbekannte Zusatzfelder werden **toleriert**.
Das ist eine dokumentierte Entscheidung, kein Zufall – und deshalb ein Testfall.

## 19 · Docker

Multi-Stage in einem Satz: gebaut mit dem SDK-Image, ausgeliefert auf dem
schlanken Runtime-Image.

Die Layer-Reihenfolge erklären, wenn Zeit ist: erst `csproj` kopieren, dann
`restore`, dann der Rest. Solange sich Abhängigkeiten nicht ändern, kommt der
Restore aus dem Cache – spart bei jedem Build Zeit.

## 20 · Pipeline

Die sechs Schritte durchgehen. Zwei Details hervorheben, die Erfahrung zeigen:

- **`continue-on-error` beim Testschritt**: absichtlich. Sonst gibt es bei roten
  Tests keinen Report – und genau dann braucht man ihn. Am Ende schlägt der
  Workflow explizit fehl.
- **`docker logs` bei Fehlern**: die API-Logs landen automatisch im Lauf. Man
  muss nicht raten, warum ein Test rot war.

## 21 · Reporting

> „Ein Testlauf, dessen Ergebnis niemand ansieht, ist verschwendete Rechenzeit.“

Drei Ausgabekanäle: Allure-Report auf GitHub Pages (visuell, mit Historie über
20 Läufe), TRX-Check direkt am Commit, Rohdaten als Artefakt.

**Wenn Internet verfügbar ist:** hier den Report live öffnen. Das ist der
stärkste Moment der Präsentation.

## 22 · Herausforderungen

Ehrlich sein – diese Folie macht die Präsentation glaubwürdig. Die erste Zeile
ist die wichtigste:

> „Zwei Testfälle zum Überlauf sind fehlgeschlagen. Das war kein Testfehler,
> sondern ein echter Defect: die API hat `Infinity` ausgeliefert. Der Test hat
> also nicht nur geprüft, sondern das Design verbessert – genau dafür schreibt
> man ihn.“

Die letzte Zeile als Abschluss:

> „Und die unangenehmste Erkenntnis: meine Pipeline war einmal grün, ohne dass
> Tests gelaufen sind. Ein grüner Haken muss etwas bedeuten, sonst ist er
> schädlich.“

## 23 · Fazit

Sechs Punkte, nicht einzeln vorlesen. Die Kernaussage aus der Karte sprechen:

> „Das Ergebnis dieses Projekts ist nicht die Rechenlogik. Es ist der belegbare,
> wiederholbare Weg von der Anforderung zum getesteten, ausgelieferten
> Artefakt.“

## 24 · Ausblick

Zeigt, dass die Grenzen des Projekts bekannt sind. Besonders erwähnen:
**Rate Limiting ist implementiert, aber nicht unter Last verifiziert** – eine
bewusst offene Lücke, nicht ein Versehen.

## 25 · Abschluss

Danken, zu Fragen einladen. Swagger UI und Allure-Report bereithalten.

---

## Vorbereitete Antworten auf wahrscheinliche Fragen

**„Warum `double` und nicht `decimal`?“**
`double` passt zum JSON-Zahlentyp und zu wissenschaftlichem Rechnen. `decimal`
wäre für Geldbeträge richtig – exakte Dezimalstellen, kein Binärbruch-Fehler.
Für einen Rechner ohne Währungsbezug ist `double` die passende Wahl, und die
Konsequenzen (Präzisionstoleranz, Überlaufprüfung) sind explizit behandelt und
getestet.

**„Warum wirft der Service Exceptions statt Result-Objekte zurückzugeben?“**
Weil Exceptions nicht versehentlich ignoriert werden können. Der zentrale
`IExceptionHandler` übersetzt sie an einer Stelle in HTTP-Antworten, dadurch
bleiben Controller und Service frei von Fehlerbehandlungscode.

**„Ist Playwright für API-Tests nicht überdimensioniert?“**
Es bringt `APIRequestContext`, Parallelisierung und Tracing mit, und es ist
dieselbe Bibliothek, die man für UI-Tests einsetzen würde. Käme eine UI hinzu,
wäre kein Werkzeugwechsel nötig.

**„Wie hoch ist die Code-Coverage?“**
Wird derzeit nicht gemessen – steht im Ausblick. Die Fachlogik im
`CalculatorService` ist durch 33 Unit-Tests inklusive aller Ausnahmepfade
abgedeckt; belastbar wäre aber nur eine gemessene Zahl mit Schwellwert in der
Pipeline.

**„Warum 200 Requests pro Sekunde?“**
Ein pragmatischer Wert: hoch genug, dass die parallelen Testläufe nicht dagegen
laufen, niedrig genug, um eine einfache Schleife zu bremsen. Ohne Lasttest wäre
jede exaktere Zahl geraten – deshalb steht der Lasttest im Ausblick.

**„Wie lange läuft die Pipeline?“**
Dominiert von den zwei Docker-Builds; die Unit-Tests selbst sind in
Millisekunden fertig. Sie laufen deshalb als erstes: ein Fachlogikfehler bricht
den Lauf ab, bevor überhaupt ein Image gebaut wird.
