# Offene Aufgaben

Stand: 2026-07-24 (nach Abschluss von Block 0a und 0d)
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

Block 0a (Solution- und Aspire-Fundament) ist abgeschlossen. Offen aus dem
MVP-Umfang der Phase 1:

- CQRS-Eigenframework, generisches Repository, ExceptionManager (Block 0b).
- Angular-Workspace (Block 0c).
- CRUD-Slices für Genre, Country, Label, Artist, Record und Tracks.
- Zustandsbewertung nach Goldmine-Standard.
- Keycloak-Authentifizierung im Code und Mandantentrennung.
- Discogs-Integration, Dashboard und Volltext-Suche.
- User Stories mit Akzeptanzkriterien (Lücke laut `offene-themen.md` im Wiki).

## 0. Fundament: Walking Skeleton

Block 0 wurde in drei einzeln prüfbare Teilblöcke zerlegt, weil das
Abnahmekriterium des Gesamtblocks erst ganz am Ende messbar gewesen wäre.

### 0a. Solution- und Aspire-Fundament

Status: **abgeschlossen** (2026-07-15)
Arbeits-Prompt: `docs/prompts/2026-07-15-block-0a-fundament.md`

Umgesetzt:

- .NET-10-Solution (`MyMusic.slnx`) mit den vier Onion-Layern `MyMusic.Domain`,
  `MyMusic.Application`, `MyMusic.Infrastructure`, `MyMusic.Api` (ADR 0001).
- Testprojekte je Layer plus `MyMusic.IntegrationTests`.
- `MyMusic.AppHost` und `MyMusic.ServiceDefaults` (Aspire 13.4.6).
- PostgreSQL, Seq und Keycloak 26.5 als Aspire-Ressourcen mit Datenvolumes.
- Boot-Reihenfolge: Migrator → `WaitFor(PostgreSQL)`, API →
  `WaitForCompletion(Migrator)` + `WaitFor(Keycloak)`.
- `MyMusic.Migrator` als einmaliger Job mit DDL-Rechten.
- Keycloak-Realm als JSON-Import unter `/keycloak/`; Admin-Credentials als
  Aspire-Parameter über User Secrets.
- DB-Berechtigungskonzept: Rolle `mymusic_api` mit reinen DML-Rechten, per
  Integrationstest abgesichert.
- Serilog mit Console- und Seq-Sink (ADR 0002).

Bewusst nicht Teil von 0a:

- JWT-Verdrahtung im Code und Auth-Smoke-Test (0b).
- Erste echte EF-Migration — separat freizugeben, gehört zum Genre-Slice.

### 0b. CQRS, Repository und Auth-Smoke-Test

Status: offen
Priorität: hoch, nächster Umsetzungsblock

Aufgaben:

- CQRS-Eigenframework (`IMediator`, Command-/Query-Schnittstellen,
  Validierungs-Pipeline-Decorator mit FluentValidation).
- Generisches `IRepository<T>` und EF-Core-Anbindung.
- `ExceptionManager` und zentraler `IExceptionHandler`.
- `AddAuthentication().AddJwtBearer()` gegen die Keycloak-Authority.
- Ein einzelner geschützter Smoke-Test-Endpunkt: ohne Token 401, mit Token 200,
  als Integrationstest gegen den laufenden Keycloak.

Abnahmekriterium:

- Unit Tests grün; die Kette Keycloak → API ist einmal nachgewiesen.

### 0c. Angular-Workspace

Status: offen
Priorität: mittel

Aufgaben:

- Angular-22-Workspace mit Tailwind CSS und Design-System-Anbindung.
- API-Basis-URL über `runtime-config.json` (Laufzeit, nicht Build-Zeit).
- Einbindung in den AppHost — **Achtung**: `AddNpmApp()` existiert in Aspire 13
  nicht mehr, Ersatz ist `AddJavaScriptApp()` (Wiki-Korrektur ausstehend).

Abnahmekriterium:

- Das Frontend startet über den AppHost.

### 0d. CI-Gate für Codequalität

Status: **abgeschlossen** (2026-07-24)
Arbeits-Prompt: `docs/prompts/2026-07-24-ci-gate-codequalitaet.md`

Umgesetzt:

- `.editorconfig` im Repo-Root: file-scoped Namespaces, Naming-Regel für
  private Felder (`_camelCase`, Konstanten ausgenommen).
- `.github/workflows/ci.yml`: Restore, Build, `dotnet format
  --verify-no-changes`, Zeilenlängen-Check (max. 120 Zeichen), Unit-Tests
  (Domain, Application, Api) bei jedem Push/PR auf `main`.
- ADR `docs/adr/0003-ci-gate-codequalitaet.md`.

Bewusst nicht Teil von 0d:

- `MyMusic.IntegrationTests` läuft nicht in der CI (braucht Docker +
  Aspire-Orchestrierung + Secrets — eigener, größerer Schritt).
- Kein Branch-Protection-Rule-Setup (Repository-Einstellung, eigener Schritt).
- Kein StyleCop/Roslynator; projektspezifische Regeln (Namensschemata,
  Feature-Kapselung, Kommentar-Ausnahmen) bleiben Aufgabe des
  `reviewer`-Subagenten.

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
