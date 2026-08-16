# Block 7b — Rollenkonzept User/Admin im Angular-Code (AdminGuard, Admin-Button)

## Kontext

TASK.md Abschnitt 7 listet als nächsten offenen Punkt „Rollen (`User`,
`Admin`) im Angular-Code (`AdminGuard`, Admin-Tab)" — ausdrücklich als
Vorstufe für den später folgenden vollen Admin-Bereich (Benutzerliste/
-löschung über die Keycloak Admin REST API) und die Swagger-Freischaltung für
die Admin-Rolle in Production, beides eigene, spätere Blöcke. Block 6
(Record/Tracks) ist vollständig abgeschlossen, Block 7a (Login-Flow) und 7f
(Keycloak-Theme) ebenfalls — dieser Block ist damit laut TASK.md-Reihenfolge
der nächste fachliche Schritt.

Die Keycloak-Realm-Rollen `User` und `Admin` existieren bereits im
Realm-Import (`keycloak/mymusic-realm.json`), werden aber aktuell nirgends im
Code gelesen. Backend-seitig ist für diesen Block **keine Änderung nötig**:
Die Ownership-Prüfung (404 statt 403) ist pro CRUD-Slice bereits serverseitig
umgesetzt, TASK.md grenzt diesen Punkt ausdrücklich auf „im Angular-Code"
ein.

Ziel dieses Blocks: Ein wiederverwendbarer Rollen-Baustein
(`UserRolesService`), ein `AdminGuard`, ein Admin-Button in der Kopfzeile
(sichtbar nur mit der Rolle `Admin`) und eine inhaltlich leere
`/admin`-Platzhalterroute — exakt nach der bereits im Wiki
(`architektur/navigation-konzept.md`) festgelegten Vorgabe. Der eigentliche
Admin-Bereich-Inhalt bleibt bewusst ein späterer Block.

### Vorgabe aus dem Wiki (`architektur/navigation-konzept.md`)

- Admin ist ein **Button, kein Tab** — rechts in der Kopfzeile, vor dem
  User-Bereich (Reihenfolge: Suchfeld · Theme-Toggle · „Admin" · Username ·
  Logout).
- Wird per `AdminGuard` ausgeblendet, wenn der Benutzer die Rolle nicht
  besitzt; Guard liest die Rolle aus dem JWT/Token — **kein zusätzlicher
  Backend-Call**.
- Label only, kein Icon.
- Route `/admin` → Platzhalter-Komponente, geschützt durch `AdminGuard`.

**Wichtige Schreibweisen-Klarstellung:** Die Wiki-Prosa schreibt gelegentlich
„admin" klein, der tatsächliche Realm-Import und
`sicherheit/sicherheitskonzept.md` definieren die Rolle groß als `Admin`. Der
Code prüft exakt gegen `Admin` (Großschreibung), da das der technische
Claim-Wert im Token ist.

## Ist-Stand (verifiziert)

- `keycloak/mymusic-realm.json` (Zeilen 18–29): Realm-Rollen `User`/`Admin`
  bereits als `realmRoles` definiert. Kein `defaultRoles`-Eintrag, kein
  Testbenutzer im Import, keine Rollenzuweisung vorhanden.
- `src/MyMusic.Api/Program.cs` (Zeilen 41–57): `AddJwtBearer` mit
  `MapInboundClaims = false`, `ValidAudience = "account"`, `AddAuthorization()`
  ohne benannte Policies. `ICurrentUserService`/`CurrentUserService` liest
  ausschließlich `sub`, keine Rollen. Keine `[Authorize(Roles=...)]`-Nutzung
  im gesamten `src`-Baum.
- `src/frontend/src/app/core/auth/auth.guard.ts`: reiner Re-Export von
  `autoLoginPartialRoutesGuard` — kein Rollenbezug, kein bestehendes Muster
  für einen funktionalen Custom-Guard im Projekt.
