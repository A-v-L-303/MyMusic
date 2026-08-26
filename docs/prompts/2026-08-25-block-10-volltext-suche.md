# Block 10 — Volltext-Suche

Branch: `block-10-volltext-suche`
Datum: 2026-08-25

## Kontext

TASK.md Abschnitt 10 führt „Volltext-Suche" als offenen Punkt (Priorität
mittel bis niedrig). Die fachliche Planung ist im Wiki dokumentiert:
`02 Wiki/MyMusic Wiki/wiki/architektur/suche.md` (korrigiert am 2026-08-24) —
die globale Kopfzeilen-Suche liefert ausschließlich Records (keine Artists/
Labels als eigene Treffer), Kriterien: Record-Titel, Record-Artist,
Track-Artist, Label, Genre (nur über Track) und Land (über Label→Country),
jeweils per ILIKE-Teilstring (case-insensitive). Ziel: Backend-Endpunkt
`GET /api/search` sowie die Angular-Ansicht `/search` gemäß dieser Vorgaben
umsetzen — Backend und Frontend zusammen in einem Block/PR, der
Search-Platzhalter aus Block 0g wird dabei ersetzt.

Geklärt mit dem Projektinhaber am 2026-08-25 (im Wiki bisher nicht
dokumentiert, wird im Rahmen dieses Blocks nachgetragen):

- Leere Sucheingabe im Kopfzeilen-Suchfeld: es passiert nichts (kein Submit,
  kein Backend-Call) — bereits durch den bestehenden Guard in
  `NavComponent.submitSearch()` erfüllt.
