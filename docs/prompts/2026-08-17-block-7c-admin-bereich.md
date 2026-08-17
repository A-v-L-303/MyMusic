# Block 7c: Admin-Bereich

## Context

`01 Repos/MyMusic/TASK.md` Abschnitt 7 führt den Admin-Bereich
(Benutzerliste/-löschung über die Keycloak Admin REST API) als offenen Punkt
aus Block 7a. Die fachlichen User Stories dafür liegen bereits vollständig im
Wiki (`wiki/user-stories/user-stories-admin.md`, Stand 2026-08-16, inkl.
Klärungen zum Service-Account-Client, zur Selbstlöschungs-Sperre und zur
Löschreihenfolge) — TASK.md Abschnitt 1 spiegelt das noch nicht wider, das
ist eine Doku-Lücke, die mit diesem Block geschlossen wird. Die Seite
beschreibt den fachlichen Inhalt von `/admin` durchgehend end-to-end (Aufruf
von `/admin` zeigt die Liste, Klick auf Löschen öffnet ein Modal usw.) — der
Slice ist bewusst nicht in Backend/Frontend aufgeteilt, anders als bei
Genre/Label/Artist. Dieser Block liefert deshalb beides zusammen: die
Backend-Endpunkte und das Angular-Feature `features/admin/`, das den seit
Block 7b bestehenden Platzhalter ersetzt (`AdminGuard`, Admin-Button,
Route `/admin` sind bereits vorhanden und bleiben unverändert).

Zwei technische Weichenstellungen ergeben sich auf der Backend-Seite, die im
bisherigen Code noch keine Präzedenz haben und hier erstmals entschieden
werden:

1. **Serverseitige Rollenprüfung existiert noch gar nicht.** `Program.cs`
   ruft `AddAuthorization()` ohne jede Policy auf; alle bisherigen Endpoints
   nutzen nur ein rollenloses `.RequireAuthorization()`. `ICurrentUserService`
   liest ausschließlich den `sub`-Claim. Für den Admin-Bereich muss aus dem
   JWT die Rolle `Admin` gelesen werden — und zwar aus dem
   `realm_access`-Claim, da `MapInboundClaims = false` gesetzt ist (roher
   Claim-Name, keine automatische Rollen-Übersetzung).
2. **Secret-Provisionierung für den neuen Keycloak-Service-Account-Client.**
   Mit dir geklärt (s. u.): Keycloak generiert das Secret selbst, die API
   liest es beim Start per Bootstrap-Admin-Credentials aus — kein neues
   Secret in User Secrets nötig.

## Scope

**In diesem Block:**

- `GET /api/admin/users` — paginierte Liste aller Keycloak-Benutzer mit
  Benutzername, E-Mail, Rolle (US-AD1).
- `DELETE /api/admin/users/{id}` — Benutzer inkl. aller App-Daten löschen,
  danach der Keycloak-Account; Selbstlöschung gesperrt (US-AD3).
- Serverseitige Autorisierung: nur Rolle `Admin` darf beide Endpunkte
  aufrufen; nicht authentifiziert → 401, authentifiziert ohne Admin-Rolle →
  403 (US-AD2).
- Kein Zugriff auf fremde Sammlungsdaten über den Admin-Bereich (US-AD4) —
  automatisch erfüllt, da der Admin-Bereich ausschließlich Kontodaten
  liefert, keine Records/Artists/Labels/Genres.
- Neuer Keycloak-Service-Account-Client in `mymusic-realm.json`.
- Angular-Feature `features/admin/`: Userliste (Benutzername, E-Mail,
  Rolle), Löschen mit Bestätigungsmodal, kein Löschen-Icon bei der eigenen
  Zeile — ersetzt den Platzhalter aus Block 7b.
- Unit- und Integrationstests (Backend), Vitest-Tests (Frontend),
  Wiki-/ADR-/TASK.md-Updates.

**Nicht Teil dieses Blocks** (eigene, spätere Punkte laut TASK.md
Abschnitt 7):

- Swagger-UI-Freischaltung für Production, Rate Limiting,
  CORS-Production-Whitelist, CSP.

