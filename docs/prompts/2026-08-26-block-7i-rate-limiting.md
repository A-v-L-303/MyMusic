# Block 7i: Rate Limiting

## Kontext

Laut `TASK.md` (Abschnitt 7, "Aufgaben (noch offen)") und dem Wiki
(`sicherheit/sicherheitskonzept.md`, Zeile 26/133-136) ist Rate Limiting einer
der letzten offenen Sicherheitspunkte aus dem MVP-Umfang, nachdem mit Block 10
(Volltext-Suche) alle fachlichen Features abgeschlossen sind. Wiki-Vorgabe:

> Globales Limit per authentifiziertem Benutzer (`userId` aus JWT) via
> eingebauter ASP.NET Core Middleware (`Microsoft.AspNetCore.RateLimiting`).
> Kein externes Paket notwendig. Limit: 100 Requests pro Minute pro Benutzer.
> Bei Überschreitung: HTTP 429.

Aktueller Iststand (`src/MyMusic.Api/Program.cs`): keine Rate-Limiting-Middleware
vorhanden. Die Middleware-Pipeline ist aktuell:
`UseExceptionHandler → UseSerilogRequestLogging → [Dev: UseCors] →
UseAuthentication → UseAuthorization → [Dev: Swagger] → Endpoint-Mappings`.

Das Frontend ist bereits vorbereitet: `ErrorModalService.mapToState()`
(`src/frontend/src/app/shared/error-modal/error-modal.service.ts:78-80`)
behandelt HTTP 429 bereits als `kind: 'rate-limit'` mit eigener Modal-Meldung —
rein auf Basis des Statuscodes, unabhängig vom Response-Body.

## Entscheidungen

1. **Technologie**: `Microsoft.AspNetCore.RateLimiting` (Teil von ASP.NET Core,
   kein neues NuGet-Paket) — exakt wie im Wiki gefordert.
2. **Algorithmus**: Fixed-Window-Limiter (`AddFixedWindowLimiter`-Äquivalent
   über `RateLimitPartition.GetFixedWindowLimiter`). Einfachste Umsetzung, die
   "100 Requests/Minute" wörtlich abbildet. Alternativen (Sliding Window,
   Token Bucket) wären präziser bei Burst-Verhalten, aber vom Wiki nicht
   gefordert und unnötig komplex — wird als Trade-off in einem neuen ADR 0022
   festgehalten.
3. **Partitionierung**: Schlüssel ist der `sub`-Claim aus dem JWT (dieselbe
   Claim-Quelle wie `CurrentUserService.cs:9`), damit jeder Benutzer sein
   eigenes Kontingent hat. Fehlt der Claim (kein/ungültiges Token), greift ein
   gemeinsamer `"anonym"`-Partitionsschlüssel als Schutz vor unautorisiertem
   Anfrageflut — betrifft praktisch nur die wenigen Aufrufe vor einem gültigen
   Login, da nahezu jeder Endpunkt ohnehin `.RequireAuthorization()` hat.
4. **Geltungsbereich**: Nur Pfade unter `/api` werden limitiert (Prüfung via
   `HttpContext.Request.Path.StartsWithSegments("/api")`). Grund: Aspires
   `/health`/`/alive`-Endpunkte (`MyMusic.ServiceDefaults/Extensions.cs:126,129`,
   nur in Development gemappt) und `/swagger` liegen außerhalb von `/api` und
   dürfen nicht durch häufiges Dashboard-/Health-Polling ausgebremst werden —
   das ist eine Erweiterung über den wörtlichen Wiki-Text hinaus, aber
   notwendig, um die bestehende Aspire-Orchestrierung nicht zu brechen.
5. **Response bei Überschreitung**: `RejectionStatusCode = 429` (Default der
   Middleware ist sonst 503) plus ein `ProblemDetails`-Body im selben Stil wie
   `GlobalExceptionHandler.cs` (Title "Zu viele Anfragen"), zusätzlich ein
   `Retry-After`-Header aus den Limiter-Metadaten. Der Response-Body wird vom
   Frontend aktuell nicht ausgewertet (nur der Statuscode), dient aber der
   Konsistenz für Swagger/manuelle API-Nutzung.
6. **Keine Konfigurierbarkeit über `appsettings`**: Grenzwert (100) und Fenster
   (1 Minute) werden als lokale Konstanten direkt in `Program.cs` gesetzt,
   analog zu bestehenden Literalen dort (z. B. CORS-Methodenliste). Der
   Integrationstest prüft die echten 100 Requests/Minute, keine verkürzte
   Testkonfiguration nötig (siehe Verifikation).

## Umsetzung

1. **Branch**: `block-7i-rate-limiting` von `main` (aktuell sauberer Stand auf
   `main`, siehe Git-Status-Prüfung).
