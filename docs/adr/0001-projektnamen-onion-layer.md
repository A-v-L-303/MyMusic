# ADR 0001 — Projektnamen der Onion-Layer und Zuschnitt der Testprojekte

**Status**: Angenommen
**Datum**: 2026-07-15
**Betrifft**: Solution-Struktur, alle Backend-Projekte

## Kontext

Das Wiki legt die Aspire-Projektnamen fest (`MyMusic.AppHost`, `MyMusic.ServiceDefaults`,
`MyMusic.Migrator`), lässt die Namen der vier Onion-Layer aber offen
(`architektur/architektur-übersicht.md` spricht nur von „Core (Domain) Layer",
„Application Layer", „Infrastructure Layer" und „API Layer"). Ohne festgelegte Namen konnte
die Solution nicht angelegt werden.

Ebenso offen war der Zuschnitt der Testprojekte. Die Teststrategie (CLAUDE.md §10) trennt
Unit Tests von Integrationstests; letztere brauchen laufende Container und sind entsprechend
langsam.

## Entscheidung

Die vier Layer heißen:

| Layer | Projekt |
|---|---|
| Core (Domain) | `MyMusic.Domain` |
| Application | `MyMusic.Application` |
| Infrastructure | `MyMusic.Infrastructure` |
| API | `MyMusic.Api` |

Die Testprojekte werden je Layer geschnitten, die container-abhängigen Tests separat:

- `MyMusic.Domain.Tests`
- `MyMusic.Application.Tests`
- `MyMusic.Api.Tests`
- `MyMusic.IntegrationTests`

## Begründung

- Das Präfix `MyMusic` ist laut CLAUDE.md §1 für alle Projekte, Namespaces und Assemblies
  verbindlich und verhindert Kollisionen generischer Assembly-Namen wie `Domain` mit
  NuGet-Paketen.
- `Domain` statt `Core`, weil CLAUDE.md §4.2 bereits konkret auf
  `Domain/Contracts/Repository/` verweist — der Name war dort faktisch schon gesetzt.
- `Api` statt `API`, weil die .NET-Framework-Design-Guidelines Akronyme ab drei Buchstaben in
  PascalCase schreiben (`Api`, `Xml`, `Html`) und die ASP.NET-Core-Templates dem folgen. Das
  Verbot „keine Abkürzungen" aus den Codierrichtlinien zielt auf gekürzte Wörter wie `ArtId`
  statt `ArtistId`, nicht auf etablierte Akronyme.
- Die Trennung der Integrationstests erlaubt es, den langsamen, container-abhängigen Teil in
  CI getrennt auszuführen.

## Konsequenzen

- Die Solution nutzt das `.slnx`-Format, das .NET 10 standardmäßig erzeugt. Ältere
  Tooling-Versionen (Visual Studio vor 17.13) können es nicht öffnen.
- `MyMusic.Domain.Tests` und `MyMusic.Application.Tests` sind zunächst leer, da die
  zugehörigen Layer erst mit den fachlichen Slices Code bekommen.
