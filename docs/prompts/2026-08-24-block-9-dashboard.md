# Block 9 — Dashboard

Branch: `block-9-dashboard`
Datum: 2026-08-24

## Kontext

TASK.md Abschnitt 9 führt „Dashboard" als offenen Punkt (Priorität mittel bis
niedrig). Die fachliche Planung ist im Wiki vollständig und wurde am
2026-08-24 mit dem Projektinhaber anhand eines Mockups geklärt:
`02 Wiki/MyMusic Wiki/wiki/user-stories/user-stories-dashboard.md` (US-DA1–DA6),
ergänzt um `wiki/architektur/api-endpunkte.md` (Endpunkt-Vertrag) und
`wiki/architektur/angular-projektstruktur.md` (Komponentenhierarchie). Ziel:
den Endpunkt `GET /api/dashboard` sowie die Angular-Ansicht `/dashboard`
gemäß dieser Vorgaben umsetzen — Backend und Frontend zusammen in einem
Block/PR, der Dashboard-Platzhalter aus Block 0g wird dabei ersetzt.

Geklärt mit dem Projektinhaber am 2026-08-24:
- Ein Block/ein PR (Backend + Frontend zusammen).
- Jahresverteilung (US-DA5) wird chronologisch aufsteigend sortiert (im Wiki
  nicht explizit festgelegt, anders als bei Format/Top-Artists/Top-Labels).
- Aggregation der Record-Statistiken erfolgt über eine neue Projektions-Methode
  auf `IRepository<T>` statt über das volle Laden der `Record`-Entität — die
  ursprünglich geplante Verwendung von `GetPagedAsync` mit voller Entität hätte
  pro Record auch `album_cover BYTEA` (bis zu 5 MB) mitgeladen, obwohl die
  Aggregation nur `Format`, `ArtistId`, `LabelId` und `ReleaseYear` braucht.
  Bewusst nicht gewählt: echte SQL-`GROUP BY`/`COUNT`-Aggregation in der
  Datenbank (größerer Eingriff in die Repository-Abstraktion, für eine
  private Sammlung ohne Massendaten nicht nötig, sobald die Cover-Blobs
  draußen sind).

Nicht Teil dieses Blocks: Volltext-Suche, Diagramm-Bibliothek (eigene
Balken-Markup-Lösung statt externer Library), Zustandsbewertung.

## Architekturentscheidungen

1. **Repository-Erweiterung `GetProjectedAsync`**: `IRepository<T>` bekommt
   eine neue generische Methode `GetProjectedAsync<TProjection>(filter,
   selector, ct)`, die EF Core in ein SQL-`SELECT` nur der referenzierten
   Spalten übersetzt. Nur eine bestehende Implementierung (`Repository<TEntity>`
   in Infrastructure) — additive, ungefährliche Schnittstellenerweiterung.
   Dokumentiert in ADR `docs/adr/0021-repository-projektion-fuer-dashboard-aggregation.md`.
2. **Feature-Kategorie**: `Features/Sammlung/Dashboard/` — das Dashboard ist
   eine Auswertung über „meine Sammlung" (US-DA1), passt zu Record/RecordTrack,
   die bereits unter `Sammlung` liegen.

## Backend

**Repository-Erweiterung:**
- `src/MyMusic.Domain/Contracts/Repository/IRepository.cs` — neue Methode
  `GetProjectedAsync<TProjection>(Expression<Func<TEntity,bool>> filter,
  Expression<Func<TEntity,TProjection>> selector, CancellationToken ct)`
  mit vollständigem XML-Dokumentationskommentar.
- `src/MyMusic.Infrastructure/Persistence/Repositories/Repository.cs` —
  Implementierung: `await _dbSet.Where(filter).Select(selector).ToListAsync(cancellationToken);`.

**Neue Dateien unter `src/MyMusic.Application/Features/Sammlung/Dashboard/`:**
- `Queries/GetDashboard/GetDashboardQuery.cs` —
  `public sealed record GetDashboardQuery(Guid UserId) : IQuery<DashboardResponse>;`
- `Queries/GetDashboard/RecordAggregationProjection.cs` — sealed record
  `(int Id, int LabelId, int? ArtistId, RecordFormat Format, int ReleaseYear)`,
  bewusst ohne Cover-Feld.
- `Queries/GetDashboard/GetDashboardQueryHandler.cs` — injiziert
  `IRepository<RecordEntity>`, `IRepository<ArtistEntity>`,
  `IRepository<LabelEntity>`, `IRepository<GenreEntity>`,
  `DashboardResponseBuilder`. Reine Datenbeschaffung, keine Aggregationslogik:
  - Records über `GetProjectedAsync` auf `RecordAggregationProjection`
    (kein Cover geladen); `RecordsTotal` = Listenlänge.
  - `ArtistsTotal`/`LabelsTotal`/`GenresTotal` je über
    `GetPagedAsync(filter: userId, page: 1, pageSize: 1, ...)`, nur
    `TotalCount` verwendet.
  - Namens-Auflösung für Top-Artists/Top-Labels über private
    `ResolveArtistNamesAsync`/`ResolveLabelNamesAsync`-Methoden nach dem
    Muster in `GetPagedRecordsQueryHandler.cs` (gezielter `GetPagedAsync`
    nur für die tatsächlich vorkommenden Ids).