2. **Arbeits-Prompt**: `docs/prompts/2026-08-26-block-7i-rate-limiting.md`
   anlegen (Inhalt = dieser Plan), vor Umsetzungsbeginn geschrieben und danach
   nicht mehr verändert (CLAUDE.md §2.3).
3. **`src/MyMusic.Api/GlobalUsing.cs`**: ergänzen um
   `global using Microsoft.AspNetCore.RateLimiting;` und
   `global using System.Threading.RateLimiting;`.
4. **`src/MyMusic.Api/Program.cs`**:
   - Neue Registrierung `builder.Services.AddRateLimiter(options => { ... })`
     vor `builder.Services.AddEndpointsApiExplorer()`:
     - `options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;`
     - `options.OnRejected = async (context, cancellationToken) => { ... }`
       schreibt `ProblemDetails` + `Retry-After`-Header.
     - `options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext => { ... })`
       mit der `/api`-Pfadprüfung, `sub`-Claim-Partitionierung und
       `FixedWindowRateLimiterOptions { PermitLimit = 100, Window = 1 Minute,
       QueueLimit = 0, QueueProcessingOrder = OldestFirst }`.
   - In der Pipeline `app.UseRateLimiter();` zwischen `app.UseAuthentication();`
     und `app.UseAuthorization();` einfügen (Partitionierung braucht die schon
     authentifizierte `HttpContext.User`).
5. **Neuer Integrationstest** `tests/MyMusic.IntegrationTests/RateLimitingTests.cs`
   (Muster wie `MeEndpointTests.cs`/`CorsPolicyTests.cs`: eigener AppHost pro
   Testmethode, `KeycloakTestClient.CreateTestUserAsync`/
   `RequestUserAccessTokenAsync`/`DeleteTestUserAsync`):
   - Ein Testbenutzer feuert 100 authentifizierte `GET /api/me`-Aufrufe
     (alle 200), der 101. Aufruf liefert 429.
   - Ein zweiter, unabhängiger Testbenutzer erhält im selben Testlauf weiterhin
     200 auf `GET /api/me` — belegt die Partitionierung pro Benutzer
     (Mandantentrennung-Analogie, CLAUDE.md §10.2).
   - Aufräumen beider Testbenutzer in `finally`.
6. **ADR** `docs/adr/0022-rate-limiting.md`: Fixed-Window-Entscheidung,
   Partitionierung über `sub`-Claim, `/api`-Scoping-Grund, verworfene
   Alternativen (Sliding Window, Token Bucket, externes Paket).
7. **Dokumentation**:
   - `TASK.md`: neuen Abschnitt `### 7i. Rate Limiting` nach dem Muster der
     bestehenden Unterabschnitte (Status, Arbeits-Prompt-Verweis, Umgesetzt,
     Abnahmekriterium); "Rate Limiting" aus der offenen Liste in Abschnitt 7
     (Zeile ~2125) sowie aus dem "Aktuell nicht umgesetzt"-Abschnitt oben
     entfernen.
   - `README.md`: neuer Abschnitt `### Rate Limiting (Block 7i)` im Stil der
     bestehenden Block-Abschnitte.
   - `../../MyMusic/CLAUDE.md` (Projekt-Wurzel): neuer "Stand"-Absatz im
     Projektstatus-Abschnitt, "Rate Limiting" aus der abschließenden
     "Offen sind ..."-Aufzählung entfernen.
   - Repo-`CLAUDE.md` §5.3 beschreibt die Anforderung bereits korrekt (100
     req/min, eingebaute Middleware, HTTP 429) — keine Änderung nötig, nur zur
     Kenntnis genommen.
8. Build, Tests (Unit + der neue Integrationstest, plus vollständige
   `MyMusic.IntegrationTests`-Suite zur Regressionsprüfung), `dotnet format
   --verify-no-changes` vor Abschluss.

## Verifikation

- `dotnet build MyMusic.slnx`
- `dotnet test MyMusic.slnx` (inkl. neuem `RateLimitingTests`, bestehende
  Suite muss weiterhin grün bleiben — insbesondere `CorsPolicyTests` und
  `MeEndpointTests`, die denselben Pipeline-Abschnitt berühren)
- `dotnet format --verify-no-changes`
- Manuelle Live-Prüfung gegen den laufenden Aspire-AppHost: mit einem echten
  Token schnell >100 Anfragen gegen `/api/me` senden (z. B. kleines PowerShell-
  Loop-Skript) und den 429 samt `Retry-After`-Header in den Response-Headern
  prüfen; parallel bestätigen, dass `/health`, `/alive` und `/swagger`
  weiterhin uneingeschränkt erreichbar bleiben.

## Freigabepflichtige Folgeschritte (nicht Teil dieser Umsetzung)

Branch-Anlage, Commits, Push, Pull Request und Merge bleiben nach CLAUDE.md
§2.2/§2.4 gesondert freigabepflichtig und werden erst nach Abschluss der
Implementierung und Tests angefragt.
