# Fix: Record-Detail-Modal – Breite und Track-Genre-Spalte

## Kontext

Block 6j (Track-CRUD in der Detailansicht,
`docs/prompts/2026-08-15-block-6j-tracks-frontend.md`) hat das
Records-Frontend vollständig abgeschlossen. Beim Live-Test der
Detailansicht (`RecordDetail`/`TrackList`) fand der Projektinhaber zwei
Mängel:

1. Das Detail-Modal ist mit `max-width: 460px` zu schmal — bei langen
   Künstler-/Tracknamen in der Tracklist reicht die Breite nicht aus, um
   den Inhalt sauber darzustellen.
2. In der Tracklist der Detailansicht fehlt die Anzeige des Genres je
   Track (`track.genreName`) direkt hinter „Künstler · Trackname".

**Kein Backend-Change** — `RecordTrack.genreName` liefert die API bereits
seit Block 6c/6j, reiner Frontend-Fix.

## Ist-Stand (verifiziert)

- `shared/modal/modal.ts`/`modal.html`: `app-modal` hatte keinen
  Größen-Input; `.modal` (`styles/design-system/components.css`) ist die
  einzige, global für **alle** Modals im Projekt geltende Breitenregel
  (RecordForm, LabelForm, TrackForm, ConfirmModal, ErrorModal,
  RecordDetail). Eine globale Änderung von `.modal` hätte daher alle
  Modals betroffen — nicht gewünscht, da nur die Detailansicht das
  Problem hat.
- `features/records/track-list/track-list.html`: Tabellenzeile mit drei
  Spalten (Tracknummer, Künstler/Trackname + Information, Aktionen) —
  keine Genre-Spalte, obwohl `RecordTrack.genreName` im Frontend-Modell
  bereits vorhanden ist.

## Fix

- `shared/modal/modal.ts`: neuer Input `readonly wide = input(false);`.
- `shared/modal/modal.html`: `[class.modal-wide]="wide()"` auf dem
  `.modal`-Div.
- `styles/design-system/components.css`: neue Modifier-Klasse
  `.modal-wide { max-width: 720px; }` direkt unter `.modal`. Alle anderen
  Modals bleiben unverändert bei 460px; nur `RecordDetail` setzt
  `[wide]="true"`.
- `features/records/record-detail/record-detail.html`: `<app-modal
  [wide]="true" …>`.
- `features/records/track-list/track-list.html`: `track.genreName` als
  Badge (`badge badge-neutral`, wie das bestehende Format-Badge im
  Modal-Kopf) direkt hinter „Künstler · Trackname" in derselben Zelle
  (`flex items-center gap-2`) — zunächst als eigene Tabellenspalte
  umgesetzt, nach Rückmeldung des Projektinhabers korrigiert: als eigene
  Spalte stand die Badge durch die automatische Tabellenspaltenbreite mit
  deutlich zu großem Abstand weit rechts statt direkt am Tracknamen.
- Tests: `modal.spec.ts` um zwei Fälle ergänzt (Default ohne
  `modal-wide`-Klasse, `wide=true` setzt die Klasse).
  `track-list.spec.ts`: bestehender Test „zeigt kein Genre je Track
  (nicht Teil der Detailansicht)" durch „zeigt das Genre je Track rechts
  neben Künstler und Trackname" ersetzt (prüft jetzt das Gegenteil).

## Verifikation

1. `npm test -- --watch=false` — 334 Frontend-Tests grün (332 zuvor + 2
   neue Modal-Tests).
2. `npm run build` — Production-Build erfolgreich.
3. Prettier-Check meldet projektweit auch für unveränderte
   Bestandsdateien Formatierungsabweichungen (bekannte, bereits in
   TASK.md dokumentierte CRLF-Diskrepanz unter Windows,
   `core.autocrlf=true`; CI läuft auf Linux und ist nicht betroffen) —
   an den eigenen Änderungen selbst keine über dieses Grundrauschen
   hinausgehenden Abweichungen.
4. Zeilenlängen-Check (≤120 Zeichen) der geänderten Zeilen manuell
   geprüft — neue Zeile `.modal-wide { max-width: 720px; }` deutlich
   darunter; gemeldete Überlängen in `components.css` sind ausschließlich
   vorbestehende, unveränderte Zeilen.
5. Manuelle Live-Prüfung im Browser steht aus (kein laufender
   Aspire-AppHost während der Umsetzung).

## Risiken und offene Punkte

- Keine fachliche Verhaltensänderung außer der Korrektur selbst — geringes
  Risiko.
- Manuelle Live-Prüfung (lange Künstler-/Tracknamen, Genre-Spalte optisch)
  noch nicht durchgeführt.
