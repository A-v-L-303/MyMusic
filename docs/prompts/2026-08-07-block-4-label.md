# Block 4 — Slice Label (Backend)

## Kontext

Block 3 (Country) ist abgeschlossen; Block 2 (Genre) dient weiterhin als
Referenzimplementierung für Architektur und Konventionen. Laut `TASK.md`
Abschnitt 4 ist Label der nächste fachliche Slice. Die Voraussetzung aus
Abschnitt 1 ("User Stories vor dem zugehörigen Slice") ist in dieser Sitzung
erfüllt worden: `wiki/user-stories/user-stories-label.md` (US-L1 bis US-L5,
2026-08-07) liegt vor, ebenso die dafür nötigen Klärungen mit dem
Projektinhaber (Namenseindeutigkeit, Mindestlänge, Zeichenset, Verhalten bei
ungültigem Land) — bereits im Wiki dokumentiert und ins GitHub-Wiki
übertragen (Commit `f51fbc2`).

Label unterscheidet sich von Genre und Country dadurch, dass es der erste
Slice mit einer **Fremdschlüsselbeziehung zu einer anderen Stammdaten-Entität**
ist (`label.country_id → country.id`, `NOT NULL`). Das erfordert zwei neue,
in Genre/Country noch nicht vorhandene Muster:

1. Eine asynchrone Validierung gegen die Datenbank (Existenz des Landes) in
   FluentValidation.
2. Eine EF-Core-Fremdschlüsselkonfiguration zwischen zwei Domain-Entitäten,
   ohne die für dieses Projekt geltende DDD-Regel zu verletzen, dass eine
   Domain-Entität keine Navigationseigenschaft auf eine andere Aggregate
   besitzt (nur `CountryId` als Wert, keine `Country`-Referenz).

Scope dieses Blocks: **nur Backend** (Domain, Infrastructure, Application,
API, Migration, Tests) — analog zu Genre und Country wird das
Angular-Feature `labels/` zurückgestellt, bis Block 0c (Angular-Workspace)
existiert.

## Referenzimplementierung

Als Vorlage dienten (vollständig gelesen): `Genre.cs`, `GenreConfiguration.cs`,
`CountryConfiguration.cs`, Migration `CreateGenreTable`, alle Dateien unter
`Features/Stammdaten/Genre/`, `ExceptionManager.cs`,
`CommandValidationDecorator.cs`, `ApplicationServiceCollectionExtensions.cs`,
`GenreEndpoints.cs`, `Program.cs`, `IRepository.cs` sowie alle
`*Genre*`/`*Country*`-Testdateien.

## Vorgeschlagene Schritte

### 1. Domain (`MyMusic.Domain`)

- `DomainModels/Stammdaten/Label/Label.cs`: `internal`-Konstruktor
  `(int id, string name, int countryId, string? information, Guid userId)`.
  - `MinNameLength = 1`, `MaxNameLength = 60`,
    `NamePattern = @"^[\p{L}\p{N} \-&'./]+$"` (wie Genre, zusätzlich `.` und
    `/`, bewusst ohne Klammern — Klärung 2026-08-07).
  - `Information`: optional; wenn nicht `null`/leer, `MaxLength(255)`
    geprüft (`ArgumentException`, analog zu den übrigen Feldregeln).
  - `CountryId`: nur Plausibilitätsprüfung (`> 0`) — die eigentliche
    Existenzprüfung ist eine Datenbankabfrage und gehört nicht in die
    Domain (siehe Application-Layer, Punkt 3).
  - `static Create(name, countryId, information, userId)`; `Update(name,
    countryId, information)` gibt neue Instanz zurück (nie `this` mutieren,
    wie bei Genre).
  - `Id`/`Name`/`CountryId`/`Information`/`UserId` als `{ get; private
    init; }`. **Keine** `Country`-Navigationseigenschaft (DDD-Aggregatgrenze,
    Analog-Entscheidung zu Genre/Country, die auch keine Navigation
    zueinander haben).

### 2. Infrastructure (`MyMusic.Infrastructure`)

- Neu: `Persistence/Configurations/LabelConfiguration.cs`
  (`IEntityTypeConfiguration<Label>`): `ToTable("label")`; `Id` → `id`;
  `Name` → `label_name` (`HasMaxLength(60)`, `IsRequired()`); `CountryId` →
  `country_id` (`IsRequired()`); `Information` → `information`
  (`HasMaxLength(255)`, optional); `UserId` → `user_id` (`IsRequired()`);
  `HasIndex(l => new { l.UserId, l.Name }).IsUnique()` (analog Genre).
  - **Neues Muster**: Fremdschlüssel ohne CLR-Navigation:
    `builder.HasOne<CountryEntity>().WithMany().HasForeignKey(l =>
    l.CountryId).OnDelete(DeleteBehavior.Restrict);` — `Restrict` bewusst
    explizit gesetzt, weil EF Cores Default für Pflicht-Beziehungen
    `Cascade` ist (würde beim Löschen eines Landes referenzierte Labels
    mitlöschen; Country hat aktuell zwar keinen Delete-Endpoint, aber die
    DB-Konsistenz soll das nicht stillschweigend dem EF-Default überlassen).
