# Block 2 (Angular) — Slice Genre, Frontend

## Kontext

Block 2 (Backend, `docs/prompts/2026-08-04-block-2-genre.md`) ist seit
2026-08-04 abgeschlossen: Genre-CRUD ist vollständig über `/api/genres`
erreichbar. Das Angular-Feature `genres/` wurde damals ausdrücklich
zurückgestellt, bis Block 0c (Angular-Workspace) existiert. Mittlerweile sind
zusätzlich Block 0f (Theme-Infrastruktur), 0g (NavComponent/Routing-Skelett)
und 7a (Login-Flow) abgeschlossen — `features/genres/` enthält aktuell nur
eine leere Platzhalter-Komponente (`export class Genres {}`), alle
Voraussetzungen für den echten Feature-Slice liegen aber vor.

Laut TASK.md und Wiki (`architektur/angular-projektstruktur.md`) ist Genre
der „einfachste vertikale Slice, Referenz für alle weiteren Entitäten" —
genau wie das Genre-Backend bereits Referenz für Country/Label/Artist/Record
war. Dieser Block ist damit der erste reale HTTP-Datenzugriff aus Angular
gegen die MyMusic-API und soll Muster etablieren, die Label/Artist/Record
1:1 übernehmen.

**Kein Backend-Change.** Genre-Backend wird unverändert konsumiert.

**Bezug zu `03 Ressourcen/offene-punkte-angular-feature-slices.md`**: Dieser
Block beantwortet im Rahmen der Umsetzung mehrere dort offen geführte Punkte:

- Punkt 1 (technische Verdrahtung der Fehlerdarstellung) — durch
  `ErrorModalService` + `submit()`-basiertes Inline-Mapping.
- Punkt 2 (Signal Forms als eigenes Konzept) — durch das erste
  mehrfeldrige* Formular mit echter Validierung (Block 0g deckte nur den
  einfachen, validierungsfreien Suchfeld-Fall ab). *Genre hat zwar nur ein
  Feld, aber erstmals mit `required`/`minLength`/`maxLength`/`pattern`.
- Punkt 4 (Angular-Testing-Konzept) — durch das erste
  `HttpTestingController`-Testmuster im Repo.
- Punkt 3 (Paginierung) — teilweise: Paginierungsmuster wird festgelegt;
  Sortierung bleibt offen, da Genre serverseitig fest nach Name sortiert
  (kein `sortBy`-Parameter) und das UI-Muster dafür erst mit einem Slice mit
  wählbarer Sortierung (Record) geklärt werden kann.
- Punkt 6 (API-Client-Codegenerierung) — wird durch manuell gepflegte
  Interfaces (`genre.ts`) implizit gegen Codegenerierung entschieden,
  konsistent mit der bestehenden Namenskonvention.

Nach Abschluss sind die entsprechenden Punkte dort abzuhaken (siehe
Dokumentation unten).

## Mit dem Projektinhaber geklärt (2026-08-13)

- **Formular-Komponentenaufteilung**: Ein gemeinsames `GenreForm` für
  Anlegen **und** Bearbeiten (Input-Signal steuert den Modus), keine zwei
  getrennten Dateien. Das weicht vom Namensschema in
  `entwicklung/codierrichtlinien.md` (`create{Entität}.modal.ts` /
  `{entität}-update.modal.ts`) ab — folgt stattdessen dem expliziten
  Komponentendiagramm für Genre in `architektur/angular-projektstruktur.md`
  (zeigt genau eine `GenreFormComponent`). Der Widerspruch zwischen beiden
  Wiki-Seiten wird nach Umsetzung im Wiki aufgelöst (siehe Dokumentation).

## Ist-Stand (verifiziert)

- `features/genres/genres.ts` (+`.html`, `.routes.ts`, `.spec.ts`) ist eine
  reine Platzhalter-Komponente, identisch aufgebaut wie `records/`,
  `artists/`, `labels/`, `dashboard/`. `genres.routes.ts` bindet bereits
  `{ path: '', component: Genres }`, `app.routes.ts` lädt `genresRoutes`
  bereits lazy — beides bleibt unverändert.
