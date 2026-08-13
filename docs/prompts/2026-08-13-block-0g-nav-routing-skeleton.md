# Block 0g — NavComponent und Routing-Skelett

## Kontext

Backend-seitig sind Genre/Country/Label/Artist/Record vollständig fertig. Das
Angular-Frontend hat seit Block 7a einen funktionierenden Login-Flow, aber
noch keine einzige echte Feature-Seite — `app.routes.ts` zeigte bis zu diesem
Block für `''` und `'**'` gleichermaßen auf eine provisorische
`HomePlaceholder`-Komponente aus Block 0c.

Der nächste fachliche Schritt wäre die Angular-Anbindung der bestehenden
CRUD-Backends (Genre/Label/Artist/Record). Bevor das passiert, soll aber die
Navigation (Kopfzeile mit Tabs, Suchfeld, Option-Dropdown) samt
Routing-Skelett einmal sauber stehen — sonst müsste jeder der vier kommenden
Feature-Blöcke die Navigationsanbindung einzeln nachziehen, statt einmal
zentral zu verdrahten.

Das Navigations-Konzept im Wiki (`wiki/architektur/navigation-konzept.md`)
wurde am 2026-08-13 final abgeschlossen und ist damit vollständig
implementierbar.

**Mit dem Projektinhaber geklärter Scope:**

- **Kein Admin-Button/AdminGuard** in diesem Block — bleibt eigener Punkt des
  noch offenen Rollenkonzepts (TASK.md Abschnitt 7).
- **Kein responsives Hamburger-Menü** — nur normales Desktop/Tablet-Verhalten,
  Icon-only/Overflow-Verhalten wird zurückgestellt.
- **Suchfeld funktional**: Eingabe + Enter navigiert zu `/search?q=...`;
  Zielseite ist selbst nur ein Platzhalter.
- **Kein Benutzerprofil-Modal** — Username bleibt reiner Text ohne
  Klick-Handler, wie bisher.
- **`@lucide/angular` wird installiert** — der Nachfolger des in ADR 0011
  genannten (mittlerweile deprecated) `lucide-angular`. Unterstützt
  Angular 17+ mit Standalone/Signals/Zoneless, ISC-lizenziert, passt zum
  vorhandenen Zoneless-Setup.

## Ist-Stand (verifiziert)

- `src/frontend/src/app/` hatte noch keinen `features/`-Ordner und keine
  `NavComponent`.
- `app.routes.ts`: `{ path: '', component: HomePlaceholder, canActivate:
  [authGuard] }`, `{ path: '**', component: HomePlaceholder, canActivate:
  [authGuard] }`.
- `app.ts`/`app.html`: Root-Komponente injizierte `OidcSecurityService`
  direkt (Signals `authenticated`, `username`), rendert Logo,
  `<app-theme-toggle>`, Login/Logout/Username — mit reinen
  Tailwind-Utilities, nicht mit den Design-System-Klassen `.appbar`/`.brand`.
- `core/theme/theme-toggle/` ist das Referenzmuster für Signal-basierte,
  standalone Komponenten (`inject()`, `computed()`, kein `NgModule`, Tests
  via `TestBed` + `useValue`-Mocks, `// arrange`/`// act`/`// assert`).
- `core/shell/home-placeholder/` wird nur von sich selbst und
  `app.routes.ts` referenziert — gefahrlos entfernbar.
- Design-System-CSS (`components.css`) enthielt bereits ungenutzt: `.appbar`,
  `.brand`, `.tabs`/`.tab`/`.tab.is-active`, `.search`, `.btn*`-Varianten.
  Die `.select`-Klasse verwendet bereits ein Chevron-Down-SVG — exakt das
  Lucide-`chevron-down`-Icon, guter Präzedenzfall für den Dropdown-Trigger.
- `@angular/forms/signals` (Signal Forms) ist bereits über `@angular/forms`
  installiert, aber noch nirgends verwendet.
- ADRs waren lückenlos 0001–0011 durchnummeriert, nächste freie Nummer: 0012.

## Vorgeschlagene Schritte

### 1. `@lucide/angular` installieren

`npm install @lucide/angular` in `src/frontend`. Verifiziert: Version 1.31.0,
Peer-Deps `@angular/common`/`@angular/core` `>=17.0.0`, ISC-Lizenz — kompatibel
mit Angular 22.1. API: einzelne Standalone-Icon-Komponenten (z. B.
`LucideLayoutDashboard`, Selector `svg[lucideLayoutDashboard]`), kein
`LucideAngularModule.pick(...)` mehr wie beim alten `lucide-angular`.