- Eingabevalidierung am Kopfzeilen-Suchfeld: mindestens 2 Zeichen, erlaubtes
  Zeichenset identisch zu `Record.AlbumNamePattern`
  (`^[\p{L}\p{N} \-&'./()]+$`). Bei Verstoß Inline-Fehlermeldung am Suchfeld,
  keine Navigation. Prüfung ausschließlich im Frontend — das Backend
  validiert `q` nicht serverseitig (konsistent zur Regel „Queries werden im
  CQRS-Framework nicht validiert", siehe `wiki/architektur/api-endpunkte.md`).
- Suchergebnisse sind voll editierbar: Bearbeiten/Löschen direkt aus der
  Ergebnisliste (RecordForm-Modal, Lösch-Bestätigung), wie im Records-Feature.
- Darstellung: **Tabelle**, keine Cards — nach dem bestehenden
  Tabellen-Slice-Muster (Artist/Label/Genre, siehe
  `wiki/architektur/ui-ux-konzept.md` Abschnitt „Seiten-Layout:
  Tabellen-Slices"). Jedes Feld der Record-Entität bekommt eine eigene
  Spalte.

Nicht Teil dieses Blocks: Rate Limiting, CORS-Production-Whitelist, Content
Security Policy, Swagger-UI-Freischaltung für Production (bereits an anderer
Stelle in TASK.md als offen geführt).

## Architekturentscheidungen

1. **Volle Feature-Kapselung statt Response-Wiederverwendung**: Der
   naheliegende Ansatz, den bestehenden `RecordResponseBuilder`/
   `RecordListResponse` aus dem Record-Feature direkt zu injizieren, verstößt
   gegen die Feature-Kapselungsregel in `CLAUDE.md` Abschnitt 4.3 („kein
   Handler greift in den Ordner eines anderen Features") — dafür gibt es
   aktuell keinen Präzedenzfall im Code. Entscheidung: Search bekommt eigene
   Response-DTOs (`SearchResultResponse`, `SearchResultListResponse`) und
   einen eigenen `SearchResponseBuilder`, strukturell identisch zu
   `RecordResponse`/`RecordListResponse` (ohne `Tracks`-Feld, da auch die
   Record-Listenansicht keine Tracks lädt). Das Frontend braucht dafür keine
   Parallel-Typen — die JSON-Form ist identisch, Cross-Feature-Imports sind
   im Frontend ohnehin bereits gängige Praxis.
2. **Kein Navigationsproperty, ID-Set-Auflösung**: `Record`, `RecordTrack`,
   `Artist`, `Label`, `Genre`, `Country` sind reine POCOs ohne
   EF-Navigationsproperties. Die Suche über mehrere Entitäten hinweg löst
   daher — analog zum bestehenden `countryId`-Filter in
   `GetPagedRecordsQueryHandler` — zunächst passende IDs je Referenz-Entität
   auf (`IRepository<T>.GetProjectedAsync<int>`, schlanker als
   `GetPagedAsync` mit `pageSize: int.MaxValue`) und kombiniert sie im
   finalen `Record`-Filter per `HashSet<int>.Contains(...)`. Keine neue
   Repository-Methode nötig, keine ADR — reine Anwendung eines bereits
   etablierten Musters.
3. **Feature-Kategorie**: `Features/Sammlung/Search/` — die Suche ist eine
   Sicht auf „meine Sammlung", passt zu Record/RecordTrack, die bereits unter
   `Sammlung` liegen.

## Backend

**Neue Dateien unter `src/MyMusic.Application/Features/Sammlung/Search/`:**

- `ResponseDtos/SearchResultResponse.cs` — sealed record im Stil von
  `RecordResponse.cs` (Id, LabelId, LabelName, ArtistId, ArtistName, Format,
  AlbumName, ReleaseYear, Condition, Information, AlbumCoverDataUrl).
- `ResponseDtos/SearchResultListResponse.cs` — sealed record (Items,
  TotalCount, Page, PageSize, TotalPages).
- `ResponseDtos/Builder/SearchResponseBuilder.cs` — `BuildPaged(...)`
  strukturell identisch zu `RecordResponseBuilder.BuildPaged`, inkl.
  `BuildAlbumCoverDataUrl`-Logik für die Data-URL-Erzeugung.
- `Queries/GetPaged/GetPagedSearchQuery.cs` —
  `public sealed record GetPagedSearchQuery(Guid UserId, int Page, int PageSize, string? Query) : IQuery<SearchResultListResponse>;`
- `Queries/GetPaged/GetPagedSearchQueryHandler.cs` — injiziert
  `IRepository<RecordEntity>`, `IRepository<ArtistEntity>`,
  `IRepository<LabelEntity>`, `IRepository<GenreEntity>`,
  `IRepository<CountryEntity>`, `IRepository<RecordTrackEntity>`,
  `SearchResponseBuilder`. Ablauf:
  1. `normalizedQuery = query.Query?.Trim().ToLower()`. Leer/null: sofort
     `SearchResultListResponse` mit leerer Liste zurückgeben, **ohne** einen
     der sechs Repositories aufzurufen (kein Validierungsfehler, reine
     Kurzschluss-Logik — ein leerer Teilstring wäre sonst in jedem Namen
     „enthalten" und würde versehentlich die komplette Sammlung liefern).
  2. `matchingArtistIds` über `artistRepository.GetProjectedAsync(a => a.UserId==userId && a.Name.ToLower().Contains(q), a => a.Id)`.
  3. `matchingGenreIds` analog über `genreRepository` (Genre hat `UserId`).
  4. `matchingCountryIds` analog über `countryRepository`, **ohne**
     `UserId`-Filter (Country ist globale Referenztabelle ohne `user_id`).
  5. `matchingLabelIds` über eine Abfrage:
     `l => l.UserId==userId && (l.Name.ToLower().Contains(q) || matchingCountryIds.Contains(l.CountryId))`.
  6. `matchingRecordIdsViaTrack`: nur wenn `matchingArtistIds` oder
     `matchingGenreIds` nicht leer sind (sonst leeres `HashSet<int>` ohne
     DB-Zugriff) —
     `recordTrackRepository.GetProjectedAsync(t => t.UserId==userId && (matchingArtistIds.Contains(t.ArtistId) || matchingGenreIds.Contains(t.GenreId)), t => t.RecordId)`,
     `.Distinct().ToHashSet()`.
  7. Finaler Filter über `repository.GetPagedAsync(...)`:
     ```
     record => record.UserId == query.UserId
         && (record.AlbumName.ToLower().Contains(normalizedQuery)
             || (record.ArtistId != null && matchingArtistIds.Contains(record.ArtistId.Value))
             || matchingLabelIds.Contains(record.LabelId)
             || matchingRecordIdsViaTrack.Contains(record.Id))
     ```
     Sortierung fix `OrderBy(record => record.AlbumName)` (kein `sortBy`
     dokumentiert).
  8. Label-/Artist-Namen für die Treffer per Batch-Dictionary auflösen
     (`ResolveLabelNamesAsync`/`ResolveArtistNamesAsync`, lokal in diesem
     Handler dupliziert — Feature-Kapselung lässt keinen Cross-Feature-Import
     zu).
  9. `SearchResponseBuilder.BuildPaged(items, labelNamesById, artistNamesById, totalCount, page, pageSize)`.

**Wiring:**

- `ApplicationServiceCollectionExtensions.cs`: `SearchResponseBuilder`
  registrieren.
- `GlobalUsing.cs` (Application, Application.Tests): neue Usings für
  `Search.Queries.GetPaged` (+ `Search.ResponseDtos` in Application).
- `src/MyMusic.Api/Endpoints/Sammlung/Search/SearchEndpoints.cs` —
  `MapGroup("/api/search").RequireAuthorization()`, `GET` mit `q`, `page`,
  `pageSize` (gleiches Normalisierungsmuster wie `RecordEndpoints`: Page
  min. 1, PageSize geclampt 1–100), XML-`<summary>`-Pflicht.
- `Program.cs`: `app.MapSearchEndpoints();` nach `app.MapRecordEndpoints();`.
- `GlobalUsing.cs` (Api): neue Usings für Endpoint- und Query-Namespace.

Keine manuelle DI-Registrierung für Handler nötig (Assembly-Scan in
`ApplicationServiceCollectionExtensions.RegisterHandlers`), `IRepository<T>`
ist generisch registriert.

**Tests:**

- `tests/MyMusic.Application.Tests/Features/Sammlung/Search/Queries/GetPaged/GetPagedSearchQueryHandlerTests.cs`
  (NSubstitute-Mocks aller sechs Repositories, Muster
  `GetPagedRecordsQueryHandlerTests.cs`):
  - Leerer/`null`/nur-Leerzeichen-Query → leeres Ergebnis, keiner der sechs
    Repositories wird aufgerufen (`DidNotReceive()`).
  - Treffer je Kriterium einzeln: Album-Titel, Record-Artist, Track-Artist
    (Record hat anderen Haupt-Artist als der matchende Track), Label-Name,
    Genre (nur über Track), Land (über Label→Country).
  - Mandantentrennung: Treffer eines anderen Users bei Artist/Label/Genre
    darf beim aktuellen User zu keinem Ergebnis führen.
  - Case-Insensitivität an mindestens einem Kriterium.
  - Seitenparameter-Weiterleitung.
- `tests/MyMusic.IntegrationTests/SearchEndpointsTests.cs` (Muster
  `RecordEndpointsTests.cs`): 401 ohne Token, ein Treffer je Kriterium über
  echten HTTP-Call, Mandantentrennung, leerer `q` → 200 mit 0 Treffern.

## Frontend

**Neue Dateien unter `src/frontend/src/app/features/search/`:**

- `search.service.ts` (+ `.spec.ts`) — `getPaged(page, pageSize, q): Observable<RecordListResponse>`
  gegen `GET /api/search`, verwendet die bestehenden `Record`/
  `RecordListResponse`-TS-Typen aus `../records/record` (JSON-Form
  identisch zur Backend-Response).
- `search-result-table/search-result-table.ts` (+ `.html`, `.spec.ts`) —
  Tabellen-Komponente nach dem Muster von `LabelTable`
  (`features/labels/label-table/`), **nicht** `RecordCard`. Spalten (jedes
  Record-Feld gemäß Tabellen-Slice-Regel): Cover-Thumbnail, Titel, Artist,
  Label, Format (`RECORD_FORMAT_LABELS`), Jahr, Zustand (Grade-Badge via
  `RECORD_CONDITION_GRADE_CLASS`/`_TEXT`), Information (gekürzt +
  `title`-Tooltip), Aktionen (Edit-/Delete-Icons). Inputs: `items: Record[]`,
  `loading`, `page`, `totalPages`. Outputs: `editRequested`,
  `deleteRequested`, `pageChange`. Kein Klick-Handler auf der Zeile selbst
  (konsistent zu Label-/Artist-/Genre-Tabellen — keine Zeilennavigation).
  Loading-/Empty-State exakt wie `LabelTable`.

**Geänderte Dateien:**

- `search.ts` — ersetzt Platzhalter, Muster wie `Labels`
  (`features/labels/labels.ts`): `query` weiterhin per
  `toSignal(route.queryParamMap...)`; `page`-Signal mit `effect()`, das bei
  Änderung von `query()` auf Seite 1 zurücksetzt; `searchResource`
  (`rxResource`) ruft `searchService.getPaged` nur wenn `query()` gesetzt
  ist, sonst leeres Ergebnis ohne HTTP-Call; `formOpen`, `editingRecord`,
  `pendingDelete`, `pendingDeleteMessage`, `openEditForm`, `onFormCancelled`,
  `onFormSaved` (`searchResource.reload()`), `onDeleteRequested`,
  `onDeleteCancelled`, `onDeleteConfirmed` wörtlich aus `records.ts`
  übernommen (nutzen den bestehenden `RecordService.update`/`.delete`).
- `search.html` — Überschrift „Suche" + Treffer-Anzahl-Badge (analog
  `labels.html`), darunter `<app-search-result-table>`, danach
  `<app-record-form>` + `<app-confirm-modal>`; ohne Query ein Hinweistext
  statt Tabelle.
- `search.spec.ts` — kompletter Umbau: kein Query → Hinweistext; Treffer →
  Tabelle; Edit-Flow öffnet `RecordForm` und lädt nach Speichern neu;
  Delete-Flow öffnet `ConfirmModal` und ruft `RecordService.delete`;
  Query-Wechsel setzt Seite zurück auf 1.
- `nav.ts` — Signal-Forms-Validatoren am bestehenden `searchForm` (Muster
  `record-form.ts`): `minLength(path.query, 2, {...})`,
  `pattern(path.query, /^[\p{L}\p{N} \-&'./()]+$/u, {...})`. Neues Signal
  `attemptedSubmit`. `submitSearch()` bricht zusätzlich ab, wenn
  `searchForm.query().invalid()`.
- `nav.html` — Inline-Fehlermeldung unter dem Suchfeld
  (`searchForm.query().errors()[0].message`), sichtbar bei
  `touched() || attemptedSubmit()` (gleiches Muster wie `labelId` in
  `record-form.html`).
- `nav.spec.ts` — neue Tests: zu kurze Eingabe / verbotenes Zeichen zeigen
  Fehlermeldung und navigieren nicht; bestehender Leer-Test bekommt
  Zusatz-Assertion „keine Fehlermeldung".

`search.routes.ts` und `app.routes.ts` bleiben unverändert (bereits seit
Block 0g verdrahtet).

## Dokumentation

- `wiki/architektur/suche.md`: Darstellung als Tabelle (nicht Card) sowie
  die Eingabevalidierung (min. 2 Zeichen, Zeichenset) ergänzen.
- `wiki/architektur/angular-projektstruktur.md`: Komponentenbaum „Feature:
  Search" von `SearchResultRecordsComponent` auf die tatsächliche
  Tabellen-Komponente korrigieren.
- `wiki/index.md`/`wiki/log.md`: nach Wiki-eigenen Regeln aktualisieren.
- `TASK.md` Abschnitt 10: „Records, Artists und Labels in kombinierter
  Ansicht" korrigieren auf „ausschließlich Records" (Wiki-Korrektur vom
  2026-08-24 war hier noch nicht nachgezogen), konkrete Unteraufgaben und
  Status nach Umsetzung ergänzen.
- Root-`CLAUDE.md` Abschnitt 1 Punkt 3: gleiche Korrektur
  („Volltext-Suche über Records, Artists und Labels" → „ausschließlich
  Records"), neuer „Stand 2026-08-25"-Absatz nach Umsetzung.
- Kein neues ADR (siehe Architekturentscheidungen oben).

## Verifikation

1. Backend: `dotnet restore`, `dotnet build --no-restore`,
   `dotnet format --verify-no-changes`, `dotnet test --no-build`
   (Domain-, Application-, Api-, Infrastructure-Testprojekte).
2. Frontend: `ng test --watch=false`, `ng lint`.
3. Manuelle Live-Verifikation gegen den laufenden Aspire-AppHost (Standard-
   Launch-Profil): Suche nach Titel/Artist/Label/Genre/Land liefert die
   erwarteten Records; leere/zu kurze/ungültige Eingabe zeigt Fehlermeldung
   statt zu navigieren; Edit/Delete direkt aus der Ergebnistabelle
   funktionieren und aktualisieren die Tabelle; Mandantentrennung geprüft
   (zweiter Testbenutzer sieht keine fremden Treffer).
4. Ergebnisse im Abschlussbericht dokumentieren; nicht geprüfte Punkte
   explizit nennen.
