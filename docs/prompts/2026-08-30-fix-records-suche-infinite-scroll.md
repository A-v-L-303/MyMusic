# Fix: Infinite Scroll für Records und Suche

## Kontext

`2026-08-28-notwendige-korrekturen.md` fordert unter „Records", dass die
Cards nicht mehr seitenweise paginiert werden, sondern beim Scrollen
automatisch nachladen („Details müssen noch besprochen werden"). Die dabei
zu berücksichtigende `2026-08-29-responsive-design-review.md` beschreibt in
Punkt 2 den zugrunde liegenden Missstand: Die geteilte `Pagination`-
Komponente rendert einen Button pro Seite ohne Umbruch oder Begrenzung.

Bei den im Rahmen dieser Planung korrigierten Wiki-NFR-Richtwerten
(`nicht-funktionale-anforderungen.md`, Abschnitt „Datenmenge": ~500–1.000
Records bzw. ~1.000 Artists pro Benutzer — die Tabelle nannte zuvor
fälschlich ~5.000 für beide, eine Verwechslung mit der erwarteten
Gesamt-Nutzerzahl der Anwendung, am 2026-08-30 mit dem Projektinhaber
korrigiert) ergeben sich bei Seitengröße 20 rund 25–50 Seiten. Laut
Design-Review reicht die Button-Zeile bereits ab knapp 30 Seiten im
1080-px-Container und ab ca. 9 Seiten bei 375 px nicht mehr aus — ein
Verstoß gegen „vollständig nutzbar" auf allen Breakpoints.

**Mit dem Projektinhaber geklärt (nicht mehr offen):**

1. **Scope**: Records-Seite **und** Suche (beide teilen Card-Grid und die
   `Pagination`-Komponente).
2. **Trigger**: Automatisches Nachladen per Scroll **plus** ein sichtbarer
   „Mehr laden"-Button als Fallback (Tastatur/Screenreader).
3. **Reload-Verhalten nach Anlegen/Bearbeiten/Löschen**: Liste wird auf
   Seite 1 zurückgesetzt und neu geladen (kein lokales Patchen).

**Kein Backend-Change** — `GET /api/records` und `GET /api/search`
unterstützen bereits `page`/`pageSize` und liefern `totalPages`/`totalCount`
unverändert.

## Ist-Stand (verifiziert)

- `features/records/records.ts`: `rxResource` mit `page`-Signal in den
  `params()`; `onPageChange(page)` setzt `page` direkt; Filteränderung setzt
  `page` auf 1 zurück; `onFormSaved`/`onDeleteConfirmed` rufen
  `recordsResource.reload()` (lädt aktuell nur die aktive Seite neu).
  Template `records.html` nutzt `<app-pagination [page] [totalPages]
  (pageChange)>` unterhalb des Card-Grids.
- `features/search/search.ts`/`search.html`: strukturell identisches Muster,
  bezogen auf `searchResource`/`results()`.
- `shared/pagination/pagination.ts`/`.html`: `pages()` erzeugt
  `Array.from({length: totalPages()})` — ein Button pro Seite, kein
  `flex-wrap`, keine Fenster-/Ellipsis-Logik. Bleibt für Genre-/Label-/
  Artist-/Admin-Tabellen unverändert im Einsatz, ist nicht Teil dieses Fixes.
- `RecordListResponse` (`features/records/record.ts`): `{ items, totalCount,
  page, pageSize, totalPages }` — `page`/`totalPages` sind serverseitig
  gesetzt und damit synchron mit den zugehörigen `items`.
- `shared/autocomplete/autocomplete.ts:26` nutzt bereits `linkedSignal(() =>
  this.initialQuery())` als Präzedenzfall für dieses Signal-Muster im
  Projekt.
- `records.spec.ts`/`pagination.spec.ts`: Tests nutzen Vitest + Angular
  TestBed + `HttpTestingController`, Selektor `button.btn-sm:not(.btn-icon)`
  für Seiten-Buttons. Kein globales Test-Setup für Browser-APIs — `jsdom`
  kennt `IntersectionObserver` nicht nativ.
- `styles/design-system/components.css`: `.btn-secondary`, `.btn-sm`,
  `.spinner` (22×22px, kein Größen-Modifier) vorhanden und wiederverwendbar,
  kein bestehendes Muster für einen Spinner innerhalb eines Buttons.

## Geplanter Fix

**Neuer Shared-Baustein `shared/load-more/`** (`load-more.ts`/`.html`/
`.spec.ts`, analog zu `shared/pagination/`):

- Inputs `hasMore: boolean`, `loading: boolean`, Output `loadMore`.
- Sentinel-`<div aria-hidden="true">` plus sichtbarer „Mehr laden"-Button
  (`.btn-secondary`), Inline-`.spinner` während `loading()`.
- `IntersectionObserver` über `effect((onCleanup) => { ... })` +
  `viewChild<ElementRef>('sentinel')`, `onCleanup` disconnected den
  Observer — läuft automatisch neu, wenn das Sentinel-Element durch
  `@if (hasMore())` aus dem DOM verschwindet/wiederkehrt.
- Rendert nichts, wenn `!hasMore()`.
- Klick-Handler und Intersection-Callback laufen über dieselbe
  `triggerLoadMore()`-Methode, die bei `loading()` bzw. `!hasMore()` früh
  zurückkehrt.

**`records.ts`/`search.ts`** (identisches Muster in beiden Dateien):

- Akkumulation über `linkedSignal`, `source` ausschließlich aus
  `recordsResource.value()` (nicht aus `page()` — sonst falsche Akkumulation
  in der Ladelücke zwischen Seitenwechsel und Serverantwort):

  ```ts
  protected readonly accumulatedItems = linkedSignal<RecordListResponse | undefined, Record[]>({
    source: () => this.recordsResource.value(),
    computation: (response, previous) => {
      if (!response) return previous?.value ?? [];
      return response.page > 1 ? [...(previous?.value ?? []), ...response.items] : response.items;
    },
  });
  ```

- Nächste Seite idempotent aus der letzten Serverantwort ableiten (nicht
  `page.update(p => p + 1)`), macht Doppel-Trigger folgenlos und heilt einen
  fehlgeschlagenen Nachlade-Request selbst:

  ```ts
  protected readonly nextPage = computed(() => (this.recordsResource.value()?.page ?? 0) + 1);

  protected onLoadMore(): void {
    if (this.recordsResource.isLoading()) return;
    this.page.set(this.nextPage());
  }
  ```

- Abgeleitete Signale:

  ```ts
  protected readonly isInitialLoading = computed(() => this.recordsResource.isLoading() && this.page() === 1);
  protected readonly isLoadingMore = computed(() => this.recordsResource.isLoading() && this.page() > 1);
  protected readonly hasMore = computed(() => {
    const value = this.recordsResource.value();
    return value ? value.page < value.totalPages : false;
  });
  ```

- Reset nach Create/Update/Delete in `onFormSaved`/`onDeleteConfirmed`:

  ```ts
  private resetToFirstPage(): void {
    if (this.page() === 1) this.recordsResource.reload();
    else this.page.set(1);
  }
  ```

- `records()`/`results()` liefern künftig `accumulatedItems()` statt
  `recordsResource.value().items`.
- `Pagination`-Import/Verwendung durch `LoadMore` ersetzt; Spinner-Bedingung
  im Template von `recordsResource.isLoading()` auf `isInitialLoading()`
  umgestellt.

**`docs/adr/0027-infinite-scroll-records-suche.md`**: Entscheidung
„seitenweise Akkumulation via `linkedSignal`" gegenüber den verworfenen
Alternativen „wachsende `pageSize`" (überträgt bereits geladene Items erneut,
O(n²)-Traffic) und „Cursor-basierte Backend-Umstellung" (unnötig großer
Eingriff für den aktuellen Bedarf, Backend-API bleibt unverändert).

