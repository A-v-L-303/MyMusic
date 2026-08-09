# Block 7a — Angular-Login-Flow (Keycloak Authorization Code + PKCE)

## Kontext

Alle Backend-CRUD-Slices (Genre, Country, Label, Artist, Record/Tracks) sind
fertig, aber der Angular-Workspace (Block 0c) hat noch keinen Login-Flow,
keinen `AuthGuard`, keinen HTTP-Interceptor — jeder Endpunkt der API verlangt
aber bereits `.RequireAuthorization()`. Ohne diesen Block kann keine der
anstehenden Angular-CRUD-Masken (`genres/`, `labels/`, `artists/`,
`records/`) an ein gültiges Access Token kommen. Dieser Block liefert genau
die Voraussetzung dafür — nicht mehr.

Grundlage sind die im Wiki abgenommenen User Stories US-AU1–US-AU7
(`../../02 Wiki/MyMusic Wiki/wiki/user-stories/user-stories-authentifizierung.md`).
US-AU8 (Keycloak-Custom-Theme) ist bewusst **nicht** Teil dieses Blocks —
eigener, späterer Schritt.

In der Planungssitzung mit dem Projektinhaber entschieden:

- Eine minimale Development-CORS-Policy wird Teil dieses Blocks.
- OIDC-Bibliothek: `angular-auth-oidc-client` (Begründung: ADR 0010).
- Custom-Theme: eigener, späterer Block.

## Vorgeschlagene Schritte

### 1. Runtime-Config erweitern

`RuntimeConfig`-Interface um `keycloakAuthority` erweitern, `load()`
memoisieren (`RuntimeConfigService`). `write-runtime-config.mjs` liest
zusätzlich `MYMUSIC_KEYCLOAK_AUTHORITY`. `AppHost.cs` setzt diese Env-Var auf
der Frontend-Ressource und pinnt deren Port fest auf `4200` (analog zum
bestehenden Keycloak-Port-Pinning — der Realm-Client `mymusic-angular` hat
`redirectUris`/`webOrigins` hart auf `localhost:4200` hinterlegt, ein
dynamischer Port hätte den Login-Flow beim Start über den AppHost gebrochen).

### 2. `core/auth/`-Modul

- `keycloak-config.factory.ts`: Factory für
  `provideAuth({ loader: { provide: StsConfigLoader, useFactory: ... } } )`,
  baut aus `RuntimeConfigService` eine `OpenIdConfiguration` (`clientId:
  'mymusic-angular'`, `responseType: 'code'`, `useRefreshToken: true`,
  `silentRenew: true`, `secureRoutes: [apiBaseUrl]`, `scope: 'openid profile
  email'` bewusst ohne `offline_access`, siehe ADR 0010).
- `auth.guard.ts`: Re-Export von `autoLoginPartialRoutesGuard` als
  `authGuard`.
- `unauthorized-redirect.interceptor.ts`: projekteigener funktionaler
  Interceptor, leitet bei HTTP 401/403 von einer MyMusic-API-URL zur
  Anmeldung um (`oidcSecurityService.authorize()`), loggt nur im
  Development-Modus.

### 3. App-Verdrahtung

`app.config.ts`: `provideHttpClient(withInterceptors([
unauthorizedRedirectInterceptor, authInterceptor() ]))` sowie `provideAuth`.
`app.routes.ts`: `authGuard` auf allen Routen, inkl. einer für diesen Block
ergänzten Wildcard-Route (Nachweis von US-AU2 mit einer echten, von `/`
abweichenden Ziel-URL — temporär, entfällt mit den echten Feature-Routen).
`app.ts`/`app.html`: minimaler Login-/Logout-Button in der bestehenden
App-Shell (bewusst kein Vorgriff auf die echte `NavComponent`). Neue
Komponente `core/shell/home-placeholder/` ruft `GET /api/me` (bestehender
Block-0b-Endpoint) auf — konkreter Ende-zu-Ende-Nachweis für US-AU4.

### 4. Backend-CORS

