# Projektanweisungen für Claude Code

## 1. Projektziel

Dieses Repository enthält die Multi-User-Webanwendung **MyMusic** zur Verwaltung
privater Musiksammlungen mit Schwerpunkt Schallplatten (Vinyl) und CDs.

Solution, Projekte, Namespaces und Assemblies verwenden das Präfix `MyMusic`.

Die Anwendung besteht aus folgenden fachlichen Teilen:

1. CRUD für Records, Tracks, Artists, Labels und Genres.
2. Dashboard mit Sammlungsstatistiken.
3. Volltext-Suche über Records, Artists und Labels.
4. Metadaten-Import über die Discogs-API (serverseitig proxied).
5. Zustandsbewertung physischer Tonträger nach dem Goldmine-Standard.
6. Multi-User-Betrieb mit Keycloak-Authentifizierung und strikter Mandantentrennung.

Sekundäres Ziel: Das Repository dient als Referenz und Demonstration der
Fachkenntnisse im Bewerbungskontext (Portfolioarbeit).

**Verbindliche fachliche Quelle** ist das Planungs-Wiki unter
`../../02 Wiki/MyMusic Wiki/wiki/` (Einstieg: `index.md`). Bei Konflikten zwischen
Wiki und anderen Dokumenten gilt das Wiki; die Abweichung wird trotzdem gemeldet.
Das Arbeitsmodell ist in `../../03 Ressourcen/leitfaden-zusammenarbeit-mit-claude-code.md`
beschrieben.

## 2. Verbindlicher Arbeitsmodus

### 2.1 Analyse vor Änderung

- Untersuche zunächst den vorhandenen Stand.
- Trenne Fakten, Annahmen, offene Fragen und Empfehlungen.
- Erstelle vor jeder größeren Änderung einen nachvollziehbaren Plan (Plan Mode).
- Verändere keine Dateien, solange der Benutzer nicht ausdrücklich die Umsetzung
  freigegeben hat.
- Eine Freigabe gilt ausschließlich für den konkret beschriebenen Änderungsschritt.
- Frühere Freigaben dürfen nicht auf spätere Änderungen übertragen werden.
- Bei fehlenden fachlichen Informationen sind gezielte Rückfragen zu stellen.
- Für reine Analyseaufträge bleibt das Repository unverändert.

### 2.2 Verbotene implizite Aktionen

Ohne ausdrückliche Freigabe sind insbesondere verboten:

- Dateien anlegen, verändern, verschieben oder löschen.
- NuGet- oder npm-Pakete hinzufügen, entfernen oder aktualisieren.
- Datenbankmigrationen erzeugen oder ausführen.
- Datenbankschemata verändern.
- Secrets, Zertifikate oder Zugangsdaten erzeugen.
- Deployment- oder Aspire-Konfiguration verändern.
- Git-Commits, Pushes, Rebases, Resets oder Branch-Löschungen ausführen.
- `git push --force`, `git reset --hard`, `git clean`, rekursive Löschungen oder
  vergleichbare destruktive Befehle.
- Sicherheitsmechanismen vereinfachen oder vorübergehend deaktivieren.
- Nicht angeforderte Refactorings.

### 2.3 Durchführung nach Freigabe

Nach einer ausdrücklichen Freigabe:

1. Ändere nur den freigegebenen Umfang.
2. Halte Änderungen klein und thematisch geschlossen.
3. Prüfe vor Beginn den Iststand erneut (Git-Status, Build, betroffene Dateien).
4. Führe passende Builds und Tests aus.
5. Dokumentiere:
   - geänderte Dateien,
   - fachliche und technische Auswirkungen,
   - ausgeführte Prüfungen,
   - verbleibende Risiken,
   - nicht überprüfte Annahmen.

### 2.4 Verbindliche Projektdokumentation

- Fachliche Planung liegt im Wiki und wird dort gepflegt (eigene Regeln des Wikis
  beachten).
- Grundlegende und langfristig relevante technische Entscheidungen erhalten einen
  ADR unter `docs/adr/`.
- Freigegebene Arbeits-Prompts werden unter `docs/prompts/` archiviert.
- Bei jeder Änderung ist zu prüfen, welche vorhandenen Dokumente aktualisiert
  werden müssen (`README.md`, `TASK.md`, ADRs, Wiki).
