# ADR 0018 — Discogs-Proxy: Token-Handhabung und Fehlerbehandlung

**Status**: Angenommen
**Datum**: 2026-08-21
**Betrifft**: `src/MyMusic.Api`, `src/MyMusic.Application`,
`src/MyMusic.Infrastructure`, `src/MyMusic.AppHost`

## Kontext

Block 8a führt den ersten rein lesenden externen REST-Proxy ohne eigene
Persistenz ein (`GET /api/discogs/search`, `GET /api/discogs/releases/{id}`,
siehe `wiki/tech-stack/discogs-api.md`,
`wiki/user-stories/user-stories-discogs.md`). Zwei Punkte haben kein
bestehendes Muster im Projekt: die Weitergabe des Discogs-Secrets an den
externen Client und die Abbildung eines Discogs-Fehlerfalls auf HTTP-Ebene.
Das bisher einzige Vorbild für einen externen HTTP-Client
(`IKeycloakAdminClient`/`KeycloakAdminClient`, ADR 0016) passt nur teilweise:
Keycloak braucht ein dynamisch zur Laufzeit ausgelesenes Secret, Discogs ein
einzelnes, statisches.

## Entscheidung 1 — Token-Handhabung

Der Discogs Personal Access Token (mit dem Projektinhaber geklärt: einzelnes,
geteiltes Server-Credential, keine Einzelbenutzer-Integration, siehe
`wiki/tech-stack/discogs-api.md`) wird als neuer Aspire-Secret-Parameter
`discogs-token` (`secret: true`) eingeführt und über
`.WithEnvironment("Discogs__Token", discogsToken)` an die `api`-Ressource
durchgereicht (`AppHost.cs`, Muster wie `keycloak-admin-password`). Anders als
beim Keycloak-Service-Account (ADR 0016) gibt es **keinen** eigenen
Provisioner/Secret-Provider — der Wert wird direkt beim `AddHttpClient`-Aufruf
in `Program.cs` als `Authorization`-Default-Header
(`AuthenticationHeaderValue("Discogs", $"token={token}")`) gesetzt, nicht als
Query-String-Parameter. Grund: `ServiceDefaults/Extensions.cs` aktiviert
`AddHttpClientInstrumentation()` — ein Token im Query-String würde in der
HTTP-Instrumentation (Seq/OpenTelemetry) landen, ein Header-Wert nicht.

Die Discogs-Base-URL (`https://api.discogs.com`) ist kein Secret und
umgebungsunabhängig — sie wird als Literal in `AppHost.cs`
(`.WithEnvironment("Discogs__BaseUrl", "https://api.discogs.com")`) gesetzt,
ohne eigenen Aspire-Parameter.

**Verworfene Alternative**: Dynamisches Auslesen/Provisionieren analog
Keycloak — verworfen, weil der Discogs-Token kein serverseitig generierbarer
Wert ist (der Benutzer muss ihn manuell in seinem Discogs-Konto erzeugen),
ein Auslese-Mechanismus zur Laufzeit hätte hier keinen Vorteil, nur
unnötige Komplexität.

## Entscheidung 2 — Statuscode und Exception-Design für Discogs-Fehler

Neue Exception-Klasse `DiscogsUnavailableException`
(`Application/Common/Exceptions/`), ein neuer `ExceptionManager.DiscogsUnavailable()`-
Eintrag, gemappt in `GlobalExceptionHandler.cs` auf **HTTP 502 Bad Gateway**.

Begründung für 502 statt der Alternativen:

- **503 Service Unavailable** wurde verworfen — impliziert, dass die eigene
  API nicht verfügbar ist; tatsächlich funktioniert MyMusic einwandfrei, nur
  die vorgelagerte Discogs-API antwortet nicht oder fehlerhaft.
- **500 (bisheriger Default-Fallback)** wurde verworfen — `wiki/architektur/
  fehler-und-ausnahmekonzept.md` führt „Externe API-Fehler (Discogs)"
  ausdrücklich als eigene Fehlerklasse mit eigener Frontend-Darstellung
  (Hinweis auf manuelle Eingabe); ein genereller 500 würde das nicht vom
  Frontend unterscheidbar machen.
- **502** ist semantisch korrekt: MyMusic tritt hier bewusst als Proxy/Gateway
  zu einem Drittsystem auf (`wiki/tech-stack/discogs-api.md`, Abschnitt
  „Designentscheidung: Serverseitiger Proxy") — 502 ist laut HTTP-Semantik
  exakt für den Fall reserviert, dass ein als Gateway agierender Server eine
  ungültige Antwort vom Upstream erhalten hat.

Jeder Discogs-Query-Handler fängt `HttpRequestException`/`JsonException`/
echten Timeout (`TaskCanceledException` ohne bereits angefordertem Abbruch)
und wirft `exceptionManager.DiscogsUnavailable()` — nicht der `DiscogsClient`
selbst. Grund: CLAUDE.md §4.3 verlangt, dass kein Handler Exceptions direkt
erzeugt, sondern immer über den `ExceptionManager`; umgekehrt kennt die
Infrastructure-Schicht im gesamten Projekt keine Application-Exceptions
(kein Präzedenzfall dafür) — das würde die bestehende Schichtentrennung
aufweichen. Eine unbekannte Discogs-Release-ID (Discogs antwortet 404) wird
ebenfalls einheitlich als `DiscogsUnavailableException`/502 behandelt, nicht
als MyMusic-eigenes 404 — das Fehlerkonzept trennt „Externe API-Fehler
(Discogs)" bewusst von „Nicht gefunden" (entitätsspezifisch für
MyMusic-eigene Ressourcen).

**Bekannte, bewusst nicht in diesem Block behobene Lücke**: `KeycloakAdminClient`
hat denselben ungefangenen `HttpRequestException`-Fall (fällt auf 500) — das
bleibt unangetastet, ein Fix wäre ein eigener, fachlich unabhängiger Schritt
(CLAUDE.md §2.3.4 „Halte Änderungen klein und thematisch geschlossen").

## Konsequenzen

- Neue Namenskategorie `Integration` in `Application/Features/` und
  `Api/Endpoints/` (bisher nur Stammdaten/Sammlung/System/Verwaltung) — passt
  zu keiner bestehenden Kategorie, da weder CRUD auf eigenen Stammdaten noch
  Admin-only.
- Kein automatisierter Test für `DiscogsClient` selbst (Teststrategie-
  Entscheidung, siehe `wiki/user-stories/user-stories-discogs.md`) — ersetzt
  durch manuelle Live-Verifikation gegen die echte Discogs-API.
- Der Discogs-Token muss vom Projektinhaber manuell erzeugt und per
  `dotnet user-secrets set "Parameters:discogs-token" "<Token>"` im
  AppHost-Projekt hinterlegt werden, bevor die API startet — ohne diesen Wert
  scheitert bereits der Aufbau des `HttpClient`-Headers beim Start.