- `ResponseDtos/DashboardResponse.cs`, `FormatCountResponse.cs`,
  `TopArtistResponse.cs`, `TopLabelResponse.cs`, `YearCountResponse.cs` —
  sealed records im Stil von `RecordResponse.cs`.
- `ResponseDtos/Builder/DashboardResponseBuilder.cs` — übernimmt die gesamte
  Aggregation (Format absteigend, TopArtists/TopLabels absteigend + Take(10),
  Records ohne ArtistId nicht in TopArtists, Jahresverteilung aufsteigend).

**Wiring:**
- `ApplicationServiceCollectionExtensions.cs`: `DashboardResponseBuilder`
  registrieren.
- `GlobalUsing.cs`: zwei neue globale Usings für Dashboard-ResponseDtos.
- `src/MyMusic.Api/Endpoints/Sammlung/Dashboard/DashboardEndpoints.cs` —
  `GET /api/dashboard`, `RequireAuthorization()`, XML-`<summary>`-Pflicht.
- `Program.cs`: `app.MapDashboardEndpoints();` ergänzen.

**Tests:**
- `GetDashboardQueryHandlerTests.cs` (NSubstitute-Mocks aller vier
  Repositories): normale Aggregation, leere Sammlung (US-DA6),
  Mandantentrennung.
- `DashboardResponseBuilderTests.cs`: Sortierung/Filterung/Take(10)/leere
  Eingabe.
- Kein neuer Test in `RepositoryTests.cs` (siehe Begründung im
  Plan/ADR — Query-Übersetzung wird konsistent über Integrationstests
  bzw. Live-Verifikation abgedeckt, nicht generisch auf Repository-Ebene).

## Frontend

**Layout:** Seiten-Container wie `genres.html`
(`mx-auto max-w-[1080px] px-6 pb-16 pt-6`), Überschrift „Dashboard" ohne
Toolbar/Badge. 4er-Grid Kennzahlen-Kacheln, darunter 2×2-Grid Detail-Kacheln.
Ein Top-Level-Spinner beim Laden; jede Detail-Kachel zeigt bei leerer Liste
unabhängig ihren eigenen `.empty`-Text (US-DA6, kein dashboard-weiter Empty
State).

**Bar-Optik:** Format/Top-Artists/Top-Labels teilen denselben visuellen Stil
(Name, Balken, Zahl) — bewusst als drei eigenständige, kleine Komponenten
ohne gemeinsame Shared-Abstraktion (passend zur Wiki-Komponentenhierarchie).
Balken mit Tailwind-Utilities auf Token-Basis (`bg-inset`/`bg-accent-solid`),
Breite relativ zu `max(count)`. Jahresverteilung als vertikales
Balkendiagramm, horizontal scrollbar, Jahr+Anzahl als natives
`title`-Tooltip.

**Neue/geänderte Dateien unter `src/frontend/src/app/features/dashboard/`:**
- `dashboard-stats.ts` (neu, Modell-File) — `DashboardStats`, `FormatCount`,
  `TopArtist`, `TopLabel`, `YearCount`; `FormatCount.format` nutzt den
  bestehenden `RecordFormat`-Typ aus `../records/record`.
- `dashboard.service.ts` (+ `.spec.ts`) — `getDashboard(): Observable<DashboardStats>`.
- `dashboard.ts` (ersetzt Platzhalter) — `rxResource`, `ErrorModalService`-Wiring.
- `dashboard.html` (ersetzt Platzhalter) — Layout wie oben.
- `dashboard.spec.ts` (ersetzt Platzhalter-Test).
- `stat-tile/` (+ `.html`, `.spec.ts`) — nutzt `.stat-card`/`.stat-value`/`.stat-label`.
- `format-chart/` (+ `.html`, `.spec.ts`) — Label via `RECORD_FORMAT_LABELS`.
- `top-artists/` (+ `.html`, `.spec.ts`) — Rang aus Array-Index + 1.
- `top-labels/` (+ `.html`, `.spec.ts`) — analog Top-Artists.
- `year-distribution/` (+ `.html`, `.spec.ts`).

`dashboard.routes.ts`, `app.routes.ts` und `nav.html` bleiben unverändert
(bereits seit Block 0g verdrahtet).

## Dokumentation

- `docs/adr/0021-repository-projektion-fuer-dashboard-aggregation.md` — neu.
- `TASK.md` Abschnitt 9: Status aktualisieren nach erfolgreicher Verifikation.
- Root-`CLAUDE.md`: neuer „Stand 2026-08-24"-Absatz.
- Kein Wiki-Update erwartet (User Stories/API-Endpunkte bereits aktuell).
- README.md: keine Änderung nötig.

## Verifikation

1. Backend: `dotnet restore`, `dotnet build --no-restore`,
   `dotnet format --verify-no-changes`, `dotnet test --no-build`
   (Domain-, Application-, Api-, Infrastructure-Testprojekte).
2. Frontend: `ng test --watch=false`, `ng lint`.
3. Manuelle Live-Verifikation gegen den laufenden Aspire-AppHost (Standard-
   Launch-Profil, nicht `--no-launch-profile`): Login, `/dashboard` öffnen,
   vier Kennzahlen und vier Detail-Kacheln mit echten Daten prüfen sowie den
   Empty-State (US-DA6).
4. Ergebnisse im Abschlussbericht dokumentieren; nicht geprüfte Punkte
   explizit nennen.
