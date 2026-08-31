# ADR 0027 — Infinite Scroll für Records und Suche

**Status**: Angenommen
**Datum**: 2026-08-30
**Betrifft**: `src/frontend/src/app/features/records`,
`src/frontend/src/app/features/search`,
`src/frontend/src/app/shared/load-more`

## Kontext

Die Notwendige-Korrekturen-Liste des Projektinhabers fordert, dass die
Records-Cards nicht mehr seitenweise paginiert werden, sondern beim
Scrollen automatisch nachladen. Die zugehörige Responsive-Design-Review
beschreibt den zugrunde liegenden Missstand (Punkt 2): Die geteilte
`Pagination`-Komponente rendert einen Button pro Seite ohne Umbruch oder
Begrenzung. Bei den Wiki-NFR-Richtwerten von ~500–1.000 Records bzw. ~1.000
Artists pro Benutzer (`wiki/projekt/nicht-funktionale-anforderungen.md`,
Abschnitt „Datenmenge") ergeben sich bei Seitengröße 20 rund 25–50 Seiten —
die Button-Zeile reicht laut Design-Review bereits ab knapp 30 Seiten im
1080-px-Container und ab ca. 9 Seiten bei 375 px nicht mehr aus.

Mit dem Projektinhaber geklärt: Records-Seite und Suche (beide teilen
Card-Grid und `Pagination`) erhalten Infinite Scroll — automatisches
Nachladen per Scroll plus ein sichtbarer „Mehr laden"-Button als Fallback
(Tastatur/Screenreader). Nach Anlegen/Bearbeiten/Löschen wird die Liste auf
Seite 1 zurückgesetzt statt lokal gepatcht. Backend-API (`page`/`pageSize`/
`totalPages`/`totalCount`) bleibt unverändert.

## Entscheidung

Seitenweise inkrementieren und client-seitig akkumulieren, über
`linkedSignal` mit Zugriff auf den vorherigen berechneten Wert:

```ts
protected readonly records = linkedSignal<RecordListResponse | undefined, Record[]>({
  source: () => this.recordsResource.value(),
  computation: (response, previous) => {
    if (!response) return previous?.value ?? [];
    return response.page > 1 ? [...(previous?.value ?? []), ...response.items] : response.items;
  },
});
```

`source` hängt ausschließlich von `recordsResource.value()` ab, nicht von
einem lokalen `page()`-Signal — `value()` bleibt während eines laufenden
Reloads auf dem zuletzt aufgelösten Stand und ist damit garantiert synchron
mit den zugehörigen `items`. Die nächste angeforderte Seite wird ebenfalls
aus der letzten Serverantwort abgeleitet (`(value()?.page ?? 0) + 1`), nicht
über einen lokal hochgezählten Zähler — das macht Doppel-Trigger (Scroll und
Button im selben Tick) folgenlos und die Fehlerbehandlung selbstheilend: Bei
einem fehlgeschlagenen Nachlade-Request bleibt `value()` unverändert, ein
erneuter Versuch fordert exakt dieselbe (fehlgeschlagene) Seite wieder an
statt eine Seite zu überspringen — dafür ruft `onLoadMore()` bei
unverändertem Zielwert explizit `reload()` statt `page.set()` auf, da
Angular Signals bei einem `set()` mit unverändertem Wert keine
Benachrichtigung auslösen und der Request sonst ausbliebe.

Neuer gemeinsamer Baustein `shared/load-more/` (Sentinel-Element mit
`IntersectionObserver` plus Button, ein `loadMore`-Output) ersetzt
`shared/pagination/` ausschließlich in `records.ts`/`search.ts`.
`shared/pagination/` bleibt für Genre-/Label-/Artist-/Admin-Tabellen
unverändert im Einsatz.

## Verworfene Alternative 1 — wachsende `pageSize`

Statt der Seitenzahl bei jedem Nachladen die `pageSize` erhöhen (z. B.
`pageSize = page * 20`) und stets ab Seite 1 neu laden.

Verworfen, weil dabei bei jedem Nachladen bereits vollständig übertragene
Items erneut über das Netz gehen — der Gesamt-Traffic wächst quadratisch
mit der Anzahl geladener Seiten (O(n²)) statt linear. Bereits bei den
realistischen ~500–1.000 Records pro Benutzer wäre das spürbar, bei
größeren Sammlungen zunehmend gravierend.

## Verworfene Alternative 2 — Cursor-basierte Backend-Umstellung

Backend-API von Offset-Paginierung (`page`/`pageSize`) auf Cursor-basierte
Paginierung umstellen.

Verworfen, weil die bestehende Offset-Paginierung für den aktuellen Bedarf
ausreicht (keine hochfrequenten Schreibzugriffe während des Scrollens, die
zu Offset-Verschiebungen führen würden) und eine Backend-API-Änderung ein
unnötig großer Eingriff für ein rein clientseitig lösbares Problem wäre.

## Konsequenzen

- Erster Einsatz von `effect()` mit `onCleanup`-Callback im Frontend
  (`shared/load-more/load-more.ts`), zum Binden/Lösen des
  `IntersectionObserver` an das Sentinel-Element. Bisherige
  Lifecycle-Bindungen im Projekt (`shared/modal/modal.ts`) nutzen
  `OnInit`/`OnDestroy`, da dort keine mehrfach wechselnde
  DOM-Verfügbarkeit des gebundenen Elements vorliegt.
- `jsdom` kennt `IntersectionObserver` nicht nativ — `load-more.spec.ts`
  stubbt die globale Klasse selbst (`vi.stubGlobal`), es gibt kein
  projektweites Test-Setup für Browser-APIs.
- `records.ts`/`search.ts` verlieren die direkte `page`-basierte
  Navigation (`onPageChange`); ein Sprung zu einer beliebigen Seite ist mit
  Infinite Scroll nicht mehr möglich, nur noch sequenzielles Nachladen.
