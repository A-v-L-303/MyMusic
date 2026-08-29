# Fix: Tracklist im Record-Detail-Modal – maximale Größe

## Kontext

Notwendige-Korrekturen-Liste des Projektinhabers, Abschnitt „Records":
Die Tracklist im Record-Detail-Modal wird bei großen Alben (z. B. Doppel-LP)
zu groß und sprengt den sichtbaren Bereich statt intern zu scrollen
(dokumentiert per Screenshot). Zusätzlich referenziert die Korrekturnotiz
Punkt 4 des Responsive-Design-Reviews: Auf schmalen Viewports (Mobile, 375 px)
fehlt der Tracklist-Tabelle ein Fallback (weder horizontaler Scroll noch
vereinfachte Darstellung), wenn Künstlername + Tracktitel + Genre-Badge
breiter sind als die verfügbare Modalbreite.

**Ursache** (laut Responsive-Design-Review, Punkt 3): Der gemeinsame
Modal-Baustein (`shared/modal`, `components.css` `.modal`/`.modal-body`) hat
kein `max-height` und kein `overflow-y` — das betrifft strukturell jedes Modal
der Anwendung.

**Abgestimmter Scope** (Rückfrage an den Projektinhaber beantwortet): Fix
ausschließlich lokal in der Tracklist-Komponente, **keine** Änderung am
gemeinsamen `shared/modal`-Baustein. Andere Modals (z. B. das 8-Felder-
Record-Formular, ebenfalls in Punkt 3 des Reviews genannt) sind bewusst nicht
Teil dieses Korrekturschritts — das bleibt ein separates, noch offenes Thema.

**Kein Backend-Change** — reiner Frontend-Fix.

## Ist-Stand (verifiziert)

- `features/records/track-list/track-list.html`: Rendert je Plattenseite
  eine `<table class="w-full">` ohne `overflow-x-auto`-Umhüllung; die Liste
  der Seiten-Gruppen selbst hat keine Höhenbegrenzung — der umgebende
  `shared/modal` skaliert nicht selbst, sondern wächst mit dem Inhalt.
- Etablierte Präzedenzfälle im selben Projekt, die exakt dasselbe Problem
  bereits lösen:
  - Vertikale Begrenzung: `features/records/discogs-search/discogs-search.html`
    (`<ul class="flex max-h-80 flex-col gap-2 overflow-y-auto">`).
  - Horizontale Begrenzung: `features/genres/genre-table/genre-table.html`,
    `features/labels/label-table/label-table.html`,
    `features/artists/artist-table/artist-table.html`,
    `features/admin/admin-user-table/admin-user-table.html` (jeweils
    `<div class="overflow-x-auto"><table>…</table></div>`).
- `track-list.spec.ts` prüft ausschließlich Textinhalte, ARIA-Labels sowie
  die Klassen `.empty`/`.badge`/`.text-fg-subtle` — keine Abhängigkeit von der
  DOM-Verschachtelung der Gruppen-/Tabellen-Container.

## Geplanter Fix

`features/records/track-list/track-list.html`, keine weitere Datei:

1. Die Schleife über `groups()` wird in `<div class="mt-2 max-h-80 overflow-y-auto">`
   gewickelt (Wert identisch zum Discogs-Präzedenzfall). Die Überschrift
   „Tracklist" bleibt außerhalb dieses Containers und damit fix sichtbar; nur
   die Liste der Seiten/Tracks scrollt intern.
2. Die bisherige `mt-2`-Klasse auf dem Gruppen-`<div>` wird zu
   `mt-2 first:mt-0`, damit durch die neue äußere `mt-2`-Klasse kein doppelter
   Abstand vor der ersten Gruppe entsteht.
3. Jede `<table class="w-full">` wird in `<div class="overflow-x-auto">`
   gewickelt (identisch zum Muster der vier Tabellen-Views).

Keine Änderung an `track-list.ts`, `record-detail.html`, `modal.html`/
`modal.ts` oder `components.css`.

## Geplante Verifikation

1. `npm test -- --watch=false` im Frontend-Workspace — bestehende
   `track-list.spec.ts` soll unverändert grün bleiben (reine
   Layout-/CSS-Änderung, kein neues Verhalten).
2. `npm run build` — Production-Build muss weiterhin erfolgreich sein.
3. Zeilenlängen-Check (≤120 Zeichen) der geänderten Zeilen.
4. Manuelle Live-Prüfung gegen den laufenden Aspire-AppHost, falls verfügbar:
   Record mit vielen Tracks (Doppel-LP) im Detail-Modal öffnen und prüfen,
   dass die Liste intern scrollt statt die Seite zu sprengen; Browserfenster
   auf 375 px verschmälern und prüfen, dass die Tabelle bei Bedarf horizontal
   scrollt statt den Modal-Rahmen zu sprengen.

## Bekannte Risiken und offene Punkte

- Kein Fix des gemeinsamen `shared/modal`-Bausteins — die in Punkt 3 des
  Responsive-Design-Reviews beschriebene Schwäche bleibt für andere Modals
  (u. a. das Record-Formular) bestehen und ist nicht Gegenstand dieses Fixes.
- Ob eine feste Höhe von `max-h-80` (320px) für alle Bildschirmgrößen die
  richtige Grenze ist, wurde nicht gesondert geprüft — sie folgt bewusst dem
  bereits im Projekt akzeptierten Discogs-Präzedenzfall statt eines neuen,
  viewport-relativen Wertes.
