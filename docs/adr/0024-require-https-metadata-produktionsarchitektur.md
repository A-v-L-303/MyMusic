# ADR 0024 — RequireHttpsMetadata von IsDevelopment() entkoppelt

**Status**: Angenommen
**Datum**: 2026-08-26
**Betrifft**: `src/MyMusic.Api`

## Kontext

`Program.cs` setzte bislang `options.RequireHttpsMetadata =
!builder.Environment.IsDevelopment();` in der `AddJwtBearer(...)`-Konfiguration
— außerhalb Development sollte die OIDC-Metadaten-/JWKS-Abfrage bei Keycloak
zwingend über HTTPS laufen.

Entdeckt beim Schreiben der Integrationstests für Block 7j (ADR 0023): Sobald
die `api`-Ressource testweise auf `ASPNETCORE_ENVIRONMENT=Production` gesetzt
wird, antwortet die API auf **jede** Anfrage mit HTTP 500 — auch auf eine
komplett unauthentifizierte. Ursache: `JwtBearerPostConfigureOptions` prüft
beim ersten Zugriff auf die Optionen (bei jeder Anfrage, die die
Authentifizierungs-Middleware durchläuft — nicht erst beim Vorhandensein eines
Tokens), ob `RequireHttpsMetadata` gesetzt und die `Authority` gleichzeitig
`http://` ist, und wirft in diesem Fall eine `InvalidOperationException`
("The MetadataAddress or Authority must use HTTPS unless disabled via
RequireHttpsMetadata=false."). Keycloaks `Authority` ist sowohl in der
Aspire-Testumgebung als auch — laut Wiki
(`projekt/deployment-konzept.md`, `sicherheit/sicherheitskonzept.md`) — in der
später zu bauenden Production-Umgebung eine `http://`-Adresse: TLS wird
ausschließlich am Reverse Proxy terminiert, die interne Kommunikation
zwischen den Containern (also auch API zu Keycloak) bleibt bewusst
unverschlüsselt.

Das ist kein Testartefakt, sondern ein latenter Fehler, der die reale
Production-API träfe: Sobald `ASPNETCORE_ENVIRONMENT=Production` gesetzt wird
(unabhängig von Block 7j), könnte die API keine einzige Anfrage mehr
authentifizieren.

Dieser Befund liegt außerhalb des ursprünglich für Block 7j freigegebenen
Arbeits-Prompts (der ausschließlich Swagger-Gate, CORS-Whitelist und CSP
umfasste). Der Projektinhaber wurde vor der Behebung informiert und hat die
Scope-Erweiterung ausdrücklich freigegeben — ohne diese Korrektur wäre weder
der Swagger-Gate-Integrationstest für Nicht-Development-Umgebungen noch der
CORS-Production-Test lauffähig gewesen.

## Entscheidung

`options.RequireHttpsMetadata = false;` unabhängig von der Umgebung, mit
Code-Kommentar zur Begründung (TLS-Terminierung ausschließlich am Reverse
Proxy, kein umgebungsabhängiger Unterschied in der internen
API-zu-Keycloak-Kommunikation).

## Verworfene Alternativen

- **Neuer Konfigurationswert** (z. B. `Keycloak:RequireHttpsMetadata`, per
  Umgebung gesetzt): hätte eine Konfigurierbarkeit für einen Fall geschaffen,
  der laut der bereits getroffenen und dokumentierten Architekturentscheidung
  (TLS nur am Reverse Proxy) in keiner der beiden Umgebungen (Development,
  Production) je `true` sein soll — eine Konfigurationsmöglichkeit ohne
  vorgesehenen abweichenden Wert wäre unnötige Komplexität (CLAUDE.md-Prinzip:
  keine Konfigurierbarkeit für hypothetische künftige Anforderungen).
- **Weiterhin an `IsDevelopment()` koppeln, aber Testumgebung gesondert
  ausnehmen**: hätte das eigentliche Problem (echte Production wäre ebenso
  betroffen) verdeckt statt behoben.

## Konsequenzen

- Betrifft ausschließlich die HTTPS-Pflicht für die OIDC-Metadaten-/JWKS-
  Abfrage der API gegenüber Keycloak (interner, vertrauenswürdiger
  Docker-/Aspire-Netzwerkbereich) — keine Auswirkung auf die Browser-zu-API-
  oder Browser-zu-Keycloak-Verbindung, die weiterhin über den in Production
  vorgesehenen Reverse Proxy mit TLS läuft.
- Sollte sich die Deployment-Architektur künftig ändern (z. B. TLS auch
  intern), muss diese Entscheidung zusammen mit ADR
  `0023-swagger-admin-gate-production.md`s Testaufbau erneut geprüft werden.
- Der neue Swagger-Gate-Integrationstest (ADR 0023) sowie die erweiterten
  `CorsPolicyTests` verifizieren als Nebeneffekt, dass die API außerhalb
  Development überhaupt lauffähig authentifiziert — vorher gab es dafür
  keinen automatisierten Nachweis.
