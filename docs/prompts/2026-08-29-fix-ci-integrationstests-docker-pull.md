# Fix: Docker-Images für Integrationstests in CI vorab laden

## Kontext

In PR #97 (`fix-globale-suche-live-eingabe`) schlug der CI-Schritt
„Integrationstests (Aspire-AppHost, Docker)" fehl:
`CorsPolicyTests.PreflightRequest_AusserhalbDevelopmentNurVonWhitelisteterOrigin`
warf `System.TimeoutException` beim `app.StartAsync(...).WaitAsync(5 Min.)`. Der
Branch enthält ausschließlich einen Frontend-Fix (`nav.ts`, `nav.spec.ts`) — kein
Backend- oder CORS-Code wurde geändert.

**Diagnose** (siehe `gh run view 33242869302 --log-failed`): Der Testlauf
startete um 08:20:25 Uhr; die erste (und einzige) Ausgabe erschien um 08:25:27
Uhr mit einer Eigenlaufzeit des Tests von „5 m 1 s" — die verstrichene Zeit
deckt sich fast exakt mit der Eigenlaufzeit des Tests, er war also so gut wie
sicher der erste der 20 Tests der Assembly. Alle übrigen 19 Tests liefen danach
in ca. 14 m 53 s (~47 s/Test) fehlerfrei durch. `AssemblyInfo.cs` deaktiviert
Testparallelisierung (`DisableTestParallelization = true`), Ressourcenkonkurrenz
zwischen Testklassen scheidet damit aus.

**Wahrscheinlichste Ursache** (nicht abschließend beweisbar, da GitHub Actions
keine granularen Docker-Daemon-Logs im Nachhinein liefert): Jeder Testfall
spinnt eine eigene, vollständige Aspire-Instanz hoch (`AssemblyInfo.cs` erzwingt
sequenzielle Ausführung, jede Testdatei ruft
`DistributedApplicationTestingBuilder.CreateAsync<Projects.MyMusic_AppHost>()`
separat auf). Auf einem frischen, ephemeren GitHub-Actions-Runner müssen die
Docker-Images für Postgres, Keycloak und Seq beim allerersten Container-Start
kalt aus der Registry gezogen werden — bei allen folgenden Tests sind sie
bereits lokal gecacht (passt zum Sprung von 5 m 1 s auf ~47 s/Test danach). Der
feste `_defaultTimeout` von 5 Minuten für `BuildAsync`/`StartAsync`/
`WaitForResourceAsync` ist für genau diesen kalten Erststart knapp bemessen.

Vergleichbares Muster bereits einmal im Projekt aufgetreten und behoben: Bei
Block 7j (Commit `cc44ddd`, „CI-Timeout bei Integrationstests behoben") wurde
der äußere Job-Timeout von 15 auf 20 Minuten angehoben, weil die
Gesamtlaufzeit der Testsuite strukturell knapp geworden war.

Mit dem Projektinhaber abgestimmt: Der Docker-Pull soll in einen expliziten,
vorgelagerten CI-Schritt verschoben werden (Option 1 aus der Diagnose), statt
die Timeouts in den 14 Testdateien zu erhöhen (Option 2, größerer Diff,
kaschiert die Ursache eher) oder den Lauf nur erneut anzustoßen (Option 3,
behebt nichts strukturell). Klargestellt: Das verkürzt die
Gesamtlaufzeit der Integrationstests nicht — der Pull-Aufwand fällt so oder so
einmal pro CI-Lauf an, er verlässt nur das knappe 5-Minuten-Zeitbudget des
ersten Tests und landet in einem eigenen Schritt.

## Ist-Stand (verifiziert)

- `.github/workflows/ci.yml`: Schritt „Integrationstests (Aspire-AppHost,
  Docker)" läuft direkt nach „HTTPS-Entwicklungszertifikat erzeugen", ohne
  vorherigen Docker-Pull.
- Exakte Image-Referenzen der drei Container-Ressourcen ermittelt über
  `dotnet run --publisher manifest` im `MyMusic.AppHost`-Projekt (rein lesend,
  startet keine Container):
  - Postgres: `docker.io/library/postgres:18.3`
  - Seq: `docker.io/datalust/seq:2025.2`
  - Keycloak: `quay.io/keycloak/keycloak:26.5`
- Diese drei Tags sind in den jeweiligen Aspire-Hosting-Paketen
  (`Aspire.Hosting.PostgreSQL`/`Aspire.Hosting.Seq`, Version `13.4.6`) bzw.
  direkt in `AppHost.cs` (Keycloak) festgelegt — nicht in `ci.yml` konfigurierbar.

## Geplanter Fix

Einziger neuer Schritt in `.github/workflows/ci.yml`, im Job `build-and-check`,
zwischen „HTTPS-Entwicklungszertifikat erzeugen" und „Integrationstests
(Aspire-AppHost, Docker)":

```yaml
      - name: Docker-Images fuer Integrationstests vorab laden
        run: |
          docker pull docker.io/library/postgres:18.3 &
          docker pull docker.io/datalust/seq:2025.2 &
          docker pull quay.io/keycloak/keycloak:26.5 &
          wait
```

Die drei Pulls laufen parallel im Hintergrund (`&` / `wait`), damit der Schritt
nicht länger dauert als der langsamste einzelne Pull. Keine Änderung an
Testdateien, keine Änderung an `_defaultTimeout`, keine Änderung an
`AppHost.cs`.

## Geplante Verifikation

1. YAML-Syntax lokal prüfen (Einrückung, `run: |`-Block).
2. Nach Push: GitHub-Actions-Lauf abwarten, prüfen dass der neue Schritt
   erfolgreich drei Images lädt und der anschließende Integrationstest-Schritt
   fehlerfrei durchläuft.
3. Beobachten, ob sich die Gesamtlaufzeit des Jobs wie erwartet nicht wesentlich
   ändert (Pull-Kosten verschieben sich nur, entfallen nicht).

## Bekannte Risiken und offene Punkte

- Die drei Image-Tags sind hart kodiert und an `AppHost.cs` bzw. die
  Aspire-Hosting-Paketversionen gekoppelt. Bei einem künftigen Versions-Update
  (Aspire-Pakete, Keycloak-Version) muss dieser Schritt manuell nachgezogen
  werden — sonst pullt er ein nicht mehr benötigtes altes Tag (harmlos, aber
  wirkungslos, kein Fehlerfall).
- Behebt eine wahrscheinliche, aber nicht abschließend bewiesene Ursache
  (fehlende Docker-Daemon-Logs im Nachhinein). Sollte der Timeout trotzdem
  erneut auftreten, ist das ein Hinweis auf eine andere/zusätzliche Ursache.
- Kein Einfluss auf den bereits einmal angehobenen äußeren Job-Timeout
  (20 Minuten) — dieser bleibt unverändert.
