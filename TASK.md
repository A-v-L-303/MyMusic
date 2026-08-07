# Offene Aufgaben

Stand: 2026-08-07 (nach Abschluss von Block 0a, 0b, 0d, 0e, dem Genre-Backend aus
Block 2, dem Country-Backend aus Block 3, dem Label-Backend aus Block 4 und dem
Artist-Backend aus Block 5)
Branch: `block-5-artist`

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

Block 0a, 0b, 0d und 0e sind abgeschlossen. Offen aus dem MVP-Umfang der Phase 1:

- Angular-Workspace (Block 0c).
- CRUD-Slices für Record und Tracks (Genre-, Country-, Label- und
  Artist-Backend erledigt, siehe Abschnitte 2–5; Angular-Features `genres/`,
  `labels/` und `artists/` zurückgestellt bis Block 0c).
- Zustandsbewertung nach Goldmine-Standard.
- Keycloak-Authentifizierung im Code und Mandantentrennung.
- Discogs-Integration, Dashboard und Volltext-Suche.
- User Stories mit Akzeptanzkriterien für Record/Tracks (Genre, Country,
  Label und Artist erledigt, siehe `offene-themen.md` im Wiki).

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

Status: offen
Priorität: mittel

Aufgaben:

- Angular-22-Workspace mit Tailwind CSS und Design-System-Anbindung.
- API-Basis-URL über `runtime-config.json` (Laufzeit, nicht Build-Zeit).
- Einbindung in den AppHost — **Achtung**: `AddNpmApp()` existiert in Aspire 13
  nicht mehr, Ersatz ist `AddJavaScriptApp()` (Wiki-Korrektur ausstehend).

Abnahmekriterium:

- Das Frontend startet über den AppHost.

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

Bewusst nicht Teil von 0e:

- Freischaltung der Swagger-UI in Production für die Admin-Rolle (CLAUDE.md
  §5.3) — das Rollenkonzept (`User`/`Admin`) existiert im Code noch nicht
  (siehe Abschnitt 7). Wird dort nachgezogen, sobald die Admin-Rolle entsteht.

## 1. Planung: User Stories und Akzeptanzkriterien

Status: teilweise abgeschlossen (Genre: 2026-07-29; Country: 2026-08-05;
Label: 2026-08-07; Artist: 2026-08-07; Record/Tracks offen)
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
  - Record/Tracks: vor dem zugehörigen Slice nachzuziehen.
- Die sechs Prüfkriterien der groben Testplanung als Grundlage nutzen.

Abnahmekriterium:

- Für den jeweils nächsten Slice existieren im Wiki Stories mit beobachtbaren,
  testbaren Kriterien, bevor der Arbeits-Prompt erstellt wird.

## 2. Slice: Genre

Status: **Backend abgeschlossen** (2026-08-04); Angular-Feature `genres/`
zurückgestellt bis Block 0c (Angular-Workspace) umgesetzt ist — siehe Klärung
im Arbeits-Prompt `docs/prompts/2026-08-04-block-2-genre.md`.
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

Bewusst nicht Teil dieses Standes:

- Angular-Feature `genres/` (Tabellenansicht, Filterung, Add/Edit als
  Modal) — braucht Block 0c.
- Referenzprüfung gegen `record_track` in `DeleteGenreCommandHandler` (siehe
  Slice 6 unten) — die Tabelle existiert erst dort.

Abnahmekriterium:

- Genres lassen sich anlegen, anzeigen, filtern, bearbeiten und löschen;
  fremde Benutzerdaten sind nicht sichtbar; Tests decken Happy Path,
  Validierung und unbekannte IDs ab. **Backend erfüllt**; die UI-seitigen
  Teile des Kriteriums (Tabellenansicht, Modal) folgen mit dem
  Angular-Feature nach Block 0c.

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

Status: **Backend abgeschlossen** (2026-08-07); Angular-Feature `labels/`
zurückgestellt bis Block 0c (Angular-Workspace) umgesetzt ist — analog Genre.
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

Bewusst nicht Teil dieses Standes:

- Angular-Feature `labels/` (Tabellenansicht, Filterung, Add/Edit als Modal)
  — braucht Block 0c.
- Referenzprüfung gegen `record` in `DeleteLabelCommandHandler` (siehe
  Slice 6 unten) — die Tabelle existiert erst dort, analog zur bereits
  bestehenden Nachtrag-Pflicht für `DeleteGenreCommandHandler`.

