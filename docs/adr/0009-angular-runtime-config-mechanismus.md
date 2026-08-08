# ADR 0009 — Mechanismus für die Angular-Runtime-Config

**Status**: Angenommen
**Datum**: 2026-08-08
**Betrifft**: `src/frontend`, `src/MyMusic.AppHost`

## Kontext

Laut Wiki (`architektur/aspire-orchestrierung.md`, Glossar-Eintrag „Service Discovery")
kann Aspire die API-Basis-URL nicht direkt in den Angular-Build injizieren — anders als
bei .NET-Projekten, wo Connection Strings und Endpunkt-URLs zur Laufzeit als
Umgebungsvariablen in den Prozess injiziert werden. Angular kompiliert zu statischen
Browser-Assets; Umgebungsvariablen des Node-Prozesses (Dev-Server oder Build) landen
nicht automatisch im ausgelieferten Bundle. Die Wiki-Entscheidung lautet deshalb:
Angular lädt die API-Basis-URL zur Laufzeit aus einer `runtime-config.json`.

Offen war, **wie** diese Datei mit dem tatsächlichen, von Aspire dynamisch vergebenen
API-Endpunkt befüllt wird — dafür kamen mehrere Ansätze in Frage:

1. Ein Build-/Start-Vorlauf-Skript (Node), das vor `ng serve`/`ng build` eine
   Umgebungsvariable liest und `public/runtime-config.json` schreibt.
2. Eine kleine Middleware im Angular-Dev-Server, die `/runtime-config.json` zur
   Laufzeit dynamisch aus `process.env` generiert, statt eine Datei zu schreiben.
3. Ein eigener, vom .NET-Backend ausgelieferter Endpoint (z. B. `GET /runtime-config.json`
   auf der API), den Angular zur Laufzeit abfragt.

## Entscheidung

Ansatz 1: Ein Node-Skript (`src/frontend/scripts/write-runtime-config.mjs`) liest die
Umgebungsvariable `MYMUSIC_API_BASE_URL` und schreibt
`src/frontend/public/runtime-config.json` neu. Es läuft über npms `pre`-Skript-Konvention
automatisch vor `npm start` (`prestart`) und vor `npm run build` (`prebuild`) — ohne
zusätzliche manuelle Schritte oder eigene Tooling-Abhängigkeit.

Der AppHost (`src/MyMusic.AppHost/AppHost.cs`) setzt `MYMUSIC_API_BASE_URL` beim
Start der `frontend`-Ressource über `.WithEnvironment("MYMUSIC_API_BASE_URL",
api.GetEndpoint("https"))`.

Eine statische Platzhalterdatei `public/runtime-config.json` (`{ "apiBaseUrl": "" }`)
bleibt eingecheckt, damit `ng build`/`ng serve` auch ohne vorherigen Skriptlauf ein
gültiges, wenn auch leeres, Runtime-Config-Dokument findet.

## Begründung

- **Gegenüber Ansatz 2 (Dev-Server-Middleware)**: Angulars `@angular/build:dev-server`
  bietet keinen dokumentierten, stabilen Erweiterungspunkt für eigene Request-Handler
  in dieser Version — eine eigene Middleware wäre an interne, nicht versionierte APIs
  gekoppelt. Ein eigenständiges Skript ist stabil gegenüber Angular-CLI-Updates und
  funktioniert identisch für `ng serve` und `ng build` (Production-Artefakt enthält die
  zum Build-Zeitpunkt gültige Datei als statisches Asset).
- **Gegenüber Ansatz 3 (Endpoint auf der API)**: Würde eine zusätzliche,
  unauthentifizierte Route auf `MyMusic.Api` erfordern, nur um Konfiguration
  auszuliefern, die rein infrastrukturell ist (Aspire kennt die URL bereits beim
  Start der `frontend`-Ressource). Zusätzliche Netzwerklatenz und eine weitere
  Fehlerquelle (API muss erreichbar sein, bevor die Konfiguration überhaupt geladen
  werden kann) ohne erkennbaren Vorteil gegenüber Ansatz 1.
- Das gewählte Muster (`.WithEnvironment(...)` auf der AppHost-Ressource, ausgelesen
  von einem `pre`-npm-Skript) folgt demselben Grundprinzip, das im Projekt bereits für
  andere Dienste verwendet wird (z. B. `Keycloak__Authority` als Umgebungsvariable für
  die API) — kein neues Konzept, nur auf den JS-Sonderfall übertragen.

## Konsequenzen

- Nach jedem lokalen `npm start`/`npm run build` zeigt `git status` die neu
  geschriebene `public/runtime-config.json` als geändert an, wenn `apiBaseUrl`
  vom eingecheckten Platzhalter abweicht — das ist erwartetes, nicht zu committendes
  Arbeitsverzeichnis-Rauschen, kein Fehler.
- Ein direkter `ng serve`-Aufruf außerhalb des AppHosts (ohne `npm start`) liest weiter
  die zuletzt geschriebene bzw. die eingecheckte Platzhalter-Datei — die API-Basis-URL
  ist dann leer, bis erneut über `npm start`/`npm run build` mit gesetzter
  `MYMUSIC_API_BASE_URL` gebaut wurde.
- Der Mechanismus ist rein lesend für das Frontend (`fetch()` einer statischen Datei)
  und erfordert keine Änderung an der Backend-API.

## Nachtrag (2026-08-08, Block 7a — Angular-Login-Flow)

Die Ladereihenfolge wurde geändert: `RuntimeConfigService` lud die Datei bisher selbst
per `provideAppInitializer()` **nach** `bootstrapApplication()`. Mit der Einführung von
`provideKeycloak()` (siehe ADR 0010) reicht das nicht mehr, da dessen Konfigurations-
objekt (u. a. die Keycloak-URL) bereits beim Aufbau des Provider-Arrays vorliegen muss —
also bevor `bootstrapApplication()` überhaupt aufgerufen wird. `src/frontend/src/main.ts`
lädt `runtime-config.json` deshalb jetzt selbst *vor* `bootstrapApplication()` und
übergibt das Ergebnis an eine Factory (`buildAppConfig(runtimeConfig)` in
`app.config.ts`), die daraus das komplette `ApplicationConfig`-Array baut.
`RuntimeConfigService` selbst wurde dadurch einfacher: Er hält nur noch die bereits
geladene Konfiguration (per `InjectionToken` `RUNTIME_CONFIG` injiziert), kein `load()`
mehr. Die Grundentscheidung dieses ADRs (Werte zur Laufzeit statt zur Build-Zeit aus
einer `runtime-config.json`) bleibt unverändert gültig — nur der Lademechanismus wandert
vor den Angular-Bootstrap.
