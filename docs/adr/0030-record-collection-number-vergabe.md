# ADR 0030 — Vergabe der Sammlungsnummer (CollectionNumber) bei Record

**Status**: Angenommen
**Datum**: 2026-08-31
**Betrifft**: `src/MyMusic.Domain`, `src/MyMusic.Infrastructure`,
`src/MyMusic.Application`

## Kontext

`2026-08-28-notwendige-korrekturen.md` verlangt für Record ein neues,
benutzerbezogenes Feld: eine bei jedem Benutzer bei 1 beginnende fortlaufende
Ganzzahl (`CollectionNumber`), die dem Benutzer ermöglicht, seine physischen
Tonträger real durchsuchbar zu machen (z. B. per Aufkleber mit dieser
Nummer). Mit dem Projektinhaber geklärt: Eine Nummer wird einmalig beim
Anlegen vergeben und danach nie mehr verändert — nach dem Löschen eines
Records bleibt die dadurch freigewordene Nummer dauerhaft ungenutzt (kein
Auffüllen von Lücken), da sonst bereits geklebte physische Aufkleber
ungültig würden.

## Entscheidung

Die nächste Nummer wird bei jedem `POST /api/records` im
`CreateRecordCommandHandler` ermittelt: alle vorhandenen
`CollectionNumber`-Werte des anfragenden Benutzers werden über die
vorhandene Projektions-Methode `IRepository<T>.GetProjectedAsync` gelesen
(liest nur die eine `int`-Spalte, kein Laden ganzer Records inkl.
`album_cover`, gleiches Muster wie ADR 0021), und die neue Nummer ist
`Max(vorhandene Werte) + 1` (bzw. `1`, wenn noch keine existieren).

Zusätzlich sichert ein Unique-Constraint `(user_id, collection_number)` auf
Datenbankebene ab, dass innerhalb eines Benutzers keine Nummer doppelt
vergeben wird.

Bestehende Records erhalten die Nummer per Backfill in der Migration,
zugewiesen aufsteigend nach ihrer internen `id` (= Anlagereihenfolge) je
Benutzer.

## Verworfene Alternativen

**PostgreSQL-Sequenz pro Benutzer.** Native Postgres-Sequenzen sind global,
nicht partitioniert je Fremdschlüsselwert — eine Sequenz pro Benutzer müsste
entweder dynamisch per Trigger/Funktion erzeugt werden (zusätzliche
DB-seitige Logik außerhalb des über EF-Migrationen gepflegten Schemas,
schwerer nachvollziehbar) oder es bräuchte eine feste Obergrenze an
vorab angelegten Sequenzen. Beides unverhältnismäßig für eine
Einzelbenutzer-Sammlungsanwendung.

**Eigene Zähler-Tabelle je Benutzer (`user_id` → `next_collection_number`).**
Hätte eine zusätzliche Tabelle samt Synchronisationslogik erfordert und wäre
bei jedem Löschen eines Records nie wieder verringert worden (per
Anforderung gewollt) — der Zusatznutzen gegenüber der einfachen
`MAX + 1`-Abfrage über die bereits vorhandene `GetProjectedAsync`-Projektion
ist damit gering, der Wartungsaufwand höher.

**Auffüllen von Lücken (`COUNT + 1` bzw. Neunummerierung nach Löschen).**
Vom Projektinhaber ausdrücklich abgelehnt — eine Neunummerierung nach einem
Löschen würde die auf den physischen Tonträgern bereits angebrachten
Aufkleber alter Records ungültig machen.

## Konsequenzen

- Theoretische Race Condition: Zwei gleichzeitige `POST /api/records`
  desselben Benutzers könnten denselben Höchstwert lesen und dieselbe
  nächste Nummer berechnen. Der Unique-Constraint verhindert eine doppelt
  vergebene Nummer auf Datenbankebene — der zweite, zeitgleiche Request
  schlägt dann mit einem Serverfehler fehl, statt eine Dublette zu
  erzeugen. Kein Retry oder Locking implementiert; bei einer
  Einzelbenutzer-Sammlungsanwendung ohne mehrere parallele Clients pro
  Benutzer als vernachlässigbar eingestuft.
- Die Sammlungsnummer aufsteigend wird zugleich die neue Standard-Sortierung
  von `GET /api/records` (siehe `api-endpunkte.md`, `user-stories-record.md`
  US-R1/US-R3) — sie ist bewusst nicht editierbar und taucht nicht im
  Record-Formular auf, da sie ausschließlich der Wiederauffindbarkeit
  physischer Tonträger dient, nicht der fachlichen Bearbeitung.
