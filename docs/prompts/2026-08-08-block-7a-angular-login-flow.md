# Block 7a: Angular-Login-Flow (Keycloak, Authorization Code + PKCE)

## Kontext

Backend und Angular-Workspace (Block 0c) sind fertig, aber der Angular-Client kann
noch keine authentifizierten API-Aufrufe machen — es gibt weder Login-Flow noch
HTTP-Interceptor noch Route Guard. Alle Backend-Endpunkte verlangen bereits
`.RequireAuthorization()`; ohne diesen Block lässt sich keine der als Nächstes
geplanten Angular-Feature-Slices (Genre/Label/Artist/Record) überhaupt gegen die
echte API testen. Der Keycloak-Realm-Import enthält den dafür vorgesehenen
öffentlichen Client `mymusic-angular` bereits fertig konfiguriert
(Authorization Code + PKCE/S256, `redirectUris`/`webOrigins` auf
`http://localhost:4200`) — nur die Angular-seitige Umsetzung und die
AppHost-Verdrahtung dorthin fehlen.

Bewusst **nicht** Teil dieses Blocks: das Custom-Theme für die von Keycloak
gehostete Login-Seite im MyMusic-Design (eigener Folgeblock **7b**, da technisch
eigenständig — FreeMarker/CSS statt Angular/TypeScript, eigener Branch). In
diesem Block bleibt die Keycloak-Login-Seite im Standard-Theme.

Getroffene Entscheidungen (mit dem Projektinhaber abgestimmt):

- **Library**: `keycloak-angular` + `keycloak-js`. Verifiziert: `keycloak-angular@22.0.0`
  verlangt `@angular/core@^22`, `@angular/common@^22`, `@angular/router@^22`,
  `keycloak-js@^18...^26`, `rxjs@^7` — passt exakt zu Angular 22.1.x, keine
  zone.js-Abhängigkeit. `keycloak-js` in einer `^26.x`-Version (passend zum
  Keycloak-Server 26.5) separat als Dependency ergänzen (Peer Dependency wird
  nicht automatisch mitinstalliert).
- **Token-Storage**: nur im Speicher (In-Memory, keycloak-js-Standardverhalten),
  kein localStorage/sessionStorage. Beim Neuladen der Seite übernimmt ein
  verstecktes iframe (`onLoad: 'check-sso'` + `silentCheckSsoRedirectUri`) die
  stille Wiederherstellung der Session.
- **Login-Einstieg**: direkter Redirect zu Keycloak, keine eigene Angular
  `/login`-Route.
- Ausschließlich die moderne `keycloak-angular`-API verwenden
  (`provideKeycloak()`, `includeBearerTokenInterceptor` +
  `createInterceptorCondition`, funktionale Guards) — die alten Klassen
  `KeycloakService`/`KeycloakAuthGuard`/`KeycloakBearerInterceptor` sind seit
  v19 deprecated.
- Der Bearer-Interceptor wird per URL-Regex ausschließlich auf `apiBaseUrl`
  beschränkt — das Token darf niemals an Keycloak selbst oder andere Origins
  gehen.

## Wichtiger Architektur-Punkt: Reihenfolge Runtime-Config vs. Keycloak-Init

`provideKeycloak({ config: { url, realm, clientId } })` braucht die
Keycloak-URL bereits beim Aufbau des Provider-Arrays. Diese URL soll aber aus
`runtime-config.json` kommen (umgebungsabhängig, wie `apiBaseUrl`), die heute
erst **nach** dem Bootstrap per `provideAppInitializer()` geladen wird — das
ist eine echte Reihenfolge-Kollision, kein Detail, das sich "nebenbei" lösen
lässt.

