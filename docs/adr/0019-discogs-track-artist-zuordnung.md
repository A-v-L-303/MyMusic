# ADR 0019 — Discogs-Track-Artist-Zuordnung

**Status**: Angenommen
**Datum**: 2026-08-22
**Betrifft**: `src/MyMusic.Application`, `src/MyMusic.Infrastructure`,
`src/frontend`

## Kontext

Block 8b verbindet die in Block 8a bereitgestellten Discogs-Endpunkte mit dem
RecordForm. Nach Auswahl eines Discogs-Treffers werden automatisch nicht nur
Albumname, Erscheinungsjahr, Label, Record-Artist und Cover übernommen,
sondern auch alle Tracks der Discogs-Tracklist — jeder Track braucht dabei
einen `artistId` (Pflichtfeld auf `record_track`, siehe
`wiki/domain/record-track.md`, Abschnitt „Hinweis: Doppelte Artist-Referenz").

Zwei fachliche Fälle unterscheiden sich:

- **Album mit durchgehendem Hauptkünstler**: Alle Tracks haben denselben
  Artist wie das Release. Discogs liefert in diesem Fall in seinen Rohdaten
  auch kein separates Pro-Track-Artist-Feld, weil keines nötig ist.
- **Compilation (Various Artists)**: Jeder Track kann einen eigenen, vom
  Record-Artist abweichenden Artist haben (1 Record-Artist, x
  Track-Artists). Discogs bildet das bei entsprechenden Releases über ein
  `artists`-Array je Tracklist-Eintrag ab (dieselbe Form wie das
  Release-Artists-Array).

Block 8a (`DiscogsTrackResponse`) kennt bisher nur `Position`, `Title`,
`Duration` — kein Pro-Track-Artist.

## Entscheidung

`DiscogsTrackResponse` (und die zugrunde liegenden Schichten
`DiscogsTrack`/`DiscogsTrackRepresentation`) werden um ein optionales Feld
`Artist: string?` erweitert. Der `DiscogsClient` befüllt es aus Discogs'
Pro-Track-`artists`-Array, sofern vorhanden (mehrere Namen werden wie beim
bestehenden Label-/Artist-Mapping mit `", "` verkettet); ist das Array leer
oder fehlt es, bleibt das Feld `null`.

Die Fallback-Logik liegt bewusst im **Frontend**, nicht im Backend: Ist
`track.artist` `null`, verwendet das RecordForm beim automatischen
Track-Import den bereits aufgelösten Record-Artist. Das Backend liefert nur
durch, was Discogs tatsächlich liefert, ohne selbst zu interpretieren.

## Verworfene Alternative

**Immer der Record-Artist für jeden Track, keine Backend-Erweiterung.**
Einfacher umzusetzen (keine Änderung an einem bereits gemergten Block
nötig), wurde aber mit dem Projektinhaber ausdrücklich verworfen: Bei
Compilations wäre der automatisch gesetzte Track-Artist dann fachlich
falsch — genau der Fall, den die automatische Übernahme eigentlich
vermeiden soll (Ziel: „die Ersterfassung beschleunigen", nicht falsche
Daten erzeugen, die der Benutzer nachträglich korrigieren muss).

## Konsequenzen

- Abwärtskompatible Erweiterung einer bestehenden Response — kein neuer
  Endpunkt, keine Breaking Change für bestehende Aufrufer.
- Die genaue Discogs-JSON-Struktur für das Pro-Track-`artists`-Array ist aus
  der öffentlichen API-Dokumentation abgeleitet, nicht vorab live
  verifiziert (dieselbe bekannte Einschränkung wie bereits in ADR 0018 für
  die übrigen Discogs-Mappings dokumentiert) — wird bei der manuellen
  Live-Verifikation von Block 8b gegen einen echten Various-Artists-Sampler
  bestätigt oder korrigiert.
- Das Frontend braucht eine Pro-Track-Referenzauflösung (Existenzprüfung je
  distinktem Track-Artist-Namen, Rückfrage bei Neuanlage) statt einer
  einmaligen Auflösung für den gesamten Record — mehr Komplexität im
  RecordForm, aber fachlich notwendig.
- Liefert Discogs bei einer Compilation keine oder unvollständige
  Pro-Track-Artists, bleibt das eine Grenze der Discogs-Datenqualität; kein
  eigener Korrekturmechanismus in MyMusic vorgesehen.