Abnahmekriterium:

- Labels lassen sich anlegen, anzeigen, filtern, bearbeiten und löschen;
  fremde Benutzerdaten sind nicht sichtbar; Tests decken Happy Path,
  Validierung und unbekannte IDs ab. **Backend erfüllt**; die UI-seitigen
  Teile (Tabellenansicht, Filter, Modal) folgen mit dem Angular-Feature nach
  Block 0c.

## 5. Slice: Artist

Status: **Backend abgeschlossen** (2026-08-07); Angular-Feature `artists/`
zurückgestellt bis Block 0c (Angular-Workspace) umgesetzt ist — analog
Genre und Label.
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

Bewusst nicht Teil dieses Standes:

- Angular-Feature `artists/` (Tabellenansicht, Filterung, Add/Edit als
  Modal) — braucht Block 0c.
- **Label-Filter bei `GET /artists`**: Laut `api-endpunkte.md` vorgesehen,
  aber fachlich erst mit Slice 6 umsetzbar — die `artist`-Tabelle hat keine
  `label_id`-Spalte, die Beziehung zu Label besteht nur indirekt über
  `record.artist_id → record.label_id`, siehe Wiki `domain/artist.md` und
  `user-stories-artist.md` (US-A2).
- Referenzprüfung gegen `record`/`record_track` in
  `DeleteArtistCommandHandler` (siehe Slice 6 unten) — beide Tabellen
  existieren erst dort, analog zur bereits bestehenden Nachtrag-Pflicht für
  `DeleteGenreCommandHandler`/`DeleteLabelCommandHandler`.

Abnahmekriterium:

- Artists lassen sich anlegen, anzeigen, filtern, bearbeiten und löschen;
  fremde Benutzerdaten sind nicht sichtbar; Tests decken Happy Path,
  Validierung und unbekannte IDs ab. **Backend erfüllt** (Filterung
  vorerst nur nach Name, siehe oben); die UI-seitigen Teile (Tabellenansicht,
  Filter, Modal) folgen mit dem Angular-Feature nach Block 0c.

## 6. Slice: Record und Tracks

Status: offen
Priorität: hoch, fachlicher Kern

Aufgaben:

- Record-CRUD mit Card-Ansicht, Paginierung, Filterung (Name, Artist, Label,
  Erscheinungsjahr, Land) und Sortierung (Name, Erscheinungsjahr, Format).
- Album-Cover hochladen und Vorschau anzeigen.
- Record-Detailansicht mit Track-Liste.
- Track-CRUD in der Detailansicht (Unteransicht, kein eigener Reiter).
- Zustandsbewertung nach Goldmine-Standard (Datenmodell-Erweiterung,
  Wiki `domain/zustandsbewertung.md`).
- **Pflicht (Nachtrag aus Block 2)**: `DeleteGenreCommandHandler`
  (`Application/Features/Stammdaten/Genre/Commands/Delete/`) um die in
  US-G5 beschriebene Referenzprüfung gegen `record_track` ergänzen (HTTP 409,
  wenn noch mindestens ein Track das Genre referenziert). Im Genre-Slice
  bewusst ausgelassen, da `record_track` dort noch nicht existierte — siehe
  `docs/prompts/2026-08-04-block-2-genre.md` und Wiki
  `user-stories/user-stories-genre.md` (US-G5).
- **Pflicht (Nachtrag aus Block 4)**: `DeleteLabelCommandHandler`
  (`Application/Features/Stammdaten/Label/Commands/Delete/`) um die in
  US-L5 beschriebene Referenzprüfung gegen `record` ergänzen (HTTP 409, wenn
  noch mindestens ein Record das Label referenziert). Im Label-Slice bewusst
  ausgelassen, da `record` dort noch nicht existierte — siehe
  `docs/prompts/2026-08-07-block-4-label.md` und Wiki
  `user-stories/user-stories-label.md` (US-L5).
- **Pflicht (Nachtrag aus Block 5)**: `DeleteArtistCommandHandler`
  (`Application/Features/Stammdaten/Artist/Commands/Delete/`) um die in
  US-A5 beschriebene Referenzprüfung gegen `record` **und** `record_track`
  ergänzen (HTTP 409, wenn noch mindestens ein Record oder Track den Artist
  referenziert — zwei Existenzabfragen, da Artist anders als Genre/Label von
  beiden Tabellen referenziert wird). Im Artist-Slice bewusst ausgelassen, da
  `record`/`record_track` dort noch nicht existierten — siehe
  `docs/prompts/2026-08-07-block-5-artist.md` und Wiki
  `user-stories/user-stories-artist.md` (US-A5).
