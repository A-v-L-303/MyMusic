# Block 2 — Slice Genre (Backend)

## Kontext

Block 0a, 0b und 0d sind abgeschlossen: Solution- und Aspire-Fundament,
CQRS-Eigenframework, generisches Repository (nur gegen gemockten Kontext
getestet), Auth-Smoke-Test `GET /api/me`, CI-Gate für Codequalität. Die
Domain enthält bisher keine Entität, es existiert keine EF-Migration.

Laut `TASK.md` ist Genre der erste fachliche Slice und dient als
Referenzimplementierung für alle folgenden Entitäten (Country, Label, Artist,
Record/Tracks). User Stories mit Akzeptanzkriterien liegen vor
(`wiki/user-stories/user-stories-genre.md`, 2026-07-29).

Mit dem Benutzer geklärt (2026-08-04):

1. **Scope**: Nur Backend (Domain, Application, Infrastructure, API,
   Migration, Tests). Block 0c (Angular-Workspace) existiert nicht
   (`src/frontend/` fehlt) — das Angular-Feature `genres/` wird
   zurückgestellt, bis 0c separat umgesetzt ist.
2. **Ordner-Kategorie**: `Stammdaten` für
   `Domain/DomainModels/Stammdaten/Genre/` und
   `Application/Features/Stammdaten/Genre/` — Konvention gilt auch für die
   künftigen Slices Country, Label, Artist.
3. **Löschprüfung (US-G5)**: `record_track` existiert erst mit Slice 6.
   `DeleteGenreCommandHandler` löscht daher jetzt ohne Referenzprüfung (kann
   aktuell nicht referenziert sein). Ein kurzer, erklärender Kommentar steht
   an dieser Stelle (Ausnahme 1 der Kommentarregel — Verhalten wäre ohne
   Hinweis unerwartet unvollständig). `TASK.md` erhält unter Slice 6 einen
   Pflichtpunkt: Referenzprüfung nachrüsten.

Zwei technische Ist-Stand-Lücken aus 0a/0b werden mit diesem Slice
geschlossen:

- `MyMusic.Api/Program.cs` verdrahtet noch keinen `MyMusicDbContext` und kein
  `IRepository<T>` (in 0b bewusst zurückgestellt).
- `IRepository<T>` hat noch keine `GetPagedAsync`-Methode (im Wiki am
  2026-08-04 konzeptionell entschieden: `repository-pattern.md`,
  `cqrs-framework.md` — Filter-Expression + OrderBy-Delegate + page/pageSize,
  komplett datenbankseitig, kein rohes `IQueryable<T>` nach außen).

## Vorgeschlagene Schritte

### 1. Domain (`MyMusic.Domain`)

- `DomainModels/Stammdaten/Genre/Genre.cs`: `internal`-Konstruktor
  `(int id, string name, Guid userId)` mit Validierung (nicht leer, max. 50
  Zeichen, `ArgumentException` mit deutscher Meldung); `static Create(name,
  userId)`; `Update(name)` gibt neue Instanz zurück; `Id`/`Name`/`UserId` als
  `{ get; private init; }`. Der volle Konstruktor dient EF Core zugleich als
  Materialisierungs-Konstruktor (Constructor Binding).
- `IRepository.cs`: neue Methode
  `Task<(IReadOnlyList<TEntity> Items, int TotalCount)> GetPagedAsync(Expression<Func<TEntity,bool>> filter, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy, int page, int pageSize, CancellationToken cancellationToken)`
  mit vollständigem XML-Dokumentationskommentar (Pflicht).
- Neu: `GlobalUsing.cs` (Domain hat jetzt mehr als eine Datei, Ein-Datei-
  Ausnahme entfällt) mit `global using System.Linq.Expressions;`.

### 2. Infrastructure (`MyMusic.Infrastructure`)

- `Repository.cs`: `GetPagedAsync` baut `Where` → `OrderBy` → `Skip`/`Take`
  auf `_dbSet` auf, materialisiert mit `ToListAsync`, `TotalCount` separat
  per `CountAsync` auf dem gefilterten (nicht paginierten) Query.
