# ADR 0010 — OIDC-Bibliothek für den Angular-Login-Flow

**Status**: Angenommen
**Datum**: 2026-08-09
**Betrifft**: `src/frontend`

## Kontext

Block 7a liefert den Angular-seitigen Login-Flow gegen Keycloak 26.5
(Authorization Code + PKCE, kein Client Secret, kein Implicit Flow — CLAUDE.md
§5.1, Wiki `sicherheitskonzept.md`). Bisher ist im Angular-Workspace keine
OIDC-/PKCE-Client-Bibliothek installiert. CLAUDE.md §12 verlangt vor jedem
neuen Paket eine Begründung des Bedarfs, eine Prüfung vorhandener Lösungen,
eine Bewertung von Wartungsstatus/Lizenz/Sicherheit sowie mindestens eine
genannte Alternative.

Ein Eigenbau des PKCE-Flusses (Code-Verifier/Challenge über die Web Crypto
API, Token-Austausch über `fetch`) wurde geprüft und verworfen: Refresh-Token-
Handling, stille Erneuerung und die diversen Fehlerfälle eines OIDC-Flows
selbst korrekt zu implementieren und zu testen, ist bei einem
sicherheitskritischen Baustein ein größeres Risiko als eine geprüfte, weit
verbreitete Bibliothek zu verwenden — eine vorhandene Lösung war damit zu
bevorzugen.

## Entscheidung

`angular-auth-oidc-client`, Version **21.0.2** (zum Zeitpunkt der Umsetzung
aktuell; Peer-Dependency für `@angular/core`/`@angular/common`/`@angular/router`
ist `>=20.0.0` und deckt Angular 22.1.0 ohne Konflikt ab — per
`npm install`/`npm list` in dieser Sitzung geprüft).

## Begründung

- **Angular-natives API**: Konfiguration über `provideAuth()` (Environment
  Providers, passt zur bestehenden `ApplicationConfig`-Struktur in
  `app.config.ts`), `OidcSecurityService` exponiert `authenticated`/`userData`
  bereits als Angular Signals (nicht nur als Observables) — passt zu CLAUDE.md
  §8 (`inject()`, State per Signals). Fertiger Guard
  (`autoLoginPartialRoutesGuard`) und fertiger funktionaler Interceptor
  (`authInterceptor()`) decken US-AU2/US-AU3/US-AU4 aus
  `wiki/user-stories/user-stories-authentifizierung.md` ohne Eigenbau ab.
- **Asynchrone Konfiguration**: Die Keycloak-Authority ist erst zur Laufzeit
  aus `runtime-config.json` bekannt (ADR 0009). `angular-auth-oidc-client`
  unterstützt das nativ über `StsConfigLoader`/`StsConfigHttpLoader`
  (`provideAuth({ loader: { provide: StsConfigLoader, useFactory: ... } })`,
  akzeptiert ein `Observable<OpenIdConfiguration>`) — kein Workaround nötig.
- **Alternative `angular-oauth2-oidc`** (aktuell Version 22.0.2, Peer-Dependency
  `>=22.0.0` — enger an Angular 22 gebunden als die gewählte Bibliothek):
  generischere, ältere OAuth2/OIDC-Implementierung ohne natives Signal-API;
  Guard/Interceptor müssten selbst stärker verdrahtet werden. Kein technischer
  Blocker, aber mehr Eigencode für denselben Funktionsumfang.
- **Alternative `keycloak-angular`/`keycloak-js`**: direkteste Kopplung an
  Keycloak selbst. Gegen diese Wahl spricht eine reale Wartungsunsicherheit:
  `keycloak-js` wurde aus dem Haupt-Keycloak-Repository in ein eigenständiges
  Projekt ausgegliedert; das Keycloak-Team selbst prüft offen, ob der
  JavaScript-Adapter langfristig weitergeführt oder durch eine reife
  Drittbibliothek ersetzt wird (Quelle: keycloak.org-Blogeinträge zur
  Adapter-Deprecation 2022/2023, GitHub-Diskussion
  `keycloak/keycloak#11975`). Für ein sicherheitskritisches Modul mit
  unklarer langfristiger Pflege war das ein Ausschlusskriterium, nicht nur ein
  Nachteil.
