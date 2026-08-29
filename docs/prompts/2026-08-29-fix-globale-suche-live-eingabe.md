# Fix: Globale Suche reagiert live auf Eingabe

## Kontext

Notwendige-Korrekturen-Liste des Projektinhabers (`2026-08-28-notwendige-korrekturen.md`), Abschnitt „Globale Suche":

- Suchergebnisse werden direkt bei Eingabe geladen, nicht erst mit Drücken von „Enter".
- Wenn das Suchfeld leer ist, werden keine Suchergebnisse angezeigt.

Aktuell öffnet das Kopfzeilen-Suchfeld (`NavComponent`) die Ergebnisseite `/search?q=...` nur, wenn der Benutzer das Formular absendet (Enter-Taste). Das widerspricht der gewünschten Live-Suche. Reiner Frontend-Fix, kein Backend-Change (`GET /api/search` bleibt unverändert).

Zusätzlich mit behoben (Rückfrage an den Projektinhaber, ausdrücklich bestätigt): Das Kopfzeilen-Suchfeld zeigte beim Direktaufruf/Reload von `/search?q=...` bisher keinen Suchbegriff an (Feld blieb leer, obwohl Ergebnisse angezeigt wurden) — Deep-Link-Lücke, eng verwandt mit dem oben genannten Punkt.

## Ist-Stand (verifiziert)

- `src/frontend/src/app/nav/nav.ts`: `submitSearch()` validiert (`minLength(2)`, Zeichen-Pattern) und navigiert nur bei `(submit)` des `<form class="search">` in `nav.html` zu `/search?q=...`.
- `src/frontend/src/app/features/search/search.ts`: `searchResource` (rxResource) hängt bereits vollständig am Route-Query-Param `q` — bei fehlendem/leerem `q` liefert `search.html` (`@if (query(); as q) … @else …`) bereits „Bitte einen Suchbegriff eingeben." ohne Suchergebnisse. Hier ist keine Änderung nötig.
- Etabliertes Codebase-Muster für genau dieses Problem: `shared/autocomplete/autocomplete.ts` debounced bereits ein Signal live:
  `toObservable(this.queryText).pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed()).subscribe(...)`.
  Dieses Muster wird 1:1 für das Kopfzeilen-Suchfeld übernommen (gleicher Debounce-Wert 300 ms).
- Wiki `wiki/architektur/suche.md` (Abschnitt „Eingabevalidierung") und `wiki/architektur/navigation-konzept.md` (Abschnitt „Suchfeld") dokumentieren aktuell explizit das Enter-Verhalten — muss mit angepasst werden (verbindliche fachliche Quelle).
- Tests: `nav.spec.ts` hat 4 bestehende Tests rund um `submitSearch()` (Submit mit Treffer, leere Eingabe, zu kurz, verbotene Zeichen) — bleiben unverändert grün, da `submitSearch()` inhaltlich nicht verändert wird.

## Geplanter Fix

Nur `src/frontend/src/app/nav/nav.ts` (kein Template-Change nötig — `[formField]="searchForm.query"` aktualisiert `searchModel` bereits bei jedem Tastendruck):

1. Neue Subscription im Constructor, die `searchModel` aus dem aktuellen `q`-Query-Param der Route hält — löst die Deep-Link-Lücke:
   ```ts
   this.router.events
     .pipe(
       filter((event): event is NavigationEnd => event instanceof NavigationEnd),
       startWith(null),
       map(() => this.router.parseUrl(this.router.url).queryParamMap.get('q')),
       takeUntilDestroyed(),
     )
     .subscribe((query) => {
       if (query !== null && query !== this.searchModel().query) {
         this.searchModel.set({ query });
       }
     });
   ```
   `startWith(null)` sorgt dafür, dass beim Constructor-Aufbau einmal synchron mit der aktuell aktiven URL synchronisiert wird (deckt Direktaufruf/Reload von `/search?q=...` ab); jedes weitere `NavigationEnd` (Browser-Zurück/Vor, Link von außerhalb des Suchfelds) hält das Feld danach synchron. Der Vergleich `query !== this.searchModel().query` verhindert, dass eine soeben selbst getippte, bereits navigierte Eingabe durch das eigene `NavigationEnd` erneut überschrieben wird.
