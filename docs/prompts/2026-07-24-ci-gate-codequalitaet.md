# CI-Gate für Codequalität einrichten

## Kontext

In einer Diskussion über die Verbindlichkeit von CLAUDE.md-Regeln wurde
herausgearbeitet: Eine Regel in CLAUDE.md ist eine Instruktion, der in jeder
Session erneut gefolgt werden muss — ein wiederkehrendes, im Voraus nicht
prüfbares Risiko. Ein technisches Gate verschiebt den Fehlerfall auf einen
einmaligen, testbaren Zeitpunkt (die Einrichtung selbst) und wirkt danach
unabhängig davon, ob eine Regel in der jeweiligen Session korrekt angewendet
wurde.

Ziel: die generisch durch Tooling prüfbaren Teile der Codierrichtlinien
(`entwicklung/codierrichtlinien.md`) technisch in einer CI-Pipeline
durchsetzen, plus die dazugehörige Dokumentation.

## Entscheidungen

Siehe `docs/adr/0003-ci-gate-codequalitaet.md` für die vollständige Begründung.
Kurzfassung:

1. Kein StyleCop.Analyzers/Roslynator — Bordmittel des .NET-SDK
   (`.editorconfig` + `dotnet format`) reichen für die aktuell verbindlichen,
   generisch prüfbaren Regeln.
2. Zeilenlänge (120 Zeichen) wird per eigenem `grep`-Schritt in der
   CI-Pipeline geprüft, nicht über `.editorconfig`/`dotnet format` — dafür
   gibt es keinen eingebauten SDK-Analyzer.
3. `MyMusic.IntegrationTests` wird kompiliert, aber nicht ausgeführt (braucht
   Docker + Aspire-Orchestrierung + Secrets — eigener, größerer Schritt).
4. Kein Branch-Protection-Rule-Setup (Repository-Einstellung außerhalb des
   Codes, eigener Schritt).
5. Projektspezifische Regeln (Namensschemata, Feature-Kapselung,
   Kommentar-Ausnahmen) bleiben Aufgabe des `reviewer`-Subagenten.

## Umgesetzte Schritte

1. `.editorconfig` im Repo-Root: file-scoped Namespaces, Naming-Regel für
   private Felder (`_camelCase`, Konstanten ausgenommen), `max_line_length`
   als IDE-Hinweis.
2. Bestehenden Code per `dotnet format` an das neue `.editorconfig`
   angeglichen (Feld `DefaultTimeout` → `_defaultTimeout` in
   `DatabasePermissionTests.cs`, einzige inhaltliche Korrektur).
3. `.github/workflows/ci.yml`: Restore, Build, `dotnet format
   --verify-no-changes`, Zeilenlängen-Check, Unit-Tests (Domain, Application,
   Api) — ohne `MyMusic.IntegrationTests`.
4. `docs/adr/0003-ci-gate-codequalitaet.md`.
5. `TASK.md` (neuer Block 0d) und `README.md` (Hinweis im Abschnitt „Prüfen").
6. `CLAUDE.md` Abschnitt 11 „Build und Verifikation" ergänzt — keine anderen
   Abschnitte verändert.

## Verifikation

1. `dotnet format MyMusic.slnx --verify-no-changes` — lokal grün (Exit-Code 0).
2. `dotnet build MyMusic.slnx` — fehlerfrei.
3. Zeilenlängen-Skript mit einer absichtlich zu langen Zeile probeweise
   getestet, danach wieder entfernt — bestätigt, dass der Check tatsächlich
   greift.
4. `dotnet test` für die drei Unit-Test-Projekte lokal — erwartungsgemäß ohne
   Tests (Projekte sind noch leer), kein Fehler.
5. Nach `git push`: GitHub-Actions-Lauf abgewartet und Ergebnis geprüft.

## Risiken und offene Punkte

- `MyMusic.IntegrationTests` bleibt in der CI ungeprüft — die
  Datenbank-Rechtetrennung wird weiterhin nur lokal (mit laufendem Docker)
  abgesichert.
- Kein Merge-Block bei rotem CI-Lauf, solange keine Branch-Protection-Rule
  eingerichtet ist.
- Projektspezifische Codierrichtlinien (Namensschemata, Feature-Kapselung,
  Kommentar-Ausnahmen) bleiben ausschließlich review-basiert abgesichert, kein
  technisches Gate dafür.