**Lösung**: `main.ts` lädt `runtime-config.json` selbst *vor* dem Aufruf von
`bootstrapApplication()` und übergibt die geladene Config an eine Factory, die
daraus das `ApplicationConfig`-Array baut (inkl. `provideKeycloak(...)` und der
auf `apiBaseUrl` beschränkten Interceptor-Bedingung). Das ersetzt den
bisherigen `provideAppInitializer()`-Mechanismus aus ADR 0009 für
`RuntimeConfigService` — `RuntimeConfigService` wird dadurch simpler (hält nur
noch die bereits geladene Config, kein `load()` mehr nötig). ADR 0009 wird
durch einen neuen ADR-Eintrag ergänzt/präzisiert (siehe unten), nicht ersetzt
— die Grundidee (Werte zur Laufzeit statt zur Build-Zeit) bleibt gültig, nur
der Lademechanismus wandert vor den Bootstrap.

## Offene Design-Fragen — Entscheidung mit Begründung

1. **Verifikationsfläche im Browser**: Da `app.routes.ts` aktuell leer ist und
   keine Angular-Feature-Route existiert, gibt es sonst nichts zum
   Durchklicken. Ergänze die im Wiki (`navigation-konzept.md`) bereits
   festgelegte, minimale "User-Bereich"-Darstellung (Login-Button /
   Username + Logout-Button — sonst nichts, kein Logo/Suchfeld/Admin-Button,
   die bleiben einem künftigen Navigations-Block vorbehalten) direkt in
   `app.html`/`app.ts`. Nach erfolgreichem Login ruft die App einmalig
   `GET /api/me` auf (bestehender Smoke-Test-Endpunkt aus Block 0b) — das
   beweist Guard, Interceptor und Token-Fluss end-to-end an einem echten,
   bereits vorhandenen, authentifizierten Endpunkt.
2. **Guard-Strategie**: `app.routes.ts` bekommt eine Wurzel-Route mit
   `canActivateChild: [authGuard]` und einem (noch leeren) `children`-Array —
   jede künftige Feature-Route hängt sich automatisch darunter ein, ohne dass
   jeder Feature-Block den Guard selbst verdrahten muss. Da es noch keine
   echte Kind-Route gibt, wird der Guard selbst per Unit-Test abgesichert; die
   Login/Logout/Interceptor-Kette wird stattdessen über die User-Bereich-UI
   (Punkt 1) end-to-end im Browser geprüft.
3. **`onLoad`-Strategie**: `check-sso` (still, App rendert immer, Guard
   entscheidet pro Route) statt `login-required` (erzwingt sofortigen
   Redirect vor jedem Rendern) — das Wiki (`navigation-konzept.md`) zeigt
   explizit einen "nicht eingeloggt"-Header-Zustand mit Login-Button, die App
   muss also ungeloggt zumindest kurz sichtbar/nutzbar sein.
4. **Runtime-Config-Erweiterung**: nur `keycloakUrl: string` (Basis-URL, z. B.
   `http://localhost:8080`) fließt umgebungsabhängig durch den bestehenden
   Mechanismus (neue AppHost-Env-Var `MYMUSIC_KEYCLOAK_URL`, analog
   `MYMUSIC_API_BASE_URL`). `realm` (`mymusic`) und `clientId`
   (`mymusic-angular`) werden als TS-Konstanten hart hinterlegt — das sind
   feste Bezeichner des bereits im Realm-Import fixierten Clients, keine
   Umgebungsvariable im eigentlichen Sinn; `keycloak-js` verlangt sie ohnehin
   als getrennte Felder, keine kombinierte Issuer-URL wie beim Backend.
5. **AppHost-Port-Pinning**: Der `frontend`-Ressource fehlt heute ein fester
   Port — sie bekommt nur `.WithHttpEndpoint(env: "PORT")` ohne Portnummer,
   obwohl der Keycloak-Client bereits fest auf `http://localhost:4200`
   registriert ist. Muss auf `.WithHttpEndpoint(port: 4200, env: "PORT")`
   gepinnt werden — exakt dieselbe Begründung, die im Wiki bereits für die
   festen Keycloak-Ports dokumentiert ist (sonst funktionieren die
   `redirectUris` nicht zuverlässig).
