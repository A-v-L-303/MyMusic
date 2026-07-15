# MyMusic — Anwendungs-Repository

Multi-User-Webanwendung zur Verwaltung privater Musiksammlungen mit Schwerpunkt
Schallplatten (Vinyl) und CDs: CRUD für Records, Tracks, Artists, Labels und Genres,
Dashboard, Volltext-Suche, Metadaten-Import über die Discogs-API und Zustandsbewertung
nach dem Goldmine-Standard. Das Projekt dient zugleich als Portfolioarbeit.

## Verbindliche Quellen

- **Planungs-Wiki** (fachlich verbindlich): `../../02 Wiki/MyMusic Wiki/wiki/index.md`
- **Arbeitsmodell**: `../../03 Ressourcen/leitfaden-zusammenarbeit-mit-claude-code.md`
- `TASK.md`: priorisierte Themenliste — **kein** direkter Implementierungsauftrag
- Bei Konflikten zwischen Wiki und Code: Abweichung melden, nicht stillschweigend auflösen

## Tech-Stack (entschieden — Änderungen nur nach ausdrücklicher Freigabe)

- **Backend**: C# / .NET 10, ASP.NET Core Minimal API, EF Core, PostgreSQL,
  FluentValidation, Serilog + Seq, xUnit, Swagger
- **Frontend**: Angular 21, Tailwind CSS 3
- **Infrastruktur**: Keycloak 26.5, .NET Aspire 13, Docker
- **Extern**: Discogs-API (serverseitig proxied)

## Architektur

- Onion-Architektur: Core (Domain), Application, Infrastructure, API + Angular-Frontend
- DDD, eigene CQRS-Implementierung (kein MediatR), generisches Repository-Pattern
- Details: Wiki `architektur/`; Feature-Checkliste: Wiki `architektur/application-layer.md`

## Codierrichtlinien (Kurzfassung)

Vollständig und verbindlich: Wiki `entwicklung/codierrichtlinien.md` und
`entwicklung/domain-regeln.md`.

- Code-Bezeichner Englisch, Fehlermeldungen und Log-Nachrichten Deutsch, keine Abkürzungen
- Domain: Properties `private set`/`private init`, Konstruktor `internal`,
  Erzeugung nur über `Create(...)`, `Update(...)` gibt neue Instanz zurück
- Handler hängen nur von `ExceptionManager` und `ResponseBuilder` ab;
  keine Exception-Konstruktion im Handler
- API: Minimal-API-Endpoints (keine Controller), Endpoint-Methoden `private static`,
  `.RequireAuthorization()` auf der Gruppe, kein `try-catch` in Endpoints
- Frontend: Standalone Components, `inject()`, Signals/`rxResource`, kein `any`

## Arbeitsmodell: Analyse → Freigabe → Umsetzung → Review

- Plan Mode verpflichtend für größere Änderungen
- Eine Freigabe gilt nur für den ausdrücklich beschriebenen Datei-, Paket- und
  Funktionsumfang; frühere Freigaben gelten nicht automatisch weiter
- Fakten, Annahmen und offene Fragen immer getrennt benennen
- Subagenten: `analyst` (Iststand), `planner` (Zerlegung, Prompts),
  `implementer` (freigegebene Umsetzung), `reviewer` (unabhängige Prüfung)
- Freigegebene Arbeits-Prompts unter `docs/prompts/` archivieren;
  Grundsatzentscheidungen als ADR unter `docs/adr/`

## Verifikation vor Abschluss

- `dotnet build`, `dotnet test`, `ng test --watch=false`, `git diff --check`
- Nicht vorhandene Tests, unprüfbare Annahmen und manuelle Prüfungen ausdrücklich
  im Bericht nennen — nicht nur erfolgreiche Tests melden

## Verbote (ohne ausdrückliche Freigabe)

- Keine Pakete hinzufügen, entfernen oder aktualisieren
- Keine Migrationen oder Datenbankschemata verändern
- Keine Secrets, Zertifikate oder Zugangsdaten erzeugen
- Keine nicht angeforderten Refactorings
- Kein `git push --force`, `git reset --hard`, `git clean`; keine Branches löschen
