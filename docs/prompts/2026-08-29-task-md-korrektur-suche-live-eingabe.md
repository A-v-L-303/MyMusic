# TASK.md nach Suche-Live-Eingabe-Fix korrigieren

## Kontext

PR #97 (`fix-globale-suche-live-eingabe`, gemergt) hat das Verhalten des
Kopfzeilen-Suchfelds geändert: Die Suche löst seitdem live beim Tippen aus
(debounced), eine Bestätigung durch Enter ist nicht mehr nötig, funktioniert
aber weiterhin zusätzlich. Der Projektinhaber bat darum, die Dokumentation
nach diesem Fix auf Aktualität zu prüfen.

**Geprüft** (nach Freigabe „ja, beides vorbereiten" bereits als Scope-Prüfung
mit übernommen):

- Wiki (`suche.md`, `navigation-konzept.md`, `log.md`) — bereits vollständig
  aktuell, war Teil des gemergten Fix-Commits (`cdc5c2e`).
- `README.md` — erwähnt den Enter-Trigger an keiner Stelle, nichts veraltet.
- `01 Repos/MyMusic/CLAUDE.md` und das projektübergreifende
  `01 Projekte/MyMusic/CLAUDE.md` — erwähnen den Enter-Trigger ebenfalls
  nicht, nichts veraltet.
- `TASK.md`, Abschnitt „0. Grundgerüst" (Block 0g, Zeile 585–591): Die
  historische „Abnahmekriterium erfüllt"-Notiz zur manuellen Verifikation von
  Block 0g behauptet weiterhin „Suche mit Eingabe+Enter navigiert zu
  `/search?q=...`" — das war zum Zeitpunkt von Block 0g (13.08.2026) korrekt,
  ist es aber nicht mehr.

## Geplanter Fix

`TASK.md`, nach Zeile 591 (`` `/dashboard`. ``), ergänzende Zeile in derselben
Aufzählung, analog zur bereits im Wiki verwendeten
„Korrigiert am ..."-Konvention — die ursprüngliche Aussage bleibt als
historischer Stand von Block 0g stehen, wird nicht überschrieben:

```
  (Korrigiert am 2026-08-29: Die Suche löst seitdem live beim Tippen aus,
  eine Enter-Bestätigung ist nicht mehr nötig, funktioniert aber weiterhin
  zusätzlich — siehe docs/prompts/2026-08-29-fix-globale-suche-live-eingabe.md.)
```

Keine weitere Datei betroffen.

## Geplante Verifikation

Rein redaktionelle Änderung, kein Code betroffen — keine Build-/Testschritte
nötig. Manuelle Prüfung: Zeile liest sich im Kontext des umgebenden Absatzes
korrekt.

## Bekannte Risiken und offene Punkte

Keine.