6. **`silent-check-sso.html`**: statische Datei unter
   `src/frontend/public/silent-check-sso.html`, keine Variablen-Ersetzung
   nötig, kein Eingriff in `write-runtime-config.mjs` erforderlich (reiner
   Passthrough, wird nur von Keycloak-JS im versteckten iframe geladen).
7. **Doku-Lücken schließen**: `sicherheitskonzept.md` dokumentiert bisher
   weder Token-Storage noch Silent-Refresh noch Logout-Mechanik — wird um
   die hier getroffenen Entscheidungen ergänzt. Neuer ADR (nächste freie
   Nummer nach 0009) für die zwei Entscheidungen mit echten verworfenen
   Alternativen: Library-Wahl (`keycloak-angular` vs. `angular-oauth2-oidc`
   vs. Eigenbau) und Token-Storage (In-Memory vs. session-/localStorage).

## Zu ändernde/neue Dateien

**Neu** (`src/frontend/src/app/core/auth/`, analog zur bestehenden
`core/runtime-config/`-Struktur):
- `keycloak.config.ts` — Konstanten `KEYCLOAK_REALM`/`KEYCLOAK_CLIENT_ID` und
  Hilfsfunktion, die aus der geladenen `RuntimeConfig` das `provideKeycloak()`-
  Config-Objekt baut.
- `auth.guard.ts` + `.spec.ts` — funktionaler `CanActivateChildFn`; prüft
  Authentifizierungsstatus, löst bei Bedarf `keycloak.login({ redirectUri })`
  mit der ursprünglich angeforderten URL aus (erfüllt die im Wiki
  (`ui-ux-konzept.md`) verlangte Rückkehr zur zuletzt besuchten Seite).
- `current-user.service.ts` + `.spec.ts` — dünner Wrapper um die
  `keycloak-angular`-Injectables: `isAuthenticated`-Signal,
  `username`-Signal (aus `preferred_username`-Claim), `login()`, `logout()`
  (mit `redirectUri` passend zu `post.logout.redirect.uris`).
- `public/silent-check-sso.html` — Standard-Keycloak-JS-Silent-Check-Seite.

**Ändern**:
- `src/frontend/package.json` — `keycloak-angular` (`^22.0.0`) und
  `keycloak-js` (`^26.x`, genaue Version bei Umsetzung gegen npm prüfen) neu
  in `dependencies`.
- `src/frontend/src/main.ts` — Runtime-Config-Fetch vor
  `bootstrapApplication()` verschieben (siehe Architektur-Punkt oben).
- `src/frontend/src/app/app.config.ts` — wird zur Factory
  `buildAppConfig(runtimeConfig)`; ergänzt `provideHttpClient(withInterceptors([includeBearerTokenInterceptor]))`,
  `provideKeycloak(...)` und die auf `apiBaseUrl` beschränkte
  Interceptor-Bedingung.
- `src/frontend/src/app/core/runtime-config/runtime-config.service.ts` +
  `.spec.ts` — vereinfachen (kein `load()` mehr, Config kommt bereits fertig
  geladen rein); `RuntimeConfig`-Interface um `keycloakUrl: string` erweitern.
- `src/frontend/src/app/app.routes.ts` — Wurzel-Route mit
  `canActivateChild: [authGuard]`, leeres `children`-Array.
- `src/frontend/src/app/app.ts` / `app.html` — minimale User-Bereich-Anzeige
  (Login-Button / Username + Logout) plus einmaliger `/api/me`-Aufruf nach
  Login als Verifikationsanker.
- `src/frontend/scripts/write-runtime-config.mjs` — liest zusätzlich
  `MYMUSIC_KEYCLOAK_URL`.
- `src/frontend/public/runtime-config.json` — Fallback-Default um
  `"keycloakUrl": ""` ergänzen.
- `src/MyMusic.AppHost/AppHost.cs` — Frontend-Port auf 4200 pinnen,
  `.WithEnvironment("MYMUSIC_KEYCLOAK_URL", keycloak.GetEndpoint("http"))`
  und `.WaitFor(keycloak)` auf der `frontend`-Ressource ergänzen.
