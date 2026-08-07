# MyMusic

Eine Multi-User-Webanwendung zur Verwaltung von Schallplatten- und CD-Sammlungen.

---

## Über das Projekt

MyMusic soll die strukturierte Verwaltung privater Musiksammlungen ermöglichen. Schwerpunkt sind physische Tonträger — **Schallplatten (Vinyl)** und **CDs**.

Benutzer können ihre Sammlung durchsuchen, filtern und sortieren, Records mit vollständiger Track-Liste pflegen, Metadaten automatisch über die Discogs-API abrufen und den physischen Zustand nach dem Goldmine-Standard bewerten.

Das Projekt dient gleichzeitig als **Portfolioarbeit** und demonstriert den Einsatz moderner .NET- und Angular-Technologien in einer produktionsreifen Webanwendung.

Die vollständige Projektdokumentation ist im [Wiki](https://github.com/A-v-L-303/MyMusic/wiki) verfügbar.

---

## Geplante Features

- **Records** — CRUD, Album-Cover-Upload, Card-Ansicht mit Paginierung, Filter und Sortierung, Detailansicht mit Tracks
- **Tracks** — Verwaltung direkt in der Record-Detailansicht
- **Artists** — CRUD, Tabellenansicht, Filter und Sortierung
- **Labels** — CRUD, Tabellenansicht, Filter und Sortierung
- **Genres** — CRUD, Tabellenansicht
- **Authentifizierung** — Anmeldung via Keycloak (Authorization Code + PKCE), Mandantentrennung
- **Dashboard** — Anzahl Records je Format, Top Artists, Top Labels, Verteilung nach Erscheinungsjahr
- **Volltext-Suche** — Globale Suche über Records, Artists und Labels
- **Discogs-Integration** — Metadaten-Suche beim Anlegen eines Records, manuell editierbar
- **Zustandsbewertung** — Physischer Zustand nach Goldmine-Standard (Mint, VG+, VG, …)

---

## Geplanter Tech-Stack

### Backend

| Technologie | Version | Zweck |
|---|---|---|
| .NET / C# | 10 | Laufzeit und Sprache |
| ASP.NET Core (Minimal API) | 10 | HTTP-Endpunkte |
| Entity Framework Core | — | ORM / Datenzugriff |
| PostgreSQL | — | Relationale Datenbank |
| Keycloak | 26.5 | Identitäts- und Zugriffsmanagement |
| FluentValidation | — | Input-Validierung |
| Serilog + Seq | — | Strukturiertes Logging |
| xUnit | — | Tests |
| Swagger / OpenAPI | — | API-Dokumentation |

### Frontend

| Technologie | Version | Zweck |
|---|---|---|
| Angular | 22 | SPA-Framework |
| Tailwind CSS | 3 | Styling |

### Infrastruktur

| Technologie | Zweck |
|---|---|
| .NET Aspire 13 | Orchestrierung verteilter Dienste |
| Docker | Containerisierung |

---

## geplante Architektur

Das Backend folgt der **Onion-Architektur** in Kombination mit **Domain Driven Design** und einer eigens implementierten **CQRS**-Lösung (ohne MediatR). Der Datenzugriff erfolgt über ein generisches **Repository-Pattern**.

```
┌─────────────────────────────────────────┐
│                API Layer                │  ASP.NET Core Minimal API
├─────────────────────────────────────────┤
│           Application Layer             │  Commands, Queries, Services
├─────────────────────────────────────────┤
│          Infrastructure Layer           │  EF Core, PostgreSQL, Keycloak
├─────────────────────────────────────────┤
│           Core (Domain) Layer           │  Entities, Value Objects, Aggregates
└─────────────────────────────────────────┘

         Angular 22 Frontend (SPA)
```

---

## Repository-Struktur

```
.claude/         Arbeitsmodell für Claude Code (Agenten, Permissions)
.github/         GitHub-Actions-CI (Restore, Build, Format, Tests)
docs/adr/        Architekturentscheidungen (ADRs)
docs/prompts/    Archiv der freigegebenen Arbeits-Prompts
keycloak/        Realm-Definition als JSON-Import (versioniert)
src/             Anwendungscode (Backend und Frontend)
tests/           Automatisierte Tests
.editorconfig    Formatierungs- und Namenskonventionen (CI-Gate)
CLAUDE.md        Dauerhafte Projekt- und Arbeitsregeln
MyMusic.slnx     Solution (neues XML-Format, .NET 10)
NuGet.config     Paketquellen (nur nuget.org, für reproduzierbare Builds)
TASK.md          Operative Arbeitsliste der nächsten Umsetzungsschritte
```

---

## Lokale Entwicklung

### Voraussetzungen

| Werkzeug | Version | Hinweis |
|---|---|---|
| .NET SDK | 10.0 | |
| Docker | laufender Daemon | für PostgreSQL, Seq und Keycloak |
| Aspire CLI | 13.4.x | `dotnet tool install -g Aspire.Cli` (optional, `dotnet run` genügt) |
| Node.js | 22 | erst für den Angular-Workspace nötig |

### Secrets einrichten

Der AppHost erwartet drei Parameter als User Secrets. Sie liegen außerhalb des Repositories
und gehören nie ins Git:

```powershell
cd src/MyMusic.AppHost
dotnet user-secrets set "Parameters:postgres-password" "<wert>"
dotnet user-secrets set "Parameters:api-database-password" "<wert>"
dotnet user-secrets set "Parameters:keycloak-admin-password" "<wert>"
```

### Starten

```powershell
dotnet run --project src/MyMusic.AppHost
```

Der AppHost startet PostgreSQL, Seq und Keycloak, lässt den Migrator einmalig laufen und
startet danach die API. Die Adresse des Aspire-Dashboards steht in der Konsolenausgabe.

### Wichtig: Datenbank-Init-Skript läuft nur einmal

`src/MyMusic.AppHost/initdb/01-create-api-role.sh` legt die Datenbank und die eingeschränkte
Rolle `mymusic_api` an. Das PostgreSQL-Image führt Init-Skripte **ausschließlich bei leerem
Datenverzeichnis** aus. Da die Entwicklungsdaten in einem Volume liegen, läuft das Skript
genau einmal — nach Änderungen daran greift es erst, wenn das Volume verworfen wird:

```powershell
docker volume rm mymusic-postgres-data
```

Das ist die häufigste Ursache dafür, dass Änderungen an den Datenbankrechten scheinbar
wirkungslos bleiben.

### Berechtigungskonzept der Datenbank

Der Migrator besitzt DDL- und DML-Rechte, die API ausschließlich DML — Anwendungscode kann
das Schema also nicht verändern. Die API bekommt deshalb **nicht** den automatisch von Aspire
injizierten Connection String, sondern einen eigenen mit der Rolle `mymusic_api`. Abgesichert
ist das durch `tests/MyMusic.IntegrationTests/DatabasePermissionTests.cs`.

### CQRS, Repository, Auth-Smoke-Test (Block 0b)

`MyMusic.Application` enthält seit Block 0b ein eigenes CQRS-Grundgerüst
(`IMediator`, `ICommand<T>`/`IQuery<T>`, FluentValidation-Decorator), einen
`ExceptionManager` mit zentralem `GlobalExceptionHandler` in `MyMusic.Api` sowie
ein generisches `IRepository<T>`/`Repository<T>` (erste konkrete Entität und
DI-Verdrahtung in `MyMusic.Api` seit dem Genre-Slice, Block 2). Als einziger geschützter Endpunkt existiert `GET /api/me`
(`.RequireAuthorization()`), der ausschließlich die `userId` aus dem
`sub`-Claim des Zugriffstokens zurückgibt — Nachweis der Kette
Keycloak → JWT → Mediator.

Für einen manuellen Testaufruf gegen den laufenden AppHost wird ein Access
Token benötigt. Der Realm-Import enthält dafür den Client
`mymusic-integration-tests` (Resource-Owner-Password-Grant, siehe
`docs/adr/0005-keycloak-integrationstest-client.md`):

```powershell
$token = (Invoke-RestMethod -Method Post `
  -Uri "http://localhost:8080/realms/mymusic/protocol/openid-connect/token" `
  -Body @{ grant_type = "password"; client_id = "mymusic-integration-tests"; username = "<vorhandener-testbenutzer>"; password = "<passwort>" }).access_token

Invoke-RestMethod -Uri "http://localhost:<api-port>/api/me" -Headers @{ Authorization = "Bearer $token" }
```

Ohne Token liefert `/api/me` HTTP 401; mit gültigem Token HTTP 200 mit der
eigenen `userId`. Der Keycloak-Port ist seit Block 0a fest auf `8080`; der
API-Port steht in der Aspire-Dashboard-Ausgabe.

### Genre-Slice (Block 2)

Erster fachlicher Durchstich durch alle vier Layer inklusive Datenbank.
`GenreEndpoints` (`/api/genres`, `.RequireAuthorization()`) bietet CRUD plus
paginierte, nach Name filterbare und sortierte Liste:

| Methode | Route | Beschreibung |
|---|---|---|
| GET | `/api/genres?page=&pageSize=&name=` | Paginierte Liste, sortiert nach Name |
| GET | `/api/genres/{id}` | Einzelnes Genre |
| POST | `/api/genres` | Genre anlegen (`{ "name": "..." }`) |
| PUT | `/api/genres/{id}` | Genre umbenennen |
| DELETE | `/api/genres/{id}` | Genre löschen |

Die `userId` kommt in jedem Fall aus dem `sub`-Claim des Tokens (Token-Bezug
siehe oben) — nie aus dem Request. Ein doppelter Name innerhalb der eigenen
Sammlung liefert HTTP 409, eine fremde oder unbekannte Id HTTP 404 (nicht
403). Das Angular-Feature `genres/` folgt erst mit Block 0c
(Angular-Workspace), siehe `TASK.md`.

### Country-Slice (Block 3)

Reine, read-only Referenztabelle ohne CRUD und ohne Mandantenbezug —
Herkunftsländer für die künftige Label-Pflege. `CountryEndpoints`
(`.RequireAuthorization()`) bietet nur einen Endpoint:

| Methode | Route | Beschreibung |
|---|---|---|
| GET | `/api/countries` | Vollständige, alphabetisch sortierte Länderliste (238 Einträge) |

Anders als bei Genre gibt es kein `userId` in der Query (keine `user_id`-Spalte,
siehe `country`-Tabelle) und kein `Update()` auf der Entität (Länder werden nie
mutiert). Die Referenzdaten werden einmalig per Migration geseedet. Kein
Angular-Feature vorgesehen (keine CRUD-Maske, siehe `TASK.md`).

### Label-Slice (Block 4)

Erster Slice mit einer Fremdschlüsselbeziehung zu einer anderen
Stammdaten-Entität (`country_id → country.id`, `ON DELETE RESTRICT`).
`LabelEndpoints` (`/api/labels`, `.RequireAuthorization()`) bietet CRUD plus
paginierte, nach Name und Land filterbare und nach Name sortierte Liste:

| Methode | Route | Beschreibung |
|---|---|---|
| GET | `/api/labels?page=&pageSize=&name=&countryId=` | Paginierte Liste, sortiert nach Name |
| GET | `/api/labels/{id}` | Einzelnes Label |
| POST | `/api/labels` | Label anlegen (`{ "name", "countryId", "information" }`) |
| PUT | `/api/labels/{id}` | Label bearbeiten |
| DELETE | `/api/labels/{id}` | Label löschen |

Die Response löst den Ländernamen serverseitig auf (`CountryName` neben
`CountryId`). Eine nicht existierende `countryId` liefert HTTP 400 (nicht
404), ein doppelter Name innerhalb der eigenen Sammlung HTTP 409, eine fremde
oder unbekannte Id HTTP 404 (nicht 403) — analog Genre. Das Angular-Feature
`labels/` folgt erst mit Block 0c (Angular-Workspace), siehe `TASK.md`.

### Artist-Slice (Block 5)

Strukturell nahezu identisch zu Genre (kein Fremdschlüssel, kein
Zusatzfeld) — nur mit anderer Namenslänge und breiterem Zeichensatz.
`ArtistEndpoints` (`/api/artists`, `.RequireAuthorization()`) bietet CRUD
plus paginierte, nach Name filterbare und sortierte Liste:

| Methode | Route | Beschreibung |
|---|---|---|
| GET | `/api/artists?page=&pageSize=&name=` | Paginierte Liste, sortiert nach Name |
| GET | `/api/artists/{id}` | Einzelner Artist |
| POST | `/api/artists` | Artist anlegen (`{ "name": "..." }`) |
| PUT | `/api/artists/{id}` | Artist umbenennen |
| DELETE | `/api/artists/{id}` | Artist löschen |

Die `userId` kommt in jedem Fall aus dem `sub`-Claim des Tokens — nie aus
dem Request. Ein doppelter Name innerhalb der eigenen Sammlung liefert
HTTP 409, eine fremde oder unbekannte Id HTTP 404 (nicht 403) — analog
Genre. Ein `labelId`-Filter fehlt in diesem Slice bewusst: `artist` hat
keine `label_id`-Spalte, die Beziehung zu Label besteht erst indirekt über
die künftige `record`-Tabelle (Slice 6). Das Angular-Feature `artists/`
folgt erst mit Block 0c (Angular-Workspace), siehe `TASK.md`.

### Swagger/OpenAPI (Block 0e)

Im Development-Modus ist unter `http://localhost:<api-port>/swagger` eine
Swagger-UI erreichbar, die alle Minimal-API-Endpunkte (`/api/me`,
`/api/genres`, `/api/countries`, `/api/labels`, `/api/artists`) samt ihrer
`<summary>`-Beschreibungen auflistet. Über den Button "Authorize" lässt sich
ein Access Token (siehe
oben, Token-Bezug) als Bearer-Token hinterlegen, danach sind auch die
geschützten Endpunkte direkt aus der UI heraus aufrufbar. Im
Aspire-Dashboard erscheint neben der `api`-Ressource ein direkter
„Swagger UI"-Link, der genau dorthin führt.

In Production ist die Swagger-UI aktuell **nicht** erreichbar — die laut
CLAUDE.md §5.3 vorgesehene Freischaltung für die Admin-Rolle setzt das noch
nicht existierende Rollenkonzept voraus und folgt mit Block 7 (siehe `TASK.md`
und `docs/adr/0007-swagger-openapi-nur-development.md`).

### Prüfen

```powershell
dotnet build MyMusic.slnx
dotnet test MyMusic.slnx
```

Der Integrationstest startet den kompletten AppHost inklusive Container und braucht daher
rund eine Minute je Testklasse.

Restore, Build, `dotnet format --verify-no-changes`, ein Zeilenlängen-Check und die
Unit-Test-Projekte (Domain, Application, Infrastructure, Api) laufen zusätzlich automatisch
bei jedem Push und Pull Request auf `main` in `.github/workflows/ci.yml`.
`MyMusic.IntegrationTests` läuft dort bewusst nicht mit (siehe
`docs/adr/0003-ci-gate-codequalitaet.md`).