- EF-Migration:
  `dotnet ef migrations add CreateLabelTable --project src/MyMusic.Infrastructure --startup-project src/MyMusic.Migrator --output-dir Persistence/Migrations`
  (Up/Down werden von EF generiert, wie bei Genre/Country — keine
  Handkorrektur nötig, da kein Seed wie bei Country).
- `GlobalUsing.cs`: `global using MyMusic.Domain.DomainModels.Stammdaten.Label;` ergänzen.

### 3. Application (`MyMusic.Application`)

Struktur unter `Features/Stammdaten/Label/`, Muster wie Genre:

- **Commands**
  - `Create/CreateLabelCommand.cs`: `Name`, `CountryId`, `Information`
    (`string?`), `UserId` (analog `CreateGenreCommand`).
  - `Create/CreateLabelCommandValidator.cs`: Name-Regeln wie Genre
    (Pflicht, 1–60 Zeichen, `NamePattern`); zusätzlich **neues Muster**:
    `RuleFor(c => c.CountryId).MustAsync(async (countryId, ct) => await
    countryRepository.GetByIdAsync(countryId, ct) is not null).WithMessage("Das
    angegebene Land existiert nicht.")` — Validator erhält
    `IRepository<CountryEntity>` per Constructor Injection; `Information`:
    `MaximumLength(255)` wenn gesetzt.
  - `Create/CreateLabelCommandHandler.cs`: Abhängigkeiten
    `IRepository<LabelEntity>`, `IRepository<CountryEntity>`,
    `ExceptionManager`, `LabelResponseBuilder`. Eindeutigkeitsprüfung wie
    Genre (`GetPagedAsync` mit `UserId == ... && Name == ...`, `page: 1,
    pageSize: 1`) → `Conflict` bei Treffer. Nach `Create` +
    `AddAsync`/`SaveChangesAsync`: lädt den Ländernamen per
    `countryRepository.GetByIdAsync(label.CountryId, ct)` und ruft
    `responseBuilder.Build(label, country!.Name)`.
  - `Update/…` analog Genre (`existingLabel.Update(...)`), inkl. derselben
    Eindeutigkeits- und Landes-Validierung (auch beim Bearbeiten, siehe
    US-L4).
  - `Delete/DeleteLabelCommandHandler.cs`: analog
    `DeleteGenreCommandHandler`, inkl. desselben Kommentars zur (noch)
    fehlenden Referenzprüfung gegen `record` (Tabelle existiert erst mit
    Slice 6 — siehe `user-stories-label.md`, US-L5-Nachtrag).
- **Queries**
  - `GetById/GetLabelByIdQuery.cs` + Handler: analog Genre, zusätzlich
    Country-Namen-Auflösung wie oben.
  - `GetPaged/GetPagedLabelsQuery.cs`: `(Guid UserId, int Page, int
    PageSize, string? Name, int? CountryId)`. Handler filtert in
    `GetPagedAsync` zusätzlich `(query.CountryId == null || label.CountryId
    == query.CountryId)`. Zur Länder-Namen-Auflösung für die gesamte Seite:
    einmaliger `countryRepository.GetAllAsync()`-Aufruf (Country ist eine
    kleine, vollständig zwischenspeicherbare Referenztabelle mit 238
    Zeilen — kein N+1-Problem), daraus `Dictionary<int, string>`
    `Id → Name`, damit pro `Label` der Name nachgeschlagen wird.
- **ResponseDtos**
  - `LabelResponse.cs`: `(int Id, string Name, int CountryId, string
    CountryName, string? Information)`.
  - `LabelListResponse.cs`: analog `GenreListResponse`.
  - `Builder/LabelResponseBuilder.cs`: bleibt zustandslose Mapping-Klasse
    wie `GenreResponseBuilder`; `Build(LabelEntity label, string
    countryName)` und `BuildPaged(IReadOnlyList<LabelEntity> labels,
    IReadOnlyDictionary<int, string> countryNamesById, int totalCount, int
    page, int pageSize)`.
- `ApplicationServiceCollectionExtensions.cs`: `services.AddScoped<LabelResponseBuilder>();` ergänzen.
- `GlobalUsing.cs`: `LabelEntity`-Alias (`= MyMusic.Domain.DomainModels.Stammdaten.Label.Label`,
  analog `GenreEntity`/`CountryEntity`, ADR 0006) sowie die neuen
  `Features.Stammdaten.Label.ResponseDtos`/`...ResponseDtos.Builder`-Namespaces.

### 4. API (`MyMusic.Api`)

