# ADR 0010 — Angular-Keycloak-Integration: Library und Token-Storage

**Status**: Angenommen
**Datum**: 2026-08-08
**Betrifft**: `src/frontend`

## Kontext

Block 7a (Angular-Login-Flow) musste den Angular-Client an den bereits im
Keycloak-Realm fertig konfigurierten öffentlichen Client `mymusic-angular`
(Authorization Code + PKCE/S256) anbinden. Dafür waren zwei Entscheidungen mit
echten, ernsthaft geprüften Alternativen zu treffen: die Wahl der
OIDC/Keycloak-Integrationsbibliothek und die Strategie, wo das Access Token im
Browser gehalten wird.

## Entscheidung 1: Bibliothek

**Gewählt**: `keycloak-angular` (+ `keycloak-js` als Peer Dependency).

Geprüfte Alternativen:

1. **`keycloak-angular` + `keycloak-js`** (gewählt) — offizieller
   Keycloak-JS-Adapter mit dünnem, aktiv gepflegtem Angular-Wrapper.
   `keycloak-angular@22.0.0` verlangt laut npm-Registry exakt
   `@angular/core@^22`, `@angular/common@^22`, `@angular/router@^22` und
   `keycloak-js@^18...^26` (passend zum Keycloak-Server 26.5) — keine
   zone.js-Abhängigkeit, kompatibel mit dem zoneless Angular-22-Workspace aus
   Block 0c. Bringt PKCE, Silent-SSO-Check und Rollen-Auslesen aus dem Token
   nativ mit.
2. **`angular-oauth2-oidc`** (verworfen) — generischer, provider-unabhängiger
   OIDC/OAuth2-Client. Funktioniert auch gegen Keycloak, aber ohne
   Keycloak-spezifische Komfortfunktionen (z. B. direktes Rollen-Mapping aus
   Realm-/Resource-Rollen). Der Mehrwert der Provider-Unabhängigkeit entfällt,
   da MyMusic dauerhaft auf Keycloak festgelegt ist (CLAUDE.md §3).
3. **Eigene Implementierung ohne Zusatzpaket** (verworfen) — PKCE-Flow
   (`code_verifier`/`code_challenge`) selbst mit `fetch` und der Web-Crypto-API
   bauen. Kein zusätzliches Paket, aber Silent-Refresh, Token-Parsing und alle
   Fehlerfälle (abgelaufene Session, ungültiger State-Parameter, Netzwerkfehler
   während des Redirects) müssten komplett selbst entwickelt und getestet
   werden — unverhältnismäßiger Aufwand für eine Aufgabe, die eine aktiv
   gepflegte, genau für diesen Anwendungsfall gebaute Bibliothek bereits löst.

## Entscheidung 2: Token-Storage

**Gewählt**: nur im Speicher (In-Memory, keycloak-js-Standardverhalten), kein
localStorage/sessionStorage.

Geprüfte Alternativen:

1. **In-Memory** (gewählt) — Token liegt nur im JS-Heap, nirgends persistent.
   Sicherste Variante gegen Diebstahl per XSS (OWASP-Empfehlung), passt zur im
   Wiki (`sicherheit/sicherheitskonzept.md`) bereits dokumentierten strikten
   Content Security Policy. Nachteil: Nach jedem Neuladen der Seite ist das
   Token weg und muss neu beschafft werden — gelöst über Keycloaks
   Silent-SSO-Check (`onLoad: 'check-sso'` + `silentCheckSsoRedirectUri`,
   verstecktes iframe gegen die bestehende Keycloak-Session), sodass der
   Benutzer davon nichts bemerkt, solange die SSO-Session (8 Stunden) noch
   gültig ist.
2. **sessionStorage** (verworfen) — übersteht einen Tab-Reload ohne
   Silent-Check, ist aber per JavaScript auslesbar und damit einem höheren
   XSS-Risiko ausgesetzt. Zusätzlich pro Tab isoliert, was mehrfachen Login
   bei mehreren gleichzeitig offenen Tabs erzwingen würde.
3. **localStorage** (verworfen) — übersteht auch einen Browser-Neustart,
   höchstes XSS-Risiko der drei Optionen, da das Token dauerhaft und
   tab-übergreifend im Klartext zugreifbar bleibt.

## Konsequenzen

- `src/frontend/package.json` führt `keycloak-angular` und `keycloak-js` als
  neue `dependencies`.
- Ausschließlich die moderne `keycloak-angular`-API (`provideKeycloak()`,
  `includeBearerTokenInterceptor`, `createAuthGuard()`) wird verwendet — die
  seit v19 deprecateten Klassen (`KeycloakService`, `KeycloakAuthGuard`,
  `KeycloakBearerInterceptor`) werden nicht eingesetzt.
- Der Bearer-Interceptor ist per URL-Regex explizit auf `apiBaseUrl`
  beschränkt (`createInterceptorCondition`) — das Token wird nie an Keycloak
  selbst oder andere Origins gesendet.
- `wiki/sicherheit/sicherheitskonzept.md` wurde um die bisher fehlende
  Dokumentation von Token-Storage, Silent-Refresh und Logout-Mechanik
  ergänzt (siehe dortiger Abschnitt „Token-Storage, Silent-Refresh und Logout
  im Angular-Client").
