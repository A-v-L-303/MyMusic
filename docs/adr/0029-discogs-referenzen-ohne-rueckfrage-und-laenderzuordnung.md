# ADR 0029 — Discogs-Referenzen ohne Rückfrage und Länderzuordnung für neues Label

**Status**: Angenommen
**Datum**: 2026-08-31
**Betrifft**: `src/MyMusic.Infrastructure`, `src/MyMusic.Application`,
`src/frontend`

## Kontext

US-DI3 (Block 8b) sah vor, dass für jeden aus einem Discogs-Treffer
übernommenen Artist, Label oder Genre, der beim Benutzer noch nicht
existiert, vor der Anlage eine Rückfrage erscheint. Der Projektinhaber hat
das per Korrekturauftrag umgekehrt: Regelfall ist die automatische Anlage
ohne Interaktion. Für ein neues Label ist das nicht ohne Weiteres möglich,
da `Label.CountryId` ein Pflicht-Fremdschlüssel ist und Discogs' Release-Feld
`country` bisher nirgends durch die Anwendung propagiert wurde.

Discogs liefert `country` als englischen Freitext (überwiegend Standard-
Ländernamen, daneben auch Sammelbegriffe wie „Europe" oder „UK & Europe").
Die eigene `country`-Tabelle führt deutsche Namen mit ISO-3166-1-Codes
(`country-referenzdaten.md`) — ein direkter Textabgleich ist damit nicht
möglich.

## Entscheidung

1. **Artist und Genre**: Referenzen aus dem Discogs-Import (Record-Artist,
   Track-Artist, Genre) werden bei fehlendem Treffer direkt angelegt, ohne
   Rückfrage. Die manuelle Artist-Eingabe im RecordForm (Freitext + Blur,
   unabhängig von Discogs) bleibt unverändert bei der Rückfrage — dafür
   existiert mit `resolveOrCreateArtistId` eine von `resolveArtistId`
   getrennte Methode nur für die Discogs-Pfade.
2. **Land für Label**: Discogs' `country`-Feld wird unverändert durch die
   Backend-Pipeline durchgereicht (`DiscogsReleaseRepresentation` →
   `DiscogsRelease` → `DiscogsReleaseResponse`), ohne Zuordnungslogik im
   Backend. Im Frontend übersetzt eine statische Tabelle
   (`discogs-country-mapping.ts`, abgeleitet aus den 238 Code/Name-Paaren
   der Wiki-Referenzliste) den üblichen englischen Ländernamen in den
   passenden ISO-Code, der dann gegen die bereits geladene
   Country-Stammdatenliste aufgelöst wird.
3. **Fallback ohne Mapping-Treffer**: Lässt sich kein Land ermitteln (kein
   Tabellentreffer, Discogs liefert keinen Wert, oder ein regionaler
   Sammelbegriff wie „Europe"), öffnet sich weiterhin das bestehende,
   vorbefüllte `LabelForm`-Modal zur manuellen Länderwahl — unverändert zum
   bisherigen Verhalten, nur nicht mehr der Regelfall.

## Verworfene Alternativen

**Externe Bibliothek zur Länder-Normalisierung.** Hätte eine neue
NuGet-/npm-Abhängigkeit erfordert (Freigabepflicht, Kap. 12
Projekt-CLAUDE.md) und deckt Discogs-Eigenheiten wie „Europe"/„UK & Europe"
ohnehin nicht ab, da diese keine gültigen ISO-Länder sind.

**Nur eine Teilliste der häufigsten Länder.** Weniger Pflegeaufwand, hätte
aber reale, aber seltenere Herkunftsländer unnötig in den Fallback gedrängt,
obwohl für sie längst ein valider `country_code` in den Stammdaten existiert.

**Stiller Platzhalter „Unbekannt" statt Fallback-Modal.** Hätte die
Automatisierung für den Ausnahmefall vollständig gemacht, aber die
Datenqualität beim Land unbemerkt verschlechtert — Land ist ein aktiv
genutztes Filter-/Suchattribut (Volltextsuche, Label-Filterung nach Land).

## Konsequenzen

- Kein neuer Endpunkt, keine Breaking Change — reine additive Erweiterung
  bestehender Discogs-DTOs um `Country`.
- Die Zuordnungstabelle deckt Standard-Ländernamen ab; ob Discogs für
  einzelne Releases abweichende Schreibweisen verwendet, die nicht in der
  Tabelle enthalten sind, kann nur die manuelle Live-Verifikation mit echten
  Discogs-Daten zeigen — solche Fälle fallen korrekt in den
  LabelForm-Fallback.
- `resolveArtistId` (manuelle Eingabe) und `resolveOrCreateArtistId`
  (Discogs-Import) teilen sich Bereinigung und Existenzprüfung als kleine,
  bewusst in Kauf genommene Code-Duplizierung, um den Scope der Änderung auf
  die Discogs-Pfade zu begrenzen.
- US-DI3 im Wiki (`user-stories-discogs.md`) und der zugehörige Abschnitt in
  `discogs-api.md` wurden entsprechend korrigiert.
