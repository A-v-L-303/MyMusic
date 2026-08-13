# Fix: GenreForm befüllt das Namensfeld im Bearbeiten-Modus nicht vor

## Kontext

Während der Umsetzung von Block 4 (Label-Frontend,
`docs/prompts/2026-08-13-block-4-angular-label.md`) wurde ein echter Bug in
`LabelForm` gefunden: Das Signal-Formularmodell wurde mit
`signal(this.buildInitialModel())` als reinem Feld-Initialisierer gelesen —
zu diesem Zeitpunkt ist der `label`-Input noch nicht gesetzt, wodurch das
Formular im Bearbeiten-Modus mit leeren statt vorbefüllten Feldern startete.
Fix dort: `linkedSignal(() => this.buildInitialModel())` statt `signal(...)`.

`GenreForm` (`features/genres/genre-form/genre-form.ts`, Block 2 Frontend,
bereits auf `main` seit PR #47) verwendet exakt dasselbe Muster:
`signal<GenreFormModel>({ name: this.genre()?.name ?? '' })`. Der
bestehende Test (`genre-form.spec.ts`, „Bearbeiten-Modus") überschreibt das
Namensfeld beim Testen immer unbedingt (`typeName(fixture, 'Jazz')`), ohne
vorher den tatsächlich vorbefüllten Wert zu prüfen — ein fehlendes Prefill
wäre also unbemerkt geblieben. Als Nachtrag in TASK.md (Abschnitt 2)
festgehalten, mit dem Projektinhaber zunächst auf „später" zurückgestellt,
jetzt zur Umsetzung freigegeben.

**Kein Backend-Change, keine fachliche Änderung** — reiner Frontend-Bugfix,
identisches Verhalten bis auf die korrekte Vorbefüllung.

## Ist-Stand (verifiziert)

- `GenreForm` init: `protected readonly formModel = signal<GenreFormModel>(
  { name: this.genre()?.name ?? '' });` — liest `this.genre()` beim
  Konstruieren, bevor der Input gebunden ist.
- Kein bestehender Test prüft den vorbefüllten Wert vor dem Überschreiben.
- `linkedSignal` ist bereits im Projekt verifiziert und produktiv im Einsatz
  (`LabelForm`, `node_modules/@angular/core/types/core.d.ts:2445`,
  `@publicApi 20.0`) — kein neues API-Risiko.

## Fix

- `genre-form.ts`: Import `signal` durch `linkedSignal` ersetzt;
  `formModel = linkedSignal<GenreFormModel>(() => ({ name:
  this.genre()?.name ?? '' }))` statt `signal(...)`. Bei Genre genügt die
  Inline-Form (kein separates `buildInitialModel()` nötig, da nur ein Feld
  ohne Verzweigung — anders als bei `LabelForm` mit `countryId`).
- `genre-form.spec.ts`: neuer Test „befüllt das Namensfeld im
  Bearbeiten-Modus mit dem bestehenden Namen vor" — erstellt die Komponente
  mit einem bestehenden Genre und prüft `input.value` **vor** jeder
  Eingabe. Schlägt ohne den Fix fehl (verifiziert).

## Verifikation

1. `npm run build` — Production-Build erfolgreich.
2. `npm test -- --watch=false` — 149 Frontend-Tests grün (148 zuvor + 1
   neuer Test).
3. Prettier-Check und Zeilenlänge (≤120 Zeichen) grün.
4. Manuelle Live-Prüfung im Browser steht aus (wie schon für Block 2 und
   Block 4 vermerkt).

## Risiken und offene Punkte

- Keine fachliche Verhaltensänderung außer der Korrektur selbst — geringes
  Risiko.
- Manuelle Live-Prüfung (Genre bearbeiten, Feld zeigt jetzt den
  bestehenden Namen) noch nicht durchgeführt.
