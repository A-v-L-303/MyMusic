# Block 7g — Registrierung

## Kontext

TASK.md Abschnitt 7 hatte bisher keinen Punkt „Registrierung" — Benutzer wurden
ausschließlich manuell über die Keycloak-Admin-Konsole/`kcadm.sh` angelegt
(siehe Abschnitt 7b/7c). Auf Wunsch des Projektinhabers (Klärung 2026-08-20)
soll sich ein neuer Benutzer selbst registrieren können, für die anstehende
Live-Verifikation von Block 7c und für die App allgemein.

User Story (neu, `wiki/user-stories/user-stories-authentifizierung.md`,
US-AU9):

> Als neuer, noch nicht registrierter Benutzer möchte ich mich selbst ein
> Konto anlegen können, damit ich MyMusic ohne Umweg über einen Administrator
> nutzen kann.

Akzeptanzkriterien: Button „Registrieren" neben „Login" in der Kopfzeile
(nur wenn nicht angemeldet); leitet zu Keycloaks Registrierungsseite weiter;
dieselbe Gestaltung wie die Anmeldeseite (Block 7f, `mymusic`-Theme); nach
erfolgreicher Registrierung derselbe OIDC-Rücksprung wie beim Login; neue
Benutzer erhalten automatisch die Realm-Rolle `User`.

Mit dem Projektinhaber geklärt: Registrierung läuft ausschließlich über
Keycloak selbst — kein eigenes Angular-Formular gegen einen neuen
Backend-Endpunkt (folgt aus CLAUDE.md §1/§5.1, „Keycloak als alleiniger
Identity Provider"). E-Mail-Verifizierung und Passwort-Reset sind nicht Teil
dieses Blocks.

## Ist-Stand (verifiziert)

- `keycloak/mymusic-realm.json`, Zeile 6: `"registrationAllowed": false`.
- `keycloak/mymusic-realm.json`, Zeile 18–28: `roles.realm` enthält `User`
  und `Admin`, kein `defaultRole`-Eintrag — neue Benutzer bekämen ohne
  weitere Konfiguration keine Rolle automatisch zugewiesen.
- Kein SMTP-Server im Projekt konfiguriert (keine `smtpServer`-Angabe in der
  Realm-JSON) — E-Mail-Verifizierung ist damit technisch nicht sinnvoll
  aktivierbar, ohne das über diesen Block hinausgehend nachzuziehen.
- `src/frontend/src/app/nav/nav.ts`: bestehender Login-Mechanismus,
  `protected login(): void { this.oidcSecurityService.authorize(); }`
  (Zeile 71–73). `nav.html` zeigt im nicht angemeldeten Zustand
  ausschließlich den Button „Login" (`class="btn btn-secondary"`,
  `title="Anmelden"`, Zeile 80–84).
- `angular-auth-oidc-client` v21 (ADR 0010) bietet keinen dedizierten aufruf
  für die Registrierung, aber einen passenden Hook, empirisch im
  Bibliothekscode verifiziert:
  - `node_modules/angular-auth-oidc-client/types/angular-auth-oidc-client.d.ts`,
    Zeile 257–264: `AuthOptions.urlHandler(url: string)`.
  - `fesm2022/angular-auth-oidc-client.mjs`,
    `StandardLoginService.loginStandard`, Zeile 4944–4969: Bei gesetztem
    `urlHandler` baut die Bibliothek die Autorisierungs-URL normal
    (inkl. PKCE `code_challenge`, `state`, `nonce`, `createAuthorizeUrl`),
    ruft aber nicht selbst `redirectService.redirectTo(url)` auf, sondern
    übergibt die fertige URL an `urlHandler`.
  - Das Discovery-Dokument wird bei jedem `authorize()`-Aufruf frisch
    geladen (`AuthWellKnownService.queryAndStoreAuthWellKnownEndPoints`) —
    ein String-Replace auf der fertigen URL ist robust.
  - Keycloak akzeptiert an `{authority}/protocol/openid-connect/registrations`
    dieselben Query-Parameter wie an `{authority}/protocol/openid-connect/auth`,
    zeigt aber direkt das Registrierungsformular (so funktioniert auch
    `keycloak-js`s `register()` intern).
- Keycloak-Theme `mymusic` (Block 7f, ADR 0014): `template.ftl`
  (Marken-Header) und `mymusic.css` (u. a. `.pf-v5-c-login__container`,
  `.pf-v5-c-login__main`, `.pf-v5-c-title`, `.pf-v5-c-button.pf-m-primary`)
  gelten für alle Seiten des Theme-Typs `login`. `register.ftl` selbst kommt
  unverändert vom Parent-Theme `keycloak.v2` (MyMusic hat keine eigene
  `register.ftl`) — die Registrierungsseite müsste die MyMusic-Gestaltung
  damit automatisch übernehmen, ungeprüft bis zur Live-Verifikation.
- Keine automatisierten Tests decken Keycloak-Seiten-Rendering ab
  (Projektkonvention, siehe Block 0f/0g/7f) — Verifikation der
  Registrierungsseite selbst bleibt manuell/live.

## Entscheidungen mit Empfehlung

1. **Redirect via `urlHandler` + Pfad-Replace**
   (`/protocol/openid-connect/auth` → `/protocol/openid-connect/registrations`),
   keine PKCE-/State-Eigenimplementierung — nutzt ausschließlich die
   vorhandene Bibliotheksmechanik.
2. **Keine neue Keycloak-Theme-Datei einplanen.** Erst live prüfen, ob
   `register.ftl` (vom Parent-Theme) bereits korrekt gestylt ist. Nur bei
   nachgewiesener Lücke eine gezielte Ergänzung in `mymusic.css`.