## Geklärt mit dir

- **Backend und Frontend gemeinsam**: Anders als bei Genre/Label/Artist wird
  der Admin-Bereich nicht in getrennte Backend-/Frontend-Blöcke aufgeteilt,
  weil der Slice laut User Stories durchgehend als ein zusammenhängendes
  Feature beschrieben ist.
- **Secret-Provisionierung**: Der neue Service-Account-Client bekommt
  **kein** Secret im JSON (Keycloak generiert eines zufällig beim
  Realm-Import). Die API liest dieses Secret beim Start einmalig über die
  Keycloak Admin REST API aus, authentifiziert dafür transient mit den
  bereits bestehenden Bootstrap-Admin-Credentials (`admin` /
  Aspire-Parameter `keycloak-admin-password`, bisher nur an den
  Keycloak-Container, neu zusätzlich an die API durchgereicht) — kein neuer
  Aspire-Secret-Parameter nötig.
- **Paginierung**: `GET /api/admin/users` wird paginiert wie die übrigen
  Listen-Endpunkte (`UserListResponse` mit `Items`/`TotalCount`/`Page`/
  `PageSize`/`TotalPages`, Default `page=1`/`pageSize=20`), statt
  unpaginiert wie Country.
- **Kategorie „Verwaltung"**: Neue, dritte Kategorie unter
  `Application/Features/` zusätzlich zu `Stammdaten`/`Sammlung` — Admin
  passt fachlich in keine der beiden bestehenden Kategorien.
- **Lösch-Bestätigungstext**: Bekommt einen stärkeren Warnhinweis als das
  sonst übliche knappe Muster (siehe Abschnitt 8), weil beim Löschen eines
  Benutzers dessen gesamte Sammlung unwiderruflich mitgelöscht wird.

## Design — Backend

### 1. Keycloak-Realm (`keycloak/mymusic-realm.json`)

Neuer Client, analog zu den bestehenden zwei Einträgen im `clients`-Array:

```json
{
  "clientId": "mymusic-admin-service",
  "name": "MyMusic Admin Service Account",
  "enabled": true,
  "protocol": "openid-connect",
  "publicClient": false,
  "standardFlowEnabled": false,
  "implicitFlowEnabled": false,
  "directAccessGrantsEnabled": false,
  "serviceAccountsEnabled": true
}
```

Kein `"secret"`-Feld (Keycloak generiert eines). Die Rollen
`view-users`/`manage-users` aus `realm-management` müssen dem Service-Account
zugewiesen werden — im Realm-Import über einen `users`-Eintrag für
`service-account-mymusic-admin-service` mit
`clientRoles: { "realm-management": ["view-users", "manage-users"] }`
(Keycloak-Standardformat für Service-Account-Rollenzuweisung per Import,
siehe Recherche).

### 2. Serverseitige Rollenautorisierung (`src/MyMusic.Api/`)

Neuer Ordner `src/MyMusic.Api/Authorization/`:

- `AdminRequirement.cs` — leeres Marker-`IAuthorizationRequirement`.
- `AdminAuthorizationHandler.cs` — `AuthorizationHandler<AdminRequirement>`:
  liest `context.User.FindFirst("realm_access")?.Value`, deserialisiert das
  JSON (`{"roles": [...]}`), prüft auf `"Admin"`. Fehlt der Claim oder ist
  er nicht parsebar → kein `Succeed`, kein Fehler werfen (führt zu 403,
  nicht zu 500).

`Program.cs`-Ergänzung:

```csharp
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", policy => policy
        .RequireAuthenticatedUser()
        .Requirements.Add(new AdminRequirement()));
builder.Services.AddSingleton<IAuthorizationHandler, AdminAuthorizationHandler>();
```

**Wichtig**: `.RequireAuthenticatedUser()` ist zwingend nötig, sonst liefert
ein nicht authentifizierter Aufruf 403 statt der laut US-AD2 geforderten 401
(die `AdminRequirement`-Prüfung allein unterscheidet nicht zwischen „kein
Token" und „Token ohne Admin-Rolle").

