# Block 5 (Angular) — Slice Artist, Frontend

## Kontext

Das Artist-Backend ist seit 2026-08-07 abgeschlossen (`ArtistEndpoints`,
`/api/artists`, vollständig getestet). Laut `TASK.md` Abschnitt 5 war das
Angular-Feature `artists/` zurückgestellt, bis Block 0c (Angular-Workspace)
und ein Referenz-Slice vorliegen. Beides ist erfüllt: Genre (Block 2
Frontend, PR #47) hat die Muster etabliert (Signal Forms, `rxResource`,
`ErrorModalService`, `shared/`-Bausteine), Label (Block 4 Frontend, PR #49)
hat sie ein zweites Mal bestätigt. `features/artists/` enthält aktuell nur
eine leere Platzhalter-Komponente (`artists.ts/.html/.routes.ts/.spec.ts`);
Route und Nav-Eintrag („Artists", `LucideUsers`, `nav.html:34-40`) sind
bereits vollständig verdrahtet (Block 0g) — daran ändert sich nichts.

**Kein Backend-Change.** Das Artist-Backend wird unverändert konsumiert.

Anders als Label hat Artist **keinen Fremdschlüssel auf sich selbst** — die
`artist`-Tabelle hat keine `label_id`-Spalte. Der Backend-Endpoint
`GET /api/artists` unterstützt zwar seit Block 6d (PR #34) einen
`labelId`-Query-Parameter (löst die Beziehung indirekt über
`record.label_id` auf, siehe `ArtistEndpoints.cs:29`,
`GetPagedArtistsQuery.cs`), aber:

**Mit dem Projektinhaber geklärt (2026-08-13)**: Der `labelId`-Filter wird in
diesem Block **nicht** als UI umgesetzt. Grund: Anders als bei Country (238
Einträge, `GET /countries` liefert ungefiltert alle) gibt es für Label
keinen ungefilterten „Alle Labels"-Endpunkt — nur das paginierte
`GET /labels` (serverseitig auf max. 100 Einträge geklemmt). Eine
Dropdown-Quelle dafür wäre entweder unvollständig (Cap bei 100) oder
bräuchte einen neuen Backend-Endpoint (out of scope). Der Artist-Slice
bekommt daher **nur den Namensfilter** — strukturell macht das Artist zu
einem fast unveränderten Duplikat von **Genre**, nicht von Label (wie schon
beim Backend in `docs/prompts/2026-08-07-block-5-artist.md` festgehalten).
Die offene UI-Lücke wird dokumentiert (siehe Schritt 8), nicht stillschweigend
fallengelassen.

**Ist-Stand geprüft (nicht nur aus Doku übernommen)**: `ArtistEndpoints.cs`,
`GetPagedArtistsQuery.cs`, `ArtistResponse.cs`, `ArtistListResponse.cs`,
`CreateArtistCommandValidator.cs` und `DeleteArtistCommandHandler.cs`
direkt gelesen — API-Vertrag unten ist damit gegen den tatsächlichen Code
verifiziert, nicht nur aus `TASK.md`/Wiki übernommen (die an dieser Stelle
teils veraltet sind, siehe „Beobachtete Doku-Abweichungen" unten).

### API-Vertrag Artist (aus Code verifiziert)

- `GET /api/artists?page&pageSize&name` → `200 ArtistListResponse { items:
  {id, name}[], totalCount, page, pageSize, totalPages }`. `name`:
  case-insensitive Contains-Filter, optional. Sortierung serverseitig fest
  nach Name (kein `sortBy`, wie Genre). `page`/`pageSize` serverseitig
  geklemmt (min 1 / 1–100). (`labelId`-Parameter existiert serverseitig,
  wird von diesem Frontend-Slice bewusst nicht gesendet.)
- `POST /api/artists` Body `{ name }` → `201 { id, name }`.
  `PUT /api/artists/{id}` Body `{ name }` → `200 { id, name }`.
  `DELETE /api/artists/{id}` → `204`.
- Validierung (`CreateArtistCommandValidator`/`UpdateArtistCommandValidator`):
  `name` Pflicht, **3–120 Zeichen** (nicht 3–50 wie Genre), Regex
  `^[\p{L}\p{N} \-&'./]+$` (JS: `/^[\p{L}\p{N} \-&'./]+$/u` — gegenüber
  Genre zusätzlich `.` und `/`, bewusst ohne Klammern, identisch zu Label).
- 400-Fehlerformat wie Genre: `{ errors: { Name: string[] }, title, status
  }`. 404/409/500: `{ title, detail, status }`. 409 bei Create/Update =
  Name-Duplikat pro Benutzer. 409 bei Delete = Artist wird noch von
  mindestens einem `Record` **oder** `RecordTrack` referenziert
  (`DeleteArtistCommandHandler.cs`, zwei getrennte Existenzprüfungen,
  Meldung bereits serverseitig im `detail`-Feld vorformuliert).
- Fehlerdarstellung exakt wie Genre: 400 → Inline am Namensfeld. 404/409/500
  → Modal mit OK. Netzwerkfehler → Modal mit „Erneut versuchen". Delete →
  `ConfirmModal` zuerst, 409 danach im `ErrorModalService` (identisches
  Muster wie beim Label-409-Referenzfall, siehe dortiges Risiko).

### Beobachtete Doku-Abweichungen (zu melden, nicht stillschweigend zu lösen)

- `README.md` Abschnitte „Genre-Slice (Block 2)" (Zeile 205) und
  „Label-Slice (Block 4)" (Zeile 244) behaupten weiterhin „Das
  Angular-Feature … folgt erst mit Block 0c" — das ist seit PR #47/#49
  überholt. Wird in diesem Block **nicht mitkorrigiert** (nicht Teil des
  Artist-Scopes), aber dem Projektinhaber gemeldet.
- Repo hat aktuell zwei unstaged Änderungen auf `main`
  (`CLAUDE.md`, `src/frontend/public/runtime-config.json` — Letzteres ist
  das per `prestart`-Skript generierte, nicht versionierungsrelevante
  Laufzeit-Artefakt). Diese bleiben unangetastet; der neue Feature-Branch
  wurde vom aktuellen Arbeitsstand von `main` abgezweigt (git behält
  unstaged Änderungen beim Branch-Wechsel automatisch bei).

## Vorgeschlagene Schritte

### 0. Branch und Arbeits-Prompt

- Branch `block-5-angular-artist` von `main` abgezweigt.
- Dieser Plan als `docs/prompts/2026-08-13-block-5-angular-artist.md`
  archiviert (CLAUDE.md §2.3 Punkt 2 — kein Code vor Arbeits-Prompt).

### 1. Artist-Modell und -Service (1:1 Muster `genre.ts`/`genre.service.ts`)

`features/artists/artist.ts`: `Artist { id, name }`, `ArtistListResponse`,
`CreateArtistRequest { name }`, `UpdateArtistRequest { name }` — identisch
zu `features/genres/genre.ts`.

`features/artists/artist.service.ts`/`.spec.ts`: `ArtistService`
(`providedIn: 'root'`), `getPaged(page, pageSize, name?)`, `create`,
`update(id, req)`, `delete(id)`, `baseUrl` aus
`RuntimeConfigService.apiBaseUrl` + `/api/artists` — Kopie von
`genre.service.ts`/`genre.service.spec.ts` mit ausgetauschten Bezeichnern.

### 2. `Artists`-Shell mit `rxResource` (1:1 Muster `genres.ts`/`.html`)

`features/artists/artists.ts`/`.html`/`.spec.ts` ersetzt den bestehenden
Platzhalter vollständig. Signals `filterName`/`page`; `artistsResource =
rxResource({ params: () => ({page: this.page(), pageSize: PAGE_SIZE, name:
this.filterName()}), stream: ({params}) => this.artistService.getPaged(...)
})`. `formOpen`/`editingArtist`, `pendingDelete`/`pendingDeleteMessage`,
`effect()` auf `artistsResource.error()` → `ErrorModalService` mit Retry —
Struktur identisch zu `Genres` (`features/genres/genres.ts`).

### 3. `ArtistFilter` (1:1 Muster `genre-filter.ts`/`.html`)

`features/artists/artist-filter/artist-filter.ts`/`.html`/`.spec.ts`:
Namensfeld mit `debounce(path.name, 300)`, `filterChange`-Output — Kopie von
`genre-filter.ts`. Kein Land-/Label-`<select>` (siehe Kontext).

### 4. `ArtistTable` mit Paginierung (1:1 Muster `genre-table.ts`/`.html`)

`features/artists/artist-table/artist-table.ts`/`.html`/`.spec.ts`: Spalten
Name, Aktionen; Loading/Empty/gefüllt, `editRequested`/`deleteRequested`,
eingebettete `<app-pagination>` — strukturell identisch zu `GenreTable`.

### 5. `ArtistForm` mit Signal-Forms-Validierung (1:1 Muster `genre-form.ts`/`.html`)

`features/artists/artist-form/artist-form.ts`/`.html`/`.spec.ts`: `artist:
Artist | null`-Input, `formModel` via `linkedSignal(() => ({ name:
this.artist()?.name ?? '' }))` (**nicht** `signal(...)` — der bei Genre
ursprünglich verwendete, mit `docs/prompts/2026-08-13-fix-genreform-vorbefuellung.md`
bereits behobene Bug wird hier von Anfang an vermieden). Signal Form mit:

- `required(path.name, ...)`
- `minLength(path.name, 3, ...)`
- `maxLength(path.name, 120, ...)` — **nicht 50** wie Genre
- `pattern(path.name, /^[\p{L}\p{N} \-&'./]+$/u, ...)` — **inkl. `.`/`/`**
  wie Label, nicht wie Genre

`submit(artistForm, action)`: Erfolg → `saved`-Output; HTTP 400 → Server-
Fehler auf `Name`-Feldschlüssel inline zugeordnet (`handleSaveError`-Muster
1:1 aus `genre-form.ts`); 404/409/500/Netzwerkfehler → `ErrorModalService`.

### 6. `ConfirmModal`-Verdrahtung für Delete

Identisch zum Genre-Muster: Löschen-Icon setzt `pendingDelete`, öffnet
`ConfirmModal` mit Artistnamen, Bestätigen löst `ArtistService.delete(id)`
aus, danach `artistsResource.reload()`; 409 (referenziert) oder 404
(zwischenzeitlich gelöscht) gehen an `ErrorModalService`; Abbrechen löst
keinen HTTP-Call aus.

### 7. Tests

Durchgängig je Schritt, `// arrange`/`// act`/`// assert`-Pflichtkommentare,
`await fixture.whenStable()` nach jedem `flush()` (Zoneless-Pflicht). Analog
zum Genre-Testumfang, zusätzlich:

- Validierungstest für die erweiterte Zeichenmenge (`.` und `/` werden
  akzeptiert, Klammern weiterhin abgelehnt) — Genre-Tests decken das nicht
  ab, da Genre diese Zeichen nicht erlaubt.
- Formulartest „Bearbeiten-Modus befüllt das Namensfeld vor" (analog zum
  Label-Fix, aber hier von Anfang an mit `linkedSignal` korrekt implementiert
  — kein Bug zu reproduzieren, aber der Test bleibt sinnvoll als
  Regressionsschutz).
- Lösch-Test für den 409-Referenzfall mit dem serverseitig gelieferten
  `detail`-Text (Record- **oder** Track-Referenz).

### 8. Dokumentation

- `TASK.md`: Abschnitt 5 (Artist) auf „Backend und Frontend abgeschlossen"
  setzen, analog zu Abschnitt 2/4; Kopfzeile und „Aktuell nicht umgesetzt"
  aktualisieren.
- `README.md`: Abschnitt „Artist-Slice (Block 5)" (Zeile 247–272) um einen
  Hinweis ergänzen, dass das Angular-Feature `artists/` jetzt existiert,
  **ohne** UI für den `labelId`-Filter (bewusste Abgrenzung, siehe Kontext) —
  nach demselben Muster wie ein künftiges Update von Genre/Label nachgezogen
  werden müsste (siehe „Beobachtete Doku-Abweichungen").
- `docs/prompts/2026-08-13-block-5-angular-artist.md`: dieser Plan, wie in
  Schritt 0 archiviert.
- Kein Wiki-Update nötig für ein neues UI-Muster (kein neuer Präzedenzfall,
  reine Wiederholung von Genre) — die Entscheidung „`labelId`-Filter-UI
  bewusst zurückgestellt" wird stattdessen in diesem Arbeits-Prompt unter
  „Risiken" festgehalten.

## Verifikation

1. `npm run build` in `src/frontend` — Production-Build erfolgreich.
2. `npm test -- --watch=false` — alle Frontend-Tests grün.
3. Prettier-Check (`ng lint` weiterhin nicht konfiguriert, wie bei
   Genre/Label); `git diff --check` (Zeilenlänge 120 Zeichen).
4. Manuelle Live-Prüfung im Browser über den Aspire-AppHost: Liste laden,
   nach Name filtern, paginieren, Artist anlegen (inkl. Inline-400 bei
   ungültigem Namen, inkl. Test mit `.`/`/`-Zeichen), Duplikat anlegen
   (409-Modal), Artist bearbeiten, Artist löschen mit Bestätigung, Löschen
   abbrechen, Netzwerkfehler simulieren. Der 409-Referenzfall (Artist wird
   von einem Record/Track verwendet) ist nur prüfbar, wenn bereits ein
   Record mit diesem Artist existiert — Record-Frontend ist noch
   Platzhalter, daher ggf. Nachweis über Swagger statt UI (wie bei Label).

## Risiken und offene Punkte

- **`labelId`-Filter-UI bewusst nicht umgesetzt**: Das Backend unterstützt
  ihn bereits; sobald eine belastbare Quelle für „alle Labels des Benutzers"
  existiert (z. B. weil ein anderer Slice einen unpaginierten
  Label-Endpoint braucht, oder eine Such-Combobox eingeführt wird), sollte
  der Filter nachgezogen werden. Bis dahin bleibt US-A2
  (`user-stories-artist.md`) im Frontend nicht vollständig erfüllt.
- 409-Referenzfall beim Löschen nur über Swagger nachweisbar (kein
  Record-Frontend).
- Sortierungs-UI-Muster weiterhin offen (unverändert gegenüber Genre/Label).
- README-Abschnitte zu Genre/Label sind bzgl. Angular-Feature-Status veraltet
  — separate Korrektur, nicht Teil dieses Blocks.
