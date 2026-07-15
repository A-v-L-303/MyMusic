# Offene Aufgaben

Stand: 2026-07-15
Branch: `main`

Diese Datei ist die operative Arbeitsliste für die nächsten Umsetzungsschritte.
Sie ersetzt nicht die fachliche Planung im Wiki
(`../../02 Wiki/MyMusic Wiki/wiki/`), sondern verdichtet die offenen Punkte aus
Feature-Roadmap und aktuellem Repository-Stand.

## Arbeitsregeln

- Jeder Block wird als eigener Arbeits-Prompt geplant (Plan Mode) und unter
  `docs/prompts/` archiviert.
- Jeder Block wird separat freigegeben, umgesetzt, geprüft und committet.
- Das Wiki ist die fachlich verbindliche Quelle; Abweichungen werden gemeldet.
- Keine Secrets, Zertifikate oder produktiven Daten ins Repository.
- Diese `TASK.md` wird nach jedem abgeschlossenen Block aktualisiert.

## Aktuell nicht umgesetzt

Das Repository enthält bisher ausschließlich die Arbeitsstruktur
(CLAUDE.md, Subagenten, Permissions, Ordnergerüst) — noch keinen Anwendungscode.
Offen ist damit der gesamte MVP-Umfang aus Phase 1 der Feature-Roadmap:

- Solution-Grundgerüst mit Onion-Layern, Aspire-Orchestrierung und
  Angular-Workspace.
- CQRS-Eigenframework, generisches Repository, ExceptionManager.
- CRUD-Slices für Genre, Country, Label, Artist, Record und Tracks.
- Zustandsbewertung nach Goldmine-Standard.
- Keycloak-Authentifizierung und Mandantentrennung.
- Discogs-Integration, Dashboard und Volltext-Suche.
- User Stories mit Akzeptanzkriterien (Lücke laut `offene-themen.md` im Wiki).

## 0. Fundament: Walking Skeleton

Status: offen
Priorität: hoch, erster Umsetzungsblock

Ziel:

- Lauffähiges technisches Grundgerüst ohne Fachlogik, das den gesamten Stack
  einmal validiert.

Aufgaben:

- Projektnamen der vier Onion-Layer vorschlagen und freigeben lassen
  (im Wiki noch nicht festgelegt; Aspire-Projektnamen sind festgelegt).
- .NET-10-Solution mit den Onion-Layern und Testprojekten anlegen.
- `MyMusic.AppHost` und `MyMusic.ServiceDefaults` einrichten
  (OpenTelemetry, Health Checks, Service Discovery).
- PostgreSQL, Seq und Keycloak als Aspire-Ressourcen einbinden;
  Boot-Reihenfolge per `WaitFor()` gemäß Wiki `architektur/aspire-orchestrierung.md`.
- PostgreSQL-Volume für die Entwicklung binden (Container sonst ephemer).
- `MyMusic.Migrator` als einmaligen Aspire-Job anlegen (DDL-Rechte nur hier).
- Keycloak-Realm/Client als JSON-Import unter `/keycloak/` versionieren;
  Admin-Credentials als Aspire-Parameter (`secret: true`) über User Secrets.
- CQRS-Eigenframework umsetzen (`IMediator`, Command-/Query-Schnittstellen,
  Validierungs-Pipeline-Decorator mit FluentValidation).
- Generisches `IRepository<T>` und EF-Core-Anbindung umsetzen.
- `ExceptionManager` und zentralen `IExceptionHandler` umsetzen.
- Angular-21-Workspace mit Tailwind CSS und Design-System-Anbindung aufsetzen;
  API-Basis-URL über `runtime-config.json`.
- Serilog + Seq im API-Projekt konfigurieren (Header-Ausschlüsse gemäß
  Sicherheitskonzept).

Bewusst später:

- Fachliche Endpunkte und UI-Features (eigene Slices ab Block 2).
- Production-Deployment und TLS-Strategie (im Wiki noch offen).

Abnahmekriterium:

- Alle Dienste starten über den AppHost in der dokumentierten Boot-Reihenfolge,
  der Migrator läuft durch und beendet sich, die leere API ist erreichbar und
  `dotnet build` sowie `dotnet test` laufen fehlerfrei.

## 1. Planung: User Stories und Akzeptanzkriterien

Status: offen
Priorität: hoch, jeweils vor dem zugehörigen Slice

Ziel:

- Die im Wiki (`offene-themen.md`) benannte Lücke schließen: strukturierte
  Szenarien mit messbaren Abnahmekriterien je MVP-Feature.

Aufgaben:

- Pro anstehendem Slice (beginnend mit Genre) User Stories mit
  Akzeptanzkriterien im Wiki ergänzen — nicht alles auf einmal.
- Die sechs Prüfkriterien der groben Testplanung als Grundlage nutzen.

Abnahmekriterium:

- Für den jeweils nächsten Slice existieren im Wiki Stories mit beobachtbaren,
  testbaren Kriterien, bevor der Arbeits-Prompt erstellt wird.

## 2. Slice: Genre

Status: offen
Priorität: hoch, erster fachlicher Durchstich

Ziel:

- Einfachster vertikaler Slice durch alle Schichten als Referenz für alle
  weiteren Entitäten.

Aufgaben:

- Domain-Entität `Genre` nach den Domain-Regeln.
- Commands (Create, Update, Delete), Queries (GetAll, GetById), Validatoren,
  Response-DTO und ResponseBuilder gemäß Feature-Checkliste.
