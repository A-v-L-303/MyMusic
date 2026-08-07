# Block 5 — Slice Artist (Backend)

## Kontext

Block 4 (Label) ist abgeschlossen und auf `main` gemerged. Laut `TASK.md`
Abschnitt 5 ist Artist der nächste fachliche Slice — nötig, weil Slice 6
(Record/Tracks) `record.artist_id` und `record_track.artist_id`
referenziert und Artist damit Voraussetzung ist. Die Wiki-Vorarbeit für
diesen Slice ist in dieser Sitzung bereits abgeschlossen:
`wiki/user-stories/user-stories-artist.md` (US-A1–US-A5) und
`wiki/domain/artist.md` liegen vor und sind ins GitHub-Wiki übertragen
(Commit `0288ec2`), inklusive der mit dem Projektinhaber geklärten Punkte:

- Name 3–120 Zeichen, Zeichensatz wie Label (`\p{L}\p{N} \-&'./`, ohne
  Klammern), pro Benutzer eindeutig.
- Der laut `api-endpunkte.md` vorgesehene `labelId`-Filter bei
  `GET /artists` wird **in diesem Slice nicht umgesetzt** — `artist` hat
  keine `label_id`-Spalte, die Beziehung existiert erst indirekt über die
  künftige `record`-Tabelle (Slice 6).
- Die Löschreferenzprüfung gegen `record`/`record_track` kann ebenfalls
  erst mit Slice 6 real geprüft werden (beide Tabellen existieren noch
  nicht).

Ziel dieses Blocks ist ausschließlich das Backend (Domain, Infrastructure,
Application, API, Migration, Tests) — analog zu Genre, Country und Label
bleibt das Angular-Feature `artists/` bis Block 0c zurückgestellt.

Architektonisch ist Artist **kein neuer Präzedenzfall**: anders als Label
(erster Slice mit Fremdschlüssel + asynchroner Validierung) hat Artist
weder Fremdschlüssel noch Zusatzfelder. Strukturell ist Artist ein nahezu
unverändertes Duplikat von **Genre** (nicht Label!) — nur mit anderer
Namenslänge (3–120 statt 3–50) und breiterem Zeichensatz (zusätzlich `.`
und `/`, wie bei Label). Genre, nicht Label, ist daher die Kopiervorlage.

## Referenzimplementierung

Als Vorlage dienten (vollständig gelesen): `Genre.cs`,
`GenreConfiguration.cs`, alle Dateien unter `Features/Stammdaten/Genre/`,
`GenreEndpoints.cs`, `Program.cs`, `ApplicationServiceCollectionExtensions.cs`,
`GlobalUsing.cs` (Domain/Application/Api), `IRepository.cs`,
`Repository.cs` sowie alle `*Genre*`-Testdateien und
`GenreEndpointsTests.cs`.

EF-Migrationsbefehl (PowerShell-Pflicht, CLAUDE.md §11):

```powershell
dotnet ef migrations add CreateArtistTable --project src/MyMusic.Infrastructure --startup-project src/MyMusic.Migrator --output-dir Persistence/Migrations
```

## Vorgeschlagene Schritte

### 1. Domain (`MyMusic.Domain`)

Neu: `DomainModels/Stammdaten/Artist/Artist.cs` — 1:1-Kopie von `Genre.cs`
mit angepassten Werten:

```csharp
public const int MinNameLength = 3;
public const int MaxNameLength = 120;
public const string NamePattern = @"^[\p{L}\p{N} \-&'./]+$";
```

`internal Artist(int id, string name, Guid userId)` mit denselben vier
Prüfungen wie `Genre` (leer/zu kurz/zu lang/Zeichensatz), deutsche
Meldungstexte ("Der Name des Artists …", Zeichensatzmeldung inkl. `.` und
`/`). `static Create(name, userId)`; `Update(name)` gibt neue Instanz
zurück (nie `this` mutieren). `Id`/`Name`/`UserId` als `{ get; private
init; }`. Keine Änderung an `IRepository<T>` nötig.

### 2. Infrastructure (`MyMusic.Infrastructure`)

- Neu: `Persistence/Configurations/ArtistConfiguration.cs`
  (`IEntityTypeConfiguration<Artist>`): `ToTable("artist")`; `Id` → `id`;
  `Name` → `artist_name` (`HasMaxLength(Artist.MaxNameLength)`,
  `IsRequired()`); `UserId` → `user_id` (`IsRequired()`);
  `HasIndex(a => new { a.UserId, a.Name }).IsUnique()` (bildet
  `UNIQUE (user_id, artist_name)` ab). Kein `HasOne(...)` — kein
  Fremdschlüssel.
