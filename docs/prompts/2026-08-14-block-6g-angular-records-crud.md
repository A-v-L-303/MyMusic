# Block 6g: Record anlegen/bearbeiten/löschen (US-R4–R6)

Freigegeben am 2026-08-14. Branch: `block-6g-angular-records-crud`.

## Kontext

`records/` hat mit Block 6f (PR #55, gemergt) die Card-Ansicht mit Filter,
Sortierung und Paginierung erhalten (US-R1–R3). Laut TASK.md ist das Records-
Frontend auf Wunsch des Projektinhabers in fünf Teilblöcke 6f–6j zerlegt;
6f ist abgeschlossen, 6g–6j sind geplant, aber noch nicht begonnen. Dieser
Block deckt US-R4 (Anlegen), US-R5 (Bearbeiten) und US-R6 (Löschen) ab —
ohne Detailseite (US-R7, Block 6h), ohne Cover-Upload (US-R8, Block 6i),
ohne Tracks (Block 6j).

Das Backend ist bereits vollständig vorhanden (Block 6a, verifiziert):
`POST /api/records`, `PUT /api/records/{id}`, `DELETE /api/records/{id}`
samt Commands, Validatoren und Domain-Regeln
(`src/MyMusic.Domain/DomainModels/Sammlung/Record/Record.cs`,
`CreateRecordCommandValidator.cs`, `UpdateRecordCommandValidator.cs`). Es
sind **keine Backend-Änderungen** nötig — reiner Frontend-Block.

Zwei Design-Lücken wurden mit dem Projektinhaber vorab geklärt (2026-08-14):

1. **Edit/Delete-Trigger**: `RecordCard` hat bisher nur ein `opened`-Output
   ohne Aktions-Buttons. Entschieden: Icon-Buttons (Stift/Papierkorb) direkt
   auf der Card, analog zur Aktionsspalte der Tabellen-Slices
   (`.btn.btn-ghost.btn-icon.btn-sm`, `LucidePencil`/`LucideTrash2`).
2. **Autocomplete-Vorbefüllung**: Label/Artist im Formular sollen dieselbe
   `app-autocomplete`-Komponente wie im Filter nutzen (6f-Entscheidung:
   Listen können sehr groß sein, kein natives `<select>`). Die Komponente
   hat aber keine Möglichkeit, im Bearbeiten-Modus den aktuellen Namen
   vorzubefüllen. Entschieden: `shared/autocomplete/` um ein optionales
   Prefill-Input erweitern.

Referenz-Muster für die gesamte Umsetzung ist der Label-Slice
(`features/labels/`), der CRUD-Formulare, Confirm-Modal und
Fehlerbehandlung bereits nach dem im Projekt etablierten Muster umsetzt.

## Umsetzung

### 1. `shared/autocomplete/autocomplete.ts` (geändert)

Neues optionales Input `initialQuery = input<string>('')`. `queryText` von
`signal('')` auf `linkedSignal(() => this.initialQuery())` umgestellt —
das ist dasselbe Vorbefüll-Muster wie bei `LabelForm.formModel`
(`linkedSignal(() => this.buildInitialModel())`). Verhalten bleibt für
bestehende Aufrufer (Filter in 6f, kein `initialQuery` gesetzt) unverändert,
da der Default `''` ist. Falls noch kein `autocomplete.spec.ts` existiert,
neu anlegen; sonst Testfälle ergänzen: zeigt `initialQuery` beim Rendern an,
behält eigene Eingabe bei, solange `initialQuery` unverändert bleibt.

### 2. `features/records/record.ts` (geändert)

`CreateRecordRequest`/`UpdateRecordRequest`-Interfaces ergänzen, analog
`CreateLabelRequest`/`UpdateLabelRequest` in `features/labels/label.ts`:

```ts
export interface CreateRecordRequest {
  labelId: number;
  artistId: number | null;
  format: RecordFormat;
  albumName: string;
  releaseYear: number;
  condition: RecordCondition;
  information: string | null;
}
export interface UpdateRecordRequest extends CreateRecordRequest {}
```

### 3. `features/records/record.service.ts` (geändert)

`create()`, `update()`, `delete()` ergänzen, 1:1 nach dem Muster aus
`features/labels/label.service.ts`:

```ts
create(request: CreateRecordRequest): Observable<Record> {
  return this.http.post<Record>(this.baseUrl, request);
}
update(id: number, request: UpdateRecordRequest): Observable<Record> {
  return this.http.put<Record>(`${this.baseUrl}/${id}`, request);
}
delete(id: number): Observable<void> {
  return this.http.delete<void>(`${this.baseUrl}/${id}`);
}
```

Tests in `record.service.spec.ts` ergänzen (`HttpTestingController`, wie
`label.service.spec.ts`).

### 4. `features/records/record-form/` (neu)

`record-form.ts`, `record-form.html`, `record-form.spec.ts` — Signal-Form-
Komponente analog `features/labels/label-form/label-form.ts`, aber mit
eigener Autocomplete-Anbindung für Label/Artist (eigenständiges
`rxResource`-Paar innerhalb der Komponente, gespeist durch direkt
injizierte `LabelService`/`ArtistService` — dieselbe Debounce/Suchlogik wie
in `Records`/`RecordFilter`, aber im Formular gekapselt statt über den
Elternknoten geteilt, damit Formular-Suche und Filter-Suche sich nicht
gegenseitig beeinflussen).

Felder (Reihenfolge nach `user-stories-record.md` US-R4):

- **Label** (Pflicht) — `app-autocomplete`, `initialQuery` vorbefüllt mit
  `record()?.labelName`, `selected`-Output setzt `labelId`.
- **Artist** (optional) — `app-autocomplete`, `initialQuery` vorbefüllt mit
  `record()?.artistName ?? ''`.
- **Format** (Pflicht) — natives `<select>` mit `RECORD_FORMAT_LABELS`
  (gleiches `Object.entries(...)`-Muster wie in `record-filter.ts`).
- **Albumname** (Pflicht) — `required`, `minLength(1)`, `maxLength(150)`,
  `pattern(/^[\p{L}\p{N} \-&'./()]+$/u)` — exakt das Backend-Pattern aus
  `Record.cs` (`AlbumNamePattern`), inkl. Klammern.
- **Erscheinungsjahr** (Pflicht) — Zahlenfeld, Bereich 1860 bis aktuelles
  Jahr (`new Date().getFullYear()`, nicht hartkodiert). Umsetzung über
  `min`/`max` aus `@angular/forms/signals` falls der numerisch typisierte
  Formularpfad mit dem `type="number"`-Input sauber zusammenspielt, sonst
  äquivalente `validate()`-Bereichsprüfung mit derselben Fehlermeldung wie
  serverseitig (`CreateRecordCommandValidator`) — während der Umsetzung
  gegen `ng test` verifizieren, welche Variante kompiliert.
- **Zustand** (`condition`) — natives `<select>` mit
  `RECORD_CONDITION_LABELS`, Default `Vg` im Anlegen-Modus.
- **Information** (optional) — Textfeld, `maxLength(255)`.

Serverseitige 400-Fehler feldweise zugeordnet (gleiches Muster wie
`LabelForm.handleSaveError`), Property-Namen aus dem Backend-DTO:
`LabelId`, `ArtistId`, `Format`, `AlbumName`, `ReleaseYear`, `Condition`,
`Information`.

Kein "Discogs-Suche"-Button — das verschachtelte Discogs-Modal aus
`ui-ux-konzept.md` gehört zu Block 8 (weiterhin offen) und ist bewusst
nicht Teil dieses Blocks.

### 5. `features/records/record-card/` (geändert)

`record-card.ts`: zwei neue Outputs `editRequested = output<Record>()`,
`deleteRequested = output<Record>()`.

`record-card.html`: Icon-Buttons (`btn btn-ghost btn-icon btn-sm`,
`LucidePencil`/`LucideTrash2`) in der Card platziert, mit
`(click)="$event.stopPropagation(); editRequested.emit(record())"` bzw.
`deleteRequested.emit(...)`, damit das bestehende `(click)="opened.emit()"`
auf dem Card-Root nicht mitausgelöst wird. `aria-label` je Button mit
Albumname, analog `label-table.html`.

Tests in `record-card.spec.ts` ergänzen: Klick auf Edit/Delete-Button löst
das jeweilige Output aus, ohne `opened` auszulösen.

### 6. `features/records/records.ts` + `records.html` (geändert)

Wiring 1:1 nach dem Muster aus `features/labels/labels.ts`/`labels.html`:

- `formOpen`/`editingRecord`-Signals, `openCreateForm()`/`openEditForm()`.
- `pendingDelete`-Signal + `pendingDeleteMessage` (Text mit Albumname).
- `onFormCancelled()`/`onFormSaved()` (schließt Modal, `recordsResource.reload()`).
- `onDeleteRequested()`/`onDeleteCancelled()`/`onDeleteConfirmed()`
  (`recordService.delete(...)`, Fehlerbehandlung über `ErrorModalService`
  wie in `labels.ts`).
- Toolbar: „+ Neuer Record"-Button (`btn btn-primary`, `LucidePlus`) neben
  der bestehenden Filter-Zeile.
- `app-record-card` erhält `(editRequested)`/`(deleteRequested)`-Bindings.
- `@if (formOpen()) { <app-record-form ... /> }` und
  `@if (pendingDelete()) { <app-confirm-modal ... /> }` unterhalb der
  bestehenden `<section>`, wie in `labels.html`.

`records.spec.ts` um Testfälle für Anlegen/Bearbeiten/Löschen-Flow ergänzen,
analog `labels.spec.ts`.

## Erweiterung: Label/Artist direkt aus dem Record-Formular anlegen

Freigegeben am 2026-08-14 (nach dem Live-Test des obigen Umfangs).

Beim Live-Test wurde sichtbar: Wählt der Benutzer im Record-Formular ein
Label oder einen Artist, das/der noch nicht existiert, bleibt das Feld
schlicht ungültig — er müsste die Ansicht wechseln, dort anlegen und
zurückkehren. Weder Wiki noch bisheriger Code sehen dafür etwas vor.

Mit dem Projektinhaber geklärt, unterschiedlich je Entität, weil `Artist`
nur ein Pflichtfeld hat (`CreateArtistCommand.Name`), `Label` aber zwingend
auch `countryId` braucht:

- **Artist**: Verlässt der Benutzer das Künstler-Feld (Blur) mit einem Text,
  der zu keinem Vorschlag passt, aber für sich genommen ein gültiger
  Artist-Name ist (`Artist.cs`: 3–120 Zeichen, Pattern
  `^[\p{L}\p{N} \-&'./]+$`), erscheint eine Rückfrage „Soll der Künstler
  '<Name>' neu angelegt werden?". Ja → Artist wird mit nur dem Namen
  angelegt und automatisch übernommen. Nein → Feld wird geleert.
- **Label**: Kleiner Icon-Button (mit Tooltip) neben dem Label-Feld öffnet
  das bestehende `LabelForm` als zweites, verschachteltes Modal.

Voraussetzungen/Nebenwirkungen, die dafür mitgelöst wurden:

- `shared/modal/modal.ts`: `@HostListener('document:keydown.escape')`
  wirkte bisher global auf jede offene `Modal`-Instanz — bei zwei
  gleichzeitig offenen Modals (verschachteltes `LabelForm` bzw.
  `ConfirmModal` für die Artist-Rückfrage über dem `RecordForm`) hätte
  Escape beide gleichzeitig geschlossen. Fix: modulweiter Stack, nur die
  oberste Instanz reagiert auf Escape.
- `shared/autocomplete/autocomplete.ts`: neuer Output `blur = output<string>()`
  (emittiert den aktuellen Text) und neue öffentliche Methode
  `setQuery(value: string)` zum programmatischen Setzen des Anzeigetexts
  von außen (nötig zum Leeren nach Ablehnen bzw. Setzen nach Quick-Create —
  das bestehende `initialQuery`/`linkedSignal`-Muster reicht dafür nicht,
  da ein erneutes Setzen auf denselben vorherigen Wert von Angular nicht
  als Änderung erkannt wird).
- `features/labels/label-form/label-form.ts`: `saved` von `output<void>()`
  auf `output<Label>()` umgestellt, damit der Aufrufer weiß, welches Label
  entstanden ist. Bestehender Aufrufer `Labels` bleibt kompatibel.
- `shared/confirm-modal/confirm-modal.ts`: erst während der Umsetzung
  aufgefallen — der Bestätigen-Button war fest auf Text „Löschen" und
  `.btn-danger` verdrahtet, für die Artist-Anlegen-Rückfrage inhaltlich
  falsch (rot = destruktiv). Neue optionale Inputs `confirmLabel` (Default
  `'Löschen'`) und `confirmVariant` (`'danger' | 'primary'`, Default
  `'danger'`) ergänzt, bestehende Aufrufer unverändert kompatibel.

Umgesetzt in `features/records/record-form/`:

- Neuer `countriesResource` (via `CountryService`) für das verschachtelte
  `LabelForm`.
- `#labelAutocomplete`/`#artistAutocomplete` per `viewChild()`.
- Label-Button (`btn btn-ghost btn-icon btn-sm`, `LucidePlus`, `title` und
  `aria-label` „Neues Label anlegen") öffnet `labelCreateOpen`; verschachteltes
  `<app-label-form [countries]="countries()" ...>` ohne `[label]`-Input
  (Anlegen-Modus); `onLabelCreateSaved(label)` übernimmt Id und Anzeigetext.
- `onArtistBlur(text)` prüft Länge/Pattern und Abweichung vom zuletzt
  bestätigten Namen, setzt bei Bedarf `pendingNewArtistName`;
  `app-confirm-modal` fragt nach; Bestätigen ruft `ArtistService.create(...)`
  und übernimmt Id/Name, Ablehnen leert das Feld.

Tests ergänzt in `modal.spec.ts`, `autocomplete.spec.ts`, `confirm-modal.spec.ts`,
`label-form.spec.ts` und `record-form.spec.ts` — 271 Frontend-Tests
insgesamt, alle grün; Production-Build und Prettier-Check grün. Live gegen
den laufenden Aspire-AppHost verifiziert (Tooltip, verschachteltes
Label-Formular inkl. Übernahme, Escape schließt nur das oberste Modal,
Artist-Rückfrage mit „Anlegen"-Button, keine Konsolenfehler).

Nicht Teil dieser Erweiterung: Discogs-Integration/-Modal (Block 8, nutzt
künftig dasselbe Verschachtelungsmuster, aber eigenständig umgesetzt); keine
Änderung an `ArtistForm` (Artist-Quick-Create läuft ausschließlich über
`ConfirmModal` + direkten `ArtistService.create()`-Aufruf, ohne das
bestehende Formular).

## Nicht Teil dieses Blocks

- Detailseite `/records/:id` (US-R7, Block 6h).
- Album-Cover-Upload (US-R8, Block 6i).
- Tracks (US-T1–T3, Block 6j).
- Discogs-Integration/-Modal (Block 8).
- Backend-Änderungen (bereits vollständig vorhanden).

## Verifikation

- `npm test` (Vitest) — alle Frontend-Tests grün, inkl. neuer Fälle.
- `npm run build` (Production-Build) grün.
- Prettier-Check (`printWidth: 100`, wie in den Vorgänger-Blöcken verwendet).
- Keine Backend-Datei betroffen — zur Kontrolle `git status`/`git diff --stat`
  vor Abschluss prüfen, dass nur Frontend-Dateien geändert wurden.
- Live-Verifikation im Browser gegen den laufenden Aspire-AppHost: Record
  anlegen (inkl. Validierungsfehler inline), Record bearbeiten (Label/Artist-
  Autocomplete zeigt den bisherigen Namen vorbefüllt), Record löschen mit
  Bestätigungsdialog, neuer Record erscheint korrekt in der Liste.

## Dokumentation nach Abschluss

TASK.md (Abschnitt 6g ergänzen, Status), README.md falls betroffen. Kein
neuer ADR erwartet (Autocomplete-Erweiterung ist Fortführung der 6f-
Entscheidung, kein neuer Trade-off) — endgültig beim Schreiben entscheiden.
