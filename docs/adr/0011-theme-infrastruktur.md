# ADR 0011 — Dark/Light-Theme-Infrastruktur

**Status**: Angenommen
**Datum**: 2026-08-11
**Betrifft**: `src/frontend`

## Kontext

Das Wiki (`architektur/navigation-konzept.md`, `design/ui-kit.md`) sieht seit dem
2026-08-10 durchgeführten Abgleich einen Light/Dark-Toggle in der Kopfzeile vor —
ein Icon-Button (Sonne/Mond, kein Label), dessen Auswahl in `localStorage`
persistiert wird. Das Design-System (`tailwind.config.js`,
`src/styles/design-system/colors_and_type.css`) war bereits vollständig auf
Dark-Mode vorbereitet (`darkMode: ["selector", '[data-theme="dark"]']`, Light- und
Dark-Tokens, OS-Präferenz-Fallback über `prefers-color-scheme`) — die
Angular-Anwendungsschicht hat das bislang nie genutzt: kein Toggle, kein
`ThemeService`, keine Steuerung des `data-theme`-Attributs.

Drei Detailentscheidungen waren dabei nicht durch das Wiki vorgegeben und mussten
technisch getroffen werden.

## Entscheidung

### 1. `localStorage`-Key: `mymusic-theme`

Der einzige lauffähige Referenzcode (`raw/design_system/ui_kits/web/index.html`,
kein Wiki-Text, sondern Beispielcode) verwendet den Key `mm-theme`. CLAUDE.md §9
verbietet Abkürzungen ausnahmslos (`ArtistId` statt `ArtId`); der einzige
vergleichbare Bezeichner im bestehenden Frontend-Code
(`clientId: 'mymusic-angular'`, `core/auth/keycloak-config.factory.ts`) schreibt
„mymusic" bereits voll aus. Gewählt wurde deshalb `mymusic-theme`, nicht die
Abkürzung aus dem Beispielcode.

### 2. Drei-Zustands-Logik statt hartem Default

`design-system-überblick.md` legt fest: „Standard-Theme = Light; Dark-Mode via
`data-theme="dark"` am `<html>` oder OS-Präferenz." Der Beispielcode weicht davon
ab (`localStorage.getItem("mm-theme") || "dark"`, harter Dark-Default, immer
explizites Attribut). Da das Wiki laut Projektregel vor Beispielcode aus `raw/`
sticht, implementiert `ThemeService`
(`src/app/core/theme/theme.service.ts`) stattdessen drei Zustände: keine
gespeicherte Präferenz → `data-theme` bleibt unentfernt, reiner
CSS-OS-Fallback greift (inkl. Live-Reaktion auf OS-Änderungen); explizite Wahl
(`light`/`dark`) → Attribut wird gesetzt und übersteuert die OS-Einstellung dauerhaft.

### 3. Icon-Herkunft: Inline-SVG statt `lucide-angular`

Lucide ist laut `glossar.md`/`design-system-überblick.md` das verbindliche
Icon-Set des Projekts, aber `lucide-angular` ist noch kein installiertes Paket
(Block 0c hat es bewusst zurückgestellt). Für die zwei hier benötigten Icons
(Sonne/Mond) wurden die öffentlichen, ISC-lizenzierten Lucide-Pfaddaten direkt als
Inline-SVG in `theme-toggle.html` übernommen — keine neue Abhängigkeit für nur
zwei Icons. Die Einführung von `lucide-angular` (samt CLAUDE.md-§12-Freigabe:
Bedarf, Alternativen, Lizenz/Wartung) bleibt dem künftigen Block mit der echten
`NavComponent` vorbehalten, der ohnehin rund zehn weitere Icons braucht (siehe
Icon-Zuordnung in `glossar.md`).

### 4. Inline-Script gegen Flash-of-wrong-theme in `index.html`

`app.config.ts` hängt den gesamten Angular-Bootstrap an einen echten
`fetch()`-Aufruf (`RuntimeConfigService.load()`, ADR 0009) — `ThemeService`
existiert erst nach diesem Netzwerk-Roundtrip. Bei einer explizit vom OS
abweichenden Nutzerwahl entstünde ohne Gegenmaßnahme ein kurzer sichtbarer
Farbsprung beim Laden (der reine OS-Follow-Fall ist davon nicht betroffen, der
funktioniert bereits allein über die CSS-Media-Query). Ergänzt wurde ein
synchrones, drei Zeilen kurzes Inline-Script im `<head>` von `index.html`, das
vor jeder Angular-Ausführung den gespeicherten Wert liest und das Attribut
setzt. Kein Angular-Code, sondern reines Host-HTML — vergleichbar mit dem dort
bereits vorhandenen `favicon.ico`-Link, kein Anwendungslogik-Bruch, da der
Vorgang der Angular-Ausführung technisch zwingend vorgelagert ist.

## Begründung

- Alle vier Punkte sind eng miteinander verzahnte Implementierungsentscheidungen
  desselben, in sich abgeschlossenen Blocks (Theme-Infrastruktur) — ein gemeinsamer
  ADR vermeidet vier separate, stark redundante Einzeldokumente.
- In allen vier Fällen sticht eine explizite Wiki-Aussage oder eine
  ausnahmslose CLAUDE.md-Regel vor dem abweichenden Verhalten des
  Rohprototyps — die Abweichung wird hier dokumentiert statt stillschweigend
  aufgelöst (CLAUDE.md §2.1).

## Konsequenzen

- `ThemeService` (`providedIn: 'root'`) und `ThemeToggle`
  (`src/app/core/theme/theme-toggle/`) sind die einzigen neuen Bausteine dieses
  Blocks; `tailwind.config.js` und die Design-System-CSS-Dateien blieben
  unverändert, da sie die nötige Grundlage bereits vollständig enthielten.
- Ein späterer Wechsel von Inline-SVG auf `lucide-angular` bleibt lokal auf
  `theme-toggle.html` begrenzt (Template-Austausch, keine API-Änderung der
  Komponente).
- Der `localStorage`-Key `mymusic-theme` und die im Inline-Script duplizierte
  Werte-Prüfung (`'light'`/`'dark'`) sind ein manuell zu pflegender Sync-Punkt
  zwischen `index.html` und `theme.service.ts` — bei nur zwei möglichen Werten
  ein geringes, beherrschbares Risiko.
