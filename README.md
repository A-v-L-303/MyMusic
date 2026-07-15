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
│           Application Layer            │  Commands, Queries, Services
├─────────────────────────────────────────┤
│          Infrastructure Layer          │  EF Core, PostgreSQL, Keycloak
├─────────────────────────────────────────┤
│           Core (Domain) Layer          │  Entities, Value Objects, Aggregates
└─────────────────────────────────────────┘

         Angular 22 Frontend (SPA)
```

---

## Repository-Struktur

```
.claude/         Arbeitsmodell für Claude Code (Agenten, Permissions)
docs/adr/        Architekturentscheidungen (ADRs)
docs/prompts/    Archiv der freigegebenen Arbeits-Prompts
keycloak/        Realm-Definition als JSON-Import (versioniert)
src/             Anwendungscode (Backend und Frontend)
tests/           Automatisierte Tests
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

### Prüfen

```powershell
dotnet build MyMusic.slnx
dotnet test MyMusic.slnx
```

Der Integrationstest startet den kompletten AppHost inklusive Container und braucht daher
rund eine Minute.
