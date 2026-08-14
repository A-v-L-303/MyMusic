# Block 6f: Record-Liste (Card-Ansicht, Filter, Sortierung, Paginierung)

Freigegeben am 2026-08-14. Branch: `block-6f-angular-records-liste`.

## Kontext

Der Records-Frontend-Slice ist der letzte offene MVP-CRUD-Slice und laut Wiki
"der fachlich umfangreichste Slice" (mehr Felder, Datei-Upload, zwei
Entitäten, Card- statt Tabellenansicht, eigene Detail-Route). Auf Wunsch des
Projektinhabers wird er in fünf einzeln abnehmbare Blöcke zerlegt, analog zur
Backend-Aufteilung 6a–6e (siehe TASK.md, Abschnitt 6). Dies ist der erste
Block: die reine Leseansicht (US-R1–R3), ohne Anlegen/Bearbeiten/Löschen,
ohne Detailseite, ohne Cover-Upload, ohne Tracks.

Bei der Planung wurde geklärt (siehe `wiki/architektur/ui-ux-konzept.md`,
Abschnitt „Filter-Leiste Records"), dass die Toolbar zusätzlich zu den fünf
in US-R2 genannten Filtern einen Format-Bezug enthalten soll. Die Wiki-Seite
spricht dort von einem "Format-Umschalter", ohne Werte oder
Backend-Verhalten festzulegen. Ein erster Entwurf (Alle/Vinyl/CD-Gruppierung)
wurde vom Projektinhaber verworfen: Diese Gruppierung existiert im
Datenmodell nicht und müsste künstlich aus den 10 `record_format`-Werten
abgeleitet werden. Stattdessen am 2026-08-14 geklärt: Der Filter arbeitet
direkt auf dem tatsächlichen `format`-Feld (exakter Wert, kein Gruppieren),
als natives Dropdown-Filterfeld in der Filter-Zeile — "Alle" ist Standard,
alle 10 `RecordFormat`-Werte einzeln wählbar. Das entspricht exakt dem schon
etablierten Muster für `artistId`/`labelId`/`countryId` (native `<select>`,
kein Sonderelement).

Dafür fehlt aktuell ein Backend-Filter — `GET /api/records` kennt nur
`sortBy=format` zum Sortieren, keinen Format-Filter. Mit dem Projektinhaber
geklärt: Der Formatfilter wird jetzt mit umgesetzt, inkl. der dafür nötigen
kleinen Backend-Erweiterung (direkter Gleichheitsfilter, kein
Gruppierungs-Set nötig). Das ist eine bewusste, eng begrenzte Ausnahme von
der bisherigen Regel "Frontend-Blöcke fassen keine Backend-Änderungen an"
(Backend für Record war mit 6a–6e als abgeschlossen markiert) — Umfang und
Grund werden hier dokumentiert, damit die Abweichung nicht stillschweigend
bleibt.

## Gesicherte Fakten aus der Recherche

- **Query-Validierung**: Das CQRS-Framework validiert nur Commands
  (`CommandValidationDecorator`), nie Queries (`Mediator.cs`, Query-Zweig
  löst den Handler ohne Validierungsschritt auf). Ein
  `AbstractValidator<GetPagedRecordsQuery>` würde nie ausgeführt. Bestehende
  Precedent: Unbekannte `sortBy`/`sortDirection`-Werte werden in
  `GetPagedRecordsQueryHandler.BuildOrderBy` still auf den Default
  normalisiert (kein 400). Kein GET-Filter im Projekt liefert bei
  ungültigem Wert HTTP 400 — ungültige Id-Filter liefern schlicht eine leere
  Liste. **Konsequenz für `format`**: kein neuer Validator, sondern stille
  Normalisierung nach demselben Muster wie `sortBy` — der rohe
  Query-String-Wert wird per `Enum.TryParse<RecordFormat>(format,
  ignoreCase: true, ...)` interpretiert; schlägt das fehl (inkl. `null`),
  bedeutet das "kein Filter", keine 400. Bewusst **kein** `RecordFormat?`
  als direkter Minimal-API-Parametertyp (das würde ASP.NET Cores
  eingebautes Enum-Model-Binding nutzen, dessen Verhalten bei einem
  ungültigen Wert vom etablierten Toleranz-Muster abweichen und
  möglicherweise implizit 400 auslösen könnte) — stattdessen `string?
  format` wie bei `sortBy`, Parsing im Handler.
- **JSON-Enum-Serialisierung**: `Program.cs` registriert
  `new JsonStringEnumConverter()` ohne Naming Policy → Wire-Format ist exakt
  der C#-Membername: `RecordFormat` → `Album`, `MaxiSingle`, `Single`, `Ep`,
  `Compilation`, `CdAlbum`, `CdMaxiSingle`, `CdSingle`, `CdEp`,
  `CdCompilation`; `RecordCondition` → `Mint`, `Nm`, `VgPlus`, `Vg`, `GPlus`,
  `G`, `P`.
- **Deutsche Format-Bezeichnungen**: bereits in `wiki/glossar.md` (Zeile 95)
  dokumentiert: `Album`, `MaxiSingle`, `Single`, `EP`, `Compilation`,
  `CD-Album`, `CD-MaxiSingle`, `CD-Single`, `CD-EP`, `CD-Compilation`.
- **Zustands-Anzeige**: `wiki/domain/zustandsbewertung.md` liefert die
  komplette ENUM-Mapping-Tabelle (Mint→"Mint (M)", Nm→"Near Mint (NM)",
  VgPlus→"Very Good Plus", Vg→"Very Good", GPlus→"Good Plus", G→"Good",
  P→"Poor") und legt fest: RecordCard zeigt den Zustand als Grade-Badge.
- **Design-System-Klassen** (`wiki/design/komponenten-klassen.md`):
  `.record-card`, `.record-cover` (+ `.fmt`-Pille LP/CD, Platzhalter bei
  fehlendem Cover), `.record-meta`, `.record-title`, `.record-artist`,
  `.record-sub` (Jahr · Label), `.grade`/`.grade-m`/`.grade-nm`/`.grade-vgp`/
  `.grade-vg`/`.grade-gplus`/`.grade-g`/`.grade-p`. Hinweis: `.record-sub`
  wird dort zusätzlich mit "Katalognummer" beschrieben — es gibt aber kein
  entsprechendes Feld auf `Record` (`RecordResponse.cs` hat keine
  Katalognummer). Wird als Abweichung gemeldet, `.record-sub` zeigt in
  diesem Block nur Jahr · Label.
- **UI-Zustände** (`wiki/architektur/ui-ux-konzept.md`): Empty State = reiner
  Text "Keine Daten vorhanden" (kein Icon/Bild); Loading State = animierte
  Ladeanzeige, Cards erst nach Laden gerendert; Filter-Leiste sitzt bei
  Records in eigener Zeile unterhalb der Toolbar (nicht inline wie bei
  Tabellen-Slices).
- **`GET /api/records`-Endpunkt** (`RecordEndpoints.GetPagedRecordsAsync`):
  bindet `page`, `pageSize` (normalisiert per `Math.Max`/`Math.Clamp`),
  `name`, `artistId`, `labelId`, `yearFrom`, `yearTo`, `countryId`, `sortBy`,
  `sortDirection` direkt aus dem Query-String; `GetPagedRecordsQueryHandler`
  filtert per LINQ-Prädikat.
- **`/all`-Endpunkte** aus Block 6e liefern flache Arrays (kein
  Paginierungs-Wrapper): `GetAllLabelsQueryHandler` → `IEnumerable<LabelResponse>`,
  analog `GetAllArtistsQueryHandler` → `IEnumerable<ArtistResponse>`
  (`ArtistResponse(int Id, string Name)`). Frontend konsumiert diese
  Endpunkte laut 6e-Notiz bewusst noch nicht — das holt dieser Block nach.
- **Angular-Referenzmuster** (`features/labels/`, `features/artists/`,
  `shared/`): `inject()`-Service mit `private get baseUrl()`-Getter auf
  `RuntimeConfigService.apiBaseUrl`; `HttpParams` wird nur bei gesetztem Wert
  ergänzt (truthy-Check); `rxResource({ params: () => ({...signale}),
  stream: ({params}) => service.getPaged(...) })` in der Shell-Komponente;
  Ladezustand über `resource.isLoading()`, Fehlerzustand über `effect()` +
  `ErrorModalService.showFromHttpError(error, '<Entität>', () =>
  resource.reload())`; Filter-Komponenten als Signal Form mit `debounce()`
  nur auf Textfeldern, native `<select>` für Fremdschlüssel/Land ohne
  Debounce, Ergebnis per `output()` an die Shell; `shared/pagination` wird
  per `[page]`/`[totalPages]`/`(pageChange)` eingebunden.
- **Country ist aktuell feature-lokal** unter `features/labels/country.ts`/
  `country.service.ts`, mit der im Code vermerkten Begründung "da Country
  aktuell nur von Label konsumiert wird". Records braucht `Country` jetzt
  ebenfalls (Länder-Filter) — zweiter Konsument, daher Verschiebung nach
  `shared/country/` in diesem Block (kein rein kosmetisches Refactoring,
  sondern notwendig, um Code-Duplizierung zu vermeiden; Umfang: zwei Dateien
  + ihr Spec verschieben, Importpfade in `features/labels/` anpassen).

## Umsetzung Backend (neuer, eng begrenzter Scope)

1. `src/MyMusic.Application/Features/Sammlung/Record/Queries/GetPaged/GetPagedRecordsQuery.cs`:
   neuen Parameter `string? Format` ergänzen (roher Query-String-Wert, wie
   `SortBy`).
2. `GetPagedRecordsQueryHandler.cs`:
   - Neue private Methode `ResolveFormatFilter(string? format)` →
     `RecordFormat?` (`Enum.TryParse<RecordFormat>(format, ignoreCase: true,
     out var parsed)` → `parsed` bei Erfolg, sonst `null`).
   - Filterprädikat um
     `(formatFilter == null || record.Format == formatFilter)` ergänzen —
     einfacher Gleichheitsvergleich, kein Gruppierungs-Set.
3. `src/MyMusic.Api/Endpoints/Sammlung/Record/RecordEndpoints.cs`:
   `GetPagedRecordsAsync` um Parameter `string? format` erweitern und in
   `GetPagedRecordsQuery` durchreichen (kein Default nötig, `null`/nicht
   parsebar bedeutet "kein Filter").
4. Tests:
   - `tests/MyMusic.Application.Tests/.../GetPagedRecordsQueryHandlerTests.cs`:
     neue Fälle nach dem bestehenden Filter-Capture-Muster (Filter greift bei
     gültigem Format-String case-insensitive, kein Filter bei `null`/
     unbekanntem Wert).
   - `tests/MyMusic.IntegrationTests/RecordEndpointsTests.cs`: bestehenden
     Filtertestfall um eine `format=CdAlbum`-artige Prüfung erweitern
     (Testdaten enthalten bereits Records unterschiedlichen Formats).
5. Wiki-Aktualisierung (nur `wiki/`, nicht `raw/`):
   - `wiki/architektur/api-endpunkte.md`: `format` in der
     `GET /records`-Zeile ergänzen.
   - `wiki/architektur/ui-ux-konzept.md`: Abschnitt „Filter-Leiste Records"
     präzisieren — Format-Filter ist ein natives Dropdown mit den 10
     `RecordFormat`-Werten (Default "Alle"), kein Toolbar-Umschalter, kein
     Gruppieren nach Vinyl/CD — löst die heute aufgefallene Lücke.
   - `wiki/user-stories/user-stories-record.md`: US-R2-Akzeptanzkriterien um
     den Format-Filter ergänzen.
   - `wiki/log.md`: neuer Eintrag oben.

## Umsetzung Frontend

Alle neuen Dateien unter `src/frontend/src/app/`.

1. **Country nach `shared/` verschieben**: `features/labels/country.ts`,
   `country.service.ts`, `country.service.spec.ts` →
   `shared/country/country.ts`, `country.service.ts`, `country.service.spec.ts`.
   Importpfade in `features/labels/*` (Labels-Shell, `LabelFilter`,
   `LabelForm`) anpassen. Kein Verhaltens-/Testinhalt ändert sich.
2. **`LabelService`/`ArtistService` um `getAll()` ergänzen**
   (`GET /api/labels/all` bzw. `/api/artists/all`, Rückgabe `Observable<Label[]>`/
   `Observable<Artist[]>`, analog zu `CountryService.getAll()`) + Erweiterung
   der bestehenden Service-Specs um je einen `getAll()`-Testfall.
3. **`features/records/record.ts`** (neu): `Record`-Interface (id, labelId,
   labelName, artistId, artistName, format, albumName, releaseYear,
   condition, information, albumCoverDataUrl, tracks — `tracks` typisiert,
   aber in diesem Block ungenutzt), `RecordListResponse`, Union-Types
   `RecordFormat`/`RecordCondition` (exakte Wire-Strings, siehe oben), sowie
   die Anzeige-Konstanten `RECORD_FORMAT_LABELS` (aus `glossar.md`, dient dem
   Format-Dropdown im Filter UND der Formatbeschriftung auf der Card),
   `VINYL_FORMATS`/`CD_FORMATS` (reine Frontend-Anzeigekonstante nur für die
   `.fmt`-Pille auf der Card — "LP"/"CD" —, hat mit dem Backend-Filter nichts
   zu tun), `RECORD_CONDITION_LABELS` (aus `zustandsbewertung.md`),
   `RECORD_CONDITION_GRADE_CLASS` (`grade-m` usw.), `RECORD_CONDITION_GRADE_TEXT`
   (M/NM/VG+/VG/G+/G/P).
4. **`features/records/record.service.ts`** (neu): `getPaged(page, pageSize,
   name?, artistId?, labelId?, yearFrom?, yearTo?, countryId?, format?:
   RecordFormat, sortBy?, sortDirection?)` — `format` nur angehängt, wenn
   gesetzt (truthy-Muster wie bei den übrigen Filtern, "Alle" liefert
   `undefined`, kein Sonderfall nötig). Plus `record.service.spec.ts` nach
   dem `label.service.spec.ts`-Muster (`HttpTestingController`, je Filter
   ein "wird nur bei gesetztem Wert angehängt"-Test, inkl. `format`).
5. **`features/records/record-filter/`** (neu): Signal Form mit `name`
   (debounce 300 ms), `artistId`/`labelId`/`countryId`/`format` als native
   `<select>` — `format` exakt nach dem Country-Filter-Muster ("Alle
   Formate" als leere Option + `RECORD_FORMAT_LABELS`-Einträge, keine
   Debounce), `artistId`/`labelId`/`countryId` mit Optionen aus
   `artistsResource`/`labelsResource`/`countriesResource` der Shell,
   `yearFrom`/`yearTo` als zwei `<input type="number" class="input">`.
   Sortierung: `<select>` "Name"/"Erscheinungsjahr"/"Format" (`sortBy`) + ein
   zweiter kleiner Auf/Ab-Button für `sortDirection` (Icon aus
   `@lucide/angular`, bereits installiert). Alles zusammen per `output()`
   als ein `RecordFilterValue`-Objekt an die Shell. Plus Spec.
6. **`features/records/record-card/`** (neu): rein präsentational,
   `input.required<Record>()`, `output<void>() opened` (Klick öffnet später
   die Detailseite — in diesem Block ohne Ziel, nur der Hook wird bereits
   angelegt, aber nicht verdrahtet). Markup nach den o.g.
   `komponenten-klassen.md`-Klassen: Cover-`<img>` bei
   `albumCoverDataUrl`, sonst Platzhalter-Icon in `.record-cover`; `.fmt`-
   Pille zeigt "LP"/"CD" je nach `VINYL_FORMATS`/`CD_FORMATS`-Zugehörigkeit;
   `.record-title` = Albumname; `.record-artist` nur falls `artistName`
   gesetzt; `.record-sub` = Jahr · Label + Grade-Badge
   (`RECORD_CONDITION_GRADE_CLASS`/`_GRADE_TEXT`). Plus Spec.
7. **`features/records/records.ts`/`.html`** (ersetzt Platzhalter):
   Filter-Signale + `page`-Signal, `rxResource` für die paginierte
   Record-Liste (Params aus allen Filtersignalen inkl. `format`,
   zurückgesetzt auf `page=1` bei Filteränderung — Muster aus
   `labels.ts`/`onFilterChange`), plus drei unabhängige `rxResource`-Aufrufe
   für `artistsResource`/`labelsResource`/`countriesResource`
   (`getAll()`-Aufrufe aus Schritt 2). Toolbar: Titel „Records" +
   `badge badge-neutral` mit `totalCount()` (kein "Platte hinzufügen"-Button
   in diesem Block — Anlegen folgt erst in Block 6g, ein nicht
   funktionierender Button wäre irreführend). Darunter `<app-record-filter>`
   in eigener Zeile. Darunter: Ladezustand (Spinner) /
   "Keine Daten vorhanden" / Card-Grid + `<app-pagination>` — Dreiklang
   analog zu `label-table.html`, aber auf ein Card-Grid statt `<table>`
   angewendet. Fehlerzustand per `effect()` + `ErrorModalService` wie bei
   Labels. Plus `records.spec.ts` (ersetzt den bisherigen Platzhalter-Test).
   `records.routes.ts` bleibt unverändert.

## Bewusst nicht Teil dieses Blocks

- Anlegen/Bearbeiten/Löschen eines Records (US-R4–R6, Block 6g).
- Detailseite `/records/:id` (US-R7, Block 6h) — `record-card` bekommt zwar
  bereits ein `opened`-Output, aber noch keine Navigation.
- Album-Cover-Upload (US-R8, Block 6i).
- Tracks (US-T1–T3, Block 6j).
- Manuelle Live-Prüfung im Browser gegen den laufenden Aspire-AppHost wird
  nach der Umsetzung empfohlen, ist aber kein Blocker für den Commit (wie in
  den meisten bisherigen Blöcken).

## Verifikation

- Backend: `dotnet build`, `dotnet test` (Domain/Application/Api),
  `dotnet format --verify-no-changes` — alles über PowerShell (CLAUDE.md
  §11, nie Git Bash). `MyMusic.IntegrationTests` (`RecordEndpointsTests`)
  separat mit laufendem Docker/Aspire.
- Frontend: `npm test` (Vitest, alle bestehenden + neuen Specs grün),
  Production-Build, Prettier-Check.
- Empfohlen (nicht blockierend): Live-Check im Aspire-AppHost — `/records`
  zeigt Cards, alle Filter inkl. Format-Dropdown wirken serverseitig, Empty/
  Loading-State korrekt, Paginierung funktioniert.

## Branch/Dokumentation

- Branch `block-6f-angular-records-liste` von `main`.
- `TASK.md` wird nach Abschluss um Abschnitt 6f ergänzt (inkl. Hinweis auf
  die Backend-Ausnahme und die Country-Verschiebung); Blöcke 6g–6j werden
  dort als geplant, aber noch offen vermerkt.
