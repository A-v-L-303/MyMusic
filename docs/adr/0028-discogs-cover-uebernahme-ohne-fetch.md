# ADR 0028 — Discogs-Cover-Übernahme ohne fetch() auf die Data-URL

**Status**: Angenommen
**Datum**: 2026-08-31
**Betrifft**: `src/frontend`

## Kontext

Ein Live-Test des Projektinhabers zeigte zwei Discogs-Cover-Bugs: fehlende
Vorschaubilder in der Discogs-Trefferliste und ein aus Discogs angelegter
Record, der ohne Cover gespeichert wurde. Untersuchung siehe
`03 Ressourcen/2026-08-30-discogs-cover-bug-untersuchung.md` (Notiz, nicht
dauerhaft archiviert).

Für die fehlende Cover-Übernahme (zweiter Bug) ergab die Live-Verifikation
gegen den laufenden Aspire-AppHost die eigentliche Ursache: `record-form.ts`
(`applyDiscogsCover`) rief bisher `fetch(coverImageUrl)` auf die vom Backend
gelieferte `data:`-URL auf (`DiscogsRelease.CoverImageUrl`, serverseitig als
Base64-Data-URL eingebettet, siehe ADR 0020), um daraus ein `File`-Objekt für
die Vorschau und den Upload zu bauen. Block 7j hat am 2026-08-26 — vier Tage
nach der Umsetzung dieses Musters in Block 8b (2026-08-22) — eine Content
Security Policy eingeführt (ADR 0025), deren `connect-src` nur `'self'` +
Keycloak-/API-Origin erlaubt. Ein `fetch()` wird unabhängig vom URL-Schema
gegen `connect-src` geprüft — auch bei einer `data:`-URL, obwohl dabei keine
tatsächliche Netzwerkanfrage stattfindet. In der Konsole erschien dafür ein
generisches `TypeError: Failed to fetch` ohne weiteren Hinweis; der
Fehlerpfad landete im bestehenden, bewusst stillen Catch-Block (kein
Cover, keine Fehlermeldung für den Benutzer, siehe ADR 0020). Live
verifiziert: Nach dem unten beschriebenen Fix wird ein aus Discogs
angelegter Record korrekt mit Cover gespeichert (Release „Nevermind",
Nirvana, DGC, 1991).

`wiki/sicherheit/sicherheitskonzept.md` reserviert `data:` ausdrücklich nur
für `img-src`, nicht für `connect-src` — die beiden Blöcke 8b und 7j wurden
nie zusammen gegen diese Interaktion verifiziert; keiner der beiden ADRs
(0020, 0025) hat sie vorausgesehen.

## Entscheidung

`applyDiscogsCover` wandelt die Data-URL jetzt direkt in ein `File`-Objekt
um, ohne `fetch()`: neue Funktion `dataUrlToFile()`
(`src/frontend/src/app/features/records/discogs-cover.ts`) zerlegt die
Data-URL in MIME-Typ und Base64-Anteil, dekodiert den Base64-Anteil über
`atob()` und baut daraus ein `Uint8Array`/`File`. Der bestehende
`URL.createObjectURL(file)`-Mechanismus für die Vorschau bleibt unverändert
(siehe ADR 0025, Nachtrag vom 2026-08-31, für den dabei zusätzlich
gefundenen `img-src`/`blob:`-Fix).

## Verworfene Alternative

**`connect-src` um `data:` erweitern** — hätte den `fetch()`-Aufruf ebenfalls
funktionsfähig gemacht, aber die im Wiki dokumentierte CSP-Mindest-Direktive
aufgeweicht (`connect-src` ist dort bewusst auf `'self'` + Keycloak-/API-URL
begrenzt) und weiterreichende Auswirkungen als nötig gehabt (jede beliebige
`fetch()`/XHR-Anfrage im Client hätte danach auch `data:`-Ziele erreichen
können, nicht nur dieser eine Aufruf). Verworfen zugunsten einer Lösung, die
das eigentliche Problem behebt, ohne die Sicherheitsrichtlinie zu lockern:
Für eine bereits vollständig inline vorliegende Data-URL ist ohnehin keine
Netzwerkanfrage nötig.

## Konsequenzen

- Kein Netzwerk-Overhead mehr für die Cover-Übernahme (vorher ein
  zusätzlicher, im Ergebnis lokal von Chrome/CSP ohnehin blockierter
  `fetch()`-Roundtrip).
- `applyDiscogsCover` ist nicht mehr `async` (keine tatsächliche
  asynchrone Arbeit mehr nötig); der Aufruf in `onDiscogsReleaseApplied`
  entsprechend ohne `await`.
- Der bestehende, bewusst stille Fehlerpfad (ADR 0020: kein
  `DiscogsUnavailableException`/502 nur wegen des Covers) bleibt erhalten —
  ein Fehler beim Dekodieren landet weiterhin im Catch-Block und wird nur im
  Development-Modus geloggt.
- Betrifft ausschließlich den Übernahme-Pfad aus Discogs. Der ebenfalls
  behobene, architektonisch getrennte Fehler in der Discogs-Trefferliste
  (fehlende Vorschaubilder, serverseitiges Nachladen der Thumbnails als
  Data-URL analog zum Release-Cover) ist nicht Teil dieses ADRs, da dort
  keine vergleichbare Entscheidung zwischen Alternativen zu treffen war.
