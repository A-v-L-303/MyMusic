# ADR 0023 — Swagger-UI außerhalb Development: Middleware-Gate statt Endpoint-Autorisierung

**Status**: Angenommen
**Datum**: 2026-08-26
**Betrifft**: `src/MyMusic.Api`

## Kontext

`wiki/sicherheit/sicherheitskonzept.md` verlangt, dass die Swagger-UI in
Production nur für authentifizierte Benutzer mit der Rolle `Admin` erreichbar
ist. ADR 0007 hatte die Umsetzung zurückgestellt, weil das Rollenkonzept zum
damaligen Zeitpunkt (Block 0e) noch nicht existierte — seit Block 7c gibt es
eine echte serverseitige `"Admin"`-Policy (`AddAuthorizationBuilder().AddPolicy
("Admin", ...)`, ausgewertet über `AdminAuthorizationHandler`), die die übrigen
Admin-Endpunkte (`AdminEndpoints.cs`) bereits über
`.RequireAuthorization("Admin")` auf einer `MapGroup` absichern.

Dieses Muster lässt sich für Swagger nicht direkt übertragen: Die installierte
`Swashbuckle.AspNetCore`-Version 10.0.1 stellt `UseSwagger()`/`UseSwaggerUI()`
ausschließlich als `IApplicationBuilder`-Middleware bereit (verifiziert gegen
`Swashbuckle.AspNetCore.Swagger.xml` der installierten Assembly — kein
`MapSwagger()` oder vergleichbares Endpoint-Routing-Pendant, über das
`.RequireAuthorization(...)` als Endpoint-Metadaten angehängt werden könnte).

## Entscheidung

Außerhalb von Development läuft vor `UseSwagger()`/`UseSwaggerUI()` ein
bedingter Middleware-Zweig auf dem Pfad `/swagger`
(`app.MapWhen(context => context.Request.Path.StartsWithSegments("/swagger"),
branch => { ... })`). Der Zweig prüft:

1. Ist `context.User` nicht authentifiziert → HTTP 401.
2. Ist die bestehende `"Admin"`-Policy nicht erfüllt (per
   `IAuthorizationService.AuthorizeAsync(context.User, "Admin")`, derselbe
   Mechanismus wie bei den übrigen Admin-Endpunkten) → HTTP 403.
3. Sonst `await next()`, gefolgt von `branch.UseSwagger()`/
   `branch.UseSwaggerUI()`.

In Development bleibt Swagger wie bisher vollständig ungegatet.

**Statussemantik 401/403 statt der 404-Ownership-Konvention**: CLAUDE.md §5.2
verlangt bei fremden Benutzerdaten 404 statt 403, um die Existenz einer
Ressource nicht zu bestätigen. Swagger ist kein benutzereigenes Fachobjekt,
sondern ein Admin-Werkzeug — die Konvention greift hier nicht, Standard-REST-
Semantik ist die richtige Wahl.

## Verworfene Alternativen

- **Eigene Minimal-API-Route, die Swagger-JSON/UI durchreicht**: hätte
  `.RequireAuthorization("Admin")` als Endpoint-Metadaten ermöglicht, aber
  Swashbuckles UI besteht aus mehreren zusammenhängenden Ressourcen (HTML,
  eingebettete Assets, `swagger.json`) — das manuelle Nachbauen dieses
  Routings wäre erheblich aufwendiger und fragiler gegenüber
  Swashbuckle-Updates als ein einzelner Middleware-Gate auf Pfadebene.
- **Reverse-Proxy-Regel** (z. B. Nginx-Basic-Auth oder IP-Whitelist vor
  `/swagger`): würde die Autorisierung außerhalb der Anwendung und außerhalb
  des bestehenden Rollenkonzepts verlagern, dupliziert die Admin-Prüfung
  statt sie wiederzuverwenden, und ist ohnehin nicht umsetzbar, solange keine
  Production-Infrastruktur (Nginx/Docker Compose) existiert.
- **Warten auf ein künftiges Swashbuckle-Feature mit Endpoint-Routing-
  Unterstützung**: keine belastbare Zeitschiene, die Wiki-Vorgabe ist bereits
  seit Block 0e offen.

## Konsequenzen

- `AddSwaggerGen(...)` bleibt unverändert immer registriert; nur die
  Middleware-Aktivierung ist umgebungsabhängig.
- Der neue Zweig läuft nach `app.UseAuthentication()`/`app.UseAuthorization()`
  in der Pipeline, sodass `context.User` bereits befüllt ist, bevor
  `IAuthorizationService.AuthorizeAsync(...)` aufgerufen wird.
- Integrationstest `SwaggerEndpointTests.GetSwaggerJson_AusserhalbDevelopmentNurMitAdminRolle`
  deckt alle drei Fälle (kein Token → 401, Token ohne Admin-Rolle → 403, Token
  mit Admin-Rolle → 200) in einem AppHost-Lauf ab, analog zum Muster in
  `AdminEndpointsTests.cs`. Die `api`-Ressource wird dafür testweise auf
  `ASPNETCORE_ENVIRONMENT=Production` gesetzt (`appHost.CreateResourceBuilder
  (...)` vor `BuildAsync()`) — das deckte einen weiteren, unabhängigen Fehler
  auf (siehe ADR 0024).