- EF-Migration `CreateArtistTable` wie oben; Up/Down werden von EF
  generiert, keine Handkorrektur (kein Seed, kein FK, anders als Label).
- `GlobalUsing.cs`: `global using
  MyMusic.Domain.DomainModels.Stammdaten.Artist;` ergänzen.

### 3. Application (`MyMusic.Application`)

Struktur unter `Features/Stammdaten/Artist/`, exakte Kopie der
Genre-Struktur (**nicht** Label — kein Fremdschlüssel-Repository, kein
`MustAsync`):

- **Commands**
  - `Create/CreateArtistCommand.cs`: `Name` (`string`), `UserId` (`Guid`,
    `{ get; set; }`, wird serverseitig überschrieben).
  - `Create/CreateArtistCommandValidator.cs`: `Cascade(CascadeMode.Stop)`,
    `NotEmpty`, `MinimumLength(Artist.MinNameLength)`,
    `MaximumLength(Artist.MaxNameLength)`, `Matches(Artist.NamePattern)`,
    deutsche `.WithMessage(...)`.
  - `Create/CreateArtistCommandHandler.cs`: `IRepository<ArtistEntity>`,
    `ExceptionManager`, `ArtistResponseBuilder`. Eindeutigkeitsprüfung wie
    Genre (`GetPagedAsync` mit `page: 1, pageSize: 1` als Existenzcheck →
    `Conflict(...)` bei Treffer), dann `Artist.Create(...)`, `AddAsync`,
    `SaveChangesAsync`, `responseBuilder.Build(artist)`.
  - `Update/UpdateArtistCommand.cs`, `UpdateArtistCommandHandler.cs`,
    `UpdateArtistCommandValidator.cs`: analog Genre — `GetByIdAsync`,
    Ownership-Check → `NotFound("Artist", id)`, Konfliktprüfung unter
    Ausschluss der eigenen `Id`, `existing.Update(name)`,
    `repository.Update(...)`, `SaveChangesAsync`.
  - `Delete/DeleteArtistCommand.cs`: `sealed record
    DeleteArtistCommand(int Id) : ICommand<bool>;`.
    `DeleteArtistCommandHandler.cs`: `IRepository<ArtistEntity>`,
    `ICurrentUserService`, `ExceptionManager`. Ownership-Check →
    `NotFound`. Kommentar (angepasst auf zwei künftige Tabellen):
    ```csharp
    // Eine Referenzprüfung gegen record und record_track entfällt hier
    // bewusst: Die Tabellen entstehen erst mit Slice 6, bis dahin kann
    // kein Record und kein Track einen Artist referenzieren.
    ```
    `repository.Remove(artist)`, `SaveChangesAsync`, Rückgabe `bool`.
- **Queries**
  - `GetById/GetArtistByIdQuery.cs` + Handler: analog `GetGenreByIdQuery`.
  - `GetPaged/GetPagedArtistsQuery.cs`: `sealed record
    GetPagedArtistsQuery(Guid UserId, int Page, int PageSize, string?
    Name) : IQuery<ArtistListResponse>;` — **kein** `LabelId`-Parameter.
    Handler: Filter `a.UserId == query.UserId && (query.Name == null ||
    a.Name.ToLower().Contains(query.Name.ToLower()))`, `OrderBy(a =>
    a.Name)`.
- **ResponseDtos**: `ArtistResponse.cs` (`int Id, string Name`),
  `ArtistListResponse.cs` (analog `GenreListResponse`),
  `Builder/ArtistResponseBuilder.cs` (`Build`/`BuildPaged`, zustandslos).
- `ApplicationServiceCollectionExtensions.cs`:
  `services.AddScoped<ArtistResponseBuilder>();` ergänzen.
- `GlobalUsing.cs`: `ArtistEntity`-Alias (analog `GenreEntity`, ADR 0006)
  sowie die neuen `Features.Stammdaten.Artist.ResponseDtos`/
  `...ResponseDtos.Builder`-Namespaces.

### 4. API (`MyMusic.Api`)

- Neu: `Endpoints/Stammdaten/Artist/ArtistEndpoints.cs`, Muster wie
  `GenreEndpoints.cs`: `MapGroup("/api/artists").RequireAuthorization()`
  mit `GetPagedArtistsAsync` (Parameter `page`, `pageSize`, `name` — kein
  `labelId`), `GetArtistByIdAsync`, `CreateArtistAsync` →
  `Results.Created($"/api/artists/{response.Id}", response)`,
  `UpdateArtistAsync`, `DeleteArtistAsync` → `Results.NoContent()`. Jede
  Endpoint-Methode `private static` mit Pflicht-XML-`<summary>`.
- `Program.cs`: `app.MapArtistEndpoints();` nach `app.MapLabelEndpoints();`.
- `GlobalUsing.cs`: neue Endpoints-/Application-Namespaces ergänzen.

