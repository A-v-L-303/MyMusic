# Block 4 (Angular) — Slice Label, Frontend

## Kontext

Label ist laut TASK.md (Abschnitt 4) der nächste vorgesehene Angular-Feature-Slice
nach Genre — das Backend ist seit 2026-08-07 abgeschlossen und wurde bislang
zurückgestellt, bis Block 0c/0g (Angular-Workspace, Navigation) und ein
Referenz-Slice vorlagen. Beides ist jetzt erfüllt: Genre (Block 2 Frontend,
PR #47) hat die Muster etabliert, die Label 1:1 übernehmen soll (Signal Forms,
`rxResource`, `ErrorModalService`, `shared/`-Bausteine). `features/labels/`
enthält aktuell nur eine leere Platzhalter-Komponente; Route und Nav-Eintrag
(„Labels", `LucideTag`) sind bereits vollständig verdrahtet.

Label unterscheidet sich von Genre in zwei Punkten, die neue Muster brauchen:
ein Fremdschlüssel-Feld (`countryId`, referenziert `Country`) und ein
optionales Freitextfeld (`information`). Beides ist nicht durch den
Genre-Slice abgedeckt.

**Kein Backend-Change.** Label-Backend (inkl. der bereits mit Block 6d aktiven
Referenzprüfung beim Löschen) wird unverändert konsumiert.

## Mit dem Projektinhaber geklärt (2026-08-13)

**Länderauswahl (Formular + Filter)**: Natives HTML-`<select>` statt einer
neuen Searchable-Combobox-Komponente. Begründung:
- `components.css` enthält bereits eine `.select`-Klasse (Chevron-Icon,
  Fehlerzustand, identisches Styling zu `.input`) — kein neuer CSS-Code nötig.
- Verifiziert gegen die installierten Typdefinitionen
  (`node_modules/@angular/forms/fesm2022/signals.mjs:1390`): Der
  `[formField]`-Direktiven-Fehlertext nennt `<select>` explizit als gültigen
  nativen Host neben `<input>`/`<textarea>` — funktioniert also identisch zum
  bereits etablierten Genre-Muster.
- Eine Combobox wäre für einen Referenz-Slice unverhältnismäßiger Mehraufwand
  ohne Design-System-Vorlage.

Diese Entscheidung löst die im Wiki identifizierte Planungslücke (keine
UI-Vorgabe für die Länderauswahl) und wird im Rahmen der Dokumentation
(Schritt 9) in `wiki/architektur/ui-ux-konzept.md` nachgetragen.

## Ist-Stand (verifiziert)

- `features/labels/labels.ts` (+`.html`, `.routes.ts`, `.spec.ts`) ist eine
  reine Platzhalter-Komponente, strukturell identisch zum vormaligen
  Genre-Platzhalter. `labels.routes.ts` bindet bereits `{ path: '', component:
  Labels }`, `app.routes.ts` lädt `labelsRoutes` bereits lazy, `nav.ts`
  verlinkt „Labels" bereits im Options-Dropdown — nichts davon ändert sich.
- `shared/` (aus Block 2 Frontend) wird **unverändert wiederverwendet**, kein
  neuer Baustein nötig: `shared/modal/`, `shared/confirm-modal/`,
  `shared/error-modal/` (Service + Komponente, bereits global in `app.html`
  gemountet), `shared/pagination/`, `shared/http/problem-details.ts`.
- Signal-Forms-Validierung (`required`, `minLength`, `maxLength`, `pattern`,
  `debounce`, `submit`) und `rxResource` sind seit Genre im Repo etabliert und
  bleiben unverändert nutzbar.
- **Debounce-Mechanik pro Feld verifiziert**
  (`node_modules/@angular/forms/fesm2022/signals.mjs:609`): `debounce(path,
  config)` registriert den Debouncer als Metadata-Regel auf dem jeweiligen
  Pfad-Node — unabhängig je Feld. Für den Label-Filter bedeutet das: `debounce
  (path.name, 300)` bleibt wie bei Genre bestehen, `path.countryId` bekommt
  **keine** Debounce-Regel (Default `immediate`) — ein Auswahlwechsel im
  Select löst sofort eine Filteränderung aus, während das Namensfeld weiter
  mit 300 ms entprellt wird. Beide Felder können im selben `filterModel`-
  Signal koexistieren, der äußere `effect()` (wie bei Genre) liest das
  gesamte Modell und emittiert bei jeder Feld-Änderung.
- API-Vertrag Label (aus Backend-Code gelesen, verbindlich):
  - `GET /api/labels?page&pageSize&name&countryId` → `200 { items:
    {id,name,countryId,countryName,information}[], totalCount, page,
    pageSize, totalPages }`. `name`: case-insensitive Contains-Filter.
    `countryId`: exakter Treffer. Beide Filter kombinierbar, beide optional.
    Sortierung serverseitig fest nach Name (kein `sortBy`, wie bei Genre).
    `page`/`pageSize` serverseitig geklemmt (min 1 / 1–100).
  - `POST /api/labels` Body `{ name, countryId, information }` → `201
    { id, name, countryId, countryName, information }`.
    `PUT /api/labels/{id}` Body `{ name, countryId, information }` → `200`
    (gleiche Form). `DELETE /api/labels/{id}` → `204`.
  - **`countryName` wird serverseitig aufgelöst und mitgeliefert** — die
    Tabelle braucht keinen Client-Join gegen die Länderliste; nur das
    Formular/der Filter brauchen `GET /api/countries` für die Select-Optionen.
  - Validierung: `name` Pflicht, 1–60 Zeichen, Regex
    `^[\p{L}\p{N} \-&'./]+$` (JS: `/^[\p{L}\p{N} \-&'./]+$/u` — gegenüber
    Genre zusätzlich `.` und `/`, bewusst ohne Klammern). `countryId`:
    Pflicht, muss ein existierendes Land referenzieren (asynchrone
    Server-Prüfung → **400**, nicht 404, bei ungültiger/unbekannter Id).
    `information`: optional, max. 255 Zeichen, keine Zeichensatzbeschränkung.
  - 400-Fehlerformat wie bei Genre: `{ errors: { <PascalCase-Feldname>:
    string[] }, title, status }`. Feldnamen entsprechen den
    Command-Properties (`Name`, `CountryId`, `Information`) — dieselbe
    FluentValidation-Konvention wie bei Genre (`"Name"`), hier erstmals mit
    drei möglichen Feldschlüsseln statt einem. 404/409/500:
    `{ title, detail, status }`. 409 bei Create/Update = Name-Duplikat pro
    Benutzer; 409 bei Delete = Label wird noch von mindestens einem `Record`
    referenziert (Prüfung seit Block 6d/PR #34 aktiv, anders als der historisch
    unsichere Stand in den User Stories) — die 409-Meldung liefert der Server
    bereits fertig formuliert im `detail`-Feld, `ErrorModalService` zeigt sie
    unverändert an.
  - `GET /api/countries` (kein Paging/Filter) → `200` Array von `{ id, name,
    code }`, alphabetisch nach `name` sortiert, 238 Einträge. Kein
    CRUD-Endpunkt für Country.
- Fehlerdarstellung exakt wie Genre (`ErrorModalService`): 400 → Inline am
  jeweiligen Feld (jetzt für drei Felder statt einem). 404/409/500 → Modal mit
  OK. Netzwerkfehler → Modal mit „Erneut versuchen". 401/403 → bereits global
  behandelt. Delete → eigenes Sicherheits-Modal — **außer** das Label wird
  noch referenziert: dann direkt Fehler-Modal ohne vorherige Sicherheitsabfrage
  (US-L5, da die Aktion ohnehin nicht ausgeführt werden kann). Das lässt sich
  mit dem bestehenden `ConfirmModal`-Wiring nicht ohne Weiteres vorab prüfen
  (der Client kennt vor dem Löschversuch nicht, ob eine Referenz besteht) —
  praktikable Umsetzung: Sicherheitsabfrage wird wie bei Genre immer zuerst
  gezeigt, ein 409 bei Bestätigung geht an `ErrorModalService` (Verhalten
  entspricht dem etablierten Genre-Muster; die US-L5-Formulierung „keine
  Sicherheitsabfrage bei Referenz" wird dadurch nicht exakt erfüllt, da der
  Client die Referenz vorab nicht kennt — als Risiko unten vermerkt).
- Layout-Vorgabe (`architektur/ui-ux-konzept.md`, „Tabellen-Slices"): wie
  Genre (Container 1080px, Toolbar-Muster, `.empty`/`.spinner`, Paginierung
  unter der Tabelle), zusätzlich für Label ausdrücklich zwei Filterfelder
  (Name, Land) **inline in der Toolbar** vorgesehen (passt laut Wiki noch in
  eine Zeile).

## Vorgeschlagene Schritte

### 1. `Country`-Modell und -Service (neu, feature-lokal)

`features/labels/country.ts` (`Country { id, name, code }`), `features/labels/
country.service.ts` (`CountryService`, `providedIn: 'root'`, `getAll():
Observable<Country[]>` gegen `GET /api/countries`). Bewusst **innerhalb**
`features/labels/` statt in `shared/`, da Country aktuell nur von Label
konsumiert wird (kein „mehrere Features brauchen es"-Fall) — falls ein
späterer Slice (z. B. Record) ebenfalls eine Länderauswahl braucht, wird das
Verschieben nach `shared/` dann nachgezogen, nicht vorab spekulativ angelegt.

### 2. Label-Modell und -Service

`features/labels/label.ts`: Interfaces `Label { id, name, countryId,
countryName, information }`, `LabelListResponse`, `CreateLabelRequest {
name, countryId, information }`, `UpdateLabelRequest` (gleiche Form).
`features/labels/label.service.ts`/`.spec.ts`: `LabelService` (`providedIn:
'root'`), `getPaged(page, pageSize, name?, countryId?)`, `create`, `update
(id, req)`, `delete(id)`, `baseUrl` aus `RuntimeConfigService.apiBaseUrl` +
`/api/labels`. Testmuster identisch zu `genre.service.spec.ts`
(`HttpTestingController`, Query-Parameter nur bei gesetztem Wert).

### 3. `Labels`-Shell mit `rxResource`

`features/labels/labels.ts`/`.html`/`.spec.ts` ersetzt den Platzhalter.
Signals `filterName`/`filterCountryId`/`page`; `labelsResource = rxResource({
params: () => ({page: this.page(), pageSize: PAGE_SIZE, name:
this.filterName(), countryId: this.filterCountryId()}), stream: ({params}) =>
this.labelService.getPaged(...) })`. Zusätzlich `countriesResource` (oder
einfacher `rxResource`/`toSignal` auf `countryService.getAll()`) für die
Select-Optionen in Filter **und** Formular — einmal auf Shell-Ebene laden und
per `input()` an `LabelFilter`/`LabelForm` durchreichen, um den Endpunkt nicht
doppelt aufzurufen. Fehlerbehandlung wie Genre: `effect()` auf
`labelsResource.error()` → `ErrorModalService` mit Retry.

### 4. `LabelFilter`

`features/labels/label-filter/label-filter.ts`/`.html`/`.spec.ts`: Namensfeld
mit `debounce(path.name, 300)` (wie Genre) **plus** Länder-`<select>` ohne
Debounce (siehe Ist-Stand). `countries = input.required<Country[]>()` für die
Optionsliste, erste Option „Alle Länder" (Wert leer/`undefined` → kein
Filter). Output weiterhin `filterChange` — Form auf `{ name: string,
countryId: number | undefined }` erweitert. `onFilterChange()` in `Labels`
setzt zusätzlich `page.set(1)` zurück (wie Genre).

### 5. `LabelTable` mit Paginierung

`features/labels/label-table/label-table.ts`/`.html`/`.spec.ts`: Spalten
Name, Land (`countryName`), Information, Aktionen — **jedes Feld der
Entität bekommt immer eine eigene Spalte** (mit dem Projektinhaber geklärt,
2026-08-13); lange Information wird per CSS (`truncate`, feste maximale
Breite) gekürzt dargestellt, der volle Text bleibt über das `title`-Attribut
als Tooltip einsehbar. Ansonsten strukturell identisch zu `GenreTable`
(Loading/Empty/gefüllt, `editRequested`/`deleteRequested`, eingebettete
`<app-pagination>`).

### 6. `LabelForm` mit Signal-Forms-Validierung (drei Felder)

`features/labels/label-form/label-form.ts`/`.html`/`.spec.ts`: `label: Label
| null`-Input, `countries = input.required<Country[]>()`. Signal Form mit:
- `name`: `required`, `minLength(1)`, `maxLength(60)`,
  `pattern(/^[\p{L}\p{N} \-&'./]+$/u)`.
- `countryId`: `required` (natives `<select>` mit `.select`-Klasse,
  Platzhalter-Option „Bitte wählen" mit leerem Wert, damit `required`
  greift).
- `information`: kein `required`, `maxLength(255)` (Textarea, `.textarea`-
  Klasse).

Speichern über `submit(labelForm, action)`: bei Erfolg `saved`-Output; bei
HTTP 400 wird der Server-Fehler anhand des Feldschlüssels (`Name`,
`CountryId` oder `Information`) dem passenden Feld zugeordnet — Erweiterung
des Genre-Musters (`handleSaveError`) von einem auf drei mögliche
Feldschlüssel; bei 404/409/500/Netzwerkfehler geht der Fehler an
`ErrorModalService`. Wird wie bei Genre bei jedem Öffnen frisch erzeugt.

### 7. `ConfirmModal`-Verdrahtung für Delete

Identisch zum Genre-Muster: Löschen-Icon setzt `pendingDelete`, öffnet
`ConfirmModal` mit Labelnamen, Bestätigen löst `LabelService.delete(id)` aus,
danach `labelsResource.reload()`; 409 (noch referenziert, Meldung kommt
serverseitig vorformuliert) oder 404 (zwischenzeitlich gelöscht) gehen an
`ErrorModalService`; Abbrechen löst keinen HTTP-Call aus.

### 8. Tests

Durchgängig je Schritt (nicht gesammelt am Ende), `// arrange`/`// act`/
`// assert`-Pflichtkommentare, `await fixture.whenStable()` nach jedem
`flush()`. Mindestfälle zusätzlich zum Genre-Umfang: `CountryService`-Test
(`getAll`-analog, aber ohne Parameter); Filter-Test für kombiniertes
Name+Land-Filtern inkl. „Alle Länder"-Reset; Formular-Test für alle drei
Felder einzeln (inkl. 400-Zuordnung je nach zurückgegebenem Feldschlüssel);
Tabellen-Test zeigt `countryName`-Spalte korrekt; Lösch-Test für den
409-Referenzfall mit dem serverseitig gelieferten `detail`-Text.

### 9. Dokumentation

- `TASK.md`: Abschnitt 4 (Label) auf „Backend und Frontend abgeschlossen"
  aktualisieren.
- `wiki/architektur/ui-ux-konzept.md`: Ergänzung, dass die Länderauswahl im
  Label-Formular/-Filter als natives `<select>` (`.select`-Klasse) umgesetzt
  ist — löst die zuvor offene Planungslücke.
- `03 Ressourcen/offene-punkte-angular-feature-slices.md`: keine der
  bestehenden Punkte 1/2/4 ändert sich; Punkt 3 (Sortierung) bleibt weiterhin
  offen (Label hat wie Genre keine wählbare Sortierung) — kein neuer Eintrag
  nötig, da die Länderauswahl-Frage nicht Teil dieser Liste war (sie wurde
  erst während dieser Planung als Lücke identifiziert und direkt geklärt statt
  eingetragen).

## Verifikation

1. `npm run build` in `src/frontend` — Production-Build erfolgreich.
2. `npm test -- --watch=false` — alle Frontend-Tests grün.
3. `ng lint` (falls zwischenzeitlich konfiguriert, siehe TASK.md-Hinweis zu
   Genre — bislang kein Lint-Target vorhanden), sonst Prettier-Check;
   `git diff --check` (Zeilenlänge 120 Zeichen).
4. Manuelle Live-Prüfung im Browser über den Aspire-AppHost: Liste laden,
   nach Name filtern, nach Land filtern, beide kombiniert, paginieren, Label
   anlegen (inkl. Inline-400 bei ungültigem Namen und bei fehlendem Land),
   Duplikat anlegen (409-Modal), Label bearbeiten inkl. Länderwechsel, Label
   löschen mit Bestätigung, Löschen abbrechen, Netzwerkfehler simulieren.
   Der 409-Referenzfall beim Löschen (Label wird von einem Record verwendet)
   ist nur prüfbar, wenn bereits ein Record mit diesem Label existiert — das
   Record-Frontend ist noch Platzhalter, daher ggf. Nachweis direkt über
   Swagger/API statt UI (siehe Risiken).

## Risiken und offene Punkte

- **409-Referenzprüfung beim Löschen ohne vorherige Client-Kenntnis**: Die
  User Story (US-L5) verlangt „keine Sicherheitsabfrage, wenn das Label
  referenziert ist" — der Client kann das vor dem Löschversuch aber nicht
  wissen. Umsetzung folgt stattdessen dem etablierten Genre-Muster (immer
  erst Sicherheitsabfrage, 409 danach im Modal). Sollte das als Abweichung
  von US-L5 nicht akzeptabel sein, müsste der Endpunkt um eine
  Vorab-Prüfung erweitert werden (Backend-Change, nicht Teil dieses Blocks).
- Manuelle Verifikation des 409-Referenzfalls ist mangels Record-Frontend nur
  über direkte API-Aufrufe (Swagger) möglich, nicht über einen vollständigen
  UI-Workflow.
- `CountryService`/`Country`-Modell liegt bewusst feature-lokal in
  `features/labels/` statt in `shared/` — falls ein späterer Slice (Record)
  ebenfalls eine Länderauswahl braucht, ist der Umzug nach `shared/`
  nachzuziehen.
- Sortierungs-UI-Muster bleibt weiterhin offen (unverändert gegenüber Genre).
- Rate Limiting (429) weiterhin backendseitig nicht verifizierbar
  (unverändert gegenüber Genre).
