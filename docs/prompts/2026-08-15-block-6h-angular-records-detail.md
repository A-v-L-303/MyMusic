# Block 6h: Record-Detailansicht als Modal (US-R7)

Freigegeben am 2026-08-15. Branch: `block-6h-angular-records-detail`.

## Kontext

`records/` hat mit Block 6f (Liste) und 6g (Anlegen/Bearbeiten/Löschen) die
Grundfunktionen erhalten. Dieser Block deckt US-R7 (Detailansicht mit
Tracklist, `wiki/user-stories/user-stories-record.md`) ab — ohne
Cover-Upload (US-R8, Block 6i) und ohne Track-CRUD (US-T1–T3, Block 6j).

Backend-seitig ist `GET /api/records/{id}` bereits vollständig vorhanden
(`RecordEndpoints.cs`, `GetRecordByIdQueryHandler.cs`) und liefert
`RecordResponse` inkl. `Tracks` (sortiert nach `RecordSide`, `TrackNumber`)
und `AlbumCoverDataUrl`. **Keine Backend-Änderungen** nötig — reiner
Frontend-Block.

Die Detailansicht ist laut Design ein **Modal**, kein eigenes
Vollbild-Layout. Beleg: `wiki/design/ui-kit.md` dokumentiert unter
„Enthaltene Oberflächen" ein „Record-Detail-Modal" ("Pressungs-Details +
Tracklist; Edit / Delete-Aktionen"), und der zugehörige Design-Prototyp-
Screenshot (`raw/photo_2026-05-30_17-48-03 (2).jpg`) zeigt: das Records-Grid
bleibt im Hintergrund sichtbar (abgedunkelt), darüber ein zentrierter Dialog
mit ×-Schließen-Button, Cover, Titel, Artist, Jahr/Label, Format-/Grade-/
Genre-Badges, Tracklist-Sektion (Seite/Nummer, Trackname, Dauer) und
Footer-Buttons „Löschen" und „Bearbeiten".

`/records/:id` bleibt dabei eine echte, verlinkbare Route (erklärt den
Zurück-Link/Browser-Back-Button aus `ui-ux-konzept.md` und die Formulierung
in `wiki/architektur/angular-projektstruktur.md`), wird aber als **Kind-Route
von `/records`** realisiert: Die Record-Liste bleibt im Hintergrund gemountet,
`RecordDetail` rendert als Modal darüber — genau das im Screenshot gezeigte
Bild.

Edit/Delete-Aktionen sind laut Screenshot Teil des Modals — dafür wird die in
Block 6g bereits bestehende Verdrahtung wiederverwendet (`RecordForm`-Modal,
`ConfirmModal`), nicht neu gebaut.

**Wichtiger Fund aus der Analyse**: `RecordResponseBuilder.BuildPaged(...)`
setzt `tracks: []` für Listenelemente — das per Klick übergebene
Listenobjekt aus der Card hat nie echte Tracks. `RecordDetail` muss deshalb
zwingend per `getById` neu laden, darf das Listenobjekt nicht weiterverwenden.

`app.config.ts` aktiviert `withComponentInputBinding()` nicht (nur
`provideRouter(routes)`) — der Routenparameter wird deshalb wie in
`features/search/search.ts` über `ActivatedRoute` + `toSignal(...)` gelesen,
nicht per Component-Input-Binding.

## Umsetzung

### 1. `features/records/record.service.ts` (geändert)

`getById` ergänzen, analog zu den bestehenden Methoden:

```ts
getById(id: number): Observable<Record> {
  return this.http.get<Record>(`${this.baseUrl}/${id}`);
}
```

Tests in `record.service.spec.ts` ergänzen: GET gegen `/api/records/1`,
Response-Mapping, sowie ein 404-Fehler-Fall, der den Fehler durchreicht
(analog zum bestehenden 500-Test).

### 2. `features/records/records.routes.ts` (geändert)

`:id` als Kind-Route von `''`, damit `Records` gemountet bleibt und
`RecordDetail` über dem Grid rendert:

```ts
export const recordsRoutes: Routes = [
  {
    path: '',
    component: Records,
    children: [{ path: ':id', component: RecordDetail }],
  },
];
```

### 3. `features/records/records.ts` + `records.html` (geändert)

`Router` injizieren, `record-card`s bestehendes `(opened)`-Output verdrahten
(bisher ungenutzt):

```ts
protected onRecordOpened(record: Record): void {
  this.router.navigate(['/records', record.id]);
}
```

`records.html`: `(opened)="onRecordOpened(record)"` an `app-record-card`
ergänzen; `<router-outlet />` ergänzen, damit die Kind-Route `RecordDetail`
darüber rendert.

**Testfolge**: Sobald `Records` einen `Router` injiziert, benötigen alle
bestehenden Tests in `records.spec.ts` `provideRouter(...)` in den
TestBed-Providern, sonst schlägt die Komponentenerzeugung fehl — betrifft
den gesamten `beforeEach`-Block, nicht nur den neuen Test. Neuer Testfall:
Klick auf `.record-card` löst Navigation zu `/records/1` aus.

### 4. `features/records/record-detail/` (neu)

`record-detail.ts`, `record-detail.html`, `record-detail.spec.ts`.

Nutzt `shared/modal/modal.ts` als Hülle (liefert ESC- und
Scrim-Click-Schließen bereits mit). `ActivatedRoute` + `toSignal(
route.paramMap.pipe(map(params => params.get('id'))), { initialValue: null })`
für die Id, `rxResource` lädt `recordService.getById(id)`.
Fehlerbehandlung wie in `genres.ts`: `effect()` ruft bei Fehler
`errorModalService.showFromHttpError(error, 'Record', () => reload())`.
**Fund während der Umsetzung**: `ErrorModalService`/`ErrorModal` bieten den
„Erneut versuchen"-Button ausschließlich für `kind === 'network'` an
(`error-modal.html` Zeile 13–18) — für `kind === 'server'` (HTTP 500) gibt es
nur einen „OK"-Button, der `onRetry` verworfen wird
(`error-modal.service.ts`, `mapToState`). Der `onRetry`-Callback wird trotzdem
übergeben (konsistent mit allen anderen Slices), ist aber für 500 aktuell
funktionslos — kein neues Verhalten für diesen Block, nur bestehendes Muster
übernommen. Schließen (×, ESC, Scrim-Click) navigiert per
`router.navigate(['/records'])` zurück.

Inhalt: Cover (Bild oder Platzhalter wie `record-card.html`), Albumname,
Künstler (falls vorhanden), Format-Badge und Grade-Badge
(`RECORD_FORMAT_LABELS`, `RECORD_CONDITION_GRADE_CLASS`/`_TEXT` aus
`record.ts` wiederverwenden — `Record` hat kein eigenes Genre-Feld, das
dritte Badge aus dem Screenshot ist illustrative Mockup-Ausschmückung ohne
Entsprechung in `RecordResponse`), Jahr, Label, `information` falls vorhanden,
`<app-track-list [tracks]="record.tracks" />`.

Reiner Lesemodus — kein „Löschen"/„Bearbeiten" im Modal (mit dem
Projektinhaber während der Live-Verifikation geklärt: Bearbeiten und
Löschen sollen bei geöffnetem Detail-Modal nicht möglich sein; beides bleibt
ausschließlich über die Icons auf der Record-Card in der Liste erreichbar,
siehe Block 6g).

### 5. `features/records/track-list/` (neu)

`track-list.ts`, `track-list.html`, `track-list.spec.ts`.

`readonly tracks = input.required<RecordTrack[]>()`, `computed` gruppiert
nach `recordSide` in Einfüge-Reihenfolge (Backend liefert bereits sortiert:
Seite, dann Tracknummer — kein eigenes Sortieren im Frontend nötig).
`information`-Feld pro Track anzeigen, wenn vorhanden (analog zu
`record.information`). Leerzustand-Text, falls keine Tracks vorhanden (zum
Zeitpunkt von 6h der Regelfall, da Track-Anlegen erst 6j folgt).

## Nicht Teil dieses Blocks

- Album-Cover-Upload (US-R8, Block 6i).
- Track hinzufügen/bearbeiten/löschen (US-T1–T3, Block 6j) — Tracklist ist
  in 6h rein lesend.
- Backend-Änderungen (bereits vollständig vorhanden).

## Verifikation

- `npm test` (Vitest) — alle Frontend-Tests grün, inkl. neuer Fälle.
- `npm run build` (Production-Build) grün.
- Prettier-Check (`printWidth: 100`).
- `git status`/`git diff --stat` vor Abschluss prüfen, dass nur
  Frontend-Dateien geändert wurden.
- Live-Verifikation im Browser gegen den laufenden Aspire-AppHost: Klick auf
  eine Record-Karte öffnet das Detail-Modal über dem weiterhin sichtbaren
  Grid (URL wechselt zu `/records/:id`); Cover, Stammdaten, Grade-Badge,
  Tracklist korrekt; kein „Bearbeiten"/„Löschen" im Modal vorhanden; Schließen
  (×, ESC, Klick außerhalb) führt zurück zu `/records`; Aufruf einer fremden/
  unbekannten Id zeigt das „Record nicht gefunden"-Modal.

## Dokumentation nach Abschluss

TASK.md (Abschnitt 6h ergänzen, Status), README.md falls betroffen. Kein
neuer ADR erwartet (Kind-Route-Modal-Muster ist eine Umsetzung bestehender
Wiki-Vorgaben, kein neuer Trade-off) — endgültig beim Schreiben entscheiden.