- Einzige `userData()`-Verwendung im gesamten Frontend:
  `src/frontend/src/app/nav/nav.ts` (Zeilen 19–21, 46–49), liest bisher nur
  `preferred_username`. Ob `realm_access.roles` im ID-Token/`userData()`
  enthalten ist, ist aus dem Code nicht ablesbar (hängt von der
  Keycloak-Protocol-Mapper-Konfiguration ab).
- `angular-auth-oidc-client` (installierte Version, geprüft im
  Bibliothekscode `node_modules/angular-auth-oidc-client/fesm2022/
  angular-auth-oidc-client.mjs`): Default `autoUserInfo: true`, von
  `keycloak-config.factory.ts` nicht überschrieben. Im Code-Flow führt das zu
  einem `GET` gegen den Keycloak-`/userinfo`-Endpunkt, dessen Ergebnis
  `oidcSecurityService.userData()` liefert. `getPayloadFromAccessToken()`
  (Zeile 5401–5406) dekodiert stattdessen synchron den rohen, aktuell im
  Storage liegenden Access Token.
- `src/frontend/src/app/app.routes.ts` (39 Zeilen): äußere Route mit
  `canActivate: [authGuard]`, sechs `loadChildren`-Kinder (dashboard,
  records, artists, labels, genres, search), `''` und `'**'` redirecten auf
  `/dashboard`. Kein `admin`-Pfad.
- `src/frontend/src/app/nav/nav.html` (Zeile 68–82): Container
  `<div class="ml-auto flex items-center gap-3">` mit `<app-theme-toggle />`
  (Zeile 69), danach bedingt Username+Logout oder Login-Button. `nav.ts`
  injiziert `OidcSecurityService`/`Router` per `inject()`.
- `src/frontend/src/app/features/dashboard/` ist das Referenzmuster für
  Platzhalter-Komponenten: `dashboard.ts` (`@Component({ selector:
  'app-dashboard', templateUrl: './dashboard.html' }) export class
  Dashboard {}`), `dashboard.html` (`<section class="p-6"><h1
  class="text-lg font-bold text-fg">Dashboard</h1><p class="empty">Diese
  Ansicht folgt in einem späteren Block.</p></section>`).
- Frontend-Testzahl aktuell 353 (TASK.md-Stand 2026-08-15).

## Vorgeschlagene Schritte

### 1. `UserRolesService` (`src/frontend/src/app/core/auth/user-roles.service.ts`)

Liest die Realm-Rollen aus `OidcSecurityService.userData()` (analog zum
bestehenden `preferred_username`-Zugriff in `nav.ts`) und exponiert `roles`
sowie `isAdmin` als Signals (`computed`). Guard und Nav-Button greifen beide
ausschließlich auf diesen einen Service zu, kein duplizierter Code. Sollte
sich in der Live-Verifikation (Schritt 6 unten) zeigen, dass die Rolle nicht
im UserInfo-Ergebnis, sondern nur im rohen Access Token steckt, wird
stattdessen `getPayloadFromAccessToken()` verwendet (reaktiv über
`toObservable(oidcSecurityService.authenticated).pipe(switchMap(...))`
erneut ausgelöst bei jedem Auth-Ereignis) — die öffentliche API (`roles`,
`isAdmin`) bleibt in beiden Fällen identisch, `AdminGuard` und `Nav` sind
davon unabhängig. Test: `user-roles.service.spec.ts`.

### 2. `AdminGuard` (`src/frontend/src/app/core/auth/admin.guard.ts`)

Funktionaler `CanActivateFn`: gibt `true` zurück, wenn
`UserRolesService.isAdmin()` wahr ist, sonst einen `UrlTree` auf
`/dashboard` (Analogie zum bestehenden Wildcard-Fallback `{ path: '**',
redirectTo: 'dashboard' }`). Stiller Redirect ohne Modal — konsistent mit
der bestehenden Fehlerkonzept-Tabelle (CLAUDE.md §7: „Rolle unzureichend" →
Weiterleitung, kein Modal). Test: `admin.guard.spec.ts`.