- Neu: `Persistence/Configurations/GenreConfiguration.cs`
  (`IEntityTypeConfiguration<Genre>`), explizites Mapping (keine
  Namenskonvention-Bibliothek im Projekt): `ToTable("genre")`, `Id` → Spalte
  `id`, `Name` → Spalte `genre` (`HasMaxLength(50)`, `IsRequired()`),
  `UserId` → Spalte `user_id` (`IsRequired()`),
  `HasIndex(UserId, Name).IsUnique()` (bildet `UNIQUE (user_id, genre)` ab).
- EF-Migration:
  `dotnet ef migrations add CreateGenreTable --project src/MyMusic.Infrastructure --startup-project src/MyMusic.Migrator --output-dir Persistence/Migrations`

### 3. Application (`MyMusic.Application`)

Struktur unter `Features/Stammdaten/Genre/` gemäß `application-layer.md`:

- Commands/Create, Update, Delete (`DeleteGenreCommand` als Record, nur `Id`
  im Konstruktor) je mit Handler; Create/Update zusätzlich mit
  `AbstractValidator<T>` (`NotEmpty`, `MaximumLength(50)`, deutsche
  Meldungen).
- Queries/GetById (`GetGenreByIdQuery(int Id, Guid UserId)`), Queries/GetPaged
  (`GetPagedGenresQuery(Guid UserId, int Page, int PageSize, string? Name)`).
- ResponseDtos: `GenreResponse(int Id, string Name)`,
  `GenreListResponse(IReadOnlyList<GenreResponse> Items, int TotalCount, int
  Page, int PageSize, int TotalPages)`; `GenreResponseBuilder`.

Handler-Reihenfolge (Prüfen → Laden/Erzeugen → Persistieren → Response):

- Create/Update: Namenskonflikt prüfen via `GetPagedAsync` (Filter auf
  `UserId`+`Name`, `pageSize: 1`) → `TotalCount > 0` → `_exception.Conflict`;
  Update schließt den eigenen Datensatz per `Id`-Ungleichheit aus.
- Update/Delete/GetById: Laden per `GetByIdAsync`; `null` oder abweichende
  `UserId` → `_exception.NotFound("Genre", id)` (404, nicht 403).
- Delete: kein Referenz-Check (siehe Klärung Punkt 3), begründender
  Kommentar an dieser Stelle. Rückgabetyp `bool`.
- GetPaged: Filter `UserId` + optionaler `EF.Functions.ILike`-Namensfilter,
  `OrderBy(g => g.Name)`.

`ApplicationServiceCollectionExtensions.cs`: `services.AddScoped<GenreResponseBuilder>();`
ergänzen (Handler/Validatoren laufen über die bestehenden Assembly-Scans).

### 4. API (`MyMusic.Api`)

- Neu: `GenreEndpoints.cs`, `MapGroup("/api/genres").RequireAuthorization()`
  mit `GET` (paginierte Liste, Query-Parameter `page`/`pageSize`/`name`),
  `GET /{id:int}`, `POST`, `PUT /{id:int}`, `DELETE /{id:int}`.
  `UserId` wird in jedem Command/jeder Query nach der Modellbindung aus
  `ICurrentUserService` überschrieben, nie aus dem Client-Body übernommen.
  Jede Endpoint-Methode `private static` mit XML-`<summary>` (Pflicht), kein
  `try-catch`.
- `Program.cs`: `AddDbContext<MyMusicDbContext>` (Connection String
  `mymusicdb`, analog `MyMusic.Migrator/Program.cs`),
  `AddScoped(typeof(IRepository<>), typeof(Repository<>))`,
  `app.MapGenreEndpoints()`.
- `GlobalUsing.cs` (Api) um die neuen Application-/Infrastructure-Namespaces
  ergänzen.

### 5. Tests