- Dokumentation darf keine Secrets oder echten Zugangsdaten enthalten.

## 3. Technologischer Rahmen

Entschiedener Zielstack (Änderungen nur nach ausdrücklicher Freigabe, anschließend
Wiki aktualisieren):

- .NET 10 mit C#.
- ASP.NET Core Minimal API (keine Controller).
- Entity Framework Core mit PostgreSQL.
- FluentValidation (als Pipeline-Decorator, keine DataAnnotations auf Commands).
- Serilog mit Seq als strukturiertes Logging.
- xUnit für automatisierte Tests.
- Swagger / OpenAPI für die Endpunkte.
- Keycloak 26.5 als Identity Provider.
- .NET Aspire 13 für die Orchestrierung.
- Docker als Containerisierungs-Basis.
- Angular 22 mit Tailwind CSS 3 als Frontend.
- Discogs-API als externe Metadatenquelle, serverseitig proxied.

Eigene CQRS-Implementierung — **kein MediatR** und kein vergleichbares Framework.

## 4. Architekturprinzipien

### 4.1 Zielarchitektur

Das Backend folgt der Onion-Architektur mit vier Layern (Details:
Wiki `architektur/architektur-übersicht.md`):

- **Core (Domain)**: Entities, Value Objects, Aggregates, Domain Events.
- **Application**: Use-Case-Interfaces (Commands und Queries) und Services.
- **Infrastructure**: Datenzugriff via PostgreSQL und EF Core.
- **API**: Web-APIs im Minimal-API-Stil.

Dazu kommen die im Wiki festgelegten Aspire-Projekte
(`architektur/aspire-orchestrierung.md`):

- `MyMusic.AppHost` — Orchestrierer, kennt alle Projekte und Ressourcen.
- `MyMusic.ServiceDefaults` — OpenTelemetry, Health Checks, Service Discovery.
- `MyMusic.Migrator` — separates Konsolen-Projekt für EF-Migrationen als
  einmaliger Aspire-Job (kein Dauerdienst).

Die konkreten Projektnamen der vier Onion-Layer sind im Wiki noch nicht
festgelegt und werden vor dem Anlegen der Solution zur Freigabe vorgeschlagen.
Zielbild — nicht ohne Prüfung des vorhandenen Standes blind erzeugen:

```text
src/
├── MyMusic.AppHost/
├── MyMusic.ServiceDefaults/
├── MyMusic.Migrator/
├── <Domain-, Application-, Infrastructure-, API-Projekt>
└── frontend/            # Angular-22-Workspace

tests/
└── <Testprojekte je Layer>
```

### 4.2 Abhängigkeitsrichtung

- Abhängigkeiten zeigen ausschließlich von außen nach innen.
- Die Domain kennt keine Infrastruktur, kein UI und keine Frameworks.
- Application darf Domain referenzieren.
- Infrastructure implementiert die in Application/Domain definierten Verträge
  (z. B. `IRepository<T>` aus `Domain/Contracts/Repository/`).
- Die API-Schicht exponiert die Anwendung nach außen und ist Composition Root.
- Kein PostgreSQL-/EF-Code in der Application-Schicht — nur `IRepository<T>`.
- Das Angular-Frontend greift ausschließlich über die HTTP-API zu.

### 4.3 CQRS und Repository

- Eigener `IMediator`, der Handler per Reflection aus dem DI-Container auflöst
  (Wiki `architektur/cqrs-framework.md`).
- Feature-Struktur im Application Layer
  (Wiki `architektur/application-layer.md`):

```text
Application/Features/{Kategorie}/{Entität}/
├── Commands/{Create|Update|Delete}/
├── Queries/{GetAll|GetById}/
└── ResponseDtos/ (+ Builder/)
```

- Jedes Feature ist vollständig in seinem Unterordner gekapselt; kein Handler
  greift in den Ordner eines anderen Features.
- Handler hängen nur von `ExceptionManager` und `ResponseBuilder` ab; das Mapping
  auf Response-DTOs übernimmt vollständig der `ResponseBuilder`.