### 2. Routing-Skelett und sechs Platzhalter-Features

`app.routes.ts` neu: verschachtelte Route mit `canActivate: [authGuard]` auf
dem Eltern-Knoten, sechs `loadChildren`-Einträge auf
`features/*/*.routes.ts` (dashboard, records, artists, labels, genres,
search), `''` → Redirect `/dashboard`, `'**'` → Redirect `/dashboard` (kein
eigenes 404-Konzept im Wiki, Redirect ist wartungsfrei).

Je Feature ein eigener, minimaler `features/{name}/`-Ordner mit
Platzhalter-Komponente (Feature-Titel + `.empty`-Text „Diese Ansicht folgt in
einem späteren Block.") und `{name}.routes.ts`. Kein gemeinsames
`shared/feature-placeholder/` — `shared/` ist laut Wiki für dauerhaft
wiederverwendbare Bausteine reserviert, ein Platzhalter ist reiner
Wegwerfcode, der pro Feature-Block komplett ersetzt wird. `search` liest den
`q`-Query-Parameter über `toSignal(route.queryParamMap...)`.

### 3. `core/shell/home-placeholder/` entfernen

`''` redirected künftig auf `/dashboard`, die Komponente wird nirgends mehr
referenziert.

### 4. `NavComponent` (`src/app/nav/`)

- Brand (Logo + „MyMusic") mit den Design-System-Klassen `.appbar`/`.brand`;
  1080px-Breitenbegrenzung des Inhalts als Tailwind-Utility auf einem inneren
  Wrapper-`<div>` (`components.css` bleibt unverändert).
- Tabs Dashboard/Records (`.tab`/`.tab.is-active` einzeln, ohne
  `.tabs`-Wrapper, da dessen `border-bottom` für die *alte* separate
  Tab-Zeile gedacht war).
- Option-Dropdown (Artists/Labels/Genres) ohne eigene Sub-Komponente:
  `signal(false)` + Methoden direkt in `Nav`, Schließen bei Klick außerhalb
  über `@HostListener('document:click', ...)`.
- Suchfeld via Signal Forms (`form(signal({ query: '' }))` +
  `[formField]`-Direktive), Submit/Enter → `router.navigate(['/search'], {
  queryParams: { q } })`, leere Eingabe navigiert nicht.
- `<app-theme-toggle>` (nur Positionswechsel, Komponente bleibt
  unverändert), Login/Logout/Username-Logik unverändert aus `App`
  übernommen.
- Icons: `layout-dashboard`, `disc-3`, `users`, `tag`, `music`, `search`,
  `chevron-down` (Namen bis auf `chevron-down` in `wiki/glossar.md`
  festgelegt; `chevron-down` durch Präzedenzfall `.select`-Chevron
  begründet).

### 5. `App` auf reine Shell reduzieren

`app.ts`/`app.html`/`app.css` auf `<app-nav /><router-outlet />` kürzen —
die Auth- und Theme-Logik ist nach `Nav` gewandert.

### 6. Tests

Je Schritt mitgeschrieben: `nav.spec.ts`, reduziertes `app.spec.ts`, neues
`app.routes.spec.ts` (`RouterTestingHarness`, `angular-auth-oidc-client` per
`vi.mock` neutralisiert, um nur die eigene Routing-Verdrahtung zu prüfen —
das Guard-Verhalten selbst ist Bibliothekscode), sechs
`features/*/*.spec.ts`.

### 7. Dokumentation

Neuer TASK.md-Block „0g. NavComponent und Routing-Skelett" (passt zur
Fundament-Reihe 0a–0f, da Shell-/Routing-Infrastruktur statt fachlicher
Slice), ADR 0012 (Paketname-Änderung `lucide-angular` → `@lucide/angular`,
Nachfolge zu ADR 0011), `03 Ressourcen/offene-punkte-angular-feature-slices.md`
Punkt 2 (Signal Forms) als „teilweise beantwortet" vermerken (nur der
einfache Ein-Feld-Fall, Validierungsregeln bleiben offen).

## Verifikation

1. `npm run build` in `src/frontend` — Production-Build erfolgreich.
2. `npm test -- --watch=false` — alle Frontend-Tests grün.
3. Manuelle Live-Prüfung im Browser über den Aspire-AppHost: Kopfzeile nach
   Login, Tab-Navigation inkl. aktivem Zustand, Option-Dropdown, Suche mit
   Enter, direkter Aufruf einer unbekannten URL landet auf `/dashboard`.
4. `git diff --check` und Zeilenlängen-Stichprobe (120 Zeichen).
