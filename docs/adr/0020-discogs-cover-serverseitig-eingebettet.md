# ADR 0020 — Discogs-Cover serverseitig geladen und eingebettet

**Status**: Angenommen
**Datum**: 2026-08-22
**Betrifft**: `src/MyMusic.Infrastructure`

## Kontext

Block 8b sollte das Discogs-Cover ursprünglich per browserseitigem `fetch()`
direkt von Discogs' Bild-CDN laden (bewusste, im Planungsgespräch getroffene
Entscheidung, um Block 8a nicht anzufassen). Die manuelle Live-Verifikation
zeigte: Das schlägt fehl — Discogs blockiert das Hotlinking von Bildern ohne
passenden `User-Agent`/Referer, das Cover-Feld im RecordForm blieb leer.

## Entscheidung

`DiscogsClient.GetReleaseAsync` lädt das Cover jetzt selbst über denselben
`HttpClient` herunter, mit dem auch die übrigen Discogs-Aufrufe laufen (fester
`User-Agent` und `Authorization`-Header aus `Program.cs`, siehe ADR 0018) und
bettet es als Base64-Data-URL in `DiscogsRelease.CoverImageUrl` ein
(`DiscogsReleaseResponse.CoverImageUrl` entsprechend). Der Browser kontaktiert
Discogs für das Bild damit nicht mehr direkt — das bestehende
Frontend-`fetch()` auf diese Data-URL funktioniert unverändert (Data-URLs
lösen im Browser ohne Netzwerkzugriff auf). Schlägt der serverseitige Download
fehl, liefert die Methode `null` — der restliche Release bleibt nutzbar, nur
ohne Cover (kein `DiscogsUnavailableException`/502 nur wegen des Covers).

## Verworfene Alternative

**Client-seitiger `fetch()` direkt auf die Discogs-Bild-URL** — ursprünglich
gewählt, um Block 8a unangetastet zu lassen. Verworfen, nachdem die
Live-Verifikation den Hotlink-Schutz von Discogs bestätigte; keine
browserseitige Umgehung möglich (Discogs prüft serverseitig, nicht per CORS).

## Konsequenzen

- Der Release-Detailabruf (`GET /api/discogs/releases/{id}`) macht jetzt
  einen zusätzlichen HTTP-Aufruf gegen Discogs' Bild-CDN, bevor er antwortet
  — etwas höhere Latenz, kein zusätzlicher Endpunkt, keine Frontend-Änderung.
  `DiscogsUnavailableException`/502 wird weiterhin nur für Fehler beim
  eigentlichen Release-Abruf geworfen, nicht für einen fehlgeschlagenen
  Cover-Download.
- Kein Wechsel der Feld-Semantik nach außen (`CoverImageUrl` bleibt ein
  String, der sich direkt in ein `<img src>`/`fetch()` einsetzen lässt) —
  nur die interne Herkunft ändert sich von einer externen URL zu eingebetteten
  Bilddaten.
