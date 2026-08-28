# ADR 0026 — Benutzerprofil-Self-Service über den bestehenden Keycloak-Service-Account

**Status**: Angenommen
**Datum**: 2026-08-28
**Betrifft**: `src/MyMusic.Application/Common/Services/IKeycloakAdminClient.cs`,
`src/MyMusic.Infrastructure/ExternalServices/Keycloak/KeycloakAdminClient.cs`,
`src/MyMusic.Application/Features/System/CurrentUser/`,
`src/MyMusic.Api/Endpoints/System/CurrentUser/MeEndpoints.cs`

## Kontext

Block 7k (Benutzerprofil) benötigt zwei neue Self-Service-Endpunkte, mit
denen der angemeldete Benutzer seine eigene E-Mail-Adresse und sein eigenes
Passwort ändern kann (siehe
`wiki/user-stories/user-stories-benutzerprofil.md`). Keycloak ist alleiniger
Identity Provider; es gibt keine eigene User-Domain-Entität.

Für Änderungen an einem Keycloak-Benutzerkonto bieten sich technisch zwei
grundsätzlich verschiedene Wege an:

1. **Keycloaks eigene Account-REST-API** (`/realms/{realm}/account/...`),
   aufgerufen mit dem eigenen Access Token des angemeldeten Benutzers. Dieser
   Weg wird von Keycloak selbst genau für den Zweck „Benutzer verwaltet sein
   eigenes Konto" vorgesehen und ist von Haus aus auf den Token-Inhaber
   beschränkt.
2. **Erweiterung des bereits bestehenden Admin-Service-Account-Clients**
   `mymusic-admin-service` (Client-Credentials-Grant, `realm-management`-
   Rollen `view-users`/`query-users`/`manage-users`, siehe ADR 0016) um zwei
   neue Methoden, die serverseitig auf die Keycloak Admin REST API zugreifen
   — dieselbe API, über die bereits der Admin-Bereich (Block 7c) fremde
   Benutzer verwaltet.

Die Keycloak-Account-REST-API wird im Projekt bislang an keiner Stelle
verwendet — ADR 0014 hält zum Login-Theme explizit fest, dass ein
`account`-Theme „bislang bewusst nicht abgedeckt" ist, und auch sonst
existiert weder ein `account`-Client-Scope noch ein `manage-account`-Bezug im
Realm-Import.

## Entscheidung

Die Self-Service-Änderungen laufen über Variante 2: `IKeycloakAdminClient`
erhält zwei neue Methoden (`UpdateEmailAsync`, `ResetPasswordAsync`), die wie
`GetUsersAsync`/`DeleteUserAsync` den bestehenden Service-Account-Client
verwenden. Die Beschränkung auf den eigenen Account erfolgt ausschließlich in
der Anwendungsschicht: Die neuen Minimal-API-Endpunkte (`PUT /api/me/email`,
`PUT /api/me/password`) setzen die `UserId` des Commands immer aus
`ICurrentUserService.UserId` (JWT-`sub`-Claim) — nie aus dem Request-Body —
bevor der Command an den Mediator geht.

**Verworfene Alternative**: Keycloaks Account-REST-API mit dem eigenen
Access Token des Benutzers einführen. Hätte den Vorteil, dass die
Beschränkung „nur der eigene Account" bereits durch Keycloak selbst über den
Token-Scope erzwungen würde, statt sich allein auf korrekten Anwendungscode
zu verlassen. Verworfen, weil dafür ein komplett neuer Integrationspfad
(neuer Client-Scope, eigene Response-Formate, eigene Fehlerbehandlung) hätte
aufgebaut werden müssen, während der Admin-REST-API-Weg bereits vollständig
etabliert, getestet und mit Least-Privilege-Rollen abgesichert ist (ADR
0016) — für zwei einzelne Feldänderungen stand der Aufwand für einen
zweiten, parallelen Keycloak-Integrationsweg in keinem Verhältnis zum
Nutzen.

## Begründung

- Kein neuer Keycloak-Client, kein neuer Client-Scope, keine neue Rolle
  nötig — der Service-Account hat mit `manage-users` bereits die für
  `PUT /admin/realms/mymusic/users/{id}` und
  `PUT /admin/realms/mymusic/users/{id}/reset-password` erforderliche
  Berechtigung (verifiziert gegen `keycloak/mymusic-realm.json`).
- Ein einziges, bereits bekanntes Fehlerbild (Admin-REST-API-Antworten) statt
  zweier unterschiedlicher Keycloak-API-Oberflächen im selben Projekt.
- Konsistent mit dem bestehenden Muster aus Block 7c (Admin-Bereich): auch
  dort läuft jede Änderung an einem Keycloak-Konto über denselben
  Service-Account, nur mit einer zusätzlichen Rollenprüfung
  (`"Admin"`-Policy) für fremde Accounts.

## Konsequenzen

- Die „nur der eigene Account"-Grenze wird ausschließlich im Anwendungscode
  durchgesetzt, nicht durch einen von Keycloak selbst scope-begrenzten
  Token: Der Service-Account-Token, den `KeycloakAdminClient` verwendet,
  könnte technisch jeden beliebigen Benutzer ändern. Ein Fehler in der
  Anwendungsschicht (z. B. eine `UserId` versehentlich aus dem Request-Body
  statt aus `ICurrentUserService` übernommen) hätte damit ein größeres
  Schadenspotenzial als bei einem echten Self-Service-Token. Dieses Risiko
  besteht im Projekt bereits seit Block 7c für Admin-Operationen und wird
  hier bewusst auf denselben Mechanismus (serverseitige Ownership-Prüfung
  statt Token-Scope) ausgedehnt.
- Ein künftiger Block, der eine „Konto endgültig löschen"- oder
  „alle Sitzungen abmelden"-Funktion für den eigenen Account einführt, kann
  demselben Muster folgen (`IKeycloakAdminClient` um eine weitere,
  self-only nutzbare Methode erweitern), statt einen neuen
  Keycloak-Integrationsweg zu bewerten.
- Sollte künftig ein echter Self-Service-Anwendungsfall entstehen, der eine
  vom Benutzer selbst initiierte, token-basierte Beschränkung erfordert (z.
  B. OAuth-Consent-Verwaltung), müsste die Keycloak-Account-REST-API separat
  evaluiert werden — diese Entscheidung deckt das nicht ab.