3. **Default-Rolle `User` für neu registrierte Benutzer**: Die exakte
   JSON-Repräsentation von Keycloaks „Default Roles"-Mechanismus
   (`default-roles-mymusic`) ist in diesem Projekt noch nicht empirisch
   geprüft — anders als bei ADR 0014/0016 wird hier nicht aus der
   Erinnerung heraus eine JSON-Struktur behauptet. Vorgehen: Während der
   Live-Verifikation in der Keycloak-Admin-Konsole unter „Realm settings →
   User registration → Default roles" die Rolle `User` eintragen, danach
   die tatsächlich entstandene Struktur per `kcadm.sh get realms/mymusic`
   auslesen und diese (nicht eine geratene) in `mymusic-realm.json`
   übernehmen.
4. **Lokales `mymusic-keycloak-data`-Volume**: `--import-realm` läuft mit
   `IGNORE_EXISTING` (ADR 0014/0016) — `registrationAllowed` und die
   Default-Rolle wirken dort nicht automatisch. Einmaliger manueller
   Nachzug per `kcadm.sh update realms/mymusic -s registrationAllowed=true`
   und die Default-Rollen-Zuweisung über die Admin-Konsole, analog zum
   bisherigen Vorgehen.

## Schritte

1. Wiki-Vorarbeit (bereits erledigt vor diesem Arbeits-Prompt): US-AU9 in
   `user-stories-authentifizierung.md`, User-Bereich-Tabelle in
   `navigation-konzept.md`, Log-Eintrag.
2. Branch `block-7g-registrierung` von `main` (bereits angelegt).
3. `keycloak/mymusic-realm.json`: `"registrationAllowed": true` setzen.
4. Aspire-AppHost starten (PowerShell,
   `dotnet run --project src/MyMusic.AppHost`), Keycloak-Admin-Konsole
   öffnen, Default Roles um `User` ergänzen, per `kcadm.sh` die reale
   Struktur auslesen und in `mymusic-realm.json` übernehmen.
5. Lokales Datenvolume: `registrationAllowed` und Default-Rolle einmalig
   manuell nachziehen (siehe Entscheidung 4).
6. `src/frontend/src/app/nav/nav.ts`: neue Methode `register()`:
   ```ts
   protected register(): void {
     this.oidcSecurityService.authorize(undefined, {
       urlHandler: (url) => {
         window.location.href = url.replace(
           '/protocol/openid-connect/auth',
           '/protocol/openid-connect/registrations',
         );
       },
     });
   }
   ```
7. `src/frontend/src/app/nav/nav.html`: im `@else`-Zweig (nicht angemeldet)
   zusätzlichen Button vor „Login" — `class="btn btn-secondary"`,
   `title="Registrieren"`, Text „Registrieren", `(click)="register()"`.
8. `src/frontend/src/app/nav/nav.spec.ts`: neuer Test analog zum
   bestehenden Login-Test (Zeile 248) — prüft, dass `authorize` mit einem
   `urlHandler` aufgerufen wird und dieser Callback
   `/protocol/openid-connect/auth` durch
   `/protocol/openid-connect/registrations` ersetzt.
9. `docs/adr/0010-angular-oidc-bibliothek.md`: neuer
   „Nachtrag (Registrierung)"-Abschnitt mit dem `urlHandler`-Fund.
10. `TASK.md`: neuer Unterabschnitt 7g mit Status und Ergebnissen nach
    Abschluss.
11. Tests: `npm test`, `npm run build` im Frontend-Workspace.
12. Live-Verifikation (siehe Tabelle unten).

## Verifikation

| Schritt | Prüfung |
|---|---|
| `npm test` | Neuer + bestehende Nav-Tests grün |
| `npm run build` | Production-Build grün |
| Aspire-AppHost starten | Kein Fehler beim Keycloak-Start |
| Klick auf „Registrieren" | Weiterleitung zu Keycloaks Registrierungsseite, nicht zur Login-Seite |
| Registrierungsseite optisch prüfen | Gleiches Design wie Anmeldeseite (Marke, Farben, Typografie) |
| Neues Konto anlegen | Rücksprung in die App im angemeldeten Zustand |
| `GET /api/me` | Liefert die `userId` des neuen Kontos |
| CRUD-Aufruf (z. B. Genre anlegen) | Gelingt — bestätigt automatische `User`-Rollenzuweisung |

## Risiken und offene Punkte

- Die exakte `defaultRole`-JSON-Struktur ist bis zur Live-Verifikation
  unbekannt (siehe Entscheidung 3) — kein Blocker, aber der erste
  Implementierungsschritt mit echtem Unsicherheitsfaktor.
- Laut TASK.md Abschnitt 7c hing der Aspire-AppHost beim letzten Merge
  reproduzierbar beim Start von Postgres/Keycloak; Ursache ungeklärt. Falls
  das weiterhin auftritt, blockiert es die Live-Verifikation dieses Blocks
  genauso wie die von Block 7c.
- Falls `register.ftl` vom Parent-Theme optisch von der Anmeldeseite
  abweicht, ist eine kleine, gezielte `mymusic.css`-Ergänzung nötig
  (kein neuer ADR, fällt unter ADR 0014).
- Kein automatisierter Test für das Keycloak-Seiten-Rendering
  (Projektkonvention, kein neues Risiko).
- E-Mail-Verifizierung bleibt deaktiviert (kein SMTP-Server) — neu
  registrierte Konten sind sofort nutzbar, ohne Verifizierungsschritt.