- Neu: `Endpoints/Stammdaten/Label/LabelEndpoints.cs`, Muster wie
  `GenreEndpoints.cs`: `MapGroup("/api/labels").RequireAuthorization()` mit
  `GetPagedLabelsAsync` (Query-Parameter `page`, `pageSize`, `name`,
  `countryId`, gleiche Normalisierung wie bei Genre), `GetLabelByIdAsync`,
  `CreateLabelAsync` (`Results.Created($"/api/labels/{response.Id}",
  response)`), `UpdateLabelAsync`, `DeleteLabelAsync`
  (`Results.NoContent()`). Jede Endpoint-Methode `private static` mit
  Pflicht-XML-`<summary>`.
- `Program.cs`: `app.MapLabelEndpoints();` nach `app.MapCountryEndpoints();`.
- `GlobalUsing.cs` (Api): neue Endpoints-/Application-Namespaces ergänzen.

### 5. Tests

- **Domain.Tests**: `LabelTests.cs` — `Create` mit gültigen Werten;
  leerer/zu langer/zu kurzer Name wirft; verbotenes Zeichen (inkl. Klammer)
  wirft; erlaubte Sonderzeichen (`.`, `/`, `-`, `&`, `'`) akzeptiert;
  `Information` zu lang wirft, `null`/leer erlaubt; `Update` gibt neue
  Instanz zurück.
- **Application.Tests**:
  - `CreateLabelCommandHandlerTests`, `UpdateLabelCommandHandlerTests`,
    `DeleteLabelCommandHandlerTests`, `GetLabelByIdQueryHandlerTests`,
    `GetPagedLabelsQueryHandlerTests` — NSubstitute für
    `IRepository<LabelEntity>` **und** `IRepository<CountryEntity>` (neu
    gegenüber Genre); Mandantentrennung, Konfliktfall, Not-Found-Fall,
    Country-Namen-Auflösung in der Response.
  - `CreateLabelCommandValidatorTests`, `UpdateLabelCommandValidatorTests`:
    inkl. **neuem Testfall** für die asynchrone Länder-Existenzprüfung.
  - `LabelResponseBuilderTests`.
- **Infrastructure.Tests**: keine neuen Label-spezifischen Tests.
- **IntegrationTests**: `LabelEndpointsTests.cs` nach Muster
  `GenreEndpointsTests.cs`. Abgedeckt: 401 ohne Token; voller CRUD-Fluss;
  Paginierung; Filter nach Name und `countryId` (einzeln + kombiniert);
  Sortierung nach Name; 409 bei doppeltem Namen; 400 bei ungültigem/
  fehlendem `countryId` bzw. verbotenem Zeichen; 404 bei fremdem/
  unbekanntem Label. Neu: `TestSupport/LabelResponseDto.cs`,
  `TestSupport/LabelListResponseDto.cs`.

### 6. Dokumentation

- `TASK.md`: Abschnitt 4 auf „Backend abgeschlossen" setzen.
- `README.md`: Abschnitt „Label-Slice (Block 4)" ergänzen.

## Benötigte NuGet-Pakete

Keine.

## Verifikation

1. `dotnet build` — fehlerfrei.
2. `dotnet test` für Domain/Application/Api/Infrastructure — neue Tests grün.
3. `dotnet ef migrations add CreateLabelTable ...` erzeugt Migration mit
   FK-Constraint `country_id → country(id)` und `ON DELETE RESTRICT`;
   AppHost-Migrator-Job läuft durch.
4. `MyMusic.IntegrationTests` (inkl. neuer `LabelEndpointsTests`) grün.
5. `dotnet format --verify-no-changes`, Zeilenlängen-Check.
6. Manueller Stichprobentest: AppHost starten, per Swagger CRUD-Fluss für
   `/api/labels` durchspielen, Filter nach `countryId` und `name` prüfen.

## Risiken und offene Punkte

- Erste Fremdschlüsselbeziehung zwischen zwei Stammdaten-Entitäten in
  diesem Projekt — Muster (FK ohne CLR-Navigation, explizites
  `OnDelete(DeleteBehavior.Restrict)`) ist eine Implementierungsentscheidung
  ohne Wiki-Vorgabe.
- `GetPagedAsync`s Erfolgsfall ist wie bei Genre/Country nicht sinnvoll per
  NSubstitute unit-testbar — Nachweis über Integrationstest.
- Die Referenzprüfung beim Löschen (`record.label_id`) bleibt bis Slice 6
  ungeprüft (siehe Wiki-Nachtrag).
- Country-Namen-Auflösung per zusätzlichem Repository-Aufruf ist eine
  Implementierungsentscheidung — Alternative wäre, nur `CountryId`
  zurückzugeben und die Namensauflösung dem Frontend zu überlassen.
  Empfehlung: serverseitige Auflösung, da dadurch die API selbsterklärend
  bleibt und Swagger/Integrationstests ohne zusätzlichen Join testbar sind.