`Program.cs`: Development-only CORS-Policy (`Uri.IsLoopback` — deckt jeden
`localhost`-Port ab, entspricht der in `sicherheitskonzept.md` entschiedenen
Dev-Policy), registriert vor `UseAuthentication()`. Production-Whitelist
bleibt offen.

## Benötigte npm-Pakete

`angular-auth-oidc-client` (siehe ADR 0010 für Begründung und
Alternativenvergleich).

## Sicherheitsanforderungen

- Kein Client Secret im Browser, kein Implicit Flow (Authorization Code +
  PKCE, bereits im Realm-Client `mymusic-angular` als `pkce.code.challenge.method:
  S256` hinterlegt).
- `scope` ohne `offline_access` (Bindung an SSO-Session-Grenzen erhalten).
- CORS nur in Development, beschränkt auf `localhost`-Origins.
- `keycloak/mymusic-realm.json` bleibt unverändert (ADR 0005: Auth-Code-only
  ist bewusst so).

## Verifikation

| Story | Automatisiert | Manuell |
|---|---|---|
| US-AU1–US-AU7 | Vitest-Specs je Modul (Factory, Guard-Nutzung über Routenstruktur, Interceptor, App-Buttons) | AppHost + manuell angelegter Keycloak-Testbenutzer, siehe Detailtabelle im Plan |
| CORS | `CorsPolicyTests.cs` (Preflight erlaubt/verweigert) | — |

Vor Abschluss: `dotnet build`, `dotnet format --verify-no-changes`,
Zeilenlängen-Check, `npm run build`, `npm test -- --watch=false`.

## Risiken und offene Punkte

- Wildcard-Route (`'**'`) ist eine bewusste, temporäre Abweichung von
  `angular-projektstruktur.md` — entfällt mit den echten Feature-Routen.
- Gewählter CORS-Mechanismus (`Uri.IsLoopback`) ist eine Implementierungswahl;
  das Wiki gibt nur die Policy vor, nicht den Mechanismus.
- Die zeitbasierten Kriterien (5-Minuten-Token-Ablauf, 8-Stunden-SSO-Grenze)
  sind nicht automatisiert prüfbar und bleiben manuelle Verifikationsschritte.

## Nachtrag nach Live-Verifikation (2026-08-09)

Die erste echte Prüfung im Browser (Aspire-AppHost, per Keycloak-Admin-REST-API
angelegter Testbenutzer) deckte eine Endlosschleife auf: `autoLoginPartialRoutesGuard`
verarbeitet den OIDC-Callback nicht selbst, sondern prüft nur vorhandene Tokens
im Storage. Ohne das Bibliotheks-Feature `withAppInitializerAuthCheck()` wurde
nie ein Token angefordert, der Guard sah dauerhaft „nicht angemeldet" und löste
bei jedem Rücksprung erneut `authorize()` aus. Alle bisherigen automatisierten
Tests hatten `OidcSecurityService` gemockt und diese Lücke deshalb nicht
erkannt — ein Beleg dafür, dass die manuelle Verifikationsspalte der
Story-Tabelle oben nicht optional ist. Fix: `withAppInitializerAuthCheck()`
zu `provideAuth(...)` in `app.config.ts` ergänzt (samt Kommentar). Danach
live erneut geprüft und bestätigt: echtes Login-Formular, Rückkehr zur
ursprünglich aufgerufenen URL, korrekte `userId` über `GET /api/me`, Logout
beendet die reale Keycloak-Session. Details siehe ADR 0010, Nachtrag.

Zweiter Nachtrag: Die Kopfzeile zeigte entgegen `navigation-konzept.md`
(„User-Bereich") bei Anmeldung nur einen nackten Logout-Button statt
`[Username]` + `[Logout-Button]` — eine bewusst minimal gehaltene, aber nicht
klar genug kommunizierte Abweichung, auf die der Projektinhaber hinwies.
Ergänzt: `App` liest `OidcSecurityService.userData()` und zeigt
`preferred_username` neben dem Logout-Button; live mit einem frischen
Testbenutzer bestätigt. Profil-Modal (Klick auf den Namen) bleibt bewusst
offen für die echte `NavComponent`.