- Noch **kein `shared/`-Ordner** vorhanden. Laut
  `architektur/angular-projektstruktur.md` dafür vorgesehen: Paginierung,
  Filter-Bar, Modal-Wrapper — wird mit diesem Block erstmals angelegt.
- `core/runtime-config/runtime-config.service.ts`: `RuntimeConfigService
  .apiBaseUrl` liefert die API-Basis-URL, `load()` bereits global in
  `app.config.ts` per `provideAppInitializer` aufgerufen.
- `HttpClient` ist bereits global via `provideHttpClient(withInterceptors([
  unauthorizedRedirectInterceptor, authInterceptor()]))` in `app.config.ts`
  bereitgestellt. Access Token wird für jede URL unter `apiBaseUrl`
  automatisch angehängt (`secureRoutes` in `core/auth/
  keycloak-config.factory.ts`); 401/403 werden bereits automatisch von
  `core/auth/unauthorized-redirect.interceptor.ts` behandelt (Re-Login) —
  ein neuer `GenreService` muss dafür keinen eigenen Code schreiben.
- `core/theme/theme.service.ts` und `src/app/nav/nav.ts` sind die
  Referenzmuster für Signal-Services/-Komponenten (`inject()`,
  `signal()`/`computed()`/`effect()`, `toSignal()` statt `AsyncPipe`, kein
  `NgModule`). `nav.ts` zeigt den bisher einzigen Signal-Forms-Einsatz im
  Projekt (`form(signal({query:''}))` + `[formField]`), **ohne**
  Validierung.
- **Verifiziert gegen die installierten Typdefinitionen** (nicht nur
  Dokumentation):
  - `rxResource` (`node_modules/@angular/core/types/rxjs-interop.d.ts:189`):
    Signatur `rxResource({ params: (ctx) => R, stream: ({params}) =>
    Observable<T> })` → `ResourceRef<T>` mit `.value()`, `.isLoading()`,
    `.error()`, `.reload()`. **Kein Einsatz bisher im Repo** — dieser Block
    ist der Präzedenzfall, obwohl CLAUDE.md `rxResource` statt
    `ngOnInit`+`subscribe` vorschreibt.
  - Signal-Forms-Validierung (`node_modules/@angular/forms/types/
    signals.d.ts`): `required(path, config?)`, `minLength(path, n,
    config?)`, `maxLength(path, n, config?)`, `pattern(path, regex,
    config?)`, `debounce(path, ms)` bestätigt vorhanden;
    `submit(form, action)` (`_structure-chunk.d.ts:2093`) →
    `Promise<boolean>` für serverseitige Fehler-Einhängung ins Formular.
  - `@lucide/angular`: `LucidePlus`, `LucidePencil`, `LucideTrash2`,
    `LucideTriangleAlert`, `LucideCircleAlert`, `LucideChevronLeft`,
    `LucideChevronRight`, `LucideRefreshCw`, `LucideX` vorhanden, keine neue
    Abhängigkeit nötig.
- `tailwind.config.js`: Token-Utilities (`bg-surface`, `text-fg`,
  `border-line`/`border-line-strong`, `danger`/`danger-bg` usw.) bereits
  definiert. **Keine `.table`/`.pagination`-CSS-Klasse** im Design System
  (`components.css`) vorhanden — Tabellen-/Paginierungs-Layout wird daher
  mit diesen Token-Utilities umgesetzt statt neuer globaler CSS-Klassen
  (kein Ad-hoc-Hex, keine beliebigen px-Werte), analog zum bereits in
  `nav.html` etablierten Mischstil (Component-Klassen + Tailwind-Utilities).
- **Kein `HttpTestingController`-Einsatz bisher im Repo** — `@angular/
  common/http/testing` ist über das bestehende `@angular/common`-Paket
  verfügbar, aber noch nirgends verwendet.
