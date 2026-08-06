# Block 3 — Slice Country (Backend)

## Kontext

Block 2 (Genre) ist abgeschlossen und dient als Referenzimplementierung für
Architektur und Konventionen. Laut `TASK.md` Abschnitt 3 ist Country der
nächste fachliche Slice. User Stories mit Akzeptanzkriterien liegen vor
(`wiki/user-stories/user-stories-country.md`, US-C1, 2026-08-05) — nur eine
Story, da Country anders als Genre keinen eigenen Reiter und keine CRUD-Maske
hat: eine globale Referenztabelle ohne `user_id`, nur `GET /api/countries`
mit der vollständigen, alphabetisch sortierten Länderliste (238 Einträge,
einmalig per Migration geseedet aus `wiki/domain/country-referenzdaten.md`).

Mit dem Benutzer geklärt (2026-08-05):

1. **Scope**: Nur Backend (Domain, Infrastructure, Application, API,
   Migration, Tests). Kein Angular-Frontend — Block 0c existiert noch nicht,
   genau wie bei Genre zurückgestellt.
2. **Sortierung**: Alphabetisch nach Landesname (war in keiner Wiki-Seite
   dokumentiert, mit dem Benutzer geklärt und in
   `user-stories-country.md` nachgetragen).
3. **`GetAll` statt `GetPaged`**: Country ist der erste Slice, der das in
   `cqrs-framework.md` bereits skizzierte, bislang ungenutzte `GetAll`-Muster
   umsetzt — Rückgabetyp `IEnumerable<CountryResponse>` direkt, kein
   `CountryListResponse`-Wrapper (`cqrs-framework.md`: „Rückgabetyp GetAll:
   IEnumerable<T>, nicht List<T>"; die Paginierungs-Response-Form gilt laut
   `api-endpunkte.md` nur für paginierte Listen, `GET /countries` ist davon
   ausdrücklich ausgenommen).