### 5. Tests

- **Domain.Tests**: `ArtistTests.cs` — `Create` mit gültigen Werten; leerer
  Name wirft; Name mit 2 Zeichen (< Minimum) wirft; Name mit 121 Zeichen
  (> Maximum) wirft; verbotenes Zeichen (z. B. Klammer) wirft; erlaubte
  Sonderzeichen `.`, `/`, `-`, `&`, `'` und Umlaute akzeptiert; `Update`
  gibt neue Instanz zurück.
- **Application.Tests**:
  - `CreateArtistCommandHandlerTests`, `UpdateArtistCommandHandlerTests`,
    `DeleteArtistCommandHandlerTests`, `GetArtistByIdQueryHandlerTests`,
    `GetPagedArtistsQueryHandlerTests` — NSubstitute für
    `IRepository<ArtistEntity>`; Mandantentrennung, Konfliktfall,
    Not-Found-Fall.
  - `CreateArtistCommandValidatorTests`, `UpdateArtistCommandValidatorTests`.
  - `ArtistResponseBuilderTests`.
- **Infrastructure.Tests**: keine neuen Artist-spezifischen Tests.
- **IntegrationTests**: `ArtistEndpointsTests.cs` nach Muster
  `GenreEndpointsTests.cs`. Abgedeckt: 401 ohne Token; voller CRUD-Fluss;
  Paginierung; Namensfilter; Sortierung nach Name; Mandantentrennung; 409
  bei doppeltem Namen; 400 bei ungültigem/fehlendem Namen bzw. verbotenem
  Zeichen; 404 bei fremdem/unbekanntem Artist. **Kein** `labelId`-
  Filtertest (out of scope). Neu: `TestSupport/ArtistResponseDto.cs`,
  `TestSupport/ArtistListResponseDto.cs`.

### 6. Dokumentation

- `TASK.md`: Abschnitt 5 auf „Backend abgeschlossen" setzen; Abschnitt 6
  um zwei neue Pflicht-Nachtrag-Punkte ergänzen (Referenzprüfung gegen
  `record`/`record_track` in `DeleteArtistCommandHandler`; `labelId`-
  Filter bei `GetPagedArtistsQuery`/`ArtistEndpoints`); Kopfzeile,
  Abschnitt „Aktuell nicht umgesetzt" und Abschnitt 1 aktualisieren.
- `README.md`: neuen Abschnitt „Artist-Slice (Block 5)" nach
  „Label-Slice (Block 4)" ergänzen; Endpoint-Liste im Abschnitt
  „Swagger/OpenAPI (Block 0e)" um `/api/artists` ergänzen.

## Benötigte NuGet-Pakete

Keine.

## Verifikation

1. `dotnet build` — fehlerfrei.
2. `dotnet test` für Domain/Application/Api/Infrastructure — neue Tests
   grün, bestehende Genre-/Country-/Label-Tests weiterhin grün.
3. `dotnet ef migrations add CreateArtistTable ...` erzeugt Migration mit
   ausschließlich `artist`-Tabelle + `UNIQUE (user_id, artist_name)` (kein
   FK); AppHost-Migrator-Job läuft durch.
4. `MyMusic.IntegrationTests` (inkl. neuer `ArtistEndpointsTests`) grün.
5. `dotnet format --verify-no-changes`, Zeilenlängen-Check.
6. Manueller Stichprobentest: AppHost starten, per Swagger CRUD-Fluss für
   `/api/artists` durchspielen, Namensfilter/Sortierung/Paginierung
   prüfen; verifizieren, dass kein `labelId`-Parameter in Swagger
   erscheint.

## Risiken und offene Punkte

- `labelId`-Filter bei `GET /artists` bewusst zurückgestellt bis Slice 6
  (bereits im Wiki geklärt und als Pflichtpunkt in `TASK.md` Abschnitt 6
  nachzutragen).
- Löschreferenzprüfung bleibt bis Slice 6 ungeprüft — betrifft hier zwei
  künftige Tabellen statt einer (Genre/Label), macht die Slice-6-
  Nachrüstung etwas umfangreicher (zwei Existenzabfragen).
- `GetPagedAsync`s Erfolgsfall ist wie bei Genre/Country/Label nicht
  sinnvoll per NSubstitute unit-testbar — Nachweis über Integrationstest.
- Verwechslungsrisiko beim Kopieren: Genre (nicht Label) ist die
  Vorlage — nur `NamePattern`/`MinNameLength`/`MaxNameLength` und der
  Zwei-Tabellen-Löschkommentar kommen aus dem Label-/Wiki-Kontext.
- Ohne Angular-Frontend ist der Slice nur über HTTP/Swagger nachweisbar.