- API-Vertrag Genre (aus Backend-Code gelesen, verbindlich):
  - `GET /api/genres?page&pageSize&name` → `200 { items: {id,name}[],
    totalCount, page, pageSize, totalPages }`. `name`: case-insensitive
    Contains-Filter. Sortierung serverseitig fest nach Name (kein
    `sortBy`). `page`/`pageSize` werden serverseitig geklemmt (min 1 /
    1–100), kein 400 bei ungültigen Werten.
  - `POST /api/genres` Body `{ name }` → `201 { id, name }`.
    `PUT /api/genres/{id}` Body `{ name }` → `200 { id, name }`.
    `DELETE /api/genres/{id}` → `204`.
  - Validierung: `name` Pflicht, 3–50 Zeichen, Regex
    `^[\p{L}\p{N} \-&']+$` (JS mit `u`-Flag:
    `/^[\p{L}\p{N} \-&']+$/u`) — identisch client- und serverseitig.
  - 400-Fehlerformat: `{ errors: { "Name": string[] }, title, status }` —
    Key im `errors`-Objekt ist **PascalCase** `"Name"` (FluentValidation-
    Verhalten, nicht die sonstige camelCase-JSON-Konvention). 404/409/500:
    `{ title, detail, status }`. 409 bei Create/Update = Name-Duplikat für
    diesen Benutzer; 409 bei Delete = Genre noch von `record_track`
    referenziert.
- Fehlerdarstellung exakt (`architektur/fehler-und-ausnahmekonzept.md`): 400
  → Inline am Feld. 404/409/500 → Modal mit OK. Netzwerkfehler (`status ===
  0`) → Modal mit „Erneut versuchen". 401/403 → bereits global behandelt.
  Delete → eigenes Sicherheits-Modal, kein Inline-Confirm, kein Toast.
