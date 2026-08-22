# Block 8b — Discogs-Frontend-Integration

## Kontext

Block 8a (Backend-Proxy: `GET /api/discogs/search`, `GET /api/discogs/releases/{id}`)
ist umgesetzt, getestet und gemergt (PR #82). Block 8b schließt die fachliche
Lücke aus TASK.md Abschnitt 8: Die Discogs-Metadaten sind zwar abrufbar, aber
noch nicht mit dem RecordForm verbunden. User Stories mit Akzeptanzkriterien
liegen vor (`wiki/user-stories/user-stories-discogs.md`, US-DI1–DI4), lassen
den genauen Ablauf des Track-Nachimports aber ausdrücklich „für Block 8b"
offen.

Mit dem Projektinhaber geklärt (Planungsgespräch 2026-08-22):

1. **Kein Auswahl-Assistent.** Nach Auswahl eines Discogs-Suchtreffers
   (zweistufiger Abruf: Suche → `GET /discogs/releases/{id}`) werden
   automatisch übernommen: Albumname, Erscheinungsjahr, Label, Record-Artist,
   Cover (heruntergeladen und über den bestehenden Upload-Endpunkt
   gespeichert) sowie **alle** Tracks aus der Discogs-Tracklist (Trackname,
   Position, Track-Artist, Track-Genre). Einzig das Format bleibt manuell
   (bereits im Wiki als „Format-Mapping" begründet, `discogs-api.md`).
2. **Track-Artist folgt der Discogs-Realität, nicht pauschal dem
   Record-Artist.** Bei einem Album mit durchgehendem Künstler liefert
   Discogs kein separates Pro-Track-Artist-Feld — Record-Artist =
   Track-Artist ist dort korrekt, weil identisch. Bei einer Compilation
   (Various Artists) hat jeder Track einen eigenen, vom Record-Artist
   abweichenden Artist — liefert Discogs diese Pro-Track-Artists, werden
   genau diese pro Track übernommen. Liefert Discogs sie nicht, ist das eine
   Grenze der Discogs-Datenqualität, keine eigene Fallback-Logik nötig.
   **Konsequenz**: Block 8a wird um ein optionales Pro-Track-Artist-Feld
   erweitert (siehe Abschnitt 1).
3. Genre liefert Discogs nur auf Release-Ebene — jeder Track bekommt dasselbe
   aufgelöste Release-Genre, keine VA-Sonderbehandlung nötig.
4. Rückfrage bei neuer Referenz (US-DI3) bleibt bestehen, aber **einmal je
   distinktem Namen** — bei einem Album also je einmal für Label/Artist/
   Genre, bei einer Compilation potenziell mehrfach für Artist (einmal je
   neuem Track-Artist-Namen).
5. Cover wird nach Auswahl automatisch heruntergeladen (`fetch()` der
   Discogs-Bild-URL) und über den bestehenden Cover-Upload-Mechanismus
   gespeichert (kein Vorschau-only-Fallback) — die Speicherung selbst ändert
   sich nicht (`Record.AlbumCover` als `byte[]` in PostgreSQL, siehe
   `MyMusic.Domain/DomainModels/Sammlung/Record/Record.cs`).

## 1. Backend-Erweiterung — Pro-Track-Artist (`MyMusic.Application`, `MyMusic.Infrastructure`)

Einzige Backend-Änderung in diesem Block, eng begrenzt auf ein zusätzliches
Feld an der bestehenden Discogs-Track-Kette:

- `Application/Common/Services/DiscogsTrack.cs`: neues optionales Feld.
  ```csharp
  public sealed record DiscogsTrack(string Position, string Title, string? Duration, string? Artist);
  ```
- `Application/Features/Integration/Discogs/ResponseDtos/DiscogsTrackResponse.cs`:
  analog `string? Artist` ergänzen.
- `Application/Features/Integration/Discogs/ResponseDtos/Builder/DiscogsResponseBuilder.cs`:
  `BuildTrack` reicht `track.Artist` durch.
- `Infrastructure/ExternalServices/Discogs/DiscogsTrackRepresentation.cs`:
  neues Feld für die Discogs-Rohantwort. Discogs liefert bei
  Various-Artists-Compilations pro Tracklist-Eintrag ein `artists`-Array in
  derselben Form wie auf Release-Ebene:
  ```csharp
  public sealed record DiscogsTrackRepresentation(
      string? Position,
      string? Title,
      string? Duration,
      List<DiscogsArtistRepresentation>? Artists);
  ```
  **Diese JSON-Struktur ist aus der öffentlichen Discogs-API-Dokumentation
  abgeleitet, nicht in dieser Planungssitzung live verifiziert** — analog zur
  bereits dokumentierten Unsicherheit in ADR 0018. Bei der Live-Verifikation
  (Abschnitt „Verifikation" unten) gegen einen echten VA-Sampler prüfen und
  bei Abweichung korrigieren, nicht vorab erraten.
- `Infrastructure/ExternalServices/Discogs/DiscogsClient.cs`, in der
  Tracklist-Mapping-Stelle von `MapRelease`: analog zum bestehenden
  Label-Mapping in `MapSearchResult` (mehrere Namen mit `", "` verketten,
  leeres/fehlendes Array → `null`):
  ```csharp
  var tracklist = (release.Tracklist ?? [])
      .Select(track => new DiscogsTrack(
          track.Position ?? string.Empty,
          track.Title ?? string.Empty,
          track.Duration,
          MapTrackArtist(track.Artists)))
      .ToList();
  ```
  mit einer neuen privaten Hilfsmethode `MapTrackArtist`, die `null`
  zurückgibt, wenn `Artists` leer/fehlend ist, sonst die verketteten Namen
  (gleiches Filtermuster wie bei `artists`/`labels`: `Where(name =>
  !string.IsNullOrWhiteSpace(name))`).

Keine Änderung an Routing, Endpunkten oder Fehlerbehandlung — nur eine
zusätzliche, abwärtskompatible Eigenschaft an einer bestehenden Response.

### Tests (Application.Tests)

- `DiscogsResponseBuilderTests`: neuer Fall für `BuildTrack` mit gesetztem
  und mit `null`-Artist.
- `GetDiscogsReleaseQueryHandlerTests`: bestehenden Happy-Path-Test um ein
  Tracklist-Element mit Pro-Track-Artist erweitern (gemockter
  `IDiscogsClient`, kein echter Discogs-Call).

### ADR

Neue Datei `docs/adr/0019-discogs-track-artist-zuordnung.md` (Muster ADR
0018: Kontext, Entscheidung, verworfene Alternative, Begründung,
Konsequenzen). Kernpunkte:

- Entscheidung: Pro-Track-Artist wird aus Discogs übernommen, sofern
  vorhanden; sonst Fallback auf den Record-Artist im Frontend (nicht im
  Backend — das Backend liefert nur `null` durch).
- Verworfene Alternative: „Immer Record-Artist für jeden Track, keine
  Backend-Erweiterung" — verworfen, weil das bei Compilations fachlich
  falsche Track-Artists erzeugen würde (mit dem Projektinhaber ausdrücklich
  klargestellt).
- Bekannte Grenze: Die genaue Discogs-JSON-Struktur für
  Pro-Track-Artists ist ungetestet übernommen, wird erst bei der
  Live-Verifikation bestätigt.

## 2. Frontend — neue Dateien (`src/frontend/src/app/features/records/`)

- `discogs.ts` — Typen, camelCase, spiegeln die Backend-Response-DTOs:
  ```typescript
  export interface DiscogsSearchResult {
    id: number;
    title: string;
    year: number | null;
    label: string | null;
    thumbnailUrl: string | null;
  }

  export interface DiscogsFormat {
    name: string;
    descriptions: string[];
  }

  export interface DiscogsTrack {
    position: string;
    title: string;
    duration: string | null;
    artist: string | null;
  }

  export interface DiscogsRelease {
    id: number;
    title: string;
    year: number | null;
    artists: string[];
    labels: string[];
    genres: string[];
    styles: string[];
    formats: DiscogsFormat[];
    coverImageUrl: string | null;
    tracklist: DiscogsTrack[];
  }
  ```
- `discogs.service.ts` (+ `.spec.ts`): gleiches Muster wie `genre.service.ts`.
  ```typescript
  search(q: string): Observable<DiscogsSearchResult[]> {
    return this.http.get<DiscogsSearchResult[]>(`${this.baseUrl}/search`, {
      params: new HttpParams().set('q', q),
    });
  }

  getRelease(id: number): Observable<DiscogsRelease> {
    return this.http.get<DiscogsRelease>(`${this.baseUrl}/releases/${id}`);
  }
  ```
  `baseUrl` = `${runtimeConfigService.apiBaseUrl}/api/discogs`.
- `discogs-search/discogs-search.ts` + `.html` (+ `.spec.ts`) — nested Modal
  (`Modal` importieren, analog `label-form.ts` als verschachteltes Modal im
  RecordForm):
  - Suchfeld (native `<input>`, kein `Autocomplete` — hier keine
    Vorschlagsliste, sondern ein eigenes Ergebnis-Panel), debounced über
    `rxResource`/Signal wie beim bestehenden `labelQuery`/`artistQuery`-Muster
    in `record-form.ts`; Suche erst ab 2 Zeichen auslösen (Backend lehnt
    kürzere ohnehin mit 400 ab, siehe US-DI1).
  - Ergebnisliste: Thumbnail (`thumbnailUrl`, Platzhalter-Icon wenn `null`,
    wie beim `LucideDisc3`-Fallback in `record-form.html`), Titel, Jahr,
    Label als Text.
  - Kein Treffer → Text „Keine Daten vorhanden" (`ui-ux-konzept.md`, „Empty
    States").
  - Klick auf einen Treffer ruft `discogsService.getRelease(id)` auf,
    zeigt eine Ladeanzeige während des zweiten Abrufs; bei Erfolg
    `applied.emit(release)`, bei Fehler (502) über
    `errorModalService.showFromHttpError(error, 'Discogs')`.
  - Outputs: `applied = output<DiscogsRelease>()`, `cancelled = output<void>()`.

## 3. Frontend — `ErrorModalService`-Erweiterung

`shared/error-modal/error-modal.service.ts`:

- `ErrorModalKind` um `'discogs'` erweitern.
- `mapToState`: neuer Fall **vor** dem generischen 500-Fallback:
  ```typescript
  if (error.status === 502) {
    return {
      kind: 'discogs',
      message: 'Discogs ist aktuell nicht erreichbar. Bitte die Daten manuell eingeben.',
    };
  }
  ```

`shared/error-modal/error-modal.ts`: `TITLES` um
`discogs: 'Discogs nicht verfügbar'` ergänzen. `error-modal.html` bleibt
unverändert (502 fällt in den bestehenden `@else`-OK-Zweig).

Das ist die in ADR 0013 („Konsequenzen") bereits vorgesehene Erweiterung um
einen sechsten `ErrorModalKind` — keine neue ADR nötig, nur Verweis in der
Umsetzung.

## 4. Frontend — `ArtistService`/`LabelService`: `getAll()`

Analog zu `GenreService.getAll()` (`genre.service.ts`, ruft bereits
`GET /api/genres/all`):

```typescript
getAll(): Observable<Artist[]> {
  return this.http.get<Artist[]>(`${this.baseUrl}/all`);
}
```

bzw. für `LabelService` mit `Label[]`. Beide Backend-Endpunkte
(`GET /api/artists/all`, `GET /api/labels/all`) existieren bereits seit
Block 6e und werden nur bisher nicht vom Frontend genutzt. Tests analog
`genre.service.spec.ts` (`ruft getAll ohne Query-Parameter gegen die
all-Route auf`).

## 5. Frontend — `LabelForm`: `initialName`

`features/labels/label-form/label-form.ts`:

```typescript
readonly initialName = input('');
```

`buildInitialModel()`:

```typescript
name: label?.name ?? this.initialName(),
```

Kein Template-Änderung nötig (`name`-Feld ist bereits an `formModel`
gebunden). Test in `label-form.spec.ts` ergänzen: `initialName` füllt das
Namensfeld nur, wenn `label` `null` ist.

## 6. Frontend — `RecordForm` (Kernstück, `features/records/record-form/`)

### 6.1 Neue Resources und Signals

- `artistsResource`/`labelsResource`/`genresResource` — je ein `rxResource`
  auf `getAll()` des jeweiligen Service (analog `countriesResource`),
  gecached für die Laufzeit des Formulars.
- `discogsSearchOpen = signal(false)`.
- `pendingArtistConfirmName = signal<string | null>(null)` (ersetzt/erweitert
  die bisherige `pendingNewArtistName` — siehe 6.3).
- `pendingGenreConfirmName = signal<string | null>(null)` (neu).
- `discogsTracklist = signal<DiscogsTrack[]>([])` — für den Import nach dem
  Speichern gestagt.
- `discogsRecordArtistName = signal<string | null>(null)` — der
  Release-Artist-Name, wird für das Fallback bei Tracks ohne eigenen
  `artist`-Wert gebraucht.

### 6.2 Discogs-Button und nested Modal

`record-form.html`: neuer Textbutton „Discogs-Suche" im `modal-head`- oder
oberhalb des Label-Felds (Platzierung nach bestehendem Spacing-Muster
entscheiden), öffnet `discogsSearchOpen`. Darunter:

```html
@if (discogsSearchOpen()) {
  <app-discogs-search
    (cancelled)="discogsSearchOpen.set(false)"
    (applied)="onDiscogsReleaseApplied($event)"
  />
}
```

### 6.3 Referenz-Auflösung vereinheitlichen (Artist)

Die bestehende `pendingNewArtistName`/`onCreateArtistConfirmed`/
`onCreateArtistCancelled`-Kette (blur-getriggerte Neuanlage) wird zu einer
wiederverwendbaren, Promise-basierten Methode umgebaut, weil dieselbe Logik
jetzt mehrfach gebraucht wird (Record-Artist, und bei Compilations je
Track-Artist):

```typescript
private pendingArtistResolve: ((id: number | null) => void) | null = null;

private resolveArtistId(name: string): Promise<number | null> {
  const existing = this.artists().find(
    (artist) => artist.name.toLowerCase() === name.toLowerCase(),
  );

  if (existing) {
    return Promise.resolve(existing.id);
  }

  return new Promise<number | null>((resolve) => {
    this.pendingArtistResolve = resolve;
    this.pendingArtistConfirmName.set(name);
  });
}

protected async onArtistCreateConfirmed(): Promise<void> {
  const name = this.pendingArtistConfirmName();
  this.pendingArtistConfirmName.set(null);

  if (!name) {
    return;
  }

  try {
    const artist = await firstValueFrom(this.artistService.create({ name }));
    this.pendingArtistResolve?.(artist.id);
  } catch (error) {
    if (!(error instanceof HttpErrorResponse)) {
      throw error;
    }
    this.errorModalService.showFromHttpError(error, 'Künstler');
    this.pendingArtistResolve?.(null);
  } finally {
    this.pendingArtistResolve = null;
  }
}

protected onArtistCreateCancelled(): void {
  this.pendingArtistConfirmName.set(null);
  this.pendingArtistResolve?.(null);
  this.pendingArtistResolve = null;
}
```

`artists()` = `computed(() => this.artistsResource.hasValue() ?
this.artistsResource.value() : [])`.

`onArtistBlur` wird auf `resolveArtistId` umgestellt (setzt bei Erfolg
`formModel.artistId`/`artistDisplayName`, bei `null` die Autocomplete-Query
zurück auf `''`) — dabei wird die manuelle Erfassung nebenbei korrekter:
Tippt der Benutzer den Namen eines bereits existierenden Artists ohne ihn
aus der Autocomplete-Liste auszuwählen, wird jetzt direkt referenziert statt
einen 409-Konflikt beim Speichern zu riskieren. Das Template-Binding für den
bestehenden Bestätigungsdialog bleibt strukturell gleich, nur die Handler
werden umbenannt (`onCreateArtistConfirmed`/`-Cancelled` →
`onArtistCreateConfirmed`/`-Cancelled`, im Template entsprechend anpassen).

Analog `resolveGenreId(name): Promise<number | null>` mit eigenem
`pendingGenreConfirmName`-Signal und **neuem** `<app-confirm-modal>`-Block
im Template (`title="Genre anlegen"`, Meldung „Soll das Genre „X" neu
angelegt werden?", `GenreService.create({ name })`).

Für Label bleibt die Auflösung ein Sonderfall (volles `LabelForm`-Modal statt
einfachem Confirm, da `countryId` Pflicht ist und von Discogs nicht
geliefert wird) — ebenfalls Promise-basiert:

```typescript
private pendingLabelResolve: ((id: number | null) => void) | null = null;

private resolveLabelId(name: string): Promise<number | null> {
  const existing = this.labels().find(
    (label) => label.name.toLowerCase() === name.toLowerCase(),
  );

  if (existing) {
    return Promise.resolve(existing.id);
  }

  return new Promise<number | null>((resolve) => {
    this.pendingLabelResolve = resolve;
    this.discogsLabelPrefillName.set(name);
    this.labelCreateOpen.set(true);
  });
}
```

`onLabelCreateSaved`/`onLabelCreateCancelled` rufen zusätzlich
`this.pendingLabelResolve?.(...)` auf (Id bzw. `null`) und setzen
`pendingLabelResolve = null`. `discogsLabelPrefillName` (neues Signal,
Default `''`) wird als `[initialName]` an `<app-label-form>` gebunden und
nach Gebrauch wieder zurückgesetzt.

### 6.4 `onDiscogsReleaseApplied`

```typescript
protected async onDiscogsReleaseApplied(release: DiscogsRelease): Promise<void> {
  this.discogsSearchOpen.set(false);

  this.formModel.update((model) => ({
    ...model,
    albumName: release.title,
    releaseYear: release.year ? String(release.year) : model.releaseYear,
  }));

  const labelName = release.labels[0];
  if (labelName) {
    const labelId = await this.resolveLabelId(labelName);
    if (labelId) {
      this.formModel.update((model) => ({ ...model, labelId: String(labelId) }));
      this.labelAutocomplete()?.setQuery(labelName);
    }
  }

  const artistName = release.artists[0];
  let recordArtistId: number | null = null;
  if (artistName) {
    recordArtistId = await this.resolveArtistId(artistName);
    if (recordArtistId) {
      this.formModel.update((model) => ({ ...model, artistId: String(recordArtistId) }));
      this.artistDisplayName.set(artistName);
      this.artistAutocomplete()?.setQuery(artistName);
    }
    this.discogsRecordArtistName.set(artistName);
  }

  const genreName = release.genres[0] ?? release.styles[0];
  const genreId = genreName ? await this.resolveGenreId(genreName) : null;

  await this.applyDiscogsCover(release.coverImageUrl);

  this.discogsTracklist.set(release.tracklist);
  this.discogsResolvedGenreId.set(genreId);
}
```

(`discogsResolvedGenreId = signal<number | null>(null)` — neues Signal.)

### 6.5 Cover-Übernahme

```typescript
private async applyDiscogsCover(coverImageUrl: string | null): Promise<void> {
  if (!coverImageUrl) {
    return;
  }

  try {
    const response = await fetch(coverImageUrl);
    if (!response.ok) {
      throw new Error(`Cover-Download fehlgeschlagen: HTTP ${response.status}`);
    }
    const blob = await response.blob();
    const file = new File([blob], 'discogs-cover', { type: blob.type });

    this.revokePreviewObjectUrl();
    this.selectedCoverFile.set(file);
    this.previewUrl.set(URL.createObjectURL(file));
  } catch (error) {
    if (isDevMode()) {
      console.error('Discogs-Cover konnte nicht automatisch übernommen werden.', error);
    }
  }
}
```

Wiederverwendet exakt den bestehenden `selectedCoverFile`/`previewUrl`-Pfad
aus `onCoverFileSelected` — keine neue Speicherlogik. Schlägt der Fetch fehl
(Netzwerk, CORS/Hotlink-Schutz von Discogs — bei der Live-Verifikation
gezielt prüfen), bleibt das Cover-Feld leer, der Rest der Übernahme läuft
unbeeinflusst weiter.

### 6.6 Track-Import nach dem Speichern

`save()`: nach dem bestehenden `await
this.uploadSelectedCoverIfAny(savedRecord.id);` neuer Aufruf:

```typescript
await this.importDiscogsTracksIfAny(savedRecord.id);
```

```typescript
private async importDiscogsTracksIfAny(recordId: number): Promise<void> {
  const tracklist = this.discogsTracklist();
  if (tracklist.length === 0) {
    return;
  }

  const genreId = this.discogsResolvedGenreId();
  const recordArtistName = this.discogsRecordArtistName();
  const artistIdByName = new Map<string, number>();

  if (recordArtistName) {
    const recordArtistId = Number(this.formModel().artistId) || null;
    if (recordArtistId) {
      artistIdByName.set(recordArtistName.toLowerCase(), recordArtistId);
    }
  }

  for (const [index, track] of tracklist.entries()) {
    const artistName = track.artist ?? recordArtistName;
    if (!genreId || !artistName) {
      continue;
    }

    const key = artistName.toLowerCase();
    let artistId = artistIdByName.get(key);

    if (artistId === undefined) {
      const resolved = await this.resolveArtistId(artistName);
      if (!resolved) {
        continue;
      }
      artistId = resolved;
      artistIdByName.set(key, artistId);
    }

    const { recordSide, trackNumber } = parseDiscogsPosition(track.position, index);

    try {
      await firstValueFrom(
        this.recordService.createTrack(recordId, {
          artistId,
          genreId,
          trackName: track.title,
          recordSide,
          trackNumber,
          information: track.duration ? `Dauer: ${track.duration}` : null,
        }),
      );
    } catch (error) {
      if (!(error instanceof HttpErrorResponse)) {
        throw error;
      }
      this.errorModalService.showFromHttpError(error, 'Track');
    }
  }
}
```

Fehlt die Genre-Zusage komplett, werden **alle** Tracks übersprungen (ohne
Genre kein einziger `createTrack`-Aufruf möglich); fehlt nur für einen
bestimmten Track-Artist-Namen die Zusage, wird **nur dieser Track**
übersprungen, die übrigen mit vollständig aufgelösten Referenzen werden
trotzdem angelegt. Ein Fehler bei einem einzelnen `createTrack`-Aufruf
(z. B. 409 bei Positions-Kollision im Edit-Modus) bricht den Import nicht
komplett ab, sondern wird gemeldet und der nächste Track versucht.

### 6.7 `parseDiscogsPosition`

Neue, reine Hilfsfunktion (Modul-Ebene in `record-form.ts` oder eigene
Datei `features/records/discogs-position.ts`, je nachdem was für den Test
praktischer ist):

```typescript
export function parseDiscogsPosition(
  position: string,
  fallbackIndex: number,
): { recordSide: string; trackNumber: number } {
  const cleaned = position.replace(/[^\p{L}\p{N}]/gu, '');
  const match = cleaned.match(/^(\p{L}*)(\d+)$/u);

  if (match) {
    return {
      recordSide: match[1].slice(0, 3).toUpperCase() || '0',
      trackNumber: Number(match[2]),
    };
  }

  return { recordSide: '0', trackNumber: fallbackIndex + 1 };
}
```

Reine Implementierungsentscheidung (kein Wiki-Vorgabe) — Default `'0'` und
1-basierter Fallback spiegeln das bestehende `TrackForm`-Verhalten.

## Tests (Frontend)

- `discogs.service.spec.ts`, `discogs-search.spec.ts` — neu, Muster
  `genre.service.spec.ts`/bestehende Modal-Komponenten-Tests.
- `error-modal.service.spec.ts` — neuer 502-Fall.
- `artist.service.spec.ts`, `label.service.spec.ts` — `getAll()`-Fall
  ergänzt.
- `label-form.spec.ts` — `initialName`-Fall ergänzt.
- `record-form.spec.ts` — umfangreichste Erweiterung:
  - Discogs-Button öffnet `discogs-search`, `applied`-Event füllt
    Albumname/Jahr.
  - Bestehende Label/Artist/Genre-Namen (exakter Treffer in `getAll()`)
    werden ohne Dialog referenziert.
  - Neue Namen lösen den jeweiligen Dialog aus; Bestätigen legt an und
    referenziert, Abbrechen lässt das Feld leer.
  - Cover-Fetch Erfolg (mocked `fetch`) setzt `selectedCoverFile`; Fehlschlag
    lässt es `null`.
  - Nach dem Speichern: Album-Release → alle Tracks mit dem
    Record-Artist; VA-Release (Tracks mit eigenem `artist`-Wert) → Tracks
    mit ihren jeweils individuell aufgelösten Track-Artists, inkl. Fall
    „neuer Track-Artist wird abgelehnt" → nur dieser Track fehlt.
  - Fehlende Genre-Zusage → kein `createTrack`-Aufruf.
  - `parseDiscogsPosition` — eigene, kleine Testtabelle (`A1`, `B2`, `1`,
    `2-3`, leerer String, nur Buchstaben ohne Ziffer).

## Dokumentation

- `TASK.md` Abschnitt 8: Status auf „Block 8a und 8b abgeschlossen" (nach
  erfolgreicher Live-Verifikation) setzen, Umsetzt-Liste ergänzen
  (Backend-Erweiterung + Frontend-Umfang), Kopfzeile „Stand:" aktualisieren.
- `docs/adr/0019-discogs-track-artist-zuordnung.md` neu (siehe Abschnitt 1).
- Wiki-Hinweis an den Projektinhaber (nicht selbstständig ändern): US-DI2
  nennt „Albumname" nicht explizit in der AC-Liste (nur im Zweck-Absatz von
  `discogs-api.md`) und die Pro-Track-Artist-Logik gar nicht — Vorschlag,
  das nach Abschluss zu präzisieren.

## Verifikation

1. Backend: `dotnet build --no-restore`, `dotnet test --no-build`
   (`MyMusic.Application.Tests`), `dotnet format --verify-no-changes`,
   Zeilenlängen-Check.
2. Frontend: `ng test --watch=false`, `npx prettier --check` (bekannte,
   von diesem Block unabhängige Formatierungsabweichungen laut
   TASK.md-Historie — neue/geänderte Dateien müssen selbst sauber sein).
3. **Manuelle Live-Verifikation** gegen die echte Discogs-API im laufenden
   Aspire-AppHost (Standard-Launch-Profil, nicht `--no-launch-profile`,
   siehe TASK.md Abschnitt 7c):
   - Ein normales Album suchen und übernehmen — Albumname, Jahr, Label,
     Artist, Cover, alle Tracks mit dem Record-Artist korrekt geprüft.
   - Einen bekannten Various-Artists-Sampler suchen und übernehmen — prüfen,
     ob Discogs tatsächlich Pro-Track-Artists liefert und ob das neue
     Mapping (`DiscogsTrackRepresentation.Artists`) sie korrekt einliest;
     bei Abweichung von der angenommenen JSON-Struktur den Mapping-Code
     korrigieren und als ADR-0019-Nachtrag dokumentieren.
   - Neue Label-/Artist-/Genre-Referenz auslösen (Datensatz, den es beim
     Benutzer noch nicht gibt) — Rückfrage-Dialoge prüfen, inkl.
     Mehrfach-Rückfrage bei mehreren neuen Track-Artists.
   - Cover-Download gezielt gegen mögliche CORS-/Hotlink-Sperren von
     Discogs prüfen (bekanntes Risiko aus der Planung).
   - Discogs-Ausfall simulieren (Token temporär ungültig setzen) → 502 und
     Modal-Darstellung prüfen.

## Risiken und offene Punkte

- Die Discogs-JSON-Struktur für Pro-Track-Artists ist nicht vorab
  verifiziert (siehe Abschnitt 1) — größtes technisches Risiko dieses
  Blocks.
- Cover-Download per `fetch()` kann an Discogs' Hotlink-Schutz scheitern;
  kein Backend-Proxy als Fallback geplant (bewusste Entscheidung, siehe
  Planungsgespräch) — bei Fehlschlag bleibt das Cover leer, keine weitere
  Fehlermeldung an den Benutzer (kein Blocker für den restlichen Import).
- Mehrfache sequenzielle Rückfrage-Dialoge bei Compilations mit vielen
  neuen Artists können bei sehr langen Tracklisten UX-mäßig lästig werden —
  kein Sammel-Dialog vorgesehen, da fachlich nicht gefordert; bei Bedarf
  Rückmeldung nach der Live-Verifikation einholen.