**Testanpassungen** in `records.spec.ts`/`search.spec.ts`:

- Seitenwechsel-Test (Selektor `button.btn-sm:not(.btn-icon)`, entfällt mit
  `app-pagination`) ersetzen durch: Klick auf „Mehr laden" fordert `page=2`
  an, nach Flush sind beide Seiten im DOM sichtbar (Beleg für Append statt
  Replace).
- Neuer Test: Reset-Verhalten nach CRUD von Seite 2 aus — Reload-Request mit
  `page=1`, Liste wird ersetzt statt weiter angehängt.
- Neuer Test: während Seite 2 lädt bleiben Seite-1-Cards sichtbar, kein
  Vollbild-Spinner.
- Neuer Test: `hasMore=false` bei `totalPages===1` → kein `app-load-more`
  im DOM.
- Neuer Test: fehlgeschlagener Nachlade-Request → `ErrorModalService`,
  erneuter Klick fordert wieder `page=2` an (nicht `page=3`).
- Neuer `load-more.spec.ts` mit `IntersectionObserver`-Stub
  (`vi.stubGlobal('IntersectionObserver', FakeImpl)`), da `jsdom` die API
  nicht nativ kennt.

**Unverändert (bewusst out of scope):** `record.service.ts`,
`search.service.ts`, `shared/pagination/*` (weiterhin für Genre-/Label-/
Artist-/Admin-Tabellen), `record-card.ts`/`.html`, Backend vollständig.

## Geplante Verifikation

1. `ng lint`, `ng test --watch=false`, `ng build` im Frontend-Workspace.
2. Zeilenlängen-Check (≤120 Zeichen) der geänderten Zeilen.
3. Backend nicht betroffen — kein `dotnet test` nötig.
4. Manuelle Live-Prüfung gegen den laufenden Aspire-AppHost: Scroll-Trigger,
   Button-Trigger, Ende der Liste (`hasMore=false`), Reset nach Anlegen/
   Bearbeiten/Löschen auch von Seite 2 aus, Fehlerfall beim Nachladen inkl.
   Retry, sowie explizit bei 375 px und 768 px Breite (die eigentliche
   Responsive-Korrektur aus der Design-Review).

## Bekannte Risiken und offene Punkte

- Wiki `ui-ux-konzept.md` benötigt einen neuen Abschnitt zum
  Nachladeverhalten (Infinite Scroll) bei Records/Suche — wird nach
  erfolgreicher Umsetzung ergänzt, inklusive `wiki/log.md`-Eintrag.
- TASK.md und die CLAUDE.md-Datei im Repo-Wurzelverzeichnis erhalten nach
  Fertigstellung einen neuen „Stand"-Absatz.
- Nach Merge werden in `2026-08-28-notwendige-korrekturen.md` und
  `2026-08-29-responsive-design-review.md` (Punkt 2, nur für Records/Suche)
  die entsprechenden Stellen mit PR-Verweis als erledigt markiert; für die
  vier Tabellen-Views (Genre/Label/Artist/Admin) bleibt Punkt 2 der
  Design-Review ausdrücklich offen.
- Ob `rootMargin: '200px'` am `IntersectionObserver` ein sinnvoller
  Vorlade-Abstand ist, wird nur durch die manuelle Live-Prüfung bestätigt,
  nicht durch automatisierte Tests.