- Kein Handler erzeugt Exceptions direkt — alle laufen über den `ExceptionManager`.
- Checkliste für jedes neue Feature: Commands → Queries → Response-DTOs →
  ResponseBuilder → Handler → DI-Registrierung → `GlobalUsing.cs`.

### 4.4 Domain-Regeln

Verbindlich für alle Entitäten und Value Objects
(Wiki `entwicklung/domain-regeln.md`):

- Alle Properties `private set` oder `private init` — kein Setzen von außen.
- Konstruktor `internal` — kein `new Entity()` außerhalb der Domain.
- Erzeugung ausschließlich über die statische `Create(...)`-Factory.
- `Update(...)` gibt immer eine neue Instanz zurück — `this` wird nie mutiert.
- Validierung im Konstruktor mit Exception (deutsche Fehlermeldungen).
- Value Objects als `record`-Typ, Konstruktor `private`, Einstieg über `Create()`.

## 5. Sicherheitsanforderungen

Sicherheit ist Kernanforderung, kein späteres Add-on. Verbindliche Quelle:
Wiki `sicherheit/sicherheitskonzept.md`.

### 5.1 Authentifizierung

- Keycloak als Identity Provider; Angular-Client nutzt Authorization Code + PKCE.
- Kein Client Secret im Browser, kein Implicit Flow.
- JWT-Validierung im API Layer über `AddAuthentication().AddJwtBearer()` mit der
  Keycloak-Authority-URL; `userId` stammt aus dem `sub`-Claim.
- Token-Lebensdauer: Access Token 5 Minuten, Refresh Token 30 Minuten (Sliding),
  SSO Session Max 8 Stunden.
- Realm und Client werden per JSON-Import eingerichtet (versioniert unter
  `/keycloak/`, Import via `--import-realm`).

### 5.2 Autorisierung und Mandantentrennung

- Rollen als Keycloak Realm Roles: `User` (CRUD auf eigene Daten) und `Admin`
  (zusätzlich Benutzer inkl. Daten löschen).
- `userId` ist Pflichtparameter in allen Queries und in allen Commands außer
  DeleteCommand; dort liefert ein injizierter `ICurrentUserService` die `userId`.
- Die `userId` stammt immer aus dem JWT (`sub`-Claim) — nie aus Request-Body
  oder Routing.
- Ownership-Prüfung im Handler: Ressource laden, `resource.UserId` vergleichen,
  bei Mismatch `NotFoundException` (HTTP 404, nicht 403 — Existenz nicht bestätigen).
- `.RequireAuthorization()` auf der Endpoint-Gruppe.

### 5.3 API-Ebene

- Eingaben werden serverseitig mit FluentValidation als Pipeline-Decorator vor
  dem Handler validiert; pro Command eine eigene `AbstractValidator<T>`-Klasse.
- Rate Limiting: 100 Requests pro Minute pro Benutzer über die eingebaute
  ASP.NET-Core-Middleware; bei Überschreitung HTTP 429.
- Swagger-UI in Production nur für die Admin-Rolle.
- CORS: Development alle `localhost`-Origins, Production explizite Whitelist;
  Methoden `GET/POST/PUT/DELETE`, Header `Authorization`, `Content-Type`.
- Content Security Policy gemäß Wiki (mindestens `default-src 'self'` usw.).

### 5.4 Secrets und Logging

- Keine Secrets im Repository, keine echten Zugangsdaten in `appsettings.json`.
- Lokal: .NET User Secrets (auch für Keycloak-Admin-Credentials als
  Aspire-Parameter mit `secret: true`). Production: noch offen
  (Environment Variables oder Docker Secrets).
- `Authorization`- und `Cookie`-Header werden aus dem Request-Logging
  ausgeschlossen; keine rohen Request-Bodies oder DTOs loggen.
- Log-Nachrichten strukturiert (Serilog-Templates), ohne PII und ohne Tokens.

## 6. Datenmodell und Persistenz

- Verbindliche Quellen: Wiki `datenbank/er-modell.md` und
  `datenbank/tabellenschema.md`.
- Kernentitäten: `Record`, `RecordTrack`, `Artist`, `Label`, `Genre`, `Country`
  (Details in Wiki `domain/`).
- Datenbankrechte: Der `MyMusic.Migrator` besitzt DDL + DML; die API erhält nur
  DML — Anwendungscode kann das Schema nicht verändern.
