# Offene Aufgaben

Stand: 2026-08-28 (nach Abschluss von Block 0a, 0b, 0d, 0e, dem Genre-Backend aus
Block 2, dem Country-Backend aus Block 3, dem Label-Backend aus Block 4 und dem
Artist-Backend aus Block 5; Planung für Block 6 (Record/Tracks) abgeschlossen,
siehe Wiki `user-stories/user-stories-record.md`; Block 6a (Record-Backend),
Block 6b (Album-Cover-Upload), Block 6c (Track-Backend) und Block 6d
(Nachträge aus Block 2/4/5) umgesetzt und verifiziert — Block 6 damit
vollständig abgeschlossen; Block 6e (Nachtrag: unpaginierte GetAll-Endpunkte
für Genre/Label/Artist, siehe Abschnitt 6e) umgesetzt und verifiziert; Block 0c
(Angular-Workspace) umgesetzt und verifiziert; Block 7a (Angular-Login-Flow)
umgesetzt und verifiziert; Block 0f (Dark/Light-Theme-Infrastruktur) umgesetzt
und verifiziert, dazu ein kleiner Nachtrag (Favicon auf `mark.svg` umgestellt,
PR #44); Block 0g (NavComponent und Routing-Skelett) umgesetzt und verifiziert;
Angular-Feature `genres/` (Block 2 Frontend) umgesetzt und verifiziert, siehe
Abschnitt 2 — nach `main` gemergt; Angular-Feature `labels/` (Block 4 Frontend)
umgesetzt und verifiziert, siehe Abschnitt 4 — nach `main` gemergt;
Angular-Feature `artists/` (Block 5 Frontend) umgesetzt, siehe Abschnitt 5 —
nach `main` gemergt; die Records-Frontend-Planung (Block 6 Frontend) wurde
fortgesetzt und auf Wunsch des Projektinhabers in fünf einzeln abnehmbare
Teilblöcke 6f–6j zerlegt (analog zur Backend-Aufteilung 6a–6e), siehe
Abschnitt 6f; Block 6f (Record-Liste: Card-Ansicht, Filter, Sortierung,
Paginierung, inkl. eines neuen `format`-Filters bei `GET /api/records`,
der Verschiebung von `Country` nach `shared/` und eines serverseitigen
Freitext-Autosuggest für Artist/Label statt Dropdown) umgesetzt, live
verifiziert und nach `main` gemergt; Block 6g (Record anlegen/bearbeiten/
löschen als Modal, Edit/Delete-Icons auf der RecordCard, Vorbefüll-Erweiterung
von `shared/autocomplete/`, dazu die Erweiterung „Label/Artist direkt aus dem
Record-Formular anlegen" mit Anpassungen an `shared/modal/`,
`shared/autocomplete/`, `shared/confirm-modal/` und `LabelForm.saved`)
umgesetzt, live verifiziert und nach `main` gemergt; Block 6h
(Record-Detailansicht als Modal über der Liste, Kind-Route `/records/:id`,
reiner Lesemodus mit Tracklist, PR #59) umgesetzt, live verifiziert und
nach `main` gemergt; Block 6i (Cover-Upload im Records-Formular,
PR #61) umgesetzt, live verifiziert und nach `main` gemergt; Block 6j
(Tracks, Track-CRUD in der Detailansicht, PR #63) umgesetzt, live
verifiziert und nach `main` gemergt — Records-Frontend damit vollständig,
Block 6 (Backend+Frontend) fachlich vollständig; Block 7f
(Keycloak-Login-Theme „mymusic", siehe Abschnitt 7f) umgesetzt und live
verifiziert, PR #69, nach `main` gemergt. Zusätzlich Block 7b (Rollenkonzept
User/Admin im Angular-Code: `UserRolesService`, `AdminGuard`, Admin-Button in
der Kopfzeile, Platzhalter-Route `/admin`, siehe Abschnitt 7b) umgesetzt,
automatisiert getestet (366 Frontend-Tests grün) und live gegen den
laufenden Aspire-AppHost verifiziert, PR #71, nach `main` gemergt. Block 7c
(Admin-Bereich, Backend und Frontend: `GET /api/admin/users`,
`DELETE /api/admin/users/{id}`, serverseitige Admin-Autorisierungspolicy,
Keycloak-Service-Account-Client für die Admin REST API, Angular-Feature
`features/admin/` mit Userliste und Löschen, siehe Abschnitt 7c) umgesetzt,
automatisiert getestet (alle Unit- und Integrationstests sowie 375
Frontend-Tests grün), PR #74, nach `main` gemergt; die manuelle
Live-Verifikation im Browser (Userliste, Löschen inkl. Bestätigungsmodal,
Zugriffsschutz für Nicht-Admins, sowie eine vertiefte Prüfung direkt gegen
Postgres und Keycloak, dass App-Daten und Keycloak-Account eines gelöschten
Benutzers wirklich entfernt sind und ein Login danach mit 401 fehlschlägt)
wurde am 2026-08-20 nachgeholt, siehe Nachtrag in Abschnitt 7c — Block 7c
damit vollständig abgeschlossen. Block 7g (Registrierung:
`registrationAllowed` aktiviert,
automatische Zuweisung der Realm-Rolle `User` an neu registrierte
Benutzer, Registrieren-Button in der Kopfzeile, unbewachte Landing-Route
für den nicht angemeldeten Zustand, siehe Abschnitt 7g) umgesetzt,
automatisiert getestet (380 Frontend-Tests grün) und live gegen den
laufenden Aspire-AppHost verifiziert, PR #77, nach `main` gemergt. Block 7h
(Admin-Benutzersuche: `GET /api/admin/users` um einen optionalen
`search`-Parameter erweitert, Suchfeld mit Autocomplete im Admin-Bereich,
siehe Abschnitt 7h) umgesetzt, automatisiert getestet (253
Application-Tests und 384 Frontend-Tests grün) und live gegen den
laufenden Aspire-AppHost verifiziert, PR #80, nach `main` gemergt. Block 8a
(Discogs-Backend-Proxy: `GET /api/discogs/search`, `GET /api/discogs/
releases/{id}`, externer HTTP-Client `IDiscogsClient`/`DiscogsClient` nach
Vorbild des `KeycloakAdminClient`, neue Fehlerklasse
`DiscogsUnavailableException` auf HTTP 502 Bad Gateway, siehe Abschnitt 8)
umgesetzt und automatisiert getestet (411 Tests grün), PR #82, nach `main`
gemergt. Block 8b (Discogs-Frontend-Integration: Such-Modal
`discogs-search/` im `RecordForm`, automatische Übernahme von Albumname,
Erscheinungsjahr, Label, Record-Artist, Cover und aller Tracks nach
Auswahl eines Treffers, Rückfrage bei neuer Label-/Artist-/Genre-Referenz,
dazu eine gezielte Erweiterung von Block 8a um ein Pro-Track-Artist-Feld
und die serverseitige Cover-Einbettung als Data-URL, siehe Abschnitt 8)
umgesetzt, automatisiert getestet (397 Backend- und 437 Frontend-Tests
grün), PR #84, nach `main` gemergt; die manuelle Live-Verifikation gegen
die echte Discogs-API wurde in mehreren Runden durchgeführt (u. a. anhand
des Various-Artists-Release 91831 „Atmos – Headcleaner") und zuletzt ohne
weitere Befunde bestätigt — Block 8 (Backend+Frontend) damit vollständig
abgeschlossen. Block 9 (Dashboard: `GET /api/dashboard` mit einer neuen
Projektions-Methode `IRepository<T>.GetProjectedAsync` gegen unnötig
geladene Cover-Daten, ADR 0021; `DashboardComponent` mit fünf
Kind-Komponenten, siehe Abschnitt 9) umgesetzt, automatisiert getestet
(406 Backend- und 451 Frontend-Tests grün), PR #86, nach `main` gemergt;
die manuelle Live-Verifikation wurde in mehreren Runden durchgeführt und
deckte dabei Layout- und Lesbarkeitsprobleme auf (siehe Abschnitt 9), die
jeweils korrigiert und erneut live bestätigt wurden — Block 9
(Backend+Frontend) damit vollständig abgeschlossen. Block 10
(Volltext-Suche: `GET /api/search`, durchsucht ausschließlich Records über
Titel, Record-/Track-Artist, Label, Genre und Land per ILIKE, eigene
Response-DTOs statt Wiederverwendung der Record-Response wegen der
Feature-Kapselung aus CLAUDE.md 4.3; Frontend-Darstellung als Card-Raster
wie die Records-Ansicht, voll editierbar, Klick auf eine Card navigiert zur
Detailansicht inkl. Tracklist, dazu eine Eingabevalidierung am
Kopfzeilen-Suchfeld, siehe Abschnitt 10) umgesetzt, automatisiert getestet
und live gegen den laufenden Aspire-AppHost verifiziert, PR #88, nach
`main` gemergt. Block 7i (Rate Limiting: Fixed-Window-Limiter der
eingebauten `Microsoft.AspNetCore.RateLimiting`-Middleware, 100
Requests/Minute partitioniert über den `sub`-Claim, HTTP 429 mit
`Retry-After`-Header, begrenzt auf Pfade unter `/api`, siehe Abschnitt 7i)
umgesetzt, automatisiert getestet (440 Backend-Tests grün), PR #90, nach
`main` gemergt. Block 7j (Production-Zugriffsschutz: Swagger-UI außerhalb
Development hinter einem Middleware-Gate auf die bestehende
`"Admin"`-Policy, neue `ProductionCors`-Policy mit Origin-Whitelist aus
Konfiguration, CSP für Development/lokal per Meta-Tag mit Angulars
`CSP_NONCE`-Mechanismus, siehe Abschnitt 7j) umgesetzt, automatisiert
getestet (443 Backend- und 460 Frontend-Tests grün) und live gegen den
laufenden Aspire-AppHost verifiziert, PR #92, nach `main` gemergt; dabei
zusätzlich zwei Nebenbefunde behoben — `RequireHttpsMetadata` war an
`IsDevelopment()` gekoppelt und hätte außerhalb Development jede Anfrage
mit 500 beantwortet (ADR 0024), und das CI-Timeout für Integrationstests
war mit 15 Minuten bereits vor diesem Block strukturell knapp (12m48s bei
17 Tests im letzten Lauf zuvor) und wurde auf 20 Minuten angehoben. CSP für
Production (HTTP-Header vom Nginx) bleibt offen, abhängig vom noch nicht
begonnenen Production-/Docker-Compose-Setup. Block 7k (Benutzerprofil:
E-Mail und Passwort selbst ändern über ein neues Modal, Klick auf den
Username in der Kopfzeile, siehe Abschnitt 7k) umgesetzt, automatisiert
getestet und live gegen den laufenden Aspire-AppHost verifiziert, PR #94,
nach `main` gemergt — das Feature war seit Block 0g im Wiki beschrieben,
aber nie umgesetzt und tauchte auch nirgends als offener Punkt auf; neue
Wiki-Seite `user-stories-benutzerprofil.md` holt das nach.
Branch: `main` (Block 6b per PR #30, Block 6c per
PR #32, Block 6d per PR #34, Block 0c per PR #36, Block 7a per PR #41,
Block 0f per PR #43, der Favicon-Nachtrag per PR #44, Block 0g per PR #45,
Block 2 Frontend per PR #47, Block 4 Frontend per PR #49, Block 5 Frontend
per PR #52, Block 6e per PR #54, Block 6f per PR #55, Block 6g per PR #57,
Block 6h per PR #59, Block 6i per PR #61, Block 6j per PR #63, Block 7f per
PR #69, Block 7b per PR #71, Block 7c per PR #74, Block 7g per PR #77,
Block 7h per PR #80, Block 8a per PR #82, Block 8b per PR #84, Block 9 per
PR #86, Block 10 per PR #88, Block 7i per PR #90, Block 7j per PR #92,
Block 7k per PR #94 nach `main` gemergt)

Diese Datei ist die operative Arbeitsliste für die nächsten Umsetzungsschritte.
Sie ersetzt nicht die fachliche Planung im Wiki
(`../../02 Wiki/MyMusic Wiki/wiki/`), sondern verdichtet die offenen Punkte aus
Feature-Roadmap und aktuellem Repository-Stand.

## Arbeitsregeln

- Jeder Block wird als eigener Arbeits-Prompt geplant (Plan Mode) und unter
  `docs/prompts/` archiviert.
- Jeder Block wird separat freigegeben, umgesetzt, geprüft und committet.
- Das Wiki ist die fachlich verbindliche Quelle; Abweichungen werden gemeldet.
- Keine Secrets, Zertifikate oder produktiven Daten ins Repository.
- Diese `TASK.md` wird nach jedem abgeschlossenen Block aktualisiert.

## Aktuell nicht umgesetzt

Block 0a, 0b, 0d, 0e, 0c, 0f, 0g, 7a und 7f sind abgeschlossen. Offen aus
dem MVP-Umfang der Phase 1:

- CRUD-Slices für Record und Tracks (Genre-, Country-, Label- und
  Artist-Backend erledigt, siehe Abschnitte 2–5; Record-Backend ohne Tracks
  (Block 6a), Album-Cover-Upload (Block 6b), Track-Backend (Block 6c) und die
  Nachträge aus Block 2/4/5 (Block 6d) erledigt, siehe Abschnitt 6 — damit
  vollständig abgeschlossen; dazu Block 6e (unpaginierte GetAll-Endpunkte für
  Genre/Label/Artist, siehe Abschnitt 6e); Angular-Features `genres/`,
  `labels/`, `artists/` und `records/` jetzt entsperrt und mit gültigem
  Access Token aufrufbar (Block 7a), Navigation und Routing-Skelett stehen
  (Block 0g); `genres/` als Referenz-Slice umgesetzt (Block 2 Frontend, siehe
  Abschnitt 2); `labels/` umgesetzt (Block 4 Frontend, siehe Abschnitt 4);
  `artists/` umgesetzt, aber ohne UI für den `labelId`-Filter (Block 5
  Frontend, siehe Abschnitt 5); `records/` hat mit Block 6f (Record-Liste:
  Card-Ansicht, Filter, Sortierung, Paginierung) die Platzhalterseite
  ersetzt, siehe Abschnitt 6f, mit Block 6g Anlegen/Bearbeiten/Löschen
  erhalten, siehe Abschnitt 6g, und mit Block 6h die Detailansicht als
  Modal erhalten, siehe Abschnitt 6h, und mit Block 6i den Cover-Upload,
  siehe Abschnitt 6i, und mit Block 6j das Track-CRUD in der
  Detailansicht, siehe Abschnitt 6j — das Records-Frontend ist damit
  vollständig).
- Zustandsbewertung nach Goldmine-Standard (Datenmodell bereits Teil des
  `record`-Schemas, siehe Abschnitt 6).
- CSP für Production (HTTP-Header vom Nginx, siehe Abschnitt 7j) — abhängig
  vom noch nicht begonnenen Production-/Docker-Compose-Setup (siehe Abschnitt
  7; das Keycloak-Custom-Theme der Anmeldeseite aus demselben Abschnitt ist
  mit Block 7f erledigt, das Rollenkonzept im Angular-Code — `AdminGuard`,
  Admin-Button, Platzhalter-Route `/admin` — mit Block 7b, der Admin-Bereich
  selbst (Userliste/-löschung über die Keycloak Admin REST API) mit Block 7c
  erledigt, inklusive der am 2026-08-20 nachgeholten Live-Verifikation im
  Browser, siehe Abschnitt 7c; die Registrierung neuer Benutzer ist mit
  Block 7g erledigt, siehe Abschnitt 7g; Rate Limiting ist mit Block 7i
  erledigt, siehe Abschnitt 7i; Swagger-Admin-Gate außerhalb Development,
  CORS-Production-Whitelist und CSP für Development/lokal sind mit Block 7j
  erledigt, siehe Abschnitt 7j).

## 0. Fundament: Walking Skeleton

Block 0 wurde in drei einzeln prüfbare Teilblöcke zerlegt, weil das
Abnahmekriterium des Gesamtblocks erst ganz am Ende messbar gewesen wäre.

### 0a. Solution- und Aspire-Fundament

Status: **abgeschlossen** (2026-07-15)
Arbeits-Prompt: `docs/prompts/2026-07-15-block-0a-fundament.md`

Umgesetzt:

- .NET-10-Solution (`MyMusic.slnx`) mit den vier Onion-Layern `MyMusic.Domain`,
  `MyMusic.Application`, `MyMusic.Infrastructure`, `MyMusic.Api` (ADR 0001).
- Testprojekte je Layer plus `MyMusic.IntegrationTests`.
- `MyMusic.AppHost` und `MyMusic.ServiceDefaults` (Aspire 13.4.6).
- PostgreSQL, Seq und Keycloak 26.5 als Aspire-Ressourcen mit Datenvolumes.
- Boot-Reihenfolge: Migrator → `WaitFor(PostgreSQL)`, API →
  `WaitForCompletion(Migrator)` + `WaitFor(Keycloak)`.
- `MyMusic.Migrator` als einmaliger Job mit DDL-Rechten.
- Keycloak-Realm als JSON-Import unter `/keycloak/`; Admin-Credentials als
  Aspire-Parameter über User Secrets.
- DB-Berechtigungskonzept: Rolle `mymusic_api` mit reinen DML-Rechten, per
  Integrationstest abgesichert.
- Serilog mit Console- und Seq-Sink (ADR 0002).

Nachtrag (2026-08-06): Die obige Zeile "PostgreSQL, Seq und Keycloak 26.5 als
Aspire-Ressourcen mit Datenvolumes" traf für Keycloak ursprünglich nicht zu —
Keycloak hatte seit Block 0a kein Datenvolume, nur einen read-only Bind-Mount
für den Realm-Import. Das Keycloak-Datenvolume für die Dev-Umgebung wurde
nachträglich ergänzt (`mymusic-keycloak-data` auf `/opt/keycloak/data`, siehe
Wiki `architektur/aspire-orchestrierung.md`). Für Production ist weiterhin die
im Wiki (`projekt/backup-konzept.md`) dokumentierte Anbindung von Keycloak an
eine PostgreSQL-Datenbank offen — das betrifft das noch ausstehende
Production-/Docker-Compose-Setup, nicht Block 0a.

Nachtrag (2026-08-07): Die Keycloak-Endpunkte `http` und `management` hatten
seit Block 0a keine festen Host-Ports, sondern von Aspire zufällig vergebene
(`WithHttpEndpoint()` setzte nur `targetPort`, kein `port:`-Argument). Das
widersprach der im Wiki (`architektur/aspire-orchestrierung.md`, Abschnitt
„Port-Konfiguration") dokumentierten Entscheidung für feste Host-Ports 8080
(http) und 9000 (management). In `AppHost.cs` nachträglich ergänzt.

Bewusst nicht Teil von 0a:

- JWT-Verdrahtung im Code und Auth-Smoke-Test (0b).
- Erste echte EF-Migration — separat freizugeben, gehört zum Genre-Slice.

### 0b. CQRS, Repository und Auth-Smoke-Test

Status: **abgeschlossen** (2026-07-26)
Arbeits-Prompt: `docs/prompts/2026-07-26-block-0b-cqrs-repository-auth.md`

Umgesetzt:

- CQRS-Eigenframework (`IMediator`/`Mediator`, `ICommand<TResponse>`/
  `IQuery<TResponse>`, `ICommandHandler<,>`/`IQueryHandler<,>`,
  `CommandValidationDecorator` mit FluentValidation) in
  `MyMusic.Application/Common/CQRS/`; Handler-Registrierung per Assembly-Scan
  über `AddApplication()`.
- Generisches `IRepository<T>` (`MyMusic.Domain/Contracts/Repository/`) und
  EF-Core-Implementierung `Repository<T>`
  (`MyMusic.Infrastructure/Persistence/Repositories/`); noch nicht in die
  API-DI verdrahtet, da 0b keine Entität hat, die ihn braucht (folgt mit dem
  Genre-Slice).
- `ExceptionManager` (`ValidationException`, `NotFoundException`,
  `ConflictException`) und zentraler `GlobalExceptionHandler`
  (`IExceptionHandler`) in `MyMusic.Api`, mappt auf HTTP 400/404/409/500.
- `AddAuthentication().AddJwtBearer()` gegen die Keycloak-Authority
  (`ValidAudience = "account"`, `MapInboundClaims = false` — ADR 0004) und
  `ICurrentUserService`/`CurrentUserService` (liest `sub`-Claim).
- Smoke-Test-Endpunkt `GET /api/me` (`GetCurrentUserQuery` →
  `GetCurrentUserQueryHandler` → `CurrentUserResponseBuilder`),
  `.RequireAuthorization()`.
- Neues Testprojekt `MyMusic.Infrastructure.Tests` für den generischen
  Repository-Unit-Test (gemockter `DbContext`/`DbSet`).
- Integrationstest `tests/MyMusic.IntegrationTests/MeEndpointTests.cs`: `/api/me`
  ohne Token → 401, mit echtem Keycloak-Token → 200; dafür dedizierter
  Test-Client `mymusic-integration-tests` im Realm-Import (ADR 0005).

Nachträge nach unabhängiger Review:

- `CurrentUserResponseBuilder` ergänzt — der Handler baute das Response-DTO
  zunächst direkt, was der ausnahmslosen Regel „Handler hängen nur von
  ExceptionManager und ResponseBuilder ab" (CLAUDE.md §4.3/§9) widersprach.
- Paket-Tabelle im Arbeits-Prompt um `Microsoft.Extensions.DependencyInjection.Abstractions`
  (Application) und `Microsoft.Extensions.DependencyInjection` (Application.Tests)
  ergänzt — beide waren im Diff enthalten, aber nicht dokumentiert.
- `RepositoryTests`: negativer Testfall `GetByIdAsync` → `null` bei unbekannter
  Id ergänzt.
- `tests/MyMusic.IntegrationTests/AssemblyInfo.cs`: Testparallelität für die
  Assembly deaktiviert (`CollectionBehavior(DisableTestParallelization = true)`) —
  behebt einen im Review beobachteten Timeout bei gemeinsamer Ausführung
  beider Integrationstests (zwei parallele Aspire-Stacks konkurrierten um
  Ressourcen). Mit gemeinsamem Lauf erneut verifiziert (2/2 grün).

Abnahmekriterium erfüllt:

- Unit Tests grün (Domain/Application/Api/Infrastructure); die Kette
  Keycloak → API ist per Integrationstest gegen einen echten
  Keycloak-Container nachgewiesen (lokal mit Docker ausgeführt, isoliert und
  gemeinsam mit dem bestehenden Integrationstest).

Bewusst nicht Teil von 0b:

- Rollenkonzept, Ownership-Prüfung, Rate Limiting, CORS-Policy, CSP
  (Abschnitt 7 — dafür fehlen die Entitäten, an denen Ownership überhaupt
  geprüft werden könnte).
- DI-Verdrahtung von `IRepository<T>`/`MyMusicDbContext` in `MyMusic.Api` und
  die reale Prüfung des Repositorys gegen PostgreSQL (folgt mit dem
  Genre-Slice, Block 2).

Bekannte Lücke:

- `Repository<T>.GetAllAsync` ist per Unit Test nicht absicherbar (EF Cores
  `ToListAsync()` verlangt `IAsyncEnumerable<T>` auf dem `DbSet`, ein reiner
  NSubstitute-Mock implementiert das in der verwendeten EF-Core-Version nicht
  — empirisch geprüft). Realer Nachweis folgt mit dem
  Genre-Slice-Integrationstest gegen PostgreSQL.

### 0c. Angular-Workspace

Status: **abgeschlossen** (2026-08-08)

Umgesetzt:

- Angular-22-Workspace unter `src/frontend/` (`npx @angular/cli@22 new`,
  `--routing --style=css --ssr=false --skip-git --package-manager=npm --strict`).
  Angular 22 verwendet standardmäßig **Vitest** statt Karma/Jasmine und ist
  **zoneless** (kein `zone.js` mehr in den Dependencies); der Static-Assets-Ordner
  heißt `public/` (nicht mehr `src/assets/`).
- Tailwind CSS **3** (nicht das aktuelle npm-`latest` 4.x, passend zur
  Tech-Stack-Entscheidung) plus PostCSS/Autoprefixer; `tailwind.config.js` aus
  `../../02 Wiki/MyMusic Wiki/raw/design_system/tailwind/tailwind.config.js`
  unverändert übernommen.
- Design-System-Anbindung: `colors_and_type.css`/`components.css` unverändert
  nach `src/frontend/src/styles/design-system/` kopiert, `mark.svg` als
  Markenzeichen übernommen. Inter/JetBrains Mono **self-hosted** über
  `@fontsource/inter`/`@fontsource/jetbrains-mono` (keine Google-Fonts-CDN-
  Abhängigkeit zur Laufzeit, Klärung mit dem Projektinhaber).
- Minimale App-Shell (Marke, Wortmarke, Platzhaltertext) als Nachweis der
  Design-System-Anbindung — bewusst ohne Navigation, Routing-Hierarchie oder
  Feature-Ordner (`core/`, `shared/`, `features/*` aus
  `wiki/architektur/angular-projektstruktur.md` folgen erst mit den einzelnen
  Angular-Feature-Blöcken).
- `RuntimeConfigService` (`provideAppInitializer()` + `fetch()`) lädt
  `public/runtime-config.json` vor dem Rendern; ein `prestart`/`prebuild`-Skript
  (`scripts/write-runtime-config.mjs`) schreibt die Datei aus der Umgebungsvariable
  `MYMUSIC_API_BASE_URL` — Details und Alternativenabwägung in
  `docs/adr/0009-angular-runtime-config-mechanismus.md`.
- AppHost-Einbindung: **`AddNpmApp()` existiert in Aspire 13 tatsächlich nicht
  mehr** — der Ersatz ist nicht `AddJavaScriptApp()` allein, sondern ein eigenes
  Paket `Aspire.Hosting.JavaScript` 13.4.6 (löst das bei 9.5.2 eingefrorene
  `Aspire.Hosting.NodeJs` ab), das u. a. `AddJavaScriptApp()`, `AddNodeApp()`,
  `AddViteApp()`, `AddNextJsApp()`, `AddBunApp()` bereitstellt — verifiziert
  gegen die installierte Assembly und den offiziellen Aspire-Sample
  `playground/AspireWithJavaScript` (dotnet/aspire, Tag `v13.4.6`). `AppHost.cs`
  ergänzt um `builder.AddJavaScriptApp("frontend", "../frontend", runScriptName: "start")`
  mit `.WithReference(api)`, `.WaitFor(api)`,
  `.WithEnvironment("MYMUSIC_API_BASE_URL", api.GetEndpoint("https"))`,
  `.WithHttpEndpoint(env: "PORT")`, `.WithExternalHttpEndpoints()`. Das
  `api`-Ressourcen-Bauteil musste dafür erstmals in eine Variable gefasst werden
  (vorher inline). `package.json` nutzt dasselbe `run-script-os`-Muster wie das
  offizielle Aspire-Angular-Sample (`ng serve --port %PORT%`/`--port $PORT`), da
  `ng serve` selbst kein `PORT`-Env-Var liest.
- Node.js musste von v22.16.0 auf v22.23.2 aktualisiert werden (Angular CLI 22
  verlangt mindestens v22.22.3/v24.15.0/v26) — mit Freigabe des Projektinhabers
  systemweit per offiziellem MSI-Installer nachgezogen.

Bewusst nicht Teil dieses Standes:

- Keine Feature-Ordner, keine Navigation, kein Login-Flow, kein AuthGuard/
  Interceptor, keine CORS-Änderung am Backend, kein Lucide-Icon-Paket, kein
  echter API-Aufruf aus Angular außer `runtime-config.json`.
- Kein Production-Publish-Pfad (`PublishAsDockerFile()` o. Ä.) für das Frontend
  — nur lokale Aspire-Entwicklung.

Abnahmekriterium erfüllt:

- Das Frontend startet über den AppHost (`frontend`-Ressource, Port dynamisch
  über `PORT`), lädt Tailwind/Design-System/Fonts und die tatsächliche
  `runtime-config.json` mit der realen API-URL.

### 0d. CI-Gate für Codequalität

Status: **abgeschlossen** (2026-07-24)
Arbeits-Prompt: `docs/prompts/2026-07-24-ci-gate-codequalitaet.md`

Umgesetzt:

- `.editorconfig` im Repo-Root: file-scoped Namespaces, Naming-Regel für
  private Felder (`_camelCase`, Konstanten ausgenommen).
- `.github/workflows/ci.yml`: Restore, Build, `dotnet format
  --verify-no-changes`, Zeilenlängen-Check (max. 120 Zeichen), Unit-Tests
  (Domain, Application, Api) bei jedem Push/PR auf `main`.
- ADR `docs/adr/0003-ci-gate-codequalitaet.md`.

Bewusst nicht Teil von 0d:

- `MyMusic.IntegrationTests` läuft nicht in der CI (braucht Docker +
  Aspire-Orchestrierung + Secrets — eigener, größerer Schritt).
- Kein Branch-Protection-Rule-Setup (Repository-Einstellung, eigener Schritt).
- Kein StyleCop/Roslynator; projektspezifische Regeln (Namensschemata,
  Feature-Kapselung, Kommentar-Ausnahmen) bleiben Aufgabe des
  `reviewer`-Subagenten.

### 0e. Swagger/OpenAPI-Dokumentation

Status: **abgeschlossen** (2026-08-05)
Arbeits-Prompt: `docs/prompts/2026-08-05-block-0e-swagger-openapi.md`

Nachgeholt: Swagger/OpenAPI ist seit Projektbeginn als Tech-Stack-Entscheidung
dokumentiert (CLAUDE.md §3/§5.3/§9, Wiki `tech-stack/swagger.md`), war aber in
keinem der bisherigen Blöcke als Aufgabe erfasst und blieb trotz drei bereits
umgesetzter Endpunkte (`/api/me`, `/api/genres`, `/api/countries`) ungenutzt.

Umgesetzt:

- Paket `Swashbuckle.AspNetCore` in `MyMusic.Api.csproj`; `GenerateDocumentationFile`
  aktiviert, damit die vorhandenen `<summary>`-Kommentare der Endpoint-Handler
  (Genre, Country, Me) exportiert und von Swagger eingelesen werden.
- `Program.cs`: `AddEndpointsApiExplorer()`, `AddSwaggerGen(...)` mit
  Bearer-Security-Definition (JWT aus Keycloak, über den „Authorize"-Button in
  der UI setzbar, damit geschützte Endpunkte über die UI testbar sind) und
  `IncludeXmlComments(...)`.
- `UseSwagger()`/`UseSwaggerUI()` ausschließlich innerhalb
  `if (app.Environment.IsDevelopment())` — siehe „Bewusst nicht Teil" unten.
- ADR `docs/adr/0007-swagger-openapi-nur-development.md`.

Nachtrag (2026-08-05): Das Aspire-Dashboard zeigte für die `api`-Ressource nur
den Basis-Endpoint, keinen direkten Link auf `/swagger`. `AppHost.cs` um
`.WithUrlForEndpoint("https", url => { url.DisplayText = "Swagger UI"; url.Url
+= "/swagger"; })` ergänzt, damit im Dashboard ein direkter „Swagger
UI"-Shortcut neben der `api`-Ressource erscheint.

Nachtrag (2026-08-08): Die am 2026-08-05 offen gebliebene Live-Verifikation
(Verifikationsschritt 4/5, Aufruf von `/swagger` in Development) wurde
nachgeholt und lieferte dabei einen 500er auf `/swagger/v1/swagger.json`. Über
die Seq-Logs des laufenden AppHosts wurde die genaue Ursache ermittelt:
`SwaggerGeneratorException` beim Cover-Upload-Endpunkt
(`POST /api/records/{id}/cover`), weil der `IFormFile`-Parameter in
`UploadRecordCoverAsync` explizit mit `[FromForm]` annotiert war — Swashbuckle
unterstützt diese Kombination nicht (offizielle Doku:
„You're not supposed to decorate IFormFile parameters with the FromForm
attribute", `configure-and-customize-swaggergen.md#handle-forms-and-file-uploads`).
Fix: `[FromForm]`-Attribut in
`src/MyMusic.Api/Endpoints/Sammlung/Record/RecordEndpoints.cs` entfernt
(ASP.NET Core erkennt die Formularbindung für `IFormFile` automatisch anhand
des Typs); `.DisableAntiforgery()` bleibt unverändert bestehen (ADR 0008 ist
davon nicht betroffen). Live erneut geprüft: `/swagger/v1/swagger.json`
liefert 200, alle Endpunkte (inkl. Cover-Upload) erscheinen in der UI.
Regressionstest `tests/MyMusic.IntegrationTests/SwaggerEndpointTests.cs`
ergänzt, der `/swagger/v1/swagger.json` aufruft und 200 erwartet.

Bewusst nicht Teil von 0e:

- Freischaltung der Swagger-UI in Production für die Admin-Rolle (CLAUDE.md
  §5.3) — das Rollenkonzept (`User`/`Admin`) existiert im Code noch nicht
  (siehe Abschnitt 7). Wird dort nachgezogen, sobald die Admin-Rolle entsteht.

### 0f. Dark/Light-Theme-Infrastruktur

Status: **abgeschlossen** (2026-08-11)
Arbeits-Prompt: `docs/prompts/2026-08-11-block-0f-theme-infrastruktur.md`

Anlass: Das Wiki (`navigation-konzept.md`, `ui-kit.md`) wurde am 2026-08-10 um
einen Light/Dark-Toggle in der Kopfzeile ergänzt (vorher Widerspruch zwischen
beiden Seiten). Das Design-System (`tailwind.config.js`,
`colors_and_type.css`) war bereits vollständig auf Dark-Mode vorbereitet, die
Angular-Anwendungsschicht hat das nie genutzt — kein Toggle, kein
`ThemeService`, keine `data-theme`-Steuerung.

Umgesetzt:

- `ThemeService` (`src/frontend/src/app/core/theme/theme.service.ts`,
  `providedIn: 'root'`, Signal-basiert): Drei-Zustands-Logik — keine
  gespeicherte Präferenz folgt live der OS-Einstellung (`prefers-color-scheme`,
  kein `data-theme`-Attribut gesetzt, reiner CSS-Fallback bleibt aktiv);
  explizite Wahl (`light`/`dark`) setzt das Attribut und übersteuert die OS-
  Einstellung dauerhaft. `toggle()` kehrt das aktuell effektive Theme um.
- `ThemeToggle`-Komponente (`core/theme/theme-toggle/`): Icon-Button (Sonne/
  Mond, kein Label, laut Wiki-Ausnahme vom sonstigen Label-only-Muster der
  Kopfzeile), Inline-SVG mit den öffentlichen Lucide-Pfaddaten (kein neues
  npm-Paket für nur zwei Icons — `lucide-angular` bleibt dem künftigen
  `NavComponent`-Block vorbehalten, siehe ADR 0011).
  In `app.html` zwischen Logo/Titel und dem Login/Logout-Block eingebunden,
  unabhängig vom Anmeldestatus immer sichtbar.
- Kleines Inline-Script in `index.html` (vor jeder Angular-Ausführung), das
  eine gespeicherte explizite Präferenz sofort anwendet — verhindert einen
  sichtbaren Farb-Flash beim Laden, der sonst entstünde, weil `ThemeService`
  erst nach dem `RuntimeConfigService`-Fetch im Bootstrap existiert (ADR 0009).
- `localStorage`-Key `mymusic-theme` (nicht die Abkürzung `mm-theme` aus dem
  Rohprototyp — CLAUDE.md §9 verbietet Abkürzungen).
- Neue Unit Tests (Vitest): `theme.service.spec.ts` (7 Fälle: OS-Fallback
  Light/Dark, explizite Präferenz übersteuert OS, ungültiger gespeicherter
  Wert wird ignoriert, Live-Reaktion auf OS-Änderung nur ohne explizite Wahl,
  `toggle()` inkl. Persistenz), `theme-toggle.spec.ts` (3 Fälle: Icon/
  `aria-label` je Zustand, Klick löst `toggle()` aus). `app.spec.ts` angepasst:
  `ThemeService`-Stub ergänzt, vier `querySelector('button')`-Aufrufe auf
  `button.btn-secondary` präzisiert (sonst hätte der neu eingefügte
  Toggle-Button vor Login/Logout gefunden und die Assertions verfälscht).
  29 Frontend-Tests insgesamt, alle grün.
- ADR `docs/adr/0011-theme-infrastruktur.md` (Storage-Key, Drei-Zustands-Logik
  vs. abweichendes Rohprototyp-Verhalten, Icon-Herkunft, FOUC-Script).

Bewusst nicht Teil dieses Standes:

- Favicon (`mark.svg` als Favicon einbinden), Tab-Leiste, Tabellen-Layouts,
  Suchfeld, Admin-Button, vollständige `NavComponent` — existieren im Code
  noch nicht und sind für einen späteren Block vorgesehen.
- `lucide-angular` als npm-Paket (siehe oben).

Abnahmekriterium erfüllt:

- Ohne gespeicherte Präferenz folgt das Theme der OS-Einstellung; ein Klick
  auf den Toggle wechselt sichtbar zwischen Light/Dark und bleibt nach einem
  Neuladen der Seite erhalten; kein sichtbarer Farb-Flash bei abweichender
  expliziter Wahl.

### 0g. NavComponent und Routing-Skelett

Status: **abgeschlossen** (2026-08-13)
Arbeits-Prompt: `docs/prompts/2026-08-13-block-0g-nav-routing-skeleton.md`

Anlass: Das Wiki (`navigation-konzept.md`) wurde am 2026-08-13 final
abgeschlossen. Die Angular-Anwendungsschicht hatte bis dahin weder
`features/`-Ordner noch eine echte `NavComponent` — `app.routes.ts` zeigte
für `''` und `'**'` gleichermaßen auf die provisorische `HomePlaceholder`
aus Block 0c. Bevor die eigentlichen CRUD-Features (Genre/Label/Artist/
Record) ans Frontend angebunden werden, sollte die Navigation samt
Routing-Skelett einmal zentral stehen, statt in jedem der vier kommenden
Feature-Blöcke einzeln nachgezogen zu werden.

Mit dem Projektinhaber geklärter Scope: kein Admin-Button/AdminGuard (bleibt
Teil des noch offenen Rollenkonzepts, Abschnitt 7), kein responsives
Hamburger-Menü, Suchfeld funktional ans Routing angebunden, kein
Benutzerprofil-Modal (Username bleibt reiner Text ohne Klick-Handler).

Umgesetzt:

- `app.routes.ts`: verschachtelte Route mit `canActivate: [authGuard]` auf
  dem Eltern-Knoten, sechs `loadChildren`-Einträge auf
  `features/{dashboard,records,artists,labels,genres,search}/*.routes.ts`;
  `''` und `'**'` redirecten auf `/dashboard` (kein eigenes 404-Konzept im
  Wiki, ein Redirect ist wartungsfrei).
- Sechs neue, minimale Platzhalter-Komponenten unter `features/`, je mit
  eigener `*.routes.ts` — kein gemeinsames `shared/feature-placeholder/`,
  da `shared/` laut Wiki für dauerhaft wiederverwendbare Bausteine reserviert
  ist und ein Platzhalter reiner, pro Feature-Block vollständig zu
  ersetzender Wegwerfcode ist. `search/` liest den `q`-Query-Parameter
  bereits funktional über `toSignal(route.queryParamMap...)`.
- `core/shell/home-placeholder/` entfernt (durch echte Feature-Routen
  ersetzt).
- Neue `NavComponent` (`src/app/nav/`): Brand mit den bis dahin ungenutzten
  Design-System-Klassen `.appbar`/`.brand`, Tabs Dashboard/Records
  (`.tab`/`.tab.is-active`, `routerLinkActive`), Option-Dropdown
  (Artists/Labels/Genres, Zustand direkt in `Nav` statt eigener
  Sub-Komponente, Schließen bei Klick außerhalb per
  `@HostListener('document:click', ...)`), funktionales Suchfeld über
  **Signal Forms** (`form()`/`[formField]`, erste Verwendung im Projekt —
  `@angular/forms/signals` war bereits installiert, aber ungenutzt),
  Theme-Toggle (nur Positionswechsel), Login/Logout/Username (unverändert
  aus `App` übernommen).
- `App`/`app.html`/`app.css` auf reine Shell reduziert
  (`<app-nav /><router-outlet />`).
- Paket `@lucide/angular` installiert (Version 1.31.0) — siehe
  Nachtrag unten und ADR 0012.
- Neue/angepasste Tests (Vitest): `nav.spec.ts` (11 Fälle), reduziertes
  `app.spec.ts`, neues `app.routes.spec.ts` (`RouterTestingHarness`,
  `angular-auth-oidc-client` per `vi.mock` neutralisiert, da nur die eigene
  Routing-Verdrahtung geprüft wird, nicht das Bibliotheks-Guard-Verhalten),
  sechs `features/*/*.spec.ts`. 50 Frontend-Tests insgesamt, alle grün.

Nachtrag (2026-08-13): Bei der Paketprüfung vor der Installation (CLAUDE.md
§12) stellte sich heraus, dass `lucide-angular` — der in ADR 0011 für einen
späteren Block vorgesehene Paketname — inzwischen laut Hersteller deprecated
ist. Installiert wurde stattdessen der aktiv gepflegte Nachfolger
`@lucide/angular` (gleiche ISC-Lizenz, Peer-Deps `>=17.0.0`, kompatibel mit
Angular 22.1). API-Unterschied: einzelne Standalone-Icon-Komponenten statt
`LucideAngularModule.pick(...)`. Details und Alternativenabwägung in ADR
0012. Die Icon-Zuordnung aus `wiki/glossar.md` (dashboard, records, artists,
labels, genres, search) wurde unverändert übernommen; `chevron-down` für den
Option-Dropdown-Trigger war dort nicht benannt, aber durch den bereits
bestehenden Chevron in der `.select`-Klasse (`components.css`) als
Präzedenzfall begründet.

Bewusst nicht Teil dieses Standes:

- Admin-Button, `AdminGuard`, `/admin`-Route (Rollenkonzept, Abschnitt 7).
- Responsives Verhalten (Icon-only-Tabs, Hamburger-Menü bei schmalem
  Viewport) — nur normales Desktop/Tablet-Verhalten umgesetzt.
- Benutzerprofil-Modal (Klick auf Username ohne Funktion, wie bisher).
- Fachlicher Inhalt der sechs Feature-Seiten selbst — reine Platzhalter,
  die echte Umsetzung folgt mit den jeweiligen CRUD-Feature-Blöcken.

Abnahmekriterium erfüllt:

- Nach Login zeigt die Kopfzeile Brand, Tabs, Suchfeld, Theme-Toggle,
  Username/Logout; Klick auf Dashboard/Records/Option-Einträge navigiert
  korrekt und markiert den aktiven Tab; Suche mit Eingabe+Enter navigiert zu
  `/search?q=...`; direkter Aufruf einer unbekannten URL landet auf
  `/dashboard`.
  (Korrigiert am 2026-08-29: Die Suche löst seitdem live beim Tippen aus,
  eine Enter-Bestätigung ist nicht mehr nötig, funktioniert aber weiterhin
  zusätzlich — siehe docs/prompts/2026-08-29-fix-globale-suche-live-eingabe.md.)

## 1. Planung: User Stories und Akzeptanzkriterien

Status: abgeschlossen (Genre: 2026-07-29; Country: 2026-08-05;
Label: 2026-08-07; Artist: 2026-08-07; Record/Tracks: 2026-08-07;
Admin-Bereich: 2026-08-16 — bislang nicht in dieser Liste vermerkt, siehe
Nachtrag unten)
Priorität: hoch, jeweils vor dem zugehörigen Slice

Ziel:

- Die im Wiki (`offene-themen.md`) benannte Lücke schließen: strukturierte
  Szenarien mit messbaren Abnahmekriterien je MVP-Feature.

Aufgaben:

- Pro anstehendem Slice User Stories mit Akzeptanzkriterien im Wiki
  ergänzen — nicht alles auf einmal.
  - Genre: erledigt, siehe
    `../../02 Wiki/MyMusic Wiki/wiki/user-stories/user-stories-genre.md`.
  - Country: erledigt, siehe
    `../../02 Wiki/MyMusic Wiki/wiki/user-stories/user-stories-country.md`.
  - Label: erledigt, siehe
    `../../02 Wiki/MyMusic Wiki/wiki/user-stories/user-stories-label.md`.
  - Artist: erledigt, siehe
    `../../02 Wiki/MyMusic Wiki/wiki/user-stories/user-stories-artist.md`.
  - Record/Tracks: erledigt, siehe
    `../../02 Wiki/MyMusic Wiki/wiki/user-stories/user-stories-record.md`.
  - Admin-Bereich: erledigt (2026-08-16), siehe
    `../../02 Wiki/MyMusic Wiki/wiki/user-stories/user-stories-admin.md`.
- Die sechs Prüfkriterien der groben Testplanung als Grundlage nutzen.

Abnahmekriterium:

- Für den jeweils nächsten Slice existieren im Wiki Stories mit beobachtbaren,
  testbaren Kriterien, bevor der Arbeits-Prompt erstellt wird.

## 2. Slice: Genre

Status: **Backend und Frontend abgeschlossen** (Backend: 2026-08-04; Frontend:
2026-08-13, per PR #47 nach `main` gemergt).
Referenz-Slice: Das Angular-Feature etabliert die Muster (Signal Forms mit
Validierung, `rxResource`, `ErrorModalService`, `shared/`-Bausteine), die
Label/Artist/Record übernehmen sollen, siehe Arbeits-Prompt
`docs/prompts/2026-08-13-block-2-angular-genre.md`.
Priorität: hoch, erster fachlicher Durchstich

Ziel:

- Einfachster vertikaler Slice durch alle Schichten als Referenz für alle
  weiteren Entitäten.

Voraussetzung erledigt:

- User Stories und Akzeptanzkriterien liegen vor, siehe
  `../../02 Wiki/MyMusic Wiki/wiki/user-stories/user-stories-genre.md`
  (2026-07-29, ergänzt 2026-08-04).

Umgesetzt (Backend):

- Domain-Entität `Genre` (`Domain/DomainModels/Stammdaten/Genre/`) nach den
  Domain-Regeln; `IRepository<T>` um `GetPagedAsync(...)` erweitert
  (Filter-Expression, OrderBy-Delegate, page/pageSize, vollständig
  datenbankseitig) und in `Repository<T>` implementiert.
- Commands (Create, Update, Delete), Queries (GetById, GetPaged),
  Validatoren, Response-DTOs (`GenreResponse`, `GenreListResponse`) und
  `GenreResponseBuilder` unter `Application/Features/Stammdaten/Genre/`.
- Minimal-API-Endpoints (`GenreEndpoints`, `/api/genres`) mit
  `.RequireAuthorization()`; `MyMusic.Api/Program.cs` erstmals mit
  `MyMusicDbContext`- und `IRepository<T>`-DI-Verdrahtung (in 0b bewusst
  zurückgestellt, siehe dortige Notiz).
- Erste EF-Migration `CreateGenreTable` (legt ausschließlich die
  `genre`-Tabelle an, passend zu `tabellenschema.md`).
- Unit Tests: Domain (`GenreTests`), Application (Handler inkl.
  Mandantentrennung über kompilierte Filter-Expressions, Validatoren,
  `GenreResponseBuilder`) — 42 neue Tests, alle grün.
- Integrationstest `GenreEndpointsTests` (voller CRUD-Fluss, Paginierung,
  Filter, Sortierung, Mandantentrennung mit zwei Testbenutzern) nach Muster
  `MeEndpointTests`; Keycloak-Test-Client-Logik nach
  `TestSupport/KeycloakTestClient.cs` extrahiert. **Hinweis (korrigiert, siehe
  CLAUDE.md §11)**: In der Umsetzungssitzung schlug der Testlauf mit einem
  Fehler (`Service ... should have valid address at this point`) fehl —
  reproduzierbar auch beim unveränderten, bereits vorher funktionierenden
  `MeEndpointTests`. Es handelte sich **nicht** um eine Aspire/DCP-
  Einschränkung, sondern um die Ausführung über Git Bash statt PowerShell.
- ADR `docs/adr/0006-domain-entity-materialisierung-und-namenskollision.md`.

Umgesetzt (Frontend, 2026-08-13):

- `shared/`-Basisbausteine (neu angelegt, ab sofort für alle künftigen
  CRUD-Slices vorgesehen): `shared/http/problem-details.ts`, `shared/modal/`,
  `shared/confirm-modal/`, `shared/error-modal/` (Service + Komponente,
  global in `app.html` gemountet), `shared/pagination/`.
- `features/genres/genre.ts` (Interfaces), `genre.service.ts`
  (`HttpTestingController`-getestet) unter `features/genres/`.
- `Genres`-Shell mit `rxResource` ersetzt den bisherigen Platzhalter
  vollständig; `GenreFilter` (Signal Form mit `debounce`), `GenreTable`
  (inkl. eingebetteter `Pagination`) und `GenreForm` (Signal Forms mit
  `required`/`minLength`/`maxLength`/`pattern`, serverseitige 400-Fehler
  über `submit()` inline ins Namensfeld eingehängt) sowie die
  `ConfirmModal`-Verdrahtung für Delete.
- 105 Frontend-Tests grün (`npm test`), Production-Build und Prettier-Check
  grün; `ng lint` ist im Projekt (noch) nicht als Zielkonfiguration
  vorhanden (kein `lint`-Target in `angular.json`, kein ESLint-Setup) —
  daher nicht ausführbar, stattdessen Prettier (`printWidth: 100`) als
  Formatierungsprüfung verwendet.

Bewusst nicht Teil dieses Standes:

- Referenzprüfung gegen `record_track` in `DeleteGenreCommandHandler` (siehe
  Slice 6 unten) — die Tabelle existiert erst dort.
- Manuelle Live-Prüfung im Browser gegen den laufenden Aspire-AppHost (siehe
  Arbeits-Prompt, Abschnitt „Verifikation") steht noch aus.

Abnahmekriterium:

- Genres lassen sich anlegen, anzeigen, filtern, bearbeiten und löschen;
  fremde Benutzerdaten sind nicht sichtbar; Tests decken Happy Path,
  Validierung und unbekannte IDs ab. **Vollständig erfüllt** (Backend seit
  2026-08-04, Frontend seit 2026-08-13) — die manuelle Live-Prüfung im
  Browser steht noch aus.

Nachtrag (2026-08-05): `GenreEndpointsTests` lief seit der Umsetzung nie
erfolgreich durch — Ursache war die Ausführung über Git Bash statt PowerShell,
keine Aspire/DCP-Einschränkung (siehe unten „CI-Gate für Integrationstests" und
CLAUDE.md §11). Beim ersten tatsächlichen Lauf mit PowerShell zeigte sich ein echter
Bug in `UpdateGenreCommandHandler`: `Repository<T>.GetByIdAsync` liefert über
`DbSet.FindAsync` eine getrackte Entität zurück; da `Genre.Update(...)` laut
Domain-Regel immer eine neue Instanz erzeugt, kollidierte
`Repository<T>.Update(...)` mit dem EF-Core-Change-Tracker (zwei Instanzen
gleicher Id). Fix: `GetByIdAsync` löst die Entität nach dem Laden explizit
vom Change-Tracker (`context.Entry(entity).State = EntityState.Detached`).
Bekannte Lücke: Der Erfolgsfall von `GetByIdAsync` (Entität gefunden) ist
dadurch per Unit Test mit reinem NSubstitute-Mock nicht mehr absicherbar —
`EntityEntry<T>` hat nur einen internen Konstruktor, NSubstitute kann keinen
Proxy dafür erzeugen (empirisch geprüft, dieselbe Einschränkungsklasse wie
bei `GetAllAsync`). Verifiziert wird der Erfolgsfall stattdessen über
`GenreEndpointsTests` gegen echtes PostgreSQL.

**Nachtrag, behoben (entdeckt und behoben 2026-08-13, Branch
`fix-genreform-vorbefuellung`, Arbeits-Prompt
`docs/prompts/2026-08-13-fix-genreform-vorbefuellung.md`)**: `GenreForm`
(`features/genres/genre-form/genre-form.ts`) initialisierte das
Formularmodell mit `signal<GenreFormModel>({ name: this.genre()?.name ?? ''
})` als reinem Feld-Initialisierer. Entdeckt während Block 4
(Label-Frontend): Bei `LabelForm` verursachte exakt dieses Muster einen
echten Bug — der `label`-Input ist zum Konstruktionszeitpunkt noch nicht
gesetzt, wodurch das Formular im Bearbeiten-Modus mit einem leeren statt
vorbefüllten Feld startete. Bestätigt auch für `GenreForm`: Der bestehende
Test (`genre-form.spec.ts`, „Bearbeiten-Modus") überschrieb das Namensfeld
immer unbedingt und bemerkte die fehlende Vorbefüllung nicht. Fix:
`linkedSignal(() => ({ name: this.genre()?.name ?? '' }))` statt
`signal(...)`. Neuer Test „befüllt das Namensfeld im Bearbeiten-Modus mit
dem bestehenden Namen vor" ergänzt, der genau das prüft, statt das Feld
sofort zu überschreiben — schlägt ohne den Fix fehl (verifiziert). 149
Frontend-Tests insgesamt, alle grün.

## CI-Gate für Integrationstests (2026-08-05)

`MyMusic.IntegrationTests` lief bislang nur lokal und blieb über mehrere
Sitzungen hinweg ungeprüft — Ursache war die Ausführung über Git Bash statt
PowerShell (siehe CLAUDE.md §11), keine Aspire/DCP-Einschränkung; auf dem
Linux-CI-Runner tritt der Fehler ohnehin nicht auf, da dort kein Git Bash zum
Einsatz kommt. `.github/workflows/ci.yml`
führt den Integrationstest jetzt bei jedem Push/PR auf `main` mit aus; eine
Branch-Protection-Regel auf `main` verlangt einen erfolgreichen CI-Lauf vor
dem Merge. Details: ADR 0003, Nachtrag 2026-08-05.

## 3. Slice: Country

Status: **Backend abgeschlossen** (2026-08-05); Angular-Feature entfällt für
Country vollständig — es gibt keine CRUD-Maske, siehe Klärung im
Arbeits-Prompt `docs/prompts/2026-08-05-block-3-country.md`.
Priorität: mittel, vor Label benötigt

Ziel:

- Herkunftsländer als Stammdaten für Labels (Wiki `domain/country.md`).

Voraussetzung erledigt:

- User Story und Akzeptanzkriterium liegen vor, siehe
  `../../02 Wiki/MyMusic Wiki/wiki/user-stories/user-stories-country.md`
  (2026-08-05).

Umgesetzt (Backend):

- Domain-Entität `Country` (`Domain/DomainModels/Stammdaten/Country/`) nach
  den Domain-Regeln — `internal`-Konstruktor, `Create(...)`-Factory, **kein**
  `Update()` (Länder werden nie mutiert), keine Regex-/Zeichensatzprüfung
  (die Referenzliste enthält bewusst nicht-ISO-konforme Werte wie `YU`,
  `---`).
- Erster Slice mit dem bislang ungenutzten `GetAll`-Muster:
  `GetAllCountriesQuery` (parameterlos, kein `userId` — Ausnahme von der
  sonst geltenden Regel, siehe Nachtrag unten), `GetAllCountriesQueryHandler`,
  `CountryResponse`, `CountryResponseBuilder` unter
  `Application/Features/Stammdaten/Country/`. Sortierung alphabetisch nach
  Landesname (`StringComparer.InvariantCulture`).
- Minimal-API-Endpoint (`CountryEndpoints`, `GET /api/countries`) mit
  `.RequireAuthorization()`, ohne `ICurrentUserService` (kein Mandantenbezug).
- EF-Migration `CreateCountryTable` (legt die `country`-Tabelle an und
  seedet einmalig alle 238 Einträge aus
  `../../02 Wiki/MyMusic Wiki/wiki/domain/country-referenzdaten.md` per
  `InsertData`).
- Unit Tests: Domain (`CountryTests`, 7 Tests), Application
  (`GetAllCountriesQueryHandlerTests`, `CountryResponseBuilderTests`, 4
  Tests) — alle grün.
- Integrationstest `CountryEndpointsTests` (401 ohne Token, 200 mit Token,
  Anzahl == 238, alphabetische Sortierung, Stichprobe „Deutschland"/„DE")
  nach Muster `GenreEndpointsTests`; grün.
- Wiki-Nachtrag: `cqrs-framework.md` um die Ausnahme „`GetAll` ohne `userId`
  bei Referenztabellen ohne `user_id`" ergänzt (Widerspruch zwischen Zeile 32
  und dem `GetPaged`-Abschnitt aufgelöst).

Bewusst nicht Teil dieses Standes:

- Kein Angular-Feature — Country hat keinen eigenen Reiter und keine
  CRUD-Maske (Designentscheidung „Countries read-only", siehe
  `api-endpunkte.md`). Die Länderliste wird erst mit Block 4 (Label) und
  Block 0c (Angular) im echten Verwendungskontext (Dropdown) sichtbar.

Abnahmekriterium:

- Länder stehen bei der Label-Pflege zur Auswahl. **Backend erfüllt**
  (`GET /api/countries` liefert die vollständige, sortierte Liste); die
  UI-seitige Anbindung im Label-Formular folgt mit Block 4 und Block 0c.

## 4. Slice: Label

Status: **Backend und Frontend abgeschlossen** (Backend: 2026-08-07; Frontend:
2026-08-13, per PR #49 nach `main` gemergt). Übernimmt die Muster aus
Block 2 (Genre-Frontend, Referenz-Slice) 1:1, siehe Arbeits-Prompt
`docs/prompts/2026-08-13-block-4-angular-label.md`.
Priorität: mittel

Ziel:

- Label als Stammdaten für Records, mit Herkunftsland-Referenz auf
  [[country]] (Wiki `domain/label.md`).

Voraussetzung erledigt:

- User Stories und Akzeptanzkriterien liegen vor, siehe
  `../../02 Wiki/MyMusic Wiki/wiki/user-stories/user-stories-label.md`
  (2026-08-07).

Umgesetzt (Backend):

- Domain-Entität `Label` (`Domain/DomainModels/Stammdaten/Label/`) nach den
  Domain-Regeln — Name Pflichtfeld 1–60 Zeichen mit erweitertem Zeichenset
  gegenüber Genre (zusätzlich `.` und `/`, bewusst ohne Klammern, siehe
  Klärung 2026-08-07), `information` optional (max. 255 Zeichen),
  `CountryId` als reiner Wert ohne Navigationseigenschaft (DDD-Aggregatgrenze
  zu Country).
- Erster Slice mit einer Fremdschlüsselbeziehung zwischen zwei
  Stammdaten-Entitäten: `LabelConfiguration` konfiguriert
  `country_id → country.id` ohne CLR-Navigation
  (`HasOne<CountryEntity>().WithMany().HasForeignKey(...)`), mit explizitem
  `OnDelete(DeleteBehavior.Restrict)` (EF-Core-Default für Pflichtbeziehungen
  wäre sonst `Cascade`).
- Commands (Create, Update, Delete), Queries (GetById, GetPaged mit Filter
  nach Name und `countryId`), Response-DTOs (`LabelResponse`,
  `LabelListResponse`) und `LabelResponseBuilder` unter
  `Application/Features/Stammdaten/Label/`. `LabelResponse` löst den
  Ländernamen serverseitig auf (Einzelabruf: `IRepository<CountryEntity>.
  GetByIdAsync`; Liste: einmaliger `GetAllAsync`-Aufruf mit anschließendem
  Dictionary-Lookup, da Country eine kleine, vollständig zwischenspeicherbare
  Referenztabelle ist).
- Neues Muster: asynchrone FluentValidation-Regel (`MustAsync`) in
  `CreateLabelCommandValidator`/`UpdateLabelCommandValidator` prüft die
  Existenz der `countryId` gegen `IRepository<CountryEntity>` — liefert
  HTTP 400 bei ungültigem Land (Klärung 2026-08-07, kein 404).
- Minimal-API-Endpoints (`LabelEndpoints`, `/api/labels`) mit
  `.RequireAuthorization()`.
- EF-Migration `CreateLabelTable` (legt die `label`-Tabelle mit
  `UNIQUE (user_id, label_name)` und FK-Constraint `country_id → country.id`
  mit `ON DELETE RESTRICT` an).
- Unit Tests: Domain (`LabelTests`, 19 Fälle inkl. Zeichenset- und
  CountryId-Validierung), Application (Handler, Validatoren inkl. neuem
  Testfall für die asynchrone Länder-Existenzprüfung, `LabelResponseBuilder`
  — 34 Fälle) — 53 neue Unit-Tests, alle grün.
- Integrationstest `LabelEndpointsTests` (voller CRUD-Fluss, Paginierung,
  Filter nach Name und Land, Sortierung, Mandantentrennung, 400/404/409) nach
  Muster `GenreEndpointsTests`; grün (gemeinsamer Lauf mit
  `MeEndpointTests`/`GenreEndpointsTests`/`CountryEndpointsTests`, 5/5 grün).

Umgesetzt (Frontend, 2026-08-13):

- `features/labels/country.ts`/`country.service.ts` (neu, feature-lokal statt
  in `shared/`, da Country aktuell nur von Label konsumiert wird) gegen
  `GET /api/countries`; `features/labels/label.ts`/`label.service.ts` analog
  zu Genre, zusätzlich mit `countryId`-Filterparameter.
- `Labels`-Shell mit zwei `rxResource`-Aufrufen (Labels paginiert, Länder
  einmalig), `LabelFilter` (Namensfeld mit 300-ms-Debounce wie Genre, Land als
  natives `<select>` ohne Debounce — sofortige Filterung bei Auswahl),
  `LabelTable` (Spalten Name/Land/Information/Aktionen — lange Information
  wird per CSS gekürzt mit vollem Text im `title`-Tooltip, jedes Feld hat
  aber immer eine eigene Spalte) und `LabelForm` (Name, Herkunftsland,
  optionales Freitextfeld `information`; serverseitige 400-Fehlerzuordnung
  jetzt für drei mögliche Feldschlüssel statt einem).
- **Mit dem Projektinhaber geklärt (2026-08-13)**: Länderauswahl als natives
  HTML-`<select>` (nutzt die bereits vorhandene `.select`-Klasse aus
  `components.css`) statt einer neuen Searchable-Combobox-Komponente —
  löst eine zuvor offene Planungslücke (weder Wiki noch bisheriger Code
  hatten dafür ein Muster).
- **Nachtrag/Bugfix während der Umsetzung**: `LabelForm` befüllte das
  Länderfeld im Bearbeiten-Modus zunächst nicht vor (Signal-Formularmodell
  wurde mit `signal(this.buildInitialModel())` als reiner Feld-Initialisierer
  gelesen, bevor der `label`-Input tatsächlich gesetzt war — leeres Feld
  verhinderte durch die `required`-Regel sogar das Absenden). Durch eigene,
  über den Genre-Testumfang hinausgehende Tests aufgedeckt (Genres
  Formular-Tests überschreiben das Namensfeld immer unbedingt und hätten den
  gleichen Fehler nicht bemerkt). Fix: `formModel` nutzt jetzt `linkedSignal
  (() => this.buildInitialModel())` statt `signal(...)` — reaktiv, korrekt
  ausgewertet, sobald der Input tatsächlich vorliegt. **Offener Verdacht**:
  `GenreForm` (Block 2 Frontend) verwendet dasselbe
  `signal(this.genre()?.name ?? '')`-Muster und könnte denselben Fehler beim
  Vorbefüllen im Bearbeiten-Modus haben — dort unentdeckt, weil der
  bestehende Test das Namensfeld ebenfalls immer überschreibt. Nicht im
  Rahmen dieses Blocks behoben (Scope-Grenze), dem Projektinhaber gemeldet.
- 30 neue Frontend-Tests (`country.service.spec.ts`, `label.service.spec.ts`,
  `label-filter.spec.ts`, `label-table.spec.ts`, `label-form.spec.ts`,
  `labels.spec.ts`), 146 Frontend-Tests insgesamt, alle grün. Production-Build
  und Prettier-Check grün; `ng lint` weiterhin nicht konfiguriert (wie bei
  Genre).

Bewusst nicht Teil dieses Standes:

- Referenzprüfung gegen `record` in `DeleteLabelCommandHandler` — bereits mit
  Block 6d (PR #34) umgesetzt, siehe dortige Notiz; im Frontend nicht erneut
  aufgeführt.
- Manuelle Live-Prüfung im Browser gegen den laufenden Aspire-AppHost steht
  noch aus, insbesondere der 409-Referenzfall beim Löschen (mangels
  Record-Frontend nur über Swagger nachweisbar, siehe Arbeits-Prompt).

Abnahmekriterium:

- Labels lassen sich anlegen, anzeigen, filtern, bearbeiten und löschen;
  fremde Benutzerdaten sind nicht sichtbar; Tests decken Happy Path,
  Validierung und unbekannte IDs ab. **Vollständig erfüllt** (Backend seit
  2026-08-07, Frontend seit 2026-08-13) — die manuelle Live-Prüfung im
  Browser steht noch aus.

## 5. Slice: Artist

Status: **Backend und Frontend abgeschlossen** (Backend: 2026-08-07;
Frontend: 2026-08-13, PR #52 nach `main` gemergt). Übernimmt die Muster aus
Block 2 (Genre-Frontend, Referenz-Slice) 1:1, siehe Arbeits-Prompt
`docs/prompts/2026-08-13-block-5-angular-artist.md`.
Priorität: mittel

Ziel:

- Artist als Stammdaten für Records und Tracks (Wiki `domain/artist.md`).

Voraussetzung erledigt:

- User Stories und Akzeptanzkriterien liegen vor, siehe
  `../../02 Wiki/MyMusic Wiki/wiki/user-stories/user-stories-artist.md`
  (2026-08-07).

Umgesetzt (Backend):

- Domain-Entität `Artist` (`Domain/DomainModels/Stammdaten/Artist/`) nach
  den Domain-Regeln — strukturell nahezu identisch zu Genre (kein
  Fremdschlüssel, kein Zusatzfeld), mit eigenen Namensregeln: Name
  Pflichtfeld 3–120 Zeichen mit demselben erweiterten Zeichenset wie Label
  (zusätzlich `.` und `/`, bewusst ohne Klammern, siehe Klärung 2026-08-07).
- Commands (Create, Update, Delete), Queries (GetById, GetPaged mit Filter
  nach Name), Response-DTOs (`ArtistResponse`, `ArtistListResponse`) und
  `ArtistResponseBuilder` unter `Application/Features/Stammdaten/Artist/`.
- Minimal-API-Endpoints (`ArtistEndpoints`, `/api/artists`) mit
  `.RequireAuthorization()`.
- EF-Migration `CreateArtistTable` (legt die `artist`-Tabelle mit
  `UNIQUE (user_id, artist_name)` an; kein Fremdschlüssel).
- Unit Tests: Domain (`ArtistTests`, 15 Fälle inkl. Zeichensatz- und
  Längenvalidierung), Application (Handler, Validatoren,
  `ArtistResponseBuilder` — 39 Fälle) — 54 neue Unit-Tests, alle grün.
- Integrationstest `ArtistEndpointsTests` (voller CRUD-Fluss, Paginierung,
  Namensfilter, Sortierung, Mandantentrennung, 400/404/409) nach Muster
  `GenreEndpointsTests`; grün (gemeinsamer Lauf mit `MeEndpointTests`/
  `GenreEndpointsTests`/`CountryEndpointsTests`/`LabelEndpointsTests`,
  6/6 grün).

Umgesetzt (Frontend, 2026-08-13):

- `features/artists/artist.ts` (Interfaces), `artist.service.ts`
  (`HttpTestingController`-getestet) unter `features/artists/` — 1:1-Muster
  von `features/genres/`.
- `Artists`-Shell mit `rxResource` ersetzt den bisherigen Platzhalter
  vollständig; `ArtistFilter` (Signal Form mit `debounce`, ausschließlich
  Namensfilter), `ArtistTable` (inkl. eingebetteter `Pagination`) und
  `ArtistForm` (Signal Forms mit `required`/`minLength(3)`/`maxLength(120)`/
  `pattern` inkl. `.`/`/`, serverseitige 400-Fehler über `submit()` inline
  ins Namensfeld eingehängt, `formModel` von Anfang an mit `linkedSignal`
  statt `signal` — vermeidet den Block-2/4-Vorbefüllungsbug von vornherein)
  sowie die `ConfirmModal`-Verdrahtung für Delete.
- **Mit dem Projektinhaber geklärt (2026-08-13)**: Der seit Block 6d
  serverseitig vorhandene `labelId`-Filter bei `GET /artists` bekommt in
  diesem Block **keine** UI — Label hat anders als Country keinen
  ungefilterten „Alle Labels"-Endpunkt, nur das auf 100 Einträge geklemmte
  paginierte `GET /labels`. US-A2 bleibt im Frontend dadurch nicht
  vollständig erfüllt (siehe Arbeits-Prompt
  `docs/prompts/2026-08-13-block-5-angular-artist.md`, Abschnitt „Risiken").
- 33 neue Frontend-Tests, 182 Frontend-Tests insgesamt, alle grün.
  Production-Build und Prettier-Check (für die neu angelegten/geänderten
  Dateien) grün; `ng lint` weiterhin nicht konfiguriert (wie bei Genre/Label).

Bewusst nicht Teil dieses Standes:

- UI für den `labelId`-Filter bei `GET /artists` (siehe oben).
- Manuelle Live-Prüfung im Browser gegen den laufenden Aspire-AppHost steht
  noch aus, insbesondere der 409-Referenzfall beim Löschen (mangels
  Record-Frontend nur über Swagger nachweisbar, siehe Arbeits-Prompt).

Abnahmekriterium:

- Artists lassen sich anlegen, anzeigen, filtern, bearbeiten und löschen;
  fremde Benutzerdaten sind nicht sichtbar; Tests decken Happy Path,
  Validierung und unbekannte IDs ab. **Vollständig erfüllt** (Backend seit
  2026-08-07, Frontend seit 2026-08-13; Filterung im Frontend vorerst nur
  nach Name, siehe oben) — die manuelle Live-Prüfung im Browser steht noch
  aus.

## 6. Slice: Record und Tracks

Status: **Backend abgeschlossen** (2026-08-08); aufgeteilt in vier einzeln
prüfbare Teilblöcke (analog Block 0), da das Abnahmekriterium des
Gesamtblocks erst ganz am Ende messbar wäre. Block 6a, 6b, 6c und 6d
abgeschlossen. Dazu Block 6e (Nachtrag, siehe dortiger Abschnitt). Das
Angular-Frontend für `records/` wurde auf Wunsch des Projektinhabers
zusätzlich in fünf einzeln abnehmbare Teilblöcke 6f–6j zerlegt (analog zur
Backend-Aufteilung); Block 6f, 6g, 6h, 6i und 6j abgeschlossen — das
Records-Frontend ist damit vollständig.
Priorität: hoch, fachlicher Kern

Ziel:

- Ein Record mit Tracks und Zustandsbewertung ist vollständig anleg-, filter-,
  sortier-, bearbeit- und löschbar.

Voraussetzung erledigt:

- User Stories und Akzeptanzkriterien liegen vor, siehe
  `../../02 Wiki/MyMusic Wiki/wiki/user-stories/user-stories-record.md`
  (2026-08-07). Dabei mehrere bislang undokumentierte Regeln geklärt (u. a.
  Duplikate bei Record ausdrücklich erlaubt, Fremdreferenzen `labelId`/
  `artistId`/`genreId` immer mandantengefiltert mit HTTP 400 bei ungültiger
  oder fremder Id, `record_side` auf 1–3 alphanumerische Zeichen korrigiert,
  `track_number` eindeutig pro Record+Seite, wählbare Sortierung für
  `GET /records`, Jahresfilter als Von-Bis-Zeitraum). Offen geblieben: Die
  Fehlerdarstellung bei ungültigem Cover-Upload (Inline vs. Modal) — vor
  Umsetzung von Block 6b gesondert zu klären.

### 6a. Record-Backend (CRUD ohne Cover, ohne Tracks)

Status: **Backend abgeschlossen** (2026-08-07)

Umgesetzt (Backend):

- Domain-Entität `Record` (`Domain/DomainModels/Sammlung/Record/`) nach den
  Domain-Regeln — erste Entität mit eigenen Enums (`RecordFormat`,
  `RecordCondition`) und erste Verwendung der neuen Kategorie `Sammlung`
  neben `Stammdaten` (Klärung 2026-08-07, siehe Wiki
  `architektur/application-layer.md`).
- Commands (Create, Update, Delete), Queries (GetById, GetPaged mit Filter
  nach Name/`artistId`/`labelId`/Erscheinungsjahr-Zeitraum/`countryId` und
  wählbarer Sortierung nach Name/Erscheinungsjahr/Format), Response-DTOs
  (`RecordResponse`, `RecordListResponse`) und `RecordResponseBuilder` unter
  `Application/Features/Sammlung/Record/`. `RecordResponse` enthält bewusst
  noch kein `Tracks`-Feld (kommt mit Block 6c).
- Neues Validierungsmuster: `labelId`/`artistId` werden per `MustAsync`
  mandantengefiltert geprüft (Existenz und Zugehörigkeit zum angemeldeten
  Benutzer in einem Schritt) — eine fremde oder nicht existente Id wird
  identisch behandelt (HTTP 400), siehe Wiki `user-stories-record.md`.
- `countryId`-Filter löst zunächst die passenden `labelId`s des Benutzers auf
  (`record.label_id → label.country_id`, Record hat kein eigenes Länderfeld).
- Minimal-API-Endpoints (`RecordEndpoints`, `/api/records`) ohne
  `/cover`-Endpoint (folgt in 6b) und ohne Track-Unterressourcen (folgt in
  6c); `sortBy`/`sortDirection`-Normalisierung analog zur bestehenden
  `page`/`pageSize`-Normalisierung.
- EF-Migration `CreateRecordTable`: legt die `record`-Tabelle sowie die
  beiden nativen PostgreSQL-Enum-Typen `record_format`/`record_condition`
  an (erste Verwendung nativer Postgres-Enums im Projekt), FK zu
  `label`/`artist` mit `ON DELETE RESTRICT`, bewusst **ohne**
  Unique-Constraint (Duplikate ausdrücklich erlaubt).
- `JsonStringEnumConverter` global in `MyMusic.Api/Program.cs` aktiviert,
  damit `Format`/`Condition` als lesbare Strings (z. B. `"CdAlbum"`, `"Vg"`)
  statt als Zahlen über die API laufen.
- Unit Tests: Domain (`RecordTests`, 20 Fälle inkl. Zeichensatz mit
  Klammern, Erscheinungsjahr-Bereich), Application (Handler, Validatoren
  inkl. neuer mandantengefilterter FK-Prüfung, `RecordResponseBuilder`,
  `GetPagedRecordsQueryHandler` inkl. Sortier-/Filter-Kombinationen — 48
  Fälle) — 68 neue Unit-Tests, alle grün.
- Integrationstest `RecordEndpointsTests` (CRUD-Fluss, alle Filter inkl.
  `countryId` über Label, alle Sortierfelder, Paginierung,
  Mandantentrennung, 400/404, Duplikate ausdrücklich erlaubt) nach Muster
  `LabelEndpointsTests`; grün (gemeinsamer Lauf mit den fünf bestehenden
  Integrationstests, 7/7 grün).

Nachträge nach Implementierung (zwei grundlegende, projektweit relevante
technische Korrekturen, gefunden über den vollständigen Integrationstestlauf):

- **`HasPostgresEnum<T>`-Parameterreihenfolge**: Die generische Überladung
  lautet `(schema, name, nameTranslator)`, nicht `(name, nameTranslator)` —
  ein einzelner positionaler String wird als Schema interpretiert. Führte
  zunächst dazu, dass `record_format`/`record_condition` als Schemas mit
  darin liegenden Typen `RecordFormat`/`RecordCondition` angelegt wurden,
  statt als Typen im Standard-Schema. Fix: `name:`/`nameTranslator:`
  explizit benannt übergeben.
- **`ManyServiceProvidersCreatedWarning`**: `NpgsqlDbContextOptionsBuilder
  .MapEnum(...)` fließt in die `DbContextOptions` ein, anhand derer EF Core
  seinen internen `ServiceProvider` cacht. Neue `INpgsqlNameTranslator`
  -Instanzen bei jedem `UseNpgsql(...)`-Aufruf (also je DbContext-
  Konstruktion, je Request) ließen EF Core nach mehr als zwanzig Requests
  **jeden** Endpunkt — auch die unveränderten Genre-/Label-/Artist-/
  Country-Endpunkte — mit HTTP 500 fehlschlagen, da die Konfiguration bei
  jeder DbContext-Instanziierung als „geändert" galt. Fix:
  Übersetzer-Instanzen als Singletons (`MyMusicNpgsqlOptionsConfigurator`)
  statt `new` je Aufruf. Nur über den vollständigen
  `RecordEndpointsTests`-Lauf mit vielen aufeinanderfolgenden Requests
  aufgefallen — bei kurzen manuellen Prüfungen unauffällig.
- `RecordConfiguration.cs`: `HasDefaultValueSql("'VG'::record_condition")`
  statt `HasDefaultValue(RecordCondition.Vg)`, da EF sonst den rohen
  Int-Wert ohne Enum-Cast als DB-Default schreibt (Postgres lehnt
  `DEFAULT 3` für eine Spalte vom Typ `record_condition` ab). Zusätzlich
  `HasSentinel((RecordCondition)(-1))`, da `RecordCondition.Mint` zufällig
  der CLR-Default (`0`) ist und ohne Sentinel als „nicht gesetzt"
  missverstanden würde.

Bewusst nicht Teil dieses Standes:

- Album-Cover (`AlbumCover` bleibt `null`, Upload folgt in 6b).
- Tracks (`RecordResponse` ohne `Tracks`-Feld, folgt in 6c).

Abnahmekriterium erfüllt:

- Records lassen sich anlegen, anzeigen, filtern, sortieren, bearbeiten und
  löschen (noch ohne Cover, noch ohne Tracks); fremde Benutzerdaten sind
  nicht sichtbar; `labelId`/`artistId` werden mandantengefiltert geprüft.

### 6b. Album-Cover-Upload

Status: **Backend abgeschlossen** (2026-08-07)
Arbeits-Prompt: `docs/prompts/2026-08-07-block-6b-album-cover-upload.md`

Vorab geklärt: Fehlerdarstellung bei ungültigem Format/zu großer Datei ist
**Modal**, nicht Inline — Ausnahme von der allgemeinen 400-Regel in
`fehler-und-ausnahmekonzept.md` (Datei-Uploads sind dort nicht als eigene
Kategorie geführt). Beide betroffenen Wiki-Seiten aktualisiert.

Umgesetzt (Backend):

- Domain-Methode `Record.SetAlbumCover(byte[])` — analog zu `Update(...)`,
  gibt neue Instanz zurück. Neue Konstante `MaxAlbumCoverSizeBytes` (5 MB)
  und statische Methode `DetectAlbumCoverContentType(byte[])` (Magic-Byte-
  Erkennung JPEG/PNG), von Domain-Guard **und** Application-Validator
  gemeinsam genutzt.
- Commands unter `Application/Features/Sammlung/Record/Commands/UploadCover/`
  (`UploadRecordCoverCommand`, Validator, Handler) nach dem Muster von
  `UpdateRecordCommand`: Load → Ownership-Check (404) →
  `SetAlbumCover(...)` → `Update`/`SaveChangesAsync` → Label-/Artist-Namen
  nachladen → `RecordResponseBuilder.Build(...)`.
- `RecordResponse` um `AlbumCoverDataUrl` (`string?`) erweitert;
  `RecordResponseBuilder` baut bei vorhandenem Cover eine vollständige
  Data-URL (`data:image/jpeg;base64,...` bzw. `image/png`) — erscheint
  automatisch in Card- **und** Detailansicht, da `Build(...)` in beiden
  Pfaden verwendet wird.
- Minimal-API-Endpoint `POST /api/records/{id}/cover` in der bestehenden
  `RecordEndpoints`-Gruppe, `[FromForm] IFormFile`-Bindung,
  `.DisableAntiforgery()` (seit .NET 8 für formularbindende Endpunkte
  erforderlich; unkritisch bei reiner JWT-Bearer-API ohne Cookies, siehe
  ADR `docs/adr/0008-kein-antiforgery-fuer-cover-upload.md`).
- Keine neue EF-Migration nötig — `album_cover bytea` existiert bereits seit
  der Block-6a-Migration `CreateRecordTable`.
- Unit Tests: Domain (`RecordTests`, `SetAlbumCover`/
  `DetectAlbumCoverContentType`, 8 neue Fälle), Application
  (`UploadRecordCoverCommandValidatorTests`,
  `UploadRecordCoverCommandHandlerTests`, `RecordResponseBuilderTests`
  erweitert) — alle grün.
- Integrationstest `RecordEndpointsTests` um den Cover-Upload-Fluss im
  bestehenden CRUD-Testfall ergänzt (401 ohne Token, 400 bei ungültigem
  Format, 400 bei zu großer Datei, 404 bei fremdem Record, 200 mit
  Data-URL, Persistenz über erneuten Abruf geprüft).

Bewusst nicht Teil dieses Standes:

- Angular-Anbindung (Upload-UI, Modal-Fehlerdarstellung) — folgt erst nach
  Block 0c.
- Thumbnail-/Resize-Verarbeitung, verschärftes `RequestSizeLimit`.

Abnahmekriterium erfüllt:

- Für einen eigenen Record kann ein Cover hochgeladen werden und erscheint
  danach in Card- und Detailansicht; fremde/unbekannte Records liefern 404,
  ungültiges Format/zu große Datei liefern 400.

### 6c. Track-Backend

Status: **Backend abgeschlossen** (2026-08-07), PR #32 nach `main` gemergt

Umgesetzt (Backend):

- Domain-Entität `RecordTrack` (`Domain/DomainModels/Sammlung/RecordTrack/`)
  nach den Domain-Regeln — strukturell an `Record`/`Label` angelehnt, mit
  `ArtistId`/`GenreId` als reinen FK-Werten ohne Navigation. Namensregeln wie
  im Wiki geklärt: `TrackName` 1–150 Zeichen mit demselben erweiterten
  Zeichenset wie `AlbumName` (inkl. Klammern), `RecordSide` 1–3 Zeichen nur
  Buchstaben/Ziffern mit Default `"0"`, `TrackNumber` als eigenständige
  Entität wie jede andere (siehe Architektur-Entscheidung unten).
- **Offene Annahme**, im Wiki nicht dokumentiert: `TrackNumber >= 1`
  angenommen (kein Mindestwert im Schema/Wiki festgelegt, nur
  `SMALLINT NOT NULL` und Eindeutigkeit) — sollte bei Gelegenheit mit dem
  Projektinhaber geklärt und im Wiki (`domain/record-track.md`) nachgetragen
  werden.
- Architektur-Entscheidung: `RecordTrack` erhält trotz der begrifflichen
  Nähe zum „Record-Aggregat" (siehe `glossar.md`) eine eigene
  `IRepository<RecordTrackEntity>`-Nutzung statt einer EF-Navigation auf
  `Record` — konsistent mit `repository-pattern.md` („Die fachlichen
  Entitäten von MyMusic bilden sich 1:1 auf die Datenbanktabellen ab").
- Commands (Create, Update, Delete) unter
  `Application/Features/Sammlung/RecordTrack/Commands/`. Create/Update
  liefern die eigene `RecordTrackResponse` (nicht den vollständigen
  `RecordResponse`) — analog zu Genre/Label/Artist, im Unterschied zum
  Cover-Upload (der eine Domain-Methode auf `Record` selbst aufruft).
  `artistId`/`genreId` werden per `MustAsync` mandantengefiltert geprüft
  (HTTP 400 bei ungültig/fremd, analog zur Record-FK-Prüfung);
  `recordId`+`recordSide`+`trackNumber` eindeutig (HTTP 409 bei Verstoß,
  Muster `GetPagedAsync`+`page:1,pageSize:1` wie bei Label-Namen). Der
  Zugriff auf einen fremden/nicht existierenden Record liefert HTTP 404
  (Ownership-Check im Handler, nicht im Validator).
- Minimal-API-Endpoints ergänzen die bestehende `RecordEndpoints`-Gruppe
  (keine eigene Endpoint-Datei, da Tracks laut `api-endpunkte.md` keine
  eigenständige Ressource sind): `POST /records/{id}/tracks`,
  `PUT /records/{id}/tracks/{trackId}`, `DELETE /records/{id}/tracks/{trackId}`.
- `RecordResponse`/`RecordResponseBuilder` um `Tracks`-Feld
  (`IReadOnlyList<RecordTrackResponse>`) erweitert. `GetRecordByIdQueryHandler`
  lädt alle Tracks eines Records (sortiert nach `RecordSide`/`TrackNumber`,
  Namen aufgelöst) und bettet sie ein. **Abweichung von der ursprünglichen
  Annahme im Arbeits-Prompt**: `UpdateRecordCommandHandler` und
  `UploadRecordCoverCommandHandler` laden die Tracks ebenfalls (statt einer
  leeren Liste) — eine leere Liste hätte nach jedem Bearbeiten/Cover-Upload
  eines Records mit vorhandenen Tracks fälschlich eine leere Tracklist
  zurückgegeben. Nur `CreateRecordCommandHandler` liefert bewusst eine leere
  Liste (ein neu angelegter Record hat noch keine Tracks).
- EF-Migration `CreateRecordTrackTable`: legt die `record_track`-Tabelle an
  (FK auf `record` mit `ON DELETE CASCADE`, FK auf `artist`/`genre` mit
  `ON DELETE RESTRICT`, zusammengesetzter Unique-Index auf `record_id`,
  `record_side`, `track_number`). Erzeugt mit EF-Tools 9.0.0 (älter als die
  Runtime 10.0.10) — die generierten Migrations-/Snapshot-Dateien wichen
  stellenweise vom Projektstil ab (Block-Namespace statt file-scoped,
  Zeilenlänge) und wurden manuell auf das bestehende Muster (siehe
  `CreateRecordTable`/`CreateLabelTable`) angeglichen.
- `DeleteRecordCommandHandler`: veralteten Kommentar entfernt, der auf das
  damals noch fehlende `record_track` verwies — das Löschen kaskadiert jetzt
  automatisch über die Datenbank (`ON DELETE CASCADE`), ohne Änderung am
  Handler selbst.
- Unit Tests: Domain (`RecordTrackTests`, 16 Fälle), Application (Handler/
  Validator/Builder für Create/Update/Delete, 33 Fälle, plus zwei neue
  Testfälle in den bestehenden `RecordResponseBuilderTests`/
  `GetRecordByIdQueryHandlerTests` für die Tracks-Einbettung) — 51 neue
  Unit-Tests, alle grün (Domain gesamt 114, Application gesamt 222).
- Integrationstest: neue Testmethode
  `RecordTrackEndpoints_CrudMandantentrennungUndEindeutigkeitspruefung` in
  `RecordEndpointsTests.cs` (keine eigene Datei) — vollständiger CRUD-Fluss,
  Mandantentrennung (400 bei fremdem/ungültigem `artistId`/`genreId`, 404 bei
  fremdem/nicht existierendem Record oder Track), Eindeutigkeitsprüfung
  (409), Einbettung in `GET /records/{id}` inkl. Sortierung, sowie expliziter
  Nachweis der `ON DELETE CASCADE`-Kaskadierung (Record mit Track löschen,
  erneuter Löschversuch des Tracks liefert 404 statt 204). Grün im
  gemeinsamen Lauf mit allen sieben bestehenden Integrationstests (8/8 grün).

Bewusst nicht Teil dieses Standes:

- Angular-Anbindung (Tracklist als Unteransicht der Record-Detailansicht,
  Add/Edit/Delete) — folgt erst nach Block 0c.
- Referenzprüfungen aus Block 6d (`DeleteGenreCommandHandler` gegen
  `record_track`, `DeleteArtistCommandHandler` gegen `record`/`record_track`)
  bleiben offen — siehe Abschnitt 6d.

Abnahmekriterium erfüllt:

- Tracks lassen sich zu einem eigenen Record hinzufügen, bearbeiten und
  löschen; Mandantentrennung und Eindeutigkeitsprüfung greifen.

### 6d. Nachträge aus Block 2, 4 und 5

Status: **abgeschlossen** (2026-08-08)
Arbeits-Prompt: `docs/prompts/2026-08-07-block-6d-nachtraege.md`

Umgesetzt:

- `DeleteGenreCommandHandler`
  (`Application/Features/Stammdaten/Genre/Commands/Delete/`) um die in
  US-G5 beschriebene Referenzprüfung gegen `record_track` ergänzt (HTTP 409,
  wenn noch mindestens ein Track das Genre referenziert). Im Genre-Slice
  bewusst ausgelassen, da `record_track` dort noch nicht existierte — siehe
  `docs/prompts/2026-08-04-block-2-genre.md` und Wiki
  `user-stories/user-stories-genre.md` (US-G5).
- `DeleteLabelCommandHandler`
  (`Application/Features/Stammdaten/Label/Commands/Delete/`) um die in
  US-L5 beschriebene Referenzprüfung gegen `record` ergänzt (HTTP 409, wenn
  noch mindestens ein Record das Label referenziert). Im Label-Slice bewusst
  ausgelassen, da `record` dort noch nicht existierte — siehe
  `docs/prompts/2026-08-07-block-4-label.md` und Wiki
  `user-stories/user-stories-label.md` (US-L5).
- `DeleteArtistCommandHandler`
  (`Application/Features/Stammdaten/Artist/Commands/Delete/`) um die in
  US-A5 beschriebene Referenzprüfung gegen `record` **und** `record_track`
  ergänzt (HTTP 409, wenn noch mindestens ein Record oder Track den Artist
  referenziert — zwei getrennte, nacheinander ausgeführte Existenzabfragen
  mit Kurzschluss). Im Artist-Slice bewusst ausgelassen, da
  `record`/`record_track` dort noch nicht existierten — siehe
  `docs/prompts/2026-08-07-block-5-artist.md` und Wiki
  `user-stories/user-stories-artist.md` (US-A5).
- `GetPagedArtistsQuery`/`GetPagedArtistsQueryHandler`/`ArtistEndpoints`
  (`Application/Features/Stammdaten/Artist/Queries/GetPaged/`,
  `Api/Endpoints/Stammdaten/Artist/`) um einen `labelId`-Filter ergänzt
  (siehe US-A2), nach dem Muster von `GetPagedRecordsQueryHandler.
  ResolveLabelIdsForCountryAsync` (invertiert: löst über `record.label_id`
  die passenden `record.artist_id`s auf). Im Artist-Slice bewusst
  ausgelassen, da die Beziehung zu Label nur über die mit Block 6a
  entstandene `record`-Tabelle geprüft werden kann — siehe
  `docs/prompts/2026-08-07-block-5-artist.md` und Wiki
  `user-stories/user-stories-artist.md` (US-A2). Bewusst keine
  Existenz-/Mandantenprüfung der `labelId` (Analogie zu `countryId` bei
  `GET /records`): fremde/unbekannte `labelId` liefert eine leere Liste,
  kein HTTP 400.
- Unit Tests: neue Conflict-Testfälle in den drei bestehenden
  Delete-Handler-Testdateien (Genre 1, Label 1, Artist 2 — inkl. Nachweis
  des Kurzschlusses per `DidNotReceive()`), zwei neue Testfälle in
  `GetPagedArtistsQueryHandlerTests.cs` für den `labelId`-Filter.
- Integrationstest: neuer Fact
  `ReferenzielleIntegritaet_VerhindertLoeschenVonGenreLabelUndArtistBeiVerwendung`
  in `RecordEndpointsTests.cs` (409 bei Verwendung, 204 nach Entfernen der
  Referenzen); `labelId`-Filter als zusätzliche Schritte im bestehenden
  `ArtistEndpoints_CrudPaginierungUndMandantentrennung`-Test.

Abnahmekriterium erfüllt:

- `DeleteGenreCommandHandler`, `DeleteLabelCommandHandler` und
  `DeleteArtistCommandHandler` verhindern das Löschen real referenzierter
  Datensätze (HTTP 409); `GET /artists` unterstützt den `labelId`-Filter.

### 6e. Nachtrag: Unpaginierte GetAll-Endpunkte für Genre, Label, Artist

Status: **abgeschlossen** (2026-08-14)

Anlass: Beim Vorbereiten der Angular-Planung für den Records-Frontend-Slice
(Fortsetzung von Abschnitt 6) fiel auf, dass `RecordForm` eine vollständige
Auswahlliste für Label (Pflicht) und Artist (optional) braucht, die
RecordTrack-Formulare zusätzlich für Artist und Genre. Anders als bei
[[country]] (`GET /countries`, unpaginiert) gab es für Genre, Label und
Artist bislang nur den paginierten `GetPaged`-Endpunkt mit hartem
`pageSize`-Limit von 100 — für Dropdown-/Referenzzwecke fachlich falsch,
vom Projektinhaber als Fehler benannt und als Voraussetzung für die
Fortsetzung der Records-Frontend-Planung verlangt.

Umgesetzt:

- Je Entität ein neues `GetAll{Entität}Query`/`-QueryHandler`-Paar
  (`Application/Features/Stammdaten/{Genre,Label,Artist}/Queries/GetAll/`),
  analog zu `GetAllCountriesQuery`, aber mit `userId`-Filterung (Country ist
  laut `wiki/architektur/cqrs-framework.md` die einzige Ausnahme ohne
  Mandantenbezug). Implementiert über das bereits etablierte Muster
  `IRepository<T>.GetPagedAsync(filter, orderBy, page: 1, pageSize:
  int.MaxValue, ct)` (siehe `GetPagedRecordsQueryHandler
  .ResolveLabelIdsForCountryAsync`) — kein Repository-Änderung nötig, kein
  Risiko einer mandantenübergreifenden Datenpanne durch das ungefilterte
  `GetAllAsync()`.
- Neue Endpunkte `GET /api/genres/all`, `GET /api/labels/all`,
  `GET /api/artists/all` (Muster: `CountryEndpoints.GetAllCountriesAsync`,
  zusätzlich mit `ICurrentUserService`), Registrierung in `GenreEndpoints`,
  `LabelEndpoints`, `ArtistEndpoints`; `GlobalUsing.cs` (Api und
  Application.Tests) um die drei neuen `Queries.GetAll`-Namespaces ergänzt.
- Unit Tests je Entität (Mapping, leere Liste, Mandantentrennung über
  kompilierte Filter-Expression) — 8 neue Tests, alle grün (Application
  gesamt 236).
- Bestehende Integrationstests (`GenreEndpointsTests`, `LabelEndpointsTests`,
  `ArtistEndpointsTests`) um Prüfungen für die neuen `/all`-Endpunkte
  erweitert (401 ohne Token, 200 mit vollständiger eigener Liste, sichtbare
  Mandantentrennung) — keine zusätzlichen, kostspieligen Aspire-Testläufe,
  sondern Erweiterung der bestehenden CRUD-Testfälle.
- Wiki `architektur/api-endpunkte.md` um die drei neuen Zeilen und eine
  Klärungsnotiz (2026-08-14) ergänzt; `wiki/log.md` aktualisiert.

Bewusst nicht Teil dieses Nachtrags:

- Keine Änderung an den bestehenden `GetPaged`-Endpunkten oder ihrem
  100er-Limit — die Tabellenansichten (Genres/Labels/Artists) bleiben
  paginiert.
- Keine Frontend-Anbindung (`genre.service.ts`/`label.service.ts`/
  `artist.service.ts` bekommen noch keine `getAll()`-Methode) — die neuen
  Endpunkte werden erst konsumiert, wenn die Records-Frontend-Planung
  fortgesetzt wird.
- Keine Nachrüstung der im Artist-Block (Abschnitt 5) bewusst ausgelassenen
  `labelId`-Filter-UI bei `GET /artists` — bleibt ein separater,
  nicht angeforderter Punkt, ist mit `GET /labels/all` jetzt aber ohne
  Weiteres nachrüstbar.

Abnahmekriterium erfüllt:

- `GET /api/genres/all`, `GET /api/labels/all`, `GET /api/artists/all`
  liefern ohne Token 401, mit Token die vollständige, alphabetisch
  sortierte und mandantengefilterte Liste des angemeldeten Benutzers, ohne
  Cap bei mehr als 100 Einträgen.

### 6f. Record-Liste (Card-Ansicht, Filter, Sortierung, Paginierung)

Status: **abgeschlossen** (2026-08-14)
Arbeits-Prompt: `docs/prompts/2026-08-14-block-6f-angular-records-liste.md`

Anlass: Erster von fünf Teilblöcken, in die das Angular-Frontend für
`records/` auf Wunsch des Projektinhabers zerlegt wurde (Records ist laut
Wiki „der fachlich umfangreichste Slice"). Dieser Block deckt US-R1–R3 ab
(Card-Ansicht, Filter, Sortierung, Paginierung) — ohne Anlegen/Bearbeiten/
Löschen, ohne Detailseite, ohne Cover-Upload, ohne Tracks.

Bei der Planung stellte sich heraus, dass die im Wiki
(`architektur/ui-ux-konzept.md`) nur namentlich erwähnte Formatfilterung
("Format-Umschalter") weder Werte noch Backend-Verhalten festlegte und das
Backend keinen Format-Filter kannte. Ein erster Entwurf (Gruppierung
Alle/Vinyl/CD) wurde vom Projektinhaber verworfen, da diese Gruppierung im
Datenmodell nicht existiert. Geklärt: Der Filter arbeitet direkt und exakt
auf dem `format`-Feld (den zehn `RecordFormat`-Werten), als natives
Dropdown-Filterfeld — dieselbe Bauart wie `artistId`/`labelId`/`countryId`.

Umgesetzt (Backend, kleine, eng begrenzte Ausnahme von der Regel
„Frontend-Blöcke fassen keine Backend-Änderungen an", da Record-Backend mit
6a–6e als abgeschlossen markiert war):

- `GetPagedRecordsQuery`/`GetPagedRecordsQueryHandler`
  (`Application/Features/Sammlung/Record/Queries/GetPaged/`) um einen
  `Format`-Parameter erweitert — Gleichheitsfilter auf `RecordFormat`, roher
  Query-String-Wert wird per `Enum.TryParse(..., ignoreCase: true)` im
  Handler interpretiert (analog zum bestehenden `sortBy`/`sortDirection`-
  Muster). Bewusst kein neuer FluentValidation-Validator: Das
  CQRS-Framework validiert grundsätzlich nur Commands, nie Queries
  (`Mediator.cs`) — ein unbekannter/nicht parsebarer `format`-Wert wirkt wie
  „kein Filter", liefert kein HTTP 400 (konsistent mit dem bestehenden
  Verhalten aller anderen GET-Filter des Endpunkts).
- `RecordEndpoints.GetPagedRecordsAsync` um den Query-Parameter `format`
  erweitert.
- Unit Tests (`GetPagedRecordsQueryHandlerTests`, 4 neue Fälle: Filter
  case-insensitive wirksam in drei Schreibweisen, unbekannter Wert wirkt
  wie kein Filter) und Integrationstest (`RecordEndpointsTests`, neue
  Prüfungen für `format=cdalbum` und einen unbekannten Format-Wert ohne
  400) — 240 Application-Unit-Tests insgesamt, alle grün.

Umgesetzt (Frontend):

- `shared/country/` (neu): `country.ts`/`country.service.ts` von
  `features/labels/` hierher verschoben, da Records jetzt ein zweiter
  Konsument neben Label ist (Import-Pfade in `features/labels/*`
  angepasst, keine Verhaltensänderung).
- `shared/autocomplete/` (neu): generische Freitext-Autosuggest-Komponente
  (Debounce 300 ms, serverseitige Vorschläge über `input()`, Tastatur-
  Navigation Pfeiltasten/Enter/Escape, Klick-Auswahl per `mousedown` mit
  `preventDefault()` gegen die Blur-Race-Condition) — siehe Korrektur
  unten.
- `features/records/record.ts` (Interfaces `Record`/`RecordListResponse`,
  Union-Types `RecordFormat`/`RecordCondition` mit den exakten
  Wire-Strings der `JsonStringEnumConverter`-Serialisierung, Anzeige-
  Konstanten für Format-Bezeichnungen (`glossar.md`), Vinyl/CD-Zuordnung
  für die Card-Pille, Zustands-Bezeichnungen und Grade-Badge-Klassen
  (`zustandsbewertung.md`)), `record.service.ts` (`getPaged(...)` mit allen
  Filtern inkl. `format`).
- `features/records/record-filter/` (Signal Form für Name mit Debounce,
  Land/Format/Sortierfeld als native `<select>`, Jahr-von/-bis als
  Zahlenfelder, separater Auf/Ab-Umschalter-Button mit `title`-Tooltip für
  die Sortierrichtung; Artist/Label über `app-autocomplete`, siehe unten).
- `features/records/record-card/` (Card-Darstellung nach
  `komponenten-klassen.md`: Cover oder Platzhalter-Icon, Format-Pille
  LP/CD, Albumname, Künstler falls vorhanden, Jahr · Label, Grade-Badge).
- `features/records/records.ts`/`.html` ersetzt die bisherige
  Platzhalterseite vollständig: `rxResource` für Records paginiert,
  Countries einmalig (Dropdown) sowie Artist-/Label-Vorschläge
  query-gesteuert (debounced, kein Volllisten-Ladevorgang), Ladezustand/
  Empty-State/Card-Grid/Paginierung nach dem Muster der Tabellen-Slices,
  `ErrorModalService`-Anbindung für Records- und Country-Resource
  (Fehler bei den Autosuggest-Anfragen bewusst nicht modal, sondern
  stille leere Trefferliste — ein Tippfehler-bedingter 0-Treffer-Zustand
  soll den Nutzer nicht unterbrechen).
- 47 neue Frontend-Tests gegenüber dem Stand nach Block 5 (182 → 229),
  alle grün. Production-Build grün.

**Korrektur während der Umsetzung (Live-Test durch den Projektinhaber)**:
Der ursprüngliche Entwurf band Artist/Label als natives `<select>` ein,
befüllt über neu ergänzte `LabelService.getAll()`/`ArtistService.getAll()`
gegen die mit Block 6e geschaffenen `/all`-Endpunkte. Bei der Live-Prüfung
im Browser bemängelt: Diese Listen können hunderte oder tausende
Datensätze umfassen — ein Dropdown ist dafür nicht bedienbar. Ersetzt durch
das oben beschriebene serverseitige Freitext-Autosuggest (`shared/
autocomplete/`) gegen den bereits bestehenden `getPaged`-Endpoint mit
kleinem `pageSize` — kein neuer Backend-Endpunkt nötig. Die zunächst
ergänzten `getAll()`-Methoden auf `LabelService`/`ArtistService` wurden
wieder entfernt, da nach der Korrektur ungenutzt (die Dateien sind dadurch
wieder byte-identisch mit dem Stand vor Block 6f). Bei derselben
Gelegenheit zusätzlich behoben: Die Filter-Dropdowns (Land/Format/
Sortierung) waren zunächst ohne Breitenbegrenzung und dadurch volle
Zeilenbreite (`.select`/`.input` sind im Design System auf `width: 100%`
gesetzt) — jetzt mit `max-w-[...]`-Klassen versehen; der Sortierrichtungs-
Button hatte keinen sichtbaren Tooltip — jetzt zusätzlich zum
`aria-label` ein natives `title`-Attribut.

**Entdeckte und korrigierte Wiki-Abweichung**: `wiki/design/komponenten-klassen.md`
dokumentierte für die Zustandsstufe „Good Plus" die CSS-Klasse
`.grade-gplus` — diese Klasse existiert in
`src/frontend/src/styles/design-system/components.css` nicht. Die
tatsächlich vorhandene, farblich passende Klasse heißt `.grade-f`. Die
Anwendung nutzt die reale Klasse `.grade-f`; die Wiki-Tabelle wurde
entsprechend korrigiert (siehe Wiki `log.md`).

Bewusst nicht Teil dieses Standes:

- Anlegen/Bearbeiten/Löschen (US-R4–R6, Block 6g).
- Detailseite `/records/:id` (US-R7, Block 6h) — `RecordCard` hat bereits
  ein `opened`-Output, aber noch keine Navigation ist verdrahtet.
- Album-Cover-Upload (US-R8, Block 6i).
- Tracks (US-T1–T3, Block 6j).

Abnahmekriterium:

- Records lassen sich als Cards anzeigen, nach Name/Artist/Label/Jahr-
  Zeitraum/Land/Format filtern (einzeln oder kombiniert), nach Name/
  Erscheinungsjahr/Format auf- oder absteigend sortieren und sind
  paginiert; Empty- und Loading-State korrekt. **Vollständig erfüllt** —
  automatisierte Tests grün und zusätzlich live im Browser gegen den
  laufenden Aspire-AppHost verifiziert (Filter-Zeile, Artist-Autosuggest
  gegen echtes Backend, Tooltip, keine Konsolenfehler).
  (Korrigiert am 2026-08-30: Die Card-Liste lädt nicht mehr seitenweise
  nummeriert, sondern per Infinite Scroll automatisch nach — Grund: Die
  Seiten-Buttons-Zeile verstieß bei größeren Sammlungen gegen die
  Responsiveness-Anforderung „vollständig nutzbar", siehe
  docs/prompts/2026-08-30-fix-records-suche-infinite-scroll.md.)

### 6g. Record anlegen/bearbeiten/löschen

Status: **abgeschlossen** (2026-08-14), PR #57 nach `main` gemergt
Arbeits-Prompt: `docs/prompts/2026-08-14-block-6g-angular-records-crud.md`

Anlass: Zweiter der fünf Teilblöcke 6f–6j (siehe Abschnitt 6f). Deckt
US-R4–R6 (Anlegen, Bearbeiten, Löschen) ab — ohne Detailseite, ohne
Cover-Upload, ohne Tracks. Reiner Frontend-Block: Das Backend
(`POST`/`PUT`/`DELETE /api/records/{id}`) war seit Block 6a bereits
vollständig vorhanden, keine Backend-Änderung nötig.

Zwei Design-Lücken vorab mit dem Projektinhaber geklärt (2026-08-14):

- **Edit/Delete-Trigger**: Die Card-Ansicht hat anders als die
  Tabellen-Slices keine Aktionsspalte. Entschieden: Icon-Buttons
  (Stift/Papierkorb) direkt auf der `RecordCard`, analog zur
  Aktionsspalte der Tabellen-Slices.
- **Autocomplete-Vorbefüllung**: Label/Artist im Formular nutzen dieselbe
  `app-autocomplete`-Komponente wie im Filter (Block 6f), statt eines
  nativen `<select>` (Listen können beliebig groß werden). Entschieden:
  `shared/autocomplete/` um ein optionales Prefill-Input erweitern statt
  den Namen separat als Text anzuzeigen oder auf ein natives Dropdown
  auszuweichen.

Umgesetzt:

- `shared/autocomplete/autocomplete.ts`: neues optionales Input
  `initialQuery`, `queryText` von `signal('')` auf
  `linkedSignal(() => this.initialQuery())` umgestellt — vorbefüllt das
  Feld im Bearbeiten-Modus mit dem bisherigen Namen, ohne das bestehende
  Verhalten im Filter (kein `initialQuery` gesetzt) zu ändern. Zwei neue
  Testfälle in `autocomplete.spec.ts`.
- `features/records/record.ts`/`record.service.ts`: `CreateRecordRequest`/
  `UpdateRecordRequest` sowie `create()`/`update()`/`delete()` ergänzt,
  1:1 nach dem Muster aus `LabelService`.
- `features/records/record-form/` (neu): Signal-Form-Komponente analog
  `LabelForm` — Label (Pflicht, Autocomplete), Künstler (optional,
  Autocomplete), Format (Pflicht, natives `<select>`), Albumname (Pflicht,
  1–150 Zeichen, exaktes Backend-Pattern aus `Record.cs` inkl. Klammern),
  Erscheinungsjahr (Pflicht, 1860 bis aktuelles Jahr über
  `validate()`-Bereichsprüfung, da das Formularmodell wie im gesamten
  Projekt durchgängig String-Felder verwendet — `min`/`max` aus
  `@angular/forms/signals` hätte einen abweichenden, numerisch typisierten
  Formularpfad erfordert), Zustand (natives `<select>`, Default `Vg`),
  Information (optional, max. 255 Zeichen). Serverseitige 400-Fehler
  feldweise zugeordnet wie bei `LabelForm`. Kein „Discogs-Suche"-Button —
  das verschachtelte Discogs-Modal aus `ui-ux-konzept.md` gehört zu
  Block 8 (weiterhin offen).
- `features/records/record-card/`: neue Outputs `editRequested`/
  `deleteRequested`, zwei Icon-Buttons (`btn btn-ghost btn-icon btn-sm`,
  `LucidePencil`/`LucideTrash2`) mit `stopPropagation()`, damit das
  bestehende `opened`-Output nicht mitausgelöst wird. Platzierung oben
  links im Cover (die `.fmt`-Formatpille belegt oben rechts), mit
  halbtransparentem, dunklem Hintergrund (`bg-black/40`, `!text-white`)
  für Lesbarkeit über beliebigen Cover-Bildern — rein im Template gelöst,
  keine Änderung an `components.css` (Design-System bleibt 1:1-Kopie aus
  dem Wiki).
- `features/records/records.ts`/`records.html`: Formular-Modal,
  `ConfirmModal` für Löschen, „+ Anlegen"-Button in der Toolbar — 1:1 nach
  dem Muster aus `Labels`/`labels.html`.
- Neue/erweiterte Tests: `record-form.spec.ts` (neu, 16 Fälle),
  `record-card.spec.ts`, `record.service.spec.ts`, `records.spec.ts`
  (Anlegen/Bearbeiten inkl. Vorbefüllung/Löschen inkl. Bestätigen/
  Abbrechen/Serverfehler) — 257 Frontend-Tests insgesamt, alle grün.
  Production-Build und Prettier-Check grün.

Bewusst nicht Teil dieses Standes:

- Detailseite `/records/:id` (US-R7, Block 6h).
- Album-Cover-Upload (US-R8, Block 6i).
- Tracks (US-T1–T3, Block 6j).
- Discogs-Integration/-Modal (Block 8).

Abnahmekriterium:

- Ein Record lässt sich anlegen, bearbeiten (inkl. korrekt vorbefülltem
  Label/Künstler) und mit Bestätigungsdialog löschen; Validierungsfehler
  erscheinen inline am jeweiligen Feld. **Vollständig erfüllt** —
  automatisierte Tests grün und zusätzlich live im Browser gegen den
  laufenden Aspire-AppHost verifiziert (Anlegen mit Autosuggest gegen
  echtes Backend, Bearbeiten mit vorbefülltem Label-Feld, Pflichtfeld- und
  Jahresbereichs-Validierung inline, Löschen mit Bestätigungsdialog, keine
  Konsolenfehler).

Nachtrag (2026-08-14): Label/Artist direkt aus dem Record-Formular anlegen.
Beim Live-Test fiel auf: Wählte man ein noch nicht existierendes Label oder
einen noch nicht existierenden Artist, blieb das Feld ungültig — der
Benutzer hätte die Ansicht wechseln müssen. Mit dem Projektinhaber geklärt,
unterschiedlich je Entität (Artist hat nur ein Pflichtfeld, Label zwingend
auch `countryId`):

- **Artist**: Verlässt der Benutzer das Künstler-Feld (Blur) mit einem
  gültigen, aber unbekannten Namen (Regeln aus `Artist.cs`: 3–120 Zeichen,
  Pattern `^[\p{L}\p{N} \-&'./]+$`), fragt ein `ConfirmModal` „Soll der
  Künstler '…' neu angelegt werden?". Bestätigen legt den Artist mit nur
  dem Namen an und übernimmt ihn; Ablehnen leert das Feld.
- **Label**: Icon-Button (`LucidePlus`, `title` **und** `aria-label`
  „Neues Label anlegen") neben dem Feld öffnet das bestehende `LabelForm`
  als zweites, verschachteltes Modal (ohne `[label]`-Input, also im
  Anlegen-Modus); nach dem Speichern wird die neue Auswahl automatisch
  übernommen.

Dafür notwendige Erweiterungen an gemeinsam genutzten Bausteinen:

- `shared/modal/modal.ts`: `@HostListener('document:keydown.escape')`
  wirkte bisher global auf jede offene `Modal`-Instanz — bei zwei
  gleichzeitig offenen Modals (verschachteltes `LabelForm` bzw.
  `ConfirmModal` über dem `RecordForm`) hätte Escape beide gleichzeitig
  geschlossen. Fix: modulweiter Stack (`openModalStack`), nur die oberste
  Instanz reagiert auf Escape. Neuer Test in `modal.spec.ts`.
- `shared/autocomplete/autocomplete.ts`: neuer Output `blur = output<string>()`
  und neue öffentliche Methode `setQuery(value: string)` zum
  programmatischen Setzen des Anzeigetexts von außen (Leeren nach
  Ablehnen, Setzen nach Quick-Create) — das bestehende
  `initialQuery`/`linkedSignal`-Muster reicht dafür nicht: Ein erneutes
  Setzen auf denselben vorherigen Wert (z. B. wieder `''`) wird von
  Angular nicht als Signaländerung erkannt und hätte den zuvor getippten,
  abgelehnten Text stehen lassen. Neue Tests in `autocomplete.spec.ts`.
- `shared/confirm-modal/confirm-modal.ts`: Der Bestätigen-Button war fest
  auf Text „Löschen" und `.btn-danger` verdrahtet — für eine
  Anlegen-Rückfrage inhaltlich falsch (rot = destruktiv). Neue optionale
  Inputs `confirmLabel` (Default `'Löschen'`) und `confirmVariant`
  (`'danger' | 'primary'`, Default `'danger'`) ergänzt, bestehende
  Aufrufer unverändert kompatibel. Neuer Test in `confirm-modal.spec.ts`.
- `features/labels/label-form/label-form.ts`: `saved` von `output<void>()`
  auf `output<Label>()` umgestellt, damit der Aufrufer weiß, welches Label
  entstanden ist — bisher wurde nach dem Anlegen gar kein Ergebnis
  zurückgegeben. Bestehender Aufrufer `Labels` bleibt kompatibel (Angular
  ignoriert den zusätzlichen Payload-Parameter). Tests in
  `label-form.spec.ts` ergänzt.
- `features/records/record-form/`: neuer `countriesResource` (für das
  verschachtelte `LabelForm`), `viewChild()`-Referenzen auf beide
  Autocomplete-Felder, Handler für Label-Button/verschachteltes Modal und
  Artist-Blur-Rückfrage. 8 neue Testfälle in `record-form.spec.ts`;
  `records.spec.ts` um den zusätzlichen `/api/countries`-Request beim
  Öffnen des Formulars ergänzt (eigene Ressource von `RecordForm`, nicht
  die von `Records`).
- 271 Frontend-Tests insgesamt, alle grün. Production-Build und
  Prettier-Check grün.

Live verifiziert: Tooltip am Label-Button (native `title`-Anzeige — im
automatisierten Browser-Screenshot nicht sichtbar, vom Projektinhaber
selbst am echten Cursor bestätigt), verschachteltes Label-Formular inkl.
Übernahme der neuen Auswahl, Escape schließt bei zwei offenen Modals nur
das obere (verschachteltes Label-Formular schließt, Record-Formular bleibt
offen), Artist-Rückfrage mit grünem „Anlegen"-Button (nicht „Löschen"),
Bestätigen legt an und übernimmt, keine Konsolenfehler.

### 6h. Record-Detailansicht

Status: **abgeschlossen** (2026-08-15), PR #59 nach `main` gemergt
Arbeits-Prompt: `docs/prompts/2026-08-15-block-6h-angular-records-detail.md`

Anlass: Dritter der fünf Teilblöcke 6f–6j (siehe Abschnitt 6f). Deckt US-R7
(Detailansicht mit Tracklist) ab — ohne Cover-Upload, ohne Track-CRUD.
Reiner Frontend-Block: `GET /api/records/{id}` war seit Block 6a bereits
vollständig vorhanden (inkl. aufgelöster Tracks und Cover als Base64-Data-
URL), keine Backend-Änderung nötig.

Design-Klärung mit dem Projektinhaber während der Umsetzung: `wiki/design/
ui-kit.md` und der zugehörige Design-Prototyp-Screenshot zeigen die
Detailansicht als **Modal** über dem weiterhin sichtbaren Records-Grid,
nicht als eigene Vollbild-Seite. Umgesetzt als Kind-Route von `/records`
(`{ path: '', component: Records, children: [{ path: ':id', component:
RecordDetail }] }`), damit `Records` gemountet bleibt und `RecordDetail`
per `<router-outlet>` als Modal darüber rendert — URL bleibt dabei
verlinkbar (`/records/:id`), passend zur Zurück-Link-Vorgabe aus
`ui-ux-konzept.md`. Zusätzlich klargestellt: Bearbeiten und Löschen sind im
Detail-Modal **nicht** verfügbar (reiner Lesemodus) — beides bleibt
ausschließlich über die Icons auf der `RecordCard` in der Liste erreichbar
(Block 6g); der Design-Prototyp zeigte dafür ursprünglich Footer-Buttons im
Modal, das wurde bewusst nicht übernommen.

Umgesetzt:

- `features/records/record.service.ts`: `getById(id)` ergänzt.
- `features/records/records.routes.ts`: `:id` als Kind-Route von `''`.
- `features/records/records.ts`/`records.html`: `Router` injiziert,
  `record-card`s bisher ungenutztes `opened`-Output an
  `router.navigate(['/records', record.id])` gebunden; `<router-outlet>`
  ergänzt.
- `features/records/record-detail/` (neu): Modal-Komponente
  (`shared/modal/modal.ts`), lädt per `getById` (das Listenobjekt hat laut
  `RecordResponseBuilder.BuildPaged` nie echte Tracks, daher zwingend
  Neuladen), Routenparameter über `ActivatedRoute` + `toSignal(
  route.paramMap...)` (Projekt nutzt kein `withComponentInputBinding()`,
  analog zu `features/search/search.ts`). Zeigt Cover, Albumname, Künstler,
  Format-Badge, Grade-Badge, Jahr, Label, `information`, Tracklist.
  Fehlerbehandlung wie in `genres.ts` (`ErrorModalService`); dabei
  festgestellt: „Erneut versuchen" ist im bestehenden
  `ErrorModalService`/`ErrorModal` nur für `kind: 'network'` verdrahtet,
  nicht für `kind: 'server'` (HTTP 500) — bestehendes Verhalten, keine
  Änderung für diesen Block.
- `features/records/track-list/` (neu): reine Anzeige-Komponente, gruppiert
  Tracks nach `recordSide` mit Überschrift „Seite {{side}}" (keine
  Überschrift bei ausschließlich `recordSide = '0'`, also CD); Backend
  liefert bereits sortiert (Seite, dann Tracknummer).
- **Darstellungsfehler während der Live-Verifikation gefunden und
  behoben**: Öffnete man aus dem Detail-Modal heraus das Bearbeiten-
  Formular (vor der obigen Klarstellung, dass das gar nicht vorgesehen
  ist), lag es hinter statt vor dem Detail-Modal (DOM-Reihenfolge in
  `records.html` — `<router-outlet>` stand vor `@if (formOpen())`).
  Zwischenzeitlich durch Umsortieren behoben, mit der Entfernung der
  Bearbeiten/Löschen-Aktion aus dem Detail-Modal insgesamt hinfällig
  geworden.
- 288 Frontend-Tests insgesamt, alle grün. Production-Build und
  Prettier-Check grün (Prettier meldet projektweit, auch für unveränderte
  Bestandsdateien, Formatierungsabweichungen — Ursache ist `core.autocrlf
  =true` unter Windows (CRLF-Checkout vs. Prettier-Default `lf`), kein
  durch diesen Block verursachtes Problem; das CI-Gate läuft auf Linux und
  ist davon nicht betroffen).

Nachtrag (2026-08-15, vor dem Merge im selben PR behoben): Der Projektinhaber
fand beim Live-Test einen Anzeigefehler in `track-list.html` — je Track
erschien „Künstler · Genre" statt „Künstler · Trackname". Der Design-
Prototyp-Screenshot zeigt pro Track nur Nummer, Trackname und Dauer, kein
Genre; die Genre-Anzeige war eine eigene, nicht aus dem Screenshot
abgeleitete Ergänzung. Korrigiert auf „Künstler · Trackname" je Zeile,
Genre entfällt aus der Tracklist-Darstellung. Dabei zusätzlich ein
Dokumentationsfehler richtiggestellt: `Record` hat kein eigenes Genre-Feld
(nur die zugehörigen Tracks haben eins) — ein an mehreren Stellen
fälschlich erwähntes „Genre-Badge" auf Record-Ebene existiert im Code
nicht und wurde aus der Doku entfernt. Neuer Testfall in
`track-list.spec.ts` stellt sicher, dass kein Genre mehr angezeigt wird.
289 Frontend-Tests insgesamt, alle grün.

Bewusst nicht Teil dieses Standes:

- Album-Cover-Upload (US-R8, Block 6i).
- Tracks hinzufügen/bearbeiten/löschen (US-T1–T3, Block 6j) — Tracklist ist
  rein lesend.
- Bearbeiten/Löschen im Detail-Modal (siehe Design-Klärung oben) — bleibt
  Aufgabe der Liste (Block 6g).

Abnahmekriterium:

- Klick auf eine Record-Card öffnet die Detailansicht mit Tracklist; nur
  eigene Records sind aufrufbar, bei fremder/unbekannter Id erscheint das
  „Record nicht gefunden"-Modal; kein Breadcrumb, expliziter Zurück-Weg
  (Schließen-Button/Escape/Klick außerhalb) genügt. **Vollständig erfüllt**
  — automatisierte Tests grün und zusätzlich live im Browser gegen den
  laufenden Aspire-AppHost verifiziert (Klick auf Card öffnet Modal über
  dem Grid, URL wechselt zu `/records/:id`, Cover-Platzhalter, Stammdaten,
  Grade-Badge, leere Tracklist mit Hinweistext, Schließen per ×- und
  Scrim-Klick führt zurück zu `/records`, kein Bearbeiten/Löschen im Modal
  und Klick auf die dahinterliegenden Card-Icons löst währenddessen nichts
  aus, keine Konsolenfehler).

### 6i. Album-Cover-Upload

Status: **abgeschlossen** (2026-08-15), automatisiert und live verifiziert,
PR #61 nach `main` gemergt.
Branch: `block-6i-angular-records-cover-upload`
Arbeits-Prompt: `docs/prompts/2026-08-15-block-6i-angular-records-cover-upload.md`

Anlass: Vierter der fünf Teilblöcke 6f–6j (siehe Abschnitt 6f). Deckt US-R8
(Album-Cover hochladen) ab. `POST /api/records/{id}/cover` war seit Block 6b
bereits vollständig vorhanden — reiner Frontend-Block, **keine
Backend-Änderung**.

Design-Klärung mit dem Projektinhaber während der Planung (weicht von der
wörtlichen US-R8-Formulierung „unabhängig vom Anlegen/Bearbeiten des
Records" ab, daher hier festgehalten statt stillschweigend entschieden):
Der Upload-Trigger sitzt im `RecordForm`-Modal, sowohl beim Anlegen als
auch beim Bearbeiten — nicht im Detail-Modal (Block 6h), nicht als Icon auf
der Record-Card. Ein Cover kann hinzugefügt und ersetzt, aber **nicht
gelöscht** werden — das Backend hat weder einen `DELETE`-Endpunkt noch eine
`RemoveAlbumCover()`-Domänenmethode; eine Löschfunktion wäre ein eigener,
separat freizugebender Backend-Block und bewusst nicht Teil von 6i.

Umgesetzt:

- `features/records/record.ts`: Konstanten `MAX_ALBUM_COVER_SIZE_BYTES`
  (5 MB) und `ALLOWED_ALBUM_COVER_CONTENT_TYPES` (JPEG/PNG) ergänzt —
  spiegeln die Backend-Regel für schnelles Client-Feedback.
- `features/records/record.service.ts`: `uploadCover(id, file)` ergänzt,
  sendet `multipart/form-data` (Feldname `file`, passend zum
  Backend-Parameternamen `IFormFile file`) an `POST /records/{id}/cover`.
- `features/records/record-form/record-form.ts`/`.html`: neues Feld
  „Album-Cover" nach „Information" mit Vorschau-Thumbnail
  (`.record-cover`-Klasse, aus `record-card`/`record-detail` bekannt) und
  Datei-Input (`accept="image/jpeg,image/png"`). Client-seitige
  Vorprüfung von Typ/Größe vor jedem Server-Roundtrip. Vorschau nutzt im
  Bearbeiten-Modus zunächst `record.albumCoverDataUrl`, nach Dateiauswahl
  `URL.createObjectURL(file)` (Freigabe der Object-URL bei Auswahlwechsel
  und `ngOnDestroy`). `save()` ruft nach erfolgreichem `create`/`update`
  zusätzlich `uploadCover(...)` auf, falls eine Datei gewählt wurde — in
  einem eigenen try/catch, damit Record-Speichern und Cover-Upload
  unabhängig bleiben: Ein fehlgeschlagener Cover-Upload zeigt ein Modal,
  verhindert aber nicht `saved.emit()` (der Record selbst wurde bereits
  korrekt gespeichert, analog zu US-R4).
- `shared/error-modal/error-modal.service.ts`/`error-modal.ts`:
  `ErrorModalKind` um `'validation'` (Titel „Ungültige Eingabe") erweitert,
  `mapToState` erhält einen 400-Zweig, der die erste Meldung aus
  `ValidationProblemDetails.errors` extrahiert (statt der bisherigen
  generischen „Es ist ein unerwarteter Serverfehler aufgetreten."-Meldung,
  die für einen 400er fachlich unpassend war, siehe
  `wiki/fehler-und-ausnahmekonzept.md`: 400 = Validierungsfehler, 500 =
  Serverfehler). Neue Methode `showValidationMessage(message)` für rein
  clientseitig erkannte Fehler (kein `HttpErrorResponse` vorhanden, z. B.
  Cover-Vorprüfung). **Nebeneffekt, bewusst in Kauf genommen**: Der
  bestehende 400-Fallback in `handleSaveError` aller vier Formulare
  (Genre/Label/Artist/Record), der greift, wenn ein 400-Fehlerschlüssel zu
  keinem bekannten Formularfeld passt, zeigt jetzt die echte
  Validierungsmeldung statt der generischen Serverfehler-Meldung — eine
  Verbesserung eines bisher kaum erreichbaren Randfalls, kein neues
  Verhalten für einen zuvor funktionierenden Pfad.
- Tests ergänzt: `record.service.spec.ts` (`uploadCover`, `FormData`-Body,
  400-Fehler-Durchreichung), `error-modal.spec.ts` (400 mit/ohne `errors`,
  `showValidationMessage`), `record-form.spec.ts` (Vorschau im
  Bearbeiten-Modus, Vorschau nach Dateiauswahl, Validierungsmodal bei zu
  großer/falscher Datei ohne Server-Call, `uploadCover`-Aufruf mit neuer Id
  nach erfolgreichem Anlegen, kein Aufruf ohne Datei, Cover-Fehler zeigt
  Modal und emittiert trotzdem `saved`). 313 Frontend-Tests insgesamt, alle
  grün. Production-Build grün.

**Fund während der Live-Verifikation, im selben Block behoben**: Das
Fehler-Modal (`ErrorModal`) erschien beim Cover-Validierungsfehler zwar im
DOM mit korrektem Text, war aber visuell **unsichtbar** — verdeckt vom
geöffneten `RecordForm`-Modal. Ursache: `app.html` mountete
`<app-error-modal />` vor `<router-outlet />`; beide Modale nutzen dieselbe
`.scrim`-Klasse mit festem `z-index: 50`
(`styles/design-system/components.css`), sodass bei gleichem z-index die
DOM-Reihenfolge über die Stapelung entscheidet — das später im
`router-outlet` gerenderte `RecordForm`-Modal lag über dem `ErrorModal`,
unabhängig davon, welches zeitlich zuletzt geöffnet wurde. Dieser Fall trat
vor Block 6i nie auf, da bislang kein Formular-Modal einen weiteren
Fehler-Modal-Aufruf auslösen konnte, während es selbst noch offen war. Fix:
`app.html` — `<app-error-modal />` hinter `<router-outlet />` verschoben,
damit es unabhängig vom jeweils offenen Routen-Modal immer zuoberst liegt.
Live erneut geprüft: Fehler-Modal erscheint jetzt sichtbar über dem
Formular.

Live-Verifikation (2026-08-15, gegen den laufenden Aspire-AppHost, Login
als `testuser`): Record mit gültigem PNG-Cover angelegt → Cover erscheint
in der Card-Ansicht und in der Detailansicht; über das Bearbeiten-Modal
zeigt die Vorschau zunächst das bestehende Cover, ein neu gewähltes Bild
ersetzt es sichtbar nach dem Speichern; Datei mit falschem Format
(`.txt`) und Datei über 5 MB lösen beide das Validierungsmodal aus, ohne
einen Server-Request zu erzeugen; keine Konsolenfehler während des
gesamten Ablaufs. Test-Record nach der Prüfung wieder gelöscht.

Bewusst nicht Teil dieses Standes:

- Cover löschen (siehe Design-Klärung oben) — eigener, separat
  freizugebender Backend+Frontend-Block, falls künftig gewünscht.
- Tracks hinzufügen/bearbeiten/löschen (US-T1–T3, Block 6j).
- Backend-Änderungen (Upload-Endpunkt bereits vollständig vorhanden).

Abnahmekriterium:

- Beim Anlegen und Bearbeiten eines Records lässt sich ein Album-Cover
  (JPEG/PNG, max. 5 MB) auswählen und hochladen; ungültige Dateien zeigen
  ein Modal statt eines Server-Roundtrips bzw. eines Inline-Fehlers; ein
  fehlgeschlagener Cover-Upload verhindert nicht das Speichern des Records.
  **Vollständig erfüllt** — automatisierte Tests grün und zusätzlich live
  im Browser gegen den laufenden Aspire-AppHost verifiziert (siehe oben).

### 6j. Tracks (Track-CRUD in der Detailansicht)

Status: **abgeschlossen** (2026-08-15), automatisiert und live verifiziert,
PR #63 nach `main` gemergt.
Branch: `block-6j-tracks-frontend`
Arbeits-Prompt: `docs/prompts/2026-08-15-block-6j-tracks-frontend.md`

Anlass: Letzter der fünf Teilblöcke 6f–6j (siehe Abschnitt 6f). Deckt
US-T1–T3 (Track hinzufügen/bearbeiten/löschen) ab. Das Track-Backend
(`POST/PUT/DELETE /api/records/{id}/tracks[/{trackId}]`) war seit Block 6c
bereits vollständig vorhanden — reiner Frontend-Block, **keine
Backend-Änderung**.

Klärung mit dem Projektinhaber während der Planung (Wiki/Code-Konflikt, siehe
`wiki/architektur/api-endpunkte.md`, Klärung 2026-08-15): Die Wiki-Klärung
vom 2026-08-14 sah `/all`-Dropdowns für **beide** Fremdschlüssel (Artist und
Genre) in „RecordTrack-Formularen" vor. Das bereits umgesetzte `RecordForm`
weicht davon für Label/Artist bereits ab (Autocomplete statt Dropdown). Für
das Track-Formular gilt: **Genre** über `GET /api/genres/all` als
`<select>`-Dropdown (kleine Liste, folgt dem Wiki-Zweck), **Artist** über
Autocomplete mit `getPaged` (konsistent zum bereits etablierten Artist-Feld
in `RecordForm`). Kein Inline-„Artist neu anlegen"-Flow im Track-Formular
(anders als `RecordForm`) — die User Stories verlangen dafür keine
Anlage-Möglichkeit, nur eine Inline-Fehlermeldung bei ungültiger Referenz.

Umgesetzt:

- `features/genres/genre.service.ts`: `getAll()` ergänzt, ruft den seit
  Block 6e bestehenden, im Frontend bislang ungenutzten Endpunkt
  `GET /api/genres/all` auf (Muster: `CountryService.getAll()`).
- `features/records/record.ts`: `CreateTrackRequest`/`UpdateTrackRequest`
  ergänzt.
- `features/records/record.service.ts`: `createTrack`/`updateTrack`/
  `deleteTrack` ergänzt (Muster: `uploadCover`).
- `features/records/track-form/` (neu): `TrackForm` — Signal-Forms-Formular
  kombiniert das Create/Edit-Muster aus `GenreForm` mit dem
  Artist-Autocomplete-Block aus `RecordForm`; Genre als natives `<select>`
  über `GenreService.getAll()`. Validierung (Zeichensatz, Längen,
  Tracknummer ≥ 1) exakt gegen die Backend-Validatoren
  (`RecordTrackEntity`, `CreateRecordTrackCommandValidator`) abgeglichen.
  400-Feldfehler werden inline zugeordnet, 404/409/500 gehen an
  `ErrorModalService` (Konflikt bei doppelter Seite/Nummer-Kombination damit
  korrekt als Modal, nicht inline).
- `features/records/track-list/`: `TrackList` um `editRequested`/
  `deleteRequested`-Outputs und Icon-Buttons je Zeile erweitert (Muster:
  `RecordCard`) — bleibt sonst reine Anzeige.
- `features/records/record-detail/`: `RecordDetail` (bisher reiner
  Lesemodus, Block 6h) um „Track hinzufügen"-Button, verschachteltes
  `TrackForm`-Modal und `ConfirmModal` für das Löschen erweitert (gleiches
  Verschachtelungsmuster wie `LabelForm`/`ConfirmModal` in `RecordForm`);
  nach jeder Track-Änderung wird die bestehende `recordResource` neu
  geladen. Record selbst bleibt weiterhin nicht aus dem Modal heraus
  bearbeit-/löschbar (dafür bleiben die Icons auf `RecordCard` zuständig,
  unverändert seit Block 6h).
- Tests ergänzt: `genre.service.spec.ts` (`getAll`), `record.service.spec.ts`
  (`createTrack`/`updateTrack`/`deleteTrack`, 409-Fehler-Durchreichung),
  `track-list.spec.ts` (neue Outputs), `track-form.spec.ts` (neu, 20 Fälle:
  Vorbefüllung, alle Validierungsregeln, Create/Update, 400-Feldzuordnung,
  409/404 → Modal), `record-detail.spec.ts` (Add-Button öffnet Formular,
  Bearbeiten-Icon öffnet vorbefüllt, Löschen mit Bestätigung inkl. Reload,
  Abbrechen ohne HTTP-Aufruf; bestehender „reiner Lesemodus"-Test für die
  Record-Ebene bleibt unverändert grün, zusätzlich mit vorhandenen Tracks
  erneut geprüft). 332 Frontend-Tests insgesamt, alle grün. Production-Build
  grün, Prettier-Check grün.

Live-Verifikation (2026-08-15, gegen den laufenden Aspire-AppHost, Login als
`testuser`): Track zu einem Record ohne Tracks hinzugefügt (Künstler über
Autocomplete, Genre über das neue Dropdown, Seite/Nummer) → erscheint sofort
in der Tracklist; zweiter Track mit identischer Seite/Nummer-Kombination →
409-Konflikt-Modal mit serverseitig formulierter Meldung, kein
Tracklist-Eintrag; dritter Track mit Seite „B" → beide Tracks korrekt nach
Seite gruppiert; Bearbeiten-Icon öffnet das Formular vollständig vorbefüllt
(Künstler, Genre, Trackname, Seite, Nummer), Namensänderung wird nach
Speichern sofort in der Tracklist sichtbar; Löschen-Icon öffnet die
Sicherheitsabfrage mit korrektem Tracknamen, Bestätigen entfernt den Track
und lädt neu; Pflichtfeld-Inline-Fehler (z. B. leerer Trackname nach Blur)
erscheinen korrekt in Rot. Browser-Konsole zeigte während des gesamten
Ablaufs keine unerwarteten Fehler (nur das erwartete Dev-Mode-Logging des
absichtlich ausgelösten 409-Fehlers). Test-Track nach der Prüfung wieder
gelöscht, Ausgangszustand („Noch keine Tracks vorhanden") wiederhergestellt.

Bewusst nicht Teil dieses Standes:

- Kein Inline-„Artist neu anlegen"-Flow im Track-Formular (siehe Klärung
  oben).
- `/labels/all`/`/artists/all` bleiben weiterhin ungenutzt (nur
  `/genres/all` wird in diesem Block erstmals konsumiert).

Abnahmekriterium:

- Tracks lassen sich zu einem eigenen Record hinzufügen, bearbeiten und
  löschen; fremde Referenzen (Artist/Genre/Record/Track) liefern 400/404;
  doppelte Seite/Nummer-Kombination liefert 409 als Modal. **Vollständig
  erfüllt** — automatisierte Tests grün und zusätzlich live im Browser gegen
  den laufenden Aspire-AppHost verifiziert (siehe oben).

Nachtrag (2026-08-15, Arbeits-Prompt
`docs/prompts/2026-08-15-fix-record-detail-modalbreite-und-trackgenre.md`):
Zwei Korrekturen am Detail-Modal nach Live-Test durch den Projektinhaber,
reiner Frontend-Fix, kein Backend-Change:

- **Modal-Breite**: `app-modal` (`shared/modal/`) hat einen neuen Input
  `wide` erhalten; gesetzt auf `true` greift die neue CSS-Modifier-Klasse
  `.modal-wide` (`max-width: 720px` statt der globalen 460px). Nur
  `RecordDetail` setzt `[wide]="true"` — alle anderen Modals (RecordForm,
  TrackForm, LabelForm, ConfirmModal, ErrorModal) bleiben unverändert bei
  460px.
- **Track-Genre**: `track-list.html` zeigt `track.genreName` jetzt als
  Badge (Stil wie das Format-Badge im Modal-Kopf) direkt hinter
  „Künstler · Trackname" in derselben Zelle. `track-list.spec.ts` prüft
  das jetzt positiv.
- 334 Frontend-Tests insgesamt, alle grün (332 zuvor + 2 neue Tests für
  den `wide`-Input von `Modal`). Production-Build grün. Manuelle
  Live-Prüfung im Browser steht aus (kein laufender Aspire-AppHost
  während der Umsetzung).

## UX-Nachtrag: Tooltips für Badges und Buttons (2026-08-15)

Arbeits-Prompt:
`docs/prompts/2026-08-15-fix-fehlende-tooltips-badges-buttons.md`.

Beim Live-Test ist aufgefallen, dass Badges und Buttons im gesamten
Angular-Frontend überwiegend keine Tooltips hatten. Betrifft blockübergreifend
Genre (Block 2), Label (Block 4), Artist (Block 5), Record/Tracks (Block 6)
sowie Nav (Block 0g) und den Theme-Umschalter (Block 0f), reiner
Frontend-Fix, kein Backend-Change:

- Alle `<button>`- und `.badge`-/`.grade`-/`.fmt`-Vorkommen im
  Angular-Workspace inventarisiert. Tooltips ausschließlich über das native
  HTML-`title`-Attribut ergänzt (keine neue Library) — Icon-only-Buttons
  (Bearbeiten/Löschen in den Tabellen und auf der RecordCard, Paginierung,
  Theme-Umschalter, Modal-Schließen) übernehmen denselben Text wie ihr
  bereits vorhandenes `aria-label`; Text-Buttons (Anlegen, Abbrechen,
  Speichern, Login/Logout, Bestätigen in den Modals) bekommen ein `title`,
  das den sichtbaren Text spiegelt bzw. beim „Anlegen"-Button in den
  Toolbars präzisiert (z. B. „Neuen Artist anlegen").
- Anzahl-Badges in den Toolbars erklären jetzt, dass die Zahl die aktuell
  gefilterten Treffer zählt. Die Goldmine-Grade-Badges (`.grade`, auf der
  RecordCard und im Detail-Modal) zeigen den ausgeschriebenen Zustandsnamen
  aus der bereits vorhandenen Konstante `RECORD_CONDITION_LABELS`
  (`features/records/record.ts`), z. B. „Zustand: Very Good Plus" — die
  Abkürzungen (VG+, G+, NM, …) sind sonst nicht selbsterklärend.
- Wiki: neuer Abschnitt „Tooltips" in
  `02 Wiki/MyMusic Wiki/wiki/architektur/ui-ux-konzept.md` sowie Eintrag in
  `wiki/log.md`.
- 353 Frontend-Tests insgesamt, alle grün (334 zuvor + 19 neue
  Tooltip-Tests). Production-Build grün. `npx prettier --check` meldet
  projektweit auch für unveränderte Bestandsdateien Formatierungsabweichungen
  (bekannte, bereits dokumentierte CRLF-Diskrepanz unter Windows,
  `core.autocrlf=true`; CI prüft das für das Frontend ohnehin nicht) — an den
  eigenen Änderungen selbst keine darüber hinausgehenden Abweichungen.
  Zeilenlängen (≤120 Zeichen) der geänderten Zeilen per `git diff` geprüft,
  keine Überlänge. Manuelle Live-Prüfung im Browser durch den
  Projektinhaber erfolgt und bestätigt.

## 7. Authentifizierung und Mandantentrennung

Status: teilweise offen; Block 7a (Angular-Login-Flow), Block 7b
(Rollenkonzept im Angular-Code), Block 7c (Admin-Bereich, inkl.
Live-Verifikation) und Block 7f (Keycloak-Custom-Theme der Anmeldeseite)
abgeschlossen (siehe Abschnitt 7c)
Priorität: hoch; JWT-Validierung ist bereits im Walking Skeleton entstanden

Ziel:

- Vollständige Umsetzung des Sicherheitskonzepts
  (Wiki `sicherheit/sicherheitskonzept.md`).

Bereits umgesetzt (Block 0b, siehe oben):

- `AddAuthentication().AddJwtBearer()` gegen die Keycloak-Authority,
  `ICurrentUserService` liest den `sub`-Claim, einmal nachgewiesen per
  Integrationstest (`/api/me`).

### 7a. Angular-Login-Flow

Status: **abgeschlossen** (2026-08-09)
Arbeits-Prompt: `docs/prompts/2026-08-09-block-7a-login-flow.md`

Umgesetzt:

- Angular-seitiger Login-Flow gegen Keycloak (Authorization Code + PKCE) über
  `angular-auth-oidc-client` (ADR 0010), konfiguriert über eine asynchrone
  Factory (`core/auth/keycloak-config.factory.ts`), die die
  Keycloak-Authority zur Laufzeit aus der um `keycloakAuthority` erweiterten
  `RuntimeConfigService` liest (Fortsetzung des Musters aus ADR 0009).
- `AuthGuard` (`core/auth/auth.guard.ts`, Re-Export von
  `autoLoginPartialRoutesGuard`) auf allen Routen; projekteigener
  `unauthorized-redirect.interceptor.ts` leitet bei HTTP 401/403 einer
  API-Antwort zur Anmeldung um (die Bibliothek erkennt das nicht selbst).
- `scope` bewusst ohne `offline_access`, damit der Refresh Token an die
  Realm-Werte `ssoSessionIdleTimeout`/`ssoSessionMaxLifespan` (30 Min.
  Sliding / 8 h Hard-Cap) gebunden bleibt (siehe ADR 0010).
- Minimale Development-CORS-Policy im Backend (`Program.cs`,
  `Uri.IsLoopback`) — ohne sie hätte der Interceptor nicht gegen die echte
  API verifiziert werden können. Production-Whitelist bleibt offen (siehe
  unten).
- `AppHost.cs`: Frontend-Port fest auf `4200` gepinnt (der
  Keycloak-Realm-Client `mymusic-angular` hat `redirectUris`/`webOrigins`
  hart auf diesen Port hinterlegt, ein von Aspire dynamisch vergebener Port
  hätte den Login-Flow beim Start über den AppHost gebrochen) und
  `MYMUSIC_KEYCLOAK_AUTHORITY` als neue Env-Var ergänzt.
- Minimaler Login-/Logout-Button in der bestehenden App-Shell sowie eine
  Platzhalter-Komponente (`core/shell/home-placeholder/`), die `GET /api/me`
  aufruft — konkreter Ende-zu-Ende-Nachweis, dass der Interceptor das Token
  tatsächlich an einen echten, geschützten API-Aufruf anhängt. Bei
  Anmeldung zeigt die Kopfzeile zusätzlich den `preferred_username` aus den
  OIDC-Benutzerdaten neben dem Logout-Button (`navigation-konzept.md`,
  Abschnitt „User-Bereich" — Profil-Modal beim Klick auf den Namen folgt
  erst mit der echten `NavComponent`, hier bewusst nur Text ohne
  Klick-Handler).
- `.github/workflows/ci.yml`: neuer `frontend-check`-Job (Node, `npm ci`,
  `npm run build`, `npm test`) — schließt die seit Block 0c bestehende Lücke,
  dass Frontend-Code nie in CI lief.
- Neue Unit Tests (Vitest): `RuntimeConfigService` (Erweiterung +
  Memoisierung), `keycloak-config.factory`, `unauthorized-redirect.interceptor`,
  `HomePlaceholder`, `App` (Login-/Logout-Button, Benutzername in der
  Kopfzeile) — 19 Tests, alle grün.
- Neuer Integrationstest `CorsPolicyTests.cs` (Preflight erlaubt/verweigert).
- `keycloak/mymusic-realm.json` unverändert (ADR 0005: Auth-Code-only ist
  bewusst so).

Nachtrag (2026-08-09): Die erste Live-Verifikation im Browser (Aspire-AppHost,
echter Keycloak-Testbenutzer) deckte eine Endlosschleife auf — nach jedem
Rücksprung von Keycloak wurde sofort wieder zu Keycloak umgeleitet, ohne
jemals einen Token-Austausch durchzuführen. Ursache: `autoLoginPartialRoutesGuard`
verarbeitet den OIDC-Callback nicht selbst, sondern prüft nur vorhandene
Tokens im Storage — ohne das Bibliotheks-Feature `withAppInitializerAuthCheck()`
(registriert einen `APP_INITIALIZER`, der `checkAuth()` beim Start aufruft)
wurde nie ein Token angefordert. Alle bisherigen automatisierten Tests hatten
`OidcSecurityService` durchgängig gemockt und diese Lücke deshalb nicht
erkannt. Fix in `app.config.ts` (samt erklärendem Kommentar) ergänzt, siehe
ADR 0010 Nachtrag für Details. Nach dem Fix live erneut geprüft: echtes
Login-Formular, Rückkehr zur ursprünglich aufgerufenen URL (`/records/42`),
korrekte `userId` über `GET /api/me`, Logout beendet die reale
Keycloak-Session. US-AU5/US-AU6 (zeitbasierte Token-Erneuerung/-Ablauf)
bleiben mangels praktikabler Wartezeit nur über die bestehenden
Interceptor-Unit-Tests abgedeckt, nicht live nachgewiesen.

Nachtrag (2026-08-09): Nachträglich aufgefallen (Rückfrage des
Projektinhabers), dass die Kopfzeile bei Anmeldung entgegen
`navigation-konzept.md` („User-Bereich": `[Username]` + `[Logout-Button]`)
nur einen nackten Logout-Button zeigte — eine bewusst minimal gehaltene, aber
nicht klar genug kommunizierte Abweichung. Ergänzt: `App` liest
`OidcSecurityService.userData()` und zeigt `preferred_username` neben dem
Logout-Button. Live erneut mit einem frisch angelegten Testbenutzer geprüft
(Kopfzeile zeigt korrekt den Benutzernamen, `GET /api/me` liefert die
passende `userId`). Profil-Modal (Klick auf den Namen) bleibt bewusst offen
für die echte `NavComponent`.

Bewusst nicht Teil dieses Standes:

- Rollenkonzept (`User`/`Admin`) und `AdminGuard`/Admin-Tab in Angular.
- Keycloak-Custom-Theme der Anmeldeseite (US-AU8, siehe Wiki
  `user-stories/user-stories-authentifizierung.md`) — eigener, späterer
  Block.
- Wildcard-Route (`'**'`) in `app.routes.ts` ist eine bewusste, temporäre
  Verifikationshilfe für US-AU2 — entfällt mit den echten Feature-Routen.

Abnahmekriterium erfüllt (live verifiziert, siehe Nachtrag oben):

- Ohne Anmeldung ist keine geschützte Route erreichbar; nach Anmeldung trägt
  jeder API-Aufruf automatisch ein gültiges Access Token; Logout beendet die
  reale Keycloak-Sitzung. Die zeitbasierten Teile (stille Erneuerung nach
  Ablauf des Access Tokens, Redirect nach abgelaufener Sitzung) sind nur durch
  Unit-Tests, nicht live nachgewiesen.

Aufgaben (noch offen):

- Swagger-UI in Production für die Admin-Rolle freischalten: **erledigt**
  mit Block 7j, siehe Abschnitt 7j.
- CORS-Production-Whitelist: **erledigt** (Konfigurationsmechanismus) mit
  Block 7j, siehe Abschnitt 7j.
- CSP: **teilweise erledigt** mit Block 7j (nur Development/lokal, siehe
  Abschnitt 7j) — die Production-Variante (HTTP-Header vom Nginx) bleibt
  offen, abhängig vom noch nicht begonnenen Production-/Docker-Compose-Setup.
- Sicherheitstests: nicht authentifiziert, fremde Daten, unbekannte IDs.

Abnahmekriterium (Gesamtabschnitt 7):

- Ohne Login ist kein fachlicher Endpunkt erreichbar; Benutzer sehen
  ausschließlich eigene Daten; der Admin kann Benutzer löschen — mit
  Block 7c backend- und frontendseitig umgesetzt und automatisiert
  nachgewiesen, die manuelle Live-Prüfung im Browser steht noch aus (siehe
  Abschnitt 7c).

### 7b. Rollenkonzept User/Admin im Angular-Code

Status: **abgeschlossen** (2026-08-16), automatisiert getestet und live
verifiziert; PR #71, nach `main` gemergt.
Arbeits-Prompt: `docs/prompts/2026-08-16-block-7b-rollenkonzept-admin-guard.md`

Umfang bewusst auf den Angular-Code begrenzt (TASK.md-Vorgabe „im
Angular-Code"): kein Backend-Code geändert, die Ownership-Prüfung (404 statt
403) ist bereits je CRUD-Slice serverseitig umgesetzt. Admin-Bereich-Inhalt
(Userliste/-löschung), Swagger-Freischaltung für Production, Rate Limiting,
CORS-Whitelist und CSP bleiben eigene, spätere Punkte in Abschnitt 7.

Umgesetzt:

- `UserRolesService` (`core/auth/user-roles.service.ts`, neu): exponiert
  `roles`/`isAdmin` als Signals.
- `AdminGuard` (`core/auth/admin.guard.ts`, neu): funktionaler
  `CanActivateFn`, leitet ohne Rolle `Admin` still auf `/dashboard` um (kein
  Modal, konsistent mit CLAUDE.md §7 „Rolle unzureichend" → Weiterleitung).
- `features/admin/` (neu): Platzhalter-Komponente 1:1 nach dem Muster von
  `features/dashboard/`, eigene `admin.routes.ts`.
- `app.routes.ts`: neue `/admin`-Route mit `canActivate: [adminGuard]`.
- `NavComponent`: Admin-Button (Label only, kein Icon) zwischen
  Theme-Toggle und Username/Login, sichtbar nur bei `isAdmin()` — Position
  und Verhalten nach `wiki/architektur/navigation-konzept.md`.
- 13 neue Vitest-Tests (`user-roles.service.spec.ts`,
  `admin.guard.spec.ts`, `admin.spec.ts`, Erweiterungen von
  `app.routes.spec.ts` und `nav.spec.ts`); `app.spec.ts` um den seither
  fehlenden `getPayloadFromAccessToken`-Mock ergänzt. 366 Frontend-Tests
  insgesamt, alle grün. Production-Build grün.

Empirische Erkenntnis zur Rollenclaim-Quelle (live gegen den laufenden
Aspire-AppHost mit einem Testbenutzer geprüft, dem die Rolle `Admin` in der
Keycloak-Admin-UI zugewiesen wurde): Der geplante erste Ansatz, die Rolle
aus `OidcSecurityService.userData()` (Ergebnis des Keycloak-`/userinfo`-
Endpunkts) zu lesen, funktioniert **nicht** — die UserInfo-Antwort enthält
keinen `realm_access`-Claim, nur die Standard-Profilclaims (`sub`,
`preferred_username`, `email`, …). Der rohe **Access Token** enthält
`realm_access.roles` dagegen wie erwartet. `UserRolesService` liest die
Rolle deshalb über `oidcSecurityService.getPayloadFromAccessToken()`,
reaktiv erneut ausgelöst bei jeder Änderung des `authenticated`-Signals
(`toObservable(authenticated).pipe(switchMap(...))`) — ein einmaliges
Auslesen beim Service-Start hätte Login/Logout/Rollenänderungen nicht
nachgezogen.

Bewusst nicht Teil dieses Standes:

- Admin-Bereich-Inhalt (`/admin` zeigt nur einen Platzhalter).
- Jede Backend-Änderung.
- Verhalten bei Rollenänderung während einer laufenden Session ohne
  Token-Neuerwerb (Silent Renew übernimmt eine geänderte Rolle erst mit dem
  nächsten tatsächlichen Access-Token-Refresh — nicht gesondert geprüft).

Abnahmekriterium erfüllt (live verifiziert):

- Mit der Rolle `Admin` erscheint der Admin-Button an der vorgesehenen
  Position (Theme-Toggle · Admin · Username · Logout), ein Klick navigiert
  zu `/admin` und zeigt die Platzhalteransicht. Ohne die Rolle bleibt der
  Button laut automatisierten Tests ausgeblendet und `/admin` leitet auf
  `/dashboard` um (deterministisch über denselben Code-Pfad wie der
  positive Fall abgesichert, nicht zusätzlich live mit einem zweiten
  Testbenutzer nachgewiesen).

### 7c. Admin-Bereich

Status: **abgeschlossen** (Backend und Frontend: 2026-08-17, PR #74; manuelle
Live-Verifikation im Browser: 2026-08-20, siehe Nachtrag unten).
Arbeits-Prompt: `docs/prompts/2026-08-17-block-7c-admin-bereich.md`

Anders als bei Genre/Label/Artist wird dieser Slice nicht in getrennte
Backend-/Frontend-Blöcke aufgeteilt — die User Stories
(`../../02 Wiki/MyMusic Wiki/wiki/user-stories/user-stories-admin.md`)
beschreiben den Admin-Bereich durchgehend als ein zusammenhängendes Feature.

Umgesetzt:

- `GET /api/admin/users` (paginiert, analog zu Genre/Label/Artist) und
  `DELETE /api/admin/users/{id}` — neue Kategorie `Verwaltung` unter
  `Application/Features/` (siehe Wiki `architektur/application-layer.md`).
- Erste serverseitige Rollenautorisierung im Projekt: eigene
  `"Admin"`-Policy, die den rohen `realm_access`-Claim auswertet
  (`src/MyMusic.Api/Authorization/`, siehe ADR 0015) — bisher gab es nur
  die clientseitige Ausblendung des Admin-Buttons aus Block 7b.
- Erster externer HTTP-Client der Anwendung: `KeycloakAdminClient`
  (`src/MyMusic.Infrastructure/ExternalServices/Keycloak/`) für die
  Keycloak Admin REST API (Benutzerliste, Benutzer löschen), authentifiziert
  über einen neuen, dedizierten Service-Account-Client
  `mymusic-admin-service` (Client-Credentials-Grant, minimale
  `realm-management`-Rollen `view-users`/`query-users`/`manage-users` —
  `query-users` empirisch bei der Live-Verifikation als zusätzlich nötig
  ermittelt). Das Secret dieses Clients wird von Keycloak generiert und
  beim API-Start einmalig ausgelesen, kein neues Secret in User Secrets
  nötig; schlägt das Laden fehl, startet die API trotzdem — nur die
  Admin-Endpunkte antworten dann mit 500 (siehe ADR 0016 samt Nachtrag).
  Die Admin-Rolle je Benutzer wird über
  `GET /users/{id}/role-mappings/realm` ermittelt (ein Aufruf pro Benutzer),
  nicht über den Rollen-Mitglieder-Endpunkt (`/roles/{rolle}/users`) — der
  lieferte trotz aller drei Rollen 403, vermutlich wegen Keycloaks
  Fine-Grained-Admin-Permissions (siehe ADR-0016-Nachtrag).
- `DeleteUserCommandHandler`: Selbstlöschung gesperrt (409), App-Daten
  (Record, Label, Artist, Genre) werden vor dem Keycloak-Account gelöscht,
  in dieser Reihenfolge wegen der bestehenden FK-Restrict-Beziehungen.
- Angular-Feature `features/admin/` ersetzt den Platzhalter aus Block 7b:
  Userliste mit Paginierung (`shared/pagination/`), Löschen mit
  Bestätigungsmodal (stärkerer Warnhinweis als das sonst übliche knappe
  Muster, da hier die gesamte Sammlung eines Benutzers mitgelöscht wird),
  kein Löschen-Icon bei der eigenen Zeile.
- Unit-Tests (`MyMusic.Api.Tests`, `MyMusic.Application.Tests`),
  Integrationstest `AdminEndpointsTests.cs` (401/403/200, Selbstlöschung,
  Löschung inkl. App-Daten-Bereinigung), `KeycloakTestClient.cs` um
  Rollenzuweisung erweitert; Vitest-Tests `admin.spec.ts`,
  `admin.service.spec.ts`. `dotnet build`, `dotnet format
  --verify-no-changes`, alle Backend- und Frontend-Tests grün
  (Domain 114, Application 248, Api 11, Infrastructure 5,
  IntegrationTests 13/13, Frontend 375), `npm run build` grün.
- Live gegen den laufenden Aspire-AppHost verifiziert: Der neue
  Service-Account-Client wurde im lokalen `mymusic-keycloak-data`-Volume
  nachträglich angelegt (`--import-realm` überspringt bereits importierte
  Realms — `IGNORE_EXISTING`, dieselbe Einschränkung wie in Block 7f/ADR
  0014 dokumentiert; einmalig per `kcadm.sh` im laufenden Container
  nachgezogen, analog zum dortigen Vorgehen).

Bewusst nicht Teil dieses Blocks (eigene, spätere Punkte):

- Swagger-UI-Freischaltung für Production, Rate Limiting,
  CORS-Production-Whitelist, CSP.

**Nachtrag (2026-08-20): Live-Verifikation im Browser nachgeholt, Ursache des
AppHost-Starthängers korrigiert**

Der in der PR-#74-Beschreibung dokumentierte Hänger („`dotnet run` auf dem
AppHost hängt beim Merge reproduzierbar beim Start von Postgres/Keycloak,
Seq startet, die anderen nicht, Ursache ungeklärt") beruhte **nicht** auf
einem AppHost- oder Docker/DCP-Problem. Ursache war `dotnet run
--no-launch-profile`: Dieses Flag überspringt `launchSettings.json` und
damit `DOTNET_ENVIRONMENT=Development`; ohne diese Umgebung lädt .NET keine
User Secrets, wodurch das Aspire-Dashboard mit „nicht aufgelöste Parameter"
(Postgres-/API-DB-/Keycloak-Admin-Passwort) hängen blieb, obwohl die
Secrets lokal längst vorhanden waren. Mit `dotnet run --launch-profile
https` (bzw. ohne das Flag, dem Standardverhalten) starten alle Ressourcen
zuverlässig durch. Analog zur Korrektur in Block 2 (CLAUDE.md §11,
Git-Bash- statt PowerShell-Ausführung) eine weitere fälschlich als
Aspire/DCP-Einschränkung eingeordnete Ursache, die tatsächlich an der
Aufrufart lag.

Live gegen den so gestarteten AppHost verifiziert:

- Login als `testuser` (Rolle Admin, siehe
  `03 Ressourcen/Keycloak Accounts.md`): Admin-Button sichtbar, `/admin`
  zeigt die Userliste (Benutzername, E-Mail, Rolle) über alle 15
  registrierten Benutzer auf einer Seite, eigene Zeile ohne Löschen-Icon
  (US-AD1, US-AD3).
- Löschen eines Alt-Testkontos (`smoketest-genre-block2b`) über das
  Bestätigungsmodal: Benutzer verschwindet sofort aus der Liste ohne
  Neuladen der Seite (US-AD3).
- Login als `testhorst` (Rolle User): kein Admin-Button; direkter Aufruf
  von `/admin` per URL leitet automatisch zu `/dashboard` um (`AdminGuard`,
  US-AD2).
- Vertiefte Prüfung der Löschung direkt gegen die Infrastruktur, nicht nur
  über die Admin-Liste: Ein eigens registrierter Wegwerf-Testbenutzer
  (`claude-delete-verify`) erhielt je einen Genre-, Label-, Artist- und
  Record-Datensatz (inkl. Track). Vor der Löschung per `docker exec` gegen
  `mymusicdb` bestätigt, dass alle fünf Zeilen existieren; nach der
  Löschung über den Admin-Bereich per ID erneut geprüft — alle fünf Zeilen
  vollständig entfernt. Zusätzlich per `kcadm.sh` (Bootstrap-Admin) direkt
  gegen Keycloak geprüft, dass der Benutzer im Realm `mymusic` nicht mehr
  existiert (`get users -q username=claude-delete-verify` liefert `[ ]`),
  und ein Login-Versuch mit den alten Zugangsdaten gegen den
  Token-Endpunkt mit 401 fehlschlägt. Testbenutzer und Testdaten waren
  durch den Löschvorgang selbst bereits vollständig entfernt, keine
  weitere Aufräumung nötig.

Damit ist Block 7c vollständig abgeschlossen; keine offenen Punkte mehr.

### 7f. Keycloak-Custom-Theme der Anmeldeseite

Status: **abgeschlossen** (2026-08-15)
Arbeits-Prompt: `docs/prompts/2026-08-15-block-7f-keycloak-login-theme.md`

Umgesetzt:

- Eigenes Keycloak-Login-Theme `mymusic` unter `keycloak/themes/mymusic/login/`
  (`parent=keycloak.v2`, nur Theme-Typ `login`): `theme.properties`,
  `template.ftl` (1:1-Kopie aus dem realen `quay.io/keycloak/keycloak:26.5`-Image,
  Header um `mark.svg` + Wortmarke „MyMusic" ergänzt), `resources/css/mymusic.css`
  (Emerald-Akzent, Neutraltöne, Inter, Karten-/Button-/Titel-Farben für Light und
  Dark Mode), `resources/img/mark.svg` (Kopie aus `src/frontend/public/mark.svg`),
  `resources/fonts/inter-latin-{400,600}-normal.woff2` (Kopie aus dem bereits
  installierten `@fontsource/inter`-Paket, kein CDN, keine neue Abhängigkeit).
- `keycloak/mymusic-realm.json`: `"loginTheme": "mymusic"` sowie
  `"internationalizationEnabled": true`, `"supportedLocales": ["de"]`,
  `"defaultLocale": "de"` ergänzt — Letzteres auf Wunsch des Projektinhabers
  während der Live-Prüfung nachgezogen (Keycloak lieferte ohne aktivierte
  Internationalisierung ausschließlich englische Standardtexte; deutsche
  Übersetzungen bringt Keycloak für den Theme-Typ `login` bereits mit).
- `src/MyMusic.AppHost/AppHost.cs`: zusätzlicher Bind-Mount
  `.WithBindMount("../../keycloak/themes/mymusic", "/opt/keycloak/themes/mymusic", isReadOnly: true)`
  auf der `keycloak`-Ressource, analog zum bestehenden Realm-Import-Bind-Mount.
- ADR `docs/adr/0014-keycloak-login-theme.md`.
- Der genaue Keycloak-26.5-Theming-Mechanismus (Parent-Theme-Name,
  CSS-Ladereihenfolge, PatternFly-Variablenbindung) wurde nicht aus Doku
  übernommen, sondern am realen Image empirisch geprüft
  (`docker create`/`docker cp` der Themes-JARs, temporär, `--rm`).

Live verifiziert über den Aspire-AppHost (Browser, `claude-in-chrome`):

- Realm-Import ohne Fehler, Theme `mymusic` wird erkannt.
- Marke (`mark.svg`) + Wortmarke „MyMusic" sichtbar, Emerald-Akzent auf dem
  Primärbutton, Inter-Font geladen (kein Google-Fonts-Request), Kartenlayout,
  Light- und Dark-Mode (`prefers-color-scheme`) beide korrekt.
- Fehleranmeldung (falsches Passwort) zeigt die MyMusic-gestylte Fehlermeldung,
  jetzt auf Deutsch („Ungültiger Benutzername oder Passwort.").
- Bei der ersten Live-Prüfung zwei echte Bugs gefunden und behoben: Die
  Grid-Layout-Regel `.pf-v5-c-login__container` aus dem Parent-Theme fehlte
  (Marken-Header stand frei statt über der Karte) — gezielt nachgezogen, ohne
  den Rest der ausgeschlossenen `styles.css` zu übernehmen. Der
  Dark-Mode-Selektor `:where(.pf-v5-theme-dark)` hatte Spezifität 0 und verlor
  gegen den eigenen `:root`-Block — auf `html.pf-v5-theme-dark` (Typ- +
  Klassenselektor) korrigiert.
- Bekannte kosmetische Abweichung: Der Fokus-Rahmen der Formularfelder bleibt
  PatternFly-Blau statt Emerald (zuständige interne PatternFly-Variable im
  Dark Mode nicht gefunden, kein Blocker für US-AU8).
- Wichtiger Betriebshinweis: `--import-realm` überspringt bereits importierte
  Realms (`IGNORE_EXISTING`). In lokalen Entwicklungsumgebungen mit
  bestehendem `mymusic-keycloak-data`-Volume wirken `loginTheme` und die
  Internationalisierungs-Felder aus der JSON deshalb nicht automatisch —
  einmalig manuell nachziehen (Admin-Konsole oder
  `kcadm.sh update realms/mymusic -s <feld>=<wert>`).

Bewusst nicht Teil dieses Standes:

- `account`- oder `email`-Theme (nur `login`, siehe ADR 0014).
- Sprachumschalter (nur `de` unterstützt, wie die Angular-Anwendung selbst).
- Korrektur des Fokus-Rahmen-Farbtons (siehe oben, kosmetisch).

Abnahmekriterium erfüllt:

- Die Keycloak-Anmeldeseite verwendet ein eigenes Theme statt des
  Standard-Themes, Farben/Typografie entsprechen den Design-Tokens, Marke und
  Wortmarke erscheinen auf der Seite — live im Browser geprüft.

### 7g. Registrierung

Status: **abgeschlossen** (2026-08-20), automatisiert getestet und live
verifiziert, PR #77, nach `main` gemergt.
Arbeits-Prompt: `docs/prompts/2026-08-20-block-7g-registrierung.md`

Anlass: Bisher konnten sich Benutzer nicht selbst registrieren
(`registrationAllowed: false`) — Testbenutzer wurden ausschließlich manuell
über die Keycloak-Admin-Konsole angelegt (siehe Block 7b/7c). Auf Wunsch des
Projektinhabers ergänzt um US-AU9
(`../../02 Wiki/MyMusic Wiki/wiki/user-stories/user-stories-authentifizierung.md`),
neue Story ausschließlich über Keycloak selbst umgesetzt (kein eigenes
Passwort-Handling im Backend, folgt aus CLAUDE.md §1/§5.1).

Umgesetzt:

- `keycloak/mymusic-realm.json`: `"registrationAllowed": true`.
- `src/frontend/src/app/nav/nav.ts`: neue Methode `register()`, leitet über
  `OidcSecurityService.authorize(undefined, { urlHandler })` zu Keycloaks
  Registrierungsendpunkt (`/protocol/openid-connect/registrations`) statt
  zum Anmeldeendpunkt weiter — nutzt die PKCE-/State-Logik der Bibliothek
  unverändert, siehe ADR-0010-Nachtrag (Registrierung).
- `nav.html`: neuer Button „Registrieren" neben „Login" (nur sichtbar, wenn
  nicht angemeldet).
- `nav.spec.ts`: neuer Test, der den `urlHandler`-Callback abfängt und
  prüft, dass er `/protocol/openid-connect/auth` durch
  `/protocol/openid-connect/registrations` ersetzt; bestehender
  Login-Button-Test auf einen präziseren Selektor (`title="Anmelden"`)
  umgestellt, da beide Buttons seither dieselbe Klasse `btn-secondary`
  teilen. 376 Frontend-Tests insgesamt, alle grün. Production-Build grün.

Live gegen den laufenden Aspire-AppHost verifiziert (Browser,
`claude-in-chrome`):

- Default-Rolle `User` in der Keycloak-Admin-Konsole unter „Realm settings
  → User registration → Default roles" ergänzt; anschließend per
  `kcadm.sh get roles/default-roles-mymusic/composites -r mymusic` die
  tatsächliche Zusammensetzung ausgelesen (`offline_access`,
  `uma_authorization`, `User`, `account`-Client-Rollen
  `manage-account`/`view-profile`) und exakt so — nicht geraten — in
  `mymusic-realm.json` unter `roles.realm` als
  `default-roles-mymusic`-Eintrag mit `composites` übernommen.
- Manueller Nachzug auf dem bestehenden lokalen `mymusic-keycloak-data`-
  Datenvolume (`--import-realm` läuft mit `IGNORE_EXISTING`, siehe
  ADR 0014/0016): „User registration" in der Admin-Konsole auf „On"
  gesetzt, Default-Rolle wie oben ergänzt.
- Registrierungsformular erscheint im MyMusic-Design (Marke, Karten-Layout,
  Emerald-Akzent) — keine Theme-Datei musste ergänzt werden, `register.ftl`
  vom Parent-Theme übernimmt `template.ftl`/`mymusic.css` unverändert wie
  erwartet.
- Neues Testkonto über das echte Registrierungsformular angelegt: Rücksprung
  in die App im angemeldeten Zustand, Kopfzeile zeigt den neuen
  Benutzernamen; `GET /api/genres` (Liste) und `POST /api/genres`
  (Neuanlage) gelingen ohne manuellen Rollen-Eingriff.
- Keycloak-Admin-Konsole bestätigt: Der neue Benutzer hat die Rolle `User`
  (über `default-roles-mymusic` geerbt) automatisch erhalten.
- Test-Genre und Test-Benutzerkonto nach der Prüfung wieder entfernt.

**Nachtrag (2026-08-20, noch am selben Tag): Nav-Buttons unerreichbar — Routing-Fix**

Bei der ersten Live-Verifikation zeigte sich: Die Nav-Buttons
„Registrieren"/„Login" waren über normale Navigation praktisch nicht
klickbar. `app.routes.ts` hängte `canActivate: [authGuard]` an den
Wurzelknoten, `''` redirectete auf `/dashboard` — ein nicht angemeldeter
Aufruf von `localhost:4200` löste dadurch sofort `authorize()` aus, noch
bevor ein Klick auf den Header-Button möglich war. Betraf auch den
bestehenden „Login"-Button und war kein durch diesen Block eingeführtes
Problem, sondern eine Eigenschaft der Routing-Architektur aus Block 0g.

Nach Rückfrage mit dem Projektinhaber behoben (siehe ADR 0017): Der
Wurzelpfad `''` ist jetzt eine eigene, unbewachte Route mit neuer
Komponente `core/shell/landing/` — bereits angemeldete Benutzer werden
weiterhin sofort zu `/dashboard` geleitet, nicht angemeldete sehen die
Kopfzeile mit klickbaren Registrieren-/Login-Buttons. Alle anderen Routen
bleiben unverändert vollständig geschützt (US-AU3 nicht betroffen). Neue
Tests: `landing.spec.ts` (2 Fälle), `app.routes.spec.ts` erweitert
(Verdrahtung der Landing-Route, Verhalten mit/ohne Anmeldung). 380
Frontend-Tests insgesamt, alle grün. Production-Build grün.

Live erneut verifiziert: Klick auf den echten „Registrieren"-Button in der
Kopfzeile (nicht mehr nur der Ausweich-Link auf Keycloaks Anmeldeseite)
leitet korrekt zu `/protocol/openid-connect/registrations` weiter, neues
Testkonto erfolgreich angelegt und angemeldet.

**Nachtrag (2026-08-20): Registrierungsformular auf Benutzername/E-Mail/Passwort reduziert**

Auf Wunsch des Projektinhabers: `Vorname`/`Nachname` werden in MyMusic
nirgends verwendet (auch der Admin-Bereich zeigt nur Benutzername, E-Mail,
Rolle, siehe US-AD1). Die Attribute `firstName`/`lastName` wurden aus
Keycloaks „User profile"-Konfiguration entfernt (Realm settings → User
profile → Attributes, live in der Admin-Konsole gelöscht — der JSON-Editor
der Admin-Konsole erwies sich dabei als unzuverlässig, da er beim Tippen
Klammern automatisch schließt und den Inhalt verschachtelt; das direkte
Löschen der Attribute über die Attributliste war der zuverlässige Weg).
Für frische Realm-Imports in `mymusic-realm.json` als neuer
`components`-Block (`org.keycloak.userprofile.UserProfileProvider`,
`providerId: declarative-user-profile`) nachgezogen — die exakte
JSON-Repräsentation wurde per `kcadm.sh create
realms/mymusic/partial-export` gegen den echten Server ermittelt, nicht
geraten. Live erneut verifiziert: Registrierungsformular zeigt nur noch
Benutzername, Passwort, Passwort bestätigen, E-Mail; neues Testkonto ohne
Vor-/Nachname erfolgreich angelegt.

**Nachtrag (2026-08-20): CI-Fehler durch frischen Realm-Import — korrigiert**

Der erste Push (PR #77) ließ `build-and-check` mit Timeout in
`MyMusic.IntegrationTests` fehlschlagen (15-Minuten-CI-Timeout erreicht).
Ursache war kein Timing-/Flaky-Problem, sondern ein harter Fehlschlag des
Keycloak-Containers beim Start, isoliert nachgestellt mit einem
Wegwerf-Container (frisches, nie zuvor importiertes Datenverzeichnis, nicht
das lokale Entwicklungs-Volume):

```
ERROR: Unable to find composite realm role: uma_authorization
```

Der `default-roles-mymusic`-Eintrag in `mymusic-realm.json` referenzierte
`offline_access`/`uma_authorization`/`account`-Client-Rollen explizit —
diese existieren bei einem wirklich frischen Import zu diesem Zeitpunkt
noch nicht (sie werden sonst von Keycloaks eigener interner
Standard-Initialisierung erzeugt, die bei einer selbst angegebenen
`roles.realm`-Liste offenbar nicht mehr greift). Auf dem lokalen
Entwicklungsvolume war das nicht aufgefallen, weil die Rolle dort live über
die Admin-Konsole gesetzt wurde, nicht per Neuimport.

Fix: `default-roles-mymusic` referenziert nur noch `User`
(`"composites": {"realm": ["User"]}`), keine Referenz mehr auf die
Standard-Rollen. Bewusster Funktionsverzicht: Neu registrierte Benutzer
erhalten dadurch nicht automatisch `offline_access`/`uma_authorization`/
Account-Console-Rechte — für MyMusic ohne Auswirkung, da der Scope
`offline_access` ohnehin nicht angefragt wird (ADR 0010) und weder UMA noch
die Keycloak-Account-Console Teil des Produkts sind.

Erneut mit demselben isolierten Wegwerf-Container gegen einen wirklich
frischen Import verifiziert: Realm-Import erfolgreich
(„Import finished successfully"), `default-roles-mymusic` enthält `User`,
`registrationAllowed: true`, User-Profile-Konfiguration korrekt auf
Benutzername/E-Mail reduziert. Damit ist auch die zuvor offene
Einschränkung (fehlende Verifikation gegen einen frischen Import) beseitigt.

Alle neu angelegten Testkonten (inkl. eines Test-Genres) und der
Wegwerf-Container wurden nach der Prüfung wieder entfernt.

### 7h. Admin-Benutzersuche

Status: **abgeschlossen** (2026-08-20), automatisiert getestet und live
verifiziert, PR #80, nach `main` gemergt.
Arbeits-Prompt: `docs/prompts/2026-08-20-block-7h-admin-benutzersuche.md`

Anlass: Auf Wunsch des Projektinhabers ergänzt um US-AD5
(`../../02 Wiki/MyMusic Wiki/wiki/user-stories/user-stories-admin.md`) — die
Admin-Benutzertabelle (Block 7c) zeigte bisher nur eine ungefilterte,
paginierte Liste; neu ist eine Suche nach Benutzername, E-Mail (mit
Autocomplete) oder Benutzer-ID (exakt), ein gemeinsames Suchfeld für alle
drei Kriterien.

Umgesetzt:

- Backend: `GetPagedUsersQuery` um `string? Search` erweitert;
  `GetPagedUsersQueryHandler` filtert die bereits geladene Keycloak-
  Benutzerliste vor Sortierung/Paginierung — bei gültiger GUID exakt nach
  `Id`, sonst als Teilstring (case-insensitive) auf `Username`/`Email`.
  `AdminEndpoints.GetPagedUsersAsync` normalisiert den neuen
  `search`-Query-Parameter wie `page`/`pageSize`. Kein zusätzlicher Aufruf an
  `IKeycloakAdminClient` nötig.
- Frontend: `AutocompleteOption.id` von `number` auf `number | string`
  erweitert (einzige Änderung am gemeinsamen `shared/autocomplete/`-Baustein;
  `record-filter.ts` musste die beiden Auswahl-Handler für Artist/Label
  entsprechend auf `number` zurückcasten). `AdminService.getPaged` bekommt
  einen optionalen `search`-Parameter (Muster wie `ArtistService.getPaged`).
  `Admin`-Komponente: neue Signals `searchText`/`selectedUserId`, neue
  `searchSuggestionsResource` (liefert Autocomplete-Vorschläge über denselben
  Endpunkt, `SUGGESTION_PAGE_SIZE = 10`, wie bereits bei Artist/Label in
  `records.ts` etabliert), `usersResource` erhält zusätzlich
  `search: selectedUserId() ?? searchText()` — Auswahl eines Vorschlags
  filtert dadurch immer per exakter Benutzer-ID, nie per erneutem
  Teilstring-Abgleich des Anzeigetexts.
- Tests: `GetPagedUsersQueryHandlerTests` um fünf neue Fälle ergänzt
  (Teilstring auf Username/E-Mail, Groß-/Kleinschreibung, exakte
  Benutzer-ID, keine Treffer) — 253 Application-Tests insgesamt, alle grün.
  `admin.service.spec.ts`/`admin.spec.ts` um Fälle für den `search`-Parameter
  und das Autocomplete-Verhalten ergänzt — 384 Frontend-Tests insgesamt,
  alle grün. `dotnet format --verify-no-changes` grün.
- Doku: `wiki/user-stories/user-stories-admin.md` (neue Story US-AD5,
  Klärungsabschnitt vom 2026-08-20), `wiki/architektur/api-endpunkte.md`
  (neuer `search`-Parameter bei `GET /admin/users`).

Live gegen den laufenden Aspire-AppHost verifiziert (2026-08-20): Suche nach
Teil-Benutzername, Teil-E-Mail und vollständiger Benutzer-ID sowie die
Auswahl eines Autocomplete-Vorschlags funktionieren korrekt, keine
Korrekturen nötig.

Offen: `npx prettier --check` meldet Formatierungsabweichungen — allerdings
projektweit, auch bei unveränderten Dateien (z. B. `genres.ts`), vermutlich
CRLF-Zeilenenden unter Windows gegen Prettiers LF-Default; kein durch diesen
Block verursachter Befund. `ng lint` ist in diesem Frontend-Workspace nicht
als Target konfiguriert (kein ESLint eingerichtet), abweichend von der
Befehlsliste in CLAUDE.md §11 — dort bereits als „keine pauschale Freigabe"
relativiert, wird hier als Doku-Abweichung gemeldet.

### 7i. Rate Limiting

Status: **abgeschlossen** (2026-08-26), automatisiert getestet, PR #90,
nach `main` gemergt.
Arbeits-Prompt: `docs/prompts/2026-08-26-block-7i-rate-limiting.md`

Anlass: letzter offener Sicherheitspunkt aus
`wiki/sicherheit/sicherheitskonzept.md` (Zeile 26/133-136) neben CORS-
Production-Whitelist, CSP und der Swagger-UI-Freischaltung für die
Admin-Rolle, nachdem mit Block 10 (Volltext-Suche) der komplette
MVP-Feature-Umfang abgeschlossen war.

Umgesetzt:

- `Program.cs`: `builder.Services.AddRateLimiter(...)` mit einem
  Fixed-Window-Limiter (100 Requests/Minute, `QueueLimit = 0` — sofortige
  Ablehnung statt Warteschlange), partitioniert über den `sub`-Claim aus dem
  JWT (dieselbe Claim-Quelle wie `CurrentUserService`), Fallback-Partition
  `"anonym"` ohne gültiges Token. Der Limiter greift ausschließlich für
  Pfade unter `/api` — Aspires `/health`/`/alive` (nur Development) und
  `/swagger` bleiben unlimitiert, damit das Health-Check-Polling der
  Aspire-Orchestrierung nicht selbst blockiert wird.
- `RejectionStatusCode` explizit auf 429 gesetzt (Default der Middleware ist
  sonst 503); `OnRejected` schreibt einen `ProblemDetails`-Body im Stil von
  `GlobalExceptionHandler.cs` sowie einen `Retry-After`-Header aus den
  Limiter-Metadaten. `app.UseRateLimiter()` sitzt zwischen
  `UseAuthentication()` und `UseAuthorization()`, da die Partitionierung die
  bereits authentifizierte `HttpContext.User` braucht.
- Neuer Integrationstest `RateLimitingTests.cs` (zwei Fälle): ein
  Testbenutzer schöpft das echte Limit von 100 `GET /api/me`-Aufrufen
  innerhalb einer Minute aus, der 101. Aufruf liefert 429 samt
  `Retry-After`-Header; ein zweiter, unabhängiger Testbenutzer bekommt im
  selben Testlauf weiterhin 200 (Partitionierung pro Benutzer). Zweiter
  Testfall: 105 unauthentifizierte Anfragen gegen `/health` bleiben alle
  mit 200 erreichbar (sichert die `/api`-Pfad-Ausnahme ab).
- ADR `docs/adr/0022-rate-limiting.md`: Fixed-Window- vs. Sliding-Window-/
  Token-Bucket-Abwägung, Partitionierungs- und Scoping-Begründung, Form der
  429-Antwort.

Automatisiert getestet: vollständige Testsuite grün — 11 Api.Tests,
114 Domain.Tests, 5 Infrastructure.Tests, 294 Application.Tests und 16
IntegrationTests (davon 2 neu für `RateLimitingTests`), macht 440 Tests
insgesamt. `dotnet format --verify-no-changes` und der Zeilenlängen-Check
(120 Zeichen) sind sauber.

Auf eine zusätzliche manuelle Live-Prüfung gegen einen separat gestarteten
Aspire-AppHost wurde mit dem Projektinhaber bewusst verzichtet: Block 7i
hat keinen Frontend-Anteil, und der Integrationstest deckt bereits den
identischen echten Stack ab (echter AppHost, echtes Postgres/Keycloak,
echte HTTP-Aufrufe) — ein zusätzlicher manueller Lauf hätte denselben Pfad
nur ein zweites Mal geprüft, bei zusätzlichem Risiko verwaister
Docker-Container aus einer Ad-hoc-Session.

Abnahmekriterium:

- Ein Benutzer, der 100 Anfragen pro Minute überschreitet, bekommt HTTP 429;
  andere Benutzer sind davon unabhängig; Aspires Health-Checks und Swagger
  bleiben unlimitiert. **Erfüllt**, automatisiert nachgewiesen (siehe oben).

### 7j. Production-Zugriffsschutz (Swagger, CORS, CSP)

Status: **abgeschlossen** (2026-08-26), automatisiert getestet, live gegen
den laufenden Aspire-AppHost verifiziert, PR #92, nach `main` gemergt.
Arbeits-Prompt: `docs/prompts/2026-08-26-block-7j-production-zugriffsschutz.md`

Anlass: letzte drei offenen Punkte aus
`wiki/sicherheit/sicherheitskonzept.md` (Zeile 26/133-136) nach Block 7i
(Rate Limiting). Iststand-Recherche vor der Umsetzung ergab: Im Repository
existiert keinerlei Production-Infrastruktur (kein Docker Compose, kein
Nginx) — laut Wiki (`projekt/deployment-konzept.md`) ist der
Hosting-Anbieter für Production noch nicht entschieden. Mit dem
Projektinhaber vor Planbeginn geklärt: Der CSP-Anteil dieses Blocks umfasst
ausschließlich die Development/lokal-Variante (Meta-Tag) — die
Production-Variante (HTTP-Header vom Nginx) bleibt bewusst offen und hängt
vom noch nicht begonnenen Production-/Docker-Compose-Setup ab.

Umgesetzt (Backend):

- **Swagger-Admin-Gate außerhalb Development**: `UseSwagger()`/
  `UseSwaggerUI()` laufen jetzt in allen Umgebungen; außerhalb Development
  sitzt davor ein `app.MapWhen(...)`-Zweig auf `/swagger`, der die
  bestehende `"Admin"`-Policy per `IAuthorizationService` prüft (kein
  Token → 401, Token ohne Admin-Rolle → 403, sonst durchgereicht) —
  Swashbuckle 10.0.1 bietet dafür kein Endpoint-Routing wie die übrigen
  Admin-Endpunkte. Details und verworfene Alternativen: ADR
  `docs/adr/0023-swagger-admin-gate-production.md`.
- **CORS-Production-Whitelist**: neue `ProductionCors`-Policy, Origin-Liste
  aus `Cors:AllowedOrigins` (Konfiguration/Umgebungsvariable, Muster wie
  bei Keycloak-Authority/Discogs-Token), Development-Verhalten
  (`Uri.IsLoopback`) unverändert. Bleibt ohne echten Wert, bis eine
  Hosting-Domain feststeht.
- **Nebenbefund und -korrektur**: Beim Schreiben der Integrationstests
  stellte sich heraus, dass `options.RequireHttpsMetadata =
  !builder.Environment.IsDevelopment();` außerhalb Development **jede**
  Anfrage mit 500 beantwortete, weil Keycloaks `Authority` auch dort
  `http://` ist — kein Testartefakt, sondern ein latenter Fehler, der auch
  die echte, später gebaute Production-API getroffen hätte (laut Wiki wird
  TLS nur am Reverse Proxy terminiert, interne Kommunikation bleibt
  unverschlüsselt). Mit ausdrücklicher Freigabe des Projektinhabers behoben
  (`RequireHttpsMetadata = false` unabhängig von der Umgebung). Details:
  ADR `docs/adr/0024-require-https-metadata-produktionsarchitektur.md`.
- Erweiterte Integrationstests: `SwaggerEndpointTests.
  GetSwaggerJson_AusserhalbDevelopmentNurMitAdminRolle` (401/403/200 in
  einem AppHost-Lauf, Muster wie `AdminEndpointsTests`) und
  `CorsPolicyTests.PreflightRequest_AusserhalbDevelopmentNurVon
  WhitelisteterOrigin` (whitelistete und nicht gelistete Origin ebenfalls
  in einem AppHost-Lauf) — die `api`-Ressource wird dafür testweise auf
  `ASPNETCORE_ENVIRONMENT=Production` gesetzt
  (`appHost.CreateResourceBuilder(...)` vor `BuildAsync()`).
- **Nachtrag CI-Timeout**: Der erste Push löste in der CI einen
  Timeout im Schritt „Integrationstests" aus (15-Minuten-Limit,
  `.github/workflows/ci.yml`). Analyse ergab: Schon im letzten
  erfolgreichen Lauf zuvor (Block 7i) brauchte dieser Schritt 12m48s bei
  17 Tests — nur 2m12s Puffer, ein strukturelles, bereits vor Block 7j
  bestehendes Problem (jeder der beiden neuen Integrationstests spinnt
  einen vollständigen eigenen Aspire-AppHost hoch). Mit Freigabe des
  Projektinhabers behoben: `CorsPolicyTests` auf einen statt zwei
  AppHost-Läufe konsolidiert (s. o.) und das CI-Timeout von 15 auf 20
  Minuten angehoben.

Umgesetzt (Frontend, nur Development/lokal):

- **CSP per Meta-Tag**: neues Skript `scripts/write-csp-meta.mjs`
  (eigenständig neben `scripts/write-runtime-config.mjs`, ADR 0009, gleiche
  `prestart`/`prebuild`-Hooks), erzeugt pro Build einen zufälligen Nonce
  und die Direktivenzeile aus `MYMUSIC_API_BASE_URL`/
  `MYMUSIC_KEYCLOAK_AUTHORITY` (`connect-src` per `new URL(...).origin`).
  `index.html` behält als eingecheckte Baseline einen Platzhalter-
  Kommentar sowie `ngCspNonce="__CSP_NONCE__"` (analog zur eingecheckten
  Platzhalter-`runtime-config.json`).
- **Theme-Script ausgelagert**: Inline-Script aus Block 0f (FOUC-
  Vermeidung) liegt jetzt in `public/theme-init.js`
  (`<script src="theme-init.js">`), da `script-src 'self'` Inline-Skripte
  ohne Nonce/Hash blockiert hätte.
- **Nonce für Angular-Inline-Styles**: `<app-root ngCspNonce="...">` nutzt
  Angulars offiziellen `CSP_NONCE`-Mechanismus, damit `style-src 'self'
  'nonce-...'` die zur Laufzeit injizierten Komponenten-Styles nicht
  blockiert (kein `'unsafe-inline'` nötig).
  Details und verworfene Alternativen (Hash-Pinning, `'unsafe-inline'`):
  ADR `docs/adr/0025-csp-meta-tag-development.md`.

Automatisiert getestet: vollständige Backend-Testsuite grün — 114
Domain.Tests, 11 Api.Tests, 5 Infrastructure.Tests, 294 Application.Tests
und 19 IntegrationTests (davon 2 neu für Block 7j), macht 443 Tests
insgesamt. 460 Frontend-Tests (unverändert gegenüber dem Stand vor Block
7j — keine Angular-Komponente betroffen). `dotnet format
--verify-no-changes` sauber. `npx prettier --check .` zeigt
repo-/branch-weite Abweichungen (186 Dateien, überwiegend Zeilenenden-
bedingt) bereits auf dem unveränderten Ausgangsstand — verifiziert per
Vergleich mit zurückgestellten Änderungen (185 Dateien ohne Block 7j),
keine durch diesen Block verursachte Regression, aber auch nicht durch
diesen Block behoben (außerhalb des Scopes).

Live gegen den laufenden Aspire-AppHost verifiziert (Development):
Swagger-UI weiterhin ohne Anmeldung erreichbar (unverändert); vollständiger
Login-Flow (`testuser`) inkl. Rücksprung, Dashboard mit echten Daten,
Theme-Toggle (Light/Dark) und Neuladen ohne sichtbaren Farb-Flash — jeweils
ohne CSP-Verstöße oder sonstige Fehler in der Browser-Konsole.

Bewusst nicht Teil dieses Blocks:

- CSP für Production (HTTP-Header vom Nginx) — abhängig vom noch nicht
  begonnenen Production-/Docker-Compose-Setup, siehe ADR 0025.
- Echte Werte für `Cors:AllowedOrigins` — folgen erst mit einer
  Hosting-Entscheidung.
- Sicherheitstests „nicht authentifiziert, fremde Daten, unbekannte IDs"
  (bleibt eigener, separater Punkt in Abschnitt 7).

Abnahmekriterium:

- Swagger-UI ist außerhalb Development nur mit Admin-Rolle erreichbar; die
  CORS-Whitelist-Mechanik greift für konfigurierte Origins und blockiert
  nicht gelistete; die lokale CSP verhindert keine reguläre Funktion der
  Anwendung. **Erfüllt** für den freigegebenen Umfang (siehe oben);
  CSP-Production bleibt ausdrücklich offen.

### 7k. Benutzerprofil

Status: **abgeschlossen** (2026-08-28), automatisiert getestet und live
verifiziert, PR #94, nach `main` gemergt.
Arbeits-Prompt: `docs/prompts/2026-08-28-block-7k-benutzerprofil.md`

Anlass: Das Benutzerprofil-Modal ist seit dem 2026-08-13 in
`wiki/architektur/navigation-konzept.md` beschrieben (Klick auf Username
öffnet Modal mit Benutzername schreibgeschützt, E-Mail änderbar, Passwort
ändern), wurde bei Block 0g aber explizit aus dem Scope genommen und danach
nie wieder aufgegriffen — es gab weder eine User-Story-Seite noch einen
Eintrag im offenen MVP-Umfang. Die Funktion war dadurch vollständig aus der
Planung verschwunden, nicht bewusst zurückgestellt. Neue Wiki-Seite
`wiki/user-stories/user-stories-benutzerprofil.md` (US-BP1–US-BP3) holt das
nach.

Mit dem Projektinhaber geklärt: E-Mail- und Passwortänderung verlangen keine
erneute Bestätigung des aktuellen Passworts — die aktive Sitzung genügt
(technisch identisch zum bestehenden Admin-Reset über den
Keycloak-Service-Account, hier auf den eigenen Account beschränkt, siehe ADR
0026).

Umgesetzt (Backend):

- `IKeycloakAdminClient`/`KeycloakAdminClient` um `UpdateEmailAsync` und
  `ResetPasswordAsync` erweitert — beide über den bestehenden
  Service-Account `mymusic-admin-service` (keine Keycloak-Realm-Änderung
  nötig, `manage-users` deckt beides bereits ab).
- Neue Commands `UpdateCurrentUserEmailCommand`/`ChangeCurrentUserPasswordCommand`
  in `Features/System/CurrentUser/Commands/` (nicht unter `Verwaltung/Admin`
  — keine Rollenprüfung, nur der eigene Account über
  `ICurrentUserService.UserId`, nie aus dem Request-Body).
- Neue Endpunkte `PUT /api/me/email` und `PUT /api/me/password`
  (`.RequireAuthorization()`, keine `"Admin"`-Policy).
- E-Mail-Konflikt (bereits vergeben, `duplicateEmailsAllowed: false`) wird
  als HTTP 409 über die bestehende, generische `ConflictException`
  übersetzt — keine neue Exception-Klasse. Bewusst keine Behandlung für
  „Keycloak nicht erreichbar" ergänzt, da das Frontend HTTP 502 fest auf die
  Discogs-Fehlermeldung mappt (siehe ADR 0026 und
  `error-modal.service.ts`) — andere Fehler fallen unbehandelt auf den
  bestehenden generischen 500-Fall zurück.

Umgesetzt (Frontend):

- Neuer Ordner `src/app/nav/user-profile/` (co-located mit `Nav`, keine
  eigene Route, daher bewusst nicht unter `features/`): `UserProfileService`
  (Muster `admin.service.ts`) und die `UserProfile`-Komponente — ein Modal
  mit zwei unabhängigen Signal-Forms (E-Mail; neues Passwort +
  Wiederholung mit Cross-Field-Validierung über `validate()`).
  Fehlerbehandlung 1:1 nach dem `genre-form.ts`-Muster: 400 inline am Feld,
  409/Rest über `ErrorModalService`.
- `NavComponent`: Username ist jetzt ein klickbarer Button (öffnet das
  Modal), `OidcUserClaims` um `email` erweitert. Anzeige der aktuellen
  E-Mail über einen lokalen Override-Signal (`emailOverride`), das die
  `UserProfile`-Komponente nach erfolgreicher Änderung direkt mit dem neuen
  Wert befüllt — siehe Live-Befund unten, warum nicht auf
  `OidcSecurityService.forceRefreshSession()` verlassen wird.

Tests: 12 neue Backend-Unit-Tests (Handler + Validator, Application.Tests),
ein neuer Integrationstest `MeProfileEndpointsTests` (401 ohne Token,
E-Mail-Konflikt, erfolgreiche E-Mail-Änderung verifiziert per Login mit der
neuen E-Mail, erfolgreiche Passwortänderung verifiziert per Login mit dem
neuen Passwort) — gegen den echten Aspire-AppHost (Postgres + Keycloak)
gelaufen, grün. 15 neue Frontend-Tests (Vitest), alle 475 Frontend-Tests
grün, `npm run build` erfolgreich.

Neuer ADR 0026 (`docs/adr/0026-benutzerprofil-selfservice-ueber-service-account.md`):
Entscheidung für den bestehenden Service-Account-Weg statt Keycloaks eigener
Account-REST-API.

Bekannte, bewusst nicht in diesem Block behobene Inkonsistenz: Mangels
`passwordPolicy` im Realm erzwingt weder Keycloak selbst noch die
Registrierung (Block 7g) eine Mindestlänge, dieser Block führt aber eine
serverseitige Mindestlänge von 8 Zeichen für die Selbst-Änderung ein. Ein
einheitlicher Keycloak-`passwordPolicy`-Eintrag (wirkt auf Registrierung,
Self-Service und Admin-Reset gleichermaßen) wäre der sauberere,
langfristige Fix — eigener, separat freizugebender Punkt.

Live gegen den laufenden Aspire-AppHost verifiziert (zwei über die
Selbstregistrierung angelegte Wegwerf-Testbenutzer, nach der Prüfung über
den Admin-Bereich wieder gelöscht): Modal zeigt Benutzername und aktuelle
E-Mail korrekt vorbefüllt; E-Mail-Konflikt zeigt die korrekte
Konfliktmeldung, ausdrücklich kein Discogs-Text; Passwort-Validierung
(zu kurz, Wiederholung stimmt nicht überein) inline ohne Server-Aufruf;
Passwortänderung ohne erzwungenen Logout, danach erfolgreicher Neu-Login
mit dem neuen Passwort; `PUT /api/me/email`/`PUT /api/me/password` ohne
Token liefern 401.

Live-Befund und Fix: Nach erfolgreicher E-Mail-Änderung zeigte ein erneutes
Öffnen des Modals weiterhin die alte E-Mail. Der neue Wert war
serverseitig korrekt gespeichert (direkt über die Keycloak Admin REST API
gegengeprüft) — das Problem lag im Frontend. Ob
`OidcSecurityService.forceRefreshSession()` die Claims zuverlässig
nachzieht, ließ sich nicht abschließend isoliert klären (siehe dazu bereits
der offene Punkt im Plan). Statt sich darauf zu verlassen, hält
`NavComponent` jetzt ein lokales Override-Signal, das direkt aus dem
`emailChanged`-Event der `UserProfile`-Komponente befüllt wird — dadurch
unabhängig vom tatsächlichen Token-Refresh-Verhalten, live auch für
mehrfache Änderungen innerhalb derselben Sitzung bestätigt.

Nebenbefund (kein Code-Fix, nur für künftige Live-Prüfungen relevant):
`npm run build` schreibt über die `prebuild`-Skripte
(`write-runtime-config.mjs`, `write-csp-meta.mjs`) dieselben lokalen
Dev-Artefakte (`public/runtime-config.json`, die CSP in `src/index.html`)
neu, die ein parallel laufender `ng serve` (hier über den Aspire-AppHost)
beim eigenen Start bereits mit echten Werten befüllt hat — ein `npm run
build` während einer laufenden Live-Prüfung überschreibt sie mit den
leeren Platzhaltern und bricht dadurch API-/Keycloak-Aufrufe (CSP
`connect-src`). Während einer Live-Prüfung sollte `npm run build`
deshalb vermieden oder die beiden Dateien danach aus dem Git-Stand wieder
hergestellt werden (wie hier geschehen, `git checkout --
public/runtime-config.json src/index.html`).

Nachtrag (2026-08-28, nach PR #94): Die zunächst gewählten Maximallängen
(E-Mail 255 Zeichen, Passwort 100 Zeichen — technisch naheliegende
Standardwerte, siehe Backend-Umsetzung oben) hat der Projektinhaber als
unrealistisch verworfen und auf E-Mail höchstens 120 Zeichen, Passwort
höchstens 32 Zeichen korrigiert (Backend-Validatoren, Frontend-Konstanten
in `user-profile.ts` und zugehörige Tests angepasst, siehe
`wiki/user-stories/user-stories-benutzerprofil.md`).

## 8. Discogs-Integration

Status: **Vollständig abgeschlossen.** Backend-Proxy (Block 8a) umgesetzt,
automatisiert getestet, PR #82 nach `main` gemergt (2026-08-21).
Frontend-Integration (Block 8b) inkl. einer gezielten Erweiterung von
Block 8a um ein Pro-Track-Artist-Feld und die serverseitige Cover-
Einbettung umgesetzt, automatisiert getestet, PR #84 nach `main` gemergt
(2026-08-22). Manuelle Live-Verifikation gegen die echte Discogs-API
durchgeführt (mehrere Runden, siehe „Nachbesserungen" unten) —
abschließend vom Projektinhaber ohne weitere Befunde bestätigt.
Priorität: mittel
Arbeits-Prompt Block 8a: `docs/prompts/2026-08-21-block-8a-discogs-backend-proxy.md`
Arbeits-Prompt Block 8b: `docs/prompts/2026-08-22-block-8b-discogs-frontend.md`

Voraussetzung erledigt:

- User Stories und Akzeptanzkriterien liegen vor, siehe
  `../../02 Wiki/MyMusic Wiki/wiki/user-stories/user-stories-discogs.md`
  (US-DI1–DI4, 2026-08-21).

Umgesetzt (Backend, Block 8a):

- Zwei Endpunkte, serverseitiger Proxy zur Discogs-API:
  `GET /api/discogs/search?q=...` (Kurzdaten-Trefferliste, unpaginiert,
  Suchbegriff mindestens 2 Zeichen) und `GET /api/discogs/releases/{id}`
  (Volldaten: Cover, Tracklist, Artist(s), Label(s), Genre/Style,
  Format-Rohdaten).
- Neuer externer HTTP-Client `IDiscogsClient`/`DiscogsClient`
  (`Application/Common/Services/`, `Infrastructure/ExternalServices/Discogs/`)
  nach Vorbild des `KeycloakAdminClient` (ADR 0016); Authentifizierung über
  einen Discogs Personal Access Token als `Authorization`-Header, statischer
  Aspire-Secret-Parameter `discogs-access-token` (ADR 0018).
- Neue Fehlerklasse `DiscogsUnavailableException`, gemappt auf HTTP 502 Bad
  Gateway (nicht 404/500/503) — deckt Nichterreichbarkeit, Rate-Limit und
  unbekannte Release-IDs einheitlich ab (ADR 0018).
- Neue CQRS-Namenskategorie „Integration"
  (`Application/Features/Integration/`, `Api/Endpoints/Integration/`) — passte
  in keine bestehende Kategorie (Stammdaten/Sammlung/System/Verwaltung).
- Unit-Tests (Application, gemockter `IDiscogsClient` per NSubstitute) und ein
  minimaler Integrationstest (nur 401 ohne Token für beide Endpunkte, kein
  echter Discogs-Aufruf in Tests) — 411 Tests insgesamt, alle grün.
- ADR `docs/adr/0018-discogs-proxy-token-und-fehlerbehandlung.md`.

Bewusst nicht Teil von Block 8a:

- Kein automatisierter Test für `DiscogsClient` selbst gegen die echte
  Discogs-API (externe, ratenlimitierte Drittanbieter-API ohne Sandbox,
  gleiche Einschränkungsklasse wie `KeycloakAdminClient`) — Ausgleich über
  manuelle Live-Verifikation, die noch aussteht.
- Frontend-Integration — umgesetzt mit Block 8b.

Umgesetzt (Backend-Erweiterung + Frontend, Block 8b):

- Backend-Erweiterung an Block 8a: `DiscogsTrackResponse` (und die
  zugrunde liegenden Schichten `DiscogsTrack`/`DiscogsTrackRepresentation`)
  um ein optionales Pro-Track-Artist-Feld erweitert, im `DiscogsClient` aus
  Discogs' Tracklist-`artists`-Array gemappt (nur bei Various-Artists-
  Releases vorhanden, sonst `null`) — siehe ADR
  `docs/adr/0019-discogs-track-artist-zuordnung.md`.
- Neue Komponente `discogs-search/` (verschachteltes Modal im RecordForm):
  Suchfeld (ab 2 Zeichen), Ergebnisliste mit Thumbnail/Titel/Jahr/Label,
  Leer- und Ladezustand, Detailabruf bei Auswahl eines Treffers.
- `RecordForm` übernimmt nach Auswahl eines Discogs-Treffers automatisch
  Albumname, Erscheinungsjahr, Label, Record-Artist und Cover (per `fetch()`
  heruntergeladen und über den bestehenden Cover-Upload-Mechanismus
  gespeichert) sowie — nach dem Speichern — alle Tracks der Discogs-
  Tracklist inkl. geparster Seite/Nummer. Track-Artist folgt dabei der
  Discogs-Realität: bei Various-Artists-Releases der jeweilige
  Pro-Track-Artist, sonst einheitlich der Record-Artist.
- Rückfrage bei neuer Artist-/Label-/Genre-Referenz (US-DI3) einmal je
  distinktem Namen — bei Compilations potenziell mehrfach für Artist, da
  jeder Track seinen eigenen Namen mitbringen kann.
- Neue `getAll()`-Methoden an `ArtistService`/`LabelService` (bereits seit
  Block 6e bestehende, bisher ungenutzte `/all`-Endpunkte) für den
  Existenz-Abgleich; `LabelForm` um `initialName` für die Vorbefüllung bei
  Discogs-Neuanlage erweitert; `ErrorModalService` um den `ErrorModalKind`
  `discogs` (HTTP 502) erweitert — bereits in ADR 0013 als möglicher
  sechster Fall vorgesehen.
- Tests: 397 Backend-Unit-Tests (Domain 114, Api 11, Application 267,
  Infrastructure 5) und 437 Frontend-Tests (nach den Nachbesserungen unten),
  alle grün; `dotnet format` und `prettier --check` sauber.

Nachbesserungen aus der manuellen Live-Verifikation (2026-08-22):

- **Cover-Download umgestellt**: Der ursprünglich geplante client-seitige
  `fetch()` der Discogs-Bild-URL scheiterte live am Hotlink-Schutz von
  Discogs (Cover blieb leer). Fix: `DiscogsClient` lädt das Cover jetzt
  serverseitig über denselben authentifizierten Client herunter und
  bettet es als Base64-Data-URL in die Release-Antwort ein — siehe ADR
  `docs/adr/0020-discogs-cover-serverseitig-eingebettet.md`. Keine
  Frontend-Änderung nötig.
- **Ergebnisliste der Discogs-Suche begrenzt** (`max-h-80` mit
  vertikalem Scrollbereich) — war bei vielen Treffern unbrauchbar groß.
- **Tooltips ergänzt**: Discogs-Button („Nach Discogs-Metadaten suchen")
  und Sucheingabefeld („Discogs durchsuchen").
- **Namensbereinigung für Artist/Label/Genre**: Discogs-Namen können
  Zeichen enthalten, die die jeweiligen Formular-Validierungsmuster nicht
  erlauben (v. a. Disambiguierungs-Suffixe wie „ (2)", Kommas,
  Anführungszeichen). Neues gemeinsames Modul
  `discogs-name-sanitizer.ts` mit drei Funktionen
  (`sanitizeDiscogsArtistName`/`-LabelName`/`-GenreName`, je eigener
  Zeichensatz und Maximallänge passend zum jeweiligen Formular — Genre
  erlaubt anders als Artist/Label kein „." und kein „/"). Greift in
  `resolveArtistId`/`resolveLabelId`/`resolveGenreId` vor dem
  Existenz-Abgleich und vor einer Neuanlage — beim Artist sowohl für den
  Record-Artist als auch für jeden Track-Artist.
- Tests entsprechend ergänzt (Sanitizer-Unit-Tests je Entität plus
  Integrationstests im RecordForm), weiterhin alle grün.
- **Seitenzuordnung bei Einzeltrack-Seiten korrigiert**: Discogs lässt bei
  einer Seite mit nur einem Track die Tracknummer in der Positionsangabe
  weg (z. B. `"A"` statt `"A1"`) — `parseDiscogsPosition` verlangte bisher
  zwingend eine Ziffer und ließ solche Tracks auf Seite „0" mit einer
  zufälligen, vom Array-Index abhängigen Nummer fallen; zwei verschiedene
  Seiten konnten dadurch fälschlich auf derselben „Seite 0" landen (live
  entdeckt an Discogs-Release 91831, „Atmos – Headcleaner": Seite A und C
  hatten je einen Track ohne Ziffer, wurden beide als „Seite 0" mit
  Tracknummer 1 bzw. 4 importiert, Seite C erschien dadurch scheinbar als
  fehlend). Fix: neuer Erkennungsfall für eine reine Seitenangabe ohne
  Ziffer (Tracknummer wird dann implizit 1). Mit einem echten Abruf gegen
  `api.discogs.com/releases/91831` verifiziert, nicht nur angenommen.
  Regressionstest mit genau dieser realen Tracklist ergänzt.

Nachbesserungen aus einem gemeldeten Bugfix (2026-08-31), siehe
`docs/prompts/2026-08-31-fix-discogs-cover-bug.md`:

- **Vorschaubilder in der Trefferliste fehlten**: `discogs-search.html`
  band `result.thumbnailUrl` direkt als externe Discogs-CDN-URL in ein
  `<img src>` ein — vom selben Hotlink-Schutz betroffen, der schon beim
  Release-Cover zum Fix in ADR 0020 geführt hatte, nur nie auf diesen Pfad
  angewendet. Fix: `DiscogsClient.SearchAsync` lädt jetzt auch die
  Thumbnails serverseitig herunter und bettet sie parallel
  (`Task.WhenAll`) als Data-URL ein, analog zum Cover; schlägt der
  Download für einen einzelnen Treffer fehl, bleibt nur dessen
  `thumbnailUrl` `null` (Platzhalter-Icon), die Suche schlägt nicht
  insgesamt fehl.
- **Cover beim Anlegen aus Discogs fehlte**: Ursache war nicht der
  serverseitige Download (der funktionierte), sondern die im selben Zug
  eingeführte Content Security Policy aus Block 7j (2026-08-26, ADR 0025)
  — deren `connect-src` erlaubt kein `data:`, ein `fetch()` auf die vom
  Backend gelieferte Cover-Data-URL (`record-form.ts`,
  `applyDiscogsCover`) wurde dadurch clientseitig blockiert
  (`TypeError: Failed to fetch`), ohne dass Backend oder Nutzer einen
  Hinweis bekamen. Fix: Data-URL wird jetzt ohne `fetch()` direkt in ein
  `File`-Objekt dekodiert (neue Funktion `dataUrlToFile()`), siehe
  ADR 0028. `connect-src` bleibt unverändert (keine Aufweichung der
  CSP-Mindestvorgabe).
- **Nebenbefund beim Live-Test**: Die Cover-Vorschau im RecordForm
  (`URL.createObjectURL()`, `blob:`-URL) war seit Block 7j unabhängig vom
  Discogs-Pfad generell blockiert — `img-src` erlaubte `blob:` nicht.
  Betraf auch den ganz gewöhnlichen manuellen Cover-Upload (Block 6i); die
  eigentliche Speicherung war nicht betroffen, nur die Live-Vorschau vor
  dem Speichern. Fix: `img-src` um `blob:` ergänzt (`write-csp-meta.mjs`,
  `index.html`-Baseline, `wiki/sicherheit/sicherheitskonzept.md`), siehe
  ADR 0025 (Nachtrag 2026-08-31).
- Stiller Catch-Block beim serverseitigen Bild-Download
  (`DownloadImageAsDataUrlAsync`, vormals `DownloadCoverImageAsDataUrlAsync`)
  loggt einen fehlgeschlagenen Download jetzt strukturiert per Serilog
  (`ILogger<DiscogsClient>`, `LogWarning`) statt ihn zu verschlucken.
- Neue Unit-Tests für `DiscogsClient` (`MyMusic.Infrastructure.Tests`,
  erstes `HttpMessageHandler`-Test-Double im Projekt) sowie angepasste
  Frontend-Tests (`record-form.spec.ts`: Cover-Übernahme testet jetzt
  direkt gegen eine Data-URL statt `fetch` zu mocken). Live gegen den
  laufenden Aspire-AppHost verifiziert: Discogs-Suche mit sichtbaren
  Thumbnails, Übernahme eines echten Treffers („Nevermind", Nirvana, DGC,
  1991) inkl. korrekt gespeichertem Cover auf der RecordCard.

Nachbesserung aus einem Korrekturauftrag (2026-08-31), umgesetzt,
automatisiert getestet und live gegen den laufenden Aspire-AppHost
verifiziert, PR #102 (`fix-discogs-referenzen-ohne-rueckfrage`), nach
`main` gemergt, siehe
`docs/prompts/2026-08-31-fix-discogs-referenzen-ohne-rueckfrage.md` und
ADR `docs/adr/0029-discogs-referenzen-ohne-rueckfrage-und-laenderzuordnung.md`:

- **Keine Rückfrage mehr bei neuen Artist-/Genre-Referenzen aus Discogs**:
  Record-Artist, Track-Artist und Genre werden bei fehlendem Treffer jetzt
  ohne Bestätigung automatisch angelegt (neue Methode
  `resolveOrCreateArtistId`, angepasste `resolveGenreId`). Die manuelle
  Artist-Eingabe im RecordForm (Freitext, unabhängig von Discogs) fragt
  weiterhin wie bisher nach — dafür bleibt die bisherige `resolveArtistId`
  unverändert bestehen.
- **Land für neues Label aus Discogs-Daten**: Discogs' Release-Feld
  `country` wird jetzt durch die gesamte Pipeline durchgereicht
  (`DiscogsReleaseRepresentation` → `DiscogsRelease` →
  `DiscogsReleaseResponse`). Neue Frontend-Zuordnungstabelle
  `discogs-country-mapping.ts` (abgeleitet aus den 238 Code/Name-Paaren in
  `country-referenzdaten.md`) übersetzt den englischen Discogs-Ländertext
  in einen vorhandenen `Country`-Datensatz. Gelingt die Zuordnung, wird das
  Label ebenfalls ohne Rückfrage angelegt; sonst öffnet sich unverändert
  das bisherige `LabelForm`-Modal zur manuellen Länderwahl.
- **Discogs-Suchfeld erhält Autofokus** beim Öffnen (`afterNextRender` auf
  ein neues `viewChild`).
- Tests angepasst/ergänzt: Backend (`DiscogsClientTests`,
  `DiscogsResponseBuilderTests`, `GetDiscogsReleaseQueryHandlerTests`, alle
  20 Integrationstests weiterhin grün), Frontend (neue
  `discogs-country-mapping.spec.ts`, `record-form.spec.ts` und
  `discogs-search.spec.ts` angepasst/ergänzt, alle 505 Tests grün,
  Production-Build erfolgreich).
- Wiki korrigiert: `user-stories-discogs.md` (US-DI3) und `discogs-api.md`
  (neuer Abschnitt „Länderzuordnung für neues Label").
- Live gegen den laufenden Aspire-AppHost mit einem echten Discogs-Release
  verifiziert.

Abnahmekriterium:

- Ein Record kann mit Discogs-Vorausfüllung angelegt werden; bei
  Discogs-Ausfall bleibt die manuelle Anlage uneingeschränkt möglich.
  **Vollständig erfüllt** — automatisierte Tests grün und zusätzlich in
  mehreren Runden live gegen die echte Discogs-API verifiziert (u. a.
  anhand des Various-Artists-Release 91831 „Atmos – Headcleaner"), zuletzt
  ohne weitere Befunde. Cover-Übernahme und Trefferlisten-Vorschaubilder
  nach dem Bugfix vom 2026-08-31 erneut live bestätigt.

## 9. Dashboard

Status: Vollständig abgeschlossen. Automatisiert getestet und live gegen
den Aspire-AppHost verifiziert, PR #86 nach `main` gemergt (2026-08-24).
Priorität: mittel bis niedrig

Aufgaben:

- [x] Kennzahlen: Anzahl Records je Format, Top Artists, Top Labels,
  Verteilung nach Erscheinungsjahr (Komponenten gemäß
  Wiki `architektur/angular-projektstruktur.md`).
- [x] Backend: `GET /api/dashboard` (`Features/Sammlung/Dashboard/`),
  Aggregation über eine neue Projektions-Methode `IRepository<T>.
  GetProjectedAsync` (siehe ADR 0021), damit die Aggregation nicht die
  potenziell großen `album_cover`-Daten jedes Records mitlädt.
- [x] Frontend: `DashboardComponent` mit `StatTileComponent` (×4),
  `FormatChartComponent`, `TopArtistsComponent`, `TopLabelsComponent`,
  `YearDistributionComponent` — ersetzt den Platzhalter aus Block 0g.

Abnahmekriterium:

- Das Dashboard zeigt die vier Kennzahlen für die eigene Sammlung korrekt an.
  Automatisierte Tests (Backend: `GetDashboardQueryHandlerTests`,
  `DashboardResponseBuilderTests`; Frontend: Komponenten- und Service-Tests,
  451 grün) sind grün. **Vollständig erfüllt** — zusätzlich live gegen den
  laufenden Aspire-AppHost mit echten Daten verifiziert, nach mehreren
  Korrekturrunden ohne weitere Befunde bestätigt. Dabei aufgedeckt und
  behoben (Wiki-Klärung nach Live-Test, siehe
  `wiki/user-stories/user-stories-dashboard.md`):
  - Die vier Detail-Kacheln waren unterschiedlich groß, weil die
    Angular-Komponente selbst (nicht die `.card`-Div darin) die
    Grid-Zelle bildete — behoben über `host: { class: 'contents' }` an
    allen fünf Dashboard-Kindkomponenten, dazu ein fester Inhaltsbereich
    (Platz für zehn Zeilen) für Format/Top Artists/Top Labels.
  - Lange Artist-/Label-Namen (bis 120/60 Zeichen) wurden in der
    ursprünglichen einzeiligen Darstellung abgeschnitten und unlesbar —
    Name und Balken/Zahl stehen jetzt in zwei Zeilen je Eintrag.
  - Die Jahresverteilung zeigte ursprünglich nur Balken für Jahre mit
    Records nebeneinander (Wiki-Vorgabe zu diesem Zeitpunkt), wodurch
    zeitliche Lücken (z. B. zwischen 1990 und 2004) nicht erkennbar
    waren — korrigiert auf eine lückenlose Balkenreihe vom ersten bis
    zum letzten vorhandenen Jahr (Jahre ohne Records als 0-Balken).
  - Die Jahresverteilung stand ursprünglich in der 2×2-Kachel-Reihe und
    wäre bei einer großen Zeitspanne (z. B. 1950 bis heute) mit vielen
    Balken unlesbar geworden — steht jetzt als eigene volle Zeile unter
    einer 3er-Reihe (Format, Top Artists, Top Labels); Jahres- und
    Anzahl-Beschriftung werden bei vielen Balken automatisch ausgedünnt.

## 10. Volltext-Suche

Status: Vollständig abgeschlossen. Automatisiert getestet und live gegen
den Aspire-AppHost verifiziert, PR #88 nach `main` gemergt (2026-08-26).
Priorität: mittel bis niedrig

Aufgaben:

- [x] Backend: `GET /api/search` (`Features/Sammlung/Search/`) — durchsucht
  ausschließlich Records des angemeldeten Benutzers über Titel, Artist
  (Record- und Track-Artist), Label, Genre (nur über Track) und Land (über
  Label→Country), jeweils per ILIKE-Teilstring (case-insensitive). Kein
  Navigationsproperty auf den Domain-Entities verfügbar — Kriterien werden
  wie beim bestehenden `countryId`-Filter in `GetPagedRecordsQueryHandler`
  über vorab aufgelöste Id-Sets (`IRepository<T>.GetProjectedAsync`)
  kombiniert. Eigene Response-DTOs (`SearchResultResponse`,
  `SearchResultListResponse`, `SearchResponseBuilder`) statt Wiederverwendung
  der Record-Response, um die Feature-Kapselung aus CLAUDE.md 4.3 zu wahren.
  Leerer/fehlender `q`-Parameter liefert 0 Treffer ohne Repository-Zugriff,
  keine serverseitige Validierung von `q` (Queries werden im CQRS-Framework
  nicht validiert).
- [x] Frontend: `SearchService`, `Search`-Komponente ersetzt den Platzhalter
  aus Block 0g — Suchergebnisse als Card-Raster (`RecordCard`) wie die
  normale Records-Ansicht, nicht als Tabelle (Zwischenstand kurz auf
  Tabelle geklärt, siehe [[suche]]-Log — noch am selben Tag zurück auf
  Card-Raster korrigiert: eine Tabelle hätte nur die Record-Kopfdaten
  gezeigt, nicht aber die Tracks, über die ein Treffer per Track-Artist
  oder Genre erst zustande kam). Suchergebnisse sind voll editierbar
  (RecordForm-Modal, Lösch-Bestätigung direkt aus der Card); ein Klick auf
  eine Card navigiert zusätzlich zur Record-Detailansicht inkl. Tracklist.
- [x] Eingabevalidierung am Kopfzeilen-Suchfeld (`NavComponent`): mindestens
  2 Zeichen, Zeichenset identisch zu `Record.AlbumNamePattern`, Inline-
  Fehlermeldung („Die Eingabe muss ...", nicht „Die Suche muss ..."), keine
  Navigation bei Verstoß. Leere Eingabe löst weiterhin keine Aktion aus.
  Prüfung ausschließlich im Frontend (Signal Forms).
  (Korrigiert am 2026-08-30: Die Ergebnisliste lädt wie die Records-Ansicht
  nicht mehr seitenweise nummeriert, sondern per Infinite Scroll automatisch
  nach, siehe docs/prompts/2026-08-30-fix-records-suche-infinite-scroll.md.)

Abnahmekriterium:

- Das MVP-Szenario der Feature-Roadmap ist damit vollständig durchspielbar.
  **Erfüllt.** Automatisierte Tests: 18 neue Application-Tests
  (`GetPagedSearchQueryHandlerTests`, `SearchResponseBuilderTests`), ein
  neuer Integrationstest (`SearchEndpointsTests`, gegen echten
  Aspire-AppHost mit Postgres/Keycloak, deckt alle Suchkriterien und
  Mandantentrennung ab), 9 neue/geänderte Frontend-Tests (`search.spec.ts`,
  `search.service.spec.ts`, Erweiterung `nav.spec.ts`). Live gegen den
  laufenden Aspire-AppHost verifiziert: Suche nach Titel, Record-Artist,
  Track-Artist (weicht vom Record-Artist ab), Genre (nur über Track) und
  Land (über Label) liefert jeweils den erwarteten Treffer; zu kurze
  Eingabe und verbotene Sonderzeichen zeigen die Inline-Fehlermeldung ohne
  Navigation; leere Eingabe löst keine Aktion aus; Bearbeiten und Löschen
  direkt aus der Card-Ansicht funktionieren und aktualisieren die
  Ergebnisliste korrekt; Klick auf eine Card öffnet die Detailansicht
  inklusive Tracklist.

## Dokumentations-Nacharbeit

Status: laufend
Priorität: niedrig, aber vor jedem größeren Commit prüfen

Aufgaben:

- `README.md` und diese `TASK.md` nach jedem abgeschlossenen Block aktualisieren.
- Grundsatzentscheidungen (z. B. Projektnamen der Layer, Production-TLS,
  Production-Secrets) als ADR unter `docs/adr/` festhalten.
- Wiki bei fachlichen Änderungen aktualisieren bzw. Abweichungen melden.

Abnahmekriterium:

- Doku und tatsächlicher Codezustand widersprechen sich nicht.