- `02 Wiki/MyMusic Wiki/wiki/sicherheit/sicherheitskonzept.md` —
  Token-Storage, Silent-Refresh, Logout-Mechanik nachtragen.
- `01 Repos/MyMusic/docs/adr/00XX-angular-keycloak-integration.md` — neu,
  siehe Punkt 7 oben.
- `01 Repos/MyMusic/TASK.md` — Abschnitt 7 nach Abschluss aktualisieren.

## Ablauf

1. Vor der ersten Änderung: `git branch --show-current` prüfen, Feature-Branch
   vom aktuellen `main` anlegen (Vorschlag: `block-7a-angular-login-flow`).
2. `keycloak-angular`/`keycloak-js` zu `package.json` hinzufügen, `npm install`.
3. `RuntimeConfig`/`main.ts`/`app.config.ts` wie oben umbauen.
4. `core/auth/` mit Guard, Service, Keycloak-Config anlegen; Unit-Tests dazu
   (Vitest, Muster `runtime-config.service.spec.ts` mit `vi.stubGlobal`).
5. `app.routes.ts` mit Guard-Wrapper, `app.html`/`app.ts` um User-Bereich plus
   `/api/me`-Ping ergänzen.
6. `public/silent-check-sso.html` anlegen.
7. `AppHost.cs` anpassen (Port-Pinning, Keycloak-Env-Var, `WaitFor`).
8. Wiki (`sicherheitskonzept.md`) und neuen ADR schreiben.
9. Verifikation (siehe unten), danach `TASK.md` aktualisieren.

## Verifikation

- `npm test` (Vitest) im Frontend — alle neuen/angepassten Specs grün.
- AppHost über PowerShell starten (CLAUDE.md §11 — zwingend PowerShell, nicht
  Git Bash). Prüfen: `frontend`-Ressource läuft fest auf Port 4200,
  Keycloak-Container auf 8080.
- Im Browser `http://localhost:4200` öffnen:
  - Ungeloggt: Header zeigt "Login"-Button.
  - Klick auf Login → Redirect zu Keycloaks gehosteter Login-Seite
    (`http://localhost:8080/realms/mymusic/...`, Standard-Theme).
  - Login mit einem existierenden Realm-Testbenutzer (prüfen, ob im
    Realm-Import bereits einer vorhanden ist — keine Zugangsdaten erfinden).
  - Redirect zurück zu `http://localhost:4200` (bzw. zuletzt besuchter
    Pfad), Header zeigt jetzt Username + Logout.
  - `/api/me`-Aufruf sichtbar erfolgreich (Response im UI oder Devtools);
    Network-Tab bestätigt `Authorization: Bearer ...`-Header nur bei
    Requests an die API, **nicht** bei Requests an Keycloak selbst.
  - Seite neu laden (F5) → Login bleibt über Silent-SSO-Check erhalten, kein
    sichtbarer Redirect-Flackerer.
  - Logout → zurück zu Keycloak-Logout und wieder zu `localhost:4200`, Header
    zeigt wieder Login-Button.
- Backend unverändert — keine erneute Backend-Testausführung nötig, da keine
  Backend-Dateien angefasst werden.

## Ergebnis (Live-Verifikation, 2026-08-08)

Alle oben genannten Verifikationsschritte erfolgreich durchlaufen (echter
Realm-Testbenutzer, vom Projektinhaber bereitgestellt). Ein Befund während der
Verifikation: `GET /api/me` scheitert am fehlenden CORS auf der API
(`OPTIONS /api/me` → 405) — bereits als separat offener Punkt in TASK.md
Abschnitt 7 geführt, kein neuer Fehler dieses Blocks, aber jetzt als Blocker
für künftige Angular-Feature-Anbindungen priorisiert. Details siehe TASK.md
Abschnitt 7a.