- Migrationen sind ein eigener, separat freizugebender Schritt und laufen als
  Aspire-Job vor dem API-Start (`WaitFor`-Kette gemäß Wiki).
- PostgreSQL-Daten in der Entwicklung über ein explizites Volume persistieren
  (Aspire-Container sind sonst ephemer).

## 7. Fehler- und Ausnahmekonzept

Verbindliche Quelle: Wiki `architektur/fehler-und-ausnahmekonzept.md`.

- Kein `try-catch` in Endpoints — zentrale Behandlung über `IExceptionHandler`.
- Fehlerklassen und Frontend-Darstellung:

| Fehler | HTTP | Darstellung im Frontend |
|---|---|---|
| Validierungsfehler | 400 | Inline am Eingabefeld, kein Modal |
| Nicht gefunden | 404 | Modal mit entitätsspezifischer Meldung |
| Konflikt (z. B. Name doppelt) | 409 | Modal |
| Nicht angemeldet / Rolle unzureichend | 401/403 | Weiterleitung zur Login-Seite |
| Serverfehler | 500 | Modal |
| Rate Limit überschritten | 429 | Modal |
| Netzwerk-/Verbindungsfehler | — | Modal mit „Erneut versuchen" |
| Discogs-Fehler | — | Modal mit Hinweis auf manuelle Eingabe |

- Browser-Konsolen-Logging nur im Development-Modus; in Production wird
  serverseitig über Serilog/Seq geloggt.

## 8. Frontend-Regeln

Verbindliche Quellen: Wiki `architektur/angular-projektstruktur.md`,
`architektur/navigation-konzept.md`, `architektur/ui-ux-konzept.md`,
Design System (Wiki `design/`).

- Feature-basierte Ordnerstruktur unter `features/` (dashboard, records, artists,
  labels, genres, search); jedes Feature mit eigener `*.routes.ts`, lazy geladen.
- Alle Komponenten standalone; State per Signals.
- Add/Edit-Formulare einheitlich als Modals — keine eigenen `/new`- oder
  `/edit`-Routen.
- Styling über Tailwind CSS und die Design-Tokens/Komponenten-Klassen des
  Design Systems — keine Ad-hoc-Farben oder -Abstände.
- API-Basis-URL zur Laufzeit über `runtime-config.json` (nicht zur Build-Zeit).

## 9. Konventionen

Verbindlich und vollständig: Wiki `entwicklung/codierrichtlinien.md`. Kernpunkte:

- Code-Bezeichner Englisch; Fehlermeldungen und Log-Nachrichten Deutsch.
- Keine Abkürzungen (`ArtistId` statt `ArtId`).
- Kommentare nur, wenn das *Warum* nicht offensichtlich ist.
- Backend: `PascalCase` für Typen/Methoden/Properties, `_camelCase` für private
  Felder; Namensschemata `{Aktion}{Entität}Command`, `{Command}Handler`,
  `{Entität}Response`, `{Entität}ResponseBuilder`, `{Entität}Endpoints`.
- Eine Klasse pro Datei; neue Namespaces in `GlobalUsing.cs`.
- Endpoint-Methoden `private static`.
- Frontend: `inject()` statt Konstruktor-Injection, `rxResource` statt
  `ngOnInit` + `subscribe`, Signal Forms statt `ReactiveFormsModule`, kein
  `NgModule`, kein `AsyncPipe` (stattdessen `toSignal()`), kein `any`.
- Signals nie direkt mutieren — `.update(data => ({ ...data }))`.

## 10. Teststrategie

Verbindliche Quellen: Wiki `test/grobe-testplanung.md`, `test/unit-tests.md`.

### 10.1 Testebenen

- **Unit Tests (xUnit)** mit höchster Priorität für: CQRS-Handler (inkl.
  Mandantentrennung und Fehlerfälle), eigenen Mediator (Handler-Auflösung,
  Exception bei fehlendem Handler), ResponseBuilder (Mapping inkl. `null` und
  verschachtelter Objekte), Command-Validierung (deutsche Fehlermeldungen)
  und Filter-Logik der GetAll-Queries (ohne Filter, einzeln, kombiniert).
