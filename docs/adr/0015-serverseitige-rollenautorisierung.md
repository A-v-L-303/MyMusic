# ADR 0015 — Serverseitige Rollenautorisierung über den rohen realm_access-Claim

**Status**: Angenommen
**Datum**: 2026-08-17
**Betrifft**: `src/MyMusic.Api`

## Kontext

Bis Block 7c gab es serverseitig keinerlei Rollenprüfung — `Program.cs` rief
`AddAuthorization()` ohne jede Policy auf, alle Endpoint-Gruppen nutzten nur
ein rollenloses `.RequireAuthorization()`. Die Rolle `Admin` wurde bislang
ausschließlich clientseitig ausgewertet (`AdminGuard`, Block 7b), um den
Admin-Button und die `/admin`-Route auszublenden — ein reiner
UI-Komfortmechanismus ohne jede Durchsetzung auf API-Ebene.

Mit den neuen Endpunkten `GET /api/admin/users` und
`DELETE /api/admin/users/{id}` (Block 7c) muss die Rolle erstmals serverseitig
geprüft werden. `AddAuthentication().AddJwtBearer()` (Block 0b) ist bereits
mit `MapInboundClaims = false` konfiguriert (ADR 0004) — Claims aus dem JWT
werden also unter ihrem rohen Keycloak-Namen bereitgestellt, nicht auf
.NET-Standardclaims wie `ClaimTypes.Role` übersetzt. Die Rolleninformation
selbst liegt im `realm_access`-Claim als JSON-Objekt (`{"roles": [...]}`),
nicht als eigener, wiederholbarer Claim je Rolle — das native
`[Authorize(Roles = "Admin")]`/`policy.RequireRole(...)`-Muster von
ASP.NET Core setzt aber genau das voraus (einen Claim vom Typ
`ClaimTypes.Role` je Rolle) und funktioniert damit hier nicht ohne Weiteres.

## Entscheidung

Ein eigener, schlanker `AuthorizationHandler<AdminRequirement>`
(`src/MyMusic.Api/Authorization/`) liest `realm_access` direkt aus dem
`ClaimsPrincipal`, parst das JSON und prüft, ob `"Admin"` im `roles`-Array
enthalten ist. Registriert über
`AddAuthorizationBuilder().AddPolicy("Admin", policy =>
policy.RequireAuthenticatedUser().AddRequirements(new AdminRequirement()))`.
Die neuen Admin-Endpunkte nutzen `.RequireAuthorization("Admin")` statt des
bisherigen rollenlosen `.RequireAuthorization()`.

**Verworfene Alternative**: `MapInboundClaims = true` setzen (oder gezielt nur
für `realm_access` eine Claim-Transformation registrieren), damit ASP.NET Core
die Rolle automatisch in `ClaimTypes.Role`-Claims übersetzt und das
eingebaute `RequireRole(...)` nutzbar wird. Verworfen, weil
`MapInboundClaims = false` eine bewusste, dokumentierte Entscheidung aus
Block 0b ist (ADR 0004) und alle bestehenden Stellen, die Claims lesen
(`CurrentUserService.UserId` über den rohen `sub`-Claim), sich darauf
verlassen — eine Änderung hätte einen deutlich größeren Blast-Radius als ein
lokal begrenzter, neuer `AuthorizationHandler`.

`.RequireAuthenticatedUser()` ist Teil der Policy-Definition, nicht optional:
ohne ihn liefert ein nicht authentifizierter Aufruf 403 statt der laut
`user-stories-admin.md` (US-AD2) geforderten 401 — der
`AdminRequirement`-Handler allein unterscheidet nicht zwischen „kein Token"
und „Token ohne Admin-Rolle", da beide Fälle zu keinem `Succeed()` führen.

## Begründung

- Kleinster Eingriff: Kein bestehendes JWT-Bearer-Verhalten wird verändert,
  nur eine neue, additive Policy kommt hinzu.
- Fail-closed: Fehlt der Claim oder ist er nicht parsebar, wird kein
  `Succeed()` aufgerufen — das Ergebnis ist immer eine Verweigerung, nie ein
  ungeprüfter Zugriff oder eine unbehandelte Exception (kein 500 bei
  fehlerhaftem Claim-Inhalt).

## Konsequenzen

- Jede künftige, weitere rollenbeschränkte Route kann dieselbe `"Admin"`-Policy
  wiederverwenden, ohne den `AuthorizationHandler` zu duplizieren.
- Sollte künftig eine dritte Rolle nötig werden, muss der Handler auf ein
  parametrisiertes `RoleRequirement(string RoleName)` verallgemeinert werden —
  für aktuell zwei Rollen (`User`, `Admin`) ist das bewusst noch nicht
  vorweggenommen.