- Minimal-API-Endpoints (`GenreEndpoints`) mit `.RequireAuthorization()`.
- Unit Tests: Handler (inkl. Mandantentrennung), Validierung, ResponseBuilder,
  Filter nach Name.
- Angular-Feature `genres/`: Tabellenansicht, Filterung nach Name,
  Add/Edit als Modal.

Abnahmekriterium:

- Genres lassen sich anlegen, anzeigen, filtern, bearbeiten und löschen;
  fremde Benutzerdaten sind nicht sichtbar; Tests decken Happy Path,
  Validierung und unbekannte IDs ab.

## 3. Slice: Country

Status: offen
Priorität: mittel, vor Label benötigt

Ziel:

- Herkunftsländer als Stammdaten für Labels (Wiki `domain/country.md`).

Abnahmekriterium:

- Länder stehen bei der Label-Pflege zur Auswahl.

## 4. Slice: Label

Status: offen
Priorität: mittel

Aufgaben:

- CRUD, Tabellenansicht mit Paginierung, Filterung nach Name und Land,
  Sortierung nach Name.

Abnahmekriterium:

- Labels sind vollständig verwaltbar; Filter und Sortierung entsprechen der
  Feature-Roadmap.

## 5. Slice: Artist

Status: offen
Priorität: mittel

Aufgaben:

- CRUD, Tabellenansicht mit Paginierung, Filterung nach Name und Label,
  Sortierung nach Name.

Abnahmekriterium:

- Artists sind vollständig verwaltbar; Filter und Sortierung entsprechen der
  Feature-Roadmap.

## 6. Slice: Record und Tracks

Status: offen
Priorität: hoch, fachlicher Kern

Aufgaben:

- Record-CRUD mit Card-Ansicht, Paginierung, Filterung (Name, Artist, Label,
  Erscheinungsjahr, Land) und Sortierung (Name, Erscheinungsjahr, Format).
- Album-Cover hochladen und Vorschau anzeigen.
- Record-Detailansicht mit Track-Liste.
- Track-CRUD in der Detailansicht (Unteransicht, kein eigener Reiter).
- Zustandsbewertung nach Goldmine-Standard (Datenmodell-Erweiterung,
  Wiki `domain/zustandsbewertung.md`).

Abnahmekriterium:

- Ein Record mit Tracks und Zustandsbewertung ist vollständig anleg-, filter-,
  sortier-, bearbeit- und löschbar.

## 7. Authentifizierung und Mandantentrennung

Status: offen
Priorität: hoch; Grundlagen entstehen bereits im Walking Skeleton

Ziel:

- Vollständige Umsetzung des Sicherheitskonzepts
  (Wiki `sicherheit/sicherheitskonzept.md`).

Aufgaben:

- Angular-Login-Flow (Authorization Code + PKCE) inkl. AuthGuard und
  HTTP-Interceptor.
- JWT-Validierung, Rollen (`User`, `Admin`), Ownership-Prüfung in Handlern
  (404 statt 403).
- Rate Limiting (100 req/min pro Benutzer), CORS-Policy per Environment, CSP.
- Admin-Bereich: Benutzer inkl. aller Daten löschen (`/admin`, nur Rolle Admin).
- Sicherheitstests: nicht authentifiziert, fremde Daten, unbekannte IDs.

Offene Entscheidung:

- Wie viel Authentifizierung bereits mit dem Walking Skeleton kommt und wie viel
  in diesem Block — bei Planung von Block 0 festlegen.

Abnahmekriterium:

- Ohne Login ist kein fachlicher Endpunkt erreichbar; Benutzer sehen
  ausschließlich eigene Daten; der Admin kann Benutzer löschen.

## 8. Discogs-Integration

Status: offen
Priorität: mittel

Aufgaben:

- Serverseitiger Proxy `/discogs/search` (Wiki `tech-stack/discogs-api.md`).
- Metadaten-Suche beim Anlegen eines Records; Treffer als Vorausfüllung,
  manuell editierbar.
- Fehlerdarstellung gemäß Fehlerkonzept (Modal mit Hinweis auf manuelle Eingabe).

Abnahmekriterium:

- Ein Record kann mit Discogs-Vorausfüllung angelegt werden; bei
  Discogs-Ausfall bleibt die manuelle Anlage uneingeschränkt möglich.

## 9. Dashboard

Status: offen
Priorität: mittel bis niedrig

Aufgaben:

- Kennzahlen: Anzahl Records je Format, Top Artists, Top Labels,
  Verteilung nach Erscheinungsjahr (Komponenten gemäß
  Wiki `architektur/angular-projektstruktur.md`).

Abnahmekriterium:

- Das Dashboard zeigt die vier Kennzahlen für die eigene Sammlung korrekt an.

## 10. Volltext-Suche

Status: offen
Priorität: mittel bis niedrig

Aufgaben:

- Globale Suche über Records, Artists und Labels in kombinierter Ansicht
  (`/search?q=...`).

Abnahmekriterium:

- Das MVP-Szenario der Feature-Roadmap ist damit vollständig durchspielbar.

## Dokumentations-Nacharbeit

Status: laufend
Priorität: niedrig, aber vor jedem größeren Commit prüfen

Aufgaben:

- `README.md` und diese `TASK.md` nach jedem abgeschlossenen Block aktualisieren.
- Grundsatzentscheidungen (z. B. Projektnamen der Layer, Production-TLS,
  Production-Secrets) als ADR unter `docs/adr/` festhalten.
- Wiki bei fachlichen Änderungen aktualisieren bzw. Abweichungen melden.

Abnahmekriterium:

- Doku und tatsächlicher Codezustand widersprechen sich nicht.