4. **`GetAllCountriesQuery` ohne `userId`**: `cqrs-framework.md` Zeile 32
   verlangt pauschal `userId` für jede `GetAll`-Query, widerspricht sich
   damit aber selbst mit Zeile 43 („`GetAll` bleibt unverändert für dauerhaft
   unpaginierte Fälle wie `GET /countries`") und mit `tabellenschema.md`
   („Referenztabelle. Kein `user_id`"). Country hat keinen Mandantenbezug —
   die Query bleibt parameterlos. Der Widerspruch wird nicht stillschweigend
   aufgelöst, sondern als Ausnahme in `cqrs-framework.md` nachgetragen.
5. **Keine Regex-/Zeichensatz-Validierung** für `Name`/`Code` (anders als
   Genre). Die Referenzliste enthält bewusst nicht-ISO-konforme Werte
   (`YU`, `---`) — eine Zeichensatzregel würde diese zu Unrecht ablehnen. Nur
   Pflichtfeld + Maximallänge (`VARCHAR(50)`/`VARCHAR(3)`) werden geprüft.
6. **Sortier-Implementierung**: `StringComparer.InvariantCulture` im
   Query-Handler nach `IRepository<T>.GetAllAsync()` (keine Wiki-Vorgabe,
   bewusste Implementierungsentscheidung — kein vollständiges deutsches
   Duden-Alphabet, aber deterministisch über Umgebungen hinweg).

## Vorgeschlagene Schritte

### 1. Domain (`MyMusic.Domain`)

- `DomainModels/Stammdaten/Country/Country.cs`: `internal`-Konstruktor
  `(int id, string name, string code)` mit Validierung (Name/Code nicht
  leer, Name max. 50, Code max. 3 Zeichen, `ArgumentException` mit deutscher
  Meldung, keine Regex); `static Create(name, code)`; **kein** `Update()`
  (wird nie mutiert); `Id`/`Name`/`Code` als `{ get; private init; }`.

### 2. Infrastructure (`MyMusic.Infrastructure`)

- Neu: `Persistence/Configurations/CountryConfiguration.cs`
  (`IEntityTypeConfiguration<Country>`): `ToTable("country")`, `Id` → Spalte
  `id`, `Name` → Spalte `country_name` (`HasMaxLength(50)`, `IsRequired()`),
  `Code` → Spalte `country_code` (`HasMaxLength(3)`, `IsRequired()`),
  `HasIndex(Code).IsUnique()`.
- EF-Migration:
  `dotnet ef migrations add CreateCountryTable --project src/MyMusic.Infrastructure --startup-project src/MyMusic.Migrator --output-dir Persistence/Migrations`,
  danach `Up()` von Hand um `migrationBuilder.InsertData(table: "country",
  columns: new[] { "country_name", "country_code" }, values: ...)` mit allen
  238 Zeilen aus `wiki/domain/country-referenzdaten.md` ergänzen (`id`-Spalte
  ausgelassen, Postgres vergibt die Identity automatisch). `Down()` bleibt
  ein einfaches `DropTable` (entfernt die Seed-Zeilen mit).
- `GlobalUsing.cs`: `global using MyMusic.Domain.DomainModels.Stammdaten.Country;`
  ergänzen (kein Alias nötig, keine Namenskollision in Infrastructure).

### 3. Application (`MyMusic.Application`)

Struktur unter `Features/Stammdaten/Country/`:

- `Queries/GetAll/GetAllCountriesQuery.cs`:
  `sealed record GetAllCountriesQuery : IQuery<IEnumerable<CountryResponse>>;`
  (keine Parameter).
- `Queries/GetAll/GetAllCountriesQueryHandler.cs`: Abhängigkeiten nur
  `IRepository<CountryEntity>` + `CountryResponseBuilder` (kein
  `ExceptionManager` — kein Not-Found-Fall, analog
  `GetPagedGenresQueryHandler`). Lädt per `GetAllAsync`, sortiert per
  `OrderBy(c => c.Name, StringComparer.InvariantCulture)`, mappt per
  `responseBuilder.Build`.
- `ResponseDtos/CountryResponse.cs`:
  `sealed record CountryResponse(int Id, string Name, string Code);`
- `ResponseDtos/Builder/CountryResponseBuilder.cs`: `Build(CountryEntity) => CountryResponse`.
- `ApplicationServiceCollectionExtensions.cs`:
  `services.AddScoped<CountryResponseBuilder>();` ergänzen (Handler laufen
  über den bestehenden Assembly-Scan).
- `GlobalUsing.cs`: `CountryEntity`-Alias
  (`= MyMusic.Domain.DomainModels.Stammdaten.Country.Country`, analog
  `GenreEntity`, siehe ADR 0006) sowie die neuen
  `Features.Stammdaten.Country.ResponseDtos`- und `...ResponseDtos.Builder`-
  Namespaces ergänzen.

### 4. API (`MyMusic.Api`)

- Neu: `Endpoints/Stammdaten/Country/CountryEndpoints.cs`:
  `MapGroup("/api/countries").RequireAuthorization()` mit einer Route
  `MapGet(string.Empty, GetAllCountriesAsync)`. Endpoint-Methode
  `private static`, XML-`<summary>` (Pflicht), Signatur nur
  `(IMediator mediator, CancellationToken cancellationToken)` — kein
  `ICurrentUserService`, da keine `userId` involviert ist.
- `Program.cs`: `app.MapCountryEndpoints();` nach `app.MapGenreEndpoints();`.
- `GlobalUsing.cs` (Api): neue Endpoints-/Application-Namespaces ergänzen.

### 5. Tests

- **Domain.Tests**: `CountryTests` — `Create` mit gültigem Namen/Code;
  leerer/zu langer Name wirft; leerer/zu langer Code wirft; `[Theory]` mit
  `"YU"`, `"---"` als Code beweist explizit, dass keine Zeichensatzregel
  existiert. `GlobalUsing.cs`: `CountryEntity`-Alias ergänzen.
- **Application.Tests**: `GetAllCountriesQueryHandlerTests` (NSubstitute für
  `IRepository<CountryEntity>`, unsortierte Eingabe → alphabetisch sortierte
  Ausgabe, korrektes Mapping, leere Liste → leere Liste),
  `CountryResponseBuilderTests`. `GlobalUsing.cs`: `CountryEntity`-Alias und
  neue Namespaces ergänzen.
- **Infrastructure.Tests**: keine neuen Country-spezifischen Tests —
  `RepositoryTests.cs` bleibt generisch (`TestEntity`-Double), wie bei
  Genre.
- **IntegrationTests**: `CountryEndpointsTests.cs` nach Muster
  `GenreEndpointsTests.cs` (echter Aspire-Stack, `KeycloakTestClient` für
  Auth). Abgedeckt: 401 ohne Token; 200 mit Token, Anzahl == 238,
  Liste ist alphabetisch sortiert (`StringComparer.InvariantCulture`),
  Stichprobe (z. B. „Deutschland"/„DE" vorhanden) statt Vollvergleich. Neu:
  `TestSupport/CountryResponseDto.cs` (`record CountryResponseDto(int Id,
  string Name, string Code)`).

### 6. Dokumentation

- `wiki/architektur/cqrs-framework.md`: Ausnahme-Zeile nach der
  `userId`-Regel ergänzen (siehe Klärung Punkt 4).
- `TASK.md`: Abschnitt 3 auf „Backend abgeschlossen" setzen,
  Umsetzt-/Nicht-Teil-Liste analog Genre; Kopfzeile „Branch:" aktualisieren.
- `README.md`: Abschnitt „Country-Slice (Block 3)" nach dem bestehenden
  Genre-Abschnitt ergänzen.

## Benötigte NuGet-Pakete

Keine.

## Verifikation

1. `dotnet build` — fehlerfrei.
2. `dotnet test` für Domain/Application/Api/Infrastructure — neue Tests grün.
3. `dotnet ef migrations add CreateCountryTable ...` erzeugt Migration;
   AppHost-Migrator-Job läuft durch.
4. `MyMusic.IntegrationTests` (inkl. neuer `CountryEndpointsTests`) grün.
5. `dotnet format --verify-no-changes`, Zeilenlängen-Check.
6. Manueller Stichprobentest: AppHost starten, per Swagger `GET
   /api/countries` aufrufen — 238 alphabetisch sortierte Einträge.

## Risiken und offene Punkte

- 238-Zeilen-Transkription ist die einzige mechanisch fehleranfällige
  Stelle — abgesichert durch Integrationstest (Anzahl + Sortierprüfung +
  Stichprobe), nicht durch Vollvergleich gegen die Wiki-Quelle.
- `GetAllAsync`s Erfolgsfall ist wie schon bei Genre nicht sinnvoll per
  NSubstitute unit-testbar (bekannte, bereits dokumentierte Einschränkung:
  gemockter `DbSet` implementiert kein `IAsyncEnumerable<T>`) — einziger
  Nachweis ist der Integrationstest.
- Der Genre-Slice zeigte bereits einmal einen Testlauf-Fehler, der fälschlich
  als Aspire/DCP-Einschränkung dokumentiert wurde (TASK.md-Nachtrag
  2026-08-05, inzwischen korrigiert); tatsächliche Ursache war die Ausführung
  über Git Bash statt PowerShell (CLAUDE.md §11). Tritt hier ein ähnlicher
  Fehler auf, wird zuerst PowerShell statt Git Bash geprüft, bevor eine
  Umgebungslimitierung vermutet wird.
- `StringComparer.InvariantCulture` ist eine Implementierungsentscheidung,
  keine Wiki-Vorgabe — kein vollständiges deutsches Duden-Alphabet, aber
  deterministisch über Umgebungen hinweg.
- Country ist nur isoliert über Swagger/Integrationstest nachweisbar, nicht
  im echten Verwendungskontext (Label-Dropdown) — der folgt erst mit Block 4
  (Label) und Block 0c (Angular).