- **`scope` bewusst ohne `offline_access`**: Die Realm-Konfiguration
  (`keycloak/mymusic-realm.json`) definiert `ssoSessionIdleTimeout: 1800` und
  `ssoSessionMaxLifespan: 28800` (30 Min. Sliding / 8 h Hard-Cap, siehe Wiki
  `sicherheitskonzept.md`). Der Scope `offline_access` würde bei Keycloak
  einen Offline-Token erzeugen, der nicht an diese SSO-Session-Grenzen
  gebunden ist. Ohne `offline_access` bleibt der Refresh Token ein regulärer
  „Online"-Token und damit an die dokumentierten Zeiten gebunden — technisch
  unabhängig von der Bibliothekswahl, aber nur bewusst korrekt, wenn der Scope
  explizit gesetzt wird (`core/auth/keycloak-config.factory.ts`).

## Konsequenzen

- Neue Laufzeitabhängigkeit `angular-auth-oidc-client` in
  `src/frontend/package.json`.
- Künftige Angular-Major-Updates müssen die Peer-Dependency dieser Bibliothek
  mitprüfen (`npm view angular-auth-oidc-client peerDependencies`).
- Guard, Interceptor und Konfigurations-Factory liegen gebündelt unter
  `src/frontend/src/app/core/auth/` — projekteigene Importpfade
  (`auth.guard.ts` re-exportiert `authGuard`) halten den Bibliotheksnamen aus
  dem übrigen Anwendungscode heraus, ein späterer Bibliothekswechsel bliebe
  lokal auf dieses Modul begrenzt.

## Nachtrag (2026-08-09): Endlosschleife durch fehlenden App-Initializer

Bei der ersten echten Live-Verifikation im Browser (Aspire-AppHost, echter
Keycloak-Testbenutzer) zeigte sich eine Endlosschleife: Der Browser wurde
nach jedem Rücksprung von Keycloak sofort wieder zu Keycloak umgeleitet, ohne
dass jemals ein Token-Austausch (`POST .../protocol/openid-connect/token`)
stattfand. Automatisierte Tests hatten das nicht erkannt, weil
`OidcSecurityService` in allen bisherigen Unit-Tests durchgängig gemockt war
— der reale Zusammenspiel von Guard und Bibliothek wurde nie geprüft.

**Ursache**: `autoLoginPartialRoutesGuard` verarbeitet den OIDC-Callback
(`code`/`state`-Parameter) selbst **nicht** — er prüft ausschließlich per
`authStateService.areAuthStorageTokensValid(...)`, ob bereits gültige Tokens
im Storage liegen (Bibliothekscode,
`node_modules/angular-auth-oidc-client/fesm2022/angular-auth-oidc-client.mjs`,
Funktion `checkAuth` ab Zeile 5763). Ohne einen expliziten Aufruf von
`OidcSecurityService.checkAuth()`/`checkAuthMultiple()` wird der Code nie
gegen ein Token eingetauscht — der Guard sieht dauerhaft „nicht angemeldet"
und ruft bei jeder Routenaktivierung erneut `authorize()` auf.

**Fix**: `provideAuth(config, withAppInitializerAuthCheck())` — die
Bibliothek bietet dafür ein eigenes `AuthFeature`, das einen `APP_INITIALIZER`
registriert, der `checkAuthMultiple()` vor dem Start der Anwendung aufruft
(Bibliothekscode, Zeile 5633 ff., Docstring: „replaces the need to manually
call `OidcSecurityService.checkAuth(...)`"). Ergänzt in `app.config.ts` samt
erklärendem Kommentar, da der Zusammenhang aus der Bibliotheks-API nicht
offensichtlich ist.

**Live erneut verifiziert** nach dem Fix (Aspire-AppHost, Testbenutzer
`block7a-manueller-test`, per Keycloak-Admin-REST-API angelegt): Login mit
echtem Formular, Rückkehr zur ursprünglich aufgerufenen URL (`/records/42`,
nicht `/`), `GET /api/me` liefert die korrekte, mit dem Testbenutzer
übereinstimmende `userId`, Logout beendet die reale Keycloak-Session (danach
wieder echtes Login-Formular statt stillem Durchschlupf). US-AU5/US-AU6
(zeitbasierte Token-Erneuerung/-Ablauf) bleiben mangels praktikabler
Wartezeit in dieser Sitzung nur durch die bestehenden Unit-Tests des
`unauthorized-redirect.interceptor.ts` abgedeckt, nicht live nachgewiesen.