- **Integrationstests** für Repository-Implementierungen gegen PostgreSQL,
  Keycloak/Token-Validierung, Minimal-API-Routing und HTTP-Statuscodes.
- **Frontend-Tests** für Angular-Komponenten und Filterfunktionen.

### 10.2 Testregeln

- Tests entstehen zusammen mit der Funktionalität, nicht nachträglich.
- Tests prüfen beobachtbares Verhalten, nicht private Implementierungsdetails.
- Sicherheitsszenarien gehören dazu: nicht authentifizierter Zugriff, Zugriff
  auf fremde Daten (404), unbekannte IDs, fehlende Pflichtfelder.
- `IRepository<T>` wird in Handler-Tests gemockt — keine Datenbank nötig.
- Jeder Bugfix erhält nach Möglichkeit einen reproduzierenden Test.
- Flaky Tests dürfen nicht ignoriert werden.
- Nicht vorhandene Tests, unprüfbare Annahmen und manuelle Prüfungen werden im
  Bericht ausdrücklich genannt — nicht nur erfolgreiche Tests melden.

## 11. Build und Verifikation

Ermittle vorhandene Solution- und Projektdateien, bevor Befehle festgelegt werden.

Typische Prüfungen (sobald die Solution existiert):

```powershell
dotnet restore
dotnet build --no-restore
dotnet test --no-build
dotnet format --verify-no-changes
ng test --watch=false
ng lint
git diff --check
```

Entwicklungsumgebung ist Windows mit PowerShell. Diese Befehle sind keine
pauschale Freigabe — führe nur Befehle aus, die zum vorhandenen Repository passen
und keine nicht freigegebenen Änderungen erzeugen.

Bei Änderungen an Datenbank, Authentifizierung oder API sind zusätzlich passende
Integrationstests erforderlich.

## 12. Abhängigkeiten

Vor dem Hinzufügen eines NuGet- oder npm-Pakets:

1. Begründe den konkreten Bedarf.
2. Prüfe, ob .NET, Angular oder die vorhandene Lösung die Funktion bereits bietet.
3. Bewerte Wartungsstatus, Lizenz, Sicherheitslage und Transitivabhängigkeiten.
4. Nenne mindestens eine Alternative.
5. Hole eine ausdrückliche Freigabe ein.

Kein MediatR (eigene CQRS-Implementierung ist entschieden). Keine Bibliothek wird
ausschließlich wegen Popularität oder eines Architekturtrends hinzugefügt.

## 13. Subagent-Nutzung

Definierte Rollen unter `.claude/agents/`:

- `analyst` — Iststand, Risiken und offene Fragen; nur lesend.
- `planner` — Zerlegung in kleine Schritte und Prompt-Erstellung; nur lesend.
- `implementer` — Umsetzung ausschließlich des freigegebenen Umfangs.
- `reviewer` — unabhängige Prüfung von Diff, Scope, Architektur und Tests;
  nur lesend, mit frischem Kontext (nicht aus der Planungssitzung heraus).

Regeln:

- Subagents bevorzugt für voneinander unabhängige, lesende Analysen einsetzen.
- Schreibende Parallelität vermeiden.
- Der Hauptagent sammelt alle Ergebnisse und löst Widersprüche sichtbar auf.
- Bei Architektur- oder Sicherheitsentscheidungen ist die Empfehlung mit
  Trade-offs zu begründen.
- Nur die für die Aufgabe notwendigen Rollen verwenden.

## 14. Antwortformat

Bei Analysen:

1. Ist-Zustand.
2. Gesicherte Erkenntnisse.
3. Offene Fragen oder Annahmen.
4. Risiken.
5. Ein bis drei Alternativen.
6. Begründete Empfehlung.
7. Konkreter nächster Schritt, der eine Freigabe benötigt.

Bei Implementierungen:

1. Umgesetzter Umfang.
2. Geänderte Dateien.
3. Wesentliche Entscheidungen.
4. Build- und Testergebnisse.
5. Nicht geprüfte Punkte.
6. Risiken oder Folgearbeiten.

Quellen und Dateireferenzen müssen präzise angegeben werden. Keine Aussage als
geprüft darstellen, wenn sie nicht geprüft wurde. Antworten erfolgen auf Deutsch.
