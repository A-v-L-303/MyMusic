# TASK.md — Priorisierte Arbeitsliste

> Grobe Liste möglicher nächster Aufgaben (Quelle: Wiki `projekt/feature-roadmap.md`).
> **Kein direkter Implementierungsauftrag** — vor jeder Umsetzung wird der Iststand
> geprüft und ein konkreter Arbeits-Prompt im Plan Mode freigegeben.

## 1. Fundament (Walking Skeleton, ohne Fachlogik)

- .NET-10-Solution mit Onion-Schichten anlegen (Core, Application, Infrastructure, API)
- .NET-Aspire-AppHost mit Orchestrierung (PostgreSQL, Keycloak, Seq) einrichten
- Angular-21-Workspace mit Tailwind CSS und Design-System-Anbindung aufsetzen
- CQRS-Eigenframework umsetzen (`IMediator`, Command-/Query-Schnittstellen, Pipeline)
- Generisches Repository und EF-Core-Anbindung an PostgreSQL umsetzen
- Fehler- und Ausnahmekonzept umsetzen (`ExceptionManager`, `IExceptionHandler`)

## 2. Planung nachziehen (im Wiki, jeweils pro anstehendem Slice)

- User Stories mit Akzeptanzkriterien je MVP-Feature ergänzen
  (offen laut `offene-themen.md` im Wiki)

## 3. MVP — vertikale Slices (Phase 1 der Feature-Roadmap)

- Genre: CRUD, Tabellenansicht, Filterung nach Name (einfachster Durchstich)
- Country: Stammdaten für Labels
- Label: CRUD, Tabellenansicht mit Paginierung, Filterung nach Name und Land
- Artist: CRUD, Tabellenansicht mit Paginierung, Filterung nach Name und Label
- Record: CRUD, Cover-Upload mit Vorschau, Card-Ansicht mit Paginierung,
  Filterung und Sortierung, Detailansicht mit Track-Liste
- Tracks: CRUD in der Record-Detailansicht
- Zustandsbewertung nach Goldmine-Standard (Datenmodell-Erweiterung)
- Keycloak-Anbindung (Authorization Code + PKCE) und Mandantentrennung
- Discogs-Integration (serverseitiger Proxy `/discogs/search`, Vorausfüllung beim Anlegen)
- Dashboard: Records je Format, Top Artists, Top Labels, Verteilung nach Erscheinungsjahr
- Volltext-Suche über Records, Artists und Labels
