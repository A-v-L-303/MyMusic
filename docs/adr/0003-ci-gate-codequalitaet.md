# ADR 0003 — CI-Gate für Codequalität (Scope und technische Grenzen)

**Status**: Angenommen
**Datum**: 2026-07-24
**Betrifft**: Repository-weit (`.editorconfig`, `.github/workflows/ci.yml`)

## Kontext

CLAUDE.md ist eine Instruktion, der in jeder Session erneut gefolgt werden muss —
das ist ein wiederkehrendes, im Voraus nicht prüfbares Risiko. Ein technisches
Gate verschiebt den Fehlerfall auf einen einmaligen, testbaren Zeitpunkt (die
Einrichtung selbst) und wirkt danach unabhängig davon, ob eine Regel in der
jeweiligen Session korrekt angewendet wurde. Ziel dieser Entscheidung ist es,
den generisch durch Tooling prüfbaren Teil der Codierrichtlinien
(`entwicklung/codierrichtlinien.md`) technisch durchzusetzen.

## Entscheidung

- GitHub-Actions-Workflow `.github/workflows/ci.yml`, Trigger `push`/
  `pull_request` auf `main`: Restore, Build der gesamten Solution, `dotnet
  format --verify-no-changes`, ein eigener Zeilenlängen-Check (max. 120
  Zeichen) sowie die drei Unit-Test-Projekte (`Domain.Tests`,
  `Application.Tests`, `Api.Tests`).
- `.editorconfig` im Repo-Root setzt file-scoped Namespaces
  (`csharp_style_namespace_declarations = file_scoped`) und die
  Namenskonvention für private Felder (`_camelCase`) durch; `const`-Felder sind
  davon ausdrücklich ausgenommen (PascalCase ist dafür idiomatisch, CLAUDE.md
  trifft dazu keine Aussage).
- Zeilenlänge wird **nicht** über `.editorconfig`/`dotnet format` geprüft,
  sondern über einen eigenen `grep`-Schritt in der CI-Pipeline: Das .NET-SDK
  hat keinen eingebauten Analyzer, der `max_line_length` hart durchsetzt;
  `.editorconfig` dokumentiert den Wert nur als IDE-Hinweis.
- `MyMusic.IntegrationTests` wird als Teil des Solution-Builds kompiliert,
  in der CI aber **nicht ausgeführt**. Der Test startet den vollen
  Aspire-AppHost inklusive Docker-Container und benötigt lokale User-Secrets
  (`Parameters:api-database-password`). Das im CI-Runner nachzubilden
  (Docker-in-Docker, Aspire-Workload, GitHub Secrets für Testpasswörter) ist
  ein eigener, deutlich größerer Schritt und bewusst nicht Teil dieser
  Entscheidung.
- Kein StyleCop.Analyzers oder Roslynator hinzugefügt. Die aktuell
  verbindlichen, generisch prüfbaren Regeln deckt das .NET-SDK bereits über
  `.editorconfig` + `dotnet format` ab, ohne neue Abhängigkeit. StyleCops
  Standardregelsatz verlangt zudem XML-Doc-Kommentare (SA1600-Reihe), was der
  Projektregel „grundsätzlich keine Kommentare" widerspricht und aufwändige
  Regel-Unterdrückung bräuchte.
- Kein Branch-Protection-Rule-Setup auf GitHub. Das Gate meldet den Status auf
  jedem Push/PR, blockiert aber keinen Merge — das ist eine
  Repository-Einstellung außerhalb des Codes und ein eigener, separat zu
  entscheidender Schritt.

## Begründung

Formatierung, file-scoped Namespaces, Zeilenlänge und die Feldnamenskonvention
sind mechanisch prüfbar und betreffen jede künftige Code-Zeile — dafür lohnt
sich ein Gate, das unabhängig von der jeweiligen Session wirkt. Projektspezifische
Regeln wie Backend-Namensschemata (`{Aktion}{Entität}Command`), die
Feature-Ordner-Kapselung oder die inhaltliche Beurteilung erlaubter
Kommentar-Ausnahmen sind mit generischem Tooling nicht sinnvoll prüfbar; ein
eigener Roslyn-Analyzer dafür wäre beim aktuellen, weitgehend leeren
Projektstand unverhältnismäßig. Diese Regeln bleiben Aufgabe des
`reviewer`-Subagenten bzw. der manuellen Prüfung.

## Konsequenzen

- Formatierungs-, Stil- und Zeilenlängenverstöße werden ab sofort mechanisch
  abgefangen, unabhängig davon, ob eine Claude-Session die jeweilige Regel im
  konkreten Moment „erinnert".
- Ein rotes CI-Ergebnis blockiert aktuell keinen Merge nach `main`, solange
  keine Branch-Protection-Rule eingerichtet ist — offener Folgepunkt.
- `MyMusic.IntegrationTests` bleibt in der CI ungeprüft; die
  Datenbank-Rechtetrennung wird weiterhin nur lokal (mit laufendem Docker)
  abgesichert. Das ist ein bekanntes, bewusst in Kauf genommenes Risiko, kein
  blinder Fleck.
- Projektspezifische Codierrichtlinien (Namensschemata, Feature-Kapselung,
  Kommentar-Ausnahmen, ResponseBuilder-Pattern) bleiben ausschließlich
  review-basiert abgesichert.
