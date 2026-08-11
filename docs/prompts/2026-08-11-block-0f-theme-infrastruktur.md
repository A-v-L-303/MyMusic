# Block 0f — Dark/Light-Theme-Infrastruktur

## Kontext

Das Planungs-Wiki wurde am 2026-08-10 überarbeitet, weil bisher nicht umgesetzte
Frontend-Planungsdetails aufgefallen sind. Ein konkreter Punkt: `navigation-konzept.md`
enthielt bis dahin keinen Light/Dark-Toggle, obwohl `ui-kit.md` ihn schon seit 2026-05-30
beschrieb — ein Widerspruch, der mit dem Projektinhaber aufgelöst wurde (Toggle wird
ergänzt, siehe Wiki `log.md`, Eintrag „Navigation-Konzept: Light/Dark-Toggle ergänzt").

Der Angular-Code hat dieses (und jedes andere Navigations-)Feature bisher nie umgesetzt:
Block 0c hat nur eine minimale App-Shell (Logo, Titel, Login/Logout) angelegt, „bewusst
ohne Navigation, Routing-Hierarchie oder Feature-Ordner" (TASK.md, Block 0c). Es gibt
weder eine echte `NavComponent` noch eine Tab-Leiste noch Tabellen-Slices — diese Teile
sind für einen späteren Block vorgesehen und nicht Gegenstand dieses Blocks.

**Abgestimmter Scope**: ausschließlich die Dark/Light-Theme-Infrastruktur
(Umschalt-Mechanismus + Toggle-Button in der bestehenden Kopfzeile). Favicon, Tab-Leiste,
Tabellen-Layouts und die vollständige `NavComponent` bleiben ausdrücklich außen vor, da
dafür noch keine Code-Basis existiert.

## Ist-Stand (verifiziert)

- `tailwind.config.js:20` ist bereits korrekt konfiguriert:
  `darkMode: ["selector", '[data-theme="dark"]']` — keine Änderung nötig.
- `src/styles/design-system/colors_and_type.css` enthält bereits vollständige
  Light-Tokens (`:root`, Zeilen 12–131), einen expliziten Dark-Override
  (`:root[data-theme="dark"]`, Zeilen 134–169) und einen OS-Fallback
  (`@media (prefers-color-scheme: dark) { :root:not([data-theme="light"]) {...} }`,
  Zeilen 172–190) — keine Änderung nötig.
- `design-system-überblick.md:82` legt die verbindliche Drei-Zustands-Logik fest:
  „Standard-Theme = Light; Dark-Mode via `data-theme="dark"` am `<html>` oder
  OS-Präferenz." D. h.: kein Attribut = Light, außer OS bevorzugt Dark (Media-Query
  greift); `data-theme="dark"`/`"light"` erzwingt das jeweilige Theme unabhängig vom OS.
- **Gemeldete Abweichung**: Der Rohprototyp (`raw/design_system/ui_kits/web/index.html`)
  initialisiert stattdessen hart mit `localStorage.getItem("mm-theme") || "dark"` (Default
  Dark, immer explizites Attribut, kein echter OS-Follow-Zustand). Das widerspricht dem
  Wiki-Text. Da das Wiki laut Projektregel vor Beispielcode aus `raw/` sticht, folgt dieser
  Block dem Wiki-Text (Default Light + echter OS-Follow-Modus), nicht dem Demo-Verhalten.
- `app.html` (19 Zeilen) hatte vor diesem Block nur Logo/Titel links sowie rechts entweder
  einen Login-Button oder Username+Logout — kein Toggle. `app.ts` enthält ausschließlich
  OIDC-Logik. `app.css` ist leer.
- Es existierte im gesamten `src/app/`-Baum weder ein `ThemeService` noch irgendeine
  Datei mit „theme"/„dark" im Namen (per Grep bestätigt).
- `app.config.ts:25` hängt den kompletten Bootstrap an einen echten `fetch()`
  (`RuntimeConfigService.load()`) — `App` und damit ein `ThemeToggle` existieren
  erst nach diesem Netzwerk-Roundtrip. Das bedeutet: bei einer expliziten, vom OS
  abweichenden Nutzerwahl entsteht ohne Gegenmaßnahme ein sichtbarer Farb-Flash beim Laden
  (reiner OS-Follow-Fall ist davon nicht betroffen, der funktioniert schon rein über CSS).
- `app.spec.ts` benutzte an vier Stellen `compiled.querySelector('button')` (den *ersten*
  Button im DOM), um den Login/Logout-Button zu finden. Mit einem zweiten Button (Theme-
  Toggle) an der laut Wiki vorgeschriebenen Position **vor** Login/Logout fanden diese
  Tests künftig den falschen Button — musste zusammen mit der Umsetzung behoben werden.
