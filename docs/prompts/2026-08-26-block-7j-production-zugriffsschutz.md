# Block 7j: Production-Zugriffsschutz (Swagger, CORS, CSP)

## Kontext

Laut `TASK.md` (Abschnitt 7, "Aufgaben (noch offen)", Zeile 2126-2131) sind
nach Block 7i (Rate Limiting, PR #90, gemergt) noch drei Punkte aus dem
Sicherheitskonzept offen (Wiki `sicherheit/sicherheitskonzept.md`):

1. Swagger-UI in Production nur für die Admin-Rolle erreichbar (seit Block 0e
   zurückgestellt, ADR 0007 — das Rollenkonzept fehlte damals noch, existiert
   seit Block 7c).
2. CORS-Production-Whitelist (aktuell nur eine `DevelopmentCors`-Policy für
   `localhost`-Origins, `Program.cs:82-92`, seit Block 7a).
3. Content Security Policy für den Angular-Client (Wiki-Mindest-Direktiven:
   `default-src 'self'`, `script-src 'self'`, `style-src 'self'`,
   `connect-src 'self'` + Keycloak-URL + API-URL, `img-src 'self' data:`).

**Zentraler Iststand-Befund** (verifiziert, nicht angenommen): Im Repository
existiert keinerlei Production-Infrastruktur — kein Docker Compose, kein
Nginx, kein `appsettings.Production.json` (Suche über das gesamte Repo ohne
Treffer). Laut Wiki (`projekt/deployment-konzept.md`) ist der
Hosting-Anbieter für Production noch nicht entschieden. Die Wiki-Vorgabe für
CSP sieht in Production einen HTTP-Response-Header "vom Webserver" (Nginx)
vor, den es also noch nicht gibt.

**Mit dem Projektinhaber geklärt** (vor Planbeginn, per Rückfrage):

- CSP-Production (Nginx-Header) ist **nicht** Teil dieses Blocks — bleibt
  abhängig vom noch nicht begonnenen Production-/Docker-Compose-Setup, wird
  explizit als offen dokumentiert (kein Vorgriff auf die offene
  Hosting-Entscheidung). Umgesetzt wird nur die Development/lokal-Variante
  (Meta-Tag).
- Swagger-Gate-Ansatz: Middleware-basiertes Gate statt Endpoint-Routing (siehe
  Entscheidung 1 unten), Standard-REST-Semantik 401/403.

Aktuelle Middleware-Pipeline (`Program.cs`, nach Block 7i):
`UseExceptionHandler → UseSerilogRequestLogging → [Dev: UseCors] →
UseAuthentication → UseRateLimiter → UseAuthorization → [Dev: Swagger] →
Endpoint-Mappings`.

## Entscheidungen

1. **Swagger-Gate als Middleware, nicht als Endpoint-Autorisierung**:
   Swashbuckle.AspNetCore 10.0.1 (installierte Version verifiziert) stellt
   `UseSwagger()`/`UseSwaggerUI()` nur als reine
   `IApplicationBuilder`-Middleware bereit (`Swashbuckle.AspNetCore.Swagger.xml`
   enthält kein `MapSwagger()`/Endpoint-Routing-Pendant) — anders als die
   übrigen Admin-Endpunkte (`AdminEndpoints.cs`,
   `.RequireAuthorization("Admin")` auf einer `MapGroup`) kann hier keine
   Endpoint-Metadaten-basierte Autorisierung verwendet werden. Stattdessen:
   `app.MapWhen(path startsWith "/swagger", branch => { branch.Use(async
   (context, next) => { ...IAuthorizationService.AuthorizeAsync(context.User,
   "Admin")... }); branch.UseSwagger(); branch.UseSwaggerUI(); })` außerhalb
   Development. In Development bleibt Swagger wie bisher ungegatet.
2. **401/403 statt der 404-Ownership-Konvention**: CLAUDE.md §5.2 verlangt bei
   fremden Benutzerdaten 404 statt 403, um die Existenz einer Ressource nicht
   zu bestätigen. Swagger ist kein benutzereigenes Fachobjekt, sondern ein
   Admin-Werkzeug — Standard-REST-Semantik (401 ohne Token, 403 mit Token aber
   ohne Admin-Rolle) ist hier die richtige Wahl.
3. **CORS-Konfiguration über `Cors:AllowedOrigins`, kein neues
   `appsettings.Production.json`**: Im Repo gibt es kein Muster für gestufte
   `appsettings.*.json`-Dateien — alle variablen Werte (Keycloak-Authority,
   DB-Connection-String, Discogs-Token) kommen ausschließlich über
   `builder.Configuration["..."]` aus Environment-Variablen/Aspire-Parametern.
   `Cors:AllowedOrigins` (Array) folgt demselben Muster; ohne gesetzten Wert
   ergibt sich ein leeres Array — sicherer Default, bis eine Hosting-Domain
   feststeht. Kein eigenes ADR (reine Konfigurationsumsetzung der bereits im
   Wiki entschiedenen Policy, keine echte Alternativen-Abwägung).
4. **CSP nur Development/lokal, per Meta-Tag + Nonce**: Umsetzung nach dem
   bestehenden `prestart`/`prebuild`-Muster von
   `scripts/write-runtime-config.mjs` (ADR 0009) — dieselben
   `MYMUSIC_API_BASE_URL`/`MYMUSIC_KEYCLOAK_AUTHORITY`-Umgebungsvariablen
   liefern die für `connect-src` nötigen Origins. Zwei technische
   Voraussetzungen (gegen die installierten `@angular/core`-Typings
   verifiziert):
   - Das bestehende Inline-`<script>` in `index.html` (Block 0f,
     FOUC-Vermeidung) wird nach `public/theme-init.js` ausgelagert
     (`script-src 'self'` deckt externe same-origin Skripte ab, ein Inline-
     Skript ohne Nonce/Hash nicht).
   - Angular injiziert komponenteneigene `<style>`-Tags zur Laufzeit
     (`ViewEncapsulation`). Angular stellt dafür offiziell `CSP_NONCE`/das
     `ngCspNonce`-Attribut auf dem Root-Node bereit
     (`@angular/core/types/core.d.ts`, Zeile 2960 ff., verifiziert). Ein pro
     Build zufällig erzeugter Nonce-Wert wird auf `<app-root>` gesetzt und im
     Meta-Tag unter `style-src 'self' 'nonce-...'` eingetragen.

## Umsetzung

1. **Branch**: `block-7j-production-zugriffsschutz` von `main` — bereits
   angelegt (Git-Status vor Beginn war sauber und aktuell zu `origin/main`).
2. **Arbeits-Prompt**: diese Datei — vor Umsetzungsbeginn geschrieben, danach
   nicht mehr verändert (CLAUDE.md §2.3).

### Swagger-Gate

3. **`src/MyMusic.Api/Program.cs`**: Registrierung von `UseSwagger()`/
   `UseSwaggerUI()` von der bestehenden
   `if (app.Environment.IsDevelopment())`-Verzweigung auf ein echtes
   `if/else` umstellen — Development unverändert, `else`-Zweig mit
   `app.MapWhen(...)` (siehe Entscheidung 1/2 oben: 401 ohne authentifizierten
   `context.User`, 403 wenn die `"Admin"`-Policy nicht erfüllt ist, sonst
   `await next()` gefolgt von `branch.UseSwagger()`/`branch.UseSwaggerUI()`).
   `AddSwaggerGen(...)`-Registrierung bleibt unverändert.
4. **`AppHost.cs`**: keine Änderung nötig — der bestehende
   `.WithUrlForEndpoint("https", ...)`-Shortcut auf `/swagger` bleibt (gilt
   für Development, dort weiterhin ungegatet).
5. **Neuer ADR** `docs/adr/0023-swagger-admin-gate-production.md`:
   Middleware-Lösung (`MapWhen` + `IAuthorizationService`) gegenüber
   Alternativen (z. B. eigene Minimal-API-Route, die Swagger-JSON/UI
   durchreicht; Reverse-Proxy-Regel) begründen, 401/403-Semantik festhalten.
6. **Erweiterter Integrationstest**
   `tests/MyMusic.IntegrationTests/SwaggerEndpointTests.cs`: bestehender Test
   (Development → 200) bleibt unverändert. Neue Fälle für eine
   Nicht-Development-Umgebung — die `api`-Ressource wird vor `BuildAsync()`
   testweise auf `ASPNETCORE_ENVIRONMENT=Production` gesetzt
   (`appHost.CreateResourceBuilder(...)` auf die per `appHost.Resources`
   gefundene `api`-Projektressource, `.WithEnvironment("ASPNETCORE_ENVIRONMENT",
   "Production")` — API von `Aspire.Hosting.Testing`, gegen die installierte
   Assembly verifiziert, exakte Typisierung beim Schreiben des Tests
   festlegen): kein Token → 401, `KeycloakTestClient`-Token ohne Admin-Rolle →
   403, Token mit Admin-Rolle → 200 auf `/swagger/v1/swagger.json`.

### CORS

7. **`src/MyMusic.Api/Program.cs`**: `if (builder.Environment.IsDevelopment())`
   um die CORS-Registrierung zu einem `if/else` erweitern — Development
   registriert weiterhin `DevelopmentCors` (`Uri.IsLoopback`), der `else`-Fall
   registriert `ProductionCors` mit
   `builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? []`
   als `WithOrigins(...)`, gleiche `WithMethods`/`WithHeaders` wie bisher.
   `app.UseCors(...)` läuft künftig immer (nicht mehr hinter
   `IsDevelopment()`), wählt per `app.Environment.IsDevelopment() ?
   "DevelopmentCors" : "ProductionCors"`.
8. **Erweiterter Integrationstest**
   `tests/MyMusic.IntegrationTests/CorsPolicyTests.cs`: neue Fälle für die
   Production-Policy — `api`-Ressource testweise mit
   `ASPNETCORE_ENVIRONMENT=Production` und
   `Cors__AllowedOrigins__0=https://mymusic.example.com` (Platzhalter-Origin
   nur für den Test) gestartet; Preflight von dieser Origin wird gespiegelt,
   Preflight von einer nicht gelisteten Origin bekommt keinen
   `Access-Control-Allow-Origin`-Header.

### CSP (nur Development/lokal)

9. **`src/frontend/public/theme-init.js`** (neu): Inhalt des bisherigen
   Inline-Scripts aus `index.html` (Block 0f, `localStorage.getItem
   ('mymusic-theme')` → `data-theme`-Attribut) unverändert übernommen.
10. **`src/frontend/src/index.html`**: Inline-Script durch
    `<script src="theme-init.js"></script>` an derselben Stelle im `<head>`
    ersetzen; `ngCspNonce="__CSP_NONCE__"` auf `<app-root>` sowie ein
    Platzhalter-Kommentar für das CSP-Meta-Tag ergänzen, beide vom neuen
    Skript (Schritt 11) zu ersetzen.
11. **Neues Skript `src/frontend/scripts/write-csp-meta.mjs`** (eigenständig,
    nicht in `write-runtime-config.mjs` integriert, um dessen bereits
    verifiziertes Verhalten nicht anzufassen): liest
    `MYMUSIC_API_BASE_URL`/`MYMUSIC_KEYCLOAK_AUTHORITY`, leitet per
    `new URL(...).origin` die beiden Origins ab, erzeugt einen zufälligen
    Nonce (`crypto.randomBytes(16).toString('base64')`), baut die
    Direktivenzeile (`default-src 'self'; script-src 'self'; style-src 'self'
    'nonce-<nonce>'; connect-src 'self' <keycloak-origin> <api-origin>;
    img-src 'self' data:;`) und ersetzt in `index.html` sowohl den
    `__CSP_NONCE__`-Platzhalter als auch den Meta-Tag-Kommentar-Platzhalter.
12. **`src/frontend/package.json`**: `prestart`/`prebuild` um
    `&& node scripts/write-csp-meta.mjs` ergänzen (läuft nach
    `write-runtime-config.mjs`, da die CSP die bereits geschriebenen
    Runtime-Werte referenziert, aber unabhängig davon aus den Umgebungs-
    variablen selbst liest).
13. **Neuer ADR** `docs/adr/0024-csp-meta-tag-development.md`: Nonce-basiertes
    `style-src` statt `'unsafe-inline'`, externalisiertes Theme-Script statt
    Hash-Pinning, hält fest, dass CSP-Production (Nginx-Header) bewusst nicht
    Teil dieses Blocks ist und vom noch offenen Production-/Docker-Compose-
    Setup abhängt.
14. **Live-Verifikation** (siehe unten) statt neuer Vitest-Tests — das Skript
    ist reiner Build-Zeit-Code außerhalb von Angular, analog zu
    `write-runtime-config.mjs`, das ebenfalls nicht separat getestet wird.
    Bestehende `theme.service.spec.ts`/`theme-toggle.spec.ts` bleiben
    unverändert (Theme-Logik selbst ändert sich nicht).

### Dokumentation

15. **`TASK.md`**: neuen Abschnitt `### 7j. Production-Zugriffsschutz` im
    Stil der bestehenden Unterabschnitte von Abschnitt 7 (Status,
    Arbeits-Prompt-Verweis, Umgesetzt, Abnahmekriterium); die drei Punkte aus
    der "Aufgaben (noch offen)"-Liste (Zeile 2126-2131) sowie aus dem
    "Aktuell nicht umgesetzt"-Abschnitt oben entfernen — dabei CSP-Production
    (Nginx-Header) weiterhin explizit als offen führen, mit Verweis auf das
    fehlende Production-/Docker-Compose-Setup.
16. **`README.md`**: neuer Abschnitt im Stil der bestehenden Block-Abschnitte.
17. **Projekt-`CLAUDE.md`** (`../../MyMusic/CLAUDE.md`): neuer
    "Stand"-Absatz; die drei Punkte aus der abschließenden "Offen sind
    ..."-Aufzählung entfernen, CSP-Production dort weiterhin als offen
    nennen.
18. Build, Tests (Backend vollständig inkl. der beiden erweiterten
    Integrationstests, Frontend `npm test`), `dotnet format
    --verify-no-changes`, `prettier --check` vor Abschluss.

## Verifikation

- `dotnet build MyMusic.slnx`
- `dotnet test MyMusic.slnx` (vollständige Suite, insbesondere
  `SwaggerEndpointTests`, `CorsPolicyTests`, `MeEndpointTests`,
  `RateLimitingTests` — alle berühren denselben Pipeline-Abschnitt)
- `dotnet format --verify-no-changes`
- `npm run build`, `npm test`, `npx prettier --check .` im Frontend
- Manuelle Live-Prüfung gegen den laufenden Aspire-AppHost (Development):
  - Swagger weiterhin ohne Anmeldung erreichbar (Development-Verhalten
    unverändert).
  - CORS-Verhalten aus dem Angular-Frontend unverändert funktionsfähig (kein
    Regressionstest für den bestehenden Development-Flow).
  - CSP: Browser-Konsole ohne CSP-Verstöße, Theme-Toggle/FOUC-Vermeidung aus
    Block 0f weiterhin unauffällig, Login-Flow und API-Zugriffe
    (`connect-src`) funktionieren. Insbesondere prüfen, ob Angulars
    Dev-Server (`ng serve`, per `AddJavaScriptApp` gestartet) für
    HMR/Live-Reload zusätzliche CSP-Erlaubnisse braucht (z. B. `connect-src`
    für einen WebSocket, `'unsafe-eval'` für Sourcemaps) — wird an
    Konsolenfehlern sichtbar und bei Bedarf in der Direktivenliste
    nachgezogen, ohne die Grundarchitektur (Skript, Nonce-Mechanismus) zu
    ändern.

## Freigabepflichtige Folgeschritte (nicht Teil dieser Umsetzung)

Commits, Push, Pull Request und Merge bleiben nach CLAUDE.md §2.2/§2.4
gesondert freigabepflichtig und werden erst nach Abschluss der Implementierung
und Tests angefragt.
