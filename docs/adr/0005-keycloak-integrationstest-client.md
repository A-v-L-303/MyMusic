# ADR 0005 — Dedizierter Keycloak-Client für den Auth-Integrationstest

**Status**: Angenommen
**Datum**: 2026-07-26
**Betrifft**: `keycloak/mymusic-realm.json`, `tests/MyMusic.IntegrationTests/MeEndpointTests.cs`

## Kontext

Block 0b verlangt einen Integrationstest, der `/api/me` einmal ohne Token
(HTTP 401) und einmal mit einem echten, von Keycloak ausgestellten Access
Token (HTTP 200) aufruft (Arbeits-Prompt
`docs/prompts/2026-07-26-block-0b-cqrs-repository-auth.md`, Schritt 7). Wie ein
Testbenutzer und ein Access Token dafür beschafft werden, war im Wiki nicht
festgelegt und wurde im Arbeits-Prompt als während der Umsetzung zu klärender
Punkt benannt.

Geprüfte Optionen:

- **Direct-Grant-Token über den Produktivclient `mymusic-angular`**: nicht
  möglich, ohne `directAccessGrantsEnabled` auf dem produktiven Client zu
  aktivieren — das würde das Sicherheitskonzept aufweichen (Authorization Code
  + PKCE ist für diesen Client bewusst die einzige Option, siehe
  `sicherheitskonzept.md`).
- **Direct-Grant-Token über den eingebauten `admin-cli`-Client**: technisch
  möglich (Keycloak legt `admin-cli` mit `directAccessGrantsEnabled: true` in
  jedem Realm an), aber `admin-cli` nutzt standardmäßig
  `client.use.lightweight.access.token.enabled: true` — das dabei ausgestellte
  Access Token enthält **keinen** `sub`- und keinen `aud`-Claim. Damit ist es
  für `/api/me` (das den `sub`-Claim benötigt) nicht verwendbar. Empirisch mit
  einem Wegwerf-Keycloak-Container verifiziert.

## Entscheidung

Der Realm-Import (`keycloak/mymusic-realm.json`) erhält einen zweiten,
klar als Testwerkzeug erkennbaren Client `mymusic-integration-tests`:
`publicClient: true`, `standardFlowEnabled: false`,
`directAccessGrantsEnabled: true`, keine Redirect-URIs. Der Integrationstest
legt bei jedem Lauf einen Wegwerf-Testbenutzer per Keycloak-Admin-REST-API an
(zufälliger Benutzername, zufälliges Passwort, nur im Testprozess-Speicher),
holt darüber ein Access Token per Resource-Owner-Password-Grant und löscht den
Benutzer im `finally`-Block wieder.

## Begründung

- Der Produktivclient `mymusic-angular` bleibt unverändert bei Authorization
  Code + PKCE — keine Aufweichung der dort bewusst getroffenen
  Sicherheitsentscheidung.
- Kein Testbenutzer und kein Passwort werden im Repository abgelegt — beides
  entsteht zur Laufzeit des Tests und wird danach wieder gelöscht.
- `admin-cli` ist für Keycloaks eigene Administration vorgesehen
  (Lightweight-Token ohne `sub`/`aud`) und damit für einen Test der
  Anwendungs-API strukturell ungeeignet.
- Ein Realm-Client ohne Redirect-URIs und ohne Standard-Flow kann nicht für
  einen Login im Browser missbraucht werden — er dient ausschließlich dem
  Resource-Owner-Password-Grant aus dem Testprozess heraus.

## Konsequenzen

- `keycloak/mymusic-realm.json` enthält ab sofort einen zweiten Client, der in
  Produktion ungenutzt bleibt, aber im importierten Realm existiert. Das ist
  eine geringfügige, dauerhafte Erweiterung der Realm-Konfiguration.
- Der Integrationstest ist gegen einen echten, per `Aspire.Hosting.Testing`
  gestarteten Keycloak-Container verifiziert (lokal mit Docker erfolgreich
  ausgeführt); er läuft weiterhin nicht in der CI (siehe ADR 0003).
- Sollte künftig ein anderer, standardisierter Weg zur Testbenutzer- und
  Token-Beschaffung eingeführt werden (z. B. Service-Accounts,
  Client-Credentials-Grant für M2M-Tests), ist dieser Client entsprechend zu
  überprüfen oder abzulösen.