- Lucide ist laut `glossar.md`/`design-system-überblick.md` das verbindliche Icon-Set
  („In Production über `lucide-angular` eingebunden"), aber `lucide-angular` ist noch kein
  npm-Paket im Projekt (Block 0c hat es bewusst nicht eingeführt). Ein neues Paket nur für
  zwei Icons (Sonne/Mond) einzuführen ist eine eigene, über diesen Scope hinausgehende
  Entscheidung (CLAUDE.md §12 verlangt Bedarfsbegründung + Freigabe).

## Umgesetzte Schritte

### 1. `ThemeService` — `src/app/core/theme/theme.service.ts`

Signal-basierte Drei-Zustands-Logik, konsistent zu `RuntimeConfigService` (`signal()`,
`providedIn: 'root'`):

- `userPreference = signal<'light' | 'dark' | null>(...)`, initial aus `localStorage`
  gelesen (`null`, wenn nie gesetzt oder ungültiger Wert).
- `osPrefersDark = signal(matchMedia('(prefers-color-scheme: dark)').matches)`, per
  `change`-Listener live aktualisiert.
- `effectiveTheme = computed(() => userPreference() ?? (osPrefersDark() ? 'dark' : 'light'))`.
- `toggle()` kehrt `effectiveTheme()` um und schreibt das Ergebnis nach `userPreference`
  — reine State-Transition, keine eigenen Seiteneffekte.
- Ein `effect()` im Konstruktor reagiert auf `userPreference`: bei explizitem Wert wird
  `data-theme` auf `document.documentElement` gesetzt und nach `localStorage` geschrieben;
  bei `null` wird das Attribut entfernt (reiner CSS-OS-Fallback bleibt aktiv, bis der
  Nutzer aktiv umschaltet). Zoneless-sicher, da Angulars `effect()`-Scheduler unabhängig
  von `zone.js` arbeitet (im Projekt ohnehin nicht geladen).
- Storage-Key: `mymusic-theme` (nicht `mm-theme` aus dem Rohprototyp) — CLAUDE.md §9
  verbietet Abkürzungen ausnahmslos, und der einzige vergleichbare Bezeichner im Code
  (`clientId: 'mymusic-angular'`) schreibt „mymusic" bereits voll aus.

### 2. `ThemeToggle`-Komponente — `src/app/core/theme/theme-toggle/theme-toggle.ts` + `.html`

- Selector `app-theme-toggle`, Datei-/Klassenname ohne „.component"-Suffix (konsistent zu
  `home-placeholder.ts`), `inject(ThemeService)` statt Konstruktor-Injection.
- Reiner Icon-Button (`class="btn btn-ghost btn-icon btn-sm"` — bestehende Design-System-
  Klassen, keine neue CSS-Regel nötig), `aria-label` trägt die Beschriftung (Wiki verlangt
  „Icon-Button ohne [sichtbares] Label", Barrierefreiheit braucht trotzdem ein
  zugängliches Label).
- Icon-Logik exakt nach Rohprototyp-Muster: zeigt Sonne, wenn aktuell Dark aktiv ist
  (Klick → Light), Mond, wenn aktuell Light aktiv ist (Klick → Dark).
- Icon-Herkunft: Inline-SVG mit den öffentlichen Lucide-Pfaddaten für „sun"/„moon"
  (identische Optik, keine neue Abhängigkeit). Die Einführung von `lucide-angular` als
  npm-Paket wird auf den künftigen Block mit der echten `NavComponent` verschoben, wo
  ohnehin rund zehn weitere Icons gebraucht werden (siehe `glossar.md`-Icon-Zuordnung) —
  siehe ADR 0011.

### 3. Einbindung in `app.html` / `app.ts`

- `ThemeToggle` zu `imports` in `app.ts` ergänzt.
- In `app.html` wurde die rechte Kopfzeilen-Hälfte zu einer gemeinsamen Flex-Gruppe
  zusammengefasst; `<app-theme-toggle />` steht darin immer sichtbar (unabhängig von
  `authenticated()`, da Theme keine Auth-abhängige Einstellung ist) und vor dem
  Login/Logout-Block — passend zur Wiki-Reihenfolge „… Suchfeld · Toggle · Admin ·
  Username · Logout".

### 4. FOUC-Vermeidung — Inline-Script in `index.html`

Kleines synchrones Script im `<head>`, das vor jeder Angular-Ausführung eine gespeicherte
explizite Präferenz sofort anwendet — verhindert den Farb-Flash bei abweichender
expliziter Wahl (reiner OS-Follow-Fall funktioniert bereits allein über CSS).

### 5. Tests

- `theme.service.spec.ts` (7 Fälle), `theme-toggle.spec.ts` (3 Fälle), `app.spec.ts`
  angepasst (`ThemeService`-Stub ergänzt, vier `querySelector`-Aufrufe auf
  `button.btn-secondary` präzisiert).

### 6. Dokumentation

- ADR `docs/adr/0011-theme-infrastruktur.md` (Storage-Key, Drei-Zustands-Logik vs.
  abweichendes Rohprototyp-Verhalten, Icon-Herkunft, FOUC-Script).
- `TASK.md` um Block 0f ergänzt.

## Verifikation

1. `npm test` (Vitest) im `frontend`-Verzeichnis — alle Frontend-Tests grün.
2. `npm run build` — Production-Build erfolgreich.
3. Manuelle Live-Prüfung im Browser über den Aspire-AppHost.