- **Pflicht (Nachtrag aus Block 5)**: `GetPagedArtistsQuery`/
  `GetPagedArtistsQueryHandler`/`ArtistEndpoints`
  (`Application/Features/Stammdaten/Artist/Queries/GetPaged/`,
  `Api/Endpoints/Stammdaten/Artist/`) um einen `labelId`-Filter ergänzen,
  sobald `record.artist_id → record.label_id` existiert (siehe US-A2). Im
  Artist-Slice bewusst ausgelassen, da die Beziehung zu Label nur über die
  hier neu entstehende `record`-Tabelle geprüft werden kann — siehe
  `docs/prompts/2026-08-07-block-5-artist.md` und Wiki
  `user-stories/user-stories-artist.md` (US-A2).

Abnahmekriterium:

- Ein Record mit Tracks und Zustandsbewertung ist vollständig anleg-, filter-,
  sortier-, bearbeit- und löschbar.

## 7. Authentifizierung und Mandantentrennung

Status: teilweise offen
Priorität: hoch; JWT-Validierung ist bereits im Walking Skeleton entstanden

Ziel:

- Vollständige Umsetzung des Sicherheitskonzepts
  (Wiki `sicherheit/sicherheitskonzept.md`).

Bereits umgesetzt (Block 0b, siehe oben):

- `AddAuthentication().AddJwtBearer()` gegen die Keycloak-Authority,
  `ICurrentUserService` liest den `sub`-Claim, einmal nachgewiesen per
  Integrationstest (`/api/me`).

Aufgaben (noch offen):

- Angular-Login-Flow (Authorization Code + PKCE) inkl. AuthGuard und
  HTTP-Interceptor.
- Rollen (`User`, `Admin`), Ownership-Prüfung in Handlern (404 statt 403) —
  setzt Entitäten voraus, entsteht mit dem jeweiligen Slice.
- Swagger-UI in Production für die Admin-Rolle freischalten (CLAUDE.md §5.3,
  zurückgestellt aus Block 0e, siehe
  `docs/adr/0007-swagger-openapi-nur-development.md`).
- Rate Limiting (100 req/min pro Benutzer), CORS-Policy per Environment, CSP.
- Admin-Bereich: Benutzer inkl. aller Daten löschen (`/admin`, nur Rolle Admin).
- Sicherheitstests: nicht authentifiziert, fremde Daten, unbekannte IDs.

Abnahmekriterium:

- Ohne Login ist kein fachlicher Endpunkt erreichbar; Benutzer sehen
  ausschließlich eigene Daten; der Admin kann Benutzer löschen.

## 8. Discogs-Integration

Status: offen
Priorität: mittel

Aufgaben:

- Serverseitiger Proxy `/discogs/search` (Wiki `tech-stack/discogs-api.md`).
- Metadaten-Suche beim Anlegen eines Records; Treffer als Vorausfüllung,
  manuell editierbar.
- Fehlerdarstellung gemäß Fehlerkonzept (Modal mit Hinweis auf manuelle Eingabe).

Abnahmekriterium:

- Ein Record kann mit Discogs-Vorausfüllung angelegt werden; bei
  Discogs-Ausfall bleibt die manuelle Anlage uneingeschränkt möglich.

## 9. Dashboard

Status: offen
Priorität: mittel bis niedrig

Aufgaben:

- Kennzahlen: Anzahl Records je Format, Top Artists, Top Labels,
  Verteilung nach Erscheinungsjahr (Komponenten gemäß
  Wiki `architektur/angular-projektstruktur.md`).

Abnahmekriterium:

- Das Dashboard zeigt die vier Kennzahlen für die eigene Sammlung korrekt an.

## 10. Volltext-Suche

Status: offen
Priorität: mittel bis niedrig

Aufgaben:

- Globale Suche über Records, Artists und Labels in kombinierter Ansicht
  (`/search?q=...`).

Abnahmekriterium:

- Das MVP-Szenario der Feature-Roadmap ist damit vollständig durchspielbar.

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