### 3. `features/admin/` — Platzhalter-Komponente

`admin.ts`, `admin.html`, `admin.routes.ts`, `admin.spec.ts` 1:1 nach dem
Muster von `features/dashboard/` (Titel „Admin", Text „Diese Ansicht folgt
in einem späteren Block."). Kein `canActivate` in `admin.routes.ts` — der
Guard sitzt zentral in `app.routes.ts`, an derselben Stelle wie `authGuard`,
damit sicherheitsrelevante Guards an einer Stelle sichtbar bleiben.

### 4. `app.routes.ts` erweitern

Neuer `admin`-Kindpfad mit `canActivate: [adminGuard]` und `loadChildren`
auf `features/admin/admin.routes`, vor der abschließenden Wildcard-Route
eingefügt. Test-Erweiterung in `app.routes.spec.ts`: Guard-Verdrahtung
prüfen sowie beide Rollen-Szenarien (Zugriff erlaubt/verweigert) mit einem
`UserRolesService`-Mock.

### 5. Admin-Button in `NavComponent`

`nav.ts`: `UserRolesService` per `inject()`, `isAdmin`-Signal durchreichen
(analog `authenticated`). `nav.html`: Button
(`<a routerLink="/admin" class="btn btn-secondary" title="Admin-Bereich
öffnen">Admin</a>`, kein Icon) zwischen `<app-theme-toggle />` (Zeile 69) und
dem bestehenden `@if (authenticated().isAuthenticated)`-Block (ab Zeile 70)
einfügen, sichtbar nur wenn `isAdmin()`. Test-Erweiterung in `nav.spec.ts`
(`UserRolesService`-Mock, zwei neue Fälle: Button sichtbar/unsichtbar je
nach Rolle).

## Nicht Teil dieses Blocks

- Admin-Bereich-Inhalt (Userliste, Löschen über Keycloak Admin REST API,
  zugehörige Backend-Endpoints).
- Swagger-UI-Freischaltung für die Admin-Rolle in Production.
- Rate Limiting, CORS-Production-Whitelist, CSP.
- Jede Backend-Änderung.

## Verifikation

1. `ng test --watch=false`, `ng lint`, `ng build` (`src/frontend/`,
   PowerShell).
2. Live gegen den laufenden Aspire-AppHost (PowerShell, kein Git Bash):
   - Testbenutzer mit Rolle `Admin` in der Keycloak-Admin-UI zuweisen (kein
     Testbenutzer/keine Rollenzuweisung im Realm-Import vorhanden).
   - Anmelden, Netzwerk-/Konsolen-Prüfung des Rollenclaims (UserInfo vs.
     Access Token) zur Entscheidung der finalen `UserRolesService`-
     Implementierung.
   - Admin-Button erscheint an vorgesehener Position, Klick navigiert zu
     `/admin`, Platzhalterinhalt erscheint.
   - Mit einem Benutzer ohne Rolle `Admin`: Button unsichtbar, direkter
     Aufruf von `/admin` leitet auf `/dashboard` um.
   - Logout: Button verschwindet mit Username/Logout.
3. Ergebnis der Rollenclaim-Verifikation kurz dokumentieren (TASK.md-Eintrag,
   ggf. ADR-Nachtrag analog zu ADR 0004/0010).
4. TASK.md nach Abschluss aktualisieren (Block 7b als abgeschlossen
   markieren).

**Risiko, das eine Rückmeldung statt eigenmächtiger Lösung erfordert:**
Sollte sich zeigen, dass der Rollenclaim weder im UserInfo-Ergebnis noch im
Access Token auftaucht, wäre eine Änderung an `keycloak/mymusic-realm.json`
(Protocol-Mapper-Konfiguration) nötig — das verlässt den hier freigegebenen
Umfang „nur Angular-Code" und wird in diesem Fall vor der Umsetzung
gemeldet, nicht selbständig entschieden.
