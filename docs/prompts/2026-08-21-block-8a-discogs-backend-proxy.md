# Block 8a — Discogs-Backend-Proxy

## Kontext

TASK.md Abschnitt 8 (Discogs-Integration) ist der nächste offene MVP-Block.
User Stories mit Akzeptanzkriterien liegen vor
(`wiki/user-stories/user-stories-discogs.md`, US-DI1–DI4, 2026-08-21). Der
Block ist in Backend (8a, dieser Prompt) und Frontend (8b, separater, späterer
Block) aufgeteilt, analog zu Record (6a/6f–j) und Admin (7c).

Mit dem Projektinhaber geklärt (siehe `user-stories-discogs.md` und
ADR 0018):

1. **Zwei Endpunkte**: `GET /api/discogs/search?q=...` (Trefferliste,
   Kurzdaten) und `GET /api/discogs/releases/{id}` (Detailabruf: Cover
   Originalgröße, Tracklist, Format-Detail) — zweistufiger Abruf laut
   `wiki/tech-stack/discogs-api.md`.
2. **Suchbegriff**: mindestens 2 Zeichen, sonst Validierungsfehler (400).
   Kürzere/leere Eingaben werden serverseitig abgelehnt.
3. **Keine eigene Paginierung**: Die Suche liefert Discogs' erste
   Ergebnisseite unpaginiert als flache Liste zurück.
4. **Auth gegenüber Discogs**: Personal Access Token, als
   `Authorization`-Header (`Discogs token=...`), nicht als Query-Parameter
   (ADR 0018) — verhindert, dass das Secret in der HTTP-Instrumentation
   landet.
5. **Discogs-Fehler einheitlich auf HTTP 502**: nicht erreichbar, Rate-Limit,
   ungültige/unbekannte Release-ID — alles über eine neue
   `DiscogsUnavailableException` (ADR 0018), nicht über die bestehenden
   404/500-Fälle.
6. **Teststrategie**: Kein Integrationstest gegen die echte Discogs-API (nur
   der 401-Fall wird per Integrationstest geprüft). Der `DiscogsClient`
   selbst wird nur per manueller Live-Verifikation geprüft, nicht
   automatisiert getestet — bewusste, dokumentierte Lücke wie beim
   `KeycloakAdminClient`-Vorbild.
7. **Neue Namenskategorie „Integration"** in `Application/Features/` und
   `Api/Endpoints/` (bisher nur Stammdaten/Sammlung/System/Verwaltung).

## Vorgeschlagene Schritte

### 1. Application (`MyMusic.Application`)

- Neu: `Common/Exceptions/DiscogsUnavailableException.cs` — `sealed class`,
  parameterloser Konstruktor, feste deutsche Meldung „Die Discogs-API ist
  aktuell nicht erreichbar oder liefert einen Fehler."
- `Common/Exceptions/ExceptionManager/ExceptionManager.cs`: neue Methode
  `public DiscogsUnavailableException DiscogsUnavailable() => new();`.
- Neu: `Common/Services/IDiscogsClient.cs` —
  `Task<IReadOnlyList<DiscogsSearchResult>> SearchAsync(string query, CancellationToken cancellationToken);`,
  `Task<DiscogsRelease> GetReleaseAsync(int id, CancellationToken cancellationToken);`.
- Neu: `Common/Services/DiscogsSearchResult.cs` —
  `sealed record DiscogsSearchResult(int Id, string Title, int? Year, string? Label, string? ThumbnailUrl);`.
- Neu: `Common/Services/DiscogsRelease.cs` —
  `sealed record DiscogsRelease(int Id, string Title, int? Year, IReadOnlyList<string> Artists, IReadOnlyList<string> Labels, IReadOnlyList<string> Genres, IReadOnlyList<string> Styles, IReadOnlyList<DiscogsFormat> Formats, string? CoverImageUrl, IReadOnlyList<DiscogsTrack> Tracklist);`.
- Neu: `Common/Services/DiscogsFormat.cs` —
  `sealed record DiscogsFormat(string Name, IReadOnlyList<string> Descriptions);`.
- Neu: `Common/Services/DiscogsTrack.cs` —
  `sealed record DiscogsTrack(string Position, string Title, string? Duration);`.