`AdminEndpoints.cs` nutzt `.RequireAuthorization("Admin")` statt des
bisherigen rollenlosen `.RequireAuthorization()`.

### 3. Secret-Provisionierung beim Start

Neu in `src/MyMusic.Infrastructure/ExternalServices/Keycloak/`:

- `KeycloakServiceAccountSecretProvider.cs` — Singleton, hält das aktuell
  gültige Secret im Speicher (`string? Secret { get; set; }`).
- `KeycloakServiceAccountProvisioner.cs` — holt beim Start per
  Bootstrap-Admin-Token (Password-Grant gegen `master`-Realm,
  `admin-cli`-Client, analog zum bestehenden Muster in
  `tests/MyMusic.IntegrationTests/TestSupport/KeycloakTestClient.cs`) die
  interne Client-ID von `mymusic-admin-service`
  (`GET /admin/realms/mymusic/clients?clientId=...`) und danach dessen
  Secret (`GET /admin/realms/mymusic/clients/{id}/client-secret`), schreibt
  es in den `SecretProvider`.

`Program.cs`, vor `app.Run()`:

```csharp
await using (var scope = app.Services.CreateAsyncScope())
{
    var provisioner = scope.ServiceProvider.GetRequiredService<KeycloakServiceAccountProvisioner>();
    await provisioner.LoadSecretAsync();
}
```

Läuft einmalig beim API-Start, nach `WaitFor(keycloak)` in Aspire (Realm ist
zu dem Zeitpunkt bereits importiert).

### 4. Keycloak Admin REST API Client

Vertrag in `src/MyMusic.Application/Common/Services/IKeycloakAdminClient.cs`
(analog zu `ICurrentUserService` im selben Ordner):

```csharp
public interface IKeycloakAdminClient
{
    Task<IReadOnlyList<KeycloakUserSummary>> GetUsersAsync(CancellationToken cancellationToken);
    Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken);
}

public sealed record KeycloakUserSummary(Guid Id, string Username, string Email, bool IsAdmin);
```

Implementierung
`src/MyMusic.Infrastructure/ExternalServices/Keycloak/KeycloakAdminClient.cs`:
nimmt einen benannten `HttpClient` (BaseAddress = Keycloak-Host) und
`KeycloakServiceAccountSecretProvider` per Konstruktor. Holt sich pro Aufruf
frisch einen Client-Credentials-Token (bewusst **keine**
Token-Zwischenspeicherung — Admin-Operationen sind selten, Caching wäre
verfrühte Optimierung mit Ablauf-/Concurrency-Risiko).

`GetUsersAsync`: zwei Aufrufe unabhängig von der Benutzeranzahl —
`GET /admin/realms/mymusic/users` (alle Benutzer) und
`GET /admin/realms/mymusic/roles/Admin/users` (Benutzer mit Admin-Rolle),
Ergebnis gemerged (Admin-Liste bestimmt `IsAdmin`).

`DeleteUserAsync`: `DELETE /admin/realms/mymusic/users/{id}`.

DI in `Program.cs`:

```csharp
builder.Services.AddHttpClient<IKeycloakAdminClient, KeycloakAdminClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["Keycloak:AdminApiBaseUrl"]!));
```

Keine neue NuGet-Abhängigkeit nötig — `AddHttpClient` kommt aus dem
ASP.NET-Core-Shared-Framework, das `MyMusic.Api` als Web-SDK-Projekt bereits
referenziert.

### 5. CQRS-Feature `Application/Features/Verwaltung/Admin/`

