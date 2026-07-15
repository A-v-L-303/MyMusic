# MyMusic

Multi-User-Webanwendung zur Verwaltung privater Musiksammlungen mit Schwerpunkt
Schallplatten (Vinyl) und CDs.

**Status**: In Einrichtung — die fachliche Planung liegt im Projekt-Wiki
(außerhalb dieses Repositories), der Anwendungscode entsteht schrittweise.

## Kernfunktionen (geplant)

- CRUD für Records, Tracks, Artists, Labels und Genres
- Dashboard mit Sammlungsstatistiken
- Volltext-Suche
- Metadaten-Import über die Discogs-API
- Zustandsbewertung nach dem Goldmine-Standard
- Multi-User-Betrieb mit Keycloak-Authentifizierung und Mandantentrennung

## Tech-Stack

| Bereich | Technologie |
|---|---|
| Backend | C# / .NET 10, ASP.NET Core Minimal API, EF Core, PostgreSQL |
| Frontend | Angular 21, Tailwind CSS 3 |
| Infrastruktur | Keycloak 26.5, .NET Aspire 13, Docker |
| Qualität | xUnit, FluentValidation, Serilog + Seq, Swagger |

Architektur: Onion-Architektur, DDD, eigene CQRS-Implementierung (ohne MediatR),
generisches Repository-Pattern.

## Repository-Struktur

```
.claude/       Arbeitsmodell für Claude Code (Agenten, Permissions)
docs/adr/      Architekturentscheidungen (ADRs)
docs/prompts/  Archiv der freigegebenen Arbeits-Prompts
src/           Anwendungscode (Backend und Frontend)
tests/         Automatisierte Tests
CLAUDE.md      Dauerhafte Projekt- und Arbeitsregeln
TASK.md        Priorisierte Liste möglicher nächster Aufgaben
```
