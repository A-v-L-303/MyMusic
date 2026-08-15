# Fix: Tooltips für Badges und Buttons (Angular-Frontend)

## Kontext

Beim Live-Test der bereits umgesetzten Feature-Slices (Genre, Label, Artist,
Record/Tracks) ist aufgefallen, dass Badges und Buttons im Angular-Frontend
überwiegend keine Tooltips haben. Nur eine kleine Minderheit hatte bereits
`title` gesetzt (Lösch-Bestätigungstitel, Sortier-Umschalter in
`record-filter`, „Neues Label anlegen"-Button in `record-form`). Aufgabe:
Tooltips für die fehlenden Stellen ergänzen und die entstandene Konvention im
Wiki festhalten (`ui-ux-konzept.md`).

Reiner Frontend-Fix, kein Backend-Change, keine neuen Pakete.

## Ist-Stand (verifiziert)

- **Tooltip-Mechanismus im Projekt**: natives HTML-`title`-Attribut — keine
  eigene Tooltip-Komponente/Library vorhanden. Bereits etablierte Muster:
  - `record-filter.html` (Sortier-Button): `[title]` bindet exakt denselben
    Ausdruck wie `[attr.aria-label]`.
  - `record-form.html` (Label-Anlegen-Button): statisches `title="Neues Label
    anlegen"` neben `aria-label`.
  - `label-table.html`: langer `information`-Text wird per CSS gekürzt,
    voller Text steht als `[attr.title]` (bereits vorhanden, bleibt
    unverändert).
- Vollständige Inventur aller `<button>`- und `.badge`-Vorkommen im
  Angular-Workspace (`src/frontend/src/app/**/*.html`) durchgeführt. Viele
  Icon-only-Buttons hatten bereits ein dynamisches `[attr.aria-label]`, aber
  kein `title` — dort wurde derselbe Ausdruck einfach als `[title]` ergänzt.
- **Badges im weiteren Sinn**: Neben der CSS-Klasse `.badge` zählen auch die
  Klassen `.fmt` (Format-Pille auf der RecordCard) und `.grade`
  (Goldmine-Zustands-Badge auf RecordCard und im Detail-Modal-Kopf) dazu —
  das Design System selbst führt sie in
  `02 Wiki/MyMusic Wiki/wiki/design/komponenten-klassen.md` unter „Badges"
  bzw. „Grade-Badges (Goldmine-Zustandsbewertung)". Die vollen Klartext-Werte
  für die Zustandsstufen lagen bereits als Konstante
  `RECORD_CONDITION_LABELS` in `features/records/record.ts` vor (z. B.
  `VgPlus: 'Very Good Plus'`) — daraus wurde der Grade-Tooltip gespeist, ohne
  Text zu erfinden.

## Fix

`title`-Attribute ergänzt gemäß folgendem Schema:

- **Icon-only-Buttons mit vorhandenem `aria-label`** (Bearbeiten/Löschen in
  `artist-table`, `label-table`, `genre-table`, `track-list`, `record-card`;
  Schließen-Button in `record-detail`; Paginierung `pagination.html`;
  Theme-Umschalter `theme-toggle.html`): `[title]` bindet denselben Ausdruck
  wie `[attr.aria-label]`.
- **Anlegen-Buttons** in den Toolbars (`artists.html`, `labels.html`,
  `genres.html`, `records.html`): präzisierter Text statt nur „Anlegen"
  (z. B. „Neuen Artist anlegen").
- **Formular-Footer** (Abbrechen/Speichern) in allen fünf Formularen
  (`artist-form`, `label-form`, `genre-form`, `record-form`, `track-form`):
  `title` = sichtbarer Text.
- **Shared Modals**: `confirm-modal.html` (Abbrechen statisch, Bestätigen
  dynamisch über `confirmLabel()`), `error-modal.html` (Schließen, Erneut
  versuchen, OK).
- **Nav**: Logout „Abmelden", Login „Anmelden", Option-Dropdown-Toggle
  „Weitere Bereiche anzeigen".
- **`record-detail.html`**: „Track hinzufügen"-Button erhält `title="Track
  hinzufügen"`.
- **Badges/Pillen**: Anzahl-Badges in den Toolbars („Anzahl der gefundenen
  Artists" usw.), Genre-Badge je Track („Genre des Tracks"), Format-Badge/
  `.fmt`-Pille („Format des Albums"), Grade-Badge/`.grade`-Pille
  (ausgeschriebener Zustandsname aus `RECORD_CONDITION_LABELS`, Prefix
  „Zustand: ").

Betroffene Dateien (Templates): `shared/pagination/pagination.html`,
`shared/confirm-modal/confirm-modal.html`, `shared/error-modal/error-modal.html`,
`nav/nav.html`, `core/theme/theme-toggle/theme-toggle.html`,
`features/artists/artists.html`, `features/artists/artist-form/artist-form.html`,
`features/artists/artist-table/artist-table.html`,
`features/labels/labels.html`, `features/labels/label-form/label-form.html`,
`features/labels/label-table/label-table.html`,
`features/genres/genres.html`, `features/genres/genre-form/genre-form.html`,
`features/genres/genre-table/genre-table.html`,
`features/records/records.html`, `features/records/record-form/record-form.html`,
`features/records/track-form/track-form.html`,
`features/records/record-card/record-card.html`,
`features/records/record-detail/record-detail.html`,
`features/records/track-list/track-list.html`.

Tests: je geändertem Component-Spec eine Testerweiterung nach vorhandenem
Muster (`record-filter.spec.ts`, `record-form.spec.ts`, `label-table.spec.ts`),
die das neue/geänderte `title`-Attribut prüft.

## Wiki

Neuer Abschnitt „Tooltips" in
`02 Wiki/MyMusic Wiki/wiki/architektur/ui-ux-konzept.md` sowie ein Eintrag in
`wiki/log.md`.

## Verifikation

1. `npm test -- --watch=false` — 353 Frontend-Tests grün (334 zuvor + 19 neue
   Tooltip-Tests, je eine Erweiterung/Test pro geändertem Component-Spec).
2. `npm run build` — Production-Build erfolgreich.
3. `npx prettier --check` meldet projektweit auch für unveränderte
   Bestandsdateien Formatierungsabweichungen (bekannte, bereits in
   `docs/prompts/2026-08-15-fix-record-detail-modalbreite-und-trackgenre.md`
   dokumentierte CRLF-Diskrepanz unter Windows, `core.autocrlf=true`; CI
   prüft das für das Frontend ohnehin nicht, siehe `.github/workflows/ci.yml`
   Job `frontend-check`) — an den eigenen Änderungen selbst keine über dieses
   Grundrauschen hinausgehenden Abweichungen.
4. Zeilenlängen-Check (≤120 Zeichen) der geänderten Zeilen per
   `git diff --unified=0` geprüft — keine Überlänge.
5. Manuelle Live-Prüfung im Browser durch den Projektinhaber erfolgt und
   bestätigt.

## Risiken und offene Punkte

- Keine fachliche Verhaltensänderung außer der Tooltip-Ergänzung selbst —
  geringes Risiko.
- GitHub-Wiki-Sync für die Wiki-Ergänzung wurde nicht automatisch ausgelöst
  (siehe TASK.md-Nachtrag/Wiki-Log) — bislang nur bei „neue Quelle
  aufnehmen" vorgesehen, bei Bedarf gesondert anzustoßen.