Neue dritte Kategorie **`Verwaltung`** (bisher nur `Stammdaten`/`Sammlung`,
siehe `wiki/architektur/application-layer.md` — wird dort als Ergänzung zur
bisherigen „final"-Notiz dokumentiert).

- `Queries/GetPaged/GetPagedUsersQuery.cs` (`Page`, `PageSize`) +
  `GetPagedUsersQueryHandler.cs`: hängt von `IKeycloakAdminClient` +
  `UserResponseBuilder` ab (kein `IRepository<T>` — erster Handler ohne
  Datenbankzugriff, konsistent mit §4.3, nur der abhängige Service ist hier
  extern statt Repository). Die Keycloak Admin REST API liefert die
  vollständige Benutzerliste (kein serverseitiges Paging gegen Keycloak
  nötig — Benutzerzahl ist überschaubar), Sortierung alphabetisch nach
  Benutzername, Seitenausschnitt (`Skip`/`Take`) und `TotalCount` werden im
  Handler berechnet, `UserResponseBuilder.BuildPaged(...)` liefert
  `UserListResponse` (gleiche Form wie bei Genre/Label/Artist).
- `Commands/Delete/DeleteUserCommand.cs` (`TargetUserId`) +
  `DeleteUserCommandHandler.cs`: hängt von `IRepository<RecordEntity>`,
  `IRepository<LabelEntity>`, `IRepository<ArtistEntity>`,
  `IRepository<GenreEntity>`, `IKeycloakAdminClient`, `ICurrentUserService`,
  `ExceptionManager` ab.
  1. `TargetUserId == currentUserService.UserId` →
     `exceptionManager.Conflict("Der eigene Account kann nicht gelöscht
     werden.")` (409, passt zur bestehenden Fehlerkonzept-Tabelle für
     „Konflikt").
  2. App-Daten in FK-sicherer Reihenfolge löschen (RecordTrack hängt per
     `Cascade` an Record, muss nicht separat behandelt werden — Recherche
     bestätigt): erst alle `Record` des `TargetUserId`
     (`GetPagedAsync(r => r.UserId == TargetUserId, ..., pageSize:
     int.MaxValue, ...)`, `Remove`, `SaveChangesAsync`), dann `Label`, dann
     `Artist`, dann `Genre` (jeweils gleiches Muster, jeweils eigener
     `SaveChangesAsync`-Aufruf, da die bestehenden Restrict-FKs sonst
     zwischen den Schritten verletzt würden). Die bestehenden
     Referenz-Checks in `DeleteLabelCommandHandler`/
     `DeleteArtistCommandHandler`/`DeleteGenreCommandHandler` (die bei
     Fremdverweisen `ConflictException` werfen) werden hier bewusst
     **nicht** wiederverwendet — sie sind für Einzel-Lösch-Selfservice
     gedacht, nicht für den Komplett-Teardown eines Accounts.
  3. `await keycloakAdminClient.DeleteUserAsync(TargetUserId, ct)` —
     schlägt dieser Aufruf fehl, bleibt die Exception unbehandelt und läuft
     über den bestehenden `GlobalExceptionHandler`-Fallback auf 500
     (entspricht US-AD3 AC3: App-Daten bleiben gelöscht, Keycloak-Account
     bleibt bestehen, Admin bekommt eine Fehlermeldung — kein neuer
     Exception-Typ nötig, keine Transaktion/Rollback gewünscht).
- `ResponseDtos/UserResponse.cs` (`Username`, `Email`, `Role`) +
  `ResponseDtos/Builder/UserResponseBuilder.cs` (mappt
  `KeycloakUserSummary` → `UserResponse`, `IsAdmin` → `"Admin"`/`"User"`).

### 6. Endpoints

`src/MyMusic.Api/Endpoints/Verwaltung/Admin/AdminEndpoints.cs`:

```csharp
endpoints.MapGroup("/api/admin").RequireAuthorization("Admin");
// GET /api/admin/users?page=&pageSize=   -> GetPagedUsersQuery
// DELETE /api/admin/users/{id}           -> DeleteUserCommand
```

Je ein `<summary>`-Kommentar pro Endpoint-Methode (Swagger-Pflicht,
CLAUDE.md §9).

### 7. `src/MyMusic.AppHost/AppHost.cs`

- API-Ressource bekommt zusätzlich `KC_BOOTSTRAP_ADMIN_USERNAME`/
  `keycloakAdminPassword` als Environment-Variablen durchgereicht (gleicher
  Parameter wie beim Keycloak-Container, kein neuer Parameter).
- Neue Environment-Variable für die Admin-API-Basis-URL
  (`Keycloak:AdminApiBaseUrl`), abgeleitet aus `keycloak.GetEndpoint("http")`,
  analog zum bestehenden `Keycloak__Authority`-Muster.

## Design — Frontend

### 8. Angular-Feature `src/frontend/src/app/features/admin/`

Ersetzt den seit Block 7b bestehenden Platzhalter (`admin.ts`, `admin.html`,
`admin.spec.ts`) vollständig. `admin.routes.ts` und der `AdminGuard` bleiben
unverändert.

- `admin.ts` (Component) — analog zum Muster in `features/genres/genres.ts`
  (`rxResource`, `ErrorModalService`, `ConfirmModal`, `shared/pagination/`),
  aber ohne Filter und ohne Formular (nur Liste + Löschen). `rxResource` mit
  `page`-Signal ruft `AdminService.getPaged(page, pageSize)`.
  `pendingDelete`-Signal + `ConfirmModal` für die Löschbestätigung mit
  stärkerem Warnhinweis als das sonst übliche knappe Muster (vgl.
  `genres.ts`: „Soll „{name}" wirklich gelöscht werden?"), da hier nicht
  nur ein einzelner Datensatz, sondern die gesamte Sammlung des Benutzers
  unwiderruflich mitgelöscht wird: „Soll der Benutzer „{username}" wirklich
  gelöscht werden? Alle Records, Artists, Labels und Genres dieses
  Benutzers werden dabei unwiderruflich mitgelöscht." `ErrorModalService.
  showFromHttpError(...)` bei Ladefehlern und beim Löschfehler (u. a. 500,
  wenn die App-Daten bereits gelöscht sind, aber die Keycloak-Löschung
  fehlschlägt — US-AD3 AC3).
- `admin.html` — Tabelle mit den Spalten Benutzername, E-Mail, Rolle,
  Aktion, plus eingebettete `Pagination` (wie `GenreTable`). Die eigene
  Zeile zeigt kein Löschen-Icon (US-AD1 AC3, US-AD3 AC4) — Vergleich der
  Keycloak-User-Id mit dem `sub`-Claim aus
  `oidcSecurityService.getPayloadFromAccessToken()` (**nicht** aus
  `userData()`: Block 7b hat bereits empirisch belegt, dass der
  UserInfo-Endpunkt hinter `userData()` nicht alle benötigten Claims
  liefert — `getPayloadFromAccessToken()` ist die im Projekt bereits
  bewährte, zuverlässige Quelle, siehe `UserRolesService`).
- `admin.ts` (Model, neue Datei `admin-user.ts` oder inline) —
  `interface AdminUser { id: string; username: string; email: string;
  role: 'User' | 'Admin'; }`.
- `admin.service.ts` — `getPaged(page: number, pageSize: number):
  Observable<PagedResult<AdminUser>>` (`GET /api/admin/users`),
  `deleteUser(id: string): Observable<void>`
  (`DELETE /api/admin/users/{id}`); `HttpTestingController`-getestet
  analog zu `genre.service.spec.ts`.
- Wiederverwendet ohne Änderung: `shared/confirm-modal/`,
  `shared/error-modal/` (beide bereits global in `app.html` verdrahtet).
  Kein neuer `shared/`-Baustein nötig.
- Vitest-Tests: `admin.spec.ts` (ersetzt den bisherigen
  Platzhalter-Test), `admin.service.spec.ts`.

## Tests

- `MyMusic.Api.Tests`: `AdminAuthorizationHandlerTests` (gültige
  Admin-Rolle → Succeed; fehlender Claim; leeres `roles`-Array; malformed
  JSON → kein Succeed, keine Exception).
- `MyMusic.Application.Tests`: `GetAllUsersQueryHandlerTests`,
  `UserResponseBuilderTests`, `DeleteUserCommandHandlerTests`
  (Selbstlöschungs-Sperre, Aufrufreihenfolge
  Record→Label→Artist→Genre→Keycloak über NSubstitute-`Received.InOrder`,
  Verhalten bei fehlschlagendem `DeleteUserAsync`).
- `tests/MyMusic.IntegrationTests/TestSupport/KeycloakTestClient.cs`:
  Erweiterung um Rollenzuweisung für einen Testbenutzer (damit ein Token
  mit Admin-Rolle angefordert werden kann).
- Neu `tests/MyMusic.IntegrationTests/AdminEndpointsTests.cs`: 401 ohne
  Token, 403 mit Token ohne Admin-Rolle, 200 mit Admin-Rolle (Liste enthält
  angelegte Testbenutzer mit korrekter Rolle), Selbstlöschung → erwarteter
  Konfliktfehler, Löschen eines anderen Testbenutzers inkl. vorher
  angelegter App-Daten (z. B. ein Genre) → Benutzer verschwindet aus
  Keycloak-Liste, App-Daten sind weg (Repository-Check).
- Frontend: `admin.spec.ts`, `admin.service.spec.ts` (siehe oben).

## Doku-Updates

- `wiki/architektur/api-endpunkte.md`: neuer Abschnitt „Admin"
  (`GET /api/admin/users`, `DELETE /api/admin/users/{id}`).
- `wiki/architektur/application-layer.md`: Kategorie-Konvention um
  `Verwaltung` ergänzen.
- `wiki/sicherheit/sicherheitskonzept.md`: Rollenkonzept-Abschnitt
  ergänzen — serverseitige Durchsetzung jetzt implementiert (bisher nur
  als Zielbild beschrieben).
- `wiki/tech-stack/keycloak.md`: neuen Service-Account-Client und die
  Secret-Provisionierung beim API-Start dokumentieren.
- Neue ADRs:
  - `docs/adr/0015-serverseitige-rollenautorisierung.md` — Policy-basierte
    Rollenprüfung über den rohen `realm_access`-Claim statt
    `MapInboundClaims = true`.
  - `docs/adr/0016-keycloak-service-account-secret-provisionierung.md` —
    Keycloak-generiertes Secret + Auslesen beim Start via
    Bootstrap-Admin-Credentials, statt eines eigenen
    Aspire-Secret-Parameters.
- `TASK.md`: neue Zeile in Abschnitt 1 (User Stories Admin bereits
  2026-08-16 erledigt — bisher nicht vermerkt), neue Subsektion „### 7c.
  Admin-Bereich" nach dem Muster der anderen Blöcke, Arbeits-Prompt-
  Referenz.
- Dieses Plandokument wird nach Freigabe unverändert als
  `docs/prompts/2026-08-17-block-7c-admin-bereich.md` archiviert
  (TASK.md-Arbeitsregel).

## Ablauf nach Freigabe

1. Branch prüfen (`git branch --show-current`), Feature-Branch
   `block-7c-admin-bereich` von aktuellem `main` anlegen.
2. Plan als Arbeits-Prompt archivieren.
3. Umsetzung wie oben, Schritt für Schritt (Backend zuerst, dann Frontend
   darauf aufbauend).
4. `dotnet build`, `dotnet test` (inkl. Integrationstests gegen echten
   Aspire-Stack, PowerShell — nicht Git Bash, CLAUDE.md §11),
   `dotnet format --verify-no-changes`; `npm test`, `npm run build`,
   Prettier-Check im Frontend.
5. Live-Verifikation über den laufenden AppHost (Browser): Admin meldet
   sich an, sieht die Userliste, löscht einen Testbenutzer inkl. App-Daten;
   ein normaler Benutzer erreicht `/admin` nicht (Guard) und bekommt bei
   direktem API-Aufruf 403.
6. TASK.md/Wiki/ADR-Updates, Commit (Deutsch, echte Umlaute), Push und
   PR nur nach jeweils separater Freigabe.

## Verifikation

- Alle neuen und bestehenden Backend- und Frontend-Tests grün.
- Manuelle Live-Prüfung: `GET /api/admin/users` mit Admin-Token zeigt
  korrekte Rollen; Aufruf ohne Token → 401; mit User-Token → 403;
  Löschen eines Testbenutzers entfernt sowohl dessen App-Daten als auch
  den Keycloak-Account; Selbstlöschung wird verweigert; im Browser zeigt
  `/admin` die Liste, Löschen-Icon fehlt bei der eigenen Zeile, Löschen
  öffnet ein Bestätigungsmodal und aktualisiert die Liste ohne Neuladen.
