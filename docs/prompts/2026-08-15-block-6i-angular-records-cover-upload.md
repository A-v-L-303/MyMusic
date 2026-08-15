# Block 6i: Album-Cover-Upload (US-R8)

Freigegeben am 2026-08-15. Branch: `block-6i-angular-records-cover-upload`.

## Kontext

`records/` hat mit Block 6f (Liste), 6g (Anlegen/Bearbeiten/Löschen) und 6h
(Detailansicht) die Grundfunktionen erhalten. Dieser Block deckt US-R8
(Album-Cover hochladen, `wiki/user-stories/user-stories-record.md`) ab.

Backend-seitig ist `POST /api/records/{id}/cover` bereits seit Block 6b
vollständig vorhanden (`RecordEndpoints.cs#UploadRecordCoverAsync`,
`UploadRecordCoverCommandHandler`, `UploadRecordCoverCommandValidator`).
**Keine Backend-Änderungen** — reiner Frontend-Block.

`Record.albumCoverDataUrl` wird bereits in `record-card.html` und
`record-detail.html` angezeigt (Platzhalter-Icon `lucideDisc3`, falls
`null`) — dieser Block ergänzt lediglich den Upload-Weg dorthin.

**Design-Klärung mit dem Projektinhaber während der Planung** (weicht von
der wörtlichen US-R8-Formulierung „unabhängig vom Anlegen/Bearbeiten des
Records" ab, daher hier festgehalten statt stillschweigend entschieden):

- Der Upload-Trigger sitzt im `RecordForm`-Modal — sowohl beim Anlegen als
  auch beim Bearbeiten eines Records. Nicht im Detail-Modal, nicht als
  drittes Icon auf der Record-Card.
- **Kein Löschen** des Covers in diesem Block. Das Backend hat weder einen
  `DELETE`-Endpunkt noch eine `RemoveAlbumCover()`-Domänenmethode
  (`Record.Create(...)` setzt Cover immer auf `null`, `Record.Update(...)`
  erhält das bestehende Cover unverändert, `SetAlbumCover(byte[])` lehnt
  leere Daten ab) — eine Löschfunktion wäre ein eigener, separat
  freizugebender Backend-Block und ist bewusst nicht Teil von 6i.
- Fehler bei ungültigem Upload (falsches Format, > 5 MB) erscheinen als
  **Modal**, nicht inline am Feld — explizite Ausnahme von der sonstigen
  400-Inline-Regel (Wiki-Klärung 2026-08-07 in `user-stories-record.md`).
- Record-Speichern (Create/Update) und Cover-Upload sind unabhängige
  HTTP-Calls: Ein fehlgeschlagener Cover-Upload darf den bereits
  erfolgreichen Record-Save nicht blockieren oder zurückrollen (analog zu
  US-R4: „Nach erfolgreichem Anlegen erscheint der neue Record in der
  Liste, zunächst ohne Cover").

**Backend-Vertrag** (nur konsumieren): `IFormFile file`-Parameter, Feldname
im `multipart/form-data`-Body muss `file` heißen. Erfolgsantwort ist der
vollständige aktualisierte `RecordResponse`. Bei Validierungsfehler HTTP 400
mit `ValidationProblemDetails { errors: { FileContent: [...] }, title,
status }` (`UploadRecordCoverCommandValidator`: NotEmpty, max. 5 MB
`RecordEntity.MaxAlbumCoverSizeBytes`, JPEG/PNG-Signaturprüfung via
`RecordEntity.DetectAlbumCoverContentType`).

`CreateRecordRequest`/`UpdateRecordRequest` bleiben unverändert (kein
Cover-Feld) — der Upload bleibt laut Backend-Designentscheid
(`wiki/architektur/api-endpunkte.md`, „Cover als separater Endpunkt") ein
zweiter, eigenständiger HTTP-Call nach dem Speichern.

## Umsetzung

### 1. `features/records/record.ts` (geändert)

Neue Konstanten neben den bestehenden `RECORD_*`-Konstanten:

```ts
export const MAX_ALBUM_COVER_SIZE_BYTES = 5 * 1024 * 1024;
export const ALLOWED_ALBUM_COVER_CONTENT_TYPES = ['image/jpeg', 'image/png'];
```

Spiegeln bewusst die Backend-Regel für schnelles Client-Feedback vor dem
Server-Roundtrip — Backend bleibt die verbindliche Prüfung.

### 2. `features/records/record.service.ts` (geändert)

```ts
uploadCover(id: number, file: File): Observable<Record> {
  const formData = new FormData();
  formData.append('file', file);
  return this.http.post<Record>(`${this.baseUrl}/${id}/cover`, formData);
}
```

Tests in `record.service.spec.ts` ergänzen: `POST` gegen
`/api/records/1/cover`, Body ist `FormData` mit Feld `file`, sowie ein
Fehlerfall (400), der den Fehler durchreicht.

### 3. `shared/error-modal/error-modal.service.ts` + `error-modal.ts` (geändert)

`ErrorModalKind` um `'validation'` erweitern, Titel „Ungültige Eingabe" in
`error-modal.ts#TITLES` ergänzen. `mapToState` um einen 400-Zweig: extrahiert
die erste Meldung aus `(error.error as ValidationProblemDetails)?.errors`
(ersten vorhandenen Objekt-Key nehmen, nicht hartcodiert auf `FileContent`),
Fallback-Text falls `errors` fehlt oder leer ist.

**Wichtiger Nebeneffekt, bewusst in Kauf genommen**: `handleSaveError` in
`record-form.ts`/`label-form.ts`/`artist-form.ts`/`genre-form.ts` fällt bei
einem 400-Fehlerschlüssel, der zu keinem bekannten Formularfeld passt,
bereits heute auf `errorModalService.showFromHttpError(error, entityName)`
zurück — bisher mit der generischen „Es ist ein unerwarteter Serverfehler
aufgetreten."-Meldung (fachlich unpassend für einen 400er, siehe
`wiki/fehler-und-ausnahmekonzept.md`: 400 = Validierungsfehler, 500 =
Serverfehler, unterschiedliche Kategorien). Nach dieser Änderung zeigt dieser
bereits bestehende Fallback-Pfad die echte Validierungsmeldung — eine
Verbesserung, kein neues Verhalten für einen bisher unberührten Fall.
`error-modal.spec.ts` um einen 400-Fall mit `errors`-Objekt (Titel „Ungültige
Eingabe", erste Meldung wird angezeigt) und einen Fallback-Fall ohne
`errors` ergänzen.

### 4. `features/records/record-form/record-form.ts` + `.html` (geändert)

Neue Signals:

```ts
protected readonly selectedCoverFile = signal<File | null>(null);
protected readonly previewUrl = signal<string | null>(null);
```

`previewUrl` wird im Bearbeiten-Modus initial aus
`record()?.albumCoverDataUrl` gesetzt; `onCoverFileSelected(event)` liest die
gewählte Datei, prüft Typ (`ALLOWED_ALBUM_COVER_CONTENT_TYPES`) und Größe
(`MAX_ALBUM_COVER_SIZE_BYTES`):

- Bei Verstoß: `errorModalService.showFromHttpError`-Äquivalent geht hier
  nicht (kein `HttpErrorResponse` vorhanden) — stattdessen direkt
  `errorModalService`-State mit `kind: 'validation'` setzen (kleine
  zusätzliche Methode auf `ErrorModalService`, z. B.
  `showValidationMessage(message: string)`, die intern denselben
  `state.set(...)`-Mechanismus nutzt wie `mapToState`), Datei-Input
  zurücksetzen, kein `selectedCoverFile`/`previewUrl` gesetzt, kein
  Server-Call.
- Bei gültiger Datei: vorherige Object-URL (falls vorhanden) per
  `URL.revokeObjectURL(...)` freigeben, neue `URL.createObjectURL(file)` in
  `previewUrl` setzen, Datei in `selectedCoverFile` merken.

Klasse implementiert `OnDestroy`, revoked eine ggf. noch aktive
Object-URL beim Zerstören der Komponente (Memory-Leak-Vermeidung).

HTML: neues Feld nach „Information", vor dem `modal-foot`:

```html
<div class="field">
  <span class="label">Album-Cover</span>
  <div class="flex items-center gap-3">
    <div class="record-cover w-16 flex-none rounded-md">
      @if (previewUrl(); as url) {
        <img [src]="url" alt="Cover-Vorschau" />
      } @else {
        <svg lucideDisc3 [size]="20" class="text-fg-subtle" aria-hidden="true"></svg>
      }
    </div>
    <input
      type="file"
      accept="image/jpeg,image/png"
      class="input"
      (change)="onCoverFileSelected($event)"
    />
  </div>
</div>
```

`save()` erweitert: nach erfolgreichem `create`/`update` (liefert den
Record inkl. `id`) zusätzlich, falls `selectedCoverFile()` gesetzt ist,
`recordService.uploadCover(id, file)` in einem **inneren** try/catch
aufrufen — bewusst getrennt von `handleSaveError`, damit ein Cover-Fehler
weder fälschlich auf ein Formularfeld gemappt wird noch `saved.emit()`
verhindert:

```ts
try {
  const saved = record
    ? await firstValueFrom(this.recordService.update(record.id, request))
    : await firstValueFrom(this.recordService.create(request));

  const file = this.selectedCoverFile();
  if (file) {
    try {
      await firstValueFrom(this.recordService.uploadCover(saved.id, file));
    } catch (coverError) {
      if (!(coverError instanceof HttpErrorResponse)) {
        throw coverError;
      }
      this.errorModalService.showFromHttpError(coverError, 'Album-Cover');
    }
  }

  this.saved.emit();
  return;
} catch (error) {
  return this.handleSaveError(error, field);
}
```

Tests in `record-form.spec.ts` ergänzen: Dateiauswahl setzt Vorschau; zu
große/falsche Datei zeigt Validation-Modal ohne Server-Call; Submit mit
Datei ruft `uploadCover` nach erfolgreichem `create`/`update` auf; Submit
ohne Datei ruft `uploadCover` nicht auf; Cover-Upload-Fehler zeigt Modal,
`saved` wird trotzdem emittiert; Bearbeiten-Modus zeigt bestehendes
`albumCoverDataUrl` als initiale Vorschau.

## Nicht Teil dieses Blocks

- Cover löschen (siehe Design-Klärung oben) — eigener, separat
  freizugebender Backend+Frontend-Block, falls künftig gewünscht.
- Track hinzufügen/bearbeiten/löschen (US-T1–T3, Block 6j).
- Backend-Änderungen (Upload-Endpunkt bereits vollständig vorhanden).
- Änderungen an `record-card.html`/`record-detail.html` — zeigen
  `albumCoverDataUrl` bereits an, `records.ts#onFormSaved()` lädt nach dem
  Speichern neu.

## Verifikation

- `npm test` (Vitest) — alle Frontend-Tests grün, inkl. neuer Fälle.
- `npm run build` (Production-Build) grün.
- Prettier-Check (`printWidth: 100`).
- `git status`/`git diff --stat` vor Abschluss prüfen, dass nur
  Frontend-Dateien geändert wurden.
- Live-Verifikation im Browser gegen den laufenden Aspire-AppHost: Cover
  beim Anlegen eines Records mitgeben → erscheint in Card und Detail;
  bestehenden Record bearbeiten und Cover ersetzen → neues Bild erscheint;
  zu große Datei bzw. falsches Format auswählen → Validation-Modal, kein
  Server-Roundtrip; Record ohne Cover-Auswahl speichern → unverändertes
  Verhalten (kein Upload-Call).

## Dokumentation nach Abschluss

`TASK.md` (neuer Unterabschnitt „6i. Album-Cover-Upload" nach dem Muster
von 6h, inkl. Design-Klärung zur Trigger-Platzierung und dem
`error-modal`-Nebeneffekt; Statuszeilen in Abschnitt 6 aktualisieren). Wiki
`user-stories-record.md`, US-R8: Nachtrag „Geklärt am 2026-08-15" zur
Trigger-Platzierung im `RecordForm` und zum Verzicht auf Löschen. Kein
neuer ADR erwartet (Umsetzung einer bereits im Backend entschiedenen
API-Form, kein neuer technischer Trade-off) — endgültig beim Schreiben
entscheiden.
