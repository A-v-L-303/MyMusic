# ADR 0012 — Icon-Bibliothek `@lucide/angular` (Nachfolge zu ADR 0011)

**Status**: Angenommen
**Datum**: 2026-08-13
**Betrifft**: `src/frontend`

## Kontext

ADR 0011 (2026-08-11, Dark/Light-Theme-Infrastruktur) hat die Einführung von
`lucide-angular` als npm-Paket bewusst auf den künftigen Block mit der echten
`NavComponent` verschoben — dort werden laut `wiki/glossar.md` rund zehn
weitere Icons gebraucht (Dashboard, Records, Artists, Labels, Genres, Search,
Add, Edit, Delete, Logout).

Block 0g (NavComponent und Routing-Skelett) ist dieser Block. Bei der
Prüfung vor der Installation hat sich herausgestellt, dass `lucide-angular`
laut Hersteller inzwischen deprecated ist — Nachfolger ist das umbenannte,
gescopte Paket `@lucide/angular`.

## Entscheidung

`@lucide/angular` wird installiert, nicht `lucide-angular`.

Geprüft vor der Installation (CLAUDE.md §12):

- **Version**: 1.31.0 zum Installationszeitpunkt, Peer-Dependencies
  `@angular/common`/`@angular/core` `>=17.0.0` — kompatibel mit dem im
  Projekt installierten Angular 22.1.
- **Lizenz**: ISC, identisch zur bereits in ADR 0011 bewerteten Lizenzlage
  des Lucide-Projekts.
- **Wartung**: `lucide-angular` trägt im npm-Registry den Hinweis
  „deprecated", `@lucide/angular` ist das aktiv gepflegte Nachfolgepaket
  (letzte Version zum Prüfzeitpunkt wenige Tage alt).
- **Alternative geprüft**: weiterhin Inline-SVG wie in `theme-toggle.html`
  (ADR 0011). Bei sieben in diesem Block benötigten Icons (Dashboard,
  Records, Artists, Labels, Genres, Search, Chevron) und absehbar weiteren
  in künftigen CRUD-Blöcken (Add/Edit/Delete/Logout laut `glossar.md`)
  überwiegt der Duplikationsaufwand den Vorteil, keine zusätzliche
  Abhängigkeit einzuführen — anders als bei den zwei Icons in ADR 0011.

**API-Unterschied zur alten `lucide-angular`-API**: kein
`LucideAngularModule.pick({...})` mehr. Stattdessen stellt `@lucide/angular`
jedes Icon als eigene, tree-shakebare Standalone-Komponente bereit (z. B.
`LucideLayoutDashboard`, Selector `svg[lucideLayoutDashboard]`), die einzeln
in die `imports`-Liste der jeweiligen Komponente aufgenommen wird — passt
zum Standalone-/Signal-/Zoneless-Stil des restlichen Projekts.

## Begründung

Kein Grund, bewusst ein als deprecated markiertes Paket neu einzuführen,
wenn der nahezu identische Nachfolger keine funktionalen Nachteile hat und
die gleiche Lizenz sowie eine mindestens ebenso breite Angular-Kompatibilität
mitbringt.

## Konsequenzen

- Wiki-Erwähnungen von `lucide-angular` (`glossar.md`,
  `design-system-überblick.md`) sind durch die Umbenennung technisch
  veraltet und sollten bei Gelegenheit auf `@lucide/angular` aktualisiert
  werden — reine Doku-Korrektur ohne fachliche Auswirkung, nicht Teil dieses
  Blocks.
- ADR 0011 bleibt inhaltlich gültig (Storage-Key, Drei-Zustands-Logik,
  FOUC-Script unverändert) — nur dessen Punkt 3 (Icon-Herkunft) ist durch
  diesen ADR für alle künftigen Blöcke fortgeschrieben. `theme-toggle.html`
  selbst bleibt unverändert bei Inline-SVG (kein Migrationsdruck für
  bereits bestehenden, funktionierenden Code).