- Struktur unter `Features/Integration/Discogs/`:
  - `Queries/Search/SearchDiscogsQuery.cs`:
    `sealed record SearchDiscogsQuery(string Q) : IQuery<IEnumerable<DiscogsSearchResultResponse>>;`.
  - `Queries/Search/SearchDiscogsQueryHandler.cs`: Abhängigkeiten
    `IDiscogsClient`, `DiscogsResponseBuilder`, `ExceptionManager`. Prüft
    zuerst `Q.Trim().Length < 2` → `exceptionManager.ValidationFailed([new ValidationFailure(nameof(SearchDiscogsQuery.Q), "Der Suchbegriff muss mindestens 2 Zeichen lang sein.")])`.
    Ruft `discogsClient.SearchAsync(...)` in `try/catch` für
    `HttpRequestException`/`JsonException`/`TaskCanceledException` (nur wenn
    `!cancellationToken.IsCancellationRequested`) → `throw exceptionManager.DiscogsUnavailable();`.
    Mappt Erfolgsfall per `responseBuilder.BuildSearchResult(...)`.
  - `Queries/GetRelease/GetDiscogsReleaseQuery.cs`:
    `sealed record GetDiscogsReleaseQuery(int Id) : IQuery<DiscogsReleaseResponse>;`.
  - `Queries/GetRelease/GetDiscogsReleaseQueryHandler.cs`: analoges Muster,
    kein Leerstring-Check (Id ist `int`, Binding im Endpoint erzwingt das
    Format), jeder Fehlerfall (inkl. Discogs-404 bei unbekannter Id) →
    `DiscogsUnavailableException`/502.
  - `ResponseDtos/DiscogsSearchResultResponse.cs`,
    `ResponseDtos/DiscogsReleaseResponse.cs`,
    `ResponseDtos/DiscogsFormatResponse.cs`,
    `ResponseDtos/DiscogsTrackResponse.cs` — Felder identisch zur
    Common/Services-Contract-Form (zwei getrennte Schichten trotz
    Feldüberschneidung, Muster wie `KeycloakUserSummary`/`UserResponse`).
  - `ResponseDtos/Builder/DiscogsResponseBuilder.cs`:
    `BuildSearchResult(DiscogsSearchResult) => DiscogsSearchResultResponse`,
    `BuildRelease(DiscogsRelease) => DiscogsReleaseResponse` (inkl.
    Formats/Tracklist-Mapping).
- `ApplicationServiceCollectionExtensions.cs`: `services.AddScoped<DiscogsResponseBuilder>();`
  ergänzen.
- `GlobalUsing.cs`: neue `Features.Integration.Discogs.*`-Namespaces ergänzen.

### 2. Infrastructure (`MyMusic.Infrastructure`)

Neu unter `ExternalServices/Discogs/`:

- `DiscogsClient.cs`: implementiert `IDiscogsClient`, Konstruktor
  `(HttpClient httpClient)`. `SearchAsync`: `GET /database/search?q={query}&type=release`,
  `response.EnsureSuccessStatusCode()`, Deserialisierung mit
  `PropertyNameCaseInsensitive = true`, mappt
  `DiscogsSearchResponseRepresentation.Results` (nur `Type == "release"`,
  falls die Discogs-API den Filter nicht bereits serverseitig anwendet) auf
  `DiscogsSearchResult` — `Label` durch `", "`-Verkettung des
  `Label`-Arrays, `Thumb` → `ThumbnailUrl`. `GetReleaseAsync`:
  `GET /releases/{id}`, mappt `DiscogsReleaseRepresentation` auf
  `DiscogsRelease` — `Artists`/`Labels` als Namensliste, `Images` → erstes
  Element mit `Type == "primary"`, sonst erstes Element, sonst `null`.
- `DiscogsSearchResponseRepresentation.cs`, `DiscogsSearchResultRepresentation.cs`
  (`Id`, `Type`, `Title`, `Year` **als `string?`** — Discogs liefert hier
  einen String, anders als im Release-Detail), `DiscogsReleaseRepresentation.cs`
  (`Year` als `int?`), `DiscogsArtistRepresentation.cs`,
  `DiscogsLabelRepresentation.cs`, `DiscogsFormatRepresentation.cs`,
  `DiscogsImageRepresentation.cs`, `DiscogsTrackRepresentation.cs` — je eine
  Klasse pro Datei, `sealed record`.
