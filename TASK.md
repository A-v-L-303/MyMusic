# Offene Aufgaben

Stand: 2026-07-26 (nach Abschluss von Block 0a, 0b und 0d)
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

Block 0a, 0b und 0d sind abgeschlossen. Offen aus dem MVP-Umfang der Phase 1:

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

Status: **abgeschlossen** (2026-07-26)
Arbeits-Prompt: `docs/prompts/2026-07-26-block-0b-cqrs-repository-auth.md`

Umgesetzt:

- CQRS-Eigenframework (`IMediator`/`Mediator`, `ICommand<TResponse>`/
  `IQuery<TResponse>`, `ICommandHandler<,>`/`IQueryHandler<,>`,
  `CommandValidationDecorator` mit FluentValidation) in
  `MyMusic.Application/Common/CQRS/`; Handler-Registrierung per Assembly-Scan
  über `AddApplication()`.
- Generisches `IRepository<T>` (`MyMusic.Domain/Contracts/Repository/`) und
  EF-Core-Implementierung `Repository<T>`
  (`MyMusic.Infrastructure/Persistence/Repositories/`); noch nicht in die
  API-DI verdrahtet, da 0b keine Entität hat, die ihn braucht (folgt mit dem
  Genre-Slice).
- `ExceptionManager` (`ValidationException`, `NotFoundException`,
  `ConflictException`) und zentraler `GlobalExceptionHandler`
  (`IExceptionHandler`) in `MyMusic.Api`, mappt auf HTTP 400/404/409/500.
- `AddAuthentication().AddJwtBearer()` gegen die Keycloak-Authority
  (`ValidAudience = "account"`, `MapInboundClaims = false` — ADR 0004) und
  `ICurrentUserService`/`CurrentUserService` (liest `sub`-Claim).
- Smoke-Test-Endpunkt `GET /api/me` (`GetCurrentUserQuery` →
  `GetCurrentUserQueryHandler` → `CurrentUserResponseBuilder`),
  `.RequireAuthorization()`.
- Neues Testprojekt `MyMusic.Infrastructure.Tests` für den generischen
  Repository-Unit-Test (gemockter `DbContext`/`DbSet`).
- Integrationstest `tests/MyMusic.IntegrationTests/MeEndpointTests.cs`: `/api/me`
  ohne Token → 401, mit echtem Keycloak-Token → 200; dafür dedizierter
  Test-Client `mymusic-integration-tests` im Realm-Import (ADR 0005).

Nachträge nach unabhängiger Review:

- `CurrentUserResponseBuilder` ergänzt — der Handler baute das Response-DTO
  zunächst direkt, was der ausnahmslosen Regel „Handler hängen nur von
  ExceptionManager und ResponseBuilder ab" (CLAUDE.md §4.3/§9) widersprach.
- Paket-Tabelle im Arbeits-Prompt um `Microsoft.Extensions.DependencyInjection.Abstractions`
  (Application) und `Microsoft.Extensions.DependencyInjection` (Application.Tests)
  ergänzt — beide waren im Diff enthalten, aber nicht dokumentiert.
- `RepositoryTests`: negativer Testfall `GetByIdAsync` → `null` bei unbekannter
  Id ergänzt.
- `tests/MyMusic.IntegrationTests/AssemblyInfo.cs`: Testparallelität für die
  Assembly deaktiviert (`CollectionBehavior(DisableTestParallelization = true)`) —
  behebt einen im Review beobachteten Timeout bei gemeinsamer Ausführung
  beider Integrationstests (zwei parallele Aspire-Stacks konkurrierten um
  Ressourcen). Mit gemeinsamem Lauf erneut verifiziert (2/2 grün).

Abnahmekriterium erfüllt:

- Unit Tests grün (Domain/Application/Api/Infrastructure); die Kette
  Keycloak → API ist per Integrationstest gegen einen echten
  Keycloak-Container nachgewiesen (lokal mit Docker ausgeführt, isoliert und
  gemeinsam mit dem bestehenden Integrationstest).

Bewusst nicht Teil von 0b:

- Rollenkonzept, Ownership-Prüfung, Rate Limiting, CORS-Policy, CSP
  (Abschnitt 7 — dafür fehlen die Entitäten, an denen Ownership überhaupt
  geprüft werden könnte).
- DI-Verdrahtung von `IRepository<T>`/`MyMusicDbContext` in `MyMusic.Api` und
  die reale Prüfung des Repositorys gegen PostgreSQL (folgt mit dem
  Genre-Slice, Block 2).

Bekannte Lücke:

- `Repository<T>.GetAllAsync` ist per Unit Test nicht absicherbar (EF Cores
  `ToListAsync()` verlangt `IAsyncEnumerable<T>` auf dem `DbSet`, ein reiner
  NSubstitute-Mock implementiert das in der verwendeten EF-Core-Version nicht
  — empirisch geprüft). Realer Nachweis folgt mit dem
  Genre-Slice-Integrationstest gegen PostgreSQL.

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

Status: teilweise offen
Priorität: hoch; JWT-Validierung ist bereits im Walking Skeleton entstanden

Ziel:

- Vollständige Umsetzung des Sicherheitskonzepts
  (Wiki `sicherheit/sicherheitskonzept.md`).

Bereits umgesetzt (Block 0b, siehe oben):

- `AddAuthentication().AddJwtBearer()` gegen die Keycloak-Authority,
  `ICurrentUserService` liest den `sub`-Claim, einmal nachgewiesen per
  Integrationstest (`/api/me`).

Aufgaben (noch offen):

- Angular-Login-Flow (Authorization Code + PKCE) inkl. AuthGuard und
  HTTP-Interceptor.
- Rollen (`User`, `Admin`), Ownership-Prüfung in Handlern (404 statt 403) —
  setzt Entitäten voraus, entsteht mit dem jeweiligen Slice.
- Rate Limiting (100 req/min pro Benutzer), CORS-Policy per Environment, CSP.
- Admin-Bereich: Benutzer inkl. aller Daten löschen (`/admin`, nur Rolle Admin).
- Sicherheitstests: nicht authentifiziert, fremde Daten, unbekannte IDs.

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
