# Block 7k — Benutzerprofil

## Kontext

`wiki/architektur/navigation-konzept.md` beschreibt seit dem 2026-08-13 ein
Benutzerprofil-Modal (Klick auf den Username in der Kopfzeile öffnet ein
Modal mit Benutzername schreibgeschützt, E-Mail änderbar, Passwort ändern).
Bei Block 0g wurde es explizit aus dem Scope genommen ("Kein
Benutzerprofil-Modal — Username bleibt reiner Text ohne Klick-Handler, wie
bisher"), aber danach nie wieder aufgegriffen: Es gab keine User-Story-Seite
dafür und es tauchte nicht in `TASK.md` unter "Aktuell nicht umgesetzt" auf.
Die Funktion wurde dadurch vollständig aus der weiteren Planung vergessen,
nicht bewusst zurückgestellt. Dieser Block holt sie nach.

**Mit dem Projektinhaber geklärter Scope** (siehe Wiki
`wiki/user-stories/user-stories-benutzerprofil.md`, "Geklärt mit dem
Projektinhaber am 2026-08-28"):

- E-Mail- und Passwortänderung verlangen **keine** erneute Bestätigung des
  aktuellen Passworts — die aktive Sitzung genügt (technisch identisch zum
  bereits bestehenden Admin-Reset über den Service-Account, hier nur auf den
  eigenen Account beschränkt).
- Keine E-Mail-Verifizierung (kein SMTP im Projekt konfiguriert, konsistent
  mit der bereits bei der Registrierung getroffenen Entscheidung).
- Benutzername bleibt read-only, kein Self-Service-Account-Delete.

## Ist-Stand (verifiziert)

- `GET /api/me` existiert bereits (`src/MyMusic.Api/Endpoints/System/CurrentUser/MeEndpoints.cs`,
  Feature-Ordner `src/MyMusic.Application/Features/System/CurrentUser/`,
  aktuell nur `Queries/GetCurrentUser/` mit `CurrentUserResponse(Guid UserId)`).
  `ICurrentUserService.UserId` (`src/MyMusic.Api/CurrentUserService.cs`) ist
  der etablierte Weg, die eigene UserId aus dem JWT-`sub`-Claim zu ermitteln.
- `IKeycloakAdminClient`/`KeycloakAdminClient` (`src/MyMusic.Application/Common/Services/IKeycloakAdminClient.cs`,
  `src/MyMusic.Infrastructure/ExternalServices/Keycloak/KeycloakAdminClient.cs`)
  haben aktuell nur `GetUsersAsync`/`DeleteUserAsync`. Der Service-Account
  `mymusic-admin-service` hat bereits die Keycloak-Client-Rollen
  `view-users`, `query-users`, `manage-users` (`keycloak/mymusic-realm.json`)
  — das deckt sowohl User-Update (E-Mail) als auch Passwort-Reset ab. Keine
  Keycloak-Realm-Änderung nötig.
- `keycloak/mymusic-realm.json`: `duplicateEmailsAllowed: false` (E-Mail-
  Konflikt ist ein reales Fehlerszenario, Keycloak liefert dafür HTTP 409 —
  vor der finalen Implementierung des `catch`-Filters live gegenprüfen), kein
  `passwordPolicy`-Eintrag (keine serverseitige Mindestlänge durch Keycloak
  selbst).
- `ExceptionManager.Conflict(message)` existiert bereits generisch (genutzt
  z. B. in `DeleteUserCommandHandler` zur Selbstlöschungs-Sperre) — keine
  neue Exception-Klasse für "E-Mail bereits vergeben" nötig.
- Fehler-Übersetzungsmuster für externe Services:
  `Features/Integration/Discogs/` — `try/catch` im Handler mit `when`-Filter
  auf den echten `HttpRequestException.StatusCode`, danach `throw
  exceptionManager.Xyz()`.
- **Wichtig**: `src/frontend/src/app/shared/error-modal/error-modal.service.ts`
  mappt HTTP 502 fest auf die Discogs-Fehlermeldung (`kind: 'discogs'`).
  Deshalb darf für "Keycloak nicht erreichbar" **keine neue Exception mit
  502-Mapping** eingeführt werden — sonst zeigt die UI bei einem
  Keycloak-Ausfall fälschlich den Discogs-Text. Alles außer 409 bleibt im
  neuen Handler unbehandelt und fällt auf den bestehenden generischen
  500-Fall (`kind: 'server'`) zurück.
- Referenz-Feature `Verwaltung/Admin` zeigt das Command-Muster
  (`sealed class : ICommand<bool>` mit mutablen Properties, z. B.
  `UpdateGenreCommand.cs`), DI-Registrierung (Handler/Validator per
  Assembly-Scan, kein manueller Eintrag nötig außer für `ResponseBuilder` —
  hier nicht gebraucht) und `GlobalUsing.cs`-Muster (Zeilen 71–75).
- Frontend: `shared/modal/modal.ts` (`app-modal`, generisches Scrim+Panel mit
  Escape-Stack) wird von jeder Feature-Modal-Komponente selbst umschlossen.
  `features/genres/genre-form/genre-form.ts` ist das Referenzmuster für
  Signal Forms (`form()`/`[formField]`, `submit()` mit 400 inline über
  `FieldTree`, alles andere über `errorModalService.showFromHttpError(error,
  entityName)`). `features/admin/admin.service.ts` ist das Referenzmuster
  für einen HTTP-Service.
- `src/frontend/src/app/nav/nav.ts`: `OidcUserClaims`-Interface ist lokal
  definiert und deklariert bisher nur `preferred_username`. Der OIDC-Scope
  ist `'openid profile email'` (`core/auth/keycloak-config.factory.ts`), das
  `email`-Claim sollte also im ID-Token stecken, wurde aber bisher nirgends
  gelesen — vor der Implementierung live verifizieren
  (`oidcSecurityService.userData()` im Browser inspizieren).
- `nav.html`: Der Username ist aktuell ein reines `<span>`, kein
  Klick-Handler vorhanden.
- Kein bestehender `UserProfile`/`Profile`/`Account`-Code irgendwo im
  Frontend, kein bestehender Email-/Passwort-Validator im Backend (die
  Registrierung läuft komplett über Keycloaks eigene UI, kein eigenes
  Formular gegen einen MyMusic-Endpoint).
- ADRs waren lückenlos 0001–0025 durchnummeriert, nächste freie Nummer: 0026.

## Vorgeschlagene Schritte

### 1. Backend: `IKeycloakAdminClient` erweitern

`IKeycloakAdminClient`: zwei neue Methoden `UpdateEmailAsync(Guid userId,
string newEmail, CancellationToken)` und `ResetPasswordAsync(Guid userId,
string newPassword, CancellationToken)`.

`KeycloakAdminClient`:
- `UpdateEmailAsync`: `GET /admin/realms/mymusic/users/{id}` als `JsonNode`
  lesen, nur die Eigenschaft `email` überschreiben, den vollständigen (nur
  minimal veränderten) JSON-Baum per `PUT` zurückschreiben — kein schmales
  DTO verwenden, damit kein anderes Keycloak-Feld (z. B. `enabled`) verloren
  geht.
- `ResetPasswordAsync`: `PUT /admin/realms/mymusic/users/{id}/reset-password`,
  Body `{ "type": "password", "value": newPassword, "temporary": false }` —
  `temporary: false` ist Pflicht, sonst erzwingt Keycloak beim nächsten
  Login eine weitere Passwortänderung.
- Beide Methoden übersetzen keine Statuscodes selbst (`EnsureSuccessStatusCode()`
  wie bisher) — die Übersetzung passiert im Application-Handler.

### 2. Backend: Feature-Slice `System/CurrentUser` erweitern

Neue Commands unter `Features/System/CurrentUser/Commands/` (nicht unter
`Verwaltung/Admin` — das ist fremdbezogen mit `"Admin"`-Policy):

- `UpdateEmail/UpdateCurrentUserEmailCommand.cs` (`Email`, `UserId`),
  `UpdateCurrentUserEmailCommandHandler.cs` (ruft `UpdateEmailAsync`; `catch
  (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Conflict)`
  → `throw exceptionManager.Conflict("Diese E-Mail-Adresse wird bereits von
  einem anderen Konto verwendet.")`),
  `UpdateCurrentUserEmailCommandValidator.cs` (`NotEmpty()`,
  `.EmailAddress()`, `.MaximumLength(255)`).
- `ChangePassword/ChangeCurrentUserPasswordCommand.cs` (`NewPassword`,
  `UserId`), `ChangeCurrentUserPasswordCommandHandler.cs` (ruft
  `ResetPasswordAsync`, kein Konfliktfall zu übersetzen),
  `ChangeCurrentUserPasswordCommandValidator.cs` (`NotEmpty()`,
  `.MinimumLength(8)`, `.MaximumLength(100)`).

Bekannte, bewusst nicht in diesem Block behobene Inkonsistenz: Registrierung
und Keycloak selbst erzwingen mangels `passwordPolicy` im Realm keine
Mindestlänge; nach diesem Block gilt lokal (nur beim Self-Service-Wechsel)
eine Mindestlänge von 8 Zeichen. Als Folgepunkt in `TASK.md` vermerken, nicht
hier miterledigen.

### 3. Backend: Endpunkte

`MeEndpoints.cs`: `group.MapPut("/email", ...)` und `group.MapPut("/password",
...)`, beide mit XML-`<summary>`, `command.UserId =
currentUserService.UserId` vor `mediator.SendAsync(...)` (nie aus dem Body),
Rückgabe `Results.NoContent()`. Gruppenweites `.RequireAuthorization()`
bleibt unverändert, keine `"Admin"`-Policy. `GlobalUsing.cs`: zwei neue
Zeilen für die neuen Commands.

`GET /api/me`/`CurrentUserResponse` bleiben unverändert.

### 4. Backend-Tests

- Handler-Tests (Erfolgsfall + Konfliktfall für E-Mail) und Validator-Tests
  für beide Commands in `tests/MyMusic.Application.Tests/Features/System/CurrentUser/Commands/`.
- `tests/MyMusic.IntegrationTests/MeProfileEndpointsTests.cs` (neu): 401 ohne
  Token auf beide Routen; E-Mail-Änderung mit Verifikation; Passwort-Änderung
  mit anschließendem Neu-Login über das geänderte Passwort als Beweis;
  Konfliktfall über einen zweiten Testuser.

### 5. Frontend

- `nav.ts`: `OidcUserClaims` um `email?: string` erweitern, `email =
  computed(...)` analog `username`, `profileModalOpen = signal(false)`,
  `openProfileModal()`/`closeProfileModal()`, `onEmailChanged()` →
  `oidcSecurityService.forceRefreshSession().subscribe()`.
- `nav.html`: Username-`<span>` wird `<button (click)="openProfileModal()">`;
  `@if (profileModalOpen()) { <app-user-profile [username]="name"
  [email]="email()" (closed)="closeProfileModal()"
  (emailChanged)="onEmailChanged()" /> }`.
- Neuer Ordner `src/app/nav/user-profile/` (co-located mit Nav, da
  ausschließlich von dort geöffnet — nicht `shared/` da nicht
  wiederverwendbar, nicht `features/` da keine eigene Route):
  - `user-profile.service.ts`/`.service.spec.ts` — `updateEmail(email)`/
    `changePassword(newPassword)` gegen `PUT /api/me/email`/`PUT
    /api/me/password`, Muster 1:1 `admin.service.ts`.
  - `user-profile.ts`/`.html`/`.spec.ts` — wrappt sich in `<app-modal>`; zwei
    unabhängige Signal-Forms (E-Mail; Passwort mit `newPassword` +
    `newPasswordConfirmation`, Cross-Field-Validierung für die
    Übereinstimmung). Fehlerbehandlung 1:1 nach `genre-form.ts`: 400 inline
    über `FieldTree`, alles andere (insbesondere 409) über
    `errorModalService.showFromHttpError(error, 'E-Mail-Adresse')`.
- `nav.spec.ts`: bestehende Tests an neuen Button anpassen, neuer Test
  "Klick auf Username öffnet Profil-Modal".

### 6. Dokumentation

Neuer ADR `docs/adr/0026-...md`: Entscheidung, die Self-Service-Änderung
über den bestehenden `mymusic-admin-service`-Client abzuwickeln statt
Keycloaks eigene Account-REST-API einzuführen (verworfener Alternativpfad,
siehe ADR 0014) — inkl. der Abwägung, dass die "nur eigener Account"-Grenze
ausschließlich im Anwendungscode (JWT-`sub`-Claim) durchgesetzt wird, nicht
durch einen von Keycloak selbst scope-begrenzten Token. `TASK.md`: neuer
Abschnitt "7k. Benutzerprofil", "Aktuell nicht umgesetzt"-Liste
aktualisieren, Passwort-Policy-Inkonsistenz als Folgepunkt vermerken.
`README.md`, root-`CLAUDE.md` (Stand-Absatz) falls einschlägig.

## Verifikation

1. `dotnet restore` / `dotnet build --no-restore` / `dotnet test --no-build`
   (Domain, Application, Api-Testprojekte) / `dotnet format
   --verify-no-changes`.
2. `dotnet test tests/MyMusic.IntegrationTests --filter
   "FullyQualifiedName~MeProfileEndpointsTests"`.
3. `ng lint` / `ng test --watch=false` in `src/frontend`.
4. Manuelle Live-Prüfung gegen den laufenden Aspire-AppHost: Login, Klick auf
   Username öffnet Modal mit korrektem Benutzernamen/E-Mail; E-Mail auf
   bereits vergebenen Wert ändern → Konfliktmeldung, ausdrücklich kein
   Discogs-Text; E-Mail auf gültigen neuen Wert ändern → Erfolg, Anzeige
   aktualisiert sich, Gegenprüfung in der Keycloak-Admin-Konsole; Passwort
   ändern (<8 Zeichen → Inline-Fehler; gültig → Erfolg), danach Logout und
   Login mit dem neuen Passwort; `PUT /api/me/email` und `PUT
   /api/me/password` ohne Token → 401.