- Im Konstruktor von `DiscogsClient` **nicht** den `User-Agent`-Header setzen
  (das passiert zentral beim `AddHttpClient`-Aufruf in `Program.cs`, siehe
  Schritt 3 — ein `HttpClient` erlaubt `DefaultRequestHeaders` nur einmal
  global, nicht sinnvoll pro Aufruf neu zu setzen).
- `GlobalUsing.cs`: kein Eintrag nötig (`System.Net.Http.Headers`,
  `System.Net.Http.Json`, `System.Text.Json*` sind bereits global vorhanden).

### 3. Api (`MyMusic.Api`)

- Neu: `Endpoints/Integration/Discogs/DiscogsEndpoints.cs`:
  ```csharp
  var group = endpoints.MapGroup("/api/discogs").RequireAuthorization();
  group.MapGet("/search", SearchAsync);
  group.MapGet("/releases/{id:int}", GetReleaseAsync);
  ```
  Beide Endpoint-Methoden `private static`, XML-`<summary>` (Pflicht,
  CLAUDE.md §9), Signatur `(IMediator mediator, string q, CancellationToken cancellationToken)`
  bzw. `(IMediator mediator, int id, CancellationToken cancellationToken)` —
  kein `ICurrentUserService` (mandantenlos).
- `Program.cs`:
  ```csharp
  builder.Services.AddHttpClient<IDiscogsClient, DiscogsClient>(client =>
  {
      client.BaseAddress = new Uri(builder.Configuration["Discogs:BaseUrl"]!);
      client.DefaultRequestHeaders.UserAgent.ParseAdd("MyMusic/1.0 +https://github.com/<Repo>");
      client.DefaultRequestHeaders.Authorization =
          new AuthenticationHeaderValue("Discogs", $"token={builder.Configuration["Discogs:Token"]}");
  });
  ```
  (User-Agent-Wert im Arbeits-Prompt-Review konkretisieren — Discogs verlangt
  nur „aussagekräftig", keine feste Wiki-Vorgabe zum genauen Inhalt.)
  Danach `app.MapDiscogsEndpoints();` nach `app.MapAdminEndpoints();`
  ergänzen.
- `GlobalExceptionHandler.cs`: neuer Case vor dem Default-Fallback:
  ```csharp
  DiscogsUnavailableException discogsException => (
      StatusCodes.Status502BadGateway,
      new ProblemDetails
      {
          Title = "Discogs nicht erreichbar",
          Detail = discogsException.Message,
          Status = StatusCodes.Status502BadGateway
      }),
  ```
- `GlobalUsing.cs`: `global using MyMusic.Infrastructure.ExternalServices.Discogs;`
  ergänzen (analog zur bestehenden Keycloak-Zeile).

### 4. AppHost (`MyMusic.AppHost`)

- `AppHost.cs`: neuer Parameter `var discogsToken = builder.AddParameter("discogs-token", secret: true);`
  nach `keycloakAdminPassword`. Auf der `api`-Ressource ergänzen:
  `.WithEnvironment("Discogs__Token", discogsToken)`,
  `.WithEnvironment("Discogs__BaseUrl", "https://api.discogs.com")`.

### 5. Tests

- **Application.Tests**:
  - `SearchDiscogsQueryHandlerTests` (NSubstitute `Substitute.For<IDiscogsClient>()`):
    Suchbegriff mit 1 Zeichen → `ValidationException`; leerer/Whitespace-
    Suchbegriff → `ValidationException`; gültiger Suchbegriff → korrektes
    Mapping über den `DiscogsResponseBuilder`; `IDiscogsClient` wirft
    `HttpRequestException` → `DiscogsUnavailableException`; wirft
    `TaskCanceledException` ohne angefordertem Abbruch →
    `DiscogsUnavailableException`.
  - `GetDiscogsReleaseQueryHandlerTests`: Happy Path (inkl. Tracklist,
    Formats, `null`-Cover-Fall); `IDiscogsClient` wirft
    `HttpRequestException` → `DiscogsUnavailableException`.
  - `DiscogsResponseBuilderTests`: Mapping-Details (leere Listen,
    `null`-Werte in optionalen Feldern).
  - `ExceptionManagerTests.cs`: neuer Fall
    `DiscogsUnavailable_ErzeugtFesteDeutscheFehlermeldung`.
- **IntegrationTests**: Neu `DiscogsEndpointsTests.cs` (Muster
  `CountryEndpointsTests.cs`) — **nur** 401 ohne Token für beide Endpunkte
  (`GET /api/discogs/search?q=test`, `GET /api/discogs/releases/1`). Kein
  echter Discogs-Call. Voraussetzung: `Parameters:discogs-token` muss beim
  Testlauf als (auch Dummy-)User-Secret im AppHost-Projekt gesetzt sein,
  sonst scheitert bereits der API-Start beim Aufbau des
  `Authorization`-Headers.

### 6. Dokumentation

- `TASK.md`: Abschnitt 8 auf „Backend (8a) abgeschlossen, 8b offen" setzen,
  Umsetzt-Liste analog anderen Blöcken; Kopfzeile „Branch:" aktualisieren.
- `README.md`: Secrets-Abschnitt um `discogs-token` ergänzen (Hinweis: vom
  Benutzer manuell bei Discogs zu erzeugen, per `dotnet user-secrets`
  hinterlegt, kein Default-Wert).
- Wiki: nur bei tatsächlichen Abweichungen zur Planung korrigieren (z. B.
  falls die Live-Verifikation abweichende Discogs-Feldnamen/-typen zeigt) —
  als ADR-0018-Nachtrag, nicht durch stille Wiki-Änderung.

## Benötigte NuGet-Pakete

Keine — `AddHttpClient<TInterface, TImplementation>()` ist über
`Microsoft.NET.Sdk.Web` bereits verfügbar (wie beim
`KeycloakAdminClient`-Vorbild ohne zusätzliches Paket genutzt).

## Verifikation

1. `dotnet build` — fehlerfrei.
2. `dotnet test` für Domain/Application/Api/Infrastructure — neue Tests grün.
3. `dotnet format --verify-no-changes`, Zeilenlängen-Check.
4. `MyMusic.IntegrationTests` (inkl. neuer `DiscogsEndpointsTests`) grün —
   vorher `Parameters:discogs-token` als Dummy-Secret setzen, falls noch
   nicht vorhanden.
5. **Manuelle Live-Verifikation** gegen die echte Discogs-API (ersetzt einen
   Infrastructure-Test, siehe ADR 0018 und Teststrategie-Entscheidung):
   - Personal Access Token unter discogs.com/settings/developers erzeugen.
   - `dotnet user-secrets set "Parameters:discogs-token" "<echter Token>"`
     im AppHost-Projekt.
   - AppHost starten, über Swagger UI `GET /api/discogs/search?q=<bekannter
     Titel>` aufrufen — Antwortform gegen das DTO-Design prüfen
     (`year`-Typ, Label-Verkettung, nur `type=release`-Treffer).
   - Eine zurückgelieferte `id` gegen `GET /api/discogs/releases/{id}`
     prüfen (Cover-URL, Tracklist, Formats, Artists/Labels).
   - Suchbegriff mit 1 Zeichen → 400 prüfen.
   - Fehlerfall (Token temporär ungültig setzen) → 502 prüfen.
   - Abweichungen von der geplanten DTO-Form als ADR-0018-Nachtrag
     dokumentieren.

## Risiken und offene Punkte

- Die genaue Discogs-JSON-Struktur ist aus der öffentlichen API-Dokumentation
  abgeleitet, nicht in dieser Planungssitzung live verifiziert — Abweichungen
  sind bei Schritt 5 der Verifikation zu erwarten und dort zu korrigieren,
  nicht vorab zu erraten.
- Kein automatisierter Test für `DiscogsClient` selbst — bewusste,
  dokumentierte Lücke (siehe ADR 0018), Discogs-Ausfälle nach dem Merge
  werden nicht automatisiert erkannt, nur durch Nutzungsbeobachtung.
- `KeycloakAdminClient` hat denselben ungefangenen-Fehler-Fall (fällt auf
  500) — bleibt in diesem Block bewusst unangetastet (siehe ADR 0018).
- Discogs' tatsächliches Rate-Limit-Verhalten (Header, exakter 429-Response-
  Body) wird erst bei der Live-Verifikation sichtbar — kein eigenes
  Throttling geplant (Wiki-Klärung), daher kein zusätzlicher Code dafür.
