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
- **Authentifizierung** — Anmeldung und Selbstregistrierung via Keycloak (Authorization Code + PKCE), Mandantentrennung
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
| Node.js | ≥ 22.22.3 (oder ≥ 24.15.0 / ≥ 26) | von Angular CLI 22 vorausgesetzt; für den Angular-Workspace |

### Secrets einrichten

Der AppHost erwartet vier Parameter als User Secrets. Sie liegen außerhalb des Repositories
und gehören nie ins Git:

```powershell
cd src/MyMusic.AppHost
dotnet user-secrets set "Parameters:postgres-password" "<wert>"
dotnet user-secrets set "Parameters:api-database-password" "<wert>"
dotnet user-secrets set "Parameters:keycloak-admin-password" "<wert>"
dotnet user-secrets set "Parameters:discogs-access-token" "<wert>"
```

`discogs-access-token` ist ein Discogs Personal Access Token (unter
discogs.com/settings/developers manuell zu erzeugen) für den serverseitigen
Discogs-Proxy (`GET /api/discogs/...`, siehe ADR 0018).

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
403). `DELETE /api/genres/{id}` liefert zusätzlich HTTP 409, wenn noch
mindestens ein Track (Slice 6) das Genre referenziert (Nachtrag Block 6d).
Das Angular-Feature `genres/` (Block 2 Frontend, PR #47) ist als
Referenz-Slice umgesetzt — Tabellenansicht, Namensfilter, Add/Edit als
Modal, siehe `TASK.md`.

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
oder unbekannte Id HTTP 404 (nicht 403) — analog Genre. `DELETE
/api/labels/{id}` liefert zusätzlich HTTP 409, wenn noch mindestens ein
Record (Slice 6) das Label referenziert (Nachtrag Block 6d). Das
Angular-Feature `labels/` (Block 4 Frontend, PR #49) ist umgesetzt —
zusätzlich zum Namensfilter aus Genre ein Land-Filter (natives `<select>`),
siehe `TASK.md`.

### Artist-Slice (Block 5)

Strukturell nahezu identisch zu Genre (kein Fremdschlüssel, kein
Zusatzfeld) — nur mit anderer Namenslänge und breiterem Zeichensatz.
`ArtistEndpoints` (`/api/artists`, `.RequireAuthorization()`) bietet CRUD
plus paginierte, nach Name filterbare und sortierte Liste:

| Methode | Route | Beschreibung |
|---|---|---|
| GET | `/api/artists?page=&pageSize=&name=&labelId=` | Paginierte Liste, sortiert nach Name |
| GET | `/api/artists/{id}` | Einzelner Artist |
| POST | `/api/artists` | Artist anlegen (`{ "name": "..." }`) |
| PUT | `/api/artists/{id}` | Artist umbenennen |
| DELETE | `/api/artists/{id}` | Artist löschen |

Die `userId` kommt in jedem Fall aus dem `sub`-Claim des Tokens — nie aus
dem Request. Ein doppelter Name innerhalb der eigenen Sammlung liefert
HTTP 409, eine fremde oder unbekannte Id HTTP 404 (nicht 403) — analog
Genre. Der `labelId`-Filter (Nachtrag Block 6d) löst die Beziehung indirekt
über die `record`-Tabelle auf (`artist` hat keine eigene `label_id`-Spalte,
nur `record.artist_id → record.label_id`); eine fremde oder unbekannte
`labelId` liefert eine leere Liste, kein HTTP 400 (Analogie zu `countryId`
bei `GET /records`). `DELETE /api/artists/{id}` liefert HTTP 409, wenn noch
mindestens ein Record oder Track (Slice 6) den Artist referenziert (zwei
getrennte Existenzabfragen, Nachtrag Block 6d). Das Angular-Feature
`artists/` (Block 5 Frontend, 2026-08-13) übernimmt die Muster aus Genre
(Referenz-Slice) 1:1 — Tabellenansicht, Namensfilter, Add/Edit als Modal.
Ohne UI für den `labelId`-Filter: Anders als bei Country (`GET /countries`
liefert ungefiltert alle 238 Einträge) gibt es für Label keinen
ungefilterten Endpunkt, nur das auf 100 Einträge geklemmte paginierte
`GET /labels` — eine Dropdown-Quelle dafür bleibt offen, siehe `TASK.md`.

### Album-Cover-Upload (Block 6b)

`RecordEndpoints` (`/api/records`) bietet zusätzlich einen dedizierten
Upload-Endpunkt für das Album-Cover eines eigenen Records:

| Methode | Route | Beschreibung |
|---|---|---|
| POST | `/api/records/{id}/cover` | Album-Cover hochladen (`multipart/form-data`, JPEG/PNG, max. 5 MB) |

Der Upload ist unabhängig vom Anlegen/Bearbeiten des Records. Eine fremde
oder unbekannte Record-Id liefert HTTP 404 (nicht 403); ein ungültiges
Format oder eine zu große Datei liefern HTTP 400 (im Frontend künftig als
Modal darzustellen, siehe `wiki/architektur/fehler-und-ausnahmekonzept.md`).
Das Cover wird als `bytea` in der `record`-Tabelle gespeichert und in
`RecordResponse.AlbumCoverDataUrl` als vollständige Data-URL
(`data:image/jpeg;base64,...` bzw. `image/png`) zurückgegeben — sowohl beim
Einzelabruf als auch je Item der paginierten Liste.

### Record-Liste (Block 6f)

`GET /api/records` filtert seit Block 6f zusätzlich exakt nach Format:

| Methode | Route | Beschreibung |
|---|---|---|
| GET | `/api/records?...&format=` | Zusätzlicher Filter auf einen der zehn `RecordFormat`-Werte (kein Gruppieren nach Vinyl/CD) |

Ein unbekannter `format`-Wert wirkt wie kein Filter (kein HTTP 400) — Queries
werden im CQRS-Framework grundsätzlich nicht validiert, analog zu `sortBy`/
`sortDirection`.

Das Angular-Feature `records/` zeigt die Sammlung erstmals als Card-Grid
statt als Tabelle (`features/records/`, siehe `komponenten-klassen.md` für
die Karten-/Grade-Badge-Klassen). Die Filter-Zeile kombiniert Albumname
(Freitext), Erscheinungsjahr-Zeitraum, Land/Format (native `<select>`,
feste kleine Wertemengen) und Sortierung mit einem neuen, wiederverwendbaren
Baustein `shared/autocomplete/`: Artist und Label werden per serverseitigem
Freitext-Autosuggest gegen den bestehenden `getPaged`-Endpoint gesucht
(debounced, kleines `pageSize`) statt als Dropdown — diese Listen können
beliebig groß werden, ein vollständig geladenes `<select>` wäre nicht
bedienbar.

### Record anlegen/bearbeiten/löschen (Block 6g)

Reiner Frontend-Block — das Backend (`POST`/`PUT`/`DELETE /api/records/{id}`)
war seit Block 6a bereits vollständig vorhanden. Anlegen/Bearbeiten laufen
als Modal (`features/records/record-form/`, Signal Forms, analog zum
Label-Formular), Löschen über das bestehende `shared/confirm-modal/`. Da die
Card-Ansicht (anders als die Tabellen-Slices) keine Aktionsspalte hat,
tragen die Cards jetzt Bearbeiten-/Löschen-Icons (`RecordCard`, mit
`stopPropagation()` gegen das bestehende `opened`-Output). Label und Artist
werden im Formular über dieselbe `shared/autocomplete/`-Komponente wie im
Filter (Block 6f) gewählt, statt eines nativen `<select>` — dafür wurde die
Komponente um ein optionales `initialQuery`-Input erweitert, das im
Bearbeiten-Modus den bisherigen Namen vorbefüllt (`linkedSignal`, analog dem
Vorbefüll-Muster aus `LabelForm`). Kein "Discogs-Suche"-Button — der im
Wiki (`ui-ux-konzept.md`) beschriebene verschachtelte Discogs-Modal-Flow
gehört zum weiterhin offenen Block 8.

### Record-Detailansicht (Block 6h)

Reiner Frontend-Block — `GET /api/records/{id}` war seit Block 6a bereits
vollständig vorhanden (inkl. aufgelöster Tracks und Cover als Base64-Data-
URL). Laut Design-Prototyp (`wiki/design/ui-kit.md`) ist die Detailansicht
ein **Modal** über dem weiterhin sichtbaren Records-Grid, keine eigene
Vollbild-Seite. Umgesetzt als Kind-Route von `/records`
(`features/records/records.routes.ts`: `{ path: '', component: Records,
children: [{ path: ':id', component: RecordDetail }] }`), damit `Records`
gemountet bleibt und `RecordDetail` per `<router-outlet>` als Modal darüber
rendert — die URL bleibt dabei verlinkbar (`/records/:id`). Neue Komponente
`features/records/track-list/` zeigt die Tracks rein lesend, gruppiert nach
Plattenseite (`recordSide`, keine Gruppen-Überschrift bei ausschließlich
`recordSide = '0'`, also CD). Kein Bearbeiten/Löschen im Detail-Modal — das
bleibt Aufgabe der Icons auf der `RecordCard` in der Liste (Block 6g).

Direkt aus dem Formular lassen sich unbekannte Label/Artist anlegen, ohne
die Ansicht zu wechseln: Beim Künstler fragt ein `ConfirmModal` nach
Verlassen des Feldes mit einem gültigen, unbekannten Namen nach ("Soll der
Künstler '…' neu angelegt werden?"), beim Label öffnet ein Icon-Button
(mit Tooltip) das bestehende `LabelForm` als zweites, verschachteltes
Modal. Dafür wurden drei gemeinsam genutzte Bausteine erweitert:
`shared/modal/` schließt bei mehreren gleichzeitig offenen Modals über
Escape jetzt nur noch das oberste (modulweiter Stack), `shared/autocomplete/`
bekam einen `blur`-Output und eine öffentliche `setQuery()`-Methode für
programmatische Textänderungen von außen, und `shared/confirm-modal/`
akzeptiert jetzt ein alternatives `confirmLabel`/`confirmVariant` statt
fest "Löschen"/`.btn-danger` zu verwenden.

### Album-Cover-Upload (Block 6i)

Reiner Frontend-Block — `POST /api/records/{id}/cover` war seit Block 6b
bereits vollständig vorhanden. Der Upload-Trigger sitzt im
`RecordForm`-Modal (`features/records/record-form/`), sowohl beim Anlegen
als auch beim Bearbeiten eines Records — nicht im Detail-Modal (Block 6h)
und nicht als eigenes Icon auf der `RecordCard`. Ein Cover lässt sich
hinzufügen und ersetzen, aber nicht löschen (dafür fehlt ein
`DELETE`-Endpunkt im Backend). Nach erfolgreichem `create`/`update` wird
bei gewählter Datei zusätzlich `uploadCover(...)` aufgerufen — als
eigenständiger, unabhängiger Schritt: Ein fehlgeschlagener Cover-Upload
zeigt ein Fehler-Modal, blockiert aber nicht das bereits erfolgreiche
Speichern des Records. Client-seitige Vorprüfung von Format (JPEG/PNG) und
Größe (max. 5 MB) vor jedem Server-Roundtrip. `shared/error-modal/` bekam
dafür einen neuen `ErrorModalKind` `'validation'` für echte
Validierungsmeldungen aus `ValidationProblemDetails.errors` bei
HTTP-400-Antworten, statt der bisherigen generischen
"Serverfehler"-Meldung.

### Track-CRUD in der Detailansicht (Block 6j)

Reiner Frontend-Block — `POST/PUT/DELETE /api/records/{id}/tracks[/{trackId}]`
war seit Block 6c bereits vollständig vorhanden. Tracks lassen sich jetzt
direkt in der Record-Detailansicht (`RecordDetail`, Block 6h) hinzufügen,
bearbeiten und löschen, über ein neues, verschachteltes `TrackForm`-Modal
(gleiches Verschachtelungsmuster wie `LabelForm` im `RecordForm`) und
`ConfirmModal` für das Löschen. Nach jeder Änderung wird der Record neu
geladen. Der Record selbst bleibt weiterhin nicht aus dem Detail-Modal
heraus bearbeit-/löschbar — dafür bleiben die Icons auf der `RecordCard`
zuständig.

Fremdschlüssel-Auswahl im `TrackForm` bewusst unterschiedlich gelöst: Genre
über ein natives `<select>` (`GenreService.getAll()`, `GET
/api/genres/all` — Endpunkt bestand seit Block 6e, war im Frontend
ungenutzt), da Genre-Listen typischerweise klein bleiben; Artist über
Autocomplete mit `getPaged` (identisch zum bereits etablierten Artist-Feld
in `RecordForm`), da Artist-Listen groß werden können. Kein
Inline-„Artist neu anlegen"-Flow wie im `RecordForm` — die User Stories
verlangen dafür nur eine Inline-Fehlermeldung bei ungültiger Referenz.

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

### Frontend (Block 0c)

Der Angular-22-Workspace liegt unter `src/frontend/` und startet als eigene
Ressource `frontend` automatisch mit dem AppHost (`builder.AddJavaScriptApp(...)`,
Paket `Aspire.Hosting.JavaScript`) — der Port ist seit Block 7a fest auf `4200`
gepinnt (`WithHttpEndpoint(port: 4200, env: "PORT")`), analog zum bereits
bestehenden Keycloak-Port-Pinning: Der Keycloak-Realm-Client `mymusic-angular`
hat seine `redirectUris`/`webOrigins` hart auf `localhost:4200` hinterlegt, ein
von Aspire dynamisch vergebener Port hätte den Login-Flow beim Start über den
AppHost gebrochen. Ein direkter `ng serve` außerhalb des AppHosts startet
ebenfalls auf dem Standardport 4200.

Styling erfolgt über Tailwind CSS 3 plus das MyMusic-Design-System
(`src/frontend/src/styles/design-system/`, unverändert aus
`../../02 Wiki/MyMusic Wiki/raw/design_system/` übernommen). Inter und
JetBrains Mono sind self-hosted über `@fontsource/inter`/`@fontsource/jetbrains-mono`
eingebunden (keine Google-Fonts-Laufzeitabhängigkeit).

Die API-Basis-URL erreicht das Frontend nicht über Build-Zeit-Konfiguration,
sondern über `public/runtime-config.json`, die beim App-Start per `fetch()`
geladen wird (`RuntimeConfigService`, `provideAppInitializer()`). Ein
`prestart`/`prebuild`-Skript (`scripts/write-runtime-config.mjs`) schreibt die
Datei aus der vom AppHost gesetzten Umgebungsvariable `MYMUSIC_API_BASE_URL`
neu — Details und Alternativenabwägung in
`docs/adr/0009-angular-runtime-config-mechanismus.md`.

Block 0c lieferte bewusst nur das lauffähige Grundgerüst (Marke, Wortmarke,
Design-System-Nachweis); Navigation, Routing-Hierarchie und die
Feature-Ordner sind seit Block 0g vorhanden (siehe unten) — der fachliche
Inhalt der einzelnen Feature-Seiten folgt weiterhin erst mit den jeweiligen
Angular-Feature-Blöcken, siehe `TASK.md`.

```powershell
cd src/frontend
npm test -- --watch=false
npm run build
```

### Login-Flow (Block 7a)

Der Angular-Client meldet sich über Keycloak per Authorization Code + PKCE an
(`angular-auth-oidc-client`, Begründung und Alternativenvergleich in
`docs/adr/0010-angular-oidc-bibliothek.md`). Die Keycloak-Authority-URL folgt
demselben Runtime-Config-Mechanismus wie die API-Basis-URL (ADR 0009) —
`write-runtime-config.mjs` schreibt sie zusätzlich aus der vom AppHost
gesetzten Umgebungsvariable `MYMUSIC_KEYCLOAK_AUTHORITY` in
`public/runtime-config.json`.

`src/frontend/src/app/core/auth/` enthält die Konfigurations-Factory
(`keycloak-config.factory.ts`), den `AuthGuard` (Re-Export von
`autoLoginPartialRoutesGuard`) und einen projekteigenen Interceptor
(`unauthorized-redirect.interceptor.ts`), der bei HTTP 401/403 einer
API-Antwort zur Anmeldung umleitet. Der `scope` ist bewusst ohne
`offline_access` gesetzt, damit der Refresh Token an die in
`keycloak/mymusic-realm.json` hinterlegten SSO-Session-Grenzen (30 Minuten
Sliding Expiry, 8 Stunden Hard-Cap) gebunden bleibt.

Die API erlaubt CORS-Anfragen bislang nur in Development, beschränkt auf
`localhost`-Origins (`Program.cs`, `Uri.IsLoopback`) — die Production-Whitelist
aus `sicherheitskonzept.md` ist noch offen (`TASK.md` Abschnitt 7).

Bewusst nicht Teil dieses Blocks: Rollenkonzept/`AdminGuard`, Admin-Bereich,
Rate Limiting, Content Security Policy, Keycloak-Custom-Theme der
Anmeldeseite — siehe `TASK.md` Abschnitt 7 und
`wiki/user-stories/user-stories-authentifizierung.md`.

### Navigation und Routing-Skelett (Block 0g)

`src/frontend/src/app/nav/` enthält die `NavComponent` (Brand, Tabs
Dashboard/Records, Option-Dropdown mit Artists/Labels/Genres, funktionales
Suchfeld, Theme-Toggle, Login/Logout/Username) — sie ersetzt die bisherige,
in `app.html` fest verdrahtete Kopfzeile aus Block 0c/7a/0f. `app.routes.ts`
lädt jedes Feature (`dashboard/`, `records/`, `artists/`, `labels/`,
`genres/`, `search/`) lazy über eine eigene `*.routes.ts`; `''` und
unbekannte Pfade redirecten auf `/dashboard`. Die sechs Feature-Seiten sind
aktuell reine Platzhalter — der fachliche Inhalt folgt mit den jeweiligen
CRUD-Feature-Blöcken.

Icons stammen aus `@lucide/angular` (Nachfolger des in
`docs/adr/0011-theme-infrastruktur.md` erwähnten, mittlerweile deprecated
`lucide-angular` — Details in `docs/adr/0012-icon-bibliothek-lucide-angular.md`).
Das Suchfeld ist die erste Verwendung von Signal Forms
(`@angular/forms/signals`) im Projekt.

Bewusst nicht Teil dieses Blocks: Admin-Button/`AdminGuard` (Rollenkonzept,
`TASK.md` Abschnitt 7), responsives Icon-only-/Hamburger-Verhalten,
Benutzerprofil-Modal.

### Rollenkonzept im Angular-Code (Block 7b)

`src/frontend/src/app/core/auth/user-roles.service.ts` liest die
Keycloak-Realm-Rolle `Admin` aus dem rohen **Access Token**
(`OidcSecurityService.getPayloadFromAccessToken()`), reaktiv erneut
ausgelöst bei jeder Änderung des `authenticated`-Signals — der zunächst
naheliegende Weg über `userData()` (Ergebnis des Keycloak-`/userinfo`-
Endpunkts) funktioniert nicht, da dort kein `realm_access`-Claim enthalten
ist (live gegen den laufenden Aspire-AppHost verifiziert). Der
`AdminGuard` (`core/auth/admin.guard.ts`) leitet ohne diese Rolle still auf
`/dashboard` um; die `NavComponent` zeigt den Admin-Button (Label only,
kein Icon) nur mit der Rolle, zwischen Theme-Toggle und Username/Login.
`/admin` (`features/admin/`) ist bislang nur eine Platzhalterseite — der
eigentliche Admin-Bereich (Userliste/-löschung über die Keycloak Admin REST
API) folgt mit einem eigenen, späteren Block (`TASK.md` Abschnitt 7).

### Registrierung (Block 7g)

Neben „Login" sitzt in der Kopfzeile ein „Registrieren"-Button, der über
`OidcSecurityService.authorize(undefined, { urlHandler })` gezielt zu
Keycloaks Registrierungsendpunkt (`/protocol/openid-connect/registrations`
statt `/protocol/openid-connect/auth`) weiterleitet — dieselbe PKCE-/
State-Logik wie beim normalen Login, nur mit anderem Ziel-Pfad
(`docs/adr/0010-angular-oidc-bibliothek.md`, Nachtrag). Das
Registrierungsformular fragt nur Benutzername, Passwort und E-Mail ab
(Vorname/Nachname wurden aus Keycloaks User-Profile-Konfiguration entfernt,
siehe `keycloak/mymusic-realm.json`); neu registrierte Benutzer erhalten
automatisch die Realm-Rolle `User` über eine Default-Rollen-Konfiguration.

Damit der Registrieren-/Login-Button für nicht angemeldete Benutzer
überhaupt erreichbar ist, ist der Wurzelpfad `''` seit diesem Block eine
eigene, unbewachte `Landing`-Route (`core/shell/landing/`) statt eines
sofortigen Redirects auf das geschützte `/dashboard` — vorher leitete der
`AuthGuard` nicht angemeldete Aufrufe sofort zu Keycloak weiter, bevor ein
Klick im Header möglich war. Bereits angemeldete Benutzer landen weiterhin
automatisch auf `/dashboard`; alle anderen Routen bleiben unverändert
vollständig geschützt (Details: `docs/adr/0017-unbewachte-landing-route.md`).

### Discogs-Integration (Block 8a Backend, Block 8b Frontend)

Serverseitiger Proxy zur Discogs-API — der Angular-Client kommuniziert nie
direkt mit Discogs, das Personal Access Token bleibt im Backend:

| Methode | Route | Beschreibung |
|---|---|---|
| GET | `/api/discogs/search?q=` | Kurzdaten-Trefferliste (Titel, Jahr, Label als Text, Thumbnail), unpaginiert, `q` mindestens 2 Zeichen |
| GET | `/api/discogs/releases/{id}` | Volldaten eines Treffers: Cover (serverseitig heruntergeladen und als Data-URL eingebettet), Tracklist inkl. Pro-Track-Artist, Artist(s), Label(s), Genre/Style, Format-Rohdaten |

Ein neuer externer HTTP-Client `IDiscogsClient`/`DiscogsClient`
(`Infrastructure/ExternalServices/Discogs/`) nach Vorbild des
`KeycloakAdminClient` übernimmt Authentifizierung (Token als
`Authorization`-Header) und Fehlerbehandlung: Jeder Discogs-Fehler
(Nichterreichbarkeit, Rate-Limit, unbekannte Release-Id) liefert einheitlich
HTTP 502, nicht 404/500 (`docs/adr/0018-discogs-proxy-token-und-fehlerbehandlung.md`).

Das Cover wird **nicht** an den Browser durchgereicht und dort per `fetch()`
geladen — Discogs blockiert das Hotlinking seiner Bilder ohne passenden
`User-Agent`/Referer. Stattdessen lädt `DiscogsClient` das Bild serverseitig
über denselben authentifizierten Client und bettet es als Base64-Data-URL in
die Release-Antwort ein (`docs/adr/0020-discogs-cover-serverseitig-eingebettet.md`).

Bei Various-Artists-Releases liefert Discogs pro Tracklist-Eintrag einen
eigenen Artist, der vom Release-Artist abweichen kann — `DiscogsTrackResponse`
bildet das über ein optionales Pro-Track-Artist-Feld ab
(`docs/adr/0019-discogs-track-artist-zuordnung.md`).

Im `RecordForm` (`features/records/`) öffnet ein „Discogs-Suche"-Button ein
verschachteltes Such-Modal (`discogs-search/`, Muster wie `LabelForm` im
`RecordForm`). Nach Auswahl eines Treffers werden automatisch übernommen:
Albumname, Erscheinungsjahr, Label, Record-Artist, Cover sowie — nach dem
ersten Speichern des Records, da das Formular selbst keine Track-Eingabe hat
(Block 6j) — alle Tracks der Tracklist inkl. Seite/Tracknummer. Referenziert
ein Treffer einen beim Benutzer noch nicht vorhandenen Artist, ein Label oder
ein Genre, fragt vor der Übernahme ein Bestätigungsdialog nach (je
distinktem Namen, bei Compilations potenziell mehrfach für Artist).

Zwei Discogs-Eigenheiten, mit einem echten Release verifiziert (Discogs
91831, „Atmos – Headcleaner"), statt aus der API-Dokumentation angenommen:

- Discogs lässt bei einer Seite mit nur einem Track die Tracknummer in der
  Positionsangabe weg (`"A"` statt `"A1"`) — `discogs-position.ts` erkennt
  das als Tracknummer 1 dieser Seite, statt es auf eine Fallback-Seite „0"
  fallen zu lassen.
- Discogs-Namen (Artist/Label/Genre) können Disambiguierungs-Suffixe wie
  `" (2)"` und Zeichen enthalten, die die jeweilige Namensvalidierung nicht
  erlaubt — `discogs-name-sanitizer.ts` bereinigt sie vor Existenz-Abgleich
  und Neuanlage.

### Dashboard (Block 9)

Aggregierte Statistik-Übersicht der eigenen Sammlung unter `/dashboard` —
ersetzt den Platzhalter aus Block 0g:

| Methode | Route | Beschreibung |
|---|---|---|
| GET | `/api/dashboard` | Aggregierte Kennzahlen des angemeldeten Benutzers: Records/Artists/Labels/Genres gesamt, Records nach Format, Top-10-Artists, Top-10-Labels, Verteilung nach Erscheinungsjahr |

Die ursprünglich vorgesehene Aggregation über volle `Record`-Entitäten
(analog `GetAllArtistsQueryHandler`) hätte pro Datensatz auch das
potenziell bis zu 5 MB große `album_cover`-Feld mitgeladen, obwohl die
Auswertung nur wenige Spalten braucht. Stattdessen bekam `IRepository<T>`
eine neue, generische Projektions-Methode `GetProjectedAsync`, die EF Core
in ein SQL-`SELECT` nur der referenzierten Spalten übersetzt
(`docs/adr/0021-repository-projektion-fuer-dashboard-aggregation.md`).

Frontend (`features/dashboard/`): `DashboardComponent` mit fünf
Kind-Komponenten — vier `StatTileComponent` für die Kennzahlen-Kacheln,
`FormatChartComponent`, `TopArtistsComponent`, `TopLabelsComponent` als
horizontale Balkenlisten sowie `YearDistributionComponent` als eigene,
volle Zeile unterhalb der drei übrigen Detail-Kacheln.

Bei der manuellen Live-Verifikation gegen echte Sammlungsdaten zeigten
sich mehrere Layout- und Lesbarkeitsprobleme, die jeweils korrigiert und
erneut live bestätigt wurden:

- Die vier Detail-Kacheln waren unterschiedlich groß, weil die
  Angular-Komponente selbst (nicht die `.card`-Div darin) die Grid-Zelle
  bildete — behoben über `host: { class: 'contents' }` an allen fünf
  Dashboard-Kindkomponenten, dazu ein fester Inhaltsbereich für zehn
  Zeilen bei Format/Top Artists/Top Labels.
- Lange Artist-/Label-Namen wurden bei fester, schmaler Namensbreite
  abgeschnitten — Name sowie Balken/Zahl stehen jetzt in zwei Zeilen je
  Eintrag.
- Die Jahresverteilung zeigte ursprünglich nur Balken für Jahre mit
  Records nebeneinander, wodurch zeitliche Lücken (z. B. zwischen 1990
  und 2004) nicht erkennbar waren — korrigiert auf eine lückenlose
  Balkenreihe vom ersten bis zum letzten vorhandenen Jahr, Jahre ohne
  Records innerhalb dieser Spanne als Balken mit Anzahl 0.
- Die Jahresverteilung stand ursprünglich in einem 2×2-Grid mit den drei
  übrigen Detail-Kacheln und wäre bei einer großen Zeitspanne (z. B. 1950
  bis heute) mit vielen Balken darin nicht mehr lesbar geworden — steht
  jetzt als eigene volle Zeile; Jahres- und Anzahl-Beschriftung werden bei
  vielen Balken automatisch ausgedünnt.

### Volltext-Suche (Block 10)

Globale Suche über die eigene Sammlung unter `/search` — ersetzt den
Platzhalter aus Block 0g:

| Methode | Route | Beschreibung |
|---|---|---|
| GET | `/api/search?q=` | Durchsucht ausschließlich Records des angemeldeten Benutzers über Titel, Artist (Record- und Track-Artist), Label, Genre (nur über Track) und Land (über Label→Country), jeweils per ILIKE-Teilstring (case-insensitive) |

Da die Domain-Entities keine EF-Navigationsproperties besitzen, werden die
Suchkriterien wie beim bestehenden `countryId`-Filter im Record-Slice über
vorab aufgelöste Id-Mengen (`IRepository<T>.GetProjectedAsync`, siehe
ADR 0021) kombiniert. Eigene Response-DTOs (`SearchResultResponse`,
`SearchResponseBuilder`) statt einer Wiederverwendung der Record-Response,
um die im Projekt geltende Feature-Kapselung („kein Handler greift in den
Ordner eines anderen Features") auch für dieses Feature ohne eigene
Entität einzuhalten.

Frontend (`features/search/`): Die `Search`-Komponente ersetzt den
Platzhalter aus Block 0g — Treffer erscheinen als Card-Raster
(`RecordCard`), identisch zur normalen Records-Ansicht, und sind voll
editierbar (Bearbeiten/Löschen direkt aus der Card); ein Klick auf eine
Card navigiert zusätzlich zur Record-Detailansicht inklusive Tracklist,
damit auch Treffer über Track-Artist oder Genre (beide nur auf den Tracks
vorhanden) nachvollziehbar bleiben. Eine zunächst geplante
Tabellendarstellung nach dem Tabellen-Slice-Muster wurde noch am selben
Tag wieder verworfen, da sie diese Tracks nicht gezeigt hätte.

Zusätzlich eine Eingabevalidierung am Kopfzeilen-Suchfeld
(`NavComponent`): mindestens 2 Zeichen, gleiches Zeichenset wie
`album_name`, ausschließlich im Frontend geprüft.

### Prüfen

```powershell
dotnet build MyMusic.slnx
dotnet test MyMusic.slnx
```

Der Integrationstest startet den kompletten AppHost inklusive Container und braucht daher
rund eine Minute je Testklasse.

Restore, Build, `dotnet format --verify-no-changes`, ein Zeilenlängen-Check, die
Unit-Test-Projekte (Domain, Application, Infrastructure, Api) sowie
`MyMusic.IntegrationTests` laufen zusätzlich automatisch bei jedem Push und
Pull Request auf `main` in `.github/workflows/ci.yml` (Integrationstest-Job mit
15-Minuten-Timeout).