2. Neuer Debounce-Stream im Constructor (nach der Subscription aus Schritt 1, damit deren synchrone Erst-Synchronisierung als „Ausgangszustand" zählt), nach demselben Muster wie `autocomplete.ts`:
   ```ts
   toObservable(computed(() => this.searchModel().query))
     .pipe(skip(1), debounceTime(300), distinctUntilChanged(), takeUntilDestroyed())
     .subscribe((query) => this.handleLiveSearch(query));
   ```
   `skip(1)` verhindert, dass der beim Constructor-Aufbau gesetzte Ausgangswert (leer ODER aus der Route übernommen) selbst einen weiteren Navigate-Aufruf auslöst.
3. Neue private Methode `handleLiveSearch(rawQuery: string)`:
   - Leer (getrimmt) → nur wenn `currentUrl()` mit `/search` beginnt: `router.navigate(['/search'], { queryParams: { q: null }, replaceUrl: true })` (entfernt den Query-Param, `search.html` zeigt dadurch automatisch keine Ergebnisse mehr). Sonst keine Aktion (kein Wegnavigieren von einer anderen Seite nur weil das leere Feld „durchgetippt" wurde).
   - Nicht leer → `attemptedSubmit.set(true)` (löst ggf. Inline-Fehlermeldung aus, wie bisher bei Submit), dann bei gültiger Eingabe `router.navigate(['/search'], { queryParams: { q: query }, replaceUrl: true })`.
4. Gemeinsame Validierungslogik als kleine private Hilfsmethode `validQueryOrNull(rawQuery)` extrahiert und in `submitSearch()` wiederverwendet — `submitSearch()` selbst bleibt in Verhalten und Navigate-Aufruf-Signatur exakt unverändert (Enter funktioniert also weiterhin sofort, zusätzlich zur Live-Suche).
5. Neue Imports in `nav.ts`: `toObservable`, `takeUntilDestroyed` (aus `@angular/core/rxjs-interop`, bereits im Projekt über `autocomplete.ts` etabliert), `debounceTime`, `distinctUntilChanged`, `skip`, `startWith` (aus `rxjs`, `filter`/`map` sind schon vorhanden).

`replaceUrl: true` nur für die Live-Trigger (verhindert Browser-History-Spam bei jedem Tastendruck); der explizite Submit-Pfad (Enter) bleibt bei Standard-Push, wie bisher.

### Neue/angepasste Tests (`nav.spec.ts`)

Bestehende 4 Such-Tests bleiben unverändert (sie mocken `router.navigate`, wodurch keine echten `NavigationEnd`-Events entstehen — keine Überschneidung mit den neuen Subscriptions). Zusätzlich vier neue Tests:

1. Live-Suche ohne Submit: Eingabe „jazz" per `input`-Event (kein `submit`), `await wait(350)`, erwartet `navigateSpy` mit `['/search'], { queryParams: { q: 'jazz' }, replaceUrl: true }`.
2. Leeren auf der Such-Seite: vorab echtes `await router.navigateByUrl('/search?q=jazz')` vor dem Erzeugen der Fixture, danach `navigateSpy` erst nach `fixture.detectChanges()` installieren; Eingabe leeren, `await wait(350)`, erwartet `navigateSpy` mit `['/search'], { queryParams: { q: null }, replaceUrl: true }`.
3. Kein Wegnavigieren: Eingabe tippen und wieder leeren, während die Ausgangsroute nicht `/search` ist (Standard-Testroute), `await wait(350)`, erwartet `navigateSpy` nicht aufgerufen.
4. Deep-Link-Sync: `await router.navigateByUrl('/search?q=jazz')` vor dem Erzeugen der Fixture, dann `fixture.detectChanges(); await fixture.whenStable(); fixture.detectChanges();`, erwartet `input.value === 'jazz'`.

## Wiki-Aktualisierung

- `02 Wiki/MyMusic Wiki/wiki/architektur/suche.md`, Abschnitt „Eingabevalidierung": Bezug auf Submit/Enter wird um Live-Verhalten ergänzt; neuer Satz, dass Ergebnisse beim Leeren des Feldes auf der Suchseite verschwinden.
- `02 Wiki/MyMusic Wiki/wiki/architektur/navigation-konzept.md`, Abschnitt „Suchfeld": „Bei Eingabe und Bestätigung öffnet sich `/search?q=...`" → korrigiert zu „Bei Eingabe öffnet/aktualisiert sich `/search?q=...` live, ohne dass eine Bestätigung (Enter) nötig ist".
- Quelle für beide Änderungen: `2026-08-28-notwendige-korrekturen.md`.
- `wiki/log.md`: neuer Eintrag ganz oben.
- Kein neuer ADR (etabliertes Codebase-Muster wird wiederverwendet, keine neue Architekturentscheidung mit Trade-off).
- Kein Eintrag in `TASK.md` oder der Root-`CLAUDE.md` — Präzedenzfall (`fix-tracklist-modal-groesse`, PR #96) hat dort ebenfalls nichts ergänzt.

## Geplante Verifikation

1. `npm test -- --watch=false` im Frontend-Workspace (`src/frontend`) — alle Tests grün.
2. `npm run build` — Production-Build muss weiterhin erfolgreich sein.
3. Zeilenlängen-Check (≤120 Zeichen) der geänderten Zeilen.
4. Manuelle Live-Prüfung gegen laufenden Aspire-AppHost, falls verfügbar: Tippen ohne Enter zeigt Ergebnisse, Leeren des Feldes auf `/search` blendet Ergebnisse aus, Leeren auf anderer Seite navigiert nicht weg, Direktaufruf/Reload von `/search?q=jazz` zeigt „jazz" im Kopfzeilen-Suchfeld, Browser-Zurück auf einen anderen `/search?q=...`-Stand aktualisiert das Feld ebenfalls.

## Bekannte Risiken und offene Punkte

- Debounce-Wert 300 ms folgt bewusst dem bereits akzeptierten `autocomplete.ts`-Präzedenzfall statt eines neu diskutierten Wertes.
- Enter/Submit bleibt zusätzlich zur Live-Suche funktionsfähig (Auslegung von „nicht erst mit Enter" als „nicht mehr zwingend erforderlich", nicht als „Enter entfernen").
- Die Route→Feld-Synchronisierung deckt Navigation über den Angular-Router ab (Browser-Zurück/Vor, interne Links, Direktaufruf/Reload). Mehrere gleichzeitig offene Browser-Tabs mit unterschiedlichen `q`-Werten sind kein Sonderfall dieses Fixes, sondern bestehendes Angular-Verhalten (jeder Tab hält seinen eigenen `Router`-Zustand).
