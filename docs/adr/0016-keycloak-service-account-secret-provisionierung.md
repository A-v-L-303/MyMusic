# ADR 0016 — Keycloak-Service-Account-Secret: generiert statt selbst verwaltet

**Status**: Angenommen
**Datum**: 2026-08-17
**Betrifft**: `keycloak/mymusic-realm.json`, `src/MyMusic.Api`,
`src/MyMusic.Infrastructure`, `src/MyMusic.AppHost`

## Kontext

Für den Admin-Bereich (Block 7c) muss die API die Keycloak Admin REST API
aufrufen (Benutzerliste, Benutzer löschen). Mit dem Projektinhaber am
2026-08-16 bereits geklärt (`wiki/user-stories/user-stories-admin.md`): dafür
wird ein eigener, dedizierter Service-Account-Client verwendet
(Client-Credentials-Grant, minimale `realm-management`-Rollen — siehe
Nachtrag unten zur genauen Rollenliste) — nicht die bestehenden
Bootstrap-Admin-Credentials des Realm-Imports (Prinzip der geringsten
Berechtigung).

Offen blieb dabei die Frage, wie dieser Client an sein Secret kommt. Beide
bisherigen Keycloak-Clients (`mymusic-angular`, `mymusic-integration-tests`)
sind `publicClient: true` und haben nie ein Secret benötigt — `mymusic-
realm.json` ist ein versioniertes, öffentlich einsehbares Repository-Artefakt
und darf laut CLAUDE.md §5.4/Regeln kein Secret enthalten. Ein Client mit
Client-Credentials-Grant ist aber zwingend confidential und braucht eines.

## Entscheidung

Der neue Client `mymusic-admin-service` wird **ohne** `"secret"`-Feld im
Realm-Import angelegt — Keycloak generiert beim Import automatisch ein
zufälliges Secret. Die API liest dieses Secret beim Start einmalig über die
Keycloak Admin REST API aus (`GET /admin/realms/mymusic/clients/{id}/
client-secret`), authentifiziert sich dafür transient mit den bereits
bestehenden Bootstrap-Admin-Credentials (`admin` / Aspire-Parameter
`keycloak-admin-password`) und hält es für die Prozesslaufzeit im Speicher
(`KeycloakServiceAccountSecretProvider`, Singleton). Die
Bootstrap-Admin-Credentials wurden dafür zusätzlich an die API-Ressource
durchgereicht (`AppHost.cs`) — bisher gingen sie ausschließlich an den
Keycloak-Container selbst.

**Mit dem Projektinhaber verworfene Alternative**: Ein eigener neuer
Aspire-Secret-Parameter (`keycloak-admin-service-secret`, analog zu
`keycloak-admin-password`), den die API beim Start aktiv per
Admin-API-Aufruf (`PUT` auf die Client-Repräsentation) auf den bekannten Wert
setzt. Hätte den Vorteil, dass der Wert wie andere Secrets über
`dotnet user-secrets` rotierbar ist. Verworfen zugunsten der einfacheren
Variante ohne zusätzlichen Aspire-Parameter und ohne schreibenden
Client-Update-Aufruf beim Start.

## Begründung

- Kein neues Secret zu verwalten — der bereits bestehende Aspire-Parameter
  `keycloak-admin-password` wird lediglich zusätzlich an die API
  durchgereicht, statt einen weiteren Parameter samt User-Secrets-Eintrag
  einzuführen.
- Nur ein lesender Admin-API-Aufruf beim Start (`GET .../client-secret`),
  kein schreibender Eingriff in die Client-Konfiguration.
- Least Privilege bleibt für den laufenden Betrieb gewahrt: Die
  Bootstrap-Admin-Credentials werden nur transient beim Start verwendet, um
  das Secret des Service-Accounts zu lesen — alle eigentlichen
  Admin-Operationen (Benutzerliste, Löschung) laufen über den
  Service-Account mit seinen minimalen `realm-management`-Rollen, nicht über
  die Bootstrap-Admin-Session.

## Konsequenzen

- Startet die API, während Keycloak nicht erreichbar ist, die
  Bootstrap-Admin-Credentials falsch konfiguriert sind oder der Client noch
  nicht existiert, schlägt ausschließlich das Secret-Laden fehl — die API
  selbst startet trotzdem (Warnung statt Absturz, siehe Nachtrag unten).
  Nur die Admin-Endpunkte sind dann bis zum nächsten erfolgreichen Start
  nicht funktionsfähig (500).
- Ein Neustart von Keycloak mit frischem, leerem Datenvolume erzeugt beim
  nächsten Realm-Import ein neues, zufälliges Secret — das ist unschädlich,
  da die API es bei jedem eigenen Neustart ohnehin neu ausliest, nie
  cached über einen Prozessneustart hinweg.

## Nachtrag (2026-08-17): Startup-Fehler nicht fatal, zusätzliche Rolle nötig

Bei der ersten Live-Verifikation gegen einen echten, bereits mit einem
älteren Realm-Stand befüllten `mymusic-keycloak-data`-Datenvolume (lokale
Entwicklungsumgebung, mehrere Vorsitzungen) zeigte sich: `--import-realm`
überspringt bereits importierte Realms (`IGNORE_EXISTING`, dieselbe
Einschränkung wie in ADR 0014/Block 7f dokumentiert) — der neue Client
`mymusic-admin-service` aus der aktualisierten `mymusic-realm.json` wurde
dadurch nicht automatisch angelegt. Ursprünglich ließ `Program.cs` die API
in diesem Fall abstürzen (`LoadSecretAsync` unbehandelt geworfen) — das
riss die komplette Anwendung mit, obwohl nur der Admin-Bereich betroffen
war. Fix: Der Aufruf ist jetzt in einen `try/catch` gefasst, ein
Fehlschlag wird nur als Warnung geloggt; alle anderen Endpunkte bleiben
funktionsfähig.

Der fehlende Client wurde für die lokale Entwicklungsumgebung einmalig
manuell über `kcadm.sh` im laufenden Container nachgetragen (additiv, ohne
das Datenvolume zu löschen) — derselbe Verifikations-/Nachzieh-Mechanismus
wie in ADR 0014 für das Login-Theme beschrieben.

Dabei zeigte sich außerdem empirisch, dass `view-users` und `manage-users`
allein nicht ausreichen: `GET /admin/realms/mymusic/users` (Benutzerliste)
verlangt zusätzlich die Rolle `query-users`. Die realm-management-Rollen
des Service-Accounts sind daher `view-users`, `query-users`,
`manage-users` (`mymusic-realm.json` entsprechend aktualisiert).

Der ursprünglich geplante Endpunkt `GET /admin/realms/{realm}/roles/{rolle}/
users` (Mitglieder einer Rolle) lieferte trotz aller drei Rollen weiterhin
403 — vermutlich durch Keycloaks Fine-Grained-Admin-Permissions nicht über
einfache `realm-management`-Client-Rollen abdeckbar. Statt diese weiter zu
verfolgen, wurde `KeycloakAdminClient.GetUsersAsync` umgestellt: Die
Admin-Rolle wird jetzt je Benutzer über
`GET /admin/realms/{realm}/users/{id}/role-mappings/realm` ermittelt (ein
zusätzlicher Aufruf pro Benutzer statt eines einzelnen Aufrufs für alle) —
funktioniert mit denselben drei Rollen zuverlässig. Für die erwartete
Benutzerzahl einer privaten Sammlungsverwaltung ist das vertretbar.