- **Domain.Tests**: `GenreTests` — `Create` mit gültigem/leerem/zu langem
  Namen, `Update` liefert neue Instanz mit erhaltener `Id`/`UserId`.
- **Application.Tests**: je Handler eine Testklasse (NSubstitute für
  `IRepository<Genre>`, AAA-Kommentare Pflicht) — Happy Path, 404
  (unbekannt/fremd), 409 (Namenskonflikt), Validator-Tests,
  `GenreResponseBuilderTests`.
- **Infrastructure.Tests**: keine neuen Genre-spezifischen Tests —
  `Repository<T>` bleibt generisch, bestehende `RepositoryTests.cs` deckt
  CRUD-Delegation ab. `GetPagedAsync` ist wie `GetAllAsync` per
  NSubstitute-Mock nicht sinnvoll testbar.
- **IntegrationTests**: `GenreEndpointsTests.cs` nach Muster
  `MeEndpointTests.cs` (echter Aspire-Stack). Keycloak-Test-User-/
  Token-Hilfsmethoden werden nach `TestSupport/KeycloakTestClient.cs`
  extrahiert (Wiederverwendung, keine Duplikation). Abgedeckt: 401 ohne
  Token; Anlegen (400 bei ungültigem Namen, 409 bei Duplikat); Liste mit
  Paginierung/Filter/Sortierung/Mandantentrennung; Get-by-Id eigen/fremd;
  Update eigen/Konflikt/fremd; Delete eigen (204, danach 404)/fremd.

### 6. Dokumentation

- `TASK.md`: Slice 2 präzisieren (Angular zurückgestellt bis 0c), Slice 6 um
  Pflichtpunkt "Referenzprüfung in `DeleteGenreCommandHandler` nachrüsten"
  ergänzen, nach Abschluss auf "abgeschlossen" setzen.
- `wiki/user-stories/user-stories-genre.md`: kurze "Geklärt am
  2026-08-04"-Ergänzung bei US-G5.
- `wiki/architektur/application-layer.md`,
  `wiki/entwicklung/domain-regeln.md`: Kategorie-Konvention `Stammdaten` als
  Beispiel ergänzen.
- Neuer ADR `docs/adr/0006-domain-entity-materialisierung-ef-core.md`.
- `README.md`: bei Bedarf um Genre-Endpunkte ergänzen.

## Benötigte NuGet-Pakete

Keine. `Microsoft.EntityFrameworkCore.Design` liegt bereits im
Migrator-Projekt für `dotnet ef migrations add`.

## Verifikation

1. `dotnet build` — fehlerfrei.
2. `dotnet test` für Domain/Application/Api/Infrastructure — neue Tests grün.
3. `dotnet ef migrations add CreateGenreTable ...` erzeugt Migration;
   AppHost-Start lässt den Migrator-Job erfolgreich durchlaufen.
4. `MyMusic.IntegrationTests` (inkl. neuer `GenreEndpointsTests`) grün.
5. `dotnet format --verify-no-changes`, Zeilenlängen-Check.
6. Manueller Stichprobentest: AppHost starten, per Swagger/`curl` Genre
   anlegen, listen, filtern, bearbeiten, löschen.

## Risiken und offene Punkte

- Ohne Angular-Frontend ist der Slice nur über HTTP/Swagger nachweisbar,
  nicht über eine echte UI — akzeptiert laut Klärung Punkt 1.
- Delete-Referenzprüfung ist bewusst unvollständig bis Slice 6 — Risiko:
  falls Slice 6 verzögert wird, bleibt die Lücke sichtbar in `TASK.md`
  dokumentiert.
- `GetPagedAsync` bleibt ohne Unit-Test-Absicherung der eigentlichen
  Query-Übersetzung — einziger Nachweis ist der Integrationstest.
- Namenskonflikt-Prüfung nutzt `GetPagedAsync` mit `pageSize: 1` als
  Existenzcheck (kein dediziertes `ExistsAsync` auf `IRepository<T>`) — bei
  Bedarf später revidierbar.
