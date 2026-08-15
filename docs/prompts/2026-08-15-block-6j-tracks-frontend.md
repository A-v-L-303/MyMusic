# Block 6j (Angular) — Slice Record/Tracks, Track-CRUD in der Detailansicht

## Kontext

Block 6j ist laut TASK.md der letzte der fünf Records-Frontend-Teilblöcke
(6f–6j). Block 6i (Album-Cover-Upload, PR #61) ist gemergt. Backend-seitig ist
Track-CRUD bereits seit Block 6c vollständig implementiert
(`POST/PUT/DELETE /api/records/{id}/tracks[/{trackId}]`) — **kein
Backend-Change** in diesem Block.

Die Angular-Detailansicht `RecordDetail` (Block 6h, `/records/:id`) zeigt
Tracks bisher rein lesend über `TrackList` an. Laut Domain-Vorgabe
(`wiki/domain/record-track.md`, „UI-Anforderungen") ist Track-Verwaltung
(hinzufügen, bearbeiten, entfernen) eine Unteransicht genau dieser
Detailansicht, kein eigener Reiter. Ziel: US-T1 bis US-T3 aus
`wiki/user-stories/user-stories-record.md` umsetzen.

## Mit dem Projektinhaber geklärt (2026-08-15)

**Wiki/Code-Konflikt bei der Fremdschlüssel-Auswahl**: Die Wiki-Klärung vom
2026-08-14 in `api-endpunkte.md` begründet die unpaginierten `/genres/all`-
und `/artists/all`-Endpunkte explizit als Dropdown-Grundlage für
„RecordForm, RecordTrack-Formulare". Das bereits umgesetzte `RecordForm`
weicht davon für Label/Artist ab (Autocomplete + `getPaged`, kein
`/all`-Dropdown). Für das Track-Formular gilt für diesen Block:

- **Genre**: `<select>`-Dropdown über den bereits bestehenden, im Frontend
  aber noch ungenutzten `GET /api/genres/all` — kleine, überschaubare Liste,
  folgt dem dokumentierten Wiki-Zweck. Kein neuer Backend-Code nötig
  (Endpunkt existiert seit Block 6e, `GenreEndpoints.cs`).
- **Artist**: Autocomplete über `getPaged` — identisch zum bewährten
  Artist-Feld in `RecordForm`, da Artist-Listen groß werden können; hält die
  Track-Artist-Auswahl konsistent zur Record-Artist-Auswahl.

Diese Aufteilung wird als Klärungsnotiz in `wiki/architektur/api-endpunkte.md`
ergänzt (Schritt 8 unten), damit das Wiki den tatsächlichen Stand
widerspiegelt.

**Kein Inline-„Artist neu anlegen"-Flow im Track-Formular** (anders als
`RecordForm`): US-T1/US-T2 verlangen nur eine Inline-Fehlermeldung bei
ungültiger Artist-/Genre-Referenz, keine Anlage-Möglichkeit aus dem Formular
heraus — hält den Scope eng.

## Ist-Stand (verifiziert)

- `features/records/record-detail/record-detail.ts`/`.html` ist reiner
  Lesemodus: lädt den Record über `rxResource({ stream: ({params}) =>
  recordService.getById(params.id) })`, zeigt `<app-track-list [tracks]=
  "record.tracks" />`. Ein bestehender Test (`record-detail.spec.ts`, „bietet
  keine Bearbeiten- oder Löschen-Aktion an (reiner Lesemodus)") prüft nur, dass
  **kein Button-Text** `'Bearbeiten'`/`'Löschen'` vorkommt — bezieht sich auf
  Record-Ebene (Record selbst bleibt weiterhin nicht aus dem Modal heraus
  bearbeit-/löschbar, dafür bleiben die Icons auf `RecordCard` zuständig).
  Neue Track-Buttons sind wie bei `RecordCard` reine Icon-Buttons ohne
  sichtbaren Text und dürfen diesen Test nicht brechen — nach der Umsetzung
  gezielt prüfen.
- `features/records/track-list/track-list.ts`/`.html` ist aktuell reine
  Anzeige (`tracks = input.required<RecordTrack[]>()`, Gruppierung nach
  `recordSide`, kein CRUD, keine Outputs).
- `features/records/record.ts`: `RecordTrack`-Interface bereits vorhanden
  (`id, recordId, artistId, artistName, genreId, genreName, trackName,
  recordSide, trackNumber, information`). **Kein** `CreateTrackRequest`/
  `UpdateTrackRequest` vorhanden — neu anzulegen.
- `features/records/record.service.ts`: `getById`, `create`, `update`,
  `delete`, `uploadCover` vorhanden (Muster: `HttpClient`, `Observable<T>`,
  `baseUrl` aus `RuntimeConfigService.apiBaseUrl` + `/api/records`, kein
  eigenes Error-Handling im Service). **Keine** Track-Methoden vorhanden.
- `features/genres/genre.service.ts`: `getPaged`, `create`, `update`,
  `delete` vorhanden. **Kein** `getAll()` — trotz bereits bestehendem Backend-
  Endpunkt `GET /api/genres/all` (seit Block 6e, liefert
  `IEnumerable<GenreResponse>` → im Frontend `Genre[]`, `{id, name}`). Muster
  für die neue Methode: `shared/country/country.service.ts`
  (`getAll(): Observable<Country[]> { return this.http.get<Country[]>
  (this.baseUrl); }`).
- `features/records/record-form/record-form.ts` liefert die Vorlage für das
  Artist-Autocomplete (Query-Signal, `rxResource` mit `artistService.
  getPaged(1, 10, query)`, Mapping auf `AutocompleteOption { id, label }`,
  `viewChild<Autocomplete>` für `setQuery(...)`), für den lokalen
  `ALBUM_NAME_PATTERN`-Regex (siehe unten) und für das
  Verschachtelungsmuster mehrerer Modals auf oberster Template-Ebene
  (`app-label-form`, `app-confirm-modal` stehen als Geschwister nach dem
  schließenden `</app-modal>`-Tag, gesteuert per `@if`).
- `features/genres/genre-form/genre-form.ts` liefert die Vorlage für ein
  einfaches Create/Edit-Formular in einer Komponente: `linkedSignal(() => ({
  ...initialValues }))` für das Formularmodell (Pflicht — ein reiner
  `signal(...)`-Initialisierer würde im Edit-Modus nicht vorbefüllen, siehe
  bekannter Bugfix vom 2026-08-13 bei `GenreForm`/`LabelForm`), `submit(form,
  save)`, `handleSaveError` mit Zuordnung des 400-Feldfehlers per
  PascalCase-Schlüssel auf das passende `FieldTree`.
- `features/records/record-card/record-card.ts`/`.html` liefert die Vorlage
  für Edit-/Delete-Icon-Buttons: `LucidePencil`/`LucideTrash2`,
  `event.stopPropagation()`, `[attr.aria-label]="'Record ' + record().
  albumName + ' bearbeiten'"` bzw. `' löschen'`.
- `shared/confirm-modal/confirm-modal.ts`: Inputs `title`, `message`,
  `confirmLabel` (Default „Löschen"), `confirmVariant` (Default `'danger'`);
  Outputs `confirmed`, `cancelled`. Für Track-Delete ohne weitere Anpassung
  nutzbar.
- `shared/error-modal/error-modal.service.ts`: `showFromHttpError(error,
  entityName, onRetry?)` mappt 404 → `not-found`-Modal, 400 → Inline
  (`extractFirstValidationError`, wird in Formularen aber durch die eigene
  `handleSaveError`-Feldzuordnung vorweggenommen), 409 → `conflict`-Modal
  (nutzt `ProblemDetails.detail`, den der Server bereits vorformuliert
  liefert), 500 → `server`-Modal, 401/403 → kein Modal (global behandelt).
- Backend-Vertrag Track (aus `RecordEndpoints.cs`, `CreateRecordTrackCommand
  Validator`, `RecordTrack.cs`, verbindlich):
  - `POST /api/records/{id}/tracks` Body `{ artistId, genreId, trackName,
    recordSide, trackNumber, information }` → `201`
    `RecordTrackResponse { id, recordId, artistId, artistName, genreId,
    genreName, trackName, recordSide, trackNumber, information }`.
  - `PUT /api/records/{id}/tracks/{trackId}` gleicher Body → `200` gleiche
    Form. `DELETE /api/records/{id}/tracks/{trackId}` → `204`.
  - Validierung (Fehlerschlüssel exakt `ArtistId`, `GenreId`, `TrackName`,
    `RecordSide`, `TrackNumber`, `Information`, alle → 400):
    - `artistId`/`genreId`: müssen einen Artist/ein Genre des angemeldeten
      Benutzers referenzieren (`MustAsync`-Prüfung gegen `IRepository<T>`,
      mandantengefiltert) — sonst 400 „Der angegebene Artist existiert
      nicht." / „Das angegebene Genre existiert nicht.".
    - `trackName`: Pflicht, 1–150 Zeichen, Regex
      `^[\p{L}\p{N} \-&'./()]+$` — **identisch** zu `ALBUM_NAME_PATTERN` aus
      `record-form.ts` (dort nicht exportiert, lokal in `track-form.ts`
      duplizieren statt `record-form.ts` anzufassen).
    - `recordSide`: Pflicht im Command (`NotEmpty`), aber der C#-Default
      `"0"` greift bereits, wenn das Feld im Request-Body fehlt — für den
      Client bedeutet das „optional mit Default `0`", **niemals** einen
      Leerstring senden. Max. 3 Zeichen, Regex `^[\p{L}\p{N}]{1,3}$` (nur
      Buchstaben/Ziffern).
    - `trackNumber`: Pflicht, Ganzzahl ≥ 1.
    - `information`: optional, max. 255 Zeichen.
  - Eindeutigkeit `(recordId, recordSide, trackNumber)` → **409** bei
    Verstoß, `detail` vom Server vorformuliert (`ConflictException`).
  - Fremder/nicht existierender Record **oder** Track → **404** bei
    Add/Update/Delete.
  - `GET /api/records/{id}` liefert den Record weiterhin inklusive aller
    Tracks in einer Antwort — nach jeder Track-Änderung reicht ein Reload der
    bestehenden `recordResource`, kein separater Track-Endpunkt zum Neuladen
    nötig.

## Vorgeschlagene Schritte

### 1. `GenreService.getAll()`

`features/genres/genre.service.ts`: neue Methode `getAll(): Observable<Genre[]>
{ return this.http.get<Genre[]>(\`${this.baseUrl}/all\`); }`, 1:1 nach Muster
`CountryService.getAll()`. Test in `genre.service.spec.ts` ergänzen
(`HttpTestingController`, URL `.../api/genres/all`, kein Query-Parameter).

### 2. `Record`-Modell erweitern

`features/records/record.ts`: neue Interfaces
```ts
export interface CreateTrackRequest {
  artistId: number;
  genreId: number;
  trackName: string;
  recordSide: string;
  trackNumber: number;
  information: string | null;
}
export interface UpdateTrackRequest extends CreateTrackRequest {}
```

### 3. `RecordService` um Track-Methoden erweitern

`features/records/record.service.ts`, nach dem Muster von `uploadCover`:
```ts
createTrack(recordId: number, request: CreateTrackRequest): Observable<RecordTrack> {
  return this.http.post<RecordTrack>(`${this.baseUrl}/${recordId}/tracks`, request);
}
updateTrack(recordId: number, trackId: number, request: UpdateTrackRequest): Observable<RecordTrack> {
  return this.http.put<RecordTrack>(`${this.baseUrl}/${recordId}/tracks/${trackId}`, request);
}
deleteTrack(recordId: number, trackId: number): Observable<void> {
  return this.http.delete<void>(`${this.baseUrl}/${recordId}/tracks/${trackId}`);
}
```
Tests in `record.service.spec.ts` ergänzen (URL, HTTP-Verb, Payload je
Methode).

### 4. `TrackForm` (neu)

`features/records/track-form/track-form.ts`/`.html`/`.spec.ts`. Inputs:
`track = input<RecordTrack | null>(null)`, `recordId =
input.required<number>()`. Outputs: `cancelled = output<void>()`, `saved =
output<void>()` (Reload übernimmt `RecordDetail`, siehe Schritt 6).

Formularmodell (Signal Forms, `linkedSignal` für Vorbefüllung im Edit-Modus):
`{ artistId: string; genreId: string; trackName: string; recordSide: string;
trackNumber: string; information: string }`, Default `recordSide: '0'`.

Validierung (Werte/Regex exakt wie Backend, siehe Ist-Stand):
- `required` auf `artistId`, `genreId`, `trackName`, `trackNumber`.
- `maxLength(trackName, 150)`, `pattern(trackName, TRACK_NAME_PATTERN)`
  (lokale Konstante, gleicher Regex wie `ALBUM_NAME_PATTERN`).
- `maxLength(recordSide, 3)`, `pattern(recordSide, /^[\p{L}\p{N}]{1,3}$/u)`.
- `validate(trackNumber, ...)`: ganzzahlig und ≥ 1, deutsche Fehlermeldung
  analog `releaseYear`-Validierung in `record-form.ts`.
- `maxLength(information, 255)`.

Artist-Feld: Autocomplete-Block 1:1 aus `record-form.ts` übernehmen (Query-
Signal, `rxResource` mit `artistService.getPaged(1, 10, query)`,
`AutocompleteOption`-Mapping, `viewChild<Autocomplete>` für `setQuery`,
`[initialQuery]="track()?.artistName ?? ''"`), **ohne** die
Inline-„Artist neu anlegen"-Rückfrage (siehe Klärung oben).

Genre-Feld: natives `<select class="select">` mit `[formField]=
"trackForm.genreId"`, Optionen aus `genresResource = rxResource({ stream: ()
=> genreService.getAll() })`, Platzhalter-Option „Bitte wählen" mit leerem
Wert.

`save()`: `recordService.createTrack(recordId(), request)` bzw.
`updateTrack(recordId(), track()!.id, request)`. `handleSaveError` mappt
400-Feldfehler (`ArtistId`, `GenreId`, `TrackName`, `RecordSide`,
`TrackNumber`, `Information`) auf das jeweilige `FieldTree`, alles andere
(404/409/500) über `errorModalService.showFromHttpError(error, 'Track')` —
409 landet damit korrekt als Konflikt-Modal (Fehlerkonzept-Tabelle,
CLAUDE.md §7), nicht inline.

### 5. `TrackList` um Edit/Delete-Outputs erweitern

`features/records/track-list/track-list.ts`: neue Outputs `editRequested =
output<RecordTrack>()`, `deleteRequested = output<RecordTrack>()`, Import
`LucidePencil`, `LucideTrash2`. `track-list.html`: pro Zeile zwei
Icon-Buttons analog `record-card.html`
(`[attr.aria-label]="'Track ' + track.trackName + ' bearbeiten'"` bzw.
`' löschen'`). Bestehende Tests bleiben gültig; neue Testfälle für beide
Outputs ergänzen.

### 6. `RecordDetail` um Track-CRUD erweitern

`features/records/record-detail/record-detail.ts`: neue Signals
`trackFormOpen = signal(false)`, `editingTrack = signal<RecordTrack | null>
(null)`, `pendingDeleteTrack = signal<RecordTrack | null>(null)`,
`pendingDeleteTrackMessage = computed(...)`. Handler:
- `onAddTrackClicked()`: `editingTrack.set(null); trackFormOpen.set(true)`.
- `onEditTrackRequested(track)`: `editingTrack.set(track);
  trackFormOpen.set(true)`.
- `onTrackFormCancelled()`/`onTrackFormSaved()`: schließen Formular;
  `onTrackFormSaved()` ruft zusätzlich `recordResource.reload()` (lädt
  Record inkl. aktualisierter Tracks neu).
- `onDeleteTrackRequested(track)`/`onDeleteTrackCancelled()`.
- `onDeleteTrackConfirmed()`: `recordService.deleteTrack(id(), track.id)
  .subscribe({ next: () => { pendingDeleteTrack.set(null);
  recordResource.reload(); }, error: (e) => { pendingDeleteTrack.set(null);
  errorModalService.showFromHttpError(e, 'Track'); } })`.

`record-detail.html`: „Track hinzufügen"-Button (`LucidePlus`) im
`modal-body` bei `<app-track-list>`, verdrahtet mit
`(editRequested)="onEditTrackRequested($event)"`,
`(deleteRequested)="onDeleteTrackRequested($event)"`. `TrackForm` und
`ConfirmModal` als Geschwister-Elemente nach dem schließenden `</app-modal>`
(exaktes Verschachtelungsmuster aus `record-form.html`):
```html
@if (trackFormOpen()) {
  <app-track-form
    [track]="editingTrack()"
    [recordId]="id()"
    (cancelled)="onTrackFormCancelled()"
    (saved)="onTrackFormSaved()"
  />
}
@if (pendingDeleteTrack()) {
  <app-confirm-modal
    title="Track löschen"
    [message]="pendingDeleteTrackMessage()"
    (confirmed)="onDeleteTrackConfirmed()"
    (cancelled)="onDeleteTrackCancelled()"
  />
}
```
Neue Imports in `record-detail.ts`: `TrackForm`, `ConfirmModal`,
`LucidePlus`.

### 7. Tests

Durchgängig je Schritt, `// arrange`/`// act`/`// assert`-Pflichtkommentare.
`track-form.spec.ts`: Create-Happy-Path, Edit-Happy-Path (Vorbefüllung aller
Felder inkl. `recordSide`/`trackNumber`, `linkedSignal` korrekt geprüft),
Pflichtfeld- und Pattern-Validierung je Feld, 400-Feldfehler-Zuordnung je
Schlüssel, 409 → Konflikt-Modal, 404 → Modal. `track-list.spec.ts`: Klick auf
Bearbeiten-/Löschen-Icon emittiert den korrekten Track. `record-detail.spec.ts`:
Add-Button öffnet `TrackForm` im Create-Modus (kein `track`-Input),
Bearbeiten-Icon öffnet `TrackForm` vorbefüllt, Löschen-Icon + Bestätigung löst
`DELETE` aus und lädt den Record neu; **gezielt verifizieren**, dass der
bestehende Test „bietet keine Bearbeiten- oder Löschen-Aktion an (reiner
Lesemodus)" weiterhin grün bleibt.

### 8. Dokumentation

- `wiki/architektur/api-endpunkte.md`: Klärungsnotiz unter der bestehenden
  „Klärung 2026-08-14"-Passage ergänzen (Datum 2026-08-15): `RecordForm`
  nutzt für Label/Artist Autocomplete statt `/all`-Dropdown (bestehende
  Abweichung vom ursprünglichen Zweck); das Track-Formular löst Genre über
  `/genres/all` (Dropdown) und Artist über Autocomplete.
- `wiki/log.md`: neuer Eintrag oben.
- `TASK.md`: Abschnitt „Aktuell nicht umgesetzt" aktualisieren, neuer
  Unterabschnitt „6j. Slice: Records-Frontend – Tracks" nach Muster der
  Abschnitte 6f–6i, Branch-Zeile um PR-Nummer ergänzen (nach PR-Erstellung).
- `CLAUDE.md` (Repo-Root, §4.1 Iststand): Track-CRUD in der Detailansicht als
  abgeschlossen ergänzen.

## Verifikation

1. `npm run build` in `src/frontend` — Production-Build erfolgreich.
2. `npm test -- --watch=false` — alle Frontend-Tests grün, insbesondere
   `record-detail.spec.ts` (inkl. „reiner Lesemodus"-Test), `track-list.spec.ts`,
   `track-form.spec.ts` (neu), `record.service.spec.ts`, `genre.service.spec.ts`.
3. Prettier-Check (`printWidth: 100`, `ng lint` weiterhin nicht konfiguriert,
   siehe TASK.md-Hinweis zu Block 2 Frontend); `git diff --check`
   (Zeilenlänge 120 Zeichen).
4. Manuelle Live-Prüfung im Browser über den Aspire-AppHost: Track
   hinzufügen (erscheint in der Tracklist), Track bearbeiten (Vorbefüllung
   korrekt inkl. Seite/Nummer), Track löschen mit Bestätigung, Abbrechen löst
   keinen HTTP-Call aus, Konfliktfall (gleiche Seite+Nummer zweimal) →
   Konflikt-Modal, Pflichtfeld leer → Inline-Fehler, Netzwerkfehler
   simulieren.

## Risiken und offene Punkte

- Die Wiki-Klärung vom 2026-08-14 sah `/all`-Dropdowns für **beide**
  Fremdschlüssel (Artist und Genre) in „RecordTrack-Formularen" vor; dieser
  Block setzt nur Genre so um, Artist bleibt Autocomplete (siehe Klärung
  oben). Das Wiki wird entsprechend nachgezogen (Schritt 8), nicht
  stillschweigend.
- Die manuelle Live-Prüfung des 409-Konfliktfalls setzt voraus, dass bereits
  mindestens zwei Tracks mit derselben Seite existieren — ggf. gezielt zwei
  Tracks mit identischer Seite/Nummer anlegen, um den Fall auszulösen.
- Rate Limiting (429) bleibt wie bei allen bisherigen Slices backendseitig
  nicht gezielt verifizierbar.
