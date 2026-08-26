# ADR 0022 — Rate Limiting

**Status**: Angenommen
**Datum**: 2026-08-26
**Betrifft**: `src/MyMusic.Api`

## Kontext

`wiki/sicherheit/sicherheitskonzept.md` legt für Rate Limiting fest: ein
globales Limit von 100 Requests pro Minute pro authentifiziertem Benutzer
(`userId` aus dem JWT), umgesetzt über die eingebaute ASP.NET-Core-Middleware
`Microsoft.AspNetCore.RateLimiting` (kein externes Paket), mit HTTP 429 bei
Überschreitung. Diese Vorgabe legt Grenzwert, Partitionierungsschlüssel und
Statuscode fest, aber nicht den konkreten Limiter-Algorithmus, den
Geltungsbereich innerhalb der API oder die Form der 429-Antwort — diese
Entscheidungen trifft dieser ADR.

## Entscheidung 1 — Algorithmus: Fixed Window

Verwendet wird `RateLimitPartition.GetFixedWindowLimiter` (ein Zeitfenster von
einer Minute, `PermitLimit = 100`, `QueueLimit = 0` — Anfragen über dem Limit
werden sofort mit 429 abgelehnt statt in eine Warteschlange gestellt).

**Verworfene Alternativen**:

- **Sliding Window** vermeidet den Burst-Effekt an Fenstergrenzen (zwei kurz
  aufeinanderfolgende Fenster könnten zusammen kurzzeitig bis zu 200 Anfragen
  durchlassen), ist aber komplexer in der Konfiguration (zusätzliche
  Segmentierung) und war im Wiki nicht gefordert — der wörtliche Wortlaut
  "100 Requests pro Minute" bildet ein Fixed-Window-Verhalten exakt ab.
- **Token Bucket** eignet sich für Anwendungsfälle mit erwünschtem
  Burst-Verhalten (kurzzeitig mehr als das Dauerlimit erlauben) — dafür gibt
  es in MyMusic keinen fachlichen Bedarf, die API wird von einer einzelnen
  Angular-Anwendung mit gleichmäßigem Anfragemuster genutzt.
- **Concurrency Limiter** begrenzt gleichzeitig laufende statt zeitbasierter
  Anfragen — anderes Problem, passt nicht zur Wiki-Vorgabe.
- Ein externes Paket (z. B. `AspNetCoreRateLimit`) wurde nicht in Erwägung
  gezogen — die Wiki-Vorgabe schließt es ausdrücklich aus, die eingebaute
  Middleware deckt den Bedarf vollständig ab.

## Entscheidung 2 — Partitionierung und Geltungsbereich

Partitionsschlüssel ist der `sub`-Claim aus dem validierten JWT — dieselbe
Claim-Quelle wie `CurrentUserService` (`Program.cs`). Fehlt der Claim (kein
oder abgelehntes Token), greift ein gemeinsamer `"anonym"`-Schlüssel als
Basisschutz; da praktisch jeder fachliche Endpunkt ohnehin
`.RequireAuthorization()` verlangt, betrifft das nur die wenigen Aufrufe vor
einem gültigen Login.

Der Limiter greift ausschließlich für Pfade unter `/api`
(`HttpContext.Request.Path.StartsWithSegments("/api")`). Aspires
`/health`/`/alive`-Endpunkte (`MyMusic.ServiceDefaults/Extensions.cs`, nur in
Development gemappt) und `/swagger` liegen außerhalb dieses Präfixes und
bleiben dadurch unlimitiert.

**Verworfene Alternative**: ein wirklich globaler Limiter ohne Pfad-Ausnahme
wurde verworfen — häufiges Health-Check-Polling durch die
Aspire-Orchestrierung (Dashboard, Ressourcen-Status) hätte sich sonst
dasselbe Kontingent wie andere unauthentifizierte Anfragen geteilt und im
Entwicklungsbetrieb zu falschen "ungesund"-Anzeigen führen können — ein
Risiko für die bestehende Aspire-Infrastruktur ohne fachlichen Nutzen.

## Entscheidung 3 — Form der 429-Antwort

`RateLimiterOptions.RejectionStatusCode` wird explizit auf 429 gesetzt (der
Default der Middleware ist sonst 503). Der `OnRejected`-Callback schreibt
zusätzlich einen `ProblemDetails`-Body im selben Stil wie
`GlobalExceptionHandler.cs` (Title "Zu viele Anfragen") sowie einen
`Retry-After`-Header aus den Limiter-Metadaten.

Der Response-Body ist für das Frontend nicht erforderlich —
`ErrorModalService.mapToState()` unterscheidet Fehlerarten bereits rein über
den HTTP-Statuscode und behandelt 429 unabhängig vom Body als
`kind: 'rate-limit'`. Der einheitliche `ProblemDetails`-Body wird trotzdem
gesetzt, damit die Antwortform über alle Fehlerfälle der API konsistent
bleibt (Swagger-Dokumentation, manuelle API-Nutzung).

## Konsequenzen

- Grenzwert (100) und Fenster (1 Minute) sind Konstanten direkt in
  `Program.cs`, nicht über `appsettings` konfigurierbar — es gibt aktuell
  keinen Bedarf, sie je Umgebung zu unterscheiden.
- `app.UseRateLimiter()` muss zwischen `app.UseAuthentication()` und
  `app.UseAuthorization()` stehen, da die Partitionierung die bereits
  authentifizierte `HttpContext.User` benötigt.
- Der neue Integrationstest `RateLimitingTests.cs` feuert bewusst echte 100
  Anfragen pro Testbenutzer (statt eines verkürzten Testlimits), um exakt das
  dokumentierte Verhalten zu verifizieren; ein zweiter Testfall sichert die
  `/api`-Pfad-Ausnahme aus Entscheidung 2 automatisiert ab (105
  unauthentifizierte Anfragen gegen `/health` bleiben alle mit 200
  erreichbar).