- Layout-Vorgabe (`architektur/ui-ux-konzept.md`, „Tabellen-Slices"):
  Container `max-width 1080px` zentriert, `padding 24px 24px 64px`; Toolbar
  Überschrift+Anzahl-Badge links, Filterfeld+„Anlegen"-Button rechtsbündig;
  Tabelle feste Spaltenbreiten, zentriert; Paginierung darunter,
  rechtsbündig, nummeriert; keine Breadcrumbs; Empty State reiner Text
  `.empty`; Loading State `.spinner`, Tabelle erst nach Laden gerendert.

## Vorgeschlagene Schritte

### 1. `shared/`-Basisbausteine (neu angelegter Ordner)

- `shared/http/problem-details.ts`: Typen `ProblemDetails`,
  `ValidationProblemDetails` (spiegeln die ASP.NET-Core-Fehlerform).
- `shared/modal/modal.ts`/`.html`/`.spec.ts`: genereller Wrapper um
  `.scrim`/`.modal` aus `components.css` (Escape-Taste und Klick auf Scrim
  schließen, Klick im Panel nicht; `<ng-content>` für Kopf/Body/Footer).
- `shared/confirm-modal/confirm-modal.ts`/`.html`/`.spec.ts`:
  Lösch-Sicherheitsabfrage (`title`/`message`-Input, `confirmed`/
  `cancelled`-Output), nutzt `Modal` + `.btn-danger` + `.modal-danger-icon`.
- `shared/error-modal/error-modal.service.ts`/`.spec.ts`: `providedIn:
  'root'`, Signal-State (`ErrorModalKind: 'not-found'|'conflict'|'server'|
  'rate-limit'|'network'`), `showFromHttpError(error, entityName, onRetry?)`
  mappt `HttpErrorResponse` auf eine Anzeige, ignoriert 401/403 explizit
  (bereits vom Interceptor behandelt), loggt nur im Dev-Modus
  (`isDevMode()`).
- `shared/error-modal/error-modal.ts`/`.html`/`.spec.ts`: konsumiert den
  Service, wird einmalig in `app.html` neben `<app-nav />` gemountet — gilt
  ab sofort für alle künftigen CRUD-Slices.
- `shared/pagination/pagination.ts`/`.html`/`.spec.ts`: `page`/
  `totalPages`-Input, `pageChange`-Output, nummerierte Seiten rechtsbündig,
  Vor/Zurück mit `LucideChevronLeft`/`LucideChevronRight`.

### 2. Genre-Modell und -Service

- `features/genres/genre.ts`: Interfaces `Genre { id, name }`,
  `GenreListResponse`, `CreateGenreRequest`, `UpdateGenreRequest` (eigene
  Typen statt Alias, da Create/Update fachlich künftig auseinanderdriften
  könnten).
- `features/genres/genre.service.ts`/`.spec.ts`: `GenreService`
  (`providedIn: 'root'`), `getPaged(page, pageSize, name?)`, `create`,
  `update(id, req)`, `delete(id)` — alle als `Observable<T>` (kein
  `subscribe()` im Service). `baseUrl` aus `RuntimeConfigService
  .apiBaseUrl` + `/api/genres`. Test: erstes `HttpTestingController`-
  Beispiel im Repo (`provideHttpClientTesting()`).

### 3. `Genres`-Shell mit `rxResource`

`features/genres/genres.ts`/`.html`/`.spec.ts` ersetzt den Platzhalter
vollständig. Hält `filterName`/`page`/`pageSize` als Signals,
`genresResource = rxResource({ params: () => ({page: this.page(), pageSize:
this.pageSize(), name: this.filterName()}), stream: ({params}) =>
this.genreService.getPaged(...) })`. `genresResource.isLoading()` steuert
`.spinner`, `genresResource.error()` geht an `ErrorModalService` (per
`effect()`, mit `reload()` als Retry-Callback). Layout: Container/Toolbar
gemäß Ist-Stand-Vorgabe, zunächst ohne Subkomponenten (Platzhalter-Markup).

### 4. `GenreFilter`

`features/genres/genre-filter/genre-filter.ts`/`.html`/`.spec.ts`: ein
Namensfeld, Signal Form mit `debounce(name, 300)` (Live-Filterung ohne
Enter, im Unterschied zum Submit-Muster im Nav-Suchfeld),
`filterChange`-Output. In `genres.html` einhängen; `onFilterChange()` in
`Genres` setzt zusätzlich `page.set(1)` zurück.

### 5. `GenreTable` mit Paginierung

`features/genres/genre-table/genre-table.ts`/`.html`/`.spec.ts`: Tabelle
mit fester Spaltenaufteilung, `.spinner` während `loading()`, `.empty` bei
leerer Liste, `editRequested`/`deleteRequested`-Output je Zeile (Icons
`LucidePencil`/`LucideTrash2`), eingebettete `<app-pagination>` unterhalb.
In `genres.html` einhängen.

### 6. `GenreForm` mit Signal-Forms-Validierung

`features/genres/genre-form/genre-form.ts`/`.html`/`.spec.ts`: `genre:
Genre | null`-Input (`null` = Create-Modus), Signal Form mit
`required`/`minLength(3)`/`maxLength(50)`/`pattern(/^[\p{L}\p{N}
\-&']+$/u)`. Speichern über `submit(genreForm, action)`: bei Erfolg
`saved`-Output; bei HTTP 400 vom Server wird die Meldung ins Feld
eingehängt (Inline, `.is-error`/`.hint.is-error`); bei 404/409/500/
Netzwerkfehler geht der Fehler an `ErrorModalService`. Wird von `Genres`
per `@if (formOpen())` bei jedem Öffnen frisch erzeugt (kein
Resync-`effect()` für Input-Wechsel nötig). Verdrahtung „Genre
anlegen"-Button (Create) und Zeilen-Bearbeiten-Icon (Edit) in `Genres`.

### 7. `ConfirmModal`-Verdrahtung für Delete

In `Genres`: Löschen-Icon-Klick setzt `pendingDelete`-Signal, öffnet
`ConfirmModal` mit Genre-Namen; Bestätigen löst `GenreService.delete(id)`
aus, danach `genresResource.reload()`; 409 (noch referenziert) oder 404
(zwischenzeitlich gelöscht) gehen an `ErrorModalService`; Abbrechen löst
keinen HTTP-Call aus.

### 8. Tests

Durchgängig mitgeschrieben je Schritt (nicht am Ende gesammelt):
`// arrange`/`// act`/`// assert`-Kommentare (Pflicht),
`TestBed.configureTestingModule`, `await fixture.whenStable()` nach jedem
`HttpTestingController.flush()` (Zoneless-App). Mindestfälle:
Service-Query-Parameter/Methoden/Fehler-Propagation; Filter-Debounce;
Tabellen-Zustände (Loading/Empty/gefüllt) und Zeilen-Events; Formular-
Validierung (leer/zu kurz/zu lang/verbotenes Zeichen) inline, Create/Edit-
Unterscheidung, 400 vom Server inline, 409/404/500/Netzwerk →
`ErrorModalService`; Error-Modal-Mapping je Statuscode inkl. 401/403-Guard;
Confirm-/Modal-Wrapper isoliert; `genres.spec.ts` integrationsnah mit
`HttpTestingController` über den gesamten Lebenszyklus (Laden, Filtern,
Paginieren, Anlegen, Bearbeiten, Löschen inkl. Abbrechen, 404/409-Pfade).

### 9. Dokumentation

- `TASK.md`: Slice 2 auf „Backend und Frontend abgeschlossen" aktualisieren.
- `03 Ressourcen/offene-punkte-angular-feature-slices.md`: Punkte 1, 2 und 4
  abhaken (mit Verweis auf die hier getroffenen Entscheidungen); Punkt 3 als
  „teilweise beantwortet" (Paginierung geklärt, Sortierung offen bis
  Record) vermerken; Punkt 6 als implizit beantwortet vermerken
  (manuell gepflegte Interfaces, keine Codegenerierung) — zur expliziten
  Bestätigung durch den Projektinhaber markieren.
