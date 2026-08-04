# ADR 0004 — JWT-Bearer-Konfiguration: Audience-Validierung und Claim-Mapping

**Status**: Angenommen
**Datum**: 2026-07-26
**Betrifft**: `MyMusic.Api/Program.cs` (JWT-Bearer-Konfiguration)

## Kontext

Das Sicherheitskonzept (Wiki `sicherheit/sicherheitskonzept.md`) legt fest:
„ASP.NET Core übernimmt automatisch die Prüfung von Signatur, Audience, Issuer
und Expiry" über `AddAuthentication().AddJwtBearer()`. `TokenValidationParameters`
validiert die Audience standardmäßig (`ValidateAudience = true`), verlangt dafür
aber eine explizit gesetzte `ValidAudience` — ohne sie schlägt die Validierung
für jedes Token fehl, mit ihr muss der erwartete Wert exakt zum `aud`-Claim des
von Keycloak ausgestellten Access Tokens passen.

Der versionierte Realm-Import (`keycloak/mymusic-realm.json`) definiert für den
Client `mymusic-angular` keinen eigenen Audience-Mapper. Ohne einen solchen
Mapper trägt ein von Keycloak (Version 26.5, Standardkonfiguration) ausgestelltes
Access Token als Audience den Wert `account` — das ist der Keycloak-Standard für
den eingebauten Account-Client, unabhängig vom anfragenden Client. Diese
Konkretisierung war im Wiki nicht getroffen (nur „Audience wird geprüft", ohne
Angabe des erwarteten Werts) und mangels vorhandener Entitäten mit Block 0b noch
nicht anderweitig entschieden.

Zusätzlich mappt `System.IdentityModel.Tokens.Jwt`/`Microsoft.IdentityModel.JsonWebTokens`
den `sub`-Claim standardmäßig auf `ClaimTypes.NameIdentifier`
(`http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier`), sobald
`AddJwtBearer()` ohne weitere Konfiguration verwendet wird. Der
`ICurrentUserService` liest aber gemäß Sicherheitskonzept den rohen `sub`-Claim
(`httpContext.User.FindFirst("sub")`) — mit dem Standardverhalten liefert das
`null`, was beim Integrationstest (siehe unten) reproduzierbar zu HTTP 500 statt
200 führte.

## Entscheidung

- `ValidAudience` wird in der JWT-Bearer-Konfiguration der API auf `"account"`
  gesetzt — den von Keycloak ohne eigenen Audience-Mapper standardmäßig
  vergebenen Wert. Der Realm-Import selbst wird dafür **nicht** verändert.
- `options.MapInboundClaims = false;` wird gesetzt, damit der `sub`-Claim
  unverändert (nicht auf `ClaimTypes.NameIdentifier` umbenannt) im
  `ClaimsPrincipal` ankommt.

Beide Einstellungen sind durch den Integrationstest
(`tests/MyMusic.IntegrationTests/MeEndpointTests.cs`) gegen einen echten
Keycloak-Container verifiziert: Ohne `MapInboundClaims = false` schlug der Test
mit HTTP 500 fehl (statt 200), da `ICurrentUserService.UserId` keinen
`sub`-Claim fand.

## Begründung

- Erfüllt die im Sicherheitskonzept geforderte Audience-Prüfung, ohne die
  Keycloak-Realm-Konfiguration anzufassen (außerhalb des freigegebenen Umfangs
  von Block 0b).
- Vermeidet ein manuelles, sicherheitsrelevantes Abschalten der
  Audience-Validierung (`ValidateAudience = false`), was dem Sicherheitskonzept
  widersprechen würde.
- `MapInboundClaims = false` ist die von Microsoft empfohlene Einstellung, wenn
  die Anwendung Standard-JWT-Claim-Namen (wie `sub`) statt der .NET-eigenen
  `ClaimTypes`-URIs erwartet — genau der im Sicherheitskonzept vorgegebene Fall.
- Keine neue Abhängigkeit oder Migration nötig.

## Konsequenzen

- Sobald ein dedizierter Resource-Server-Client oder ein eigener
  Audience-Mapper für `mymusic-angular` eingeführt wird (z. B. um die API als
  eigene Audience statt `account` zu führen), muss `ValidAudience` zusammen mit
  dem Realm-Import angepasst werden — offener Folgepunkt, keine Aufgabe von
  Block 0b.
- Alle künftigen Claims-Zugriffe im Code müssen mit den rohen JWT-Claim-Namen
  arbeiten (z. B. `"sub"`, nicht `ClaimTypes.NameIdentifier`), da
  `MapInboundClaims = false` gilt.