- Wiki: Widerspruch zwischen `architektur/angular-projektstruktur.md`
  (eine `GenreFormComponent`) und `entwicklung/codierrichtlinien.md`
  (Namensschema für zwei Dateien) auflösen — Codierrichtlinien-Tabelle um
  eine Anmerkung ergänzen, dass bei einer gemeinsamen Formularkomponente
  ein anderes Namensschema gilt (z. B. `{entität}-form.ts`).
- Neuer ADR `docs/adr/0013-...` für die Fehlerdarstellungs-Architektur
  (`ErrorModalService` als zentraler, wiederverwendbarer Mechanismus für
  404/409/500/Netzwerkfehler vs. Alternativen wie ein globaler HTTP-
  Interceptor oder `catchError` je Service) — echte Trade-off-Entscheidung
  mit projektweiter Tragweite für alle künftigen CRUD-Slices.

## Verifikation

1. `npm run build` in `src/frontend` — Production-Build erfolgreich.
2. `npm test -- --watch=false` — alle Frontend-Tests grün (neue und
   bestehende).
3. `ng lint`, `git diff --check` (Zeilenlänge 120 Zeichen).
4. Manuelle Live-Prüfung im Browser über den Aspire-AppHost gegen das
   laufende Backend: Liste laden (Loading → Daten), nach Namen filtern,
   paginieren, Genre anlegen (inkl. Inline-400 bei ungültigem Namen),
   Duplikat anlegen (409-Modal), Genre bearbeiten, Genre löschen mit
   Bestätigung, Löschen abbrechen, Netzwerkfehler simulieren (Backend
   stoppen) für „Erneut versuchen".

## Risiken und offene Punkte

- Fehlende `.table`/`.pagination`-CSS-Klassen im Design System werden mit
  Tailwind-Token-Utilities pragmatisch gelöst — falls Artist/Label/Record
  denselben Bedarf bestätigen, sollte das später als eigene
  Design-System-Klasse nachgezogen werden.
- Rate Limiting (429) ist laut Recherche im Backend-Code noch nicht
  implementiert (nur CLAUDE.md-Vorgabe) — die Frontend-Fehlerbehandlung
  sieht den Fall vor, kann aber nicht gegen echtes Serververhalten
  verifiziert werden.
- Sortierungs-UI-Muster (Punkt 3 der offenen Punkte) bleibt bis zum
  Record-Slice offen, da Genre keine wählbare Sortierung hat.
- `shared/modal`, `shared/confirm-modal`, `shared/error-modal`,
  `shared/pagination` gehen über den engsten Genre-Bedarf hinaus (sind für
  Artist/Label/Record mitgedacht) — falls das im Nachhinein als zu großer
  Vorgriff bewertet wird, lassen sich Pagination/Modal-Wrapper notfalls
  auch lokal in `features/genres/` verschieben.
